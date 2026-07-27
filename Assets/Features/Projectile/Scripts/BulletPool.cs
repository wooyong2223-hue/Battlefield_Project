using UnityEngine;
using Battlefield.Pool;

namespace Battlefield.Projectile
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