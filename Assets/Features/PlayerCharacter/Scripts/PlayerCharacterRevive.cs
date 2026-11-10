using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-80)]
    public sealed class PlayerCharacterRevive : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private Team _team;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField, Min(.01f)] private float _range = 3f;
        [SerializeField, Min(.01f)] private float _duration = 3.5f;
        [SerializeField, Min(0f)] private float _dragSpeed = 2.5f;
        private readonly Collider[] _nearby = new Collider[32];
        private readonly RaycastHit[] _sight = new RaycastHit[32];
        private IPlayerCharacterLifeInput _input;
        private IPlayerCharacterInput _movementInput;
        private bool _autoCrouched;
        private uint _autoCrouchVersion;
        public InfantryLifeState Candidate { get; private set; }
        public InfantryLifeState Target { get; private set; }
        public bool IsReviving => Target != null;
        public float Progress { get; private set; }
        private void Awake() { _input = GetComponent<IPlayerCharacterLifeInput>(); _movementInput = GetComponent<IPlayerCharacterInput>(); }
        private void Update()
        {
            Candidate = FindCandidate();
            Tick(_input != null && _input.InteractHeld, _movementInput != null && _movementInput.SprintHeld, Time.deltaTime);
        }
        public bool CanRevive(InfantryLifeState target)
        {
            if (!isActiveAndEnabled || target == null || !target.isActiveAndEnabled || !target.IsDowned || target.transform == transform || _health == null || _health.IsDead ||
                (_traversal != null && _traversal.IsVaulting) || _team == null) return false;
            Team other = target.GetComponent<Team>();
            if (other == null || other.CurrentTeam != _team.CurrentTeam || other.CurrentTeam == TeamType.Neutral) return false;
            Collider body = target.GetComponent<Collider>();
            Vector3 from = transform.position, to = body != null ? body.bounds.center : target.transform.position;
            Vector3 delta = to - from;
            if (delta.magnitude > _range) return false;
            int count = Physics.RaycastNonAlloc(from, delta.normalized, _sight, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
            if (count == _sight.Length) return false;
            for (int i = 0; i < count; i++)
                if (!_sight[i].transform.IsChildOf(transform) && !_sight[i].transform.IsChildOf(target.transform)) return false;
            return true;
        }
        private InfantryLifeState FindCandidate()
        {
            if (_health == null || _health.IsDead) return null;
            int count = Physics.OverlapSphereNonAlloc(transform.position, _range, _nearby, ~0, QueryTriggerInteraction.Ignore);
            if (count == _nearby.Length) return null;
            InfantryLifeState closest = null; float distance = float.PositiveInfinity;
            for (int i = 0; i < count; i++)
            {
                var candidate = _nearby[i].GetComponentInParent<InfantryLifeState>();
                if (!CanRevive(candidate)) continue;
                float squared = (candidate.transform.position - transform.position).sqrMagnitude;
                if (squared < distance) { closest = candidate; distance = squared; }
            }
            return closest;
        }
        public bool TryBegin(InfantryLifeState target)
        {
            if (!CanRevive(target)) return false;
            if (Target == target) return true;
            Cancel();
            if (!target.TryClaimRevive(this)) return false;
            Target = target; Progress = 0f;
            _autoCrouched = _stance != null && _stance.Current == CharacterStance.Standing && _stance.TrySetCrouched(true);
            if (_autoCrouched) _autoCrouchVersion = _stance.ChangeVersion;
            return true;
        }
        public void Tick(bool held, bool drag, float deltaTime)
        {
            if (!isActiveAndEnabled || deltaTime <= 0f) return;
            if (!held) { Cancel(); return; }
            if (Target == null && !TryBegin(Candidate)) return;
            if (!CanRevive(Target)) { Cancel(); return; }
            if (drag && (_stance == null || !_stance.IsProne))
            {
                Vector3 desired = transform.position - transform.forward * 1.5f;
                Vector3 delta = Vector3.ProjectOnPlane(desired - Target.transform.position, Vector3.up);
                Target.TryDrag(Vector3.ClampMagnitude(delta, _dragSpeed * deltaTime));
            }
            Progress = Mathf.Clamp01(Progress + deltaTime / _duration);
            if (Progress >= 1f) { Target.TryRevive(); Cancel(); }
        }
        public void Cancel()
        {
            if (Target != null) Target.ReleaseRevive(this);
            Target = null; Progress = 0f;
            if (_autoCrouched && _health != null && !_health.IsDead && _stance != null &&
                _stance.ChangeVersion == _autoCrouchVersion && _stance.IsCrouched)
                _stance.TrySetCrouched(false);
            _autoCrouched = false;
        }
        private void OnDisable() { Cancel(); Candidate = null; }
    }
}
