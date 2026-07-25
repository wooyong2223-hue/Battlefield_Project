using UnityEngine;
using Battlefield.Core;

namespace Battlefield.Projectile
{
    public class Bullet : ProjectileBase
    {
        private float _damage;

        public override void Initialize(ProjectileData data)
        {
            base.Initialize(data);
            _damage = data.Damage;
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