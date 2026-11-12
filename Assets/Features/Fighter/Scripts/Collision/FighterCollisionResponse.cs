using UnityEngine;
using UnityEngine.Serialization;
using Battlefield.Framework.Core;
using Battlefield.Framework.Physics;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(FighterCollisionConstraint))]
    [RequireComponent(typeof(FighterCollisionDamage))]
    public class FighterCollisionResponse : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField, Min(0f)] private float _maxAngularSpeed = 0f;

        [Header("Surface")]
        [FormerlySerializedAs("_wallNormalUpDot")]
        [SerializeField] private float _groundNormalUpDot = 0.5f;

        private Rigidbody _rigidbody;
        private Health _health;
        private FighterCollisionConstraint _collisionConstraint;
        private FighterCollisionDamage _collisionDamage;
        private int _groundLayer;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _collisionConstraint = GetComponent<FighterCollisionConstraint>();
            _collisionDamage = GetComponent<FighterCollisionDamage>();
            _groundLayer = LayerMask.NameToLayer("Ground");
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_health.IsDead) return;

            CollisionSurfaceType surfaceType = ClassifyCollisionSurface(collision);
            _collisionDamage.Apply(collision, surfaceType);
            if (!_health.IsDead)
                RegisterContact(collision);
            else if (surfaceType == CollisionSurfaceType.Obstacle)
                LimitAngularVelocity();
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!_health.IsDead)
                RegisterContact(collision);
        }

        private void RegisterContact(Collision collision)
        {
            _collisionConstraint.RegisterContact(
                collision.collider,
                CalculateContactNormal(collision));
        }


        private void OnCollisionExit(Collision collision)
        {
            _collisionConstraint.RemoveContact(collision.collider);
        }

        private static Vector3 CalculateContactNormal(Collision collision)
        {
            Vector3 contactNormal = Vector3.zero;

            for (int i = 0; i < collision.contactCount; i++)
            {
                contactNormal += collision.GetContact(i).normal;
            }

            return contactNormal.sqrMagnitude > Mathf.Epsilon
                ? contactNormal.normalized
                : Vector3.zero;
        }

        private CollisionSurfaceType ClassifyCollisionSurface(
            Collision collision)
        {
            if (_groundLayer < 0 ||
                collision.gameObject.layer != _groundLayer)
            {
                return CollisionSurfaceType.Obstacle;
            }

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                float normalUpDot = Vector3.Dot(
                    contact.normal,
                    Vector3.up);

                if (normalUpDot >= _groundNormalUpDot)
                {
                    return CollisionSurfaceType.Ground;
                }
            }

            return CollisionSurfaceType.Obstacle;
        }

        private void LimitAngularVelocity()
        {
            float maximumAngularSpeed = Mathf.Max(0f, _maxAngularSpeed);
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(
                _rigidbody.angularVelocity,
                maximumAngularSpeed);
        }

    }
}
