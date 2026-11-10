#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterCombatTests
    {
        private sealed class InputState : IWeaponInput,IPlayerCharacterCombatInput
        {
            public bool FireWeapon {get;set;}
            public int WeaponSlotSelection {get;set;}
            public bool ReloadPressed {get;set;}
            public bool FirePressed { get; set; }
            public bool ToggleFireModePressed { get; set; }
            public bool MeleePressed {get;set;}
        }
        private static InputState Enable(CharacterViewTestContext context, bool aiming = false)
        {
            context.Input.AimHeld = aiming; context.View.Tick(context.Input, .02f);
            var input=new InputState();CharacterViewTestContext.SetField(context.Shooter,"_input",input);
            CharacterViewTestContext.SetField(context.Shooter,"_combatInput",input);context.Shooter.enabled=true;return input;
        }

        [UnityTest]
        public IEnumerator RealMuzzleAmmoReloadAndFirstPersonPresentationAreConnected()
        {
            yield return CharacterViewTestContext.Load();var context=new CharacterViewTestContext();var input=Enable(context);
            var view=context.Actor.GetComponentInChildren<PlayerCharacterCombatView>();var magazine=context.Shooter.Magazine;
            yield return null;
            Assert.That(view.Pistol,Is.Not.Null);Assert.That(context.Shooter.Muzzle,Is.EqualTo(view.Pistol.Muzzle));
            Assert.That(view.Pistol.transform.IsChildOf(context.Actor.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand)),Is.True);
            Assert.That(Vector3.Dot(view.Pistol.Muzzle.forward,context.Actor.forward),Is.GreaterThan(.8f));
            int fired=0;context.Shooter.Fired+=()=>fired++;int before=magazine.Rounds;
            input.FireWeapon=true;yield return null;input.FireWeapon=false;
            Assert.That(fired,Is.EqualTo(1));Assert.That(magazine.Rounds,Is.EqualTo(before-1));
            CharacterViewTestContext.SetField(magazine,"_reloadDuration",.12f);input.ReloadPressed=true;yield return null;input.ReloadPressed=false;
            Assert.That(magazine.IsReloading,Is.True);input.FireWeapon=true;yield return new WaitForSeconds(.05f);input.FireWeapon=false;
            Assert.That(fired,Is.EqualTo(1));yield return new WaitForSeconds(.1f);
            Assert.That(magazine.Rounds,Is.EqualTo(magazine.Capacity));Assert.That(magazine.Reserve,Is.EqualTo(89));
            context.TogglePerspective();yield return null;yield return null;Assert.That(view.FirstPersonArms.enabled,Is.True);
            yield return new WaitForSeconds(.15f);
            Vector3 projected=context.Camera.WorldToViewportPoint(view.Pistol.Muzzle.position);
            Assert.That(projected.z,Is.GreaterThan(0f));Assert.That(projected.x,Is.InRange(0f,1f));Assert.That(projected.y,Is.InRange(0f,1f));
            input.WeaponSlotSelection=PlayerCharacterLoadout.UnarmedSlot;yield return null;input.WeaponSlotSelection=0;
            Assert.That(context.Shooter.IsArmed,Is.False);Assert.That(view.Pistol.gameObject.activeSelf,Is.False);
            input.WeaponSlotSelection=PlayerCharacterLoadout.PistolSlot;yield return null;input.WeaponSlotSelection=0;
            Assert.That(context.Shooter.IsArmed,Is.True);Assert.That(view.Pistol.gameObject.activeSelf,Is.True);
        }

        [UnityTest]
        public IEnumerator EmptyMagazineReloadsOnlyAfterFireIsPressedAgain()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            var input = Enable(context);
            var magazine = context.Shooter.Magazine;
            Assert.That(magazine.Reserve, Is.GreaterThan(0));

            while (magazine.Rounds > 1)
                Assert.That(magazine.TryConsumeRound(), Is.True);
            Assert.That(magazine.Rounds, Is.EqualTo(1));

            input.FireWeapon = true;
            yield return null;
            Assert.That(magazine.Rounds, Is.Zero);
            Assert.That(magazine.IsReloading, Is.False);

            yield return null;
            Assert.That(magazine.IsReloading, Is.False, "Holding fire on empty must not start a reload.");
            input.FireWeapon = false;
            yield return null;
            Assert.That(magazine.IsReloading, Is.False, "Releasing fire must not start a reload.");

            input.FirePressed = true;
            input.FireWeapon = true;
            yield return null;
            Assert.That(magazine.IsReloading, Is.True, "A new click on empty must start a reload.");
        }

        [UnityTest]
        public IEnumerator FirstPersonWeaponFollowsPitchAdsAndCrouchWithoutMovingTheCameraOffsets()
        {
            yield return CharacterViewTestContext.Load();var context=new CharacterViewTestContext();Enable(context);context.TogglePerspective();
            var weapon=context.Actor.GetComponentInChildren<PlayerCharacterCombatView>().Pistol;
            foreach(float change in new[]{-20f,40f,-20f})
            {
                context.Input.Look=new Vector2(0,change);context.View.Tick(context.Input,.02f);context.Input.Look=Vector2.zero;
                yield return new WaitForSeconds(.2f);var point=context.Camera.WorldToViewportPoint(weapon.Muzzle.position);
                Assert.That(point.z,Is.GreaterThan(0f));Assert.That(point.x,Is.InRange(0f,1f));Assert.That(point.y,Is.InRange(0f,1f));
            }
            context.TapAim();yield return new WaitForSeconds(.3f);
            Vector3 ads=context.Camera.WorldToViewportPoint(weapon.Muzzle.position);Assert.That(ads.x,Is.EqualTo(.5f).Within(.1f));Assert.That(ads.y,Is.EqualTo(.5f).Within(.15f));
            Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().TrySetCrouched(true),Is.True);
            yield return new WaitForSeconds(.5f);var crouch=context.Camera.WorldToViewportPoint(weapon.Muzzle.position);
            Assert.That(crouch.z,Is.GreaterThan(0f));Assert.That(crouch.x,Is.InRange(0f,1f));Assert.That(crouch.y,Is.InRange(0f,1f));
        }

        [UnityTest]
        public IEnumerator CharacterFistsAndPistolStrikeUseDifferentDamageAndCancelOnDisable()
        {
            yield return CharacterViewTestContext.Load();var context=new CharacterViewTestContext();var input=Enable(context);
            Health target=Object.FindObjectsByType<Health>().Single(h=>h.name=="Dummy_Enemy");target.transform.SetParent(null,true);
            target.transform.position=context.Actor.position+Vector3.forward*2.5f;Physics.SyncTransforms();float health=target.CurrentHealth;
            input.WeaponSlotSelection=PlayerCharacterLoadout.UnarmedSlot;input.FireWeapon=true;yield return null;input.WeaponSlotSelection=0;input.FireWeapon=false;
            Assert.That(context.Shooter.IsAttacking,Is.True);yield return new WaitForSeconds(.3f);
            Assert.That(target.CurrentHealth,Is.EqualTo(health-20));yield return new WaitForSeconds(.6f);
            input.WeaponSlotSelection=PlayerCharacterLoadout.PistolSlot;input.MeleePressed=true;yield return null;input.WeaponSlotSelection=0;input.MeleePressed=false;
            yield return new WaitForSeconds(.3f);Assert.That(target.CurrentHealth,Is.EqualTo(health-50));
            yield return new WaitForSeconds(.6f);input.MeleePressed=true;yield return null;input.MeleePressed=false;
            context.Shooter.enabled=false;yield return new WaitForSeconds(.3f);Assert.That(target.CurrentHealth,Is.EqualTo(health-50));
            Assert.That(context.Shooter.IsAttacking,Is.False);
        }

        [UnityTest]
        public IEnumerator FiringKeepsGripWhileReloadAndMeleePlayTheirActionStates()
        {
            yield return CharacterViewTestContext.Load();var context=new CharacterViewTestContext();var input=Enable(context);
            var animator=context.Actor.GetComponentInChildren<Animator>();int layer=animator.GetLayerIndex("UpperCombat");
            input.FireWeapon=true;yield return null;input.FireWeapon=false;yield return null;
            Assert.That(animator.GetCurrentAnimatorStateInfo(layer).IsName("UpperCombat.PistolHold"),Is.True);
            Assert.That(context.Actor.GetComponentInChildren<PlayerCharacterCombatView>().RecoilAmount,Is.GreaterThan(0f));
            input.ReloadPressed=true;yield return null;input.ReloadPressed=false;yield return null;
            Assert.That(animator.GetCurrentAnimatorStateInfo(layer).IsName("UpperCombat.PistolReload"),Is.True);
            input.WeaponSlotSelection=PlayerCharacterLoadout.UnarmedSlot;yield return null;input.WeaponSlotSelection=0;yield return new WaitForSeconds(.15f);
            input.MeleePressed=true;yield return null;input.MeleePressed=false;yield return null;
            yield return new WaitForSeconds(.08f);
            int meleeLayer=animator.GetLayerIndex("MeleeCombat");
            Assert.That(animator.GetCurrentAnimatorStateInfo(meleeLayer).IsName("MeleeCombat.PunchJab"),Is.True);
            Assert.That(animator.GetLayerWeight(meleeLayer),Is.EqualTo(1f));
            var stance=context.Actor.GetComponent<PlayerCharacterStance>();Assert.That(stance.TrySetCrouched(true),Is.True);
            yield return new WaitForSeconds(.25f);Assert.That(animator.GetBool("Crouched"),Is.True);
        }

        [UnityTest]
        public IEnumerator ThirdPersonPistolMuzzleFollowsAimWhileTurningLookingAndCrouching()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Enable(context, true);
            context.Actor.rotation = Quaternion.Euler(0f, 53f, 0f);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            Quaternion modelRotation = animator.transform.localRotation;
            var floor = new GameObject("Third person aim test floor");
            floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(100f, .5f, 100f);
            context.Actor.GetComponent<PlayerCharacterMovement>().enabled = true;
            Physics.SyncTransforms();
            yield return new WaitForSeconds(.25f);
            foreach (bool crouched in new[] { false, true })
            {
                Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().TrySetCrouched(crouched), Is.True);
                foreach (Vector2 look in new[] { Vector2.zero, new Vector2(15f, 0f), new Vector2(-30f, -15f), new Vector2(15f, 30f), new Vector2(0f, -15f) })
                {
                    context.Input.Look = look;
                    context.View.Tick(context.Input, .02f);
                    context.Input.Look = Vector2.zero;
                    yield return new WaitForSeconds(.25f);
                    Vector3 aim = context.View.AimRotation * Vector3.forward;
                    Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, aim), Is.LessThan(2f), $"crouched={crouched}, look={look}");
                    Assert.That(Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.LeftHand).position,
                        animator.GetBoneTransform(HumanBodyBones.RightHand).position), Is.LessThan(.4f));
                    Assert.That(Quaternion.Angle(modelRotation, animator.transform.localRotation), Is.LessThan(.01f));
                }
            }
            Object.Destroy(floor);
        }

        [UnityTest]
        public IEnumerator UpperBodyAndHeadParticipateInVerticalAimingWithoutRotatingTheCharacterRoot()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Enable(context, true);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            Quaternion rootRotation = context.Actor.rotation;
            yield return new WaitForSeconds(.4f);
            Quaternion neutralChest = chest.rotation, neutralHead = head.rotation;
            foreach (float change in new[] { -20f, 40f, -20f })
            {
                context.Input.Look = new Vector2(0f, change);
                context.View.Tick(context.Input, .02f);
                context.Input.Look = Vector2.zero;
                yield return new WaitForSeconds(.5f);
                float pitch = Mathf.Abs(Mathf.DeltaAngle(0f, context.View.AimRotation.eulerAngles.x));
                if (pitch > 30f)
                {
                    Assert.That(Quaternion.Angle(neutralChest, chest.rotation), Is.GreaterThan(8f), "The torso must share the aiming pitch.");
                    Assert.That(Quaternion.Angle(neutralHead, head.rotation), Is.GreaterThan(15f), "The head must follow the aiming direction.");
                }
                else Assert.That(Quaternion.Angle(neutralChest, chest.rotation), Is.LessThan(5f));
                Assert.That(Quaternion.Angle(rootRotation, context.Actor.rotation), Is.LessThan(.01f));
                Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            }
        }

        [UnityTest]
        public IEnumerator WorldBodySharesThirdPersonAimAcrossPerspectiveChangesWithoutTiltingTheLegs()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext(); Enable(context, true);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            context.Actor.GetComponentInChildren<PlayerCharacterAnimationView>().enabled = false;
            animator.SetBool("Grounded", true); animator.SetFloat("Speed", 0f);
            animator.Play("Base Layer.Locomotion", 0, .25f);
            yield return new WaitForSeconds(.3f); animator.speed = 0f;
            var stance = context.Actor.GetComponent<PlayerCharacterStance>();
            var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            var leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            var rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Quaternion actorRotation = context.Actor.rotation, modelRotation = animator.transform.localRotation;
            foreach (bool crouched in new[] {false, true})
            {
                Assert.That(stance.TrySetCrouched(crouched), Is.True);
                animator.SetBool("Crouched", crouched);
                animator.Play(crouched ? "Base Layer.CrouchLocomotion" : "Base Layer.Locomotion", 0, .25f);
                foreach (float pitch in new[] {-60f, 60f})
                {
                    float current = Mathf.DeltaAngle(0f, context.View.AimRotation.eulerAngles.x);
                    context.Input.Look = new Vector2(0f, (current - pitch) / 3f);
                    context.View.Tick(context.Input, .02f); context.Input.Look = Vector2.zero;
                    yield return new WaitForSeconds(.25f); animator.Update(0f);
                    Quaternion thirdChest = chest.rotation, thirdHead = head.rotation;
                    Quaternion thirdPelvis = pelvis.localRotation, thirdLeft = leftLeg.localRotation, thirdRight = rightLeg.localRotation;
                    context.TogglePerspective(); yield return null; yield return null; animator.Update(0f);
                    Assert.That(context.View.IsFirstPersonView, Is.True);
                    string sample = $"crouched={crouched}, pitch={pitch}";
                    Assert.That(Quaternion.Angle(thirdChest, chest.rotation), Is.LessThan(3f), "World torso should retain the third-person aim: " + sample);
                    Assert.That(Quaternion.Angle(thirdHead, head.rotation), Is.LessThan(3f), "World head should retain the third-person aim: " + sample);
                    Assert.That(Quaternion.Angle(thirdPelvis, pelvis.localRotation), Is.LessThan(1f), "Pelvis: " + sample);
                    Assert.That(Quaternion.Angle(thirdLeft, leftLeg.localRotation), Is.LessThan(1f), "Left leg: " + sample);
                    Assert.That(Quaternion.Angle(thirdRight, rightLeg.localRotation), Is.LessThan(1f), "Right leg: " + sample);
                    Assert.That(Quaternion.Angle(actorRotation, context.Actor.rotation), Is.LessThan(.01f));
                    Assert.That(Quaternion.Angle(modelRotation, animator.transform.localRotation), Is.LessThan(.01f));
                    context.TogglePerspective(); yield return null; yield return null;
                }
            }
        }

        [UnityTest]
        public IEnumerator FirstPersonFreeLookKeepsTheWorldUpperBodyOnTheWeaponAim()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext(); Enable(context); context.TogglePerspective();
            var animator = context.Actor.GetComponentInChildren<Animator>();
            context.Actor.GetComponentInChildren<PlayerCharacterAnimationView>().enabled = false;
            animator.SetBool("Grounded", true); animator.SetFloat("Speed", 0f);
            animator.Play("Base Layer.Locomotion", 0, .25f);
            context.Input.Look = new Vector2(0f, -20f); context.View.Tick(context.Input, .02f); context.Input.Look = Vector2.zero;
            yield return new WaitForSeconds(.3f); animator.speed = 0f; animator.Update(0f);
            var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Quaternion heldChest = chest.rotation, heldAim = context.View.AimRotation;
            context.Input.FreeLookHeld = true; context.Input.Look = new Vector2(20f, 10f);
            context.View.Tick(context.Input, .02f); context.Input.Look = Vector2.zero;
            yield return new WaitForSeconds(.25f); animator.Update(0f);
            Assert.That(Vector3.Angle(context.Camera.transform.forward, heldAim * Vector3.forward), Is.GreaterThan(30f));
            Assert.That(Quaternion.Angle(heldAim, context.View.AimRotation), Is.LessThan(.01f));
            Assert.That(Quaternion.Angle(heldChest, chest.rotation), Is.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator TorsoAndWeaponReachTheNewAimInTheSameAnimationEvaluation()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Enable(context, true);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            var animationView = context.Actor.GetComponentInChildren<PlayerCharacterAnimationView>();
            animationView.enabled = false;
            animator.SetBool("Grounded", true);
            animator.SetFloat("Speed", 0f);
            animator.Play("Base Layer.Locomotion", 0, 0f);
            yield return new WaitForSeconds(.4f);
            animator.speed = 0f; // Compare aim responses against an identical underlying clip pose.
            Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            foreach (bool crouched in new[] { false, true })
            {
                Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().TrySetCrouched(crouched), Is.True);
                animator.SetBool("Crouched", crouched);
                animator.Play(crouched ? "Base Layer.CrouchLocomotion" : "Base Layer.Locomotion", 0, 0f);
                foreach (float pitch in new[] { -60f, 60f, 0f })
                {
                    float current = Mathf.DeltaAngle(0f, context.View.AimRotation.eulerAngles.x);
                    context.Input.Look = new Vector2(0f, (current - pitch) / 3f);
                    context.View.Tick(context.Input, .02f);
                    context.Input.Look = Vector2.zero;
                    animator.Update(0f);
                    Quaternion immediate = chest.rotation;
                    Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
                    yield return new WaitForSeconds(.4f);
                    animator.Update(0f);
                    Assert.That(Quaternion.Angle(immediate, chest.rotation), Is.LessThan(2f),
                        $"The torso must not catch up after the weapon has reached its aim: crouched={crouched}, pitch={pitch}");
                }
            }
        }

        [UnityTest]
        public IEnumerator FreeLookPreservesTheAimedTorsoAndWeaponPose()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Enable(context);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            context.Actor.GetComponentInChildren<PlayerCharacterAnimationView>().enabled = false;
            animator.SetBool("Grounded", true);
            animator.SetFloat("Speed", 0f);
            animator.Play("Base Layer.Locomotion", 0, 0f);
            context.Input.Look = new Vector2(0f, -20f);
            context.View.Tick(context.Input, .02f);
            context.Input.Look = Vector2.zero;
            yield return new WaitForSeconds(.5f);
            animator.speed = 0f;
            animator.Update(0f);
            Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Quaternion heldChest = chest.rotation, heldAim = context.View.AimRotation;
            Quaternion heldGun = context.Shooter.Muzzle.rotation;
            context.Input.FreeLookHeld = true;
            context.Input.Look = new Vector2(30f, 30f);
            context.View.Tick(context.Input, .02f);
            context.Input.Look = Vector2.zero;
            yield return new WaitForSeconds(.4f);
            animator.Update(0f);
            Assert.That(Vector3.Angle(context.Camera.transform.forward, heldAim * Vector3.forward), Is.GreaterThan(45f));
            Assert.That(Quaternion.Angle(heldAim, context.View.AimRotation), Is.LessThan(.01f));
            Assert.That(Quaternion.Angle(heldChest, chest.rotation), Is.LessThan(1f));
            Assert.That(Quaternion.Angle(heldGun, context.Shooter.Muzzle.rotation), Is.LessThan(1f));
            context.Input.FreeLookHeld = false;
            context.View.Tick(context.Input, .02f);
            yield return new WaitForSeconds(.5f);
            animator.Update(0f);
            Assert.That(Quaternion.Angle(heldChest, chest.rotation), Is.LessThan(1f));
            Assert.That(Quaternion.Angle(heldGun, context.Shooter.Muzzle.rotation), Is.LessThan(1f));
        }

        [UnityTest]
        public IEnumerator BothHandsKeepTheirGripAtMaximumVerticalAim()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Enable(context, true);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            Transform right = animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform left = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            yield return new WaitForSeconds(.4f);
            float gripDistance = Vector3.Distance(left.position, right.position);
            foreach (float pitch in new[] { -80f, 80f })
            {
                float current = Mathf.DeltaAngle(0f, context.View.AimRotation.eulerAngles.x);
                context.Input.Look = new Vector2(0f, (current - pitch) / 3f);
                context.View.Tick(context.Input, .02f);
                context.Input.Look = Vector2.zero;
                yield return new WaitForSeconds(.6f);
                Assert.That(Vector3.Distance(left.position, right.position), Is.EqualTo(gripDistance).Within(.04f));
                Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            }
        }

        [UnityTest]
        public IEnumerator ThirdPersonAimReleasesHandsForReloadAndMelee()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            var input = Enable(context, true);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            var hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            yield return new WaitForSeconds(.25f);
            Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.Actor.forward), Is.LessThan(2f));
            Vector3 held = hand.position;
            Assert.That(context.Shooter.Magazine.TryConsumeRound(), Is.True);
            input.ReloadPressed = true;
            yield return null;
            input.ReloadPressed = false;
            yield return new WaitForSeconds(.6f);
            Assert.That(Vector3.Distance(hand.position, held), Is.GreaterThan(.1f), "Reload must move the hand away from the aiming hold.");
            yield return new WaitForSeconds(1.2f);
            Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.Actor.forward), Is.LessThan(2f));
            held = hand.position;
            input.MeleePressed = true;
            yield return null;
            input.MeleePressed = false;
            yield return new WaitForSeconds(.3f);
            Assert.That(context.Shooter.IsAttacking, Is.True);
            Assert.That(Vector3.Distance(hand.position, held), Is.GreaterThan(.1f), "Melee must retain the animated strike instead of the aiming hold.");
            yield return new WaitForSeconds(.65f);
            Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.Actor.forward), Is.LessThan(2f));
        }

        [UnityTest]
        public IEnumerator CombatLayerPreservesLocomotionHeadAndTorsoFacingInThirdPerson()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            context.Actor.rotation = Quaternion.Euler(0f, 53f, 0f);
            context.Actor.GetComponentInChildren<PlayerCharacterCombatView>().enabled = false;
            context.Actor.GetComponentInChildren<PlayerCharacterAnimationView>().enabled = false;
            var animator = context.Actor.GetComponentInChildren<Animator>();
            int layer = animator.GetLayerIndex("UpperCombat");
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform torso = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Quaternion modelRotation = animator.transform.localRotation;
            string[] actions = { "PistolHold", "PistolShoot", "PistolReload", "FistsHold", "PunchJab", "PunchCross", "PistolMelee" };

            foreach (bool crouched in new[] { false, true })
            foreach (float speed in new[] { 0f, 1f })
            foreach (string action in actions)
            {
                animator.Rebind();
                animator.SetBool("Grounded", true);
                animator.SetBool("Crouched", crouched);
                animator.SetFloat("Speed", speed);
                animator.Play(crouched ? "Base Layer.CrouchLocomotion" : "Base Layer.Locomotion", 0, .25f);
                animator.SetLayerWeight(layer, 0f);
                animator.Update(0f);
                Vector3 headForward = Quaternion.Inverse(head.rotation) * context.Actor.forward;
                Vector3 torsoForward = Quaternion.Inverse(torso.rotation) * context.Actor.forward;

                animator.SetLayerWeight(layer, 1f);
                animator.Play("UpperCombat." + action, layer, .25f);
                animator.Update(0f);
                string state = $"{action}, crouched={crouched}, speed={speed}";
                float headYaw = Vector3.Angle(context.Actor.forward,
                    Vector3.ProjectOnPlane(head.rotation * headForward, context.Actor.up));
                float torsoYaw = Vector3.Angle(context.Actor.forward,
                    Vector3.ProjectOnPlane(torso.rotation * torsoForward, context.Actor.up));
                Assert.That(headYaw, Is.LessThan(1f), "Head facing: " + state);
                Assert.That(torsoYaw, Is.LessThan(1f), "Torso facing: " + state);
                Assert.That(Quaternion.Angle(modelRotation, animator.transform.localRotation), Is.LessThan(.01f));
            }
        }

        [UnityTest]
        public IEnumerator BoardingCancelsReloadAndMeleeAndRestoresWeaponOnExit()
        {
            yield return BoardingTestContext.Load();var context=new BoardingTestContext();var combat=context.Actor.GetComponent<PlayerCharacterController>();
            combat.GetComponent<PlayerCharacterLoadout>().TryEquip(PlayerCharacterLoadout.PistolSlot);
            var magazine=combat.Magazine;var presentation=context.Actor.GetComponentInChildren<PlayerCharacterCombatView>();
            Assert.That(magazine.TryConsumeRound(),Is.True);Assert.That(magazine.TryReload(),Is.True);
            context.PrepareEntry();Assert.That(context.Seat.TryEnter(context.Occupant),Is.True);yield return null;
            Assert.That(magazine.IsReloading,Is.False);Assert.That(presentation.Pistol.gameObject.activeSelf,Is.False);
            Assert.That(context.Seat.TryExit(),Is.True);yield return null;yield return null;
            Assert.That(presentation.Pistol.gameObject.activeSelf,Is.True);Assert.That(magazine.Rounds,Is.EqualTo(14));
            var input=new InputState{MeleePressed=true};CharacterViewTestContext.SetField(combat,"_input",input);CharacterViewTestContext.SetField(combat,"_combatInput",input);
            yield return null;input.MeleePressed=false;Assert.That(combat.IsAttacking,Is.True);
            context.PrepareEntry();Assert.That(context.Seat.TryEnter(context.Occupant),Is.True);yield return new WaitForSeconds(.3f);
            Assert.That(combat.IsAttacking,Is.False);Assert.That(presentation.FirstPersonArms.enabled,Is.False);
        }
    }
}
#endif
