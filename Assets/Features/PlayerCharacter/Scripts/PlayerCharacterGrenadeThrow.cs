using Battlefield.Features.Projectile;
using Battlefield.Features.VFX;
using Battlefield.Features.Weapon;
using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    public enum GrenadeHandlingState { Holstered, Equipping, Ready, Throwing }
    public enum GrenadeThrowMode { Overhand, Underhand }

    [DefaultExecutionOrder(-50)]
    public sealed class PlayerCharacterGrenadeThrow : MonoBehaviour
    {
        [SerializeField] private GrenadeDefinition _definition;
        [SerializeField, Min(0)] private int _capacity = 3;
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private MonoBehaviour _aimSourceBehaviour;
        [SerializeField] private MonoBehaviour _effectSourceBehaviour;
        private IPlayerCharacterAimSource _aim;
        private IPlayerCharacterThrowInput _input;
        private IExplosionEffectPlayer _effect;
        [SerializeField] private InfantryLifeState _life;
        private bool _wasEliminated, _released, _throwRequested, _aiming;
        private GrenadeHandlingState _state;
        private float _elapsed, _equipElapsed;
        private GrenadeProjectile _held;
        private readonly RaycastHit[] _clearHits = new RaycastHit[32];
        private readonly Collider[] _clearOverlaps = new Collider[32];
        public GrenadeHandlingState State => _state;
        public GrenadeThrowMode ThrowMode { get; private set; }
        public bool IsEquipped => _state != GrenadeHandlingState.Holstered;
        public bool IsThrowing => IsEquipped && _throwRequested;
        public bool IsAiming => IsEquipped && _aiming && !_throwRequested;
        public float TrajectoryDuration => _definition != null ? _definition.Fuse : 0f;
        public int Remaining { get; private set; }
        public GrenadeProjectile Held => _held;
        public float HandlingProgress => _definition == null || !_definition.UseAnimations || !_definition.HasHandlingAnimations ? 0f :
            (_state == GrenadeHandlingState.Equipping ? Mathf.Clamp01(_equipElapsed / _definition.EquipAnimation.length) :
            _state == GrenadeHandlingState.Ready ? Mathf.Repeat(_equipElapsed / _definition.IdleAnimation.length, 1f) : Progress);
        public float Progress => _definition != null && _definition.UseAnimations && _definition.ThrowAnimation != null ? Mathf.Clamp01(_elapsed / _definition.ThrowAnimation.length) : 0f;
        public float PoseWeight => !IsEquipped || _definition == null || !_definition.UseAnimations ? 0f : (_state == GrenadeHandlingState.Equipping ? Mathf.SmoothStep(0f, 1f, _equipElapsed / .08f) : 1f) *
            (1f - Mathf.SmoothStep(0f, 1f, (_elapsed - _definition.ThrowAnimation.length) / .12f));
        private void Awake()
        {
            Remaining = _capacity;
            _input = GetComponent<IPlayerCharacterThrowInput>();
            _aim = _aimSourceBehaviour as IPlayerCharacterAimSource;
            _effect = _effectSourceBehaviour as IExplosionEffectPlayer;
            if (_life != null) _life.Changed += OnLifeChanged;
            if (_combat != null) _combat.EquipmentChanged += OnEquipmentChanged;
        }
        public void Initialize(GrenadeDefinition definition, PlayerCharacterController combat, PlayerCharacterStance stance,
            PlayerCharacterMovement movement, PlayerCharacterActionPolicy actions, IPlayerCharacterAimSource aim,
            IPlayerCharacterThrowInput input, IExplosionEffectPlayer effect, InfantryLifeState life)
        {
            if (_life != null) _life.Changed -= OnLifeChanged;
            if (_combat != null) _combat.EquipmentChanged -= OnEquipmentChanged;
            _definition = definition; _combat = combat; _stance = stance; _movement = movement; _actions = actions;
            _aim = aim; _input = input; _effect = effect; _life = life; Remaining = _capacity;
            _aimSourceBehaviour = aim as MonoBehaviour; _effectSourceBehaviour = effect as MonoBehaviour;
            if (_life != null) _life.Changed += OnLifeChanged;
            if (_combat != null) _combat.EquipmentChanged += OnEquipmentChanged;
        }
        private void OnEquipmentChanged(bool armed) => Cancel();
        private void OnLifeChanged()
        {
            if (_life.IsEliminated) _wasEliminated = true;
            if (_life.State != InfantryLife.Alive) Cancel();
            else if (_wasEliminated) { Remaining = _capacity; _wasEliminated = false; }
        }
        private void Update()
        {
            if (_input != null)
            {
                bool equip = _input.EquipGrenadePressed;
                bool pressed = _input.ThrowGrenadePressed;
                bool toggleMode = _input.ToggleGrenadeThrowModePressed;
                if (IsEquipped && toggleMode) _input.ConsumeAimUntilRelease();
                // Cancellation wins over release, including both arriving this frame.
                if (_input.WeaponSlotSelection != 0 || (equip && IsEquipped) ||
                    (IsAiming && !_input.CanControlGrenade)) Cancel();
                else
                {
                    if (equip) TryEquip();
                    if (pressed && IsEquipped) TryBeginAim();
                    if (toggleMode) TryToggleThrowMode();
                    if (IsAiming && _input.ThrowGrenadeReleased) TryThrow();
                    else if (IsAiming && !_input.ThrowGrenadeHeld) Cancel();
                }
            }
            Tick(Time.deltaTime);
        }
        public bool TryEquip()
        {
            if (!isActiveAndEnabled || IsEquipped || Remaining <= 0 || _definition == null || _definition.Projectile == null ||
                (_definition.UseAnimations && (_definition.ThrowAnimation == null || !_definition.HasHandlingAnimations)) || _combat == null || !_combat.CanUseCombat || _combat.IsAttacking ||
                (_combat.Magazine != null && _combat.Magazine.IsReloading) || _aim == null || _movement == null || !_movement.IsGrounded ||
                (_stance != null && (_stance.IsProne || _stance.IsProneTransitioning)) || (_actions != null && !_actions.CanStartTacticalAction)) return false;
            _elapsed = _equipElapsed = 0f; _released = _throwRequested = _aiming = false;
            ThrowMode = GrenadeThrowMode.Overhand;
            _state = _definition.UseAnimations ? GrenadeHandlingState.Equipping : GrenadeHandlingState.Ready;
            _held = Instantiate(_definition.Projectile, transform.position, Quaternion.identity);
            _held.GetComponent<Rigidbody>().isKinematic = true; _held.GetComponent<Collider>().enabled = false;
            _input?.ConsumePrimaryUntilRelease();
            return true;
        }
        public bool TryBeginAim()
        {
            if (!isActiveAndEnabled || !IsEquipped || _throwRequested || _aiming ||
                _combat == null || !_combat.CanUseCombat || (_actions != null && !_actions.CanUseHands)) return false;
            _aiming = true;
            _input?.ConsumePrimaryUntilRelease();
            return true;
        }
        public bool TryThrow()
        {
            if (!isActiveAndEnabled || !IsEquipped || _throwRequested || _combat == null || !_combat.CanUseCombat ||
                (_actions != null && !_actions.CanUseHands)) return false;
            _throwRequested = true; _aiming = false;
            if (!_definition.UseAnimations)
            {
                // Gameplay release has no dependency on a clip, its duration or release phase.
                bool launched = Release();
                Cancel();
                return launched;
            }
            if (_state == GrenadeHandlingState.Ready) { _elapsed = 0f; _state = GrenadeHandlingState.Throwing; }
            _input?.ConsumePrimaryUntilRelease();
            return true;
        }
        public bool TryToggleThrowMode()
        {
            if (!isActiveAndEnabled || !IsEquipped || _throwRequested || _combat == null || !_combat.CanUseCombat ||
                (_actions != null && !_actions.CanUseHands)) return false;
            ThrowMode = ThrowMode == GrenadeThrowMode.Overhand ? GrenadeThrowMode.Underhand : GrenadeThrowMode.Overhand;
            _input?.ConsumeAimUntilRelease();
            return true;
        }
        public void Tick(float deltaTime)
        {
            if (!IsEquipped || deltaTime <= 0f) return;
            if (_combat == null || !_combat.CanUseCombat || (_actions != null && !_actions.CanUseHands) || (_stance != null && _stance.IsProne)) { Cancel(); return; }
            if (!_definition.UseAnimations) return;
            if (_state == GrenadeHandlingState.Equipping)
            {
                _equipElapsed = Mathf.Min(_equipElapsed + deltaTime, _definition.EquipAnimation.length);
                if (_equipElapsed >= _definition.EquipAnimation.length)
                {
                    _state = _throwRequested ? GrenadeHandlingState.Throwing : GrenadeHandlingState.Ready;
                    _elapsed = 0f;
                    if (!_throwRequested) _equipElapsed = 0f;
                }
                return;
            }
            if (_state == GrenadeHandlingState.Ready) { _equipElapsed += deltaTime; return; }
            _elapsed += deltaTime;
            if (!_released && Progress >= _definition.ReleaseFraction) Release();
            if (IsEquipped && _elapsed >= _definition.ThrowAnimation.length + .12f) Cancel();
        }
        private bool Release()
        {
            GetLaunch(out Vector3 spawn, out Vector3 velocity);
            Vector3 direction = _aim.GetAimRay().direction.normalized;
            if (!Clear(spawn)) { Cancel(); return false; }
            var projectile = _held; _held = null; _released = true;
            projectile.transform.SetParent(null, false); projectile.transform.SetPositionAndRotation(spawn, Quaternion.LookRotation(direction));
            projectile.transform.localScale = Vector3.one; projectile.GetComponent<Rigidbody>().position = spawn;
            projectile.GetComponent<Collider>().enabled = true;
            if (projectile.Arm(transform, velocity, _definition, _effect)) { Remaining--; return true; }
            Destroy(projectile.gameObject);
            return false;
        }
        public bool TryGetAimTrajectory(out Vector3 origin, out Vector3 velocity)
        {
            origin = velocity = Vector3.zero;
            if (!IsAiming || _aim == null || _definition == null || _movement == null) return false;
            GetLaunch(out origin, out velocity);
            return Clear(origin);
        }
        private void GetLaunch(out Vector3 origin, out Vector3 velocity)
        {
            Vector3 direction = _aim.GetAimRay().direction.normalized;
            bool underhand = ThrowMode == GrenadeThrowMode.Underhand;
            float height = _stance != null ? _stance.StandingHeight * (underhand ? .15f : .3f) : underhand ? .5f : 1f;
            origin = transform.position + transform.up * height + direction * .6f;
            velocity = direction * (underhand ? _definition.UnderhandLaunchSpeed : _definition.LaunchSpeed) +
                Vector3.up * (underhand ? _definition.UnderhandUpwardSpeed : _definition.UpwardSpeed) + _movement.Velocity;
        }
        private bool Clear(Vector3 point)
        {
            // Sweep from the body to prevent spawning on the far side of a thin wall.
            Vector3 origin = transform.position + transform.up * (_stance != null ? _stance.StandingHeight * .3f : 1f);
            int hits = Physics.SphereCastNonAlloc(origin, .13f, (point - origin).normalized, _clearHits, Vector3.Distance(origin, point), ~0, QueryTriggerInteraction.Ignore);
            if (hits == _clearHits.Length) return false;
            for (int i = 0; i < hits; i++)
                if (!IsOwnedCollider(_clearHits[i].transform)) return false;
            int overlaps = Physics.OverlapSphereNonAlloc(point, .13f, _clearOverlaps, ~0, QueryTriggerInteraction.Ignore);
            if (overlaps == _clearOverlaps.Length) return false;
            for (int i = 0; i < overlaps; i++)
                if (!IsOwnedCollider(_clearOverlaps[i].transform)) return false;
            return true;
        }
        private bool IsOwnedCollider(Transform target) => target.IsChildOf(transform) || (_held != null && target.IsChildOf(_held.transform));
        public void AttachHeldVisual(Transform hand)
        {
            if (_held == null || hand == null) return;
            _held.transform.SetParent(hand, false);
            Vector3 scale = hand.lossyScale;
            _held.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            _held.transform.localPosition = Vector3.zero; _held.transform.localRotation = Quaternion.identity;
        }
        public void Cancel()
        {
            if (IsEquipped) _input?.ConsumePrimaryUntilRelease();
            _state = GrenadeHandlingState.Holstered; _throwRequested = _aiming = false;
            if (_held != null) Destroy(_held.gameObject); _held = null;
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() { Cancel(); if (_life != null) _life.Changed -= OnLifeChanged; if (_combat != null) _combat.EquipmentChanged -= OnEquipmentChanged; }
    }
}
