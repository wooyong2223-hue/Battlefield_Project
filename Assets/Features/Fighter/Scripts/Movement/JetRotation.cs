using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(JetMovement))]
    public class JetRotation : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _pitchSpeed = 1f;
        [SerializeField] private float _rollSpeed = 1f;
        [SerializeField] private float _yawSpeed = 1f;
        [SerializeField, Min(0f)] private float _minimumControlSpeed = 20f;
        [SerializeField, Min(0f)] private float _fullControlSpeed = 60f;

        private IJetInput _input;
        private JetMovement _movement;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _input = GetComponent<IJetInput>();
            _movement = GetComponent<JetMovement>();
            _rigidbody = GetComponent<Rigidbody>();
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void FixedUpdate()
        {
            if (_input == null || _movement == null || _rigidbody == null) return;

            float controlMultiplier = Mathf.InverseLerp(
                _minimumControlSpeed,
                _fullControlSpeed,
                _movement.EffectiveForwardSpeed);

            Vector3 rotation = new Vector3(
                _input.Pitch * _pitchSpeed,
                _input.Yaw * _yawSpeed,
                _input.Roll * _rollSpeed
            ) * _rotationSpeed * controlMultiplier * Time.fixedDeltaTime;

            Quaternion targetRotation =
                _rigidbody.rotation * Quaternion.Euler(rotation);

            _rigidbody.MoveRotation(targetRotation);
        }
    }
}
