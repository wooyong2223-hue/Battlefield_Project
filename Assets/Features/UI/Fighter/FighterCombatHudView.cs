using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.UI
{
    public sealed class FighterCombatHudView : MonoBehaviour
    {
        [SerializeField] private WeaponController _weaponController;

        [Header("HUD Groups")]
        [SerializeField] private GameObject _gunAimUI;
        [SerializeField] private GameObject _missileAimUI;
        [SerializeField] private GameObject _overheatUI;

        private bool _isMissileSelected;

        private void OnEnable()
        {
            if (_weaponController != null)
            {
                _weaponController.SelectionChanged += HandleSelectionChanged;
                UpdateSelectedWeapon(_weaponController.SelectedWeapon);
            }

            RefreshVisibility();
        }

        private void OnDisable()
        {
            if (_weaponController != null)
            {
                _weaponController.SelectionChanged -= HandleSelectionChanged;
            }
        }

        private void HandleSelectionChanged(
            int selectedIndex,
            WeaponBase selectedWeapon)
        {
            UpdateSelectedWeapon(selectedWeapon);
            RefreshVisibility();
        }

        private void UpdateSelectedWeapon(WeaponBase selectedWeapon)
        {
            _isMissileSelected =
                selectedWeapon is AirToAirMissileLauncher;
        }

        private void RefreshVisibility()
        {
            SetActive(_gunAimUI, !_isMissileSelected);
            SetActive(_overheatUI, !_isMissileSelected);
            SetActive(_missileAimUI, _isMissileSelected);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
