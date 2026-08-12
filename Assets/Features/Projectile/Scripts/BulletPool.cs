using UnityEngine;
using Battlefield.Framework.Pool;

namespace Battlefield.Features.Projectile
{
    public class BulletPool : ObjectPool<Bullet>
    {
        [SerializeField] private Bullet _prefab;

        public Bullet ProjectilePrefab => _prefab;

        protected override Bullet Create()
        {
            Bullet bullet = Instantiate(_prefab, transform);
            return Register(bullet);
        }

    }
}
