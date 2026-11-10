#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterDirectionalAnimationTests
    {
        private class PassiveWeaponInput : Battlefield.Features.Weapon.IWeaponInput
        { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }
        private PlayerCharacterMovement _movement;
        private Animator _animator;
        private GameObject _floor;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Scenes/SampleScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
            foreach (var shooter in Object.FindObjectsByType<DummyShooter>()) shooter.enabled = false;
            foreach (var shooter in Object.FindObjectsByType<DummyMissileShooter>()) shooter.enabled = false;
            _movement = Object.FindObjectsByType<PlayerCharacterMovement>().Single();
            _animator = _movement.GetComponentInChildren<Animator>();
            Set(_movement.GetComponent<PlayerCharacterController>(), "_input", new PassiveWeaponInput());
            _movement.GetComponent<PlayerCharacterView>().enabled = false;
            Set(_movement, "_input", null);
            Set(_animator.GetComponent<PlayerCharacterAnimationView>(), "_speedDamping", 0f);
            CharacterController body = _movement.GetComponent<CharacterController>();
            body.enabled = false;
            _movement.transform.SetPositionAndRotation(new Vector3(10000f, 10002.02f, 10000f), Quaternion.Euler(0f, 37f, 0f));
            body.enabled = true;
            _floor = new GameObject("Directional Animation Test Floor");
            _floor.transform.position = new Vector3(10000f, 9999.75f, 10000f);
            _floor.AddComponent<BoxCollider>().size = new Vector3(200f, .5f, 200f);
            Physics.SyncTransforms();
            yield return Advance(Vector2.zero, false, 15);
        }

        [UnityTearDown]
        public IEnumerator TearDown() { Object.Destroy(_floor); yield return null; }

        [UnityTest]
        public IEnumerator LocalVelocitySelectsCardinalAndDiagonalGaitsWithoutRotatingTheModel()
        {
            var rotation = _animator.transform.localRotation;
            var position = _animator.transform.localPosition;
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new(-1f,1f), new(1f,1f), new(-1f,-1f), new(1f,-1f) };
            foreach (Vector2 raw in directions)
            {
                Vector2 direction = raw.normalized;
                yield return Advance(direction, false, 20);
                Assert.That(_animator.GetFloat("MoveX"), Is.EqualTo(direction.x).Within(.03f), raw.ToString());
                Assert.That(_animator.GetFloat("MoveY"), Is.EqualTo(direction.y).Within(.03f), raw.ToString());
                var clips = _animator.GetCurrentAnimatorClipInfo(0);
                void Has(string name, float minimum) => Assert.That(clips.Any(c => c.clip.name == name && c.weight > minimum), Is.True,
                    raw + ": " + string.Join(", ", clips.Select(c => c.clip.name + "=" + c.weight)));
                if (UsesMixamo)
                {
                    if (direction.y > 0) Has("rifle run", weightFor(direction));
                    if (direction.y < 0) Has("run backwards", weightFor(direction));
                    if (direction.x < 0) Has("strafe (2)", weightFor(direction));
                    if (direction.x > 0) Has("strafe", weightFor(direction));
                }
                else
                {
                    float weight = weightFor(direction);
                    if (direction.x < 0) Has("StrafeLeft", weight);
                    if (direction.x > 0) Has("StrafeRight", weight);
                    if (direction.y < 0) Has("WalkBackward", weight);
                    if (direction.y > 0) Has("Jog_Fwd_Loop", weight);
                }
                Assert.That(Quaternion.Angle(_animator.transform.localRotation, rotation), Is.LessThan(.01f));
                Assert.That(Vector3.Distance(_animator.transform.localPosition, position), Is.LessThan(.001f));
                // A directional clip must not turn the whole character to face its travel direction.
                Vector3 lateral = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position - _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position;
                Vector3 up = _animator.GetBoneTransform(HumanBodyBones.Spine).position - _animator.GetBoneTransform(HumanBodyBones.Hips).position;
                float facing = Vector3.Dot(Vector3.Cross(lateral, up).normalized, _movement.transform.forward);
                Assert.That(facing, Is.GreaterThan(.35f), raw + " facing=" + facing);
            }
        }

        [UnityTest]
        public IEnumerator StoppingFadesToIdleWithoutSwitchingTheLastDirection()
        {
            yield return Advance(Vector2.left, false, 20);
            float movingSpeed = _animator.GetFloat("Speed");
            Set(_animator.GetComponent<PlayerCharacterAnimationView>(), "_speedDamping", .1f);
            yield return Advance(Vector2.zero, false, 2);
            Assert.That(_animator.GetFloat("Speed"), Is.GreaterThan(0f).And.LessThan(movingSpeed));
            Assert.That(_animator.GetFloat("MoveX"), Is.EqualTo(-1f).Within(.03f));
            Assert.That(_animator.GetFloat("MoveY"), Is.EqualTo(0f).Within(.03f));
            yield return Advance(Vector2.zero, false, 8);
            yield return new WaitForSeconds(1f);
            _animator.Update(.02f);
            Assert.That(_animator.GetFloat("Speed"), Is.LessThan(.01f));
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c => c.clip.name == Expected("LocomotionIdle") && c.weight > .98f), Is.True);
        }

        [UnityTest]
        public IEnumerator SprintPreservesForwardRunAndBlendsTheSideGaitOnDiagonals()
        {
            yield return Advance(Vector2.up, true, 20);
            Assert.That(_animator.GetFloat("Speed"), Is.EqualTo(2f).Within(.03f));
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c => c.clip.name == Expected("LocomotionSprint") && c.weight > .9f), Is.True);
            yield return Advance(new Vector2(1f, 1f).normalized, true, 20);
            var clips = _animator.GetCurrentAnimatorClipInfo(0);
            Assert.That(clips.Any(c => c.clip.name == Expected("LocomotionSprint") && c.weight > .2f), Is.True);
            Assert.That(clips.Any(c => c.clip.name == Expected("StrafeRight") && c.weight > .2f), Is.True);
            Assert.That(clips.Any(c => c.clip.name == Expected("LocomotionWalk") && c.weight > .01f), Is.False);
            if (!UsesMixamo) Assert.That(clips.Any(c => c.clip.name == "Jog_Fwd_Loop" && c.weight > .01f), Is.False);
        }

        [UnityTest]
        public IEnumerator ResolvedSpeedSelectsWalkJogAndSprintAndReturnsThroughJog()
        {
            // Partial analog input and collision-limited speed can use the slower gait
            // without adding a new input binding or changing the movement speeds.
            yield return Advance(Vector2.up * .5f, false, 20);
            Assert.That(_animator.GetFloat("Speed"), Is.EqualTo(.5f).Within(.03f));
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c =>
                c.clip.name == Expected("LocomotionWalk") && c.weight > .9f), Is.True);

            yield return Advance(Vector2.up, false, 20);
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c =>
                c.clip.name == Expected("Jog_Fwd_Loop") && c.weight > .9f), Is.True);
            yield return Advance(Vector2.up, true, 20);
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c =>
                c.clip.name == Expected("LocomotionSprint") && c.weight > .9f), Is.True);

            yield return Advance(Vector2.up, false, 20);
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c =>
                c.clip.name == Expected("Jog_Fwd_Loop") && c.weight > .9f), Is.True);
            yield return Advance(Vector2.zero, false, 20);
            Assert.That(_animator.GetCurrentAnimatorClipInfo(0).Any(c =>
                c.clip.name == Expected("LocomotionIdle") && c.weight > .98f), Is.True);
        }

        private bool UsesMixamo => _movement.GetComponent<Battlefield.Features.PlayerCharacter.PlayerCharacterController>().IsArmed &&
            AssetDatabase.LoadAssetAtPath<Battlefield.Features.PlayerCharacter.ArmedLocomotionAnimationSet>("Assets/ThirdParty/MixamoLocal/ArmedLocomotion/ArmedLocomotion.asset") != null;
        private string Expected(string legacy) => !UsesMixamo ? legacy : legacy switch
        {
            "LocomotionIdle" => "rifle aiming idle", "LocomotionWalk" => "walking", "StrafeRight" => "strafe", _ => "rifle run"
        };
        private static float weightFor(Vector2 direction) => Mathf.Abs(direction.x)>0 && Mathf.Abs(direction.y)>0 ? .2f : .9f;

        private IEnumerator Advance(Vector2 move, bool sprint, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                _movement.Tick(move, sprint, false, .02f);
                yield return null;
                _animator.Update(.02f);
            }
        }
        private static void Set(object target, string name, object value) => target.GetType()
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    }
}
#endif
