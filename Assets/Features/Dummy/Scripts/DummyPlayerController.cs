using Battlefield.Projectile;
using UnityEngine;

namespace Battlefield.Dummy
{
    [RequireComponent(typeof(Collider))]
    public class DummyPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 8f;
        [SerializeField, Min(0.01f)] private float _rotationSensitivity = 3f;

        [Header("Fire")]
        [SerializeField, Min(0.01f)] private float _fireRate = 10f;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0f)] private float _muzzleOffset = 0.6f;

        private Collider _collider;
        private float _nextFireTime;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _nextFireTime = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            Rotate();
            Move();

            if (Input.GetMouseButton(0))
            {
                TryFire();
            }
        }

        private void Move()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = transform.right * horizontal +
                            transform.forward * vertical;

            if (input.sqrMagnitude > 1f) input.Normalize();

            transform.position += input * (_moveSpeed * Time.deltaTime);
        }

        private void Rotate()
        {
            if (!Input.GetMouseButton(1)) return;

            float yaw = Input.GetAxis("Mouse X") * _rotationSensitivity;
            transform.Rotate(0f, yaw, 0f, Space.World);
        }

        private void TryFire()
        {
            if (BulletPool.Instance == null || Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + 1f / _fireRate;

            Vector3 fireDirection = transform.forward;
            Vector3 target = transform.position + fireDirection;
            Vector3 origin = _collider.ClosestPoint(target) +
                             fireDirection * _muzzleOffset;

            Bullet bullet = BulletPool.Instance.Get();
            bullet.transform.SetPositionAndRotation(
                origin,
                transform.rotation);
            bullet.Initialize(new ProjectileData(transform, _damage));
        }
    }
}
