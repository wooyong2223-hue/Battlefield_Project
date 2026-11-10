using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    [CreateAssetMenu(menuName = "Battlefield/Character/Mantle")]
    public sealed class MantleDefinition : ScriptableObject
    {
        [SerializeField] private AnimationClip _animation;
        [SerializeField, Min(0f)] private float _minimumHeightFraction = .6f;
        [SerializeField, Min(0f)] private float _maximumHeightFraction = .9f;
        [SerializeField, Min(.01f)] private float _reach = 2.5f;
        [SerializeField, Min(.01f)] private float _reachDuration = .18f;
        public AnimationClip Animation => _animation;
        public float MinimumHeightFraction => _minimumHeightFraction;
        public float MaximumHeightFraction => Mathf.Max(_minimumHeightFraction, _maximumHeightFraction);
        public float Reach => _reach;
        public float ReachDuration => _reachDuration;
        public float Duration => _reachDuration + (_animation != null ? _animation.length : 1.13f);
    }
}
