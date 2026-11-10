using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterMantle : MonoBehaviour
    {
        [SerializeField] private MantleDefinition _definition;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        private CharacterController _body;
        private Vector3 _start, _end;
        private float _elapsed, _lift, _recoverUntil;
        public bool IsClimbing { get; private set; }
        public bool IsBusy => IsClimbing || Time.time < _recoverUntil;
        public float AnimationProgress => !IsClimbing ? 0f : Mathf.Clamp01((_elapsed - _definition.ReachDuration) / Mathf.Max(.01f, _definition.Duration - _definition.ReachDuration));
        public Vector3 LandingPosition => _end;
        private void Awake() => _body = GetComponent<CharacterController>();
        public void Initialize(MantleDefinition definition, PlayerCharacterStance stance, PlayerCharacterTraversal traversal, PlayerCharacterActionPolicy actions)
        { _definition = definition; _stance = stance; _traversal = traversal; _actions = actions; }
        public bool Step(bool requested, float deltaTime)
        {
            if (!isActiveAndEnabled || _body == null || !_body.enabled || deltaTime <= 0f) return false;
            if (_actions != null && !_actions.IsAlive) { Cancel(); return false; }
            if (!IsClimbing && (!requested || !TryBegin())) return false;
            _elapsed += deltaTime;
            float t = Mathf.Clamp01(_elapsed / _definition.Duration);
            // Lift clear of the front face before crossing it; Move retains collision authority.
            Vector3 raised = _start + Vector3.up * _lift;
            Vector3 target = t < .45f ? Vector3.Lerp(_start, raised, Mathf.SmoothStep(0f, 1f, t / .45f))
                : Vector3.Lerp(raised, _end, Mathf.SmoothStep(0f, 1f, (t - .45f) / .55f));
            CollisionFlags collision = _body.Move(target - transform.position);
            if ((collision & (CollisionFlags.Sides | CollisionFlags.Above)) != 0) { Cancel(); return true; }
            if (t >= 1f)
            {
                IsClimbing = false; _recoverUntil = Time.time + .12f;
                if (_stance != null) _stance.TrySetCrouched(true);
            }
            return true;
        }
        public bool TryBegin()
        {
            if (!isActiveAndEnabled || _definition == null || _body == null || !_body.enabled || !_body.isGrounded ||
                IsClimbing || _stance == null || (_stance.IsLowered || _stance.IsProneTransitioning) || (_actions != null && !_actions.CanStartTacticalAction) ||
                (_traversal != null && (_traversal.IsSliding || _traversal.IsVaulting))) return false;
            float height = _stance.StandingHeight * Mathf.Abs(transform.lossyScale.y);
            float radius = _body.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            Vector3 feet = transform.TransformPoint(_body.center) - Vector3.up * (_body.height * Mathf.Abs(transform.lossyScale.y) * .5f);
            if (!Ray(feet + Vector3.up * (height * .3f), transform.forward, _definition.Reach, out var wall) ||
                Vector3.Dot(wall.normal, -transform.forward) < .5f) return false;
            float maximum = height * _definition.MaximumHeightFraction;
            Vector3 probe = wall.point + transform.forward * .15f; probe.y = feet.y + maximum + .1f;
            if (!Ray(probe, Vector3.down, maximum + .1f, out var top) || Vector3.Dot(top.normal, Vector3.up) < .8f) return false;
            float rise = top.point.y - feet.y;
            if (rise <= height * _definition.MinimumHeightFraction || rise > maximum) return false;
            Vector3 landingProbe = top.point + transform.forward * (radius + .25f) + Vector3.up * .2f;
            if (!Ray(landingProbe, Vector3.down, .4f, out var landing) || Vector3.Dot(landing.normal, Vector3.up) < .8f) return false;
            Vector3 end = landing.point + Vector3.up * (_body.height * Mathf.Abs(transform.lossyScale.y) * .5f + _body.skinWidth) - transform.TransformVector(_body.center);
            if (!HasSpace(end, radius)) return false;
            _start = transform.position; _end = end; _lift = Mathf.Max(0f, end.y - _start.y) + .05f;
            _elapsed = 0f; IsClimbing = true; return true;
        }
        private bool HasSpace(Vector3 position, float radius)
        {
            Vector3 center = position + transform.TransformVector(_body.center);
            Vector3 half = Vector3.up * Mathf.Max(0f, _body.height * Mathf.Abs(transform.lossyScale.y) * .5f - radius);
            foreach (var hit in Physics.OverlapCapsule(center - half, center + half, Mathf.Max(.01f, radius - _body.skinWidth), ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform)) return false;
            return true;
        }
        private bool Ray(Vector3 origin, Vector3 direction, float distance, out RaycastHit nearest)
        {
            nearest = default; float closest = float.PositiveInfinity;
            foreach (var hit in Physics.RaycastAll(origin, direction, distance, ~0, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(transform) && hit.distance < closest) { nearest = hit; closest = hit.distance; }
            return closest < float.PositiveInfinity;
        }
        public void Cancel() { IsClimbing = false; _elapsed = _recoverUntil = 0f; }
        private void OnDisable() => Cancel();
    }
}
