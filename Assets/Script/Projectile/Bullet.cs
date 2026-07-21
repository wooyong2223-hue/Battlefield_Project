using Battlefield.Core;
using UnityEngine;

namespace Battlefield.Projectile
{
    public class Bullet : ProjectileBase
    {
        private float _damage;

        public virtual void Initialize(Transform owner, float damage)
        {
            base.Initialize(owner);
            _damage = damage;
        }

        protected override void OnHit(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_damage);
            }
        }
    }
}