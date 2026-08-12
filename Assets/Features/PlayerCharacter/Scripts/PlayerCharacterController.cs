using Battlefield.Features.Projectile;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlefield.Features.PlayerCharacter
{
    [RequireComponent(typeof(Collider))]
    public class PlayerCharacterController : MonoBehaviour
    {
        private const float MouseDeltaScale = 0.1f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _moveSpeed = 8f;
        [SerializeField, Min(0.01f)] private float _rotationSensitivity = 3f;

        [Header("Fire")]
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField, Min(0.01f)] private float _fireRate = 10f;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0f)] private float _muzzleOffset = 0.6f;

        [Header("Input")]
        [SerializeField] private InputActionAsset _inputActions;

        private Collider _collider;
        private float _nextFireTime;
        private InputActionAsset _runtimeInputActions;
        private InputActionMap _playerActions;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _rotateAction;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _runtimeInputActions = Instantiate(_inputActions);
            _playerActions =
                _runtimeInputActions.FindActionMap("PlayerCharacter", true);

            _moveAction = _playerActions.FindAction("Move", true);
            _lookAction = _playerActions.FindAction("Look", true);
            _attackAction = _playerActions.FindAction("Attack", true);
            _rotateAction = _playerActions.FindAction("Rotate", true);
        }

        private void OnEnable()
        {
            _nextFireTime = 0f;
            _playerActions.Enable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            _playerActions.Disable();
        }

        private void OnDestroy()
        {
            Destroy(_runtimeInputActions);
        }

        private void Update()
        {
            Rotate();
            Move();

            if (_attackAction.IsPressed())
            {
                TryFire();
            }
        }

        private void Move()
        {
            Vector2 move = _moveAction.ReadValue<Vector2>();
            Vector3 input = transform.right * move.x +
                            transform.forward * move.y;

            if (input.sqrMagnitude > 1f) input.Normalize();

            transform.position += input * (_moveSpeed * Time.deltaTime);
        }

        private void Rotate()
        {
            if (!_rotateAction.IsPressed()) return;

            float yaw = _lookAction.ReadValue<Vector2>().x *
                        MouseDeltaScale * _rotationSensitivity;
            transform.Rotate(0f, yaw, 0f, Space.World);
        }

        private void TryFire()
        {
            if (_bulletPool == null || Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + 1f / _fireRate;

            Vector3 fireDirection = transform.forward;
            Vector3 target = transform.position + fireDirection;
            Vector3 origin = _collider.ClosestPoint(target) +
                             fireDirection * _muzzleOffset;

            Bullet bullet = _bulletPool.Get();
            bullet.transform.SetPositionAndRotation(
                origin,
                transform.rotation);
            bullet.Initialize(new ProjectileData(transform, _damage));
        }
    }
}
