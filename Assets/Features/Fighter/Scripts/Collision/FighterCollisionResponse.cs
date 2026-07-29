using System.Collections.Generic;
using UnityEngine;
using Battlefield.Core;
using Battlefield.Fighter.Movement;

namespace Battlefield.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(JetMovement))]
    public class FighterCollisionResponse : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField, Min(0f)] private float _maxAngularSpeed = 0f;

        [Header("Impact")]
        [SerializeField] private float _minimumDamageSpeedRatio = 0.3f;
        [SerializeField] private float _fullDamageSpeedRatio = 0.8f;
        [SerializeField] private float _wallTangentialDamageMultiplier = 0.7f;

        [Header("Debug")]
        [SerializeField] private bool _logCollisionDebug = true;
        [SerializeField, Min(0.1f)] private float _collisionLogInterval = 0.5f;
        [SerializeField] private float _wallNormalUpDot = 0.5f;

        private Rigidbody _rigidbody;
        private Health _health;
        private JetMovement _movement;
        private readonly HashSet<Collider> _wallColliders = new();
        private float _nextCollisionLogTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _movement = GetComponent<JetMovement>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            float currentSpeedBefore = _movement.CurrentSpeed;
            float impactSpeed = GetImpactSpeed(collision);
            bool isWall = UpdateWallContact(collision);

            ApplyImpact(impactSpeed);
            LimitAngularVelocity();
            LogCollision(
                "Enter",
                collision,
                isWall,
                impactSpeed,
                currentSpeedBefore);
        }

        private void OnCollisionStay(Collision collision)
        {
            bool isWall = UpdateWallContact(collision);
            LimitAngularVelocity();

            if (Time.time >= _nextCollisionLogTime)
            {
                LogCollision(
                    "Stay",
                    collision,
                    isWall,
                    GetImpactSpeed(collision),
                    _movement.CurrentSpeed);
                _nextCollisionLogTime = Time.time + _collisionLogInterval;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            bool wasWall = _wallColliders.Remove(collision.collider);

            LogCollision(
                "Exit",
                collision,
                wasWall,
                0f,
                _movement.CurrentSpeed);
        }

        private void OnDisable()
        {
            _wallColliders.Clear();
            _nextCollisionLogTime = 0f;
        }

        private void ApplyImpact(float impactSpeed)
        {
            float speedRatio = impactSpeed / Mathf.Max(
                _movement.MaxSpeed,
                Mathf.Epsilon);
            float fullDamageSpeed = Mathf.Max(
                _movement.MaxSpeed * _fullDamageSpeedRatio,
                Mathf.Epsilon);
            float damageRatio = Mathf.Clamp01(
                impactSpeed / fullDamageSpeed);

            if (speedRatio < _minimumDamageSpeedRatio) return;
            if (damageRatio <= 0f) return;

            _movement.StopImmediately();
            _health.TakeDamage(_health.MaxHealth * damageRatio);
        }

        private bool UpdateWallContact(Collision collision)
        {
            bool isWall = false;

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                float normalUpDot = Mathf.Abs(Vector3.Dot(
                    contact.normal,
                    Vector3.up));

                if (normalUpDot <= _wallNormalUpDot)
                {
                    isWall = true;
                    break;
                }
            }

            if (isWall)
            {
                _wallColliders.Add(collision.collider);
            }
            else
            {
                _wallColliders.Remove(collision.collider);
            }

            return isWall;
        }

        private void LogCollision(
            string state,
            Collision collision,
            bool isWall,
            float impactSpeed,
            float currentSpeedBefore)
        {
            if (!_logCollisionDebug) return;

            Debug.Log(
                $"[Fighter Collision] state={state}, " +
                $"target={collision.collider.name}, " +
                $"wall={isWall}, " +
                $"wallContact={_wallColliders.Count > 0}, " +
                $"speedBefore={currentSpeedBefore:0.##}, " +
                $"currentSpeed={_movement.CurrentSpeed:0.##}, " +
                $"rigidbodySpeed={_rigidbody.linearVelocity.magnitude:0.##}, " +
                $"impactSpeed={impactSpeed:0.##}",
                this);
        }

        private float GetImpactSpeed(Collision collision)
        {
            float impactSpeed = 0f;
            float relativeSpeed = collision.relativeVelocity.magnitude;

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                float normalSpeed = Mathf.Abs(Vector3.Dot(
                    collision.relativeVelocity,
                    contact.normal));
                float contactImpactSpeed = normalSpeed;
                float normalUpDot = Mathf.Abs(Vector3.Dot(
                    contact.normal,
                    Vector3.up));

                if (normalUpDot <= _wallNormalUpDot)
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

                impactSpeed = Mathf.Max(impactSpeed, contactImpactSpeed);
            }

            return impactSpeed;
        }

        private void LimitAngularVelocity()
        {
            _rigidbody.angularVelocity = Vector3.ClampMagnitude(
                _rigidbody.angularVelocity,
                _maxAngularSpeed);
        }
    }
}
