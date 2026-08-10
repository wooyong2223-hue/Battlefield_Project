using System;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [Serializable]
    public sealed class JetDestructionView
    {
        [SerializeField] private float _trackingSpeed = 3f;

        private Transform _target;

        public bool IsActive { get; private set; }

        public void Begin(Transform target)
        {
            if (target == null) return;

            _target = target;
            IsActive = true;
        }

        public void Track(Transform cameraTransform, float deltaTime)
        {
            if (_target == null) return;

            Vector3 direction = _target.position - cameraTransform.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float trackingRatio = 1f - Mathf.Exp(
                -_trackingSpeed * deltaTime);

            cameraTransform.rotation = Quaternion.Slerp(
                cameraTransform.rotation,
                targetRotation,
                trackingRatio);
        }
    }
}
