using Battlefield.Features.Projectile;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public sealed class MachineGunAimPredictor
    {
        private struct TrajectorySample
        {
            public float Time;
            public Vector3 Velocity;

            public TrajectorySample(
                float time,
                Vector3 velocity)
            {
                Time = time;
                Velocity = velocity;
            }
        }

        private readonly Rigidbody _ownerRigidbody;
        private readonly Transform _firePoint;
        private readonly Bullet _projectilePrefab;
        private readonly float _predictionDistance;
        private TrajectorySample[] _trajectoryHistory;
        private int _nextSampleIndex;
        private int _sampleCount;

        public MachineGunAimPredictor(
            Transform firePoint,
            Rigidbody ownerRigidbody,
            Bullet projectilePrefab,
            float predictionDistance,
            float fixedDeltaTime)
        {
            _firePoint = firePoint;
            _ownerRigidbody = ownerRigidbody;
            _projectilePrefab = projectilePrefab;
            _predictionDistance = Mathf.Max(
                0f,
                predictionDistance);

            InitializeTrajectoryHistory(fixedDeltaTime);
        }

        public void RecordTrajectorySample(float sampleTime)
        {
            if (_firePoint == null ||
                _ownerRigidbody == null ||
                _projectilePrefab == null ||
                _trajectoryHistory == null ||
                _trajectoryHistory.Length == 0)
            {
                return;
            }

            Vector3 projectileVelocity =
                _firePoint.forward * _projectilePrefab.Speed
                + _ownerRigidbody.linearVelocity;
            _trajectoryHistory[_nextSampleIndex] =
                new TrajectorySample(
                    sampleTime,
                    projectileVelocity);

            _nextSampleIndex =
                (_nextSampleIndex + 1) % _trajectoryHistory.Length;
            _sampleCount = Mathf.Min(
                _sampleCount + 1,
                _trajectoryHistory.Length);
        }

        public void ResetTrajectoryHistory(float sampleTime)
        {
            _nextSampleIndex = 0;
            _sampleCount = 0;
            RecordTrajectorySample(sampleTime);
        }

        public bool TryGetPredictedPoint(
            float currentTime,
            out Vector3 predictedPoint)
        {
            if (_firePoint == null ||
                _projectilePrefab == null ||
                _trajectoryHistory == null ||
                _sampleCount == 0)
            {
                predictedPoint = _firePoint != null
                    ? _firePoint.position
                    : Vector3.zero;
                return false;
            }

            float projectileSpeed = Mathf.Abs(_projectilePrefab.Speed);
            if (projectileSpeed <= Mathf.Epsilon)
            {
                predictedPoint = _firePoint.position;
                return false;
            }

            float travelTime = _predictionDistance / projectileSpeed;
            float targetFireTime = currentTime - travelTime;
            TrajectorySample sample = GetSampleAt(targetFireTime);
            if (sample.Velocity.sqrMagnitude <= Mathf.Epsilon)
            {
                predictedPoint = _firePoint.position;
                return false;
            }

            predictedPoint = _firePoint.position
                + sample.Velocity.normalized
                * _predictionDistance;
            return true;
        }

        private void InitializeTrajectoryHistory(float fixedDeltaTime)
        {
            if (_projectilePrefab == null)
            {
                _trajectoryHistory = null;
                return;
            }

            float projectileSpeed = Mathf.Abs(_projectilePrefab.Speed);
            float travelTime = projectileSpeed > Mathf.Epsilon
                ? _predictionDistance / projectileSpeed
                : 0f;
            float sampleInterval = Mathf.Max(
                Mathf.Epsilon,
                fixedDeltaTime);
            int capacity = Mathf.Max(
                2,
                Mathf.CeilToInt(travelTime / sampleInterval) + 2);

            _trajectoryHistory = new TrajectorySample[capacity];
            _nextSampleIndex = 0;
            _sampleCount = 0;
        }

        private TrajectorySample GetSampleAt(float targetTime)
        {
            TrajectorySample previous = GetChronologicalSample(0);
            if (targetTime <= previous.Time)
            {
                return previous;
            }

            for (int i = 1; i < _sampleCount; i++)
            {
                TrajectorySample next = GetChronologicalSample(i);
                if (targetTime <= next.Time)
                {
                    float duration = next.Time - previous.Time;
                    float interpolation = duration > Mathf.Epsilon
                        ? Mathf.Clamp01(
                            (targetTime - previous.Time) / duration)
                        : 0f;

                    return new TrajectorySample(
                        targetTime,
                        Vector3.Lerp(
                            previous.Velocity,
                            next.Velocity,
                            interpolation));
                }

                previous = next;
            }

            return previous;
        }

        private TrajectorySample GetChronologicalSample(int offset)
        {
            int oldestIndex =
                (_nextSampleIndex - _sampleCount
                    + _trajectoryHistory.Length)
                % _trajectoryHistory.Length;
            int sampleIndex =
                (oldestIndex + offset) % _trajectoryHistory.Length;

            return _trajectoryHistory[sampleIndex];
        }
    }
}
