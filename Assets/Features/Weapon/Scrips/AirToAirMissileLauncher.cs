using Battlefield.Features.Projectile;
using Battlefield.Features.Targeting;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public sealed class AirToAirMissileLauncher : WeaponBase
    {
        [SerializeField] private HomingMissilePool _missilePool;
        [SerializeField] private AirTargetLock _targetLock;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private int _maximumAmmo = 2;
        [SerializeField] private int _reserveAmmo;

        private int _currentAmmo;

        public override string AmmoText => $"{_currentAmmo} / {_reserveAmmo}";

        private void Awake()
        {
            _maximumAmmo = Mathf.Max(0, _maximumAmmo);
            _reserveAmmo = Mathf.Max(0, _reserveAmmo);
            _currentAmmo = _maximumAmmo;
        }

        protected override bool Fire()
        {
            if (_currentAmmo <= 0 || _missilePool == null || _targetLock == null || !_targetLock.HasLock || _firePoint == null) return false;
            HomingMissile missile = _missilePool.Get();
            missile.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);
            missile.Initialize(transform.root, _targetLock.CurrentTarget, Damage);
            _currentAmmo--;
            return true;
        }
    }
}
