using System;
using Battlefield.Features.Fighter;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private WeaponBase[] _weapons;
        private IJetInput _input;

        public int SelectedIndex { get; private set; }
        public WeaponBase SelectedWeapon =>
            _weapons != null && SelectedIndex < _weapons.Length
                ? _weapons[SelectedIndex]
                : null;
        public event Action<int, WeaponBase> SelectionChanged;

        public WeaponBase GetWeapon(int index)
        {
            return _weapons != null && index >= 0 && index < _weapons.Length
                ? _weapons[index]
                : null;
        }

        private void Awake()
        {
            _input = GetComponent<IJetInput>();
            if (_input == null) Debug.LogError($"{nameof(IJetInput)} Missing.", this);
        }

        private void Start() => SelectSlot(1);

        private void Update()
        {
            if (_input == null) return;
            if (_input.WeaponSlotSelection > 0) SelectSlot(_input.WeaponSlotSelection);
            if (_input.FireWeapon) SelectedWeapon?.TryFire();
        }

        public void SelectSlot(int slotNumber)
        {
            int index = slotNumber - 1;
            if (_weapons == null || index < 0 || index >= _weapons.Length || _weapons[index] == null || index == SelectedIndex && SelectedWeapon != null) return;
            SelectedIndex = index;
            SelectionChanged?.Invoke(SelectedIndex, SelectedWeapon);
        }
    }
}
