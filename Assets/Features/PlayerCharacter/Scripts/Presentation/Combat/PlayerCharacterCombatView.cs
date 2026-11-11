using Battlefield.Features.Weapon;
using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(40)]
    public sealed class PlayerCharacterCombatView : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterCombatController _combat;
        [SerializeField] private PlayerCharacterLookController _view;
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private WeaponModel _pistolPrefab;
        [SerializeField] private SkinnedMeshRenderer _firstPersonArms;
        [SerializeField] private FirstPersonArmsView _firstPersonRig;
        [SerializeField] private Vector3 _gripPosition;
        [SerializeField] private Vector3 _gripEuler;
        [SerializeField] private AnimationClip _reloadAnimation;
        [SerializeField] private PlayerCharacterProneAnimationView _prone;
        public void SetPronePresentation(PlayerCharacterProneAnimationView view) => _prone = view;
        [SerializeField] private PlayerCharacterTacticalAnimationView _tactical;
        public void SetTacticalPresentation(PlayerCharacterTacticalAnimationView view) => _tactical = view;
        private bool HasAuthoredProne => _prone != null && _prone.IsPresenting;
        private bool _hasReloadSpeed;
        private static readonly int ReloadSpeed = Animator.StringToHash("ReloadSpeed");
        private PlayerCharacterWeaponModelPresentation _weaponModels;
        private RuntimeAnimatorController _baseController;
        [SerializeField] private ArmedLocomotionAnimationSet _locomotionAnimations;
        private readonly EquippedAnimationControllers _equipmentControllers = new();
        public void SetLocomotionAnimations(ArmedLocomotionAnimationSet animations)
        {
            if (_locomotionAnimations == animations) return;
            _locomotionAnimations = animations;
            if (Pistol != null && isActiveAndEnabled) RefreshEquipment();
        }
        private IWeaponMagazine _observedMagazine;
        private int _layer, _meleeLayer;
        private int _reloadLayer = -1;
        private float _reloadBlend, _meleeBlend;
        private bool _meleeAction;
        public float MeleePoseWeight => _meleeBlend;
        public float ReloadPoseWeight => _reloadBlend;
        public int ReloadSequence { get; private set; }
        public bool UsesItemGrip => enabled && _combat.CanUseCombat && _combat.IsArmed &&
            !HasAuthoredProne && !_combat.IsAttacking && (_combat.Magazine == null || !_combat.Magazine.IsReloading) &&
            (_actions == null || _actions.CanAttack) && _reloadBlend <= .001f && _meleeBlend <= .001f;
        private float _actionEndsAt, _recoil;
        private bool _cross;
        private PlayerCharacterStance _stance;
        private readonly PlayerCharacterWeaponPoseState _weaponPose = new();
        private static readonly int GaitSpeed = Animator.StringToHash("Speed");
        private static readonly int Pitch = Animator.StringToHash("CombatPitch");
        public float WeaponForwardScale => _weaponPose.ForwardScale;
        public float WeaponVerticalOffset => _weaponPose.VerticalOffset;
        public float SupportHandWeight => _weaponPose.SupportHandWeight;
        public Quaternion WeaponPoseRotation(Quaternion aim) => _weaponPose.Rotation(aim);
        public WeaponModel Pistol => _weaponModels?.Pistol;
        public WeaponModel ActiveWeapon => _weaponModels?.ActiveWeapon;
        public InfantryWeaponDefinition ActiveDefinition => _combat.WeaponDefinition;
        internal bool UsesAuthoredReloadClip { get; private set; }
        public SkinnedMeshRenderer FirstPersonArms => _firstPersonArms;
        public Quaternion GripRotation => ActiveDefinition != null && ActiveDefinition.CustomGrip
            ? ActiveDefinition.GripRotation : Quaternion.Euler(_gripEuler);
        public Vector3 GripPosition => ActiveDefinition != null && ActiveDefinition.CustomGrip
            ? ActiveDefinition.GripPosition : _gripPosition;
        public float RecoilAmount => _recoil;
        private void Awake()
        {
            if (_combat == null || _view == null || _animator == null || _pistolPrefab == null ||
                _firstPersonArms == null || _firstPersonRig == null || !_firstPersonRig.Initialize() ||
                !_animator.isHuman || (_layer = _animator.GetLayerIndex("UpperCombat")) < 0 ||
                (_meleeLayer = _animator.GetLayerIndex("MeleeCombat")) < 0)
            { Debug.LogError("Player combat presentation references are missing.", this); enabled = false; return; }
            _stance = _combat.GetComponent<PlayerCharacterStance>();
            _reloadLayer = _animator.GetLayerIndex("ReloadUpperBody");
            _baseController = _animator.runtimeAnimatorController;
            foreach (var parameter in _animator.parameters)
                if (parameter.nameHash == ReloadSpeed) _hasReloadSpeed = true;
            _weaponModels = new PlayerCharacterWeaponModelPresentation(
                _pistolPrefab, _animator.GetBoneTransform(HumanBodyBones.RightHand));
            AttachWeapon();
            _combat.BindMuzzle(Pistol.Muzzle);
        }
        private void OnEnable()
        {
            if (Pistol == null) return;
            _combat.Fired += OnFired;
            _combat.EquipmentChanged += OnEquipmentChanged;
            _combat.MeleeStarted += OnMelee;
            RefreshEquipment();
        }
        private void RefreshEquipment()
        {
            if (_observedMagazine != null) _observedMagazine.ReloadStarted -= OnReload;
            _observedMagazine = _combat.Magazine;
            if (_observedMagazine != null) _observedMagazine.ReloadStarted += OnReload;
            var definition = ActiveDefinition;
            _weaponModels.Select(_combat.IsArmed, definition);
            var controller = definition != null && definition.Animations != null ? definition.Animations : _baseController;
            var movement = _combat.IsArmed && _locomotionAnimations != null ? _locomotionAnimations.Find(controller) : null;
            var reloadClip = _combat.IsArmed && definition != null ? definition.ReloadClip : null;
            controller = _equipmentControllers.Resolve(movement, controller, _reloadAnimation, reloadClip);
            UsesAuthoredReloadClip = reloadClip != null && _reloadAnimation != null &&
                controller is AnimatorOverrideController overrides && overrides[_reloadAnimation] == reloadClip;
            if (_animator.runtimeAnimatorController != controller) _animator.runtimeAnimatorController = controller;
            // Selection can happen before the camera/copied rig has evaluated this frame.
            AttachWeapon(false);
            _combat.BindMuzzle(ActiveWeapon != null ? ActiveWeapon.Muzzle : Pistol.Muzzle);
            ResetPose();
            _weaponModels.SetVisible(_combat.CanUseCombat && (_actions == null || _actions.CanAttack));
        }
        private void AttachWeapon(bool useViewHand = true)
        {
            if (ActiveWeapon == null) return;
            Transform hand = useViewHand && _view.IsFirstPersonView ? _firstPersonRig.RightHand : _weaponModels.WorldHand;
            _weaponModels.Attach(hand, GripPosition, GripRotation);
        }
        public bool TryGetSupportHandPose(out Vector3 offset, out Quaternion rotation)
        {
            if (_weaponModels != null) return _weaponModels.TryGetSupportHandPose(out offset, out rotation);
            offset = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }
        private void LateUpdate()
        {
            if (Pistol == null) return;
            bool presenting = _combat.CanUseCombat && (_actions == null || _actions.CanAttack);
            AttachWeapon();
            _weaponModels.SetVisible(presenting && _combat.IsArmed);
            _firstPersonArms.enabled = (presenting || (_tactical != null && _tactical.IsPresenting)) && _view.IsFirstPersonView;
            bool action = Time.time < _actionEndsAt;
            bool ready = (_stance != null && _stance.IsProne) || _view.IsAiming || _view.IsAds || action || _combat.IsAttacking ||
                (_combat.Magazine != null && _combat.Magazine.IsReloading);
            CharacterWeaponPoseMode pose = !presenting || !_combat.IsArmed ? CharacterWeaponPoseMode.Hidden
                : _view.IsFirstPersonView ? CharacterWeaponPoseMode.FirstPerson : ready ? CharacterWeaponPoseMode.Ready
                : _animator.GetFloat(GaitSpeed) > 1.1f ? CharacterWeaponPoseMode.Sprint : CharacterWeaponPoseMode.Carry;
            _weaponPose.Tick(pose, Time.deltaTime);
            _animator.SetLayerWeight(_layer, presenting && !HasAuthoredProne && _combat.IsArmed ? _weaponPose.UpperLayerWeight : 0f);
            if (_reloadLayer >= 0)
            {
                bool reloading = presenting && !HasAuthoredProne && _combat.IsArmed && _combat.Magazine != null && _combat.Magazine.IsReloading;
                _reloadBlend = HasAuthoredProne || !presenting || !_combat.IsArmed ? 0f : reloading ? 1f : Mathf.MoveTowards(_reloadBlend, 0f, Time.deltaTime * 12.5f);
                _animator.SetLayerWeight(_reloadLayer, _reloadBlend);
            }
            float pitch = Mathf.DeltaAngle(0f, _view.AimRotation.eulerAngles.x);
            _animator.SetFloat(Pitch, _view.IsFirstPersonView ? 0f : -pitch / 80f);
            if (!action && _actionEndsAt > 0f)
            {
                _actionEndsAt = 0f; _meleeAction = false;
                _animator.CrossFadeInFixedTime(_combat.IsArmed ? "UpperCombat.PistolHold" : "MeleeCombat.FistsHold", .08f,
                    _combat.IsArmed ? _layer : _meleeLayer);
            }
            bool guarding = !_combat.IsArmed && _animator.GetBool("Grounded") &&
                (_view.IsAiming || (_view.IsFirstPersonView && _animator.GetFloat(GaitSpeed) < .05f));
            _meleeBlend = Mathf.MoveTowards(_meleeBlend, presenting && !HasAuthoredProne && (_meleeAction || guarding) ? 1f : 0f, Time.deltaTime * 12.5f);
            _animator.SetLayerWeight(_meleeLayer, _meleeBlend);
            _recoil = Mathf.MoveTowards(_recoil, 0f, Time.deltaTime * 12f);
            if (ActiveWeapon != null)
                _weaponModels.ApplyRecoil(GripPosition, GripRotation, _recoil, ActiveDefinition);
        }
        private void Play(string state, float duration, bool melee = false)
        {
            _meleeAction = melee;
            if (melee) _animator.CrossFadeInFixedTime("MeleeCombat." + state, .05f, _meleeLayer, 0f);
            else { _animator.SetLayerWeight(_layer, 1f); _animator.Play("UpperCombat." + state, _layer, 0f); }
            _actionEndsAt = Time.time + duration;
        }
        private void OnFired()
        {
            _weaponPose.RequireReady();
            _recoil = 1f;
            _meleeAction = false;
            // Keep the authored grip running; restarting a full-arm shot pose
            // on every round produces elbow snaps on the retargeted avatar.
            _actionEndsAt = Time.time + _combat.ShotInterval;
        }
        private void OnReload()
        {
            ReloadSequence = unchecked(ReloadSequence + 1);
            if (_hasReloadSpeed && _reloadAnimation != null)
            {
                AnimationClip clip = _reloadAnimation;
                if (_animator.runtimeAnimatorController is AnimatorOverrideController overrides)
                    clip = overrides[_reloadAnimation] ?? _reloadAnimation;
                _animator.SetFloat(ReloadSpeed, clip.length / _combat.Magazine.ReloadDuration);
            }
            Play("PistolReload", _combat.Magazine.ReloadDuration);
            if (_reloadLayer < 0) return;
            _reloadBlend = 1f; _animator.SetLayerWeight(_reloadLayer, 1f);
            _animator.Play("ReloadUpperBody.PistolReload", _reloadLayer, 0f);
        }
        private void OnMelee(bool armed)
        {
            _cross = !_cross;
            Play(armed ? "PistolMelee" : _cross ? "PunchJab" : "PunchCross", _combat.MeleeDuration, true);
        }
        private void OnEquipmentChanged(bool armed) => RefreshEquipment();
        private void ResetPose()
        {
            _meleeBlend = _reloadBlend = 0f;
            _animator.SetLayerWeight(_meleeLayer, 0f);
            if (_reloadLayer >= 0) _animator.SetLayerWeight(_reloadLayer, 0f);
            _weaponPose.Reset(); _actionEndsAt = _recoil = 0f; _meleeAction = false;
            _animator.Play(_combat.IsArmed ? "UpperCombat.PistolHold" : "MeleeCombat.FistsHold", _combat.IsArmed ? _layer : _meleeLayer, 0f);
            _animator.SetLayerWeight(_layer, _combat.IsArmed ? 1f : 0f);
        }
        private void OnDisable()
        {
            if (Pistol == null) return;
            _combat.Fired -= OnFired; _combat.EquipmentChanged -= OnEquipmentChanged; _combat.MeleeStarted -= OnMelee;
            if (_observedMagazine != null) _observedMagazine.ReloadStarted -= OnReload;
            _observedMagazine = null;
            _weaponPose.Reset(); _weaponModels.HideAll();
            _firstPersonArms.enabled = false;
            _animator.SetLayerWeight(_layer, 0f); _actionEndsAt = 0f;
            _animator.SetLayerWeight(_meleeLayer, 0f); _meleeBlend = _reloadBlend = 0f; _meleeAction = false;
            if (_reloadLayer >= 0) _animator.SetLayerWeight(_reloadLayer, 0f);
        }
        private void OnDestroy()
        {
            _equipmentControllers.Dispose();
            _weaponModels?.DestroyModels();
            if (_combat != null) _combat.BindMuzzle(null);
        }
    }
}
