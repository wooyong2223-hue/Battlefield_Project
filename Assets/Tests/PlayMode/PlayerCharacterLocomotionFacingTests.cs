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
    public sealed class PlayerCharacterLocomotionFacingTests
    {
        private sealed class PassiveWeaponInput : IWeaponInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection => 0;
        }

        [UnityTest]
        public IEnumerator GroundedGaitsKeepThePelvisFacingTheCharacterHeadingAcrossFullCycles()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            var motor = context.Actor.GetComponent<PlayerCharacterMovement>();
            var stance = context.Actor.GetComponent<PlayerCharacterStance>();
            var animator = context.Actor.GetComponentInChildren<Animator>();
            CharacterViewTestContext.SetField(context.Shooter, "_input", new PassiveWeaponInput());
            context.Shooter.enabled = true;
            CharacterViewTestContext.SetField(motor, "_input", null);
            CharacterViewTestContext.SetField(animator.GetComponent<PlayerCharacterAnimationView>(), "_speedDamping", 0f);
            motor.enabled = true;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Locomotion Facing Test Floor";
            floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(500f, .5f, 500f);
            Physics.SyncTransforms();
            Quaternion modelRotation = animator.transform.localRotation;
            Quaternion heading = context.Actor.rotation;
            try
            {
                foreach (int slot in new[] {PlayerCharacterLoadout.PistolSlot, PlayerCharacterLoadout.RifleSlot})
                {
                var loadout = context.Actor.GetComponent<PlayerCharacterLoadout>();
                loadout.TryEquip(slot); Assert.That(loadout.CurrentSlot, Is.EqualTo(slot));
                foreach (string gait in new[] { "idle", "walk", "jog", "sprint", "crouch-idle", "crouch-walk" })
                {
                    Assert.That(stance.TrySetCrouched(gait.StartsWith("crouch")), Is.True);
                    Vector2 move = gait.EndsWith("idle") ? Vector2.zero : Vector2.up * (gait == "walk" ? .5f : 1f);
                    float maximumYaw = 0f;
                    for (int frame = 0; frame < 210; frame++)
                    {
                        motor.Tick(move, gait == "sprint", false, .02f);
                        yield return null;
                        animator.Update(.02f);
                        if (frame < 30) continue;
                        Vector3 lateral = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position -
                                          animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
                        Vector3 forward = Vector3.ProjectOnPlane(Vector3.Cross(lateral, context.Actor.up), context.Actor.up);
                        float yaw = Vector3.Angle(context.Actor.forward, forward);
                        maximumYaw = Mathf.Max(maximumYaw, yaw);
                        Assert.That(yaw, Is.LessThan(20f), gait + " pelvis yaw=" + yaw + " frame=" + frame);
                        Assert.That(Quaternion.Angle(context.Actor.rotation, heading), Is.LessThan(.01f));
                        Assert.That(Quaternion.Angle(animator.transform.localRotation, modelRotation), Is.LessThan(.01f));
                        Assert.That(animator.applyRootMotion, Is.False);
                    }
                    Debug.Log("LOCOMOTION_FACING slot=" + slot + " " + gait + " maximumPelvisYaw=" + maximumYaw);
                }
                }
            }
            finally { Object.Destroy(floor); }
        }
    }
}
#endif
