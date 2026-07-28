using UnityEngine;
using Battlefield.Core;
using Battlefield.Projectile;

namespace Battlefield.Dummy
{
    [RequireComponent(typeof(Team))]
    public class DummyShooter : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Fire")]
        [SerializeField, Min(0.01f)] private float _fireInterval = 0.5f;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0f)] private float _startDelay = 0.5f;
        [SerializeField, Min(0f)] private float _muzzleOffset = 0.1f;

        private Collider _collider;
        private float _nextFireTime;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _nextFireTime = Time.time + _startDelay;
        }

        private void Update()
        {
            if (_target == null ||
                BulletPool.Instance == null ||
                Time.time < _nextFireTime)
            {
                return;
            }

            Fire();
            _nextFireTime = Time.time + _fireInterval;
        }

        private void Fire()
        {
            Vector3 targetPosition = _target.position;
            Vector3 origin = _collider != null
                ? _collider.ClosestPoint(targetPosition)
                : transform.position;
            Vector3 direction = targetPosition - origin;

            if (direction.sqrMagnitude <= Mathf.Epsilon) return;

            direction.Normalize();
            origin += direction * _muzzleOffset;

            Bullet bullet = BulletPool.Instance.Get();
            bullet.transform.SetPositionAndRotation(
                origin,
                Quaternion.LookRotation(direction));
            bullet.Initialize(new ProjectileData(transform, _damage));
        }
    }
}
