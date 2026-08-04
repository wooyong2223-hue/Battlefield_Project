using UnityEngine;
using Battlefield.Framework.Core;
using Battlefield.Framework.Physics;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(JetMovement))]
    public class FighterCollisionDamage : MonoBehaviour
    {
        private readonly struct CollisionImpact
        {
            public CollisionImpact(
                float relativeSpeed,
                float normalSpeed,
                float impactSpeed,
                Vector3 contactNormal)
            {
                RelativeSpeed = relativeSpeed;
                NormalSpeed = normalSpeed;
                ImpactSpeed = impactSpeed;
                ContactNormal = contactNormal;
            }

            public float RelativeSpeed { get; }
            public float NormalSpeed { get; }
            public float ImpactSpeed { get; }
            public Vector3 ContactNormal { get; }
        }

        [SerializeField] private float _minimumDamageSpeedRatio = 0.3f;
        [SerializeField] private float _instantDestructionSpeedRatio = 0.7f;
        [SerializeField] private float _maximumNonLethalDamageRatio = 0.9f;
        [SerializeField] private float _wallTangentialDamageMultiplier = 0.8f;
        [SerializeField] private float _minimumLandingUpDot = 0.7f;

        private Health _health;
        private JetMovement _movement;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _movement = GetComponent<JetMovement>();
        }

        public void Apply(
            Collision collision,
            CollisionSurfaceType surfaceType)
        {
            CollisionImpact impact = CalculateImpact(
                collision,
                surfaceType);
            float impactSpeed = CalculateDamageImpactSpeed(
                surfaceType,
                impact);
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

        private CollisionImpact CalculateImpact(
            Collision collision,
            CollisionSurfaceType surfaceType)
        {
            float impactSpeed = 0f;
            float relativeSpeed = collision.relativeVelocity.magnitude;
            float strongestNormalSpeed = 0f;
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
                strongestContactNormal = contact.normal;
            }

            return new CollisionImpact(
                relativeSpeed,
                strongestNormalSpeed,
                impactSpeed,
                strongestContactNormal);
        }
    }
}
