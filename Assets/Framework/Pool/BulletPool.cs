using UnityEngine;
using Battlefield.Projectile;

namespace Battlefield.Pool
{
    public class BulletPool : ObjectPool<Bullet>
    {
        public static BulletPool Instance { get; private set; }

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
    }
}