using UnityEngine;

namespace Battlefield.Features.Weapon
{
    [System.Serializable]
    public class Overheat
    {
        [SerializeField] private float _maxHeat = 100f;
        [SerializeField] private float _heatPerShot = 5f;
        [SerializeField] private float _coolDownPerSecond = 25f;
        [SerializeField] private float _recoverHeat = 30f;

        private float _currentHeat;
        private bool _isOverheated;

        public float Heat => _currentHeat;
        public bool IsOverheated => _isOverheated;
        public float MaxHeat => _maxHeat;
        public float RecoverHeat => _recoverHeat;

        public bool CanFire()
        {
            return !_isOverheated;
        }

        public void AddHeat()
        {
            _currentHeat += _heatPerShot;

            if (_currentHeat >= _maxHeat)
            {
                _currentHeat = _maxHeat;
                _isOverheated = true;
            }
        }

        public void CoolDown(float deltaTime)
        {
            if (_currentHeat <= 0f) return;

            _currentHeat -= _coolDownPerSecond * deltaTime;
            _currentHeat = Mathf.Max(_currentHeat, 0f);

            if (_isOverheated && _currentHeat <= _recoverHeat)
            {
                _isOverheated = false;
            }
        }
    }
}