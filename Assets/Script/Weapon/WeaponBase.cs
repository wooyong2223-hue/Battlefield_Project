using UnityEngine;

namespace Battlefield.Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Fire")]
        [SerializeField] private float _fireRate = 10f;
        [SerializeField] private float _damage = 10f;

        private float _nextFireTime;
        protected float Damage => _damage;
        protected float FireRate => _fireRate;

        public void TryFire()
        {
            if (Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + 1f / _fireRate;

            Fire();
        }

        protected abstract void Fire();
    }
}