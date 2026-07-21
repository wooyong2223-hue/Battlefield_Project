using UnityEngine;
using Battlefield.Projectile;

namespace Battlefield.Weapon
{
    public class MachineGun : WeaponBase
    {
        [Header("Projectile")]
        [SerializeField] private Transform _firePoint;
        [SerializeField] private Bullet _bulletPrefab;

        protected override void Fire()
        {
            if (_firePoint == null)
            {
                Debug.LogWarning("Fire Point Missing.", this);
                return;
            }

            if (_bulletPrefab == null)
            {
                Debug.LogWarning("Bullet Prefab Missing.", this);
                return;
            }

            // Bullet 오브젝트 생성
            var bullet = Instantiate(
                _bulletPrefab,
                _firePoint.position,
                _firePoint.rotation);

            // bullet.transform.SetParent(null, true);
            bullet.Initialize(transform.root, Damage);
        }
    }
}