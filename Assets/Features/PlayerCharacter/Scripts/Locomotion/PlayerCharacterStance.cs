using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public enum CharacterStance { Standing, Crouched, Prone }

    [DefaultExecutionOrder(-60)]
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class PlayerCharacterStance : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _crouchHeight = 2.5f;
        [SerializeField, Min(0f)] private float _crouchMoveSpeed = 4f;
        [SerializeField] private LayerMask _standingObstacleMask = ~0;
        [SerializeField, Min(.01f)] private float _proneHeight = 1.1f;
        [SerializeField, Min(0f)] private float _proneMoveSpeed = 3f;
        [SerializeField, Min(0f)] private float _proneEyeForward = 1.3f;
        [SerializeField] private PlayerCharacterActionPolicy _actions;

        [SerializeField, Min(0f)] private float _proneEnterDuration = .55f;
        [SerializeField, Min(0f)] private float _proneExitDuration = .45f;
        private float _proneTransitionStartedAt, _proneTransitionDuration;
        public float ProneEnterDuration => _proneEnterDuration;
        public float ProneExitDuration => _proneExitDuration;
        public bool IsProneTransitioning => isActiveAndEnabled && _proneTransitionDuration > 0f &&
            Time.time < _proneTransitionStartedAt + _proneTransitionDuration;
        public float ProneTransitionProgress => _proneTransitionDuration > 0f
            ? Mathf.Clamp01((Time.time - _proneTransitionStartedAt) / _proneTransitionDuration) : 1f;

        private readonly Collider[] _clearanceHits = new Collider[32];
        private CharacterController _body;
        private IPlayerCharacterStanceInput _input;
        private IPlayerCharacterTraversalInput _traversalInput;
        private readonly RaycastHit[] _movementHits = new RaycastHit[32];
        private float _standingHeight;
        private Vector3 _standingCenter;

        public CharacterStance Current { get; private set; }
        public uint ChangeVersion { get; private set; }
        // Query standing clearance without changing the capsule or stance.
        public bool CanStand => isActiveAndEnabled && _body != null && _body.enabled &&
            (Current == CharacterStance.Standing || HasClearance(_standingHeight));
        public bool IsCrouched => Current == CharacterStance.Crouched;
        public bool IsProne => Current == CharacterStance.Prone;
        public bool IsLowered => Current != CharacterStance.Standing;
        public float MoveSpeed => IsProne ? _proneMoveSpeed : _crouchMoveSpeed;
        public float EyeForwardOffset => IsProne ? _proneEyeForward : 0f;
        public float StandingHeight => _standingHeight;
        public Vector3 StandingCenter => _standingCenter;
        public float HeightReduction => _body != null ? _standingHeight - _body.height : 0f;

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _input = GetComponent<IPlayerCharacterStanceInput>();
            _traversalInput = GetComponent<IPlayerCharacterTraversalInput>();
            _standingHeight = _body.height;
            _standingCenter = _body.center;
        }

        private void Update()
        {
            if (_actions != null && !_actions.CanChangeStance) return;
            if (_traversalInput != null && _traversalInput.PronePressed && (IsLowered || _body.isGrounded))
            {
                TrySetStance(IsProne ? CharacterStance.Standing : CharacterStance.Prone);
                return;
            }
            if (_input != null && _input.CrouchPressed && (IsCrouched || _body.isGrounded))
                TrySetCrouched(!IsCrouched);
        }

        public bool TrySetCrouched(bool crouched) =>
            TrySetStance(crouched ? CharacterStance.Crouched : CharacterStance.Standing);

        public bool TrySetStance(CharacterStance stance)
        {
            if (!isActiveAndEnabled || _body == null || !_body.enabled) return false;
            if (stance < CharacterStance.Standing || stance > CharacterStance.Prone) return false;
            if (stance == Current) return true;
            float height = stance == CharacterStance.Standing ? _standingHeight :
                Mathf.Clamp(stance == CharacterStance.Prone ? _proneHeight : _crouchHeight, _body.radius * 2f, _standingHeight);
            if (height > _body.height && !HasClearance(height)) return false;
            if (stance == CharacterStance.Prone && !ProneSpaceIsClear(transform.rotation)) return false;
            // The authored actor/model origins stay put; only the capsule's top moves.
            _body.height = height;
            _body.center = _standingCenter - Vector3.up * ((_standingHeight - height) * .5f);
            if (Current == CharacterStance.Prone || stance == CharacterStance.Prone)
            {
                _proneTransitionStartedAt = Time.time;
                _proneTransitionDuration = stance == CharacterStance.Prone ? _proneEnterDuration : _proneExitDuration;
            }
            Current = stance;
            ChangeVersion++;
            return true;
        }

        private bool HasClearance(float requestedHeight)
        {
            Vector3 scale = transform.lossyScale;
            float radius = _body.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float height = Mathf.Max(radius * 2f, requestedHeight * Mathf.Abs(scale.y));
            Vector3 center = transform.TransformPoint(_standingCenter - Vector3.up * ((_standingHeight - requestedHeight) * .5f));
            Vector3 halfSegment = transform.up * (height * .5f - radius);
            float skin = _body.skinWidth * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            Physics.SyncTransforms();
            int count = Physics.OverlapCapsuleNonAlloc(center - halfSegment, center + halfSegment,
                Mathf.Max(.01f, radius - skin), _clearanceHits, _standingObstacleMask, QueryTriggerInteraction.Ignore);
            if (count == _clearanceHits.Length) return false;
            for (int i = 0; i < count; i++)
            {
                Collider hit = _clearanceHits[i];
                if (hit.transform != transform && !hit.transform.IsChildOf(transform)) return false;
            }
            return true;
        }

        private void ProneCapsule(Quaternion rotation, out Vector3 a, out Vector3 b, out float radius)
        {
            Vector3 scale = transform.lossyScale;
            radius = _body.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float skin = _body.skinWidth * Mathf.Abs(scale.y);
            Vector3 center = transform.TransformPoint(_standingCenter - Vector3.up * ((_standingHeight - Mathf.Max(_proneHeight, _body.radius * 2f)) * .5f));
            Vector3 half = rotation * Vector3.forward * Mathf.Max(0f, _standingHeight * Mathf.Abs(scale.y) * .5f - radius);
            a = center - half; b = center + half;
            radius = Mathf.Max(.01f, radius - skin);
        }

        private bool ProneSpaceIsClear(Quaternion rotation)
        {
            ProneCapsule(rotation, out Vector3 a, out Vector3 b, out float radius);
            Physics.SyncTransforms();
            int count = Physics.OverlapCapsuleNonAlloc(a, b, radius, _clearanceHits, _standingObstacleMask, QueryTriggerInteraction.Ignore);
            if (count == _clearanceHits.Length) return false;
            for (int i = 0; i < count; i++)
                if (!_clearanceHits[i].transform.IsChildOf(transform)) return false;
            return true;
        }

        public bool AllowsHeading(Quaternion rotation) => !IsProne || ProneSpaceIsClear(rotation);

        public Vector3 ConstrainPlanarMovement(Vector3 displacement)
        {
            if (!IsProne || displacement.sqrMagnitude < .000001f) return displacement;
            ProneCapsule(transform.rotation, out Vector3 a, out Vector3 b, out float radius);
            float distance = displacement.magnitude;
            Vector3 direction = displacement / distance;
            float skin = _body.skinWidth * Mathf.Abs(transform.lossyScale.y);
            int count = Physics.CapsuleCastNonAlloc(a, b, radius, direction, _movementHits,
                distance + skin, _standingObstacleMask, QueryTriggerInteraction.Ignore);
            if (count == _movementHits.Length) return Vector3.zero;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _movementHits[i];
                if (hit.transform.IsChildOf(transform) || Vector3.Dot(hit.normal, direction) >= -.001f) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - skin));
            }
            return direction * distance;
        }

        private void OnDisable()
        {
            _proneTransitionDuration = 0f;
            if (_body == null) return;
            _body.height = _standingHeight;
            _body.center = _standingCenter;
            Current = CharacterStance.Standing;
        }
    }
}
