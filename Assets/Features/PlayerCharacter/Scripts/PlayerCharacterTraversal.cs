using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // The motor calls Step so only one owner chooses the CharacterController movement each frame.
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterTraversal : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField, Min(.01f)] private float _slideDuration = .75f;
        [SerializeField, Min(0f)] private float _slideEndSpeed = 4f;
        [SerializeField, Min(0f)] private float _vaultReach = 2.5f;
        [SerializeField, Min(0f)] private float _maximumVaultHeight = 2.4f;
        [SerializeField, Min(0f)] private float _maximumVaultDepth = 2f;
        [SerializeField, Min(.01f)] private float _vaultDuration = .65f;
        [SerializeField] private LayerMask _obstacleMask = ~0;

        private readonly RaycastHit[] _hits = new RaycastHit[32];
        private readonly Collider[] _clearance = new Collider[32];
        private CharacterController _body;
        private IPlayerCharacterTraversalInput _input;
        private Vector3 _slideDirection, _vaultStart, _vaultEnd;
        private float _elapsed, _slideStartSpeed, _vaultLift;
        public bool IsSliding { get; private set; }
        public bool IsVaulting { get; private set; }
        public float Progress => IsVaulting ? Mathf.Clamp01(_elapsed / _vaultDuration) :
            IsSliding ? Mathf.Clamp01(_elapsed / _slideDuration) : 0f;

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _input = GetComponent<IPlayerCharacterTraversalInput>();
        }

        public bool Step(bool jump, float deltaTime)
        {
            if (!isActiveAndEnabled || !_body.enabled || deltaTime <= 0f) return false;
            if (_actions != null && !_actions.IsAlive) { Cancel(); return false; }
            if (IsVaulting) return AdvanceVault(deltaTime);
            if (_actions != null && !_actions.CanAttack) { if (IsSliding) FinishSlide(); return false; }
            if (IsSliding)
            {
                if (!_stance.IsCrouched || (jump && _stance.TrySetCrouched(false)))
                { IsSliding = false; return false; }
                _elapsed += deltaTime;
                float speed = Mathf.Lerp(_slideStartSpeed, _slideEndSpeed, Progress);
                CollisionFlags collision = _body.Move((_slideDirection * speed + Vector3.down * 2f) * deltaTime);
                if (_elapsed >= _slideDuration || (collision & CollisionFlags.Sides) != 0 || !_body.isGrounded) FinishSlide();
                return true;
            }
            if (_input != null && _input.SlidePressed && _movement.IsSprinting && _body.isGrounded && _stance.TrySetCrouched(true))
            {
                IsSliding = true; _elapsed = 0f;
                _slideDirection = Vector3.ProjectOnPlane(_movement.Velocity, Vector3.up).normalized;
                if (_slideDirection.sqrMagnitude < .001f) _slideDirection = transform.forward;
                _slideStartSpeed = Mathf.Max(_slideEndSpeed, Vector3.ProjectOnPlane(_movement.Velocity, Vector3.up).magnitude);
                return Step(false, deltaTime);
            }
            return jump && TryBeginVault() && AdvanceVault(deltaTime);
        }

        public bool TryBeginVault()
        {
            if (!isActiveAndEnabled || !_body.enabled || _stance == null || _stance.IsLowered ||
                !_body.isGrounded || IsSliding || IsVaulting || (_actions != null && !_actions.IsAlive)) return false;
            float scale = Mathf.Abs(transform.lossyScale.y);
            Vector3 feet = transform.TransformPoint(_body.center) - Vector3.up * (_body.height * scale * .5f);
            float probeHeight = Mathf.Max(_body.stepOffset + .1f, .5f) * scale;
            if (!Ray(new Ray(feet + Vector3.up * probeHeight, transform.forward), _vaultReach * scale, out RaycastHit obstacle)) return false;
            Vector3 topProbe = obstacle.point + transform.forward * (.1f * scale);
            topProbe.y = feet.y + (_maximumVaultHeight + .1f) * scale;
            if (!Ray(new Ray(topProbe, Vector3.down), _maximumVaultHeight * scale, out RaycastHit top)) return false;
            float height = top.point.y - feet.y;
            if (height <= probeHeight || height > _maximumVaultHeight * scale || Vector3.Dot(top.normal, Vector3.up) < .7f) return false;
            Vector3 landingProbe = obstacle.point + transform.forward * ((_maximumVaultDepth + _body.radius + .1f) * scale);
            landingProbe.y = top.point.y + .2f * scale;
            if (!Ray(new Ray(landingProbe, Vector3.down), height + .4f * scale, out RaycastHit landing) ||
                Vector3.Dot(landing.normal, Vector3.up) < .7f) return false;
            Vector3 end = landing.point + Vector3.up * (_body.height * scale * .5f + _body.skinWidth * scale) -
                          transform.TransformVector(_body.center);
            if (!HasSpace(end)) return false;
            _vaultStart = transform.position; _vaultEnd = end;
            _vaultLift = Mathf.Max(height + .25f * scale, end.y - _vaultStart.y);
            _elapsed = 0f; IsVaulting = true; return true;
        }

        private bool AdvanceVault(float deltaTime)
        {
            _elapsed += deltaTime;
            float t = Progress;
            Vector3 target;
            if (t < .35f) target = _vaultStart + Vector3.up * Mathf.Lerp(0f, _vaultLift, t / .35f);
            else if (t < .8f) target = Vector3.Lerp(_vaultStart, new Vector3(_vaultEnd.x, _vaultStart.y, _vaultEnd.z), (t - .35f) / .45f) + Vector3.up * _vaultLift;
            else target = Vector3.Lerp(new Vector3(_vaultEnd.x, _vaultStart.y + _vaultLift, _vaultEnd.z), _vaultEnd, (t - .8f) / .2f);
            CollisionFlags flags = _body.Move(target - transform.position);
            if ((flags & (CollisionFlags.Sides | CollisionFlags.Above)) != 0 || t >= 1f) IsVaulting = false;
            return true;
        }

        private bool Ray(Ray ray, float distance, out RaycastHit nearest)
        {
            nearest = default;
            int count = Physics.RaycastNonAlloc(ray, _hits, distance, _obstacleMask, QueryTriggerInteraction.Ignore);
            if (count == _hits.Length) return false;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _hits[i];
                if (hit.transform.IsChildOf(transform) || (found && hit.distance >= nearest.distance)) continue;
                nearest = hit; found = true;
            }
            return found;
        }

        private bool HasSpace(Vector3 position)
        {
            float scale = Mathf.Abs(transform.lossyScale.y);
            float radius = _body.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            Vector3 center = position + transform.TransformVector(_body.center);
            Vector3 half = Vector3.up * Mathf.Max(0f, _body.height * scale * .5f - radius);
            int count = Physics.OverlapCapsuleNonAlloc(center - half, center + half,
                Mathf.Max(.01f, radius - _body.skinWidth * scale), _clearance, _obstacleMask, QueryTriggerInteraction.Ignore);
            if (count == _clearance.Length) return false;
            for (int i = 0; i < count; i++) if (!_clearance[i].transform.IsChildOf(transform)) return false;
            return true;
        }

        private void FinishSlide()
        {
            IsSliding = false;
            // Existing stance clearance keeps crouch when standing would hit a ceiling.
            if (_stance != null && _stance.IsCrouched) _stance.TrySetCrouched(false);
        }

        public void Cancel() { IsSliding = false; IsVaulting = false; _elapsed = 0f; }
        private void OnDisable() => Cancel();
    }
}
