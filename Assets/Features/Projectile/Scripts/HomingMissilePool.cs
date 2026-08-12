using Battlefield.Framework.Pool;
using UnityEngine;

namespace Battlefield.Features.Projectile
{
    public sealed class HomingMissilePool : ObjectPool<HomingMissile>
    {
        [SerializeField] private HomingMissile _prefab;

        protected override HomingMissile Create()
        {
            HomingMissile missile = Instantiate(_prefab, transform);
            return Register(missile);
        }

    }
}
