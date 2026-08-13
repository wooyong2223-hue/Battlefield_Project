using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Targeting
{
    [RequireComponent(typeof(Team))]
    public sealed class AirTargetLock : MonoBehaviour
    {
        [SerializeField] private Transform _searchOrigin;
        [SerializeField] private float _lockDuration = 1.5f;
        [SerializeField] private AirTargetScanner _scanner = new();
        [SerializeField] private AirTargetScreenArea _screenArea = new();
        private Team _ownerTeam;
        private float _elapsed;
        private bool _isSearchingEnabled = true;

        public AirTarget CurrentTarget { get; private set; }
        public AirTargetLockState State { get; private set; }
        public float LockProgress => _lockDuration <= Mathf.Epsilon ? 1f : Mathf.Clamp01(_elapsed / _lockDuration);
        public bool HasLock => State == AirTargetLockState.Locked && CurrentTarget != null;

        private void Awake()
        {
            _ownerTeam = GetComponent<Team>();
            if (_searchOrigin == null) _searchOrigin = transform;
        }

        private void Update()
        {
            if (!_isSearchingEnabled)
            {
                return;
            }

            AirTarget candidate = _scanner.FindBestTarget(
                _searchOrigin,
                transform,
                _ownerTeam.CurrentTeam,
                _screenArea);
            if (candidate != CurrentTarget)
            {
                CurrentTarget = candidate;
                _elapsed = 0f;
            }

            if (CurrentTarget == null)
            {
                State = AirTargetLockState.Searching;
                return;
            }

            _elapsed += Time.deltaTime;
            State = _elapsed >= Mathf.Max(0f, _lockDuration) ? AirTargetLockState.Locked : AirTargetLockState.Acquiring;
        }

        private void OnDisable()
        {
            ResetLock();
        }

        public void SetSearchingEnabled(bool isEnabled)
        {
            if (_isSearchingEnabled == isEnabled)
            {
                return;
            }

            _isSearchingEnabled = isEnabled;
            if (!isEnabled)
            {
                ResetLock();
            }
        }

        private void ResetLock()
        {
            CurrentTarget = null;
            _elapsed = 0f;
            State = AirTargetLockState.Searching;
        }
    }
}
