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
    public sealed class PlayerCharacterArmedStabilityTests
    {
        private sealed class IdleInput : IWeaponInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection => 0;
        }
        [UnityTest] public IEnumerator EquippedTorsoRetainsTheHoldPoseWhileRunningAndJumping()
        {
            float previous = Time.captureDeltaTime;
            GameObject floor = null;
            try
            {
                Time.captureDeltaTime = .02f;
                yield return CharacterViewTestContext.Load();
                var context = new CharacterViewTestContext();
                context.Shooter.enabled = true;
                CharacterViewTestContext.SetField(context.Shooter, "_input", new IdleInput());
                var motor = context.Actor.GetComponent<PlayerCharacterMovement>();
                motor.enabled = true;
                CharacterViewTestContext.SetField(motor, "_input", null);
                var animator = context.Actor.GetComponentInChildren<Animator>();
                Transform chest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
                floor.transform.localScale = new Vector3(1000f, .5f, 1000f);
                Physics.SyncTransforms();
                foreach (bool firstPerson in new[] { false, true })
                {
                    if (context.View.IsFirstPersonView != firstPerson) context.TogglePerspective();
                    for (int i = 0; i < 50; i++)
                    { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                    Quaternion neutral = Quaternion.Inverse(context.Actor.rotation) * chest.rotation;
                    Vector3 modelPosition = animator.transform.localPosition;
                    Quaternion modelRotation = animator.transform.localRotation;
                    Vector3 initialFoot = context.Actor.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
                    float startHeight = context.Actor.position.y, peakHeight = startHeight, footMotion = 0f;
                    float peak = 0f;
                    bool airborne = false;
                    for (int i = 0; i < 140; i++)
                    {
                        motor.Tick(Vector2.up, i < 60, i == 35, .02f);
                        yield return null;
                        airborne |= !motor.IsGrounded;
                        peakHeight = Mathf.Max(peakHeight, context.Actor.position.y);
                        footMotion = Mathf.Max(footMotion, Vector3.Distance(initialFoot,
                            context.Actor.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position)));
                        Assert.That(Vector3.Distance(modelPosition, animator.transform.localPosition), Is.LessThan(.0001f));
                        Assert.That(Quaternion.Angle(modelRotation, animator.transform.localRotation), Is.LessThan(.01f));
                        Quaternion local = Quaternion.Inverse(context.Actor.rotation) * chest.rotation;
                        peak = Mathf.Max(peak, Quaternion.Angle(neutral, local));
                    }
                    Debug.Log($"Equipped torso firstPerson={firstPerson}: peak deviation={peak:F3}deg");
                    Assert.That(airborne, Is.True);
                    Assert.That(peakHeight - startHeight, Is.EqualTo(1.2f).Within(.15f));
                    Assert.That(footMotion, Is.GreaterThan(.15f), "Leg animation must continue underneath the stable torso.");
                    Assert.That(peak, Is.LessThan(7f), "Armed locomotion must retain a steady torso hold pose.");
                }
            }
            finally { Time.captureDeltaTime = previous; if (floor != null) Object.Destroy(floor); }
        }
    }
}
#endif
