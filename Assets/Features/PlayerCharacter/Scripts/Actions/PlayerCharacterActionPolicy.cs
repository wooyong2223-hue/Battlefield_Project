using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Resolves competing action requests without owning movement, aim or weapon state.
    public sealed class PlayerCharacterActionPolicy : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private PlayerCharacterLookController _view;
        [SerializeField] private PlayerCharacterCombatController _combat;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField] private PlayerCharacterRevive _revive;
        [SerializeField] private PlayerCharacterMantle _mantle;
        [SerializeField] private PlayerCharacterGrenadeThrow _grenade;
        public void SetTacticalActions(PlayerCharacterMantle mantle, PlayerCharacterGrenadeThrow grenade) { _mantle = mantle; _grenade = grenade; }
        private bool TacticalBusy => (_mantle != null && _mantle.IsBusy) || (_grenade != null && _grenade.IsThrowing);
        public bool CanStartTacticalAction => CanAttack && (_traversal == null || !_traversal.IsSliding) && (_combat == null || (!_combat.IsAttacking && (_combat.Magazine == null || !_combat.Magazine.IsReloading)));
        public bool CanChangeStance => IsAlive && !TacticalBusy;
        private IWeaponInput _weaponInput;
        private IPlayerCharacterCombatInput _combatInput;

        public bool IsAlive => _health == null || !_health.IsDead;
        public bool CanJump => IsAlive && !TacticalBusy && (_traversal == null || !_traversal.IsVaulting) && (_revive == null || !_revive.IsReviving);
        public float MovementMultiplier => _revive != null && _revive.IsReviving ? .3f : 1f;
        public bool CanUseHands => IsAlive && (_mantle == null || !_mantle.IsBusy) &&
            (_traversal == null || !_traversal.IsVaulting) && (_revive == null || !_revive.IsReviving);
        public bool CanAimWeapon => _grenade == null || !_grenade.IsEquipped;
        public bool CanAttack => CanUseHands && (_grenade == null || !_grenade.IsEquipped);
        public bool CanSprint => CanUseHands && !TacticalBusy && (_view == null || (!_view.IsAiming && !_view.IsAds)) &&
            (_combat == null || (!_combat.IsAttacking && (_combat.Magazine == null || !_combat.Magazine.IsReloading))) &&
            (_weaponInput == null || !_weaponInput.FireWeapon) &&
            (_combatInput == null || (!_combatInput.MeleePressed && !_combatInput.ReloadPressed));

        private void Awake()
        {
            _weaponInput = GetComponent<IWeaponInput>();
            _combatInput = GetComponent<IPlayerCharacterCombatInput>();
        }
    }
}
