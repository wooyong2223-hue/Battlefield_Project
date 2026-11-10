#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public sealed class PlayerCharacterFistAnimationTests
    {
        private static IEnumerator Ground(CharacterViewTestContext context)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Melee animation test floor";
            floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(100f, .5f, 100f);
            Physics.SyncTransforms();
            var movement = context.Actor.GetComponent<PlayerCharacterMovement>();
            movement.enabled = true;
            yield return new WaitForSeconds(.3f);
            Assert.That(movement.IsGrounded, Is.True, "Evaluate grounded combat rather than the fixture's airborne pose.");
        }

        private sealed class CombatInput : IWeaponInput, IPlayerCharacterCombatInput
        {
            public bool FireWeapon { get; set; }
            public int WeaponSlotSelection { get; set; }
            public bool ReloadPressed => false;
            public bool FirePressed => false;
            public bool ToggleFireModePressed => false;
            public bool MeleePressed { get; set; }
        }

        [UnityTest]
        public IEnumerator FirstPersonMeleeUsesVisibleArmsAndRestoresThePistolMesh()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            yield return Ground(context);
            var input = new CombatInput();
            CharacterViewTestContext.SetField(context.Shooter, "_input", input);
            CharacterViewTestContext.SetField(context.Shooter, "_combatInput", input);
            context.Shooter.enabled = true;
            context.TogglePerspective();
            yield return new WaitForSeconds(.25f);
            var presentation = context.Actor.GetComponentInChildren<PlayerCharacterCombatView>();
            var renderer = presentation.FirstPersonArms;
            Mesh pistolMesh = renderer.sharedMesh;
            input.WeaponSlotSelection = PlayerCharacterLoadout.UnarmedSlot; yield return null; input.WeaponSlotSelection = 0;
            yield return new WaitForSeconds(.25f);
            Mesh meleeMesh = renderer.sharedMesh;
            Assert.That(meleeMesh, Is.Not.SameAs(pistolMesh));
            Assert.That(meleeMesh.subMeshCount, Is.EqualTo(renderer.sharedMaterials.Length));
            Assert.That(meleeMesh.GetIndexCount(0), Is.GreaterThan(0), "Visible material slot must contain arm triangles.");
            input.WeaponSlotSelection = PlayerCharacterLoadout.PistolSlot; yield return null; input.WeaponSlotSelection = 0;
            yield return new WaitForSeconds(.25f);
            Assert.That(renderer.sharedMesh, Is.SameAs(pistolMesh));
            input.MeleePressed = true; yield return null; input.MeleePressed = false;
            yield return new WaitForSeconds(.15f);
            Assert.That(renderer.sharedMesh, Is.SameAs(meleeMesh));
            yield return new WaitForSeconds(.85f);
            Assert.That(renderer.sharedMesh, Is.SameAs(pistolMesh));
            Assert.That(presentation.Pistol.gameObject.activeSelf, Is.True);
        }
        [UnityTest]
        public IEnumerator MeleeClipsFollowPitchInBothPerspectives()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            yield return Ground(context);
            var input = new CombatInput { WeaponSlotSelection = PlayerCharacterLoadout.UnarmedSlot };
            CharacterViewTestContext.SetField(context.Shooter, "_input", input);
            CharacterViewTestContext.SetField(context.Shooter, "_combatInput", input);
            context.Shooter.enabled = true;
            context.Actor.rotation = Quaternion.Euler(0f, 53f, 0f);
            yield return null; input.WeaponSlotSelection = 0;
            var animator = context.Actor.GetComponentInChildren<Animator>();
            var rig = context.Actor.GetComponentInChildren<FirstPersonArmsView>();
            bool left = true;
            for (int perspective = 0; perspective < 2; perspective++)
            {
                foreach (float pitch in new[] {-35f, 35f})
                {
                    CharacterViewTestContext.SetField(context.View, "_pitch", pitch);
                    yield return new WaitForSeconds(.25f);
                    input.FireWeapon = true; yield return null; input.FireWeapon = false;
                    yield return new WaitForSeconds(.2f);
                    Transform hand = animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                    Transform shoulder = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
                    if (perspective == 1) { hand = rig.transform.Find(hand.name); shoulder = rig.transform.Find(shoulder.name); }
                    float angle = Vector3.Angle(hand.position - shoulder.position, context.View.AimRotation * Vector3.forward);
                    Debug.Log($"MELEE_PITCH first={perspective == 1} pitch={pitch} angle={angle}");
                    Assert.That(angle, Is.LessThan(30f), "Captured punch should follow the vertical aim.");
                    Assert.That(Mathf.DeltaAngle(context.Actor.eulerAngles.y, 53f), Is.EqualTo(0f).Within(.1f));
                    left = !left;
                    yield return new WaitForSeconds(.65f);
                }
                context.TogglePerspective(); yield return new WaitForSeconds(.2f);
            }
        }

        [UnityTest]
        public IEnumerator AlternatingFistsExtendForwardInBothPerspectives()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            yield return Ground(context);
            var input = new CombatInput { WeaponSlotSelection = PlayerCharacterLoadout.UnarmedSlot };
            CharacterViewTestContext.SetField(context.Shooter, "_input", input);
            CharacterViewTestContext.SetField(context.Shooter, "_combatInput", input);
            context.Shooter.enabled = true;
            context.Actor.rotation = Quaternion.Euler(0f, 53f, 0f);
            context.Input.Look = Vector2.zero;
            context.View.Tick(context.Input, .02f);
            context.Input.Look = Vector2.zero;
            yield return null;
            input.WeaponSlotSelection = 0;
            yield return new WaitForSeconds(.25f);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            var arms = context.Actor.GetComponentInChildren<FirstPersonArmsView>();
            for (int perspective = 0; perspective < 2; perspective++)
            {
                for (int punch = 0; punch < 2; punch++)
                {
                    bool left = punch == 0;
                    Transform hand = animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                    Transform shoulder = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
                    if (perspective == 1)
                    {
                        hand = arms.transform.Find(hand.name);
                        shoulder = arms.transform.Find(shoulder.name);
                    }
                    if (perspective == 1)
                    {
                        Vector3 guard = context.Camera.WorldToViewportPoint(hand.position);
                        Assert.That(guard.x, Is.InRange(.02f, .98f), "Guard fist should remain visible.");
                        Assert.That(guard.y, Is.InRange(.02f, .98f), "Guard fist should remain visible.");
                    }
                    Quaternion rootBefore = context.Actor.rotation;
                    Vector3 guardHand = hand.position;
                    Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                    Quaternion guardChest = chest.rotation;
                    float chestMotion = 0f;
                    float peakForward = float.NegativeInfinity, peakTime = 0f, impactReach = 0f;
                    Vector3 peak = Vector3.zero, viewport = Vector3.zero;
                    input.FireWeapon = true;
                    yield return null;
                    input.FireWeapon = false;
                    float start = Time.time;
                    while (Time.time - start < context.Shooter.MeleeDuration)
                    {
                        chestMotion = Mathf.Max(chestMotion, Quaternion.Angle(guardChest, chest.rotation));
                        Vector3 extension = Quaternion.Inverse(context.View.AimRotation) * (hand.position - shoulder.position);
                        if (Mathf.Abs(Time.time - start - context.Shooter.MeleeWindup) < .06f)
                            impactReach = Mathf.Max(impactReach, extension.z);
                        if ((Quaternion.Inverse(context.View.AimRotation) * (hand.position - guardHand)).z > peakForward)
                        {
                            peakForward = (Quaternion.Inverse(context.View.AimRotation) * (hand.position - guardHand)).z;
                            peak = extension;
                            peakTime = Time.time - start;
                            viewport = context.Camera.WorldToViewportPoint(hand.position);
                        }
                        yield return null;
                    }
                    Debug.Log($"FIST_FORWARD first={perspective == 1} left={left} extension={peak} time={peakTime:F3} viewport={viewport}");
                    Assert.That(chestMotion, Is.GreaterThan(3f), "Captured torso rotation must participate in the punch.");
                    Assert.That(peak.z, Is.GreaterThan(.65f), "Fist must extend in the aim direction.");
                    Assert.That(Mathf.Abs(peak.x), Is.LessThan(peak.z * .65f), "Punch must not travel sideways.");
                    
                    Assert.That(impactReach, Is.GreaterThan(.8f), "The fist must already extend forward at the actual hit windup.");
                    Assert.That(Quaternion.Angle(rootBefore, context.Actor.rotation), Is.LessThan(.1f), "Presentation must not rotate gameplay root.");
                    if (perspective == 1)
                    {
                        Assert.That(viewport.z, Is.GreaterThan(.05f));
                        Assert.That(viewport.x, Is.InRange(.05f, .95f));
                        Assert.That(viewport.y, Is.InRange(.05f, .95f));
                    }
                    yield return new WaitForSeconds(.2f);
                }
                context.TogglePerspective();
                yield return new WaitForSeconds(.2f);
            }
        }
    }
}
#endif
