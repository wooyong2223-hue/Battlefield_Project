using UnityEngine;

namespace Battlefield.Projectile
{
    public struct ProjectileData
    {
        public Transform Owner;
        public float Damage;

        public ProjectileData(Transform owner, float damage)
        {
            Owner = owner;
            Damage = damage;
        }
    }
}