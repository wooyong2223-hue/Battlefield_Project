using Battlefield.Features.Targeting;
using UnityEngine;

namespace Battlefield.Features.Projectile
{
    public sealed class HomingMissile : ProjectileBase
    {
        [SerializeField] private float _turnSpeed = 120f;

        private AirTarget _target;
        private IMissileWarningReceiver _warningReceiver;
        private float _damage;

        protected override void FixedUpdate()
        {
            if (_target != null && _target.IsAvailable)
            {
                RotateToward(_target.AimPosition);
            }

            SetVelocity(Rigidbody.rotation * Vector3.forward * Speed);
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

        public override void Initialize(ProjectileData data)
        {
            _damage = Mathf.Max(0f, data.Damage);
            base.Initialize(data);
            SetVelocity(transform.forward * Speed);
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
            Battlefield.Framework.Core.IDamageable damageable =
                other.GetComponentInParent<Battlefield.Framework.Core.IDamageable>();
            damageable?.TakeDamage(_damage);
        }

        private void ResetGuidance()
        {
            _target = null;
            _damage = 0f;
        }

        private void ClearWarningReceiver()
        {
            _warningReceiver?.ClearThreatState(this);
            _warningReceiver = null;
        }
    }
}
