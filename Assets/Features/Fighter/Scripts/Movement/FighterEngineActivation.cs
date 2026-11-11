using Battlefield.Features.Vehicles;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    // Interaction (-75) changes occupancy before engine state and ordinary flight updates.
    [DefaultExecutionOrder(-25)]
    [DisallowMultipleComponent]
    public sealed class FighterEngineActivation : MonoBehaviour
    {
        [SerializeField] private VehicleSeat _seat;
        [SerializeField] private JetMovement _movement;

        public bool IsRunning => _movement != null && _movement.IsEngineRunning;

        private void Start()
        {
            if (_seat == null || _movement == null)
            {
                Debug.LogError("Fighter engine requires a pilot seat and movement reference.", this);
                enabled = false;
                return;
            }
            RefreshState();
        }

        private void Update() => RefreshState();

        private void RefreshState()
        {
            if (_movement == null) return;
            _movement.SetEngineRunning(_seat != null && _seat.IsAvailable && _seat.IsOccupied);
        }

        private void OnDisable()
        {
            if (_movement != null) _movement.SetEngineRunning(false);
        }
    }
}
