using UnityEngine;
using Battlefield.Features.Projectile;
using Battlefield.Features.VFX;

namespace Battlefield.Features.Weapon
{
    public class MachineGun : WeaponBase
    {
        [Header("Projectile")]
        [SerializeField] private Transform _firePoint;

        [Header("Effect")]
        [SerializeField] private MuzzleFlash _muzzleFlash;

        protected override void Fire()
        {
            if (_firePoint == null)
            {
                Debug.LogWarning("Fire Point Missing.", this);
                return;
            }

            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }

            Bullet bullet = BulletPool.Instance.Get();
            bullet.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);

            ProjectileData projectileData = new ProjectileData(transform.root, Damage);
            bullet.Initialize(projectileData);
        }
    }
}