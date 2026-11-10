using System;
using System.Collections.Generic;
using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DisallowMultipleComponent, DefaultExecutionOrder(-30)]
    public sealed class PlayerCharacterWeaponEquipment : MonoBehaviour
    {
        [SerializeField] private InfantryWeaponDefinition _definition;
        [SerializeField] private WeaponMagazine _existingMagazine;
        private readonly Dictionary<InfantryWeaponDefinition, WeaponMagazineState> _magazines = new();
        private InfantryWeaponDefinition _currentDefinition;
        private IWeaponMagazine _magazine;
        private bool _initialized;
        public InfantryWeaponDefinition Definition => _initialized ? _currentDefinition : _definition;
        public IWeaponMagazine Magazine { get { Initialize(); return _magazine; } }
        public event Action<PlayerCharacterWeaponEquipment> Changed;

        private void Awake() => Initialize();
        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            ApplyDefinition(_definition, false);
        }
        public bool TryEquip(InfantryWeaponDefinition definition)
        {
            Initialize();
            if (definition == null || definition == _currentDefinition) return false;
            ApplyDefinition(definition, true); return true;
        }
        private void ApplyDefinition(InfantryWeaponDefinition definition, bool notify)
        {
            _magazine?.CancelReload();
            _definition = _currentDefinition = definition;
            _magazine = null;
            if (definition != null)
            {
                if (_existingMagazine != null) _magazine = _existingMagazine;
                else
                {
                    if (!_magazines.TryGetValue(definition, out var state))
                    {
                        state = new WeaponMagazineState(definition.Capacity, definition.Reserve, definition.ReloadDuration,
                            () => Time.time, () => isActiveAndEnabled);
                        _magazines.Add(definition, state);
                    }
                    _magazine = state;
                }
            }
            if (notify) Changed?.Invoke(this);
        }
        private void Update()
        {
            // Inspector changes use the same cancellation/event path as runtime equipment changes.
            if (_definition != _currentDefinition) ApplyDefinition(_definition, true);
            foreach (var state in _magazines.Values) state.Tick();
        }
        public void ResetAmmo()
        {
            Initialize(); _existingMagazine?.ResetAmmo();
            foreach (var state in _magazines.Values) state.ResetAmmo();
        }
        private void OnDisable()
        {
            _magazine?.CancelReload();
            foreach (var state in _magazines.Values) state.CancelReload();
        }
    }
}
