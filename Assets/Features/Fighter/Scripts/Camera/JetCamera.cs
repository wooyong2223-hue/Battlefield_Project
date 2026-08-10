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

        private bool _isFirstPerson;

        private void Awake()
        {
            if (_input == null) Debug.Log("KeyboardJetInput is missing", this);
            if (_firstPersonCameraPoint == null) Debug.Log("_firstPersonCameraPoint is missing", this);
            if (_thirdPersonCameraPoint == null) Debug.Log("_thirdPersonCameraPoint is missing", this);
            if (_rearViewCameraPoint == null) Debug.Log("_rearViewCameraPoint is missing", this);

            if (_input == null || _thirdPersonCameraPoint == null) return;

            _orbit.Initialize(_input.transform, _thirdPersonCameraPoint);
            _lens.Initialize(GetComponent<Camera>(), _isFirstPerson);
        }

        private void Update()
        {
            if (_input == null) return;
            if (_input.ChangeCamera) _isFirstPerson = !_isFirstPerson;
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
                isRearView);
            _lens.UpdateFieldOfView(
                _input.Zoom,
                _isFirstPerson,
                Time.deltaTime);
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
}
