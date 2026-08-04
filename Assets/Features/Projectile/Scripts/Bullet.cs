using UnityEngine;
using Battlefield.Framework.Core;

namespace Battlefield.Features.Projectile
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
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
            }
        }
    }
}
