using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Battlefield.Core;
using Battlefield.Fighter.Movement;

namespace Battlefield.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(JetMovement))]
    public class FighterCollisionResponse : MonoBehaviour
    {
        private enum CollisionSurfaceType
        {
            Ground,
            Obstacle
        }

        private readonly struct CollisionImpact
        {
            public CollisionImpact(
                float relativeSpeed,
                float normalSpeed,
                float tangentialSpeed,
                float impactSpeed,
                Vector3 contactNormal)
            {
                RelativeSpeed = relativeSpeed;
                NormalSpeed = normalSpeed;
                TangentialSpeed = tangentialSpeed;
                ImpactSpeed = impactSpeed;
                ContactNormal = contactNormal;
            }

            public float RelativeSpeed { get; }
            public float NormalSpeed { get; }
            public float TangentialSpeed { get; }
            public float ImpactSpeed { get; }
            public Vector3 ContactNormal { get; }
        }

        [Header("Rotation")]
        [SerializeField, Min(0f)] private float _maxAngularSpeed = 0f;

        [Header("Impact")]
        [SerializeField] private float _minimumDamageSpeedRatio = 0.3f;
        [SerializeField] private float _instantDestructionSpeedRatio = 0.7f;
        [SerializeField]
        private float _maximumNonLethalDamageRatio = 0.9f;
        [SerializeField] private float _wallTangentialDamageMultiplier = 0.8f;
        [SerializeField] private float _minimumLandingUpDot = 0.7f;

        [Header("Debug")]
        [SerializeField] private bool _logCollisionDebug = true;
        [SerializeField, Min(0.1f)] private float _collisionLogInterval = 0.5f;
        [FormerlySerializedAs("_wallNormalUpDot")]
        [SerializeField] private float _groundNormalUpDot = 0.5f;

        private Rigidbody _rigidbody;
        private Health _health;
        private JetMovement _movement;
        private readonly HashSet<Collider> _obstacleColliders = new();
        private int _groundLayer;
        private float _nextCollisionLogTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _movement = GetComponent<JetMovement>();
            _groundLayer = LayerMask.NameToLayer("Ground");
        }

        private void OnCollisionEnter(Collision collision)
        {
            float currentSpeedBefore = _movement.CurrentSpeed;
            CollisionSurfaceType surfaceType =
                UpdateCollisionSurface(collision);
            CollisionImpact impact =
                CalculateImpact(collision, surfaceType);

            float damageImpactSpeed = CalculateDamageImpactSpeed(
                surfaceType,
                impact);
            // ApplyImpact(surfaceType, damageImpactSpeed);

            if (_health.CurrentHealth > 0f)
            {
                _movement.ApplyCollisionImpact(
                    collision.collider,
                    CalculateContactNormal(collision));
            }

            LimitAngularVelocity();
            LogCollision(
                "Enter",
                collision,
                surfaceType,
                impact,
                currentSpeedBefore);
        }

        private void OnCollisionStay(Collision collision)
        {
            CollisionSurfaceType surfaceType =
                UpdateCollisionSurface(collision);
            _movement.MaintainCollisionContact(
                collision.collider,
                CalculateContactNormal(collision));
            LimitAngularVelocity();

            if (Time.time >= _nextCollisionLogTime)
            {
                CollisionImpact impact =
                    CalculateImpact(collision, surfaceType);

                LogCollision(
                    "Stay",
                    collision,
                    surfaceType,
                    impact,
                    _movement.CurrentSpeed);
                _nextCollisionLogTime = Time.time + _collisionLogInterval;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            bool wasObstacle =
                _obstacleColliders.Remove(collision.collider);

            _movement.EndCollisionContact(collision.collider);

            LogCollision(
                "Exit",
                collision,
                wasObstacle
                    ? CollisionSurfaceType.Obstacle
                    : CollisionSurfaceType.Ground,
                default,
                _movement.CurrentSpeed);
        }

        private void OnDisable()
        {
            _obstacleColliders.Clear();
            _movement.ClearCollisionContacts();
            _nextCollisionLogTime = 0f;
        }

        private void ApplyImpact(
            CollisionSurfaceType surfaceType,
            float impactSpeed)
        {
            float speedRatio = impactSpeed / Mathf.Max(
                _movement.MaxSpeed,
                Mathf.Epsilon);

            if (speedRatio <= _minimumDamageSpeedRatio) return;

            if (speedRatio >= _instantDestructionSpeedRatio)
            {
                _movement.StopImmediately();
                _health.TakeDamage(_health.CurrentHealth);
                return;
            }

            float damageRatio = CalculateDamageRatio(speedRatio);
            if (damageRatio <= 0f) return;

            if (surfaceType == CollisionSurfaceType.Ground)
            {
                _movement.StopImmediately();
            }

            _health.TakeDamage(_health.MaxHealth * damageRatio);
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

        private float CalculateDamageRatio(float speedRatio)
        {
            float damageProgress = Mathf.InverseLerp(
                _minimumDamageSpeedRatio,
                _instantDestructionSpeedRatio,
                speedRatio);

            return damageProgress * _maximumNonLethalDamageRatio;
        }

        private float CalculateDamageImpactSpeed(
            CollisionSurfaceType surfaceType,
            CollisionImpact impact)
        {
            if (surfaceType != CollisionSurfaceType.Ground)
            {
                return impact.ImpactSpeed;
            }

            float landingUpDot = Vector3.Dot(
                transform.up,
                impact.ContactNormal);
            bool hasLandingAttitude =
                landingUpDot >= _minimumLandingUpDot;

            return hasLandingAttitude
                ? impact.NormalSpeed
                : impact.RelativeSpeed;
        }

        private CollisionSurfaceType UpdateCollisionSurface(
            Collision collision)
        {
            CollisionSurfaceType surfaceType =
                ClassifyCollisionSurface(collision);

            if (surfaceType == CollisionSurfaceType.Obstacle)
            {
                _obstacleColliders.Add(collision.collider);
            }
            else
            {
                _obstacleColliders.Remove(collision.collider);
            }

            return surfaceType;
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

        private void LogCollision(
            string state,
            Collision collision,
            CollisionSurfaceType surfaceType,
            CollisionImpact impact,
            float currentSpeedBefore)
        {
            if (!_logCollisionDebug) return;
        }

        private CollisionImpact CalculateImpact(
            Collision collision,
            CollisionSurfaceType surfaceType)
        {
            float impactSpeed = 0f;
            float relativeSpeed = collision.relativeVelocity.magnitude;
            float strongestNormalSpeed = 0f;
            float strongestTangentialSpeed = 0f;
            Vector3 strongestContactNormal = Vector3.zero;

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                float normalSpeed = Mathf.Abs(Vector3.Dot(
                    collision.relativeVelocity,
                    contact.normal));
                float contactImpactSpeed = normalSpeed;
                if (surfaceType == CollisionSurfaceType.Obstacle)
                {
                    float tangentialSpeed = Mathf.Sqrt(Mathf.Max(
                        relativeSpeed * relativeSpeed -
                        normalSpeed * normalSpeed,
                        0f));
                    float weightedTangentialSpeed =
                        tangentialSpeed * _wallTangentialDamageMultiplier;

                    contactImpactSpeed = Mathf.Sqrt(
                        normalSpeed * normalSpeed +
                        weightedTangentialSpeed * weightedTangentialSpeed);
                }

                if (contactImpactSpeed <= impactSpeed) continue;

                impactSpeed = contactImpactSpeed;
                strongestNormalSpeed = normalSpeed;
                strongestTangentialSpeed = Mathf.Sqrt(Mathf.Max(
                    relativeSpeed * relativeSpeed -
                    normalSpeed * normalSpeed,
                    0f));
                strongestContactNormal = contact.normal;
            }

            return new CollisionImpact(
                relativeSpeed,
                strongestNormalSpeed,
                strongestTangentialSpeed,
                impactSpeed,
                strongestContactNormal);
        }

        private void LimitAngularVelocity()
        {
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(
                _rigidbody.angularVelocity,
                _maxAngularSpeed);
        }
    }
}
