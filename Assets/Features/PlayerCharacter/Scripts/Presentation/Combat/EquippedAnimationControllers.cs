using System;
using System.Collections.Generic;
using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    // Owns equipment-specific controller compositions; never owns movement or combat state.
    internal sealed class EquippedAnimationControllers : IDisposable
    {
        private readonly Dictionary<(RuntimeAnimatorController, RuntimeAnimatorController, AnimationClip, AnimationClip),
            AnimatorOverrideController> _cache = new();
        public RuntimeAnimatorController Resolve(RuntimeAnimatorController movement, RuntimeAnimatorController equipment,
            AnimationClip reloadSlot, AnimationClip reloadClip)
        {
            if (equipment == null || (movement == null && (reloadSlot == null || reloadClip == null))) return equipment;
            var key = (movement, equipment, reloadSlot, reloadClip);
            if (_cache.TryGetValue(key, out var saved)) return saved;
            var movementChain = new List<AnimatorOverrideController>();
            while (movement is AnimatorOverrideController movementOverride)
            { movementChain.Add(movementOverride); movement = movementOverride.runtimeAnimatorController; }
            string name = equipment.name;
            var chain = new List<AnimatorOverrideController>();
            while (equipment is AnimatorOverrideController overrides) { chain.Add(overrides); equipment = overrides.runtimeAnimatorController; }
            var composed = new AnimatorOverrideController(movement != null ? movement : equipment)
                { name = name + " (Equipped)", hideFlags = HideFlags.DontSave };
            var available = new List<KeyValuePair<AnimationClip, AnimationClip>>(); composed.GetOverrides(available);
            var keys = new HashSet<AnimationClip>(); foreach (var pair in available) keys.Add(pair.Key);
            var changes = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            void Apply(List<AnimatorOverrideController> overridesChain)
            {
                for (int i = overridesChain.Count - 1; i >= 0; i--)
                {
                    changes.Clear(); overridesChain[i].GetOverrides(changes);
                    foreach (var pair in changes) if (pair.Value != null && keys.Contains(pair.Key)) composed[pair.Key] = pair.Value;
                }
            }
            Apply(movementChain); Apply(chain);
            if (reloadSlot != null && reloadClip != null && keys.Contains(reloadSlot))
                composed[reloadSlot] = reloadClip;
            _cache.Add(key, composed); return composed;
        }
        public void Dispose()
        {
            foreach (var controller in _cache.Values)
                if (controller != null) { if (Application.isPlaying) UnityEngine.Object.Destroy(controller); else UnityEngine.Object.DestroyImmediate(controller); }
            _cache.Clear();
        }
    }
}
