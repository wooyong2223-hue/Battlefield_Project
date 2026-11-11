using System;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] private string _displayName;
        [Header("Fire")]
        [SerializeField] private float _fireRate = 10f;
        [SerializeField] private float _damage = 10f;

        [Header("OverHeat")]
        [SerializeField] private Overheat _overheat;

        [SerializeField]
        private float _coolDownDelay = 0.15f;

        private float _nextFireTime;
        private float _lastFireTime;
        public event Action Fired;
        public float ShotInterval => 1f / Mathf.Max(0.01f, _fireRate);
        public float Heat => _overheat.Heat;
        public float MaxHeat => _overheat.MaxHeat;
        public float RecoverHeat => _overheat.RecoverHeat;
        public bool IsOverheated => _overheat.IsOverheated;
        protected float Damage => _damage;
        protected Overheat Overheat => _overheat;
        public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
            ? GetType().Name
            : _displayName;
        public virtual string AmmoText => "∞";

        private void Update()
        {
            if (Time.time >= _lastFireTime + _coolDownDelay)
            {
                _overheat.CoolDown(Time.deltaTime);
            }
        }

        public void TryFire()
        {
            if (!_overheat.CanFire()) return;
            if (Time.time < _nextFireTime) return;

            if (!Fire()) return;

            _nextFireTime = Time.time + 1f / _fireRate;
            _lastFireTime = Time.time;
            _overheat.AddHeat();
            Fired?.Invoke();
        }

        protected abstract bool Fire();
    }
}
