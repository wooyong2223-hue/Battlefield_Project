using System;
using System.Collections.Generic;
using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-20)]
    public sealed class PlayerCharacterLoadout : MonoBehaviour
    {
        public const int RifleSlot = 1, PistolSlot = 2, UnarmedSlot = 3;
        [Serializable] private sealed class Entry
        {
            [SerializeField] internal int _slot;
            [SerializeField] internal PlayerCharacterWeaponEquipment _equipment;
        }
        [SerializeField] private Entry[] _weapons = Array.Empty<Entry>();
        private readonly Dictionary<int, PlayerCharacterWeaponEquipment> _items = new();
        public int CurrentSlot { get; private set; } = UnarmedSlot;
        private PlayerCharacterWeaponEquipment Current => GetEquipment(CurrentSlot);
        public PlayerCharacterWeaponEquipment GetEquipment(int slot) => _items.TryGetValue(slot, out var item) ? item : null;
        public InfantryWeaponDefinition Definition => Current != null ? Current.Definition : null;
        public IWeaponMagazine Magazine => Current != null ? Current.Magazine : null;
        public bool IsArmed => Definition != null;
        internal bool IsRifleSingleFire => GetEquipment(RifleSlot)?.SingleFireSelected == true;
        internal bool CanToggleFireMode => FireMode == InfantryFireMode.Selectable;
        internal bool UsesSingleFire => FireMode == InfantryFireMode.Single ||
            (FireMode == InfantryFireMode.Selectable && Current?.SingleFireSelected == true);

        private InfantryFireMode FireMode
        {
            get
            {
                InfantryFireMode mode = Definition != null ? Definition.FireMode : InfantryFireMode.DefaultForSlot;
                if (mode != InfantryFireMode.DefaultForSlot) return mode;
                // Definitions saved before FireMode was added keep their slot's original behavior.
                return CurrentSlot switch
                {
                    PistolSlot => InfantryFireMode.Single,
                    RifleSlot => InfantryFireMode.Selectable,
                    _ => InfantryFireMode.Automatic
                };
            }
        }

        internal void ToggleFireMode()
        {
            if (CanToggleFireMode)
                Current?.ToggleFireMode();
        }

        public event Action Changed;

        private void Awake()
        {
            foreach (var entry in _weapons)
            {
                if (entry._equipment == null || entry._slot <= 0 || entry._slot == UnarmedSlot ||
                    _items.ContainsKey(entry._slot) || _items.ContainsValue(entry._equipment))
                { Debug.LogError("Loadout contains an invalid or duplicate equipment slot.", this); enabled = false; return; }
                _items.Add(entry._slot, entry._equipment);
            }
            CurrentSlot = _items.ContainsKey(RifleSlot) ? RifleSlot : UnarmedSlot;
        }
        private void OnEnable()
        {
            foreach (var item in _items.Values) item.Changed += OnEquipmentChanged;
        }
        private void OnEquipmentChanged(PlayerCharacterWeaponEquipment equipment)
        {
            if (equipment == Current) Changed?.Invoke();
        }
        public bool TryEquip(int slot)
        {
            if (slot == CurrentSlot || (slot != UnarmedSlot && !_items.ContainsKey(slot))) return false;
            Magazine?.CancelReload(); CurrentSlot = slot; Changed?.Invoke(); return true;
        }
        public void ResetForRespawn()
        {
            foreach (var item in _items.Values) item.ResetAmmo();
            CurrentSlot = _items.ContainsKey(RifleSlot) ? RifleSlot : UnarmedSlot;
            Changed?.Invoke();
        }
        private void OnDisable()
        {
            foreach (var item in _items.Values)
            { item.Changed -= OnEquipmentChanged; item.Magazine?.CancelReload(); }
        }
    }
}
