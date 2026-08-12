using Battlefield.Features.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlefield.Features.UI
{
    public sealed class WeaponSelectionView : MonoBehaviour
    {
        [SerializeField] private WeaponController _controller;
        [SerializeField] private RectTransform _gunSlot;
        [SerializeField] private RectTransform _missileSlot;
        [SerializeField] private RectTransform _gunIconTransform;
        [SerializeField] private RectTransform _missileIconTransform;
        [SerializeField] private TMP_Text _gunAmmoText;
        [SerializeField] private TMP_Text _missileAmmoText;
        [SerializeField] private RectTransform _gunInfiniteIconTransform;
        [SerializeField] private Image _gunInfiniteIcon;
        [SerializeField] private Image _gunNumberBackground;
        [SerializeField] private Image _missileNumberBackground;
        [SerializeField] private TMP_Text _gunNumberText;
        [SerializeField] private TMP_Text _missileNumberText;

        private WeaponBase _gun;
        private WeaponBase _missile;
        private string _displayedMissileAmmo;

        private static readonly Vector2 SelectedPosition = new(112f, 27f);
        private static readonly Vector2 SelectedSize = new(224f, 68f);
        private static readonly Vector2 SelectedIconSize = new(116f, 40f);
        private static readonly Vector2 UnselectedPosition = new(130f, -25f);
        private static readonly Vector2 UnselectedSize = new(188f, 36f);
        private static readonly Vector2 UnselectedIconSize = new(78f, 16f);
        private static readonly Vector2 SelectedInfiniteIconPosition = new(92f, 0f);
        private static readonly Vector2 SelectedInfiniteIconSize = new(28f, 16f);
        private static readonly Vector2 UnselectedInfiniteIconPosition = new(77f, 0f);
        private static readonly Vector2 UnselectedInfiniteIconSize = new(20f, 12f);

        private void OnEnable()
        {
            if (_controller == null) return;
            _controller.SelectionChanged += UpdateSelection;
            _gun = _controller.GetWeapon(0);
            _missile = _controller.GetWeapon(1);
            UpdateSelection(_controller.SelectedIndex, _controller.SelectedWeapon);
        }

        private void OnDisable()
        {
            if (_controller != null) _controller.SelectionChanged -= UpdateSelection;
        }

        private void UpdateSelection(int selectedIndex, WeaponBase weapon)
        {
            SetSlot(_gunSlot, _gunIconTransform, _gunAmmoText, _gunNumberBackground, _gunNumberText, selectedIndex == 0);
            SetSlot(_missileSlot, _missileIconTransform, _missileAmmoText, _missileNumberBackground, _missileNumberText, selectedIndex == 1);
            SetInfiniteAmmoIcon(selectedIndex == 0);
        }

        private void LateUpdate()
        {
            if (_missileAmmoText != null && _missile != null && _displayedMissileAmmo != _missile.AmmoText)
            {
                _displayedMissileAmmo = _missile.AmmoText;
                _missileAmmoText.text = _displayedMissileAmmo;
            }
        }

        private void SetInfiniteAmmoIcon(bool selected)
        {
            if (_gunAmmoText != null) _gunAmmoText.gameObject.SetActive(false);
            if (_gunInfiniteIcon == null) return;

            _gunInfiniteIcon.gameObject.SetActive(true);

            if (_gunInfiniteIconTransform == null) return;
            _gunInfiniteIconTransform.anchoredPosition = selected
                ? SelectedInfiniteIconPosition
                : UnselectedInfiniteIconPosition;
            _gunInfiniteIconTransform.sizeDelta = selected
                ? SelectedInfiniteIconSize
                : UnselectedInfiniteIconSize;
        }

        private void SetSlot(
            RectTransform slot,
            RectTransform iconTransform,
            TMP_Text ammoText,
            Image numberBackground,
            TMP_Text numberText,
            bool selected)
        {
            if (slot != null)
            {
                slot.anchoredPosition = selected ? SelectedPosition : UnselectedPosition;
                slot.sizeDelta = selected ? SelectedSize : UnselectedSize;
                if (selected) slot.SetAsLastSibling();
            }

            if (iconTransform != null)
                iconTransform.sizeDelta = selected ? SelectedIconSize : UnselectedIconSize;

            if (ammoText != null)
                ammoText.fontSize = selected ? 24f : 14f;

            bool showNumber = !selected;
            if (numberBackground != null)
                numberBackground.gameObject.SetActive(showNumber);

            if (numberText != null)
            {
                numberText.gameObject.SetActive(showNumber);
                numberText.fontSize = 16f;
            }
        }
    }
}
