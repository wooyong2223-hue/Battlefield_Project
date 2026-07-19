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

        private bool _isFirstPerson;

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
            if (_input == null) return;

            Transform viewPoint = GetCurrentViewPoint();
            transform.position = viewPoint.position;
            transform.rotation = viewPoint.rotation;
        }

        private Transform GetCurrentViewPoint()
        {
            if (_input.RearView) return _rearViewCameraPoint;

            return _isFirstPerson ? _firstPersonCameraPoint : _thirdPersonCameraPoint;
        }
    }
}