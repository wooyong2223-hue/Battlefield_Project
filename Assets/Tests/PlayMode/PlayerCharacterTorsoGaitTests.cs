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
    public sealed class PlayerCharacterTorsoGaitTests
    {
        private sealed class PassiveWeaponInput : IWeaponInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection { get; set; }
        }

        [UnityTest]
        public IEnumerator ArmedGaitsLimitShoulderSwingInBothPerspectives() => CheckGaits(true);

        [UnityTest]
        public IEnumerator UnarmedGaitsLimitShoulderSwingInBothPerspectives() => CheckGaits(false);

        private static IEnumerator CheckGaits(bool armed)
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            var motor = context.Actor.GetComponent<PlayerCharacterMovement>();
            var stance = context.Actor.GetComponent<PlayerCharacterStance>();
            var animator = context.Actor.GetComponentInChildren<Animator>();
            var input = new PassiveWeaponInput { WeaponSlotSelection = armed ? PlayerCharacterLoadout.PistolSlot : PlayerCharacterLoadout.UnarmedSlot };
            CharacterViewTestContext.SetField(context.Shooter, "_input", input);
            context.Shooter.enabled = true;
            CharacterViewTestContext.SetField(motor, "_input", null);
            CharacterViewTestContext.SetField(animator.GetComponent<PlayerCharacterAnimationView>(), "_speedDamping", 0f);
            motor.enabled = true;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Torso Gait Test Floor";
            floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(1000f, .5f, 1000f);
            Physics.SyncTransforms();
            Quaternion rootRotation = context.Actor.rotation;
            Quaternion modelRotation = animator.transform.localRotation;
            try
            {
                for (int perspective = 0; perspective < 2; perspective++)
                {
                    if (perspective == 1) context.TogglePerspective();
                    // Exercise combined pitch/yaw, not just a level gaze.
                    context.Input.Look = new Vector2(0f, perspective == 0 ? -10f : 20f);
                    context.View.Tick(context.Input, .02f);
                    context.Input.Look = Vector2.zero;
                    foreach (string gait in new[] { "walk", "jog", "sprint", "left", "diagonal", "crouch" })
                    {
                        Assert.That(stance.TrySetCrouched(gait == "crouch"), Is.True);
                        Vector2 move = gait == "left" ? Vector2.left : gait == "diagonal" ? new Vector2(1f, 1f).normalized : Vector2.up * (gait == "walk" ? .5f : 1f);
                        float maximumYaw = 0f;
                        float maximumRoll = 0f;
                        for (int frame = 0; frame < 180; frame++)
                        {
                            motor.Tick(move, gait == "sprint", false, .02f);
                            yield return null;
                            animator.Update(.02f);
                            if (frame < 30) continue;
                            Vector3 shoulders = animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position -
                                                animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
                            float yaw = Vector3.Angle(context.Actor.right, Vector3.ProjectOnPlane(shoulders, context.Actor.up));
                            maximumYaw = Mathf.Max(maximumYaw, yaw);
                            float roll = Mathf.Abs(Mathf.Atan2(Vector3.Dot(shoulders, context.Actor.up), Vector3.ProjectOnPlane(shoulders, context.Actor.up).magnitude) * Mathf.Rad2Deg);
                            maximumRoll = Mathf.Max(maximumRoll, roll);
                            Assert.That(roll, Is.LessThan(4f), "shoulder roll, armed=" + armed + " gait=" + gait + " frame=" + frame);
                            Assert.That(yaw, Is.LessThan(5f), "armed=" + armed + " perspective=" + perspective + " gait=" + gait + " frame=" + frame);
                            Assert.That(Quaternion.Angle(context.Actor.rotation, rootRotation), Is.LessThan(.01f));
                            Assert.That(Quaternion.Angle(animator.transform.localRotation, modelRotation), Is.LessThan(.01f));
                            if (armed)
                            {
                                Vector3 head = animator.GetBoneTransform(HumanBodyBones.Head).forward;
                                Assert.That(Vector3.Angle(context.Actor.forward, Vector3.ProjectOnPlane(head, context.Actor.up)), Is.LessThan(5f));
                                float verticalAim = Vector3.Dot(context.View.AimRotation * Vector3.forward, context.Actor.up);
                                Assert.That(Vector3.Dot(head, context.Actor.up) * Mathf.Sign(verticalAim), Is.GreaterThan(.2f));
                            }
                        }
                        Debug.Log("TORSO_GAIT armed=" + armed + " perspective=" + perspective + " gait=" + gait + " maximumShoulderYaw=" + maximumYaw + " maximumShoulderRoll=" + maximumRoll);
                    }
                }
            }
            finally { Object.Destroy(floor); }
        }
    }
}
#endif
