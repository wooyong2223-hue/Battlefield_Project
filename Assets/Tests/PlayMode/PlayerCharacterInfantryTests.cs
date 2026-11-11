#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterInfantryTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly Vector3 _origin = new(60000f, 10000f, 60000f);
        private sealed class CrouchInput : IPlayerCharacterTraversalInput { public bool SlidePressed { get; set; } public bool PronePressed => false; }
        private sealed class FireInput : IWeaponInput { public bool FireWeapon { get; set; } public int WeaponSlotSelection => 0; }
        private sealed class TacticalInput : IPlayerCharacterTacticalInput { public float Lean { get; set; } public bool MountHeld => false; }
        private sealed class HeldInteraction : IPlayerCharacterLifeInput { public bool InteractHeld => true; public bool GiveUpPressed => false; public bool RespawnPressed => false; }
        private static void Set(object target, string field, object value) => CharacterViewTestContext.SetField(target, field, value);
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (GameObject item in _objects) if (item != null) Object.Destroy(item);
            _objects.Clear(); yield return null;
        }
        private GameObject Box(Vector3 position, Vector3 size)
        {
            var item = new GameObject("Infantry test obstacle"); _objects.Add(item);
            item.transform.position = position; item.AddComponent<BoxCollider>().size = size; Physics.SyncTransforms(); return item;
        }
        private PlayerCharacterMovement Motor()
        {
            Box(_origin + Vector3.down * .25f, new Vector3(100f, .5f, 100f));
            var actor = new GameObject("Infantry test actor"); _objects.Add(actor); actor.transform.position = _origin + Vector3.up * 2.05f;
            var body = actor.AddComponent<CharacterController>(); body.height = 4f; body.radius = .5f; body.center = Vector3.zero; body.skinWidth = .05f;
            var stance = actor.AddComponent<PlayerCharacterStance>();
            // Geometry-only tests use instant posture changes; native transition locking has its own PlayMode coverage.
            Set(stance, "_proneEnterDuration", 0f); Set(stance, "_proneExitDuration", 0f);
            var motor = actor.AddComponent<PlayerCharacterMovement>();
            Set(motor, "_stance", stance); Physics.SyncTransforms();
            for (int i = 0; i < 5; i++) motor.Tick(Vector2.zero, false, false, .02f);
            return motor;
        }
        private PlayerCharacterTraversal Traversal(PlayerCharacterMovement motor)
        {
            var traversal = motor.gameObject.AddComponent<PlayerCharacterTraversal>();
            Set(traversal, "_movement", motor); Set(traversal, "_stance", motor.GetComponent<PlayerCharacterStance>());
            Set(motor, "_traversal", traversal); return traversal;
        }

        [Test]
        public void PronePreservesFeetUsesCrawlSpeedAndCannotJumpOrSprint()
        {
            var motor = Motor(); var stance = motor.GetComponent<PlayerCharacterStance>(); var body = motor.GetComponent<CharacterController>();
            float feet = motor.transform.position.y + body.center.y - body.height * .5f;
            Assert.That(stance.TrySetStance(CharacterStance.Prone), Is.True);
            Assert.That(body.height, Is.EqualTo(1.1f).Within(.001f));
            Assert.That(motor.transform.position.y + body.center.y - body.height * .5f, Is.EqualTo(feet).Within(.001f));
            Vector3 start = motor.transform.position; motor.Tick(Vector2.up, true, true, .25f);
            Assert.That(motor.transform.position.z - start.z, Is.EqualTo(stance.MoveSpeed * .25f).Within(.025f));
            Assert.That(motor.transform.position.y, Is.LessThanOrEqualTo(start.y + .01f));
            Assert.That(motor.IsSprinting, Is.False);
            Assert.That(stance.TrySetCrouched(false), Is.True); Assert.That(body.height, Is.EqualTo(4f));
        }

        [Test]
        public void ProneChecksTheWholeBodyForEntryHeadingAndMovement()
        {
            var motor = Motor(); var stance = motor.GetComponent<PlayerCharacterStance>();
            var obstacle = Box(_origin + new Vector3(0f, .6f, 1.3f), new Vector3(3f, 1f, .2f));
            Assert.That(stance.TrySetStance(CharacterStance.Prone), Is.False, "Feet/head volume must fit, not just the upright capsule.");
            obstacle.transform.position = _origin + new Vector3(1f, .6f, 0f); obstacle.GetComponent<BoxCollider>().size = new Vector3(.2f, 1f, 5f); Physics.SyncTransforms();
            Assert.That(stance.TrySetStance(CharacterStance.Prone), Is.True);
            Assert.That(stance.AllowsHeading(Quaternion.Euler(0f, 90f, 0f)), Is.False);
            Object.DestroyImmediate(obstacle);
            Box(_origin + new Vector3(0f, .6f, 3f), new Vector3(4f, 1f, .2f));
            for (int i = 0; i < 100; i++) motor.Tick(Vector2.up, false, false, .02f);
            Assert.That(motor.transform.position.z - _origin.z, Is.LessThan(1.1f), "The prone head must stop at the wall before the upright capsule reaches it.");
        }

        [Test]
        public void LowCeilingBlocksBothCrouchingAndStandingFromProne()
        {
            var motor = Motor(); var stance = motor.GetComponent<PlayerCharacterStance>();
            Assert.That(stance.TrySetStance(CharacterStance.Prone), Is.True);
            var ceiling = Box(_origin + Vector3.up * 1.6f, new Vector3(5f, .2f, 5f));
            Assert.That(stance.TrySetCrouched(true), Is.False); Assert.That(stance.TrySetCrouched(false), Is.False);
            ceiling.GetComponent<Collider>().isTrigger = true;
            Assert.That(stance.TrySetCrouched(false), Is.True);
        }

        [Test]
        public void SprintRequiresForwardMovement()
        {
            var motor = Motor(); motor.Tick(Vector2.left, true, false, .1f); Assert.That(motor.IsSprinting, Is.False);
            motor.Tick(Vector2.down, true, false, .1f); Assert.That(motor.IsSprinting, Is.False);
            motor.Tick(Vector2.up, true, false, .1f); Assert.That(motor.IsSprinting, Is.True);
        }

        [Test]
        public void SprintSlideInputStartsASlideAndEndsStandingWithoutRepeatedRestart()
        {
            var motor = Motor(); var traversal = Traversal(motor); var input = new CrouchInput(); Set(traversal, "_input", input);
            for (int i = 0; i < 15; i++) motor.Tick(Vector2.up, true, false, .02f);
            Assert.That(motor.IsSprinting, Is.True);
            Assert.That(motor.GetComponent<PlayerCharacterStance>().IsCrouched, Is.False); input.SlidePressed = true;
            motor.Tick(Vector2.up, true, false, .02f); Assert.That(traversal.IsSliding, Is.True);
            Vector3 started = motor.transform.position; input.SlidePressed = false;
            for (int i = 0; i < 45; i++) motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(traversal.IsSliding, Is.False); Assert.That(motor.GetComponent<PlayerCharacterStance>().IsCrouched, Is.False);
            Assert.That(motor.transform.position.z - started.z, Is.GreaterThan(3f));
            Assert.That(motor.IsSprinting, Is.False);
        }

        [Test]
        public void SlideRecoveryRetainsCrouchWhenTheCeilingBlocksStanding()
        {
            var motor = Motor(); var traversal = Traversal(motor);
            var input = new CrouchInput(); Set(traversal, "_input", input);
            for (int n = 0; n < 15; n++) motor.Tick(Vector2.up, true, false, .02f);
            input.SlidePressed = true; motor.Tick(Vector2.up, true, false, .02f);
            Assert.That(traversal.IsSliding, Is.True); input.SlidePressed = false;
            Box(motor.transform.position + Vector3.up * 1.1f, new Vector3(100f, .2f, 100f));
            for (int n = 0; n < 45; n++) motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(traversal.IsSliding, Is.False);
            Assert.That(motor.GetComponent<PlayerCharacterStance>().IsCrouched, Is.True);
        }

        [Test]
        public void JumpVaultsALowObstacleAndStopsWhenDisabled()
        {
            var motor = Motor(); var traversal = Traversal(motor);
            Box(_origin + new Vector3(0f, .5f, 1.75f), new Vector3(4f, 1f, 1.5f));
            Assert.That(traversal.TryBeginVault(), Is.True);
            for (int i = 0; i < 40; i++) motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(traversal.IsVaulting, Is.False);
            Assert.That(motor.transform.position.z - _origin.z, Is.GreaterThan(2.5f));
            Assert.That(motor.transform.position.y - _origin.y, Is.EqualTo(2.05f).Within(.1f));
            traversal.enabled = false; Assert.That(traversal.IsVaulting || traversal.IsSliding, Is.False);
        }

        [Test]
        public void VaultMovementStopsAtAnOverheadCollisionWithoutPassingThroughIt()
        {
            var motor = Motor(); var traversal = Traversal(motor);
            Box(_origin + new Vector3(0f, .5f, 1.75f), new Vector3(4f, 1f, 1.5f));
            Box(_origin + new Vector3(0f, 4.4f, .7f), new Vector3(4f, .2f, 3f));
            Assert.That(traversal.TryBeginVault(), Is.True);
            for (int i = 0; i < 15; i++) motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(traversal.IsVaulting, Is.False);
            Assert.That(motor.transform.position.y - _origin.y, Is.LessThan(2.4f));
            Assert.That(motor.transform.position.z - _origin.z, Is.LessThan(.2f));
        }

        [Test]
        public void AHighWallCannotBeVaultedAndDeathStopsMovement()
        {
            var motor = Motor(); var traversal = Traversal(motor);
            Box(_origin + new Vector3(0f, 3f, 1.75f), new Vector3(4f, 6f, 1.5f));
            Assert.That(traversal.TryBeginVault(), Is.False);
            var health = motor.gameObject.AddComponent<Health>(); var policy = motor.gameObject.AddComponent<PlayerCharacterActionPolicy>();
            Set(policy, "_health", health); Set(motor, "_actions", policy); health.TakeDamage(health.MaxHealth);
            Vector3 position = motor.transform.position; motor.Tick(Vector2.up, true, true, .1f);
            Assert.That(motor.transform.position.x, Is.EqualTo(position.x));
            Assert.That(motor.transform.position.z, Is.EqualTo(position.z)); Assert.That(policy.CanSprint, Is.False);
        }

        [UnityTest]
        public IEnumerator AimAndReloadBlockSprintAndLeanCannotPassThroughAWall()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            var policy = context.Actor.GetComponent<PlayerCharacterActionPolicy>() ?? context.Actor.gameObject.AddComponent<PlayerCharacterActionPolicy>();
            Set(policy, "_view", context.View); Set(policy, "_combat", context.Shooter);
            Assert.That(policy.CanSprint, Is.True);
            context.Input.AimHeld = context.Input.AimPressed = true; context.View.Tick(context.Input, .02f);
            Assert.That(policy.CanSprint, Is.False);
            context.Input.AimHeld = context.Input.AimPressed = false; context.Input.AimReleased = true;
            context.View.Tick(context.Input, .4f); context.Input.AimReleased = false;
            Assert.That(context.Shooter.Magazine.TryConsumeRound(), Is.True); Assert.That(context.Shooter.Magazine.TryReload(), Is.True);
            Assert.That(policy.CanSprint, Is.False); context.Shooter.Magazine.CancelReload();
            var lean = context.Actor.GetComponent<PlayerCharacterLean>() ?? context.Actor.gameObject.AddComponent<PlayerCharacterLean>();
            Set(lean, "_camera", context.Follow);
            Box(context.Follow.UnleanedEyePosition + Vector3.right * .35f, new Vector3(.1f, 3f, 3f));
            lean.Tick(1f, .2f); Assert.That(lean.Amount, Is.InRange(0f, .35f));
            lean.enabled = false; Assert.That(lean.Amount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator CoverMountRequiresAimAndReleasesOnMovement()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            var fire = new FireInput(); Set(context.Shooter, "_input", fire); context.Shooter.enabled = true;
            var mount = context.Actor.GetComponent<PlayerCharacterWeaponBracing>() ?? context.Actor.gameObject.AddComponent<PlayerCharacterWeaponBracing>();
            Set(mount, "_camera", context.Follow); Set(mount, "_view", context.View);
            Set(mount, "_movement", context.Actor.GetComponent<PlayerCharacterMovement>()); Set(mount, "_combat", context.Shooter);
            Vector3 eye = context.Follow.EyePosition;
            Box(eye + new Vector3(0f, -.6f, 1.3f), new Vector3(3f, .7f, 1f));
            mount.Tick(true); Assert.That(mount.IsMounted, Is.False);
            context.Input.AimHeld = context.Input.AimPressed = true; context.View.Tick(context.Input, .02f);
            mount.Tick(true); Assert.That(mount.IsMounted, Is.True); Assert.That(mount.RecoilMultiplier, Is.LessThan(1f));
            context.Actor.position += Vector3.right * .5f; mount.Tick(true); Assert.That(mount.IsMounted, Is.False);
        }

        [UnityTest]
        public IEnumerator LeanMovesTheTorsoWithoutAccumulatingRotationOrLosingGunAim()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Set(context.Shooter, "_input", new FireInput()); context.Shooter.enabled = true;
            context.Input.AimHeld = true; context.View.Tick(context.Input, .02f);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            context.Actor.GetComponentInChildren<PlayerCharacterAnimationView>().enabled = false;
            animator.SetBool("Grounded", true); animator.SetFloat("Speed", 0f); animator.Play("Base Layer.Locomotion", 0, 0f);
            yield return new WaitForSeconds(.4f); animator.speed = 0f;
            var chest = animator.GetBoneTransform(HumanBodyBones.UpperChest); Quaternion neutral = chest.rotation;
            var tactical = new TacticalInput { Lean = 1f }; Set(context.Actor.GetComponent<PlayerCharacterLean>(), "_input", tactical);
            yield return new WaitForSeconds(.3f); Quaternion leaned = chest.rotation;
            Assert.That(Quaternion.Angle(neutral, leaned), Is.InRange(3f, 20f));
            yield return new WaitForSeconds(1f);
            Assert.That(Quaternion.Angle(leaned, chest.rotation), Is.LessThan(2f), "The additive pose must not accumulate each frame.");
            context.Input.Look = new Vector2(0f, -20f); context.View.Tick(context.Input, .02f); context.Input.Look = Vector2.zero;
            yield return new WaitForSeconds(.4f);
            Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            tactical.Lean = 0f; context.Input.Look = new Vector2(0f, 20f); context.View.Tick(context.Input, .02f); context.Input.Look = Vector2.zero;
            yield return new WaitForSeconds(.4f); Assert.That(Quaternion.Angle(neutral, chest.rotation), Is.LessThan(3f));
        }

        [UnityTest]
        public IEnumerator SampleSceneProneKeepsTheCameraGunAndBodyConnected()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Set(context.Shooter, "_input", new FireInput()); context.Shooter.enabled = true;
            var motor = context.Actor.GetComponent<PlayerCharacterMovement>();
            Box(context.Actor.position + Vector3.down * 2.25f, new Vector3(100f, .5f, 100f));
            Set(motor, "_input", null); motor.enabled = true;
            yield return new WaitForSeconds(.5f);
            var animator = context.Actor.GetComponentInChildren<Animator>();
            var stance = context.Actor.GetComponent<PlayerCharacterStance>();
            Vector3 modelPosition = animator.transform.localPosition; Quaternion modelRotation = animator.transform.localRotation;
            Assert.That(stance.TrySetStance(CharacterStance.Prone), Is.True);
            var prone = animator.GetComponent<PlayerCharacterProneAnimationView>();
            float entry = stance.ProneEnterDuration + .12f;
            yield return new WaitForSeconds(entry);
            float feet = context.Actor.TransformPoint(context.Actor.GetComponent<CharacterController>().center).y - .55f;
            float headHeight = animator.GetBoneTransform(HumanBodyBones.Head).position.y - feet;
            Assert.That(headHeight, Is.InRange(.2f, 1.5f), "The body must actually lie near the floor, not stand in a short collider.");
            Assert.That(Vector3.Angle(context.Shooter.Muzzle.forward, context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            context.TogglePerspective(); yield return new WaitForSeconds(.4f);
            Assert.That(context.Camera.transform.position.y - feet, Is.EqualTo(.7f).Within(.1f));
            Assert.That(Vector3.Distance(context.Follow.GetAimRay().origin, context.Camera.transform.position), Is.LessThan(.02f));
            Assert.That(stance.TrySetCrouched(false), Is.True); yield return new WaitForSeconds(.5f);
            Assert.That(animator.transform.localPosition, Is.EqualTo(modelPosition));
            Assert.That(Quaternion.Angle(animator.transform.localRotation, modelRotation), Is.LessThan(.01f));
        }

        [Test]
        public void DraggedDownedBodiesStopAtWallsAndBlockedSpawnsDoNotRestoreHealth()
        {
            var motor = Motor(); var stance = motor.GetComponent<PlayerCharacterStance>();
            var health = motor.gameObject.AddComponent<Health>(); var life = motor.gameObject.AddComponent<InfantryLifeState>();
            Set(life, "_movement", motor); Set(life, "_stance", stance);
            var policy = motor.gameObject.AddComponent<PlayerCharacterActionPolicy>(); Set(policy, "_health", health); Set(motor, "_actions", policy);
            health.TakeDamage(health.MaxHealth); Assert.That(life.IsDowned, Is.True);
            Box(_origin + new Vector3(0f, .6f, 3f), new Vector3(5f, 1f, .2f));
            Assert.That(life.TryDrag(Vector3.forward * 5f), Is.True); motor.Tick(Vector2.zero, false, false, .02f);
            Assert.That(motor.transform.position.z - _origin.z, Is.LessThan(1.1f));
            life.GiveUp(); var blocker = Box(_origin + Vector3.up * 2f, new Vector3(4f, 4f, 4f));
            Assert.That(life.TryRespawn(), Is.False); Assert.That(health.IsDead, Is.True);
            blocker.GetComponent<Collider>().enabled = false; Assert.That(life.TryRespawn(), Is.True);
            Assert.That(stance.IsLowered, Is.False); Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
        }

        [UnityTest]
        public IEnumerator BoardingSuspendsNewActionsAndADeadPilotCanRespawn()
        {
            yield return BoardingTestContext.Load(); var context = new BoardingTestContext(); context.PrepareEntry();
            var life = context.Actor.GetComponent<InfantryLifeState>(); Assert.That(life, Is.Not.Null);
            Assert.That(context.Seat.TryEnter(context.Occupant), Is.True);
            foreach (var component in new Behaviour[] { context.Actor.GetComponent<PlayerCharacterTraversal>(), context.Actor.GetComponent<PlayerCharacterLean>(),
                context.Actor.GetComponent<PlayerCharacterWeaponBracing>(), context.Actor.GetComponent<PlayerCharacterRecoil>(), context.Actor.GetComponent<PlayerCharacterRevive>() })
                Assert.That(component.enabled, Is.False);
            Assert.That(life.enabled, Is.True);
            var health = context.Actor.GetComponent<Health>(); health.TakeDamage(health.MaxHealth); yield return null;
            Assert.That(life.IsEliminated, Is.True); Assert.That(context.Occupant.HasInputControl, Is.False);
            Assert.That(life.TryRespawn(), Is.True); yield return null;
            Assert.That(context.Occupant.IsSeated, Is.False); Assert.That(health.IsDead, Is.False);
            Assert.That(context.Actor.GetComponent<PlayerCharacterTraversal>().enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator EditorTrainingAllyCanBeRevivedWithoutChangingThePlayerOrExistingDummies()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Set(context.Shooter, "_input", new FireInput()); context.Shooter.enabled = true;
            Box(context.Actor.position + Vector3.down * 2.25f, new Vector3(100f, .5f, 100f));
            context.TogglePerspective(); yield return null; yield return null;
            var originalMuzzle = context.Shooter.Muzzle;
            Assert.That(UnityEditor.EditorApplication.ExecuteMenuItem("Tools/Battlefield/Infantry/Spawn Downed Ally (Play Mode)"), Is.True);
            var ally = System.Linq.Enumerable.Single(Object.FindObjectsByType<InfantryLifeState>(), item => item.transform != context.Actor);
            _objects.Add(ally.gameObject); Assert.That(ally.IsDowned, Is.True);
            Assert.That(context.Shooter.Muzzle, Is.EqualTo(originalMuzzle));
            Assert.That(ally.GetComponentInChildren<SkinnedMeshRenderer>().shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.On));
            var revive = context.Actor.GetComponent<PlayerCharacterRevive>(); Assert.That(revive.TryBegin(ally), Is.True);
            Assert.That(context.Actor.GetComponent<PlayerCharacterActionPolicy>().CanJump, Is.False);
            Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().IsCrouched, Is.True);
            revive.Tick(true, false, 4f); Assert.That(ally.State, Is.EqualTo(InfantryLife.Alive));
            Assert.That(context.Actor.GetComponent<PlayerCharacterStance>().Current, Is.EqualTo(CharacterStance.Standing));
            yield return null; Assert.That(ally.GetComponent<Health>().IsDead, Is.False);
        }

        [UnityTest]
        public IEnumerator ReviveKneelingKeepsTheLowestFootNearTheFloor()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Set(context.Shooter, "_input", new FireInput()); context.Shooter.enabled = true;
            Box(context.Actor.position + Vector3.down * 2.25f, new Vector3(100f, .5f, 100f));
            Assert.That(UnityEditor.EditorApplication.ExecuteMenuItem("Tools/Battlefield/Infantry/Spawn Downed Ally (Play Mode)"), Is.True);
            var ally = System.Linq.Enumerable.Single(Object.FindObjectsByType<InfantryLifeState>(), item => item.transform != context.Actor);
            _objects.Add(ally.gameObject); var revive = context.Actor.GetComponent<PlayerCharacterRevive>();
            Set(revive, "_input", new HeldInteraction()); Assert.That(revive.TryBegin(ally), Is.True);
            yield return new WaitForSeconds(.8f); Assert.That(revive.IsReviving, Is.True);
            var animator = context.Actor.GetComponentInChildren<Animator>(); var body = context.Actor.GetComponent<CharacterController>();
            float floor = context.Actor.TransformPoint(body.center).y - body.height * .5f;
            float foot = Mathf.Min(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.y, animator.GetBoneTransform(HumanBodyBones.RightFoot).position.y);
            Assert.That(foot - floor, Is.InRange(-.05f, .25f), "The kneeling pose must not float or sink into the ground.");
            Set(revive, "_input", null); revive.Cancel();
        }

        [UnityTest]
        public IEnumerator CancellingRevivePreservesAPostureChosenDuringTheInteraction()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Set(context.Shooter, "_input", new FireInput()); context.Shooter.enabled = true;
            Box(context.Actor.position + Vector3.down * 2.25f, new Vector3(100f, .5f, 100f));
            Assert.That(UnityEditor.EditorApplication.ExecuteMenuItem("Tools/Battlefield/Infantry/Spawn Downed Ally (Play Mode)"), Is.True);
            var ally = System.Linq.Enumerable.Single(Object.FindObjectsByType<InfantryLifeState>(), item => item.transform != context.Actor);
            _objects.Add(ally.gameObject);
            var revive = context.Actor.GetComponent<PlayerCharacterRevive>(); var stance = context.Actor.GetComponent<PlayerCharacterStance>();
            Assert.That(revive.TryBegin(ally), Is.True); Assert.That(stance.IsCrouched, Is.True);
            Assert.That(stance.TrySetCrouched(false), Is.True); Assert.That(stance.TrySetCrouched(true), Is.True);
            revive.Cancel(); Assert.That(stance.IsCrouched, Is.True, "Do not override an explicit posture change with the earlier automatic stand restore.");
        }

        [UnityTest]
        public IEnumerator AlliedDummyBulletsFinishDownedEnemiesAndNeutralsButNotAllies()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Assert.That(context.Shooter.GetComponent<Battlefield.Features.Training.DummyShooter>(), Is.Not.Null);
            Assert.That(context.Shooter.GetComponent<Team>().CurrentTeam, Is.EqualTo(TeamType.Player));
            context.Actor.GetComponent<PlayerCharacterRecoil>().enabled = false;
            var input = context.EnableCombatInput();
            context.TogglePerspective(); yield return new WaitForSeconds(.3f);
            foreach (string name in new[] { "Dummy_Enemy", "Dummy_Neutral", "Ally_10m" })
            {
                var health = System.Linq.Enumerable.Single(Object.FindObjectsByType<Health>(), item => item.name == name);
                bool allied = name == "Ally_10m"; if (allied) Set(health.GetComponent<Team>(), "_team", TeamType.Player);
                var life = health.gameObject.AddComponent<InfantryLifeState>();
                health.transform.SetParent(null, true); health.transform.position = context.Actor.position + Vector3.forward * 20f;
                Physics.SyncTransforms(); health.TakeDamage(health.MaxHealth); Assert.That(life.IsDowned, Is.True);
                for (int shot = 0; shot < 4; shot++)
                {
                    input.PressFire(); yield return null; input.FireWeapon = false; yield return new WaitForSeconds(.15f);
                }
                Assert.That(life.State, Is.EqualTo(allied ? InfantryLife.Downed : InfantryLife.Eliminated), name);
                health.transform.position += Vector3.right * 100f; Physics.SyncTransforms();
            }
        }

        [UnityTest]
        public IEnumerator AlliedDummyActualShotsApplyRecoverableRecoilAndRetainTeamCollisionRules()
        {
            yield return CharacterViewTestContext.Load(); var context = new CharacterViewTestContext();
            Assert.That(context.Shooter.GetComponent<Battlefield.Features.Training.DummyShooter>(), Is.Not.Null);
            Assert.That(context.Shooter.GetComponent<Team>().CurrentTeam, Is.EqualTo(TeamType.Player));
            var recoil = context.Actor.GetComponent<PlayerCharacterRecoil>() ?? context.Actor.gameObject.AddComponent<PlayerCharacterRecoil>();
            recoil.enabled = false; Set(recoil, "_combat", context.Shooter); Set(recoil, "_view", context.View); recoil.enabled = true;
            Set(context.View, "_recoil", recoil); var input = context.EnableCombatInput();
            context.TogglePerspective(); yield return new WaitForSeconds(.3f);
            foreach (string name in new[] { "Dummy_Enemy", "Dummy_Neutral", "Ally_10m" })
            {
                var target = System.Linq.Enumerable.Single(Object.FindObjectsByType<Health>(), item => item.name == name);
                bool allied = name == "Ally_10m"; if (allied) Set(target.GetComponent<Team>(), "_team", TeamType.Player);
                recoil.Tick(10f); target.transform.SetParent(null, true); target.transform.position = context.Actor.position + Vector3.forward * 20f;
                Physics.SyncTransforms(); float before = target.CurrentHealth; Quaternion aim = context.View.AimRotation;
                input.PressFire(); yield return null; input.FireWeapon = false;
                Assert.That(recoil.Offset.x, Is.GreaterThan(0f)); Assert.That(Quaternion.Angle(aim, context.View.AimRotation), Is.GreaterThan(.1f));
                yield return new WaitForSeconds(.25f);
                Assert.That(target.CurrentHealth, Is.EqualTo(before - (allied ? 0f : 10f)), name);
                target.transform.position += Vector3.right * 100f;
            }
            yield return new WaitForSeconds(1f); Assert.That(recoil.Offset.magnitude, Is.LessThan(.001f));
            recoil.enabled = false; Assert.That(recoil.Offset, Is.EqualTo(Vector2.zero));
        }
    }
}
#endif
