#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterReloadPoseTests
    {
        sealed class IdleInput:IWeaponInput{public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
        [UnityTest]
        public IEnumerator WeaponReloadClipReplacesOnlyTheStandingReloadAndOwnsTheHandMotion()
        {
            yield return CharacterViewTestContext.Load();
            var c = new CharacterViewTestContext();
            var loadout = c.Actor.GetComponent<PlayerCharacterLoadout>();
            Assert.That(loadout.TryEquip(PlayerCharacterLoadout.RifleSlot), Is.True);
            var input = c.EnableCombatInput();
            var animator = c.Actor.GetComponentInChildren<Animator>();
            var reloadView = animator.GetComponent<PlayerCharacterRifleReloadView>();
            var combatView = animator.GetComponent<PlayerCharacterCombatView>();
            var equipment = loadout.GetEquipment(PlayerCharacterLoadout.RifleSlot);
            var shared = loadout.Definition;
            var binding = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var reloadSlot = (AnimationClip)combatView.GetType().GetField("_reloadAnimation", binding).GetValue(combatView);
            var originalController = (AnimatorOverrideController)animator.runtimeAnimatorController;
            var otherClips = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>();
            originalController.GetOverrides(otherClips);
            var authoredClip = Object.Instantiate(reloadSlot);
            var custom = Object.Instantiate(shared);
            try
            {
                CharacterViewTestContext.SetField(custom, "_reloadClip", authoredClip);
                Assert.That(equipment.TryEquip(custom), Is.True);
                var controller = (AnimatorOverrideController)animator.runtimeAnimatorController;
                Assert.That(controller[reloadSlot], Is.SameAs(authoredClip));
                foreach (var pair in otherClips)
                    if (pair.Key != reloadSlot)
                        Assert.That(controller[pair.Key] ?? pair.Key, Is.EqualTo(pair.Value ?? pair.Key), pair.Key.name);
                Assert.That(c.Shooter.Magazine.TryConsumeRound(), Is.True);
                input.ReloadPressed = true;
                yield return null;
                input.ReloadPressed = false;
                Assert.That(c.Shooter.Magazine.IsReloading, Is.True);
                Assert.That(reloadView.IsActive, Is.False, "The authored clip owns the hand motion.");

                Assert.That(equipment.TryEquip(shared), Is.True);
                Assert.That(c.Shooter.Magazine.TryConsumeRound(), Is.True);
                Assert.That(c.Shooter.Magazine.TryReload(), Is.True);
                Assert.That(reloadView.IsActive, Is.True, "The shared reload still uses prefab contacts.");
            }
            finally
            {
                Object.Destroy(custom);
                Object.Destroy(authoredClip);
            }
        }

        [UnityTest] public IEnumerator ReloadOverlayClearsAfterCompletionCancellationAndDisable()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();c.Shooter.enabled=true;CharacterViewTestContext.SetField(c.Shooter,"_input",new IdleInput());
            var animator=c.Actor.GetComponentInChildren<Animator>();int layer=animator.GetLayerIndex("ReloadUpperBody");Assert.That(layer,Is.GreaterThanOrEqualTo(0));
            var magazine=c.Shooter.Magazine;CharacterViewTestContext.SetField(magazine,"_reloadDuration",.12f);
            Assert.That(magazine.TryConsumeRound(),Is.True);Assert.That(magazine.TryReload(),Is.True);yield return null;Assert.That(animator.GetLayerWeight(layer),Is.EqualTo(1));
            yield return new WaitForSeconds(.3f);Assert.That(magazine.IsReloading,Is.False);Assert.That(animator.GetLayerWeight(layer),Is.LessThan(.001f));
            Assert.That(magazine.TryConsumeRound(),Is.True);Assert.That(magazine.TryReload(),Is.True);magazine.CancelReload();yield return new WaitForSeconds(.15f);Assert.That(animator.GetLayerWeight(layer),Is.LessThan(.001f));
            Assert.That(magazine.TryReload(),Is.True);var presentation=c.Actor.GetComponentInChildren<PlayerCharacterCombatView>();presentation.enabled=false;Assert.That(animator.GetLayerWeight(layer),Is.Zero);
            presentation.enabled=true;Assert.That(animator.GetLayerWeight(layer),Is.Zero);
        }

        [UnityTest] public IEnumerator ProneReloadRetainsHorizontalBodyPosture()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();c.Shooter.enabled=true;CharacterViewTestContext.SetField(c.Shooter,"_input",new IdleInput());
            Assert.That(c.Actor.GetComponent<PlayerCharacterStance>().TrySetStance(CharacterStance.Prone),Is.True);
            var animator=c.Actor.GetComponentInChildren<Animator>();
            var prone=animator.GetComponent<PlayerCharacterProneAnimationView>();
            yield return new WaitForSeconds(c.Actor.GetComponent<PlayerCharacterStance>().ProneEnterDuration + .12f);
            float before=Mathf.Abs(Vector3.Dot(animator.bodyRotation*Vector3.up,c.Actor.up));Assert.That(before,Is.LessThan(.4f));
            Assert.That(c.Shooter.Magazine.TryConsumeRound(),Is.True);Assert.That(c.Shooter.Magazine.TryReload(),Is.True);
            for(int i=0;i<10;i++){yield return null;Assert.That(Mathf.Abs(Vector3.Dot(animator.bodyRotation*Vector3.up,c.Actor.up)),Is.LessThan(.4f));}
        }
        [UnityTest] public IEnumerator ReloadKeepsAuthoredShouldersInsteadOfLocomotionSwing()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            c.Shooter.enabled=true;CharacterViewTestContext.SetField(c.Shooter,"_input",new IdleInput());
            var animator=c.Actor.GetComponentInChildren<Animator>();animator.enabled=false;
            var motor=c.Actor.GetComponent<PlayerCharacterMovement>();motor.enabled=true;CharacterViewTestContext.SetField(motor,"_input",null);
            var stance=c.Actor.GetComponent<PlayerCharacterStance>();
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(1000,.5f,1000);Physics.SyncTransforms();
            float capture=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try{
                for(int view=0;view<2;view++)for(int crouch=0;crouch<2;crouch++)for(int moving=0;moving<2;moving++)
                {
                    if(view==1 && crouch==0 && moving==0)c.TogglePerspective();
                    Assert.That(stance.TrySetCrouched(crouch==1),Is.True);
                    for(int f=0;f<35;f++){motor.Tick(moving==1?Vector2.up:Vector2.zero,false,false,.02f);yield return null;animator.Update(.02f);}
                    c.Shooter.Magazine.ResetAmmo();Assert.That(c.Shooter.Magazine.TryConsumeRound(),Is.True);Assert.That(c.Shooter.Magazine.TryReload(),Is.True);
                    var root=c.Actor.rotation;var model=animator.transform.localRotation;float minYaw=999,maxYaw=-999,minRoll=999,maxRoll=-999;float handMotion=0;Vector3 previous=Vector3.zero;
                    for(int f=0;f<72;f++){
                        motor.Tick(moving==1?Vector2.up:Vector2.zero,false,false,.02f);yield return null;animator.Update(.02f);
                        Assert.That(c.Shooter.Magazine.IsReloading,Is.True);
                        Assert.That(animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("UpperCombat")).IsName("UpperCombat.PistolReload"),Is.True);
                        Vector3 shoulders=animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position-animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
                        var planar=Vector3.ProjectOnPlane(shoulders,c.Actor.up);float yaw=Vector3.SignedAngle(c.Actor.right,planar,c.Actor.up);float roll=Mathf.Atan2(Vector3.Dot(shoulders,c.Actor.up),planar.magnitude)*Mathf.Rad2Deg;
                        var hand=c.Actor.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftHand).position);
                        if(f>10){minYaw=Mathf.Min(minYaw,yaw);maxYaw=Mathf.Max(maxYaw,yaw);minRoll=Mathf.Min(minRoll,roll);maxRoll=Mathf.Max(maxRoll,roll);handMotion+=Vector3.Distance(hand,previous);}
                        previous=hand;
                        Assert.That(Quaternion.Angle(c.Actor.rotation,root),Is.LessThan(.01f));Assert.That(Quaternion.Angle(animator.transform.localRotation,model),Is.LessThan(.01f));
                    }
                    Debug.Log($"RELOAD_REGRESSION view={view} crouch={crouch} moving={moving} yawRange={maxYaw-minYaw} rollRange={maxRoll-minRoll}");
                    Assert.That(maxYaw-minYaw,Is.LessThan(45),"reload shoulder yaw");Assert.That(maxRoll-minRoll,Is.LessThan(22),"reload shoulder roll");Assert.That(handMotion,Is.GreaterThan(.3f),"Authored reload hand motion must remain active");
                    c.Shooter.Magazine.CancelReload();yield return new WaitForSeconds(.15f);animator.Update(.02f);
                    int reload=animator.GetLayerIndex("ReloadUpperBody");if(reload>=0)Assert.That(animator.GetLayerWeight(reload),Is.LessThan(.001f));
                }
            }finally{Time.captureDeltaTime=capture;animator.enabled=true;Object.Destroy(floor);}
        }
    }
}
#endif

