using Battlefield.Projectile;
using Battlefield.Input;
using UnityEngine;

namespace Battlefield.Dummy
{
    [RequireComponent(typeof(Collider))]
    public class DummyPlayerController : MonoBehaviour
    {
        private const float MouseDeltaScale = 0.1f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 8f;
        [SerializeField, Min(0.01f)] private float _rotationSensitivity = 3f;

        [Header("Fire")]
        [SerializeField, Min(0.01f)] private float _fireRate = 10f;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0f)] private float _muzzleOffset = 0.6f;

        private Collider _collider;
        private float _nextFireTime;
        private InputSystem_Actions _inputActions;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _nextFireTime = 0f;
            _inputActions.Player.Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            Rotate();
            Move();

            if (_inputActions.Player.Attack.IsPressed())
            {
                TryFire();
            }
        }

        private void Move()
        {
            Vector2 move = _inputActions.Player.Move.ReadValue<Vector2>();
            Vector3 input = transform.right * move.x +
                            transform.forward * move.y;

            if (input.sqrMagnitude > 1f) input.Normalize();

            transform.position += input * (_moveSpeed * Time.deltaTime);
        }

        private void Rotate()
        {
            if (!_inputActions.Player.Rotate.IsPressed()) return;

            float yaw = _inputActions.Player.Look.ReadValue<Vector2>().x *
                        MouseDeltaScale * _rotationSensitivity;
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
