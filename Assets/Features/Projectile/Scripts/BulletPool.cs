using UnityEngine;
using Battlefield.Framework.Pool;
using Battlefield.Features.VFX;

namespace Battlefield.Features.Projectile
{
    public class BulletPool : ObjectPool<Bullet>
    {
        [SerializeField] private Bullet _prefab;
        [SerializeField] private HitEffectManager _hitEffectPlayer;

        public Bullet ProjectilePrefab => _prefab;

        protected override Bullet Create()
        {
            Bullet bullet = Instantiate(_prefab, transform);
            bullet.SetHitEffectPlayer(_hitEffectPlayer);
            return Register(bullet);
        }

    }
}
