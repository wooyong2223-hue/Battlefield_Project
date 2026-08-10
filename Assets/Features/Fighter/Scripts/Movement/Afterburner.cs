using UnityEngine;

namespace Battlefield.Features.Fighter
{
    public class Afterburner : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _accelerationMultiplier = 1.3f;
        [SerializeField, Min(1f)] private float _maxSpeedMultiplier = 1.15f;
        [SerializeField, Min(0.01f)] private float _duration = 1.5f;
        [SerializeField, Min(0f)] private float _recoveryPerSecond = 0.25f;
        [SerializeField, Min(0f)] private float _recoveryDelay = 1f;

        private float _charge = 1f;
        private float _recoveryDelayRemaining;

        public float Charge => _charge;
        public bool IsActive { get; private set; }
        public float AccelerationMultiplier => IsActive
            ? _accelerationMultiplier
            : 1f;
        public float MaxSpeedMultiplier => IsActive
            ? _maxSpeedMultiplier
            : 1f;

        public void Tick(bool requested, float deltaTime)
        {
            bool hasCharge = _charge > Mathf.Epsilon;
            IsActive = requested && hasCharge;

            if (IsActive)
            {
                float duration = Mathf.Max(_duration, 0.01f);
                _charge = Mathf.Max(0f, _charge - deltaTime / duration);
                _recoveryDelayRemaining = _recoveryDelay;

                if (_charge <= Mathf.Epsilon)
                {
                    IsActive = false;
                }

                return;
            }

            if (_recoveryDelayRemaining > 0f)
            {
                _recoveryDelayRemaining = Mathf.Max(
                    0f,
                    _recoveryDelayRemaining - deltaTime);
                return;
            }

            _charge = Mathf.MoveTowards(
                _charge,
                1f,
                _recoveryPerSecond * deltaTime);
        }

        public void Stop()
        {
            IsActive = false;
        }

        private void OnDisable()
        {
            Stop();
        }

    }
}
