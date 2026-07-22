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

        [Header("UI")]
        [SerializeField] private HeatDotsView _heatDotsView;

        private float _nextFireTime;
        private bool _isFiring;
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
            if(!_isFiring)
            {
                _overheat.CoolDown(Time.deltaTime);
            }

            _isFiring = false;
        }

        public void TryFire()
        {
            _isFiring = true;

            if (!_overheat.CanFire()) return;
            if (Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + 1f / _fireRate;

            Fire();

            _overheat.AddHeat();
        }

        protected abstract void Fire();
    }
}