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
    public sealed class PlayerCharacterWeaponPoseStateTests
    {
        [Test] public void CarrySprintReadyAndFirstPersonHaveDistinctPoses()
        {
            var pose = new PlayerCharacterWeaponPoseState();
            for (int i = 0; i < 60; i++) pose.Tick(CharacterWeaponPoseMode.Carry, 1f / 60f);
            Assert.That(Mathf.DeltaAngle(0f, pose.Rotation(Quaternion.identity).eulerAngles.x), Is.EqualTo(0f).Within(.1f));
            for (int i = 0; i < 60; i++) pose.Tick(CharacterWeaponPoseMode.Sprint, 1f / 60f);
            Assert.That(pose.SupportHandWeight, Is.EqualTo(1f));
            Assert.That(pose.UpperLayerWeight, Is.EqualTo(1f));
            Assert.That(Mathf.DeltaAngle(0f, pose.Rotation(Quaternion.identity).eulerAngles.x), Is.EqualTo(0f).Within(.1f));
            pose.Tick(CharacterWeaponPoseMode.FirstPerson, .02f);
            var aim = Quaternion.Euler(-60f, 70f, 0f);
            Assert.That(Quaternion.Angle(pose.Rotation(aim), aim), Is.LessThan(.001f));
            Assert.That(pose.SupportHandWeight, Is.EqualTo(1f));
        }

        [Test] public void ShotReadinessExpiresAndHiddenResetsTheTransition()
        {
            var pose = new PlayerCharacterWeaponPoseState();
            for (int i = 0; i < 60; i++) pose.Tick(CharacterWeaponPoseMode.Carry, .02f);
            pose.RequireReady();
            for (int i = 0; i < 10; i++) pose.Tick(CharacterWeaponPoseMode.Carry, .02f);
            Assert.That(pose.CarryBlend, Is.EqualTo(0f));
            for (int i = 0; i < 40; i++) pose.Tick(CharacterWeaponPoseMode.Carry, .02f);
            Assert.That(pose.CarryBlend, Is.GreaterThan(.99f));
            pose.Tick(CharacterWeaponPoseMode.Hidden, .02f);
            Assert.That(pose.CarryBlend, Is.EqualTo(0f));
            Assert.That(pose.SprintBlend, Is.EqualTo(0f));
        }

        [Test] public void PoseBlendsAreIndependentOfFrameRate()
        {
            float Blend(int fps)
            {
                var pose = new PlayerCharacterWeaponPoseState();
                for (int i = 0; i < fps / 2; i++) pose.Tick(CharacterWeaponPoseMode.Carry, 1f / fps);
                return pose.CarryBlend;
            }
            Assert.That(Blend(30), Is.EqualTo(Blend(60)).Within(.0001f));
            Assert.That(Blend(144), Is.EqualTo(Blend(60)).Within(.0001f));
        }
    }

    public sealed class PlayerCharacterWeaponPoseTests
    {
        private sealed class InputState : IWeaponInput, IPlayerCharacterCombatInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection { get; set; }
            public bool ReloadPressed => false;
            public bool FirePressed => false;
            public bool ToggleFireModePressed => false;
            public bool MeleePressed => false;
        }
        private CharacterViewTestContext _context;
        private PlayerCharacterMovement _motor;
        private PlayerCharacterCombatView _presentation;
        private Animator _animator;
        private GameObject _floor;
        private InputState _input;

        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return CharacterViewTestContext.Load();
            _context = new CharacterViewTestContext();
            _input = new InputState();
            CharacterViewTestContext.SetField(_context.Shooter, "_input", _input);
            CharacterViewTestContext.SetField(_context.Shooter, "_combatInput", _input);
            _context.Shooter.enabled = true;
            _motor = _context.Actor.GetComponent<PlayerCharacterMovement>();
            _animator = _context.Actor.GetComponentInChildren<Animator>();
            _presentation = _animator.GetComponent<PlayerCharacterCombatView>();
            CharacterViewTestContext.SetField(_motor, "_input", null);
            _motor.enabled = true;
            _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _floor.transform.position = _context.Actor.position + Vector3.down * 2.25f;
            _floor.transform.localScale = new Vector3(500f, .5f, 500f);
            Physics.SyncTransforms();
            yield return Advance(Vector2.zero, false, 15);
            yield return new WaitForSeconds(.5f);
        }
        [UnityTearDown] public IEnumerator TearDown() { Object.Destroy(_floor); yield return null; }

        [UnityTest] public IEnumerator ThirdPersonCarryLowersTheGunWithoutReplacingTheAimDirection()
        {
            Assert.That(Vector3.Angle(_context.Shooter.Muzzle.forward, _context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            float carriedHeight = _context.Shooter.Muzzle.position.y;
            _context.Input.AimHeld = true;
            yield return new WaitForSeconds(.4f);
            Assert.That(Vector3.Angle(_context.Shooter.Muzzle.forward, _context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            Assert.That(_context.Shooter.Muzzle.position.y, Is.EqualTo(carriedHeight + .1f).Within(.04f));
            _context.Input.AimHeld = false;
            yield return new WaitForSeconds(.4f);
            Assert.That(Vector3.Angle(_context.Shooter.Muzzle.forward, _context.View.AimRotation * Vector3.forward), Is.LessThan(2f));
            _context.TogglePerspective();
            yield return new WaitForSeconds(.3f);
            Assert.That(_context.View.IsFirstPersonView, Is.True);
            Assert.That(_presentation.WeaponForwardScale, Is.EqualTo(1f));
        }

        [UnityTest] public IEnumerator SprintKeepsTheGripAndRelaxedUnarmedMovementUsesTheBaseGait()
        {
            yield return Advance(Vector2.up, true, 30);
            yield return new WaitForSeconds(.5f);
            Assert.That(_presentation.SupportHandWeight, Is.EqualTo(1f));
            _input.WeaponSlotSelection = PlayerCharacterLoadout.UnarmedSlot;
            yield return null;
            _input.WeaponSlotSelection = 0;
            yield return Advance(Vector2.up, false, 30);
            yield return new WaitForSeconds(.25f);
            Assert.That(_animator.GetLayerWeight(_animator.GetLayerIndex("MeleeCombat")), Is.LessThan(.01f));
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < 90; i++)
            {
                yield return Advance(Vector2.up, false, 1);
                float z = _context.Actor.InverseTransformPoint(_animator.GetBoneTransform(HumanBodyBones.LeftHand).position).z;
                min = Mathf.Min(min, z); max = Mathf.Max(max, z);
            }
            Assert.That(max - min, Is.GreaterThan(.3f), "Relaxed running must retain authored arm swing.");
            _context.Input.AimHeld = true;
            yield return new WaitForSeconds(.3f);
            Assert.That(_animator.GetLayerWeight(_animator.GetLayerIndex("MeleeCombat")), Is.GreaterThan(.98f));
        }

        [UnityTest] public IEnumerator MousePitchTracksTheWeaponAndTorsoWithoutHoldingAim()
        {
            Assert.That(_context.Input.AimHeld, Is.False);
            Transform chest = _animator.GetBoneTransform(HumanBodyBones.UpperChest);
            foreach (bool firstPerson in new[] { false, true })
            {
                if (_context.View.IsFirstPersonView != firstPerson) _context.TogglePerspective();
                foreach (bool sprint in new[] { false, true })
                {
                    yield return Advance(Vector2.up, sprint, 20);
                    float initial = Mathf.DeltaAngle(0f, _context.View.AimRotation.eulerAngles.x);
                    _context.Input.Look = new Vector2(0f, initial / 3f);
                    _context.View.Tick(_context.Input, .02f); _context.Input.Look = Vector2.zero;
                    yield return new WaitForSeconds(.25f);
                    Quaternion neutral = chest.rotation;
                    foreach (float pitch in new[] { -60f, 60f })
                    {
                        float current = Mathf.DeltaAngle(0f, _context.View.AimRotation.eulerAngles.x);
                        _context.Input.Look = new Vector2(0f, (current - pitch) / 3f);
                        _context.View.Tick(_context.Input, .02f); _context.Input.Look = Vector2.zero;
                        yield return Advance(Vector2.up, sprint, 15);
                        Quaternion expected = _context.View.AimRotation;
                        if (firstPerson) expected *= Quaternion.Euler(_context.Follow.FirstPersonWeaponEuler);
                        Assert.That(Vector3.Angle(_context.Shooter.Muzzle.forward,
                            expected * Vector3.forward), Is.LessThan(2f),
                            $"Pitch must follow without RMB: firstPerson={firstPerson}, sprint={sprint}, pitch={pitch}");
                        Assert.That(Quaternion.Angle(neutral, chest.rotation), Is.GreaterThan(8f),
                            "The torso must participate in vertical aiming.");
                    }
                }
            }
        }

        [UnityTest] public IEnumerator DisablingAndReenablingPresentationResetsTheCarriedPose()
        {
            Assert.That(_presentation.WeaponForwardScale, Is.LessThan(.85f));
            _presentation.enabled = false;
            Assert.That(_presentation.WeaponForwardScale, Is.EqualTo(1f));
            Assert.That(_presentation.Pistol.gameObject.activeSelf, Is.False);
            _presentation.enabled = true;
            yield return new WaitForSeconds(.5f);
            Assert.That(_presentation.WeaponForwardScale, Is.LessThan(.85f));
            Assert.That(_presentation.Pistol.gameObject.activeSelf, Is.True);
        }

        private IEnumerator Advance(Vector2 move, bool sprint, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _motor.Tick(move, sprint, false, .02f);
                yield return null;
                _animator.Update(.02f);
            }
        }
    }
}
#endif
