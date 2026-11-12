using UnityEngine;

namespace Battlefield.Features.Fighter
{
    public sealed class FighterLandingGear : MonoBehaviour
    {
        private const float RollingAssistTorque = 0.01f;
        [SerializeField] private WheelCollider[] _wheels;

        private int _groundLayer;
        private bool _rollingAssistActive;

        private void Awake()
        {
            _groundLayer = LayerMask.NameToLayer("Ground");
        }

        public bool TryGetGroundNormal(out Vector3 normal)
        {
            normal = Vector3.zero;
            if (_wheels == null) return false;

            foreach (WheelCollider wheel in _wheels)
            {
                if (wheel != null &&
                    wheel.GetGroundHit(out WheelHit hit) &&
                    hit.collider != null &&
                    hit.collider.gameObject.layer == _groundLayer)
                {
                    normal = hit.normal;
                    return true;
                }
            }

            return false;
        }

        internal void SetRollingAssist(bool active)
        {
            if (_rollingAssistActive == active || _wheels == null) return;
            _rollingAssistActive = active;

            foreach (WheelCollider wheel in _wheels)
            {
                if (wheel != null)
                    wheel.motorTorque = active ? RollingAssistTorque : 0f;
            }
        }

        private void OnDisable()
        {
            SetRollingAssist(false);
        }
    }
}
