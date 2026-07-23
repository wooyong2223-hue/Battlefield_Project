using Battlefield.UI.HUD;
using UnityEngine;

namespace Battlefield.Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [Header("Fire")]
        [SerializeField] private float _fireRate = 10f;
        [SerializeField] private float _damage = 10f;

        [Header("OverHeat")]
        [SerializeField] private Overheat _overheat;

        [SerializeField]
        private float _coolDownDelay = 0.15f;

        [Header("UI")]
        [SerializeField] private HeatDotsView _heatDotsView;

        private float _nextFireTime;
        private float _lastFireTime;
        protected float Damage => _damage;
        protected Overheat Overheat => _overheat;

        private void Start()
        {
            if(_heatDotsView != null)
            {
                _heatDotsView.Bind(_overheat);
            }
        }

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

            _nextFireTime = Time.time + 1f / _fireRate;
            _lastFireTime = Time.time;

            Fire();

            _overheat.AddHeat();
        }

        protected abstract void Fire();
    }
}