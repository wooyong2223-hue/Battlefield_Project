using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Camera))]
    public class JetCamera : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyboardJetInput _input;

        [Header("Camera Point")]
        [SerializeField] private Transform _firstPersonCameraPoint;
        [SerializeField] private Transform _thirdPersonCameraPoint;
        [SerializeField] private Transform _rearViewCameraPoint;

        [Header("Camera Features")]
        [SerializeField] private JetCameraOrbit _orbit = new();
        [SerializeField] private JetCameraLens _lens = new();
        [SerializeField] private JetDestructionView _destructionView = new();

        [Header("Diagnostics")]
        [SerializeField] private bool _enableJitterDiagnostics;

        private readonly JetCameraJitterDiagnostics _jitterDiagnostics = new();
        private readonly JetCameraPositionJitterDiagnostics
            _positionJitterDiagnostics = new();
        private Rigidbody _fighterRigidbody;
        private bool _isFirstPerson;

        public bool IsFirstPersonView =>
            _input != null &&
            _isFirstPerson &&
            !_input.RearView &&
            !_destructionView.IsActive;

        private void Awake()
        {
            if (_input == null) Debug.Log("KeyboardJetInput is missing", this);
            if (_firstPersonCameraPoint == null) Debug.Log("_firstPersonCameraPoint is missing", this);
            if (_thirdPersonCameraPoint == null) Debug.Log("_thirdPersonCameraPoint is missing", this);
            if (_rearViewCameraPoint == null) Debug.Log("_rearViewCameraPoint is missing", this);

            if (_input == null || _thirdPersonCameraPoint == null) return;

            _fighterRigidbody = _input.GetComponent<Rigidbody>();
            _input.SetFreeLookAllowed(!_isFirstPerson);
            _orbit.Initialize(_input.transform, _thirdPersonCameraPoint);
            _lens.Initialize(GetComponent<Camera>(), _isFirstPerson);
        }

        private void Update()
        {
            if (_input == null) return;
            if (_input.ChangeCamera)
            {
                _isFirstPerson = !_isFirstPerson;
                _input.SetFreeLookAllowed(!_isFirstPerson);
            }
        }

        private void LateUpdate()
        {
            if (_destructionView.IsActive)
            {
                _destructionView.Track(transform, Time.deltaTime);
                return;
            }

            if (_input == null) return;

            Transform viewPoint = GetCurrentViewPoint();
            bool isRearView = viewPoint == _rearViewCameraPoint;

            _orbit.UpdatePose(
                transform,
                viewPoint,
                _input.transform,
                _input,
                _isFirstPerson,
                isRearView,
                Time.deltaTime);
            UpdateJitterDiagnostics(isRearView);
            _lens.UpdateFieldOfView(
                _input.Zoom,
                _isFirstPerson,
                Time.deltaTime);
        }

        private void UpdateJitterDiagnostics(bool isRearView)
        {
            if (!_enableJitterDiagnostics || _isFirstPerson || isRearView)
            {
                _jitterDiagnostics.Reset();
                _positionJitterDiagnostics.Reset();
                return;
            }

            _jitterDiagnostics.Sample(
                _input.transform.rotation,
                _fighterRigidbody != null
                    ? _fighterRigidbody.rotation
                    : _input.transform.rotation,
                _orbit.TargetOrbitRotation,
                _orbit.SmoothedOrbitRotation,
                _orbit.AppliedOrbitRotation,
                _orbit.AppliedRoll,
                transform.rotation,
                Time.deltaTime,
                this);
            _positionJitterDiagnostics.Sample(
                _input.transform.position,
                _fighterRigidbody != null
                    ? _fighterRigidbody.position
                    : _input.transform.position,
                transform.position,
                _orbit.ThirdPersonDistance,
                _orbit.AppliedCameraOffset,
                Time.deltaTime,
                this);
        }

        public void BeginDestructionView(Transform target)
        {
            _destructionView.Begin(target);
        }

        private Transform GetCurrentViewPoint()
        {
            if (_input.RearView) return _rearViewCameraPoint;

            return _isFirstPerson
                ? _firstPersonCameraPoint
                : _thirdPersonCameraPoint;
        }
    }

    internal sealed class JetCameraPositionJitterDiagnostics
    {
        private const float ReportInterval = 1f;

        private Vector3 _previousFighterPosition;
        private Vector3 _previousCameraPosition;
        private Vector3 _previousFighterStep;
        private Vector3 _previousCameraStep;
        private Vector3 _previousCameraOffset;
        private float _previousDistance;
        private float _elapsedTime;
        private float _fighterStepTotal;
        private float _fighterStepMaximum;
        private float _fighterStepChangeTotal;
        private float _fighterStepChangeMaximum;
        private float _cameraStepTotal;
        private float _cameraStepMaximum;
        private float _cameraStepChangeTotal;
        private float _cameraStepChangeMaximum;
        private float _distanceChangeTotal;
        private float _distanceChangeMaximum;
        private float _cameraOffsetChangeTotal;
        private float _cameraOffsetChangeMaximum;
        private float _transformRigidbodyErrorMaximum;
        private int _sampleCount;
        private bool _isInitialized;

        public void Sample(
            Vector3 fighterPosition,
            Vector3 rigidbodyPosition,
            Vector3 cameraPosition,
            float thirdPersonDistance,
            Vector3 cameraOffset,
            float deltaTime,
            Object context)
        {
            if (!_isInitialized)
            {
                _previousFighterPosition = fighterPosition;
                _previousCameraPosition = cameraPosition;
                _previousFighterStep = Vector3.zero;
                _previousCameraStep = Vector3.zero;
                _previousDistance = thirdPersonDistance;
                _previousCameraOffset = cameraOffset;
                _isInitialized = true;
                return;
            }

            Vector3 fighterStep = fighterPosition
                - _previousFighterPosition;
            Vector3 cameraStep = cameraPosition
                - _previousCameraPosition;
            Accumulate(
                fighterStep.magnitude,
                ref _fighterStepTotal,
                ref _fighterStepMaximum);
            Accumulate(
                (fighterStep - _previousFighterStep).magnitude,
                ref _fighterStepChangeTotal,
                ref _fighterStepChangeMaximum);
            Accumulate(
                cameraStep.magnitude,
                ref _cameraStepTotal,
                ref _cameraStepMaximum);
            Accumulate(
                (cameraStep - _previousCameraStep).magnitude,
                ref _cameraStepChangeTotal,
                ref _cameraStepChangeMaximum);
            Accumulate(
                Mathf.Abs(thirdPersonDistance - _previousDistance),
                ref _distanceChangeTotal,
                ref _distanceChangeMaximum);
            Accumulate(
                (cameraOffset - _previousCameraOffset).magnitude,
                ref _cameraOffsetChangeTotal,
                ref _cameraOffsetChangeMaximum);

            _transformRigidbodyErrorMaximum = Mathf.Max(
                _transformRigidbodyErrorMaximum,
                Vector3.Distance(fighterPosition, rigidbodyPosition));
            _previousFighterPosition = fighterPosition;
            _previousCameraPosition = cameraPosition;
            _previousFighterStep = fighterStep;
            _previousCameraStep = cameraStep;
            _previousDistance = thirdPersonDistance;
            _previousCameraOffset = cameraOffset;
            _sampleCount++;
            _elapsedTime += Mathf.Max(0f, deltaTime);

            if (_elapsedTime < ReportInterval) return;

            float divisor = Mathf.Max(1, _sampleCount);
            Debug.LogWarning(
                $"[CameraJitterPosition] frames={_sampleCount} " +
                $"FighterStep(avg/max)={_fighterStepTotal / divisor:F4}/{_fighterStepMaximum:F4} " +
                $"FighterChange={_fighterStepChangeTotal / divisor:F4}/{_fighterStepChangeMaximum:F4} " +
                $"CameraStep={_cameraStepTotal / divisor:F4}/{_cameraStepMaximum:F4} " +
                $"CameraChange={_cameraStepChangeTotal / divisor:F4}/{_cameraStepChangeMaximum:F4} " +
                $"DistanceChange={_distanceChangeTotal / divisor:F4}/{_distanceChangeMaximum:F4} " +
                $"OffsetChange={_cameraOffsetChangeTotal / divisor:F4}/{_cameraOffsetChangeMaximum:F4} " +
                $"Transform-Rigidbody(max)={_transformRigidbodyErrorMaximum:F4}",
                context);
            ResetInterval();
        }

        public void Reset()
        {
            _isInitialized = false;
            ResetInterval();
        }

        private static void Accumulate(
            float value,
            ref float total,
            ref float maximum)
        {
            total += value;
            maximum = Mathf.Max(maximum, value);
        }

        private void ResetInterval()
        {
            _elapsedTime = 0f;
            _fighterStepTotal = 0f;
            _fighterStepMaximum = 0f;
            _fighterStepChangeTotal = 0f;
            _fighterStepChangeMaximum = 0f;
            _cameraStepTotal = 0f;
            _cameraStepMaximum = 0f;
            _cameraStepChangeTotal = 0f;
            _cameraStepChangeMaximum = 0f;
            _distanceChangeTotal = 0f;
            _distanceChangeMaximum = 0f;
            _cameraOffsetChangeTotal = 0f;
            _cameraOffsetChangeMaximum = 0f;
            _transformRigidbodyErrorMaximum = 0f;
            _sampleCount = 0;
        }
    }

    internal sealed class JetCameraJitterDiagnostics
    {
        private const float ReportInterval = 1f;

        private Quaternion _previousFighterTransformRotation;
        private Quaternion _previousRigidbodyRotation;
        private Quaternion _previousTargetRotation;
        private Quaternion _previousSmoothedOrbitRotation;
        private Quaternion _previousAppliedRotation;
        private Quaternion _previousCameraRotation;
        private float _previousAppliedRoll;
        private float _elapsedTime;
        private float _fighterTransformStepTotal;
        private float _rigidbodyStepTotal;
        private float _targetStepTotal;
        private float _smoothedOrbitStepTotal;
        private float _appliedStepTotal;
        private float _cameraStepTotal;
        private float _rollStepTotal;
        private float _fighterTransformStepMaximum;
        private float _rigidbodyStepMaximum;
        private float _targetStepMaximum;
        private float _smoothedOrbitStepMaximum;
        private float _appliedStepMaximum;
        private float _cameraStepMaximum;
        private float _rollStepMaximum;
        private float _transformRigidbodyErrorMaximum;
        private int _sampleCount;
        private bool _isInitialized;

        public void Sample(
            Quaternion fighterTransformRotation,
            Quaternion rigidbodyRotation,
            Quaternion targetRotation,
            Quaternion smoothedOrbitRotation,
            Quaternion appliedRotation,
            float appliedRoll,
            Quaternion cameraRotation,
            float deltaTime,
            Object context)
        {
            if (!_isInitialized)
            {
                StorePreviousRotations(
                    fighterTransformRotation,
                    rigidbodyRotation,
                    targetRotation,
                    smoothedOrbitRotation,
                    appliedRotation,
                    appliedRoll,
                    cameraRotation);
                _isInitialized = true;
                return;
            }

            Accumulate(
                Quaternion.Angle(
                    _previousFighterTransformRotation,
                    fighterTransformRotation),
                ref _fighterTransformStepTotal,
                ref _fighterTransformStepMaximum);
            Accumulate(
                Quaternion.Angle(
                    _previousRigidbodyRotation,
                    rigidbodyRotation),
                ref _rigidbodyStepTotal,
                ref _rigidbodyStepMaximum);
            Accumulate(
                Quaternion.Angle(_previousTargetRotation, targetRotation),
                ref _targetStepTotal,
                ref _targetStepMaximum);
            Accumulate(
                Quaternion.Angle(
                    _previousSmoothedOrbitRotation,
                    smoothedOrbitRotation),
                ref _smoothedOrbitStepTotal,
                ref _smoothedOrbitStepMaximum);
            Accumulate(
                Quaternion.Angle(_previousAppliedRotation, appliedRotation),
                ref _appliedStepTotal,
                ref _appliedStepMaximum);
            Accumulate(
                Quaternion.Angle(_previousCameraRotation, cameraRotation),
                ref _cameraStepTotal,
                ref _cameraStepMaximum);
            Accumulate(
                Mathf.Abs(Mathf.DeltaAngle(
                    _previousAppliedRoll,
                    appliedRoll)),
                ref _rollStepTotal,
                ref _rollStepMaximum);

            _transformRigidbodyErrorMaximum = Mathf.Max(
                _transformRigidbodyErrorMaximum,
                Quaternion.Angle(
                    fighterTransformRotation,
                    rigidbodyRotation));
            _sampleCount++;
            _elapsedTime += Mathf.Max(0f, deltaTime);

            StorePreviousRotations(
                fighterTransformRotation,
                rigidbodyRotation,
                targetRotation,
                smoothedOrbitRotation,
                appliedRotation,
                appliedRoll,
                cameraRotation);

            if (_elapsedTime < ReportInterval) return;

            float divisor = Mathf.Max(1, _sampleCount);
            Debug.Log(
                $"[CameraJitter] frames={_sampleCount} " +
                $"Fighter(avg/max)={_fighterTransformStepTotal / divisor:F3}/{_fighterTransformStepMaximum:F3} " +
                $"Rigidbody={_rigidbodyStepTotal / divisor:F3}/{_rigidbodyStepMaximum:F3} " +
                $"Target={_targetStepTotal / divisor:F3}/{_targetStepMaximum:F3} " +
                $"Orbit={_smoothedOrbitStepTotal / divisor:F3}/{_smoothedOrbitStepMaximum:F3} " +
                $"Roll={_rollStepTotal / divisor:F3}/{_rollStepMaximum:F3} " +
                $"Applied={_appliedStepTotal / divisor:F3}/{_appliedStepMaximum:F3} " +
                $"Camera={_cameraStepTotal / divisor:F3}/{_cameraStepMaximum:F3} " +
                $"Transform-Rigidbody(max)={_transformRigidbodyErrorMaximum:F3}",
                context);
            ResetInterval();
        }

        public void Reset()
        {
            _isInitialized = false;
            ResetInterval();
        }

        private void StorePreviousRotations(
            Quaternion fighterTransformRotation,
            Quaternion rigidbodyRotation,
            Quaternion targetRotation,
            Quaternion smoothedOrbitRotation,
            Quaternion appliedRotation,
            float appliedRoll,
            Quaternion cameraRotation)
        {
            _previousFighterTransformRotation = fighterTransformRotation;
            _previousRigidbodyRotation = rigidbodyRotation;
            _previousTargetRotation = targetRotation;
            _previousSmoothedOrbitRotation = smoothedOrbitRotation;
            _previousAppliedRotation = appliedRotation;
            _previousCameraRotation = cameraRotation;
            _previousAppliedRoll = appliedRoll;
        }

        private static void Accumulate(
            float value,
            ref float total,
            ref float maximum)
        {
            total += value;
            maximum = Mathf.Max(maximum, value);
        }

        private void ResetInterval()
        {
            _elapsedTime = 0f;
            _fighterTransformStepTotal = 0f;
            _rigidbodyStepTotal = 0f;
            _targetStepTotal = 0f;
            _smoothedOrbitStepTotal = 0f;
            _appliedStepTotal = 0f;
            _cameraStepTotal = 0f;
            _rollStepTotal = 0f;
            _fighterTransformStepMaximum = 0f;
            _rigidbodyStepMaximum = 0f;
            _targetStepMaximum = 0f;
            _smoothedOrbitStepMaximum = 0f;
            _appliedStepMaximum = 0f;
            _cameraStepMaximum = 0f;
            _rollStepMaximum = 0f;
            _transformRigidbodyErrorMaximum = 0f;
            _sampleCount = 0;
        }
    }
}
