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
    public sealed class PlayerCharacterTraversalAnimationTests
    {
        private sealed class CrouchInput : IPlayerCharacterTraversalInput { public bool SlidePressed { get; set; } public bool PronePressed => false; }
        private sealed class PassiveWeaponInput : Battlefield.Features.Weapon.IWeaponInput
        { public bool FireWeapon => false; public int WeaponSlotSelection => 0; }
        private readonly List<GameObject> _objects = new();
        private CharacterViewTestContext _context;
        private PlayerCharacterMovement _motor;
        private PlayerCharacterTraversal _traversal;
        private PlayerCharacterStance _stance;
        private Animator _animator;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CharacterViewTestContext.Load();
            _context = new CharacterViewTestContext();
            _motor = _context.Actor.GetComponent<PlayerCharacterMovement>();
            CharacterViewTestContext.SetField(_motor, "_input", null);
            _motor.enabled = true;
            _traversal = _context.Actor.GetComponent<PlayerCharacterTraversal>();
            _stance = _context.Actor.GetComponent<PlayerCharacterStance>();
            _animator = _context.Actor.GetComponentInChildren<Animator>();
            Box(_context.Actor.position + Vector3.down * 2.25f, new Vector3(100, .5f, 100));
            yield return Step(Vector2.zero, false, 15);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var item in _objects) Object.Destroy(item);
            _objects.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator SlidePlaysTheConfiguredTimelineAndEndsStandingWithoutRootMotion()
        {
            yield return Step(Vector2.up, true, 20);
            var input = new CrouchInput { SlidePressed = true };
            CharacterViewTestContext.SetField(_traversal, "_input", input);
            Assert.That(_stance.TrySetCrouched(true), Is.True);
            Vector3 modelPosition = _animator.transform.localPosition;
            Quaternion modelRotation = _animator.transform.localRotation;
            var seen = new HashSet<int>();
            for (int frame = 0; frame < 42; frame++)
            {
                _motor.Tick(Vector2.zero, false, false, .02f);
                input.SlidePressed = false;
                yield return null;
                _animator.Update(.02f);
                int mode = _animator.GetInteger("TraversalMode");
                seen.Add(mode);
                Assert.That(_animator.GetFloat("TraversalTime"), Is.InRange(0f, 1f));
                if (mode > 0)
                {
                    string state = mode == 1 ? "SlideStart" : mode == 2 ? "SlideLoop" : "SlideExit";
                    Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName(state) || _animator.GetNextAnimatorStateInfo(0).IsName(state), Is.True, state);
                }
            }
            var local = _animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations;
            if (local != null && local.LowerBodyOnly)
            {
                Assert.That(seen, Has.No.Member(1)); Assert.That(seen, Has.No.Member(2)); Assert.That(seen, Has.No.Member(3));
            }
            else if (local != null && local.IsContinuous)
            {
                Assert.That(seen, Does.Contain(2));
                Assert.That(seen, Has.No.Member(1)); Assert.That(seen, Has.No.Member(3));
            }
            else CollectionAssert.IsSubsetOf(new[] { 1, 2, 3 }, seen);
            Assert.That(_traversal.IsSliding, Is.False);
            Assert.That(_stance.IsCrouched, Is.False);
            Assert.That(_animator.GetInteger("TraversalMode"), Is.Zero);
            yield return Step(Vector2.zero, false, 10);
            Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
            Assert.That(_animator.applyRootMotion, Is.False);
            Assert.That(_animator.transform.localPosition, Is.EqualTo(modelPosition));
            Assert.That(Quaternion.Angle(_animator.transform.localRotation, modelRotation), Is.LessThan(.01f));
        }

        [UnityTest]
        public IEnumerator SlidingBodyStaysNearTheFloorAfterFullPoseAndWeaponEvaluation()
        {
            CharacterViewTestContext.SetField(_context.Shooter, "_input", new PassiveWeaponInput());
            _context.Shooter.enabled = true;
            yield return Step(Vector2.up, true, 20);
            var input = new CrouchInput { SlidePressed = true };
            CharacterViewTestContext.SetField(_traversal, "_input", input);
            Assert.That(_stance.TrySetCrouched(true), Is.True);
            var probe = _context.Actor.gameObject.AddComponent<SlideContactProbe>();
            probe.Initialize(_context.Renderer, _traversal, _context.Actor.GetComponent<CharacterController>());
            for (int frame = 0; frame < 44; frame++)
            {
                _motor.Tick(Vector2.zero, false, false, .02f);
                input.SlidePressed = false;
                yield return null;
            }
            yield return null;
            Assert.That(probe.Heights.Count, Is.GreaterThan(20));
            foreach (float height in probe.Heights)
                Assert.That(height, Is.InRange(-.2f, .4f), "The evaluated mesh must stay near the floor.");
            if (_animator.GetComponent<PlayerCharacterSlideAnimationView>()?.Animations?.LowerBodyOnly == true)
            {
                Assert.That(probe.FadeHeights.Count, Is.GreaterThan(0));
                foreach (float height in probe.FadeHeights) Assert.That(height, Is.InRange(-.15f, .15f), "The fading leg layer must keep ground contact.");
            }
        }

        [UnityTest]
        public IEnumerator VaultFollowsMotorProgressAndRestoresLocomotion()
        {
            Box(_context.Actor.position + new Vector3(0, -1.5f, 1.75f), new Vector3(4, 1, 1.5f));
            Assert.That(_traversal.TryBeginVault(), Is.True);
            bool sawVault = false;
            Vector3 start = _context.Actor.position;
            for (int frame = 0; frame < 48; frame++)
            {
                _motor.Tick(Vector2.zero, false, false, .02f);
                yield return null;
                _animator.Update(.02f);
                if (_animator.GetInteger("TraversalMode") == 4)
                {
                    sawVault = true;
                    Assert.That(_animator.GetFloat("TraversalTime"), Is.EqualTo(_traversal.Progress).Within(.001f));
                }
            }
            Assert.That(sawVault, Is.True);
            Assert.That(_traversal.IsVaulting, Is.False);
            Assert.That(_context.Actor.position.z - start.z, Is.GreaterThan(2.5f));
            Assert.That(_animator.GetInteger("TraversalMode"), Is.Zero);
            Assert.That(_animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
            Assert.That(_animator.applyRootMotion, Is.False);
        }

        [UnityTest]
        public IEnumerator DisableClearsTraversalParametersAndCancelledVaultDoesNotResume()
        {
            Box(_context.Actor.position + new Vector3(0, -1.5f, 1.75f), new Vector3(4, 1, 1.5f));
            Assert.That(_traversal.TryBeginVault(), Is.True);
            yield return Step(Vector2.zero, false, 4);
            Assert.That(_animator.GetInteger("TraversalMode"), Is.EqualTo(4));
            var view = _animator.GetComponent<PlayerCharacterAnimationView>();
            view.enabled = false;
            _traversal.Cancel();
            Assert.That(_animator.GetInteger("TraversalMode"), Is.Zero);
            Assert.That(_animator.GetFloat("TraversalTime"), Is.Zero);
            view.enabled = true;
            yield return Step(Vector2.zero, false, 20);
            Assert.That(_animator.GetInteger("TraversalMode"), Is.Zero);
        }

        private IEnumerator Step(Vector2 input, bool sprint, int frames)
        {
            for (int i = 0; i < frames; i++) { _motor.Tick(input, sprint, false, .02f); yield return null; _animator.Update(.02f); }
        }
        private void Box(Vector3 position, Vector3 size)
        {
            var item = new GameObject("Traversal animation test floor/obstacle");
            item.transform.position = position;
            item.AddComponent<BoxCollider>().size = size;
            _objects.Add(item);
            Physics.SyncTransforms();
        }
    }
    [DefaultExecutionOrder(200)]
    public sealed class SlideContactProbe : MonoBehaviour
    {
        public readonly List<float> Heights = new();
        public readonly List<float> FadeHeights = new();
        private Animator _animator;
        private SkinnedMeshRenderer _renderer;
        private PlayerCharacterTraversal _traversal;
        private CharacterController _body;
        private Mesh _mesh;
        public void Initialize(SkinnedMeshRenderer renderer, PlayerCharacterTraversal traversal, CharacterController body)
        { _renderer = renderer; _traversal = traversal; _body = body; _mesh = new Mesh(); _animator = traversal.GetComponentInChildren<Animator>(); }
        private void LateUpdate()
        {
            if (_traversal == null) return;
            int layer = _animator.GetLayerIndex("LowerBodySlide");
            int entry = _animator.GetLayerIndex("LowerBodySlideEntry");
            bool fading = !_traversal.IsSliding && ((layer >= 0 && _animator.GetLayerWeight(layer) > .001f) ||
                (entry >= 0 && _animator.GetLayerWeight(entry) > .001f));
            if (!_traversal.IsSliding && !fading) return;
            _renderer.BakeMesh(_mesh);
            float lowest = float.MaxValue;
            foreach (Vector3 v in _mesh.vertices) lowest = Mathf.Min(lowest, _renderer.transform.TransformPoint(v).y);
            float feet = transform.TransformPoint(_body.center).y - _body.height * .5f;
            Heights.Add(lowest - feet);
            if (fading) FadeHeights.Add(lowest - feet);
            Debug.Log("SLIDE_CONTACT height=" + (lowest - feet));
        }
        private void OnDestroy() { if (_mesh != null) Destroy(_mesh); }
    }
}
#endif
