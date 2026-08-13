using System.Collections;
using Battlefield.Features.Projectile;
using Battlefield.Features.Targeting;
using Battlefield.Features.UI;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public sealed class AirToAirMissileLauncher :
        WeaponBase,
        IRefillProgressSource
    {
        [SerializeField] private HomingMissilePool _missilePool;
        [SerializeField] private AirTargetLock _targetLock;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private int _maximumAmmo = 2;
        [SerializeField] private int _reserveAmmo;
        [SerializeField] private float _reloadDuration = 5f;
        [SerializeField] private float _reserveRefillDuration = 5f;

        private int _currentAmmo;
        private int _maximumReserveAmmo;
        private WeaponController _weaponController;
        private Coroutine _reloadRoutine;
        private Coroutine _reserveRefillRoutine;
        private float _reloadProgress;
        private float _reserveRefillProgress;

        public override string AmmoText => $"{_currentAmmo} / {_reserveAmmo}";

        private void Awake()
        {
            _weaponController = GetComponent<WeaponController>();
            _maximumAmmo = Mathf.Max(0, _maximumAmmo);
            _reserveAmmo = Mathf.Max(0, _reserveAmmo);
            _reloadDuration = Mathf.Max(0f, _reloadDuration);
            _reserveRefillDuration = Mathf.Max(
                0f,
                _reserveRefillDuration);
            _currentAmmo = _maximumAmmo;
            _maximumReserveAmmo = _reserveAmmo;
        }

        private void OnEnable()
        {
            if (_weaponController != null)
            {
                _weaponController.SelectionChanged += HandleSelectionChanged;
                UpdateTargetSearching(_weaponController.SelectedWeapon);
            }

            EvaluateAmmoState();
        }

        private void OnDisable()
        {
            if (_weaponController != null)
            {
                _weaponController.SelectionChanged -= HandleSelectionChanged;
            }

            _targetLock?.SetSearchingEnabled(false);

            if (_reloadRoutine != null)
            {
                StopCoroutine(_reloadRoutine);
                _reloadRoutine = null;
            }

            _reloadProgress = 0f;

            if (_reserveRefillRoutine != null)
            {
                StopCoroutine(_reserveRefillRoutine);
                _reserveRefillRoutine = null;
            }

            _reserveRefillProgress = 0f;
        }

        protected override bool Fire()
        {
            if (_currentAmmo <= 0 || _missilePool == null || _targetLock == null || !_targetLock.HasLock || _firePoint == null) return false;
            HomingMissile missile = _missilePool.Get();
            missile.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);
            missile.Initialize(transform.root, _targetLock.CurrentTarget, Damage);
            _currentAmmo--;
            EvaluateAmmoState();
            return true;
        }

        private void EvaluateAmmoState()
        {
            TryStartReload();
            TryStartReserveRefill();
        }

        private void TryStartReload()
        {
            if (_reloadRoutine != null ||
                _currentAmmo >= _maximumAmmo ||
                _maximumAmmo <= 0 ||
                _reserveAmmo <= 0)
            {
                return;
            }

            _reloadRoutine = StartCoroutine(ReloadAfterDelay());
        }

        private IEnumerator ReloadAfterDelay()
        {
            yield return TrackProgress(
                _reloadDuration,
                progress => _reloadProgress = progress);

            int missingAmmo = _maximumAmmo - _currentAmmo;
            int reloadAmount = Mathf.Min(missingAmmo, _reserveAmmo);

            _currentAmmo += reloadAmount;
            _reserveAmmo -= reloadAmount;
            _reloadProgress = 0f;
            _reloadRoutine = null;
            EvaluateAmmoState();
        }

        private void TryStartReserveRefill()
        {
            if (_reserveRefillRoutine != null ||
                _reserveAmmo >= _maximumReserveAmmo ||
                _maximumReserveAmmo <= 0)
            {
                return;
            }

            _reserveRefillRoutine = StartCoroutine(
                RefillReserveAfterDelay());
        }

        private IEnumerator RefillReserveAfterDelay()
        {
            yield return TrackProgress(
                _reserveRefillDuration,
                progress => _reserveRefillProgress = progress);

            _reserveAmmo = Mathf.Min(
                _reserveAmmo + 1,
                _maximumReserveAmmo);
            _reserveRefillProgress = 0f;
            _reserveRefillRoutine = null;
            EvaluateAmmoState();
        }

        public bool TryGetRefillProgress(
            RefillProgressType progressType,
            out float progress)
        {
            bool isReloading = _reloadRoutine != null;
            bool isRefillingReserve = _reserveRefillRoutine != null;

            switch (progressType)
            {
                case RefillProgressType.Reload:
                    progress = _reloadProgress;
                    return isReloading;
                case RefillProgressType.Reserve:
                    progress = _reserveRefillProgress;
                    return isRefillingReserve;
                default:
                    if (isReloading)
                    {
                        progress = _reloadProgress;
                        return true;
                    }

                    progress = _reserveRefillProgress;
                    return isRefillingReserve;
            }
        }

        private static IEnumerator TrackProgress(
            float duration,
            System.Action<float> setProgress)
        {
            if (duration <= Mathf.Epsilon)
            {
                setProgress(1f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                setProgress(Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
        }

        private void HandleSelectionChanged(
            int selectedIndex,
            WeaponBase selectedWeapon)
        {
            UpdateTargetSearching(selectedWeapon);
        }

        private void UpdateTargetSearching(WeaponBase selectedWeapon)
        {
            _targetLock?.SetSearchingEnabled(selectedWeapon == this);
        }
    }
}
