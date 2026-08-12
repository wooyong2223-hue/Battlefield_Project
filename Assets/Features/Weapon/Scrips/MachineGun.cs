using UnityEngine;
using Battlefield.Features.Projectile;
using Battlefield.Features.VFX;

namespace Battlefield.Features.Weapon
{
    public class MachineGun : WeaponBase
    {
        [Header("Projectile")]
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private Transform _firePoint;

        [Header("Effect")]
        [SerializeField] private MuzzleFlash _muzzleFlash;

        public Transform FirePoint => _firePoint;
        public Bullet ProjectilePrefab => _bulletPool != null
            ? _bulletPool.ProjectilePrefab
            : null;

        protected override bool Fire()
        {
            if (_firePoint == null || _bulletPool == null)
            {
                Debug.LogWarning("Fire Point Missing.", this);
                return false;
            }

            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }

            Bullet bullet = _bulletPool.Get();
            bullet.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);

            ProjectileData projectileData = new ProjectileData(transform.root, Damage);
            bullet.Initialize(projectileData);
            return true;
        }
    }
}
