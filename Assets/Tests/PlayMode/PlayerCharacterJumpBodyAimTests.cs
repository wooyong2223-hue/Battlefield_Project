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
    public sealed class PlayerCharacterJumpBodyAimTests
    {
        private sealed class IdleInput : IWeaponInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection => 0;
        }

        [Test] public void StarterRootFollowUsesCinemachineDampingAndResetsAtPerspectiveChanges()
        {
            var follow = new PlayerCharacterJumpPoseState();
            follow.Tick(Vector3.zero, false, true, .02f);
            follow.Tick(Vector3.up, false, true, .25f);
            Assert.That(follow.Offset.y, Is.EqualTo(-.01f).Within(.00001f),
                "Starter Y damping .25 leaves Cinemachine's 1% residual after .25 seconds.");
            follow.Tick(Vector3.up, false, true, .25f);
            Assert.That(follow.Offset.y, Is.EqualTo(-.0001f).Within(.00001f));
            follow.Tick(Vector3.up, true, true, .02f);
            Assert.That(follow.Offset, Is.EqualTo(Vector3.zero));
            follow.Tick(Vector3.up * 2f, true, true, .02f);
            Assert.That(follow.Offset, Is.EqualTo(Vector3.zero), "Starter first person uses zero damping.");
            follow.Tick(Vector3.up * 2f, false, true, .02f);
            follow.Tick(Vector3.up * 3f, false, true, .02f);
            Assert.That(follow.Offset.y, Is.LessThan(0f));
            follow.Tick(Vector3.up * 3f, false, false, .02f);
            Assert.That(follow.Offset, Is.EqualTo(Vector3.zero));
        }

        [UnityTest] public IEnumerator ActualJumpUsesRootTrackingWithoutAnimationBoneMotionInBothViews()
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
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
                floor.transform.localScale = new Vector3(1000f, .5f, 1000f);
                Physics.SyncTransforms();
                foreach (bool firstPerson in new[] { false, true })
                foreach (bool changeAim in new[] { false, true })
                {
                    if (context.View.IsFirstPersonView != firstPerson) context.TogglePerspective();
                    for (int i = 0; i < 50; i++)
                    {
                        motor.Tick(Vector2.zero, false, false, .02f);
                        yield return null;
                    }
                    float peakOffset = 0f;
                    bool airborne = false;
                    for (int i = 0; i < 130; i++)
                    {
                        if (changeAim && (i == 20 || i == 35))
                        {
                            context.Input.Look = new Vector2(i == 20 ? 5f : -5f, i == 20 ? 10f : -10f);
                            context.View.Tick(context.Input, .02f);
                            context.Input.Look = Vector2.zero;
                        }
                        motor.Tick(Vector2.up, false, i == 10, .02f);
                        yield return null;
                        airborne |= !motor.IsGrounded;
                        peakOffset = Mathf.Max(peakOffset, context.Follow.JumpFollowOffset.magnitude);
                        Assert.That(Vector3.Angle(context.Camera.transform.forward,
                            context.View.LookRotation * Vector3.forward), Is.LessThan(.1f),
                            "Starter root tracking never adds animation bone rotation or jump pitch.");
                        if (firstPerson)
                            Assert.That(Mathf.Abs(context.Camera.transform.position.y - context.Follow.EyePosition.y),
                                Is.LessThan(.2f), "Head animation must not shift the first-person camera.");
                        Ray aim = context.Follow.GetAimRay();
                        Assert.That(Vector3.Distance(aim.direction, context.Follow.ResolvedAimRotation * Vector3.forward), Is.LessThan(.00001f));
                        Assert.That(Vector3.Angle(context.Camera.transform.forward, aim.direction), Is.LessThan(3f),
                            "Root follow must preserve camera and aiming direction.");
                        Quaternion weapon = context.Follow.ResolvedAimRotation;
                        if (firstPerson) weapon *= Quaternion.Euler(context.Follow.FirstPersonWeaponEuler);
                        Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, weapon * Vector3.forward), Is.LessThan(2f),
                            $"The gun must keep mouse aim during root follow: firstPerson={firstPerson}, frame={i}");
                    }
                    Assert.That(airborne, Is.True);
                    if (firstPerson) Assert.That(peakOffset, Is.LessThan(.0001f));
                    else Assert.That(peakOffset, Is.GreaterThan(.01f), "Starter third-person damping follows root ascent/descent.");
                    Assert.That(context.Follow.JumpFollowOffset.magnitude, Is.LessThan(.001f), "Root follow settles after landing.");
                    Debug.Log($"Starter root jump firstPerson={firstPerson}: changeAim={changeAim}, peak correction={peakOffset:F3}m");
                }
            }
            finally
            {
                Time.captureDeltaTime = previous;
                if (floor != null) Object.Destroy(floor);
            }
        }
    }
}
#endif
