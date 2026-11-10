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
        [SerializeField] private Mesh _projectileMesh;
        [SerializeField, Min(.01f)] private float _tracerLengthMultiplier = 1f;

        [Header("Effect")]
        [SerializeField] private MuzzleFlash _muzzleFlash;

        private bool _missingFireSetupReported;

        public Transform FirePoint => _firePoint;
        public Bullet ProjectilePrefab => _bulletPool != null
            ? _bulletPool.ProjectilePrefab
            : null;

        protected override bool Fire()
        {
            if (_firePoint == null || _bulletPool == null)
            {
                if (!_missingFireSetupReported)
                {
                    Debug.LogWarning(
                        "Machine gun requires a fire point and bullet pool.",
                        this);
                    _missingFireSetupReported = true;
                }

                return false;
            }

            _missingFireSetupReported = false;

            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }

            Bullet bullet = _bulletPool.Get();
            if (bullet.TryGetComponent<BulletVisual>(out var visual))
                visual.SetMesh(_projectileMesh);
            bullet.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);

            ProjectileData projectileData = new ProjectileData(transform.root, Damage);
            bullet.InitializeWithTracer(projectileData, _tracerLengthMultiplier);
            return true;
        }
    }
}
