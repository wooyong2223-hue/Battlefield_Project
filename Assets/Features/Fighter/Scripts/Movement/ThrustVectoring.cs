using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(JetMovement))]
    [RequireComponent(typeof(Afterburner))]
    public sealed class ThrustVectoring : MonoBehaviour
    {
        [Header("Activation")]
        [SerializeField] private float _maximumActivationSpeed = 180f;
        [SerializeField] private float _brakeInputThreshold = 0.5f;

        [Header("Control")]
        [SerializeField] private float _minimumControlAuthority = 0.65f;
        [SerializeField] private float _pitchControlMultiplier = 2.5f;
        [SerializeField] private float _yawControlMultiplier = 1.75f;
        [SerializeField] private float _velocityAlignmentMultiplier = 0.1f;

        [Header("Energy Loss")]
        [SerializeField] private float _baseEnergyLossPerSecond = 60f;
        [SerializeField] private float _inputEnergyLossPerSecond = 80f;

        private IJetInput _input;
        private JetMovement _movement;
        private Afterburner _afterburner;

        public bool IsActive =>
            _input != null &&
            _movement != null &&
            _afterburner != null &&
            _afterburner.IsActive &&
            _input.Throttle <= -Mathf.Abs(_brakeInputThreshold) &&
            _movement.CurrentSpeed <= Mathf.Max(
                0f,
                _maximumActivationSpeed);

        public float VelocityAlignmentMultiplier => IsActive
            ? Mathf.Max(0f, _velocityAlignmentMultiplier)
            : 1f;
        private void Awake()
        {
            _input = GetComponent<IJetInput>();
            _movement = GetComponent<JetMovement>();
            _afterburner = GetComponent<Afterburner>();

            if (_input == null)
            {
                Debug.Log($"{nameof(IJetInput)} is missing", this);
            }
        }

        public float GetPitchControlMultiplier(
            float aerodynamicControlMultiplier)
        {
            return GetControlMultiplier(
                aerodynamicControlMultiplier,
                _pitchControlMultiplier);
        }

        public float GetYawControlMultiplier(
            float aerodynamicControlMultiplier)
        {
            return GetControlMultiplier(
                aerodynamicControlMultiplier,
                _yawControlMultiplier);
        }

        public float CalculateEnergyLoss(
            float pitchInput,
            float yawInput,
            float deltaTime)
        {
            if (!IsActive)
            {
                return 0f;
            }

            float controlInput = Mathf.Clamp01(
                Mathf.Max(
                    Mathf.Abs(pitchInput),
                    Mathf.Abs(yawInput)));
            float energyLossPerSecond =
                Mathf.Max(0f, _baseEnergyLossPerSecond) +
                controlInput * Mathf.Max(
                    0f,
                    _inputEnergyLossPerSecond);

            return energyLossPerSecond * Mathf.Max(0f, deltaTime);
        }

        private float GetControlMultiplier(
            float aerodynamicControlMultiplier,
            float thrustVectoringMultiplier)
        {
            if (!IsActive)
            {
                return aerodynamicControlMultiplier;
            }

            float controlAuthority = Mathf.Max(
                aerodynamicControlMultiplier,
                Mathf.Max(0f, _minimumControlAuthority));

            return controlAuthority * Mathf.Max(
                0f,
                thrustVectoringMultiplier);
        }
    }
}
