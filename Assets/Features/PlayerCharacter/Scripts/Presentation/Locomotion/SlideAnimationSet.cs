using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [CreateAssetMenu(menuName = "Battlefield/Animation/Slide Animation Set")]
    public sealed class SlideAnimationSet : ScriptableObject
    {
        [SerializeField] private AnimationClip _originalStart;
        [SerializeField] private AnimationClip _originalLoop;
        [SerializeField] private AnimationClip _originalExit;
        [SerializeField] private AnimationClip _start;
        [SerializeField] private AnimationClip _loop;
        [SerializeField] private AnimationClip _exit;
        [SerializeField] private bool _lowerBodyOnly;
        public bool LowerBodyOnly => _lowerBodyOnly;
        public bool UsesForwardEntry => LowerBodyOnly && _start != null && _start != _loop && _loop == _exit;
        public AnimationClip Start => _start;
        public AnimationClip Loop => _loop;
        public AnimationClip Exit => _exit;
        public bool IsContinuous => _start != null && _start == _loop && _loop == _exit;
        public bool IsValid => _originalStart != null && _originalLoop != null && _originalExit != null &&
            _start != null && _loop != null && _exit != null;

        internal bool TryApply(AnimatorOverrideController controller)
        {
            if (!IsValid) return false;
            var clips = new List<KeyValuePair<AnimationClip, AnimationClip>>(controller.overridesCount);
            controller.GetOverrides(clips);
            int found = 0;
            for (int i = 0; i < clips.Count; i++)
            {
                AnimationClip replacement = clips[i].Key == _originalStart ? _start :
                    clips[i].Key == _originalLoop ? _loop : clips[i].Key == _originalExit ? _exit : null;
                if (replacement == null) continue;
                clips[i] = new KeyValuePair<AnimationClip, AnimationClip>(clips[i].Key, replacement); found++;
            }
            if (found != 3) return false;
            controller.ApplyOverrides(clips); return true;
        }
    }
}
