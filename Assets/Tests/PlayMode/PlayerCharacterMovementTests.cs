#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterMovementTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly Vector3 _origin = new(10000f, 10000f, 10000f);

        [UnityTearDown]
        public IEnumerator CleanUp()
        {
            foreach (GameObject item in _objects) if (item != null) Object.Destroy(item);
            _objects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator WalkingAndSprintingUseTheirOwnSpeedsWithoutDiagonalBoost()
        {
            PlayerCharacterMovement motor = CreateMotor();
            yield return Advance(motor, Vector2.zero, false, false, 0.2f);
            yield return Advance(motor, Vector2.one, false, false, 0.2f);
            Vector3 start = motor.transform.position;
            yield return Advance(motor, Vector2.one, false, false, 0.5f);
            Vector3 walk = motor.transform.position - start;
            Assert.That(new Vector2(walk.x, walk.z).magnitude, Is.EqualTo(4f).Within(0.15f));
            yield return Advance(motor, Vector2.up, true, false, 0.2f);
            start = motor.transform.position;
            yield return Advance(motor, Vector2.up, true, false, 0.5f);
            Assert.That(motor.transform.position.z - start.z, Is.EqualTo(6f).Within(0.15f));
            Assert.That(motor.IsSprinting, Is.True);
        }

        [UnityTest]
        public IEnumerator JumpRisesThenGravityReturnsTheCharacterToTheGround()
        {
            PlayerCharacterMovement motor = CreateMotor();
            yield return Advance(motor, Vector2.zero, false, false, 0.2f);
            Assert.That(motor.IsGrounded, Is.True);
            float startY = motor.transform.position.y;
            float peak = startY;
            float elapsed = 0f;
            bool first = true;
            while (elapsed < 1.5f)
            {
                float step = 0.02f;
                motor.Tick(Vector2.zero, false, first, step);
                first = false;
                peak = Mathf.Max(peak, motor.transform.position.y);
                elapsed += step;
                yield return null;
            }
            Assert.That(peak - startY, Is.EqualTo(1.2f).Within(0.15f));
            Assert.That(motor.IsGrounded, Is.True);
            Assert.That(motor.transform.position.y, Is.EqualTo(startY).Within(0.06f));
        }

        [UnityTest]
        public IEnumerator CapsuleStopsAtWallsAndClimbsAStepWithinItsStepOffset()
        {
            PlayerCharacterMovement motor = CreateMotor();
            CreateBox("Wall", _origin + new Vector3(0f, 4f, 3f), new Vector3(10f, 8f, 0.5f));
            yield return Advance(motor, Vector2.up, false, false, 0.7f);
            Assert.That(motor.transform.position.z - _origin.z, Is.LessThan(2.4f));

            CharacterController body = motor.GetComponent<CharacterController>();
            body.enabled = false;
            motor.transform.position = _origin + new Vector3(20f, 2.02f, 0f);
            body.enabled = true;
            motor.Stop();
            CreateBox("Low Step", _origin + new Vector3(20f, 0.1f, 3f), new Vector3(10f, 0.2f, 2f));
            yield return Advance(motor, Vector2.up, false, false, 0.6f);
            Assert.That(motor.transform.position.z - _origin.z, Is.GreaterThan(4f));
        }

        [UnityTest]
        public IEnumerator MigratedAlliedCharacterPreservesItsVisualAndFiringTeamRules()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/SampleScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            DummyShooter[] shooters = Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include);
            foreach (DummyShooter shooter in shooters) shooter.enabled = false;
            foreach (DummyMissileShooter shooter in Object.FindObjectsByType<DummyMissileShooter>()) shooter.enabled = false;
            PlayerCharacterController player = shooters.First(
                shooter => shooter.GetComponent<Team>().CurrentTeam == TeamType.Player)
                .GetComponent<PlayerCharacterController>();
            player.enabled = false;
            CharacterController body = player.GetComponent<CharacterController>();
            Assert.That(body.height, Is.EqualTo(4f));
            Assert.That(body.radius, Is.EqualTo(0.5f));
            Assert.That(player.GetComponent<BoxCollider>(), Is.Null);
            Assert.That(player.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(player.transform.Find("Visual").localScale, Is.EqualTo(new Vector3(2f, 4f, 1f)));
            Assert.That(player.GetComponent<IPlayerCharacterInput>(), Is.Not.Null);

            player.transform.SetParent(null, true);
            FrameInput input = new();
            SetField(player, "_input", input);
            CreateBox("Firing Floor", _origin + Vector3.down * 0.25f, new Vector3(1000f, 0.5f, 100f));
            player.enabled = true;
            string[] names = { "Dummy_Enemy", "Dummy_Neutral", "Ally_10m" };
            for (int i = 0; i < names.Length; i++)
            {
                Health target = Object.FindObjectsByType<Health>().Single(health => health.name == names[i]);
                if (i == 2) SetField(target.GetComponent<Team>(), "_team", TeamType.Player);
                Vector3 start = _origin + new Vector3(i * 100f, 2.02f, 0f);
                body.enabled = false;
                player.transform.SetPositionAndRotation(start, Quaternion.identity);
                body.enabled = true;
                target.transform.SetParent(null, true);
                target.transform.position = start + Vector3.forward * 20f;
                Physics.SyncTransforms();
                float previous = target.CurrentHealth;
                input.FireWeapon = true;
                yield return null;
                input.FireWeapon = false;
                yield return new WaitForSeconds(0.2f);
                Assert.That(target.CurrentHealth, Is.EqualTo(previous - (i == 2 ? 0f : 10f)), names[i]);
            }
        }

        [UnityTest]
        public IEnumerator ExistingAlliedDummyShooterCanFireFromTheCapsule()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/SampleScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            DummyShooter[] shooters = Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include);
            foreach (DummyShooter item in shooters) item.enabled = false;
            foreach (DummyMissileShooter item in Object.FindObjectsByType<DummyMissileShooter>()) item.enabled = false;
            DummyShooter shooter = shooters.First(
                item => item.GetComponent<Team>().CurrentTeam == TeamType.Player);
            shooter.GetComponent<PlayerCharacterController>().enabled = false;
            shooter.GetComponent<PlayerCharacterMovement>().enabled = false;
            shooter.transform.SetParent(null, true);
            CharacterController body = shooter.GetComponent<CharacterController>();
            body.enabled = false;
            shooter.transform.SetPositionAndRotation(_origin + Vector3.up * 2f, Quaternion.identity);
            body.enabled = true;
            Health target = Object.FindObjectsByType<Health>().Single(item => item.name == "Dummy_Enemy");
            target.transform.SetParent(null, true);
            target.transform.position = shooter.transform.position + Vector3.forward * 20f;
            SetField(shooter, "_target", target.transform);
            Physics.SyncTransforms();
            float previousHealth = target.CurrentHealth;
            typeof(DummyShooter).GetMethod("Fire", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(shooter, null);
            yield return new WaitForSeconds(0.2f);
            Assert.That(target.CurrentHealth, Is.EqualTo(previousHealth - 10f));
        }

        private PlayerCharacterMovement CreateMotor()
        {
            CreateBox("Floor", _origin + Vector3.down * 0.25f, new Vector3(100f, 0.5f, 100f));
            GameObject actor = new("Movement Test Character");
            _objects.Add(actor);
            actor.transform.position = _origin + Vector3.up * 2.02f;
            CharacterController body = actor.AddComponent<CharacterController>();
            body.height = 4f;
            body.radius = 0.5f;
            body.center = Vector3.zero;
            body.skinWidth = 0.05f;
            body.stepOffset = 0.3f;
            body.minMoveDistance = 0f;
            Physics.SyncTransforms();
            return actor.AddComponent<PlayerCharacterMovement>();
        }

        private void CreateBox(string name, Vector3 position, Vector3 size)
        {
            GameObject box = new(name);
            _objects.Add(box);
            box.transform.position = position;
            box.AddComponent<BoxCollider>().size = size;
            Physics.SyncTransforms();
        }

        private static IEnumerator Advance(PlayerCharacterMovement motor, Vector2 move, bool sprint, bool jump, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float step = Mathf.Min(0.02f, duration - elapsed);
                motor.Tick(move, sprint, jump, step);
                jump = false;
                elapsed += step;
                yield return null;
            }
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private sealed class FrameInput : IPlayerCharacterInput, IWeaponInput
        {
            public Vector2 Move => Vector2.zero;
            public bool SprintHeld => false;
            public bool JumpPressed => false;
            public bool FireWeapon { get; set; }
            public int WeaponSlotSelection => 0;
        }
    }
}
#endif
