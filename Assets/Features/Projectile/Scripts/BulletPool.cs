using UnityEngine;
using Battlefield.Pool;

namespace Battlefield.Projectile
{
    public class BulletPool : ObjectPool<Bullet>
    {
        public static BulletPool Instance { get; private set; }

        [SerializeField] private Bullet _prefab;

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            base.Awake();
        }

        protected override Bullet Create()
        {
            Bullet bullet = Instantiate(_prefab, transform);
            return Register(bullet);
        }
    }
}
