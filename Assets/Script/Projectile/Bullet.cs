using Battlefield.Core;
using UnityEngine;

namespace Battlefield.Projectile
{
    public class Bullet : ProjectileBase
    {
        private float _damage;

        public void Initialize(Transform owner, float damage)
        {
            base.Initialize(owner);
            _damage = damage;
        }

        protected override void OnHit(Collider other)
        {
            if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
            }
        }
    }
}