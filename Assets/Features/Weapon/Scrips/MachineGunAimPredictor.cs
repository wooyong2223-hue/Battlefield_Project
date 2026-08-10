using Battlefield.Features.Projectile;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    [RequireComponent(typeof(MachineGun))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MachineGunAimPredictor : MonoBehaviour
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

        [SerializeField] private float _predictionDistance = 1000f;

        private MachineGun _machineGun;
        private Rigidbody _ownerRigidbody;
        private Transform _firePoint;
        private Bullet _projectilePrefab;
        private TrajectorySample[] _trajectoryHistory;
        private int _nextSampleIndex;
        private int _sampleCount;

        private void Awake()
        {
            _machineGun = GetComponent<MachineGun>();
            _ownerRigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _firePoint = _machineGun.FirePoint;
            _projectilePrefab = BulletPool.Instance != null
                ? BulletPool.Instance.ProjectilePrefab
                : null;

            InitializeTrajectoryHistory();
            RecordTrajectorySample();
        }

        private void FixedUpdate()
        {
            RecordTrajectorySample();
        }

        public bool TryGetPredictedPoint(out Vector3 predictedPoint)
        {
            if (_firePoint == null ||
                _projectilePrefab == null ||
                _trajectoryHistory == null ||
                _sampleCount == 0)
            {
                predictedPoint = _firePoint != null
                    ? _firePoint.position
                    : transform.position;
                return false;
            }

            float projectileSpeed = Mathf.Abs(_projectilePrefab.Speed);
            if (projectileSpeed <= Mathf.Epsilon)
            {
                predictedPoint = _firePoint.position;
                return false;
            }

            float travelTime = Mathf.Max(0f, _predictionDistance)
                / projectileSpeed;
            float targetFireTime = Time.time - travelTime;
            TrajectorySample sample = GetSampleAt(targetFireTime);
            if (sample.Velocity.sqrMagnitude <= Mathf.Epsilon)
            {
                predictedPoint = _firePoint.position;
                return false;
            }

            predictedPoint = _firePoint.position
                + sample.Velocity.normalized
                * Mathf.Max(0f, _predictionDistance);
            return true;
        }

        private void InitializeTrajectoryHistory()
        {
            if (_projectilePrefab == null)
            {
                _trajectoryHistory = null;
                return;
            }

            float projectileSpeed = Mathf.Abs(_projectilePrefab.Speed);
            float travelTime = projectileSpeed > Mathf.Epsilon
                ? Mathf.Max(0f, _predictionDistance) / projectileSpeed
                : 0f;
            int capacity = Mathf.Max(
                2,
                Mathf.CeilToInt(travelTime / Time.fixedDeltaTime) + 2);

            _trajectoryHistory = new TrajectorySample[capacity];
            _nextSampleIndex = 0;
            _sampleCount = 0;
        }

        private void RecordTrajectorySample()
        {
            if (_firePoint == null ||
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
                    Time.fixedTime,
                    projectileVelocity);

            _nextSampleIndex =
                (_nextSampleIndex + 1) % _trajectoryHistory.Length;
            _sampleCount = Mathf.Min(
                _sampleCount + 1,
                _trajectoryHistory.Length);
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
