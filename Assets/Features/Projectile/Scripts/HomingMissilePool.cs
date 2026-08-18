using Battlefield.Framework.Pool;
using Battlefield.Features.VFX;
using UnityEngine;

namespace Battlefield.Features.Projectile
{
    public sealed class HomingMissilePool : ObjectPool<HomingMissile>
    {
        [SerializeField] private HomingMissile _prefab;
        [SerializeField] private HitEffectManager _hitEffectPlayer;
        [SerializeField] private ExplosionEffectPool _explosionPool;

        protected override HomingMissile Create()
        {
            HomingMissile missile = Instantiate(_prefab, transform);
            missile.SetHitEffectPlayer(_hitEffectPlayer);
            missile.SetExplosionEffectPlayer(_explosionPool);
            return Register(missile);
        }

    }
}
