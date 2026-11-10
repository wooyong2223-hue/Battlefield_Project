#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public sealed class ProneAnimationTests
    {
        private sealed class Idle : IWeaponInput { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }
        [UnityTest]
        public IEnumerator OriginalsOwnTheFullBodyAndReleaseAfterExitOrSuspension()
        {
            yield return CharacterViewTestContext.Load(); var c = new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter, "_input", new Idle()); c.Shooter.enabled = true;
            var animator = c.Actor.GetComponentInChildren<Animator>(); var prone = animator.GetComponent<PlayerCharacterProneAnimationView>();
            if (prone == null) { Assert.Ignore("Optional local prone originals are absent."); yield break; }
            var serialized = new UnityEditor.SerializedObject(prone);
            Assert.That(serialized.FindProperty("_animator").objectReferenceValue, Is.SameAs(animator));
            Assert.That(serialized.FindProperty("_animations").objectReferenceValue, Is.SameAs(prone.Animations));
            foreach (int slot in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            { var clip = prone.Animations.GetClip(slot); Assert.That(clip, Is.Not.Null); Assert.That(clip.humanMotion, Is.True); }
            var motor = c.Actor.GetComponent<PlayerCharacterMovement>(); CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            var stance = c.Actor.GetComponent<PlayerCharacterStance>();
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position = c.Actor.position + Vector3.down * 2.25f;
            floor.transform.localScale = new Vector3(100, .5f, 100); Physics.SyncTransforms();
            float previous = Time.captureDeltaTime; Time.captureDeltaTime = .02f;
            try
            {
                for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(1); yield return null;
                var controller = animator.runtimeAnimatorController;
                Assert.That(stance.TrySetStance(CharacterStance.Prone), Is.True);
                Vector3 enterStart = c.Actor.position;
                motor.QueueExternalDisplacement(Vector3.right * 2f);
                int enterFrames = 0;
                while (stance.IsProneTransitioning)
                {
                    motor.Tick(Vector2.up, true, true, .02f); yield return null; enterFrames++;
                    Assert.That(Vector2.Distance(new Vector2(c.Actor.position.x, c.Actor.position.z), new Vector2(enterStart.x, enterStart.z)), Is.LessThan(.001f), "Entry must block input, inertia, and queued horizontal displacement.");
                    Assert.That(enterFrames, Is.LessThan(40));
                }
                Assert.That(enterFrames * .02f, Is.InRange(stance.ProneEnterDuration - .03f, stance.ProneEnterDuration + .03f));
                yield return null;
                Assert.That(prone.IsPresenting, Is.True);
                foreach (string layer in new[] { "UpperCombat", "MeleeCombat", "ReloadUpperBody" }) Assert.That(animator.GetLayerWeight(animator.GetLayerIndex(layer)), Is.LessThan(.001f));
                Assert.That(Vector3.Angle(c.Shooter.Muzzle.forward, c.View.AimRotation * Vector3.forward), Is.LessThan(2f));
                Vector3 crawlStart = c.Actor.position;
                var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                Vector3 previousFoot = c.Actor.InverseTransformPoint(foot.position);
                float footTravel = 0f;
                for (int i = 0; i < 70; i++)
                {
                    motor.Tick(Vector2.up, false, false, .02f); yield return null;
                    Vector3 currentFoot = c.Actor.InverseTransformPoint(foot.position);
                    footTravel += Vector3.Distance(previousFoot, currentFoot); previousFoot = currentFoot;
                }
                Assert.That(Vector3.Distance(crawlStart, c.Actor.position), Is.EqualTo(stance.MoveSpeed * 70f * .02f).Within(.1f), "Forward crawling must use the configured faster movement speed.");
                Assert.That(footTravel, Is.GreaterThan(.1f), "Forward movement must animate the legs rather than slide in the prone idle pose.");
                for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                Assert.That(prone.Animations.GetClip(3).name, Is.EqualTo("Prone Left Turn"));
                Assert.That(prone.Animations.GetClip(4).name, Is.EqualTo("Prone Right Turn"));
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector3 sideStart = c.Actor.position;
                    Quaternion heading = c.Actor.rotation, aim = c.View.AimRotation;
                    for (int i = 0; i < 60; i++) { motor.Tick(Vector2.right * side, false, false, .02f); yield return null; }
                    Assert.That(Vector3.Dot(c.Actor.position - sideStart, c.Actor.right) * side, Is.GreaterThan(.3f));
                    Assert.That(Quaternion.Angle(heading, c.Actor.rotation), Is.LessThan(.01f), "Turn animations must not rotate the gameplay root.");
                    Assert.That(Quaternion.Angle(aim, c.View.AimRotation), Is.LessThan(.01f), "Sideways motion must preserve the aim direction.");
                    Assert.That(animator.applyRootMotion, Is.False);
                    for (int i = 0; i < 20; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                }
                Vector3 fistBefore = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                typeof(PlayerCharacterController).GetMethod("TryMelee", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(c.Shooter, null);
                Assert.That(c.Shooter.IsAttacking, Is.True);
                for (int i = 0; i < 10; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                Assert.That(Vector3.Distance(fistBefore, animator.GetBoneTransform(HumanBodyBones.RightHand).position), Is.GreaterThan(.1f), "Prone playback must not hide the authored upper-body melee action.");
                float feet = c.Actor.GetComponent<CharacterController>().bounds.min.y;
                Assert.That(animator.GetBoneTransform(HumanBodyBones.Head).position.y - feet, Is.InRange(.2f, 1.5f), "The upper-body strike must not replace the prone root/legs with a standing pose.");
                for (int i = 0; i < 40; i++) { motor.Tick(Vector2.zero, false, false, .02f); yield return null; }
                Assert.That(c.Shooter.Magazine.TryConsumeRound(), Is.True); Assert.That(c.Shooter.Magazine.TryReload(), Is.True);
                yield return null; Assert.That(prone.IsReloading, Is.True);
                c.Shooter.Magazine.CancelReload(); yield return null; Assert.That(prone.IsReloading, Is.False);
                prone.enabled = false; Assert.That(prone.IsPresenting, Is.False); prone.enabled = true; yield return null; Assert.That(prone.IsPresenting, Is.True);
                Assert.That(stance.TrySetStance(CharacterStance.Standing), Is.True);
                Vector3 exitStart = c.Actor.position;
                int exitFrames = 0;
                while (stance.IsProneTransitioning)
                {
                    motor.Tick(Vector2.right, true, true, .02f); yield return null; exitFrames++;
                    Assert.That(Vector2.Distance(new Vector2(c.Actor.position.x, c.Actor.position.z), new Vector2(exitStart.x, exitStart.z)), Is.LessThan(.001f), "Exit must remain stationary despite movement and jump input.");
                    Assert.That(exitFrames, Is.LessThan(35));
                }
                Assert.That(exitFrames * .02f, Is.InRange(stance.ProneExitDuration - .03f, stance.ProneExitDuration + .03f));
                yield return null;
                motor.Tick(Vector2.up, false, false, .02f); yield return null;
                Assert.That(Vector3.Distance(exitStart, c.Actor.position), Is.GreaterThan(.01f), "Movement must resume after the exit transition.");
                Assert.That(prone.IsPresenting, Is.False);
                Assert.That(animator.runtimeAnimatorController, Is.SameAs(controller), "The optional graph must not replace the equipment controller.");
            }
            finally { Time.captureDeltaTime = previous; Object.Destroy(floor); }
        }
    }
}
#endif
