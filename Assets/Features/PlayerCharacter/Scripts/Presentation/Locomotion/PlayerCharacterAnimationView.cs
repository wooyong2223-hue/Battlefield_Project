using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [RequireComponent(typeof(Animator)), DefaultExecutionOrder(20)]
    public sealed class PlayerCharacterAnimationView : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterMovement _movement;
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private InfantryLifeState _life;
        [SerializeField] private PlayerCharacterRevive _revive;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField] private SlideAnimationSet _slideAnimations;
        [SerializeField] private PlayerCharacterMantle _mantle;
        public void SetMantle(PlayerCharacterMantle mantle) => _mantle = mantle;
        public void SetSlideAnimations(SlideAnimationSet animations) => _slideAnimations = animations;
        [SerializeField, Min(0f)] private float _speedDamping = 0.1f;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int VerticalSpeed = Animator.StringToHash("VerticalSpeed");
        private static readonly int Crouched = Animator.StringToHash("Crouched");
        private static readonly int TraversalMode = Animator.StringToHash("TraversalMode");
        private static readonly int SlideTime = Animator.StringToHash("SlideTime");
        private static readonly int TraversalTime = Animator.StringToHash("TraversalTime");

        private void Awake()
        {
            if (_movement == null || _animator == null || _animator.runtimeAnimatorController == null)
            {
                Debug.LogError("Player character animation references are missing.", this);
                enabled = false;
                return;
            }

            // CharacterController remains the only movement authority.
            _animator.applyRootMotion = false;
        }

        private void Update()
        {
            Vector3 velocity = _movement.Velocity;
            float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            bool crouched = _stance != null && _stance.IsCrouched;
            float referenceSpeed = crouched ? _stance.MoveSpeed
                : _movement.MoveSpeed;
            float normalizedSpeed = referenceSpeed > 0f ? Mathf.Clamp01(planarSpeed / referenceSpeed) : 0f;
            if (!crouched && planarSpeed > _movement.MoveSpeed)
                normalizedSpeed = 1f + Mathf.InverseLerp(_movement.MoveSpeed, _movement.SprintSpeed, planarSpeed);
            if ((_stance != null && _stance.IsProne) || (_traversal != null && _traversal.IsSliding)) normalizedSpeed = 0f;

            // Use resolved movement rather than input: pushing against a wall must not
            // play a full-speed walk. Direction is relative to the movement root,
            // not the animated model or the camera (which can look around freely).
            if (planarSpeed > 0.05f)
            {
                Vector3 localVelocity = _movement.transform.InverseTransformDirection(
                    new Vector3(velocity.x, 0f, velocity.z));
                Vector2 direction = new Vector2(localVelocity.x, localVelocity.z).normalized;
                _animator.SetFloat(MoveX, direction.x, _speedDamping, Time.deltaTime);
                _animator.SetFloat(MoveY, direction.y, _speedDamping, Time.deltaTime);
            }
            // Retain the last direction as Speed fades to idle, so stopping a
            // sideways/backward walk does not briefly switch to a forward step.
            _animator.SetFloat(Speed, normalizedSpeed, _speedDamping, Time.deltaTime);
            _animator.SetBool(Grounded, (_mantle != null && _mantle.IsClimbing) || _movement.IsGrounded);
            _animator.SetFloat(VerticalSpeed, velocity.y);
            // Prepare the standing base pose behind the masked recovery legs, so
            // fading the slide does not reveal an unfinished crouch-to-stand transition.
            bool preparingSlideRecovery = _slideAnimations != null && _slideAnimations.LowerBodyOnly &&
                _traversal != null && _traversal.IsSliding && _traversal.Progress >= .65f &&
                _stance != null && _stance.CanStand;
            _animator.SetBool(Crouched, crouched && !preparingSlideRecovery);
            UpdateTraversal();
            if (_life != null) _animator.SetInteger("Life", (int)_life.State);
            if (_revive != null) _animator.SetBool("Reviving", _revive.IsReviving && (_stance == null || !_stance.IsProne));
        }
        private void UpdateTraversal()
        {
            int mode = 0;
            float time = 0f;
            if (_traversal != null && (_life == null || _life.State == InfantryLife.Alive))
            {
                float progress = _traversal.Progress;
                if (_traversal.IsVaulting) { mode = 4; time = progress; }
                else if (_traversal.IsSliding)
                {
                    if (_slideAnimations != null && _slideAnimations.LowerBodyOnly) { mode = 0; time = progress; _animator.SetFloat(SlideTime, progress); }
                    else if (_slideAnimations != null && _slideAnimations.IsContinuous) { mode = 2; time = progress; }
                    else if (progress < .25f) { mode = 1; time = progress / .25f; }
                    else if (progress < .8f) { mode = 2; time = (progress - .25f) / .55f; }
                    else { mode = 3; time = (progress - .8f) / .2f; }
                }
            }
            if (_mantle != null && _mantle.IsClimbing) mode = 5;
            _animator.SetInteger(TraversalMode, mode);
            _animator.SetFloat(TraversalTime, Mathf.Clamp01(time));
        }

        private void OnDisable()
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            _animator.SetInteger(TraversalMode, 0);
            _animator.SetFloat(TraversalTime, 0f);
        }
    }
}
