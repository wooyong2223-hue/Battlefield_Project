using Battlefield.Features.Vehicles;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-75)]
    public sealed class PlayerVehicleInteraction : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterInput _input;
        [SerializeField] private PlayerVehicleOccupant _occupant;
        [SerializeField] private VehicleSeat[] _seats;
        [SerializeField] private PlayerCharacterRevive _revive;

        public PlayerVehicleOccupant Occupant => _occupant;
        public VehicleSeat NearbySeat { get; private set; }
        public bool ExitBlocked { get; private set; }

        private void Awake()
        {
            if (_input != null && _occupant != null && _seats != null) return;
            Debug.LogError("Player vehicle interaction references are missing.", this);
            enabled = false;
        }

        private void Update()
        {
            NearbySeat = FindNearestSeat();
            if (_input.InteractPressed && (_occupant.IsSeated || _revive == null || (_revive.Candidate == null && !_revive.IsReviving)))
            {
                if (_occupant.CurrentSeat != null)
                    ExitBlocked = !_occupant.CurrentSeat.TryExit();
                else if (NearbySeat != null)
                    ExitBlocked = !NearbySeat.TryEnter(_occupant);
            }
            _occupant.CurrentSeat?.RefreshDriverControl();
            if (!_occupant.IsSeated) ExitBlocked = false;
        }

        private VehicleSeat FindNearestSeat()
        {
            if (_occupant.IsSeated) return null;
            VehicleSeat nearest = null;
            float bestDistance = float.PositiveInfinity;
            foreach (VehicleSeat seat in _seats)
            {
                if (seat == null || !seat.CanEnter(_occupant)) continue;
                float distance = (seat.SeatPoint.position - transform.position).sqrMagnitude;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                nearest = seat;
            }
            return nearest;
        }
    }
}
