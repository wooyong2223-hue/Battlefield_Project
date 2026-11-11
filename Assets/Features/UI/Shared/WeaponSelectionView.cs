using System;
using Battlefield.Features.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class WeaponSelectionView : MonoBehaviour
    {
        [Serializable]
        private sealed class WeaponSlotReferences
        {
            [SerializeField] private RectTransform _root;
            [SerializeField] private Image _weaponIcon;
            [SerializeField] private TMP_Text _ammoText;
            [SerializeField] private Image _infiniteAmmoIcon;
            [SerializeField] private Image _numberBackground;
            [SerializeField] private TMP_Text _numberText;
            [SerializeField] private RefillProgressView _refillProgress;

            public RectTransform Root => _root;
            public Image WeaponIcon => _weaponIcon;
            public TMP_Text AmmoText => _ammoText;
            public Image InfiniteAmmoIcon => _infiniteAmmoIcon;
            public Image NumberBackground => _numberBackground;
            public TMP_Text NumberText => _numberText;
            public RefillProgressView RefillProgress => _refillProgress;
        }

        [SerializeField] private WeaponController _controller;

        [Header("State Slots")]
        [SerializeField] private WeaponSlotReferences _selectedSlot = new();
        [SerializeField] private WeaponSlotReferences _unselectedSlot = new();

        [Header("Weapon Icons")]
        [SerializeField] private Sprite _gunIcon;
        [SerializeField] private Sprite _missileIcon;

        private WeaponBase _missile;
        private WeaponSlotReferences _missileDisplaySlot;
        private string _displayedMissileAmmo;

        private void OnEnable()
        {
            if (_controller == null)
            {
                return;
            }

            _missile = _controller.GetWeapon(1);
            _controller.SelectionChanged += UpdateSelection;
            UpdateSelection(
                _controller.SelectedIndex,
                _controller.SelectedWeapon);
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.SelectionChanged -= UpdateSelection;
            }
        }

        private void LateUpdate()
        {
            if (_missileDisplaySlot?.AmmoText == null ||
                _missile == null ||
                _displayedMissileAmmo == _missile.AmmoText)
            {
                return;
            }

            _displayedMissileAmmo = _missile.AmmoText;
            _missileDisplaySlot.AmmoText.text = _displayedMissileAmmo;
        }

        private void UpdateSelection(
            int selectedIndex,
            WeaponBase selectedWeapon)
        {
            int unselectedIndex = selectedIndex == 0 ? 1 : 0;

            PresentWeapon(_selectedSlot, selectedIndex, false);
            PresentWeapon(_unselectedSlot, unselectedIndex, true);

            _missileDisplaySlot = selectedIndex == 1
                ? _selectedSlot
                : _unselectedSlot;
            _displayedMissileAmmo = null;
        }

        private void PresentWeapon(
            WeaponSlotReferences slot,
            int weaponIndex,
            bool showNumber)
        {
            bool isGun = weaponIndex == 0;
            WeaponBase weapon = _controller.GetWeapon(weaponIndex);

            if (slot.WeaponIcon != null)
            {
                slot.WeaponIcon.sprite = isGun
                    ? _gunIcon
                    : _missileIcon;
            }

            SetActive(slot.AmmoText, !isGun);
            SetActive(slot.InfiniteAmmoIcon, isGun);
            SetActive(slot.NumberBackground, showNumber);
            SetActive(slot.NumberText, showNumber);
            slot.RefillProgress?.Bind(
                weapon as IRefillProgressSource);

            if (showNumber && slot.NumberText != null)
            {
                slot.NumberText.SetText((weaponIndex + 1).ToString());
            }

            if (!isGun && slot.AmmoText != null && _missile != null)
            {
                slot.AmmoText.text = _missile.AmmoText;
            }
        }

        private static void SetActive(Component target, bool active)
        {
            if (target != null && target.gameObject.activeSelf != active)
            {
                target.gameObject.SetActive(active);
            }
        }
    }
}
