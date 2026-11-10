using UnityEngine;

namespace Battlefield.Features.Vehicles
{
    public interface IVehicleSeatOccupant
    {
        Transform Root { get; }
        CharacterController CollisionBody { get; }
        bool HasInputControl { get; }
        bool CanFireWeapon { get; }
        bool CanEnterSeat { get; }

        // Returning false must leave the occupant unattached and its controls unchanged.
        bool TryAttachToSeat(VehicleSeat seat);

        // Restore independent control and unparent from the vehicle before it is removed.
        void DetachFromSeat(VehicleSeat seat, Vector3 position, Quaternion rotation);
    }
}
