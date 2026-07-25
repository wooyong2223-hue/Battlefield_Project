using UnityEngine;
using Battlefield.Fighter.Controller;

namespace Battlefield.Fighter.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class JetMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _acceleration = 100f;
        [SerializeField] private float _maxSpeed = 300f;
        [SerializeField] private float _minSpeed = 0f;

        private Rigidbody _rigidbody;
        private KeyboardJetInput _input;

        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<KeyboardJetInput>();
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void Update()
        {
            if (_input == null) return;

            CurrentSpeed += _input.Throttle * _acceleration * Time.deltaTime;
            CurrentSpeed = Mathf.Clamp(CurrentSpeed, _minSpeed, _maxSpeed);
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = transform.forward * CurrentSpeed;
        }
    }
}