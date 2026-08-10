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

        [Header("High Speed Control")]
        [SerializeField, Min(0f)]
        private float _maximumSpeedControlMultiplier = 0.65f;

        [Header("Turn Energy Loss")]
        [SerializeField, Min(0f)] private float _turnEnergyLossPerSecond = 45f;
        [SerializeField, Min(0f)] private float _pitchEnergyLossMultiplier = 1f;
        [SerializeField, Min(0f)] private float _rollEnergyLossMultiplier = 0.15f;
        [SerializeField, Min(0f)] private float _yawEnergyLossMultiplier = 0.35f;

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

            float controlMultiplier = CalculateControlMultiplier();

            Vector3 rotation = new Vector3(
                _input.Pitch * _pitchSpeed,
                _input.Yaw * _yawSpeed,
                _input.Roll * _rollSpeed
            ) * _rotationSpeed * controlMultiplier * Time.fixedDeltaTime;

            Quaternion targetRotation =
                _rigidbody.rotation * Quaternion.Euler(rotation);

            _rigidbody.MoveRotation(targetRotation);
            ApplyTurnEnergyLoss(controlMultiplier);
        }

        private float CalculateControlMultiplier()
        {
            float speed = _movement.EffectiveForwardSpeed;
            if (speed <= _fullControlSpeed)
            {
                return Mathf.InverseLerp(
                    _minimumControlSpeed,
                    _fullControlSpeed,
                    speed);
            }

            float highSpeedRatio = Mathf.InverseLerp(
                _fullControlSpeed,
                Mathf.Max(_fullControlSpeed, _movement.MaxSpeed),
                speed);
            float maximumSpeedMultiplier = Mathf.Clamp01(
                _maximumSpeedControlMultiplier);

            return Mathf.Lerp(
                1f,
                maximumSpeedMultiplier,
                highSpeedRatio);
        }

        private void ApplyTurnEnergyLoss(float controlMultiplier)
        {
            float pitchInput = Mathf.Clamp01(Mathf.Abs(_input.Pitch));
            float rollInput = Mathf.Clamp01(Mathf.Abs(_input.Roll));
            float yawInput = Mathf.Clamp01(Mathf.Abs(_input.Yaw));
            float weightedTurnInput =
                pitchInput * _pitchEnergyLossMultiplier +
                rollInput * _rollEnergyLossMultiplier +
                yawInput * _yawEnergyLossMultiplier;

            float speedLoss =
                weightedTurnInput *
                controlMultiplier *
                _turnEnergyLossPerSecond *
                Time.fixedDeltaTime;

            _movement.ApplyTurnEnergyLoss(speedLoss);
        }
    }
}
