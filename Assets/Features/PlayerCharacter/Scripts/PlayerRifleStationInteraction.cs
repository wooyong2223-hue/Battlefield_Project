using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Weapon stands have lower interaction priority than reviving and vehicle entry.
    [DefaultExecutionOrder(-70)]
    public sealed class PlayerRifleStationInteraction : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _inputBehaviour;
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private PlayerCharacterLoadout _loadout;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private PlayerVehicleInteraction _vehicles;
        [SerializeField] private PlayerCharacterRevive _revive;
        [SerializeField] private RifleSelectionStation[] _stations;
        private IPlayerCharacterInteractionInput _input;
        private readonly RaycastHit[] _sight = new RaycastHit[32];
        public RifleSelectionStation NearbyStation { get; private set; }
        public bool IsEquipped => NearbyStation != null && _loadout.CurrentSlot == PlayerCharacterLoadout.RifleSlot &&
            _loadout.Definition == NearbyStation.Definition;
        private void Awake()
        {
            _input = _inputBehaviour as IPlayerCharacterInteractionInput;
            if (_input != null && _combat != null && _loadout != null && _stations != null) return;
            Debug.LogError("Rifle station interaction references are missing.", this); enabled = false;
        }
        private bool CanInteract => isActiveAndEnabled && _input != null && _input.HasControl && _combat.CanUseCombat &&
            !_combat.IsAttacking && (_actions == null || _actions.CanAttack) &&
            (_vehicles == null || (!_vehicles.Occupant.IsSeated && _vehicles.NearbySeat == null)) &&
            (_revive == null || (!_revive.IsReviving && _revive.Candidate == null));
        private void Update()
        {
            NearbyStation = FindNearest();
            if (_input != null && _input.InteractPressed) TryUseNearest();
        }
        private RifleSelectionStation FindNearest()
        {
            if (!CanInteract) return null;
            RifleSelectionStation result = null; float distance = float.PositiveInfinity;
            foreach (var station in _stations)
            {
                if (station == null || !station.IsInRange(transform) || !HasSight(station)) continue;
                float squared = (station.InteractionPoint - transform.position).sqrMagnitude;
                if (squared < distance) { result = station; distance = squared; }
            }
            return result;
        }
        private bool HasSight(RifleSelectionStation station)
        {
            Vector3 delta = station.InteractionPoint - transform.position;
            if (delta.sqrMagnitude < .0001f) return true;
            int count = Physics.RaycastNonAlloc(transform.position, delta.normalized, _sight, delta.magnitude, ~0, QueryTriggerInteraction.Ignore);
            if (count == _sight.Length) return false;
            for (int i = 0; i < count; i++)
                if (!_sight[i].transform.IsChildOf(transform) && !_sight[i].transform.IsChildOf(station.transform)) return false;
            return true;
        }
        public bool TryUseNearest()
        {
            NearbyStation = FindNearest();
            if (NearbyStation == null) return false;
            var equipment = _loadout.GetEquipment(PlayerCharacterLoadout.RifleSlot);
            if (equipment == null) return false;
            bool changed = equipment.TryEquip(NearbyStation.Definition);
            bool selected = _loadout.TryEquip(PlayerCharacterLoadout.RifleSlot);
            return changed || selected;
        }
        private void OnDisable() => NearbyStation = null;
    }
}
