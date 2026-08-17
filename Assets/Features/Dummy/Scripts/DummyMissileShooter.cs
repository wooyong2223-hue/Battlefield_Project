using Battlefield.Features.Projectile;
using Battlefield.Features.Targeting;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Dummy
{
    [RequireComponent(typeof(Team))]
    public sealed class DummyMissileShooter : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private AirTarget _target;

        [Header("Fire")]
        [SerializeField] private HomingMissilePool _missilePool;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _fireInterval = 8f;
        [SerializeField] private float _startDelay = 2f;
        [SerializeField] private float _lockDuration = 2f;
        [SerializeField] private float _lockedDuration = 1f;
        [SerializeField] private float _damage = 10f;

        private IMissileWarningReceiver _warningReceiver;
        private MissileWarningState _state;
        private float _stateStartTime;
        private float _stateEndTime;

        private void Awake()
        {
            _fireInterval = Mathf.Max(0.01f, _fireInterval);
            _startDelay = Mathf.Max(0f, _startDelay);
            _lockDuration = Mathf.Max(0f, _lockDuration);
            _lockedDuration = Mathf.Max(0f, _lockedDuration);
            _damage = Mathf.Max(0f, _damage);
            _warningReceiver =
                _target?.GetComponent<IMissileWarningReceiver>();
        }

        private void OnEnable()
        {
            EnterState(MissileWarningState.None, _startDelay);
        }

        private void OnDisable()
        {
            _warningReceiver?.ClearThreatState(this);
        }

        private void Update()
        {
            if (_target == null ||
                !_target.IsAvailable ||
                _missilePool == null)
            {
                EnterState(MissileWarningState.None, 0f);
                return;
            }

            if (_state == MissileWarningState.Locking)
            {
                ReportLockProgress();
            }

            if (Time.time < _stateEndTime)
            {
                return;
            }

            AdvanceState();
        }

        private void AdvanceState()
        {
            switch (_state)
            {
                case MissileWarningState.None:
                    EnterState(
                        MissileWarningState.Locking,
                        _lockDuration);
                    break;
                case MissileWarningState.Locking:
                    EnterState(
                        MissileWarningState.Locked,
                        _lockedDuration);
                    break;
                case MissileWarningState.Locked:
                    Fire();
                    EnterState(
                        MissileWarningState.None,
                        _fireInterval);
                    break;
            }
        }

        private void EnterState(
            MissileWarningState state,
            float duration)
        {
            _state = state;
            _stateStartTime = Time.time;
            _stateEndTime = Time.time + duration;

            if (_state == MissileWarningState.None)
            {
                _warningReceiver?.ClearThreatState(this);
                return;
            }

            _warningReceiver?.ReportThreatState(this, _state);

            if (_state == MissileWarningState.Locking)
            {
                _warningReceiver?.ReportThreatProgress(this, 0f);
            }
        }

        private void ReportLockProgress()
        {
            float duration = _stateEndTime - _stateStartTime;
            float progress = duration <= Mathf.Epsilon
                ? 1f
                : Mathf.InverseLerp(
                    _stateStartTime,
                    _stateEndTime,
                    Time.time);
            _warningReceiver?.ReportThreatProgress(this, progress);
        }

        private void Fire()
        {
            Transform firePoint = _firePoint != null
                ? _firePoint
                : transform;
            Vector3 direction = _target.AimPosition - firePoint.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            HomingMissile missile = _missilePool.Get();
            missile.transform.SetPositionAndRotation(
                firePoint.position,
                Quaternion.LookRotation(direction));
            missile.Initialize(transform, _target, _damage);
        }
    }
}
