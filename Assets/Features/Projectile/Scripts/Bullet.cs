using UnityEngine;
using Battlefield.Framework.Core;

namespace Battlefield.Features.Projectile
{
    public class Bullet : ProjectileBase
    {
        [SerializeField] private BulletTracer _tracer;
        private float _damage;
        private BulletVisual _visual;

        protected override void Awake()
        {
            base.Awake();
            _visual = GetComponent<BulletVisual>();
        }

        public override void Initialize(ProjectileData data) => InitializeWithTracer(data, 1f);

        public void InitializeWithTracer(ProjectileData data, float lengthMultiplier)
        {
            base.Initialize(data);
            _damage = data.Damage;
            if (_tracer != null)
                _tracer.Begin(transform.position, _visual != null ? _visual.Diameter : .02f, lengthMultiplier);
        }

        protected override void OnAdvanced(Vector3 position)
        {
            if (_tracer != null) _tracer.Advance(position);
        }

        private void OnDisable()
        {
            if (_tracer != null) _tracer.Stop();
        }

        private void OnDestroy()
        {
            if (_tracer != null && !_tracer.transform.IsChildOf(transform))
                Destroy(_tracer.gameObject);
        }

        protected override void OnHit(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
            }
        }
    }
}
