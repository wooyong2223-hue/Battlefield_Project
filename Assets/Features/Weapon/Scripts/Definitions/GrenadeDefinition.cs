using UnityEngine;
using Battlefield.Features.Projectile;
namespace Battlefield.Features.Weapon
{
    [CreateAssetMenu(menuName = "Battlefield/Weapons/Grenade")]
    public sealed class GrenadeDefinition : ScriptableObject
    {
        [SerializeField] private GrenadeProjectile _projectile;
        [SerializeField] private bool _useAnimations;
        [SerializeField] private AnimationClip _throwAnimation;
        [SerializeField] private AnimationClip _leftThrowAnimation;
        [SerializeField] private AnimationClip _equipAnimation, _idleAnimation;
        [SerializeField, Min(.01f)] private float _fuse = 3f;
        [SerializeField, Min(0f)] private float _damage = 100f;
        [SerializeField, Min(.01f)] private float _radius = 6f;
        [SerializeField, Min(0f)] private float _launchSpeed = 18f;
        [SerializeField, Min(0f)] private float _upwardSpeed = 4f;
        [SerializeField, Min(0f)] private float _underhandLaunchSpeed = 8f;
        [SerializeField, Min(0f)] private float _underhandUpwardSpeed = 1.5f;
        [SerializeField] private float _releaseFraction = .7f;
        [SerializeField] private Vector3 _firstPersonOffset = new(-.35f, .6f, 1.2f);
        [SerializeField] private Vector3 _handlingFirstPersonOffset = new(-.1f, -.4f, 1.2f);
        public Vector3 HandlingFirstPersonOffset => _handlingFirstPersonOffset;
        public Vector3 FirstPersonOffset => _firstPersonOffset;
        public GrenadeProjectile Projectile => _projectile;
        public bool UseAnimations => _useAnimations;
        public AnimationClip ThrowAnimation => _throwAnimation;
        public AnimationClip LeftThrowAnimation => _leftThrowAnimation;
        public AnimationClip EquipAnimation => _equipAnimation;
        public AnimationClip IdleAnimation => _idleAnimation;
        public bool HasHandlingAnimations => _equipAnimation != null && _idleAnimation != null;
        public float Fuse => Mathf.Max(.01f, _fuse);
        public float Damage => Mathf.Max(0f, _damage);
        public float Radius => Mathf.Max(.01f, _radius);
        public float LaunchSpeed => Mathf.Max(0f, _launchSpeed);
        public float UpwardSpeed => Mathf.Max(0f, _upwardSpeed);
        public float UnderhandLaunchSpeed => Mathf.Max(0f, _underhandLaunchSpeed);
        public float UnderhandUpwardSpeed => Mathf.Max(0f, _underhandUpwardSpeed);
        public float ReleaseFraction => Mathf.Clamp01(_releaseFraction);
    }
}
