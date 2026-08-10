using System;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [Serializable]
    public sealed class JetCameraLens
    {
        [SerializeField] private float _firstPersonFieldOfView = 70f;
        [SerializeField] private float _thirdPersonFieldOfView = 80f;
        [SerializeField] private float _zoomFieldOfView = 35f;
        [SerializeField] private float _fieldOfViewChangeSpeed = 10f;

        private Camera _camera;

        public void Initialize(Camera targetCamera, bool isFirstPerson)
        {
            _camera = targetCamera;
            _camera.fieldOfView = GetTargetFieldOfView(
                false,
                isFirstPerson);
        }

        public void UpdateFieldOfView(
            bool isZooming,
            bool isFirstPerson,
            float deltaTime)
        {
            if (_camera == null) return;

            float targetFieldOfView = GetTargetFieldOfView(
                isZooming,
                isFirstPerson);
            float changeRatio = 1f - Mathf.Exp(
                -_fieldOfViewChangeSpeed * deltaTime);
            _camera.fieldOfView = Mathf.Lerp(
                _camera.fieldOfView,
                targetFieldOfView,
                changeRatio);
        }

        private float GetTargetFieldOfView(
            bool isZooming,
            bool isFirstPerson)
        {
            if (isZooming) return _zoomFieldOfView;

            return isFirstPerson
                ? _firstPersonFieldOfView
                : _thirdPersonFieldOfView;
        }
    }
}
