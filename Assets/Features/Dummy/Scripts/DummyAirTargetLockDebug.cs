using Battlefield.Features.Targeting;
using UnityEngine;

namespace Battlefield.Features.Dummy
{
    [RequireComponent(typeof(AirTargetLock))]
    public sealed class DummyAirTargetLockDebug : MonoBehaviour
    {
        private AirTargetLock _lock;
        private AirTarget _lastTarget;
        private AirTargetLockState _lastState;

        private void Awake() => _lock = GetComponent<AirTargetLock>();

        private void Update()
        {
            if (_lastTarget == _lock.CurrentTarget && _lastState == _lock.State) return;
            _lastTarget = _lock.CurrentTarget;
            _lastState = _lock.State;
            Debug.Log($"Air target lock: {_lastState}, Target: {(_lastTarget != null ? _lastTarget.name : "None")}", this);
        }
    }
}
