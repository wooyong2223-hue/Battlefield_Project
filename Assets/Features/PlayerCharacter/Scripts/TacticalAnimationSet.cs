using UnityEngine;
using System.Collections.Generic;
using Battlefield.Features.Weapon;
namespace Battlefield.Features.PlayerCharacter
{
    [CreateAssetMenu(menuName = "Battlefield/Character/Tactical Animations")]
    public sealed class TacticalAnimationSet : ScriptableObject
    {
        [SerializeField] private AnimationClip _mantlePlaceholder, _throwPlaceholder;
        [SerializeField] private AnimationClip _equipPlaceholder, _idlePlaceholder;
        [SerializeField] private AnimationClip _leftThrowPlaceholder;
        [SerializeField] private MantleDefinition _mantle;
        [SerializeField] private GrenadeDefinition _grenade;
        public MantleDefinition Mantle => _mantle;
        public GrenadeDefinition Grenade => _grenade;
        public bool HasLeftThrow => _leftThrowPlaceholder != null && _grenade != null && _grenade.LeftThrowAnimation != null;
        public bool IsValid => _mantlePlaceholder != null && _mantle != null && _mantle.Animation != null && _grenade != null && _grenade.Projectile != null &&
            (!_grenade.UseAnimations || (_throwPlaceholder != null && _grenade.ThrowAnimation != null && _grenade.HasHandlingAnimations && _equipPlaceholder != null && _idlePlaceholder != null));
        public bool IsApplied(RuntimeAnimatorController controller) => controller is AnimatorOverrideController overrides &&
            overrides[_mantlePlaceholder] == _mantle.Animation && (!_grenade.UseAnimations || (overrides[_throwPlaceholder] == _grenade.ThrowAnimation &&
            overrides[_equipPlaceholder] == _grenade.EquipAnimation && overrides[_idlePlaceholder] == _grenade.IdleAnimation &&
            (!HasLeftThrow || overrides[_leftThrowPlaceholder] == _grenade.LeftThrowAnimation)));
        public KeyValuePair<AnimationClip, AnimationClip>[] Read(AnimatorOverrideController controller)
        {
            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>
            {
                new KeyValuePair<AnimationClip, AnimationClip>(_mantlePlaceholder, controller[_mantlePlaceholder])
            };
            if (!_grenade.UseAnimations) return pairs.ToArray();
            pairs.AddRange(new[] {
                new KeyValuePair<AnimationClip, AnimationClip>(_throwPlaceholder, controller[_throwPlaceholder]),
                new KeyValuePair<AnimationClip, AnimationClip>(_equipPlaceholder, controller[_equipPlaceholder]),
                new KeyValuePair<AnimationClip, AnimationClip>(_idlePlaceholder, controller[_idlePlaceholder])
            });
            if (HasLeftThrow) pairs.Add(new KeyValuePair<AnimationClip, AnimationClip>(_leftThrowPlaceholder, controller[_leftThrowPlaceholder]));
            return pairs.ToArray();
        }
        public void Restore(AnimatorOverrideController controller, KeyValuePair<AnimationClip, AnimationClip>[] previous)
        {
            foreach (var pair in previous)
            {
                var applied = pair.Key == _mantlePlaceholder ? _mantle.Animation : pair.Key == _throwPlaceholder ? _grenade.ThrowAnimation :
                    pair.Key == _equipPlaceholder ? _grenade.EquipAnimation : pair.Key == _leftThrowPlaceholder ? _grenade.LeftThrowAnimation : _grenade.IdleAnimation;
                if (controller[pair.Key] == applied) controller[pair.Key] = pair.Value;
            }
        }
        public void Apply(AnimatorOverrideController controller)
        {
            controller[_mantlePlaceholder] = _mantle.Animation;
            if (!_grenade.UseAnimations) return;
            controller[_throwPlaceholder] = _grenade.ThrowAnimation;
            controller[_equipPlaceholder] = _grenade.EquipAnimation; controller[_idlePlaceholder] = _grenade.IdleAnimation;
            if (HasLeftThrow) controller[_leftThrowPlaceholder] = _grenade.LeftThrowAnimation;
        }
    }
}
