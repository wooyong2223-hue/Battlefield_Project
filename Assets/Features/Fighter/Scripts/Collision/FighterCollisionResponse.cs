using UnityEngine;

namespace Battlefield.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    public class FighterCollisionResponse : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _maxAngularSpeed = 0f;

        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            LimitAngularVelocity();
        }

        private void OnCollisionStay(Collision collision)
        {
            LimitAngularVelocity();
        }

        private void LimitAngularVelocity()
        {
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(
                _rigidbody.angularVelocity,
                _maxAngularSpeed);
        }
    }
}
