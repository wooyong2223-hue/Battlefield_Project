#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public sealed class PlayerCharacterHandActionTests
    {
        private sealed class InputState : IWeaponInput, IPlayerCharacterCombatInput
        {
            public bool FireWeapon => false;
            public int WeaponSlotSelection { get; set; }
            public bool ReloadPressed => false;
            public bool FirePressed => false;
            public bool ToggleFireModePressed => false;
            public bool MeleePressed { get; set; }
        }

        private CharacterViewTestContext _context;
        private PlayerCharacterMovement _motor;
        private Animator _animator;
        private PlayerCharacterCombatView _presentation;
        private InputState _input;
        private GameObject _floor;
        private float _previousCaptureDelta;
        private Vector3 _supportOffset;

        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return CharacterViewTestContext.Load();
            _previousCaptureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = .02f;
            _context = new CharacterViewTestContext();
            _input = new InputState();
            CharacterViewTestContext.SetField(_context.Shooter, "_input", _input);
            CharacterViewTestContext.SetField(_context.Shooter, "_combatInput", _input);
            _context.Shooter.enabled = true;
            _motor = _context.Actor.GetComponent<PlayerCharacterMovement>();
            _motor.enabled = true;
            CharacterViewTestContext.SetField(_motor, "_input", null);
            _animator = _context.Actor.GetComponentInChildren<Animator>();
            _presentation = _animator.GetComponent<PlayerCharacterCombatView>();
            CharacterViewTestContext.SetField(_animator.GetComponent<PlayerCharacterAnimationView>(), "_speedDamping", 0f);
            _supportOffset = (Vector3)typeof(PlayerCharacterWeaponAimView)
                .GetField("_leftHandOffset", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(_animator.GetComponent<PlayerCharacterWeaponAimView>());
            _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _floor.transform.position = _context.Actor.position + Vector3.down * 2.25f;
            _floor.transform.localScale = new Vector3(1000f, .5f, 1000f);
            Physics.SyncTransforms();
            for (int frame = 0; frame < 20; frame++)
            {
                _motor.Tick(Vector2.zero, false, false, .02f);
                yield return null;
            }
        }

        [UnityTearDown] public IEnumerator TearDown()
        {
            Time.captureDeltaTime = _previousCaptureDelta;
            Object.Destroy(_floor);
            yield return null;
        }

        [UnityTest] public IEnumerator EquippedSprintAndJumpKeepTheGripPoseInBothViews()
        {
            Transform right = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform left = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            foreach (bool firstPerson in new[] { false, true })
            {
                if (_context.View.IsFirstPersonView != firstPerson) _context.TogglePerspective();
                bool airborne = false;
                for (int frame = 0; frame < 85; frame++)
                {
                    _motor.Tick(Vector2.up, true, frame == 30, .02f);
                    yield return null;
                    airborne |= !_motor.IsGrounded;
                    if (frame < 20) continue;
                    Assert.That(_presentation.UsesItemGrip, Is.True);
                    Assert.That(Vector3.Distance(left.position, right.TransformPoint(_supportOffset)), Is.LessThan(.04f),
                        $"Actual grip contact: firstPerson={firstPerson}, frame={frame}");
                    Assert.That(_presentation.SupportHandWeight, Is.EqualTo(1f).Within(.001f));
                    Assert.That(_animator.GetLayerWeight(_animator.GetLayerIndex("UpperCombat")),
                        Is.EqualTo(1f).Within(.001f));
                }
                Assert.That(airborne, Is.True, "The grip must be checked during an actual jump.");
            }
        }

        [UnityTest] public IEnumerator ActionsReleaseGripAndEquipmentRestoresIt()
        {
            foreach (bool firstPerson in new[] { false, true })
            {
                if (_context.View.IsFirstPersonView != firstPerson) _context.TogglePerspective();
                Assert.That(_context.Shooter.Magazine.TryConsumeRound(), Is.True);
                Assert.That(_context.Shooter.Magazine.TryReload(), Is.True);
                yield return null;
                Assert.That(_presentation.UsesItemGrip, Is.False, "Reload owns the hands.");
                yield return new WaitForSeconds(_context.Shooter.Magazine.ReloadDuration + .3f);
                Assert.That(_presentation.UsesItemGrip, Is.True);
                _input.MeleePressed = true;
                yield return null;
                _input.MeleePressed = false;
                yield return new WaitForSeconds(.1f);
                Assert.That(_presentation.UsesItemGrip, Is.False, "Strike owns the hands.");
                yield return new WaitForSeconds(_context.Shooter.MeleeDuration + .3f);
                Assert.That(_presentation.UsesItemGrip, Is.True);
                _input.WeaponSlotSelection = PlayerCharacterLoadout.UnarmedSlot;
                yield return null;
                _input.WeaponSlotSelection = 0;
                Assert.That(_presentation.UsesItemGrip, Is.False);
                for (int frame = 0; frame < 45; frame++)
                {
                    _motor.Tick(Vector2.up, false, frame == 15, .02f);
                    yield return null;
                    if (frame > 10)
                        Assert.That(_presentation.MeleePoseWeight, Is.LessThan(.001f),
                            "Unarmed locomotion and jumping retain their authored arms.");
                }
                _input.MeleePressed = true;
                yield return null;
                _input.MeleePressed = false;
                yield return new WaitForSeconds(.15f);
                Assert.That(_presentation.MeleePoseWeight, Is.GreaterThan(.98f));
                yield return new WaitForSeconds(_context.Shooter.MeleeDuration + .3f);
                _input.WeaponSlotSelection = PlayerCharacterLoadout.PistolSlot;
                yield return null;
                _input.WeaponSlotSelection = 0;
                yield return null;
                Assert.That(_presentation.UsesItemGrip, Is.True, "Equipment clears the unarmed overlay.");
            }
        }
    }
}
#endif
