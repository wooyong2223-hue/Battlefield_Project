#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterMotionTests
    {
        private class PassiveWeaponInput : Battlefield.Features.Weapon.IWeaponInput
        { public bool FireWeapon { get; set; } public int WeaponSlotSelection => 0; }
        private class WalkingInput : IPlayerCharacterInput
        { public Vector2 Move => Vector2.up; public bool SprintHeld => false; public bool JumpPressed => false; }
        private PlayerCharacterMotionProfile _profile;
        private readonly List<GameObject> _objects = new();
        [SetUp] public void Setup() => _profile = ScriptableObject.CreateInstance<PlayerCharacterMotionProfile>();
        [TearDown] public void Cleanup()
        {
            Object.DestroyImmediate(_profile);
            foreach (var item in _objects) if (item != null) Object.DestroyImmediate(item);
            _objects.Clear();
        }
        private PlayerCharacterMotionState State() => new(_profile);
        private static void Step(PlayerCharacterMotionState state, Vector3 velocity, bool grounded = true,
            bool sprint = false, bool ads = false, float seconds = 1f, int fps = 60)
        {
            int frames = Mathf.RoundToInt(seconds * fps);
            for (int i = 0; i < frames; i++) state.Tick(velocity, grounded, sprint, ads, Quaternion.identity, 8f, true, 1f / fps);
        }
        [Test]
        public void JumpInertiaFollowsVerticalVelocityAndRecoversWithoutDrift()
        {
            var state = State(); Step(state, Vector3.zero);
            Step(state, Vector3.up * 5f, grounded: false, seconds: .2f);
            Assert.That(state.JumpCameraOffset.y, Is.LessThan(-.03f));
            Assert.That(state.CameraPitch, Is.LessThan(0f));
            Step(state, Vector3.down * 5f, grounded: false, seconds: .4f);
            Assert.That(state.JumpCameraOffset.y, Is.GreaterThan(.03f));
            Step(state, Vector3.zero, seconds: 2f);
            Assert.That(state.JumpCameraOffset.magnitude, Is.LessThan(.0001f));
            Assert.That(Mathf.Abs(state.CameraPitch), Is.LessThan(.001f));
            state.Tick(Vector3.up * 5f, false, false, false, Quaternion.identity, 8f, false, .02f);
            Assert.That(state.JumpCameraOffset, Is.EqualTo(Vector3.zero));
            Assert.That(state.CameraPitch, Is.Zero);
        }

        [Test]
        public void JumpCameraLagIsBoundedAndAdsStabilizesTheReaction()
        {
            var normal = State(); var ads = State();
            Step(normal, Vector3.zero); Step(ads, Vector3.zero, ads: true);
            Step(normal, Vector3.down * 100f, grounded: false, seconds: .5f);
            Step(ads, Vector3.down * 100f, grounded: false, ads: true, seconds: .5f);
            Assert.That(normal.JumpCameraOffset.y, Is.InRange(.05f, _profile.MaximumJumpLag + .0001f));
            Assert.That(ads.JumpCameraOffset.magnitude, Is.LessThan(normal.JumpCameraOffset.magnitude * .15f));
            Assert.That(Mathf.Abs(ads.CameraPitch), Is.LessThan(Mathf.Abs(normal.CameraPitch) * .15f));
        }

        [Test]
        public void JumpCameraResponseMatchesAtThirtyAndOneHundredTwentyFps()
        {
            PlayerCharacterMotionState Simulate(int fps)
            {
                var state = State(); Step(state, Vector3.zero, fps: fps);
                Step(state, Vector3.up * 5f, grounded: false, seconds: .2f, fps: fps);
                Step(state, Vector3.down * 5f, grounded: false, seconds: .4f, fps: fps);
                Step(state, Vector3.zero, seconds: .5f, fps: fps); return state;
            }
            var low = Simulate(30); var high = Simulate(120);
            Assert.That(Vector3.Distance(low.JumpCameraOffset, high.JumpCameraOffset), Is.LessThan(.0001f));
            Assert.That(Mathf.Abs(low.CameraPitch - high.CameraPitch), Is.LessThan(.001f));
        }

        [UnityTest]
        public IEnumerator JumpCameraRespondsInBothViewsWithoutChangingAimOrMovementSettings()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            var motor = context.Actor.GetComponent<PlayerCharacterMovement>();
            CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true;
            var floor = new GameObject("Jump camera test floor"); _objects.Add(floor);
            floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(500f, .5f, 500f); Physics.SyncTransforms();
            var state = (PlayerCharacterMotionState)typeof(PlayerCharacterCamera).GetField("_motion",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(context.Follow);
            foreach (bool firstPerson in new[] { false, true })
            {
                if (context.View.IsFirstPersonView != firstPerson) context.TogglePerspective();
                float settled = Time.time + .3f;
                while (Time.time < settled) { motor.Tick(Vector2.zero, false, false, Time.deltaTime); yield return null; }
                Assert.That(motor.IsGrounded, Is.True);
                Quaternion aim = context.View.AimRotation; float maxPitch = 0f, minLag = 0f; bool airborne = false;
                motor.Tick(Vector2.up, false, true, .02f); yield return null;
                float end = Time.time + 2.5f;
                while (Time.time < end)
                {
                    motor.Tick(Vector2.zero, false, false, Time.deltaTime); yield return null;
                    airborne |= !motor.IsGrounded;
                    minLag = Mathf.Min(minLag, state.JumpCameraOffset.y);
                    maxPitch = Mathf.Max(maxPitch, Mathf.Abs(Mathf.DeltaAngle(0f,
                        (Quaternion.Inverse(context.View.LookRotation) * context.Camera.transform.rotation).eulerAngles.x)));
                }
                Assert.That(airborne, Is.True); Assert.That(minLag, Is.LessThan(-.02f));
                Assert.That(maxPitch, Is.LessThan(.1f), "Starter Assets root follow adds no jump pitch.");
                Assert.That(Quaternion.Angle(aim, context.View.AimRotation), Is.LessThan(.001f));
                Assert.That(state.JumpCameraOffset.magnitude, Is.LessThan(.001f));
                Assert.That(motor.MoveSpeed, Is.EqualTo(8f)); Assert.That(motor.SprintSpeed, Is.EqualTo(12f));
                Assert.That(context.Camera.fieldOfView, Is.EqualTo(60f).Within(.01f));
            }
        }

        [Test]
        public void IdleHasNoDriftAndInitializationDoesNotInventALanding()
        {
            var state = State(); Step(state, Vector3.zero, seconds: 3f);
            Assert.That(state.CameraOffset, Is.EqualTo(Vector3.zero));
            Assert.That(state.WeaponOffset, Is.EqualTo(Vector3.zero));
            Assert.That(state.CameraRoll, Is.Zero);
        }
        [Test]
        public void ActualTravelDrivesGaitAndAdsReducesItsAmplitude()
        {
            var walk = State(); var ads = State(); float walkMax = 0f, adsMax = 0f;
            Step(walk, Vector3.forward * 8f); Step(ads, Vector3.forward * 8f, ads: true);
            for (int i = 0; i < 120; i++)
            {
                walk.Tick(Vector3.forward * 8f, true, false, false, Quaternion.identity, 8f, true, 1f / 60f);
                ads.Tick(Vector3.forward * 8f, true, false, true, Quaternion.identity, 8f, true, 1f / 60f);
                walkMax = Mathf.Max(walkMax, walk.CameraOffset.magnitude);
                adsMax = Mathf.Max(adsMax, ads.CameraOffset.magnitude);
            }
            Assert.That(walkMax, Is.GreaterThan(.01f));
            Assert.That(adsMax, Is.LessThan(walkMax * .15f));
            Step(walk, Vector3.zero);
            Assert.That(walk.CameraOffset.magnitude, Is.LessThan(.0001f), "No footsteps while stopped or blocked.");
        }
        [Test]
        public void TurnSwayIsBoundedAndSettlesInsteadOfAccumulating()
        {
            var state = State(); Step(state, Vector3.zero);
            var turn = Quaternion.Euler(0f, 90f, 0f);
            state.Tick(Vector3.zero, true, false, false, turn, 8f, true, .02f);
            Assert.That(state.WeaponEuler.magnitude, Is.InRange(.2f, _profile.MaximumSway));
            for (int i = 0; i < 100; i++) state.Tick(Vector3.zero, true, false, false, turn, 8f, true, .02f);
            Assert.That(state.WeaponEuler.magnitude, Is.LessThan(.0001f));
        }
        [Test]
        public void LandingIsASingleDampedImpulseAndRecoversAtDifferentFrameRates()
        {
            foreach (int fps in new[] {30, 120})
            {
                var state = State(); Step(state, Vector3.down * 10f, grounded: false, seconds: .5f, fps: fps);
                state.Tick(Vector3.zero, true, false, false, Quaternion.identity, 8f, true, 1f / fps);
                Assert.That(state.CameraOffset.y, Is.LessThan(0f));
                Assert.That(state.CameraOffset.y, Is.GreaterThan(-.06f));
                Step(state, Vector3.zero, seconds: 1f, fps: fps);
                Assert.That(Mathf.Abs(state.CameraOffset.y), Is.LessThan(.0001f));
            }
        }
        [Test]
        public void GaitIsFrameRateIndependentAndDisableClearsAllPresentationState()
        {
            var low = State(); var high = State();
            Step(low, Vector3.forward * 8f, fps: 30); Step(high, Vector3.forward * 8f, fps: 120);
            Assert.That(Vector3.Distance(low.CameraOffset, high.CameraOffset), Is.LessThan(.0001f));
            high.Tick(Vector3.forward * 12f, true, true, false, Quaternion.identity, 8f, false, .02f);
            Assert.That(high.CameraOffset, Is.EqualTo(Vector3.zero));
            Assert.That(high.WeaponOffset, Is.EqualTo(Vector3.zero));
            Assert.That(high.WeaponEuler, Is.EqualTo(Vector3.zero));
            high.Reset(); Assert.That(high.AimBlend, Is.Zero);
        }
        [Test]
        public void SprintPoseAndAimTransitionBlendContinuouslyAndReturnToTheAuthoredAim()
        {
            var state = State(); Step(state, Vector3.forward * 12f, sprint: true);
            Assert.That(state.WeaponOffset.y, Is.LessThan(-.03f));
            Assert.That(state.WeaponEuler.x, Is.EqualTo(_profile.SprintEuler.x).Within(.01f));
            state.Tick(Vector3.zero, true, false, true, Quaternion.identity, 8f, true, .02f);
            Assert.That(state.AimBlend, Is.InRange(.01f, .99f));
            Step(state, Vector3.zero, ads: true);
            Assert.That(state.AimBlend, Is.GreaterThan(.999f));
            Assert.That(state.WeaponOffset.magnitude, Is.LessThan(.0001f));
            Assert.That(state.WeaponEuler.magnitude, Is.LessThan(.0001f));
        }
        private PlayerCharacterMovement Motor(float x = 0f)
        {
            var floor = new GameObject("Motion response floor"); _objects.Add(floor);
            floor.transform.position = new Vector3(2000f + x, 999.75f, 2000f);
            floor.AddComponent<BoxCollider>().size = new Vector3(60f, .5f, 60f);
            var actor = new GameObject("Motion response actor"); _objects.Add(actor);
            actor.transform.position = new Vector3(2000f + x, 1002.05f, 2000f);
            var body = actor.AddComponent<CharacterController>(); body.height = 4f; body.radius = .5f; body.skinWidth = .05f;
            var motor = actor.AddComponent<PlayerCharacterMovement>(); Physics.SyncTransforms();
            for (int i = 0; i < 20; i++) motor.Tick(Vector2.zero, false, false, .02f);
            return motor;
        }
        [Test]
        public void MotorAcceleratesBrakesAndChangesDirectionWithoutAnInstantVelocityFlip()
        {
            var motor = Motor(); Assert.That(motor.IsGrounded, Is.True);
            motor.Tick(Vector2.up, false, false, .02f);
            Assert.That(motor.Velocity.z, Is.InRange(.5f, 3f));
            for (int i = 0; i < 10; i++) motor.Tick(Vector2.up, false, false, .02f);
            Assert.That(motor.Velocity.z, Is.EqualTo(8f).Within(.1f));
            motor.Tick(Vector2.left, false, false, .02f);
            Assert.That(motor.Velocity.x, Is.LessThan(0f)); Assert.That(motor.Velocity.z, Is.GreaterThan(0f));
            motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(Vector3.ProjectOnPlane(motor.Velocity, Vector3.up).magnitude, Is.GreaterThan(0f));
            for (int i = 0; i < 10; i++) motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(Vector3.ProjectOnPlane(motor.Velocity, Vector3.up).magnitude, Is.LessThan(.01f));
        }
        [Test]
        public void AirControlRetainsMomentumAndStopClearsItForBoarding()
        {
            var motor = Motor();
            for (int i = 0; i < 15; i++) motor.Tick(Vector2.up, false, false, .02f);
            motor.Tick(Vector2.up, false, true, .02f); Assert.That(motor.IsGrounded, Is.False);
            motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(motor.Velocity.z, Is.InRange(7f, 8f));
            motor.Stop(); Assert.That(motor.Velocity, Is.EqualTo(Vector3.zero));
            motor.Tick(Vector2.zero, false, false, .02f); Assert.That(Mathf.Abs(motor.Velocity.z), Is.LessThan(.01f));
        }
        [UnityTest]
        public IEnumerator FirstPersonGaitAimHitsEnemyAndNeutralButNotAlliesFromTheAlliedDummyOwner()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Assert.That(context.Shooter.GetComponent<Battlefield.Features.Training.DummyShooter>(), Is.Not.Null);
            Assert.That(context.Shooter.GetComponent<Battlefield.Framework.Core.Team>().CurrentTeam,
                Is.EqualTo(Battlefield.Framework.Core.TeamType.Player));
            var floor = new GameObject("First person dummy firing range"); _objects.Add(floor);
            floor.transform.position = new Vector3(2100f, 999.75f, 3000f);
            floor.AddComponent<BoxCollider>().size = new Vector3(600f, .5f, 100f);
            var movement = context.Actor.GetComponent<PlayerCharacterMovement>();
            var body = context.Actor.GetComponent<CharacterController>();
            var fire = context.EnableCombatInput();
            CharacterViewTestContext.SetField(movement, "_input", new WalkingInput());
            movement.enabled = true; context.TogglePerspective();
            string[] targets = {"Dummy_Enemy", "Dummy_Neutral", "Ally_10m"};
            for (int i = 0; i < targets.Length; i++)
            {
                body.enabled = false; context.Actor.position = new Vector3(2000f + 100f * i, 1002.05f, 3000f);
                movement.Stop(); body.enabled = true; Physics.SyncTransforms();
                yield return new WaitForSeconds(.25f);
                Battlefield.Framework.Core.Health target = null;
                foreach (var health in Object.FindObjectsByType<Battlefield.Framework.Core.Health>())
                    if (health.name == targets[i]) target = health;
                Assert.That(target, Is.Not.Null);
                if (i == 2) CharacterViewTestContext.SetField(target.GetComponent<Battlefield.Framework.Core.Team>(), "_team", Battlefield.Framework.Core.TeamType.Player);
                target.transform.SetParent(null, true);
                target.transform.position = context.Follow.EyePosition + Vector3.forward * 20f;
                Physics.SyncTransforms(); float before = target.CurrentHealth;
                fire.PressFire(); yield return null; fire.FireWeapon = false;
                yield return new WaitForSeconds(.25f);
                Assert.That(target.CurrentHealth, Is.EqualTo(before - (i == 2 ? 0f : 10f)), targets[i]);
                Assert.That(context.View.IsFirstPersonView, Is.True);
            }
        }

        [UnityTest]
        public IEnumerator SceneWiringProducesFirstPersonFeedbackAndKeepsTheAimRayAtTheVisualEye()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            var motor = context.Actor.GetComponent<PlayerCharacterMovement>();
            Assert.That(typeof(PlayerCharacterCamera).GetField("_motion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(context.Follow), Is.Not.Null,
                "SampleScene must instantiate feedback from a real profile reference.");
            var floor = new GameObject("Scene motion floor"); _objects.Add(floor);
            floor.transform.position = context.Actor.position + Vector3.down * 2.25f;
            floor.AddComponent<BoxCollider>().size = new Vector3(200f, .5f, 200f);
            CharacterViewTestContext.SetField(motor, "_input", null); motor.enabled = true; Physics.SyncTransforms();
            CharacterViewTestContext.SetField(context.Shooter, "_input", new PassiveWeaponInput()); context.Shooter.enabled = true;
            context.TogglePerspective(); float maxOffset = 0f, ends = Time.time + .8f;
            while (Time.time < ends)
            {
                motor.Tick(Vector2.up, false, false, Time.deltaTime); yield return null;
                maxOffset = Mathf.Max(maxOffset, Vector3.Distance(context.Camera.transform.position, context.Follow.EyePosition));
            }
            Assert.That(maxOffset, Is.InRange(.006f, .06f));
            Assert.That(Vector3.Distance(context.Follow.GetAimRay().origin, context.Camera.transform.position), Is.LessThan(.005f));
            Assert.That(context.Camera.fieldOfView, Is.EqualTo(60f).Within(.01f));
            Assert.That(motor.MoveSpeed, Is.EqualTo(8f)); Assert.That(motor.SprintSpeed, Is.EqualTo(12f));
            ends = Time.time + .4f;
            while (Time.time < ends) { motor.Tick(Vector2.up, true, false, Time.deltaTime); yield return null; }
            Assert.That(motor.IsSprinting, Is.True);
            Vector3 muzzle = context.Camera.WorldToViewportPoint(context.Shooter.Muzzle.position);
            Assert.That(muzzle.z, Is.GreaterThan(0f));
            Assert.That(muzzle.y, Is.InRange(.05f, .4f), "The sprint pose must keep the pistol in view.");
            context.Follow.enabled = false;
            Assert.That(context.Follow.FirstPersonWeaponOffset, Is.EqualTo(Vector3.zero));
        }
    }
}
#endif
