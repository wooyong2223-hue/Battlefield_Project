using Battlefield.Features.Targeting;
using Battlefield.Features.VFX;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Projectile
{
    public sealed class HomingMissile : ProjectileBase
    {
        [SerializeField] private float _turnSpeed = 120f;
        [SerializeField] private float _maximumSpeed = 800f;
        [SerializeField] private float _acceleration = 250f;
        [SerializeField] private float _proximityFuseRadius = 8f;

        private AirTarget _target;
        private IMissileWarningReceiver _warningReceiver;
        private IExplosionEffectPlayer _explosionEffectPlayer;
        private float _damage;
        private float _currentSpeed;

        protected override void FixedUpdate()
        {
            if (_target != null && _target.IsAvailable)
            {
                RotateToward(_target.AimPosition);
            }

            _currentSpeed = Mathf.MoveTowards(
                _currentSpeed,
                Mathf.Max(Speed, _maximumSpeed),
                Mathf.Max(0f, _acceleration) * Time.fixedDeltaTime);
            SetVelocity(
                Rigidbody.rotation * Vector3.forward * _currentSpeed);

            if (TryDetonateProximityFuse())
            {
                return;
            }

            base.FixedUpdate();
        }

        public void Initialize(
            Transform owner,
            AirTarget target,
            float damage)
        {
            ClearWarningReceiver();
            _target = target;
            _warningReceiver = target != null
                ? target.GetComponent<IMissileWarningReceiver>()
                : null;
            Initialize(new ProjectileData(owner, damage));
            _warningReceiver?.ReportThreatState(
                this,
                MissileWarningState.Incoming);
        }

        public void SetExplosionEffectPlayer(
            IExplosionEffectPlayer explosionEffectPlayer)
        {
            _explosionEffectPlayer = explosionEffectPlayer;
        }

        public override void Initialize(ProjectileData data)
        {
            _damage = Mathf.Max(0f, data.Damage);
            base.Initialize(data);
            _currentSpeed = Mathf.Max(0f, Speed);
            SetVelocity(transform.forward * _currentSpeed);
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            ClearWarningReceiver();
            ResetGuidance();
        }

        public override void OnDespawn()
        {
            ClearWarningReceiver();
            base.OnDespawn();
            ResetGuidance();
        }

        private void RotateToward(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - Rigidbody.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            Quaternion rotation = Quaternion.RotateTowards(
                Rigidbody.rotation,
                Quaternion.LookRotation(direction),
                Mathf.Max(0f, _turnSpeed) * Time.fixedDeltaTime);
            Rigidbody.MoveRotation(rotation);
        }

        protected override void OnHit(Collider other)
        {
            IDamageable damageable =
                other.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage);
            PlayExplosion(other.ClosestPoint(Rigidbody.position));
        }

        private bool TryDetonateProximityFuse()
        {
            float radius = Mathf.Max(0f, _proximityFuseRadius);
            if (radius <= Mathf.Epsilon ||
                _target == null ||
                !_target.IsAvailable ||
                !IsHostileTarget())
            {
                return false;
            }

            Vector3 start = Rigidbody.position;
            Vector3 displacement = Velocity * Time.fixedDeltaTime;
            float displacementSqrMagnitude = displacement.sqrMagnitude;
            float progress = displacementSqrMagnitude <= Mathf.Epsilon
                ? 0f
                : Mathf.Clamp01(Vector3.Dot(
                    _target.AimPosition - start,
                    displacement) / displacementSqrMagnitude);
            Vector3 closestPoint = start + displacement * progress;
            if ((_target.AimPosition - closestPoint).sqrMagnitude >
                radius * radius)
            {
                return false;
            }

            IDamageable damageable =
                _target.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(_damage);
            PlayExplosion(closestPoint);
            DestroyProjectile();
            return true;
        }

        private void PlayExplosion(Vector3 position)
        {
            _explosionEffectPlayer?.Play(position);
        }

        private bool IsHostileTarget()
        {
            return OwnerTeam != TeamType.Neutral &&
                   _target.Team != TeamType.Neutral &&
                   _target.Team != OwnerTeam;
        }

        private void ResetGuidance()
        {
            _target = null;
            _damage = 0f;
            _currentSpeed = 0f;
        }

        private void ClearWarningReceiver()
        {
            _warningReceiver?.ClearThreatState(this);
            _warningReceiver = null;
        }
    }
}
