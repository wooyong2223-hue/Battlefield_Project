using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Fits an authored reload to the current rifle's removable magazine and restores it on cancellation.
    [DefaultExecutionOrder(50)]
    public sealed class PlayerCharacterRifleReloadView : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterCombatController _combat;
        [SerializeField] private PlayerCharacterCombatView _presentation;
        [SerializeField] private PlayerCharacterLookController _view;
        [SerializeField] private PlayerCharacterCamera _camera;
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerCharacterWeaponAimView _aim;
        [SerializeField] private Vector3 _hipViewOffset = new(-.08f, .32f, .3f);
        [SerializeField] private Vector3 _adsViewOffset = new(-.06f, .26f, .55f);
        [SerializeField] private PlayerCharacterProneAnimationView _prone;
        public void SetPronePresentation(PlayerCharacterProneAnimationView view) => _prone = view;
        private IWeaponMagazine _observed;
        private WeaponModel _weapon;
        private Transform _torso;
        private Transform _rightUpper, _rightLower, _rightHand, _leftUpper, _leftLower, _leftHand;
        private readonly Transform[] _fingers = new Transform[15];
        private readonly Quaternion[] _fingerPose = new Quaternion[15];
        private float _rightReach, _leftReach, _startedAt, _duration;
        private Vector3 _rightTorsoOffset, _homePosition, _homeRootPosition;
        private Quaternion _homeRotation, _homeRootRotation;
        private Quaternion _rightRotationInTorso;
        private bool _active, _completed, _loweringRaisedHands;
        private Vector3 _entryTorsoOffset;
        private Quaternion _entryRotationInTorso;
        public bool IsActive => _active && _combat.CanUseCombat;
        public float Progress => !_active ? 0f : _completed ? 1f : Mathf.Clamp01((Time.time - _startedAt) / _duration);
        public float Blend => !IsActive ? 0f : Mathf.SmoothStep(0f, 1f, Progress / .12f) *
            (1f - Mathf.SmoothStep(0f, 1f, (Progress - .85f) / .15f));
        public bool IsLoweringRaisedHands => IsActive && _loweringRaisedHands;
        public Vector3 HandSettleDelta => !IsActive ? Vector3.zero :
            _torso.TransformVector(_rightTorsoOffset - _entryTorsoOffset) * Mathf.SmoothStep(0f, 1f, Progress / .18f) *
            (1f - Mathf.SmoothStep(0f, 1f, (Progress - .85f) / .15f));
        public Vector3 ViewOffset
        {
            get
            {
                Vector3 offset = Vector3.Lerp(_hipViewOffset, _adsViewOffset, _camera.FirstPersonAimBlend);
                if (IsLoweringRaisedHands) offset.y = Mathf.Min(0f, offset.y);
                return offset * Blend;
            }
        }
        private void Awake()
        {
            if (_combat == null || _presentation == null || _view == null || _camera == null || _animator == null || _aim == null || !_animator.isHuman)
            { Debug.LogError("Rifle reload presentation references are missing.", this); enabled = false; return; }
            _torso = _animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? _animator.GetBoneTransform(HumanBodyBones.Chest);
            _rightUpper = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _rightLower = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _leftUpper = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            _leftLower = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightReach = Vector3.Distance(_rightUpper.position, _rightLower.position) + Vector3.Distance(_rightLower.position, _rightHand.position);
            _leftReach = Vector3.Distance(_leftUpper.position, _leftLower.position) + Vector3.Distance(_leftLower.position, _leftHand.position);
            for (int i = 0; i < _fingers.Length; i++) _fingers[i] = _animator.GetBoneTransform((HumanBodyBones)((int)HumanBodyBones.LeftThumbProximal + i));
        }
        private void OnEnable()
        {
            if (_combat == null) return;
            _combat.EquipmentChanged += OnEquipment;
            Bind();
        }
        private void OnEquipment(bool armed) { Restore(); Bind(); }
        private void Bind()
        {
            if (_observed != null) { _observed.ReloadStarted -= Begin; _observed.ReloadCompleted -= Completed; }
            _observed = _combat.Magazine;
            if (_observed != null) { _observed.ReloadStarted += Begin; _observed.ReloadCompleted += Completed; }
        }
        private void Begin()
        {
            Restore();
            if (_presentation.UsesAuthoredReloadClip && (_prone == null || !_prone.IsPresenting)) return;
            var definition = _presentation.ActiveDefinition;
            var weapon = _presentation.ActiveWeapon;
            if (definition == null || !definition.CustomGrip || weapon == null || weapon.Magazine == null ||
                weapon.ReloadGrip == null || weapon.SupportHandGrip == null) return;
            _weapon = weapon; _active = true; _completed = false; _startedAt = Time.time; _duration = _observed.ReloadDuration;
            _entryTorsoOffset = _torso.InverseTransformPoint(_rightHand.position);
            _entryRotationInTorso = Quaternion.Inverse(_torso.rotation) * _view.AimRotation * Quaternion.Inverse(_presentation.GripRotation);
            _aim.GetReloadGrip(out _rightTorsoOffset, out _rightRotationInTorso);
            _loweringRaisedHands = Vector3.Dot(_torso.TransformVector(_entryTorsoOffset - _rightTorsoOffset), _combat.transform.up) > .05f;
            _homePosition = _weapon.Magazine.localPosition; _homeRotation = _weapon.Magazine.localRotation;
            _homeRootPosition = _weapon.transform.InverseTransformPoint(_weapon.Magazine.position);
            _homeRootRotation = Quaternion.Inverse(_weapon.transform.rotation) * _weapon.Magazine.rotation;
            for (int i = 0; i < _fingers.Length; i++) if (_fingers[i] != null) _fingerPose[i] = _fingers[i].localRotation;
        }
        private void Completed() => _completed = true;
        private void LateUpdate()
        {
            if (!_active) return;
            if (!_combat.CanUseCombat || _combat.IsAttacking || _weapon == null || _presentation.ActiveWeapon != _weapon ||
                (!_observed.IsReloading && !_completed) || (_completed && _presentation.UsesItemGrip))
            { Restore(); return; }
            float progress = Progress;
            float pull = progress > .18f && progress < .72f ? Mathf.Sin(Mathf.PI * Mathf.InverseLerp(.18f, .72f, progress)) : 0f;
            _weapon.Magazine.SetPositionAndRotation(_weapon.transform.TransformPoint(_homeRootPosition + _weapon.ReloadTravel * pull),
                _weapon.transform.rotation * _homeRootRotation * Quaternion.Euler(_weapon.ReloadTilt * pull));
            Vector3 magazine = _weapon.transform.InverseTransformPoint(_weapon.ReloadGrip.position);
            Quaternion magazineRotation = Quaternion.Inverse(_weapon.transform.rotation) * _weapon.ReloadGrip.rotation;
            Vector3 support = _weapon.transform.InverseTransformPoint(_weapon.SupportHandGrip.position);
            Quaternion supportRotation = Quaternion.Inverse(_weapon.transform.rotation) * _weapon.SupportHandGrip.rotation;
            float contact = progress < .18f ? Mathf.SmoothStep(0f, 1f, progress / .18f) :
                progress < .72f ? 1f : 1f - Mathf.SmoothStep(0f, 1f, (progress - .72f) / .28f);
            Vector3 leftLocal = Vector3.Lerp(support, magazine, contact);
            Quaternion leftLocalRotation = Quaternion.Slerp(supportRotation, magazineRotation, contact);
            if (_prone != null && _prone.IsPresenting)
            {
                // Native prone owns the torso and weapon hand; only solve magazine contact.
                Quaternion rotation = _rightHand.rotation;
                Quaternion model = rotation * _presentation.GripRotation;
                Vector3 offset = rotation * Vector3.Scale(_presentation.ActiveDefinition.GripPosition, _rightHand.lossyScale) + model * leftLocal;
                Vector3 hand = PlayerCharacterWeaponGripSolver.FitTwoHandGrip(_rightHand.position,
                    _rightUpper.position, _rightReach * .99f, _leftUpper.position, _leftReach * .99f, offset, 8);
                HumanoidArmSolver.Solve(_rightUpper, _rightLower, _rightHand, hand, rotation);
                HumanoidArmSolver.Solve(_leftUpper, _leftLower, _leftHand, hand + offset, model * leftLocalRotation);
                return;
            }
            // The authored torso carries the weapon during reload, rather than an eye-space pin.
            float settle = Mathf.SmoothStep(0f, 1f, progress / .18f);
            Quaternion rightRotation = _torso.rotation * Quaternion.Slerp(_entryRotationInTorso, _rightRotationInTorso, settle);
            Quaternion modelRotation = rightRotation * _presentation.GripRotation;
            Vector3 gripOffset = rightRotation * Vector3.Scale(_presentation.ActiveDefinition.GripPosition, _rightHand.lossyScale);
            Vector3 leftOffset = gripOffset + modelRotation * leftLocal;
            Vector3 right = _torso.TransformPoint(Vector3.Lerp(_entryTorsoOffset, _rightTorsoOffset, settle));
            right = PlayerCharacterWeaponGripSolver.FitTwoHandGrip(right, _rightUpper.position, _rightReach * .99f,
                _leftUpper.position, _leftReach * .99f, leftOffset, 16);
            HumanoidArmSolver.Solve(_rightUpper, _rightLower, _rightHand, right, rightRotation);
            HumanoidArmSolver.Solve(_leftUpper, _leftLower, _leftHand, _rightHand.position + leftOffset, modelRotation * leftLocalRotation);
            for (int i = 0; i < _fingers.Length; i++)
                if (_fingers[i] != null) _fingers[i].localRotation = Quaternion.Slerp(_fingers[i].localRotation, _fingerPose[i], contact);
        }
        private void Restore()
        {
            if (_active && _weapon != null && _weapon.Magazine != null)
            { _weapon.Magazine.localPosition = _homePosition; _weapon.Magazine.localRotation = _homeRotation; }
            _active = _completed = _loweringRaisedHands = false; _weapon = null;
        }
        private void OnDisable()
        {
            Restore();
            if (_combat != null) _combat.EquipmentChanged -= OnEquipment;
            if (_observed != null) { _observed.ReloadStarted -= Begin; _observed.ReloadCompleted -= Completed; }
            _observed = null;
        }
    }
}
