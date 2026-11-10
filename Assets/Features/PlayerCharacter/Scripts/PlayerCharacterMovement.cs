using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerCharacterMovement : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 8f;
        [SerializeField, Min(0f)] private float _sprintSpeed = 12f;
        [SerializeField, Min(0f)] private float _jumpHeight = 1.2f;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private PlayerCharacterTraversal _traversal;

        [Header("Movement response")]
        [SerializeField, Min(.01f)] private float _acceleration = 80f;
        [SerializeField, Min(.01f)] private float _deceleration = 100f;
        [SerializeField, Min(.01f)] private float _airAcceleration = 24f;
        private Vector3 _planarVelocity;
        private Vector3 _resolvedVelocity;

        [SerializeField] private PlayerCharacterMantle _mantle;
        public void SetMantle(PlayerCharacterMantle mantle) => _mantle = mantle;

        private const float GroundStickSpeed = 2f;
        private CharacterController _body;
        private IPlayerCharacterInput _input;
        private float _verticalSpeed;
        private Vector3 _externalDisplacement;

        public bool IsGrounded => _body != null && _body.isGrounded;
        public bool IsSprinting { get; private set; }
        public Vector3 Velocity => _resolvedVelocity;
        public float MoveSpeed => _moveSpeed;
        public float SprintSpeed => _sprintSpeed;

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _input = GetComponent<IPlayerCharacterInput>();
        }

        private void Update()
        {
            if (_input != null)
                Tick(_input.Move, _input.SprintHeld, _input.JumpPressed, Time.deltaTime);
        }

        public void Tick(
            Vector2 move, bool sprint, bool jump, float deltaTime)
        {
            if (!isActiveAndEnabled || !_body.enabled || deltaTime <= 0f) return;
            Vector3 started = transform.position;
            if (_actions != null && !_actions.IsAlive)
            { move = Vector2.zero; _planarVelocity = Vector3.zero; sprint = false; jump = false; if (_traversal != null) _traversal.Cancel(); }
            if (_stance != null && _stance.IsProneTransitioning)
            {
                // Freeze horizontal intent/inertia while gravity and collision resolution continue.
                move = Vector2.zero; _planarVelocity = _externalDisplacement = Vector3.zero;
                sprint = jump = false;
                if (_traversal != null) _traversal.Cancel();
            }
            if (_actions != null && !_actions.CanJump) jump = false;
            if ((_mantle != null && _mantle.Step(jump, deltaTime)) ||
                (_traversal != null && _traversal.Step(jump, deltaTime)))
            { _verticalSpeed = -GroundStickSpeed; _planarVelocity = Vector3.zero; _resolvedVelocity = (transform.position - started) / deltaTime; IsSprinting = false; return; }

            if (_body.isGrounded && _verticalSpeed < 0f)
                _verticalSpeed = -GroundStickSpeed;

            float gravity = UnityEngine.Physics.gravity.y;
            bool crouched = _stance != null && _stance.IsLowered;
            if (jump && !crouched && _body.isGrounded && gravity < 0f)
                _verticalSpeed = Mathf.Sqrt(-2f * gravity * _jumpHeight);

            _verticalSpeed += gravity * deltaTime;
            Vector2 input = Vector2.ClampMagnitude(move, 1f);
            IsSprinting = !crouched && sprint && input.y > .1f && (_actions == null || _actions.CanSprint);
            float speed = crouched ? _stance.MoveSpeed : IsSprinting ? _sprintSpeed : _moveSpeed;
            Vector3 planar = (transform.right * input.x + transform.forward * input.y) * speed;
            float response = !_body.isGrounded ? _airAcceleration
                : planar.sqrMagnitude < _planarVelocity.sqrMagnitude ? _deceleration : _acceleration;
            _planarVelocity = Vector3.MoveTowards(_planarVelocity, planar, Mathf.Max(.01f, response) * deltaTime);
            if (crouched) _planarVelocity = Vector3.ClampMagnitude(_planarVelocity, speed);
            Vector3 displacement = _planarVelocity * deltaTime + _externalDisplacement;
            bool externalMove = _externalDisplacement.sqrMagnitude > 0f;
            _externalDisplacement = Vector3.zero;
            if (_actions != null) displacement *= _actions.MovementMultiplier;
            if (_stance != null) displacement = _stance.ConstrainPlanarMovement(displacement);
            CollisionFlags collisions = _body.Move(
                displacement + Vector3.up * (_verticalSpeed * deltaTime));

            _resolvedVelocity = (transform.position - started) / deltaTime;
            // Do not keep accelerating into a blocking wall; retain the achieved tangent motion.
            if (!externalMove && (collisions & CollisionFlags.Sides) != 0)
            {
                float multiplier = _actions != null ? _actions.MovementMultiplier : 1f;
                _planarVelocity = multiplier > .001f
                    ? Vector3.ProjectOnPlane(_resolvedVelocity, Vector3.up) / multiplier : Vector3.zero;
            }
            if ((collisions & CollisionFlags.Above) != 0 && _verticalSpeed > 0f)
                _verticalSpeed = 0f;
            if ((collisions & CollisionFlags.Below) != 0 && _verticalSpeed < 0f)
                _verticalSpeed = -GroundStickSpeed;
        }

        public void Stop()
        {
            _verticalSpeed = 0f;
            _planarVelocity = _resolvedVelocity = Vector3.zero;
            _externalDisplacement = Vector3.zero;
            IsSprinting = false;
            if (_traversal != null) _traversal.Cancel();
            if (_mantle != null) _mantle.Cancel();
        }

        private void OnDisable() => Stop();
        public void QueueExternalDisplacement(Vector3 displacement) => _externalDisplacement += Vector3.ProjectOnPlane(displacement, Vector3.up);
    }
}
