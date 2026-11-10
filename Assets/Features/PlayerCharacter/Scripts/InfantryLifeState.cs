using System;
using Battlefield.Framework.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Battlefield.Features.PlayerCharacter
{
    public enum InfantryLife { Alive, Downed, Eliminated }

    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public sealed class InfantryLifeState : MonoBehaviour, IHealthDamagePolicy
    {
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private PlayerVehicleOccupant _occupant;
        [SerializeField, Min(.01f)] private float _bleedOutDuration = 20f;
        [SerializeField, Min(.01f)] private float _downedHealth = 40f;
        [SerializeField] private float _reviveHealthFraction = .35f;
        private readonly Collider[] _space = new Collider[32];
        private Health _health;
        private CharacterController _body;
        private IPlayerCharacterLifeInput _input;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private float _remainingDownedHealth;
        private Object _reviver;
        private bool _started;
        public InfantryLife State { get; private set; }
        public float BleedOutRemaining { get; private set; }
        public bool IsDowned => State == InfantryLife.Downed;
        public bool IsEliminated => State == InfantryLife.Eliminated;
        public event Action Changed;

        private void Awake()
        {
            _health = GetComponent<Health>(); _body = GetComponent<CharacterController>();
            _input = GetComponent<IPlayerCharacterLifeInput>();
            _spawnPosition = transform.position; _spawnRotation = transform.rotation;
        }
        private void OnEnable()
        {
            _health.BindDamagePolicy(this); _health.OnDeath.AddListener(OnDeath);
            if (_started && _health.IsDead && State == InfantryLife.Alive) OnDeath();
        }
        private void Start()
        {
            _started = true;
            if (_health.IsDead && State == InfantryLife.Alive) OnDeath();
        }
        private void Update()
        {
            Tick(Time.deltaTime);
            if (_input == null) return;
            if (IsDowned && _input.GiveUpPressed) GiveUp();
            else if (IsEliminated && _input.RespawnPressed) TryRespawn();
        }
        public void Tick(float deltaTime)
        {
            if (!isActiveAndEnabled || !IsDowned || deltaTime <= 0f) return;
            BleedOutRemaining = Mathf.Max(0f, BleedOutRemaining - deltaTime);
            if (BleedOutRemaining <= 0f) GiveUp();
        }
        private void OnDeath()
        {
            if (!_health.IsDead || State != InfantryLife.Alive) return;
            // A dead occupant must stop controlling the vehicle even if a blocked exit cannot place them outside.
            _movement?.Stop();
            if (_occupant != null && _occupant.IsSeated) { State = InfantryLife.Eliminated; Changed?.Invoke(); return; }
            State = InfantryLife.Downed; _remainingDownedHealth = _downedHealth;
            BleedOutRemaining = _bleedOutDuration;
            if (_stance != null) _stance.TrySetStance(CharacterStance.Prone);
            Changed?.Invoke();
        }
        public bool TryHandleDamage(float damage)
        {
            if (!isActiveAndEnabled || State == InfantryLife.Alive) return false;
            if (IsDowned && damage > 0f)
            {
                _remainingDownedHealth -= damage;
                if (_remainingDownedHealth <= 0f) GiveUp();
            }
            return true;
        }
        public void GiveUp()
        {
            if (!IsDowned) return;
            State = InfantryLife.Eliminated; BleedOutRemaining = 0f; _reviver = null; Changed?.Invoke();
        }
        public bool TryClaimRevive(Object reviver)
        {
            if (!isActiveAndEnabled || !IsDowned || reviver == null || (_reviver != null && _reviver != reviver)) return false;
            _reviver = reviver; return true;
        }
        public void ReleaseRevive(Object reviver) { if (_reviver == reviver) _reviver = null; }
        public bool TryRevive()
        {
            if (!isActiveAndEnabled || !IsDowned || !_health.TryRevive(_health.MaxHealth * Mathf.Clamp(_reviveHealthFraction, .01f, 1f))) return false;
            State = InfantryLife.Alive; BleedOutRemaining = 0f; _reviver = null;
            if (_stance != null) _stance.TrySetCrouched(true);
            Changed?.Invoke(); return true;
        }
        public bool TryRespawn()
        {
            if (!isActiveAndEnabled || !IsEliminated) return false;
            if (_body != null)
            {
                float scale = Mathf.Abs(transform.lossyScale.y);
                // Respawn uses the authored standing capsule, not the current lowered posture.
                float height = _stance != null ? _stance.StandingHeight : _body.height;
                float radius = _body.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
                Vector3 authoredCenter = _stance != null ? _stance.StandingCenter : _body.center;
                Vector3 center = _spawnPosition + _spawnRotation * Vector3.Scale(authoredCenter, transform.lossyScale);
                Vector3 half = Vector3.up * Mathf.Max(0f, height * scale * .5f - radius);
                int count = Physics.OverlapCapsuleNonAlloc(center - half, center + half, Mathf.Max(.01f, radius - _body.skinWidth * scale), _space, ~0, QueryTriggerInteraction.Ignore);
                if (count == _space.Length) return false;
                for (int i = 0; i < count; i++) if (!_space[i].transform.IsChildOf(transform)) return false;
                if (_occupant != null && _occupant.IsSeated) _occupant.CurrentSeat.ReleaseOccupant(_occupant);
                bool enabledBody = _body.enabled; _body.enabled = false;
                transform.SetPositionAndRotation(_spawnPosition, _spawnRotation); _body.enabled = enabledBody;
            }
            else transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            if (!_health.TryRevive(_health.MaxHealth)) return false;
            _movement?.Stop(); _stance?.TrySetCrouched(false); _combat?.ResetForRespawn();
            State = InfantryLife.Alive; BleedOutRemaining = 0f; _reviver = null; Changed?.Invoke(); return true;
        }
        public bool TryDrag(Vector3 displacement)
        {
            if (!isActiveAndEnabled || !IsDowned || _movement == null || !_movement.isActiveAndEnabled) return false;
            _movement.QueueExternalDisplacement(displacement); return true;
        }
        private void OnDisable()
        {
            if (_health != null) { _health.OnDeath.RemoveListener(OnDeath); _health.UnbindDamagePolicy(this); }
            _reviver = null;
        }
    }
}
