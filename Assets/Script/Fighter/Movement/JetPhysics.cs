using UnityEngine;

namespace Battlefield.Fighter.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(JetMovement))]
    public class JetPhysics : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float _gravityThresholdSpeed = 20f;

        [Header("Flight Mode")]
        [SerializeField] private float _flightDrag = 0f;
        [SerializeField] private float _flightMass = 1000f;

        [Header("Falling Mode")]
        [SerializeField] private float _fallingMass = 3000f;
        [SerializeField] private float _fallingDrag = 0f;

        private Rigidbody _rigidbody;
        private JetMovement _movement;

        private bool _isFalling;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _movement = GetComponent<JetMovement>();
        }

        private void FixedUpdate()
        {
            bool shouldFall = _movement.CurrentSpeed < _gravityThresholdSpeed;

            if (shouldFall == _isFalling) return;

            _isFalling = shouldFall;

            if(_isFalling) // 멤버? 지역? 멀로 판단하는게 좋지?
            {
                ApplyFallingMode();
            }
            else
            {
                ApplyFlightMode();
            }

        }

        private void ApplyFallingMode()
        {
            _rigidbody.useGravity = true;
            _rigidbody.mass = _fallingMass;
            _rigidbody.linearDamping = _flightDrag;
        }

        private void ApplyFlightMode()
        {
            _rigidbody.useGravity = false;
            _rigidbody.mass = _fallingMass;
            _rigidbody.linearDamping = _flightDrag;
        }  
    }
}