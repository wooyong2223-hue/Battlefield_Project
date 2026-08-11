using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(JetMovement))]
    public class JetPhysics : MonoBehaviour
    {
        [Header("Body")]
        [SerializeField] private float _mass = 1000f;
        [SerializeField] private float _linearDamping = 0f;

        private Rigidbody _rigidbody;
        private JetMovement _movement;

        private bool _isGravityActive;
        private bool _isInitialized;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _movement = GetComponent<JetMovement>();
            _rigidbody.mass = Mathf.Max(0.0001f, _mass);
            _rigidbody.linearDamping = Mathf.Max(0f, _linearDamping);
        }

        private void FixedUpdate()
        {
            bool shouldUseGravity = _movement.IsUnpowered;

            if (_isInitialized &&
                shouldUseGravity == _isGravityActive)
            {
                return;
            }

            _isInitialized = true;
            _isGravityActive = shouldUseGravity;
            _rigidbody.useGravity = shouldUseGravity;
        }
    }
}
