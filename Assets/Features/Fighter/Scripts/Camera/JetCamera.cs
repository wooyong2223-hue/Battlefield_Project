using Battlefield.Fighter.Controller;
using UnityEngine;

namespace Battlefield.Fighter.Camera
{
    public class JetCamera : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private KeyboardJetInput _input;

        [Header("Camera Point")]
        [SerializeField] private Transform _firstPersonCameraPoint;
        [SerializeField] private Transform _thirdPersonCameraPoint;
        [SerializeField] private Transform _rearViewCameraPoint;

        [Header("Destruction View")]
        [SerializeField, Min(0.01f)]
        private float _destructionTrackingSpeed = 3f;

        private bool _isFirstPerson;
        private Transform _destructionTarget;
        private bool _isDestructionView;

        private void Awake()
        {
            if (_input == null) Debug.Log("KeyboardJetInput is missing", this);
            if (_firstPersonCameraPoint == null) Debug.Log("_firstPersonCameraPoint is missing", this);
            if (_thirdPersonCameraPoint == null) Debug.Log("_thirdPersonCameraPoint is missing", this);
            if (_rearViewCameraPoint == null) Debug.Log("_rearViewCameraPoint is missing", this);
        }

        private void Update()
        {
            if (_input == null) return;
            if (_input.ChangeCamera) _isFirstPerson = !_isFirstPerson;
        }

        private void LateUpdate()
        {
            if (_isDestructionView)
            {
                TrackDestructionTarget();
                return;
            }

            if (_input == null) return;

            Transform viewPoint = GetCurrentViewPoint();
            transform.position = viewPoint.position;
            transform.rotation = viewPoint.rotation;
        }

        public void BeginDestructionView(Transform target)
        {
            if (target == null) return;

            _destructionTarget = target;
            _isDestructionView = true;
        }

        private void TrackDestructionTarget()
        {
            if (_destructionTarget == null) return;

            Vector3 direction = _destructionTarget.position - transform.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float trackingRatio = 1f - Mathf.Exp(
                -_destructionTrackingSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                trackingRatio);
        }

        private Transform GetCurrentViewPoint()
        {
            if (_input.RearView) return _rearViewCameraPoint;

            return _isFirstPerson ? _firstPersonCameraPoint : _thirdPersonCameraPoint;
        }
    }
}
