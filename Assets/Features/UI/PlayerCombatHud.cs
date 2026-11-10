using Battlefield.Features.PlayerCharacter;
using Battlefield.Framework.Core;
using TMPro;
using UnityEngine;

namespace Battlefield.Features.UI
{
    public sealed class PlayerCombatHud : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField] private PlayerCharacterWeaponMount _mount;
        [SerializeField] private Health _health;
        [SerializeField] private InfantryLifeState _life;
        [SerializeField] private PlayerCharacterRevive _revive;
        [SerializeField] private PlayerRifleStationInteraction _rifleStations;
        [SerializeField] private PlayerCharacterGrenadeThrow _grenade;
        [SerializeField] private PlayerCharacterMantle _mantle;
        public void SetTacticalActions(PlayerCharacterGrenadeThrow grenade, PlayerCharacterMantle mantle) { _grenade = grenade; _mantle = mantle; }
        private void Awake()
        {
            if (_combat != null && _label != null) return;
            Debug.LogError("Player combat HUD references are missing.", this); enabled = false;
        }
        private void LateUpdate()
        {
            var magazine = _combat.Magazine;
            string text = !_combat.CanUseCombat ? string.Empty : !_combat.IsArmed ? "FISTS" :
                magazine == null ? _combat.WeaponName : magazine.IsReloading ? $"{_combat.WeaponName}  RELOADING" :
                $"{_combat.WeaponName}  {magazine.Rounds} / {magazine.Reserve}";
            if (!string.IsNullOrEmpty(text))
            {
                if (_combat.WeaponSlot == PlayerCharacterLoadout.RifleSlot && (_grenade == null || !_grenade.IsEquipped))
                    text += _combat.IsRifleSingleFire ? " {단발}" : " {연발}";

                if (_health != null)
                    text += $"\nHP {Mathf.CeilToInt(_health.CurrentHealth)}";
            }
            if (_label.text != text) _label.text = text;
        }
    }
}
