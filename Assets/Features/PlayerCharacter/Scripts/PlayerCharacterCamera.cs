using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCharacterCamera : MonoBehaviour, IPlayerCharacterAimSource
    {
        [SerializeField] private Transform _target;
        [SerializeField] private PlayerCharacterView _view;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterLean _lean;
        [SerializeField] private Vector3 _offset = new(0.8f, 0.25f, -5f);
        [SerializeField] private Vector3 _lookOffset = new(0f, 1.6f, 0f);
        [SerializeField] private Vector3 _firstPersonOffset = new(0f, 1.6f, 0f);
        [SerializeField] private Vector3 _aimOffset = new(0.65f, 0.05f, -3f);
        [SerializeField, Min(0.01f)] private float _followSpeed = 12f;
        [SerializeField, Min(1f)] private float _normalFieldOfView = 60f;
        [SerializeField, Min(1f)] private float _aimFieldOfView = 50f;
        [SerializeField, Min(1f)] private float _adsFieldOfView = 40f;
        [SerializeField, Min(0.01f)] private float _collisionRadius = 0.2f;
        [SerializeField, Min(0f)] private float _collisionPadding = 0.05f;
        [SerializeField] private LayerMask _collisionMask = ~0;

        [Header("Locomotion presentation")]
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField] private PlayerCharacterMotionProfile _motionProfile;
        private PlayerCharacterMotionState _motion;
        private readonly PlayerCharacterJumpPoseState _jumpPose = new();
        public Quaternion ResolvedAimRotation => _view.AimRotation;
        public Vector3 JumpFollowOffset => _jumpPose.Offset;

        public Vector3 FirstPersonWeaponOffset => _motion != null ? _motion.WeaponOffset : Vector3.zero;
        public Vector3 FirstPersonWeaponEuler => _motion != null ? _motion.WeaponEuler : Vector3.zero;
        public float FirstPersonAimBlend => _motion != null ? _motion.AimBlend : _view.IsAds ? 1f : 0f;
        private Vector3 LocomotionOffset => _motion != null
            ? _target.rotation * (_motion.CameraOffset - _motion.LandingCameraOffset) : Vector3.zero;
        private Vector3 VisualEyePosition => EyePosition + LocomotionOffset + JumpFollowOffset;

        private readonly RaycastHit[] _collisionHits = new RaycastHit[64];
        private Camera _camera;
        private float _currentDistance = -1f;
        private bool _wasFirstPerson;
        private float _stanceHeightOffset;
        private float _stanceForwardOffset;

        private uint _stanceVersion;
        private bool _proneStanceTransition;
        private float _transitionStartHeight, _transitionStartForward;
        public float DistanceFromCharacter => _view.IsFirstPersonView ? 0f : _currentDistance;
        public Vector3 UnleanedEyePosition => _target.TransformPoint(GetEyeOffset(_firstPersonOffset));
        public Vector3 EyePosition => UnleanedEyePosition + LeanOffset;
        private Vector3 LeanOffset => _lean != null ? _lean.WorldOffset : Vector3.zero;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_motionProfile != null || _movement != null)
            {
                if (_motionProfile == null || _movement == null)
                {
                    Debug.LogError("Player locomotion presentation requires both movement and a motion profile.", this);
                    enabled = false;
                    return;
                }
                _motion = new PlayerCharacterMotionState(_motionProfile);
            }
            if (_target == null || _view == null)
            {
                Debug.LogError("Player character camera references are missing.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _currentDistance = -1f;
            _motion?.Reset();
            _jumpPose.Reset();
            _stanceHeightOffset = _stance != null ? _stance.HeightReduction : 0f;
            _stanceForwardOffset = _stance != null ? _stance.EyeForwardOffset : 0f;
            _stanceVersion = _stance != null ? _stance.ChangeVersion : 0;
            _proneStanceTransition = false;
            if (_camera != null) _camera.fieldOfView = _normalFieldOfView;
        }

        private void Start() => _camera.fieldOfView = _normalFieldOfView;

        private void LateUpdate()
        {
            bool firstPerson = _view.IsFirstPersonView;
            if (firstPerson != _wasFirstPerson) _currentDistance = -1f;
            _wasFirstPerson = firstPerson;
            float ratio = 1f - Mathf.Exp(-_followSpeed * Time.deltaTime);
            float heightOffset = _stance != null ? _stance.HeightReduction : 0f;
            if (_stance != null && _stanceVersion != _stance.ChangeVersion)
            {
                _stanceVersion = _stance.ChangeVersion;
                _transitionStartHeight = _stanceHeightOffset; _transitionStartForward = _stanceForwardOffset;
                _proneStanceTransition = _view.ProneCamera != null && _stance.IsProneTransitioning;
            }
            float forwardOffset = _stance != null ? _stance.EyeForwardOffset : 0f;
            float stanceRatio = _proneStanceTransition ? Mathf.SmoothStep(0f, 1f, _stance.ProneTransitionProgress) : ratio;
            _stanceHeightOffset = Mathf.Lerp(_proneStanceTransition ? _transitionStartHeight : _stanceHeightOffset, heightOffset, stanceRatio);
            _stanceForwardOffset = Mathf.Lerp(_proneStanceTransition ? _transitionStartForward : _stanceForwardOffset, forwardOffset, stanceRatio);
            if (_stance == null || !_stance.IsProneTransitioning) _proneStanceTransition = false;
            if (_motion != null)
            {
                bool active = _movement.isActiveAndEnabled && (_actions == null || _actions.CanAttack) &&
                    (_stance == null || !_stance.IsProne) &&
                    (_traversal == null || (!_traversal.IsSliding && !_traversal.IsVaulting));
                Vector3 localVelocity = _target.InverseTransformDirection(_movement.Velocity);
                _motion.Tick(localVelocity, _movement.IsGrounded, _movement.IsSprinting, _view.IsAds,
                    _view.AimRotation, _movement.MoveSpeed, active, Time.deltaTime);
            }
            bool followActive = _movement != null && _movement.isActiveAndEnabled &&
                (_actions == null || _actions.CanAttack);
            _jumpPose.Tick(_target.position, firstPerson, followActive, Time.deltaTime);
            Quaternion rotation = _view.LookRotation;
            Vector3 position;
            if (firstPerson)
            {
                position = VisualEyePosition;
                if (_motion != null) rotation *= Quaternion.Euler(0f, 0f, _motion.CameraRoll);
            }
            else
            {
                Vector3 pivot = GetThirdPersonPivot();
                Vector3 offset = rotation * GetThirdPersonOffset();
                float safeDistance = GetSafeDistance(pivot, offset.normalized, offset.magnitude);
                // Contract immediately at an obstacle; ease only the return away from it.
                _currentDistance = _currentDistance < 0f || safeDistance < _currentDistance
                    ? safeDistance : Mathf.Lerp(_currentDistance, safeDistance, ratio);
                position = pivot + offset.normalized * _currentDistance;
            }
            transform.SetPositionAndRotation(position, rotation);
            float fieldOfView = _view.IsAds ? _adsFieldOfView
                : _view.IsAiming ? _aimFieldOfView : _normalFieldOfView;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, fieldOfView, ratio);
        }

        public Ray GetAimRay()
        {
            Quaternion rotation = ResolvedAimRotation;
            if (_view.IsFirstPersonView)
                return new Ray(VisualEyePosition, rotation * Vector3.forward);

            // Free look changes the camera view, never the weapon's direction.
            Vector3 pivot = GetThirdPersonPivot();
            Vector3 offset = rotation * GetThirdPersonOffset();
            float distance = GetSafeDistance(pivot, offset.normalized, offset.magnitude);
            if (_currentDistance >= 0f && !_wasFirstPerson) distance = Mathf.Min(distance, _currentDistance);
            return new Ray(pivot + offset.normalized * distance, rotation * Vector3.forward);
        }

        private Vector3 GetThirdPersonPivot()
        {
            Vector3 local = GetEyeOffset(_lookOffset);
            if (_view.ProneCamera != null && _stance != null)
            {
                Vector3 feet = _stance.StandingCenter - Vector3.up * (_stance.StandingHeight * .5f);
                local = Vector3.Lerp(local, feet + _view.ProneCamera.PivotFromFeet, _view.ProneBlend);
            }
            return _target.TransformPoint(local) + LeanOffset + JumpFollowOffset;
        }
        private Vector3 GetThirdPersonOffset()
        {
            Vector3 authored = _view.IsAiming ? _aimOffset : _offset;
            if (_view.ProneCamera == null) return authored;
            return Vector3.Lerp(authored, _view.IsAiming ? _view.ProneCamera.AimOffset : _view.ProneCamera.ThirdPersonOffset, _view.ProneBlend);
        }

        private Vector3 GetEyeOffset(Vector3 authoredOffset) => authoredOffset - Vector3.up * _stanceHeightOffset + Vector3.forward * _stanceForwardOffset;

        private float GetSafeDistance(Vector3 origin, Vector3 direction, float distance)
        {
            int count = UnityEngine.Physics.SphereCastNonAlloc(
                origin, _collisionRadius, direction, _collisionHits, distance,
                _collisionMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = _collisionHits[i];
                if (hit.collider.transform.IsChildOf(_target)) continue;
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - _collisionPadding));
            }
            return distance;
        }

        private void OnDisable()
        {
            _motion?.Reset();
            _jumpPose.Reset();
            if (_camera != null) _camera.fieldOfView = _normalFieldOfView;
        }
    }
}
