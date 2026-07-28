using UnityEngine;
using Battlefield.Fighter.Controller;

namespace Battlefield.Fighter.Movement
{
    public class JetRotation : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _pitchSpeed = 1f;
        [SerializeField] private float _rollSpeed = 1f;
        [SerializeField] private float _yawSpeed = 1f;

        private IJetInput _input;

        private void Awake()
        {
            _input = GetComponent<IJetInput>();
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void Update()
        {
            if (_input == null) return;

            Vector3 rotation = new Vector3(
                _input.Pitch * _pitchSpeed,
                _input.Yaw * _yawSpeed,
                _input.Roll * _rollSpeed
            ) * _rotationSpeed * Time.deltaTime;

            transform.Rotate(rotation, Space.Self);
        }
    }
}
