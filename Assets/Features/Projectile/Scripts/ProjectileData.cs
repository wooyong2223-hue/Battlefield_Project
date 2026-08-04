using UnityEngine;

namespace Battlefield.Features.Projectile
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