using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Compose traversal clips over the current equipment controller without owning movement or ammo.
    [DefaultExecutionOrder(42)]
    public sealed class PlayerCharacterSlideAnimationView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerCharacterCombatController _combat;
        [SerializeField] private SlideAnimationSet _animations;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        private const string LowerBodyLayer = "LowerBodySlide";
        private const string EntryLayer = "LowerBodySlideEntry";
        private static readonly int EntryTime = Animator.StringToHash("SlideEntryTime");
        private float _lowerBodyWeight;
        private readonly Dictionary<RuntimeAnimatorController, AnimatorOverrideController> _controllers = new();
        private RuntimeAnimatorController _source;
        private AnimatorOverrideController _applied;
        private bool _subscribed;
        public SlideAnimationSet Animations => _animations;

        public void Initialize(Animator animator, PlayerCharacterCombatController combat, SlideAnimationSet animations, PlayerCharacterTraversal traversal)
        {
            if (_animator == animator && _combat == combat && _animations == animations && _traversal == traversal)
            { if (isActiveAndEnabled) Bind(); return; }
            Unbind(); ResetLowerBody(); Restore(); ClearControllers();
            _animator = animator; _combat = combat; _animations = animations; _traversal = traversal;
            if (isActiveAndEnabled) Bind();
        }
        private void OnEnable() => Bind();
        private void Bind()
        {
            if (!Application.isPlaying || _animator == null || _combat == null || _animations == null || !_animations.IsValid) return;
            if (!_subscribed) { _combat.EquipmentChanged += OnEquipment; _subscribed = true; }
            Apply();
        }
        private void OnEquipment(bool armed) => Apply();
        private void Update() { Apply(); UpdateLowerBody(); }
        private void UpdateLowerBody()
        {
            if (_animator == null) return;
            int layer = _animator.GetLayerIndex(LowerBodyLayer);
            if (layer < 0) return;
            bool sliding = _animations != null && _animations.LowerBodyOnly && _traversal != null && _traversal.IsSliding;
            _lowerBodyWeight = Mathf.MoveTowards(_lowerBodyWeight, sliding ? 1f : 0f, Time.deltaTime * 12.5f);
            // Keep the existing fade duration and interruption behavior; ease its
            // contribution so recovery does not begin/end with a weight velocity jump.
            float weight = Mathf.SmoothStep(0f, 1f, _lowerBodyWeight);
            int entryLayer = _animator.GetLayerIndex(EntryLayer);
            if (_animations == null || !_animations.UsesForwardEntry || entryLayer < 0)
            {
                _animator.SetLayerWeight(layer, weight);
                if (entryLayer >= 0) _animator.SetLayerWeight(entryLayer, 0f);
                return;
            }
            // Preserve the outgoing pose while the layer fades instead of rewinding to frame zero.
            float progress = _animator.GetFloat("SlideTime");
            _animator.SetFloat(EntryTime, Mathf.Clamp01(progress / .5f));
            float original = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.5f, .8f, progress));
            float entry = weight * (1f - original);
            // Entry overrides the original layer below it. Compensate the lower weight so
            // final contributions are base=(1-fade), entry=fade*(1-blend), original=fade*blend.
            float remaining = 1f - entry;
            _animator.SetLayerWeight(layer, remaining > .0001f ? weight * original / remaining : 0f);
            _animator.SetLayerWeight(entryLayer, entry);
        }
        private void ResetLowerBody()
        {
            _lowerBodyWeight = 0f;
            if (_animator == null) return;
            int layer = _animator.GetLayerIndex(LowerBodyLayer);
            if (layer >= 0) _animator.SetLayerWeight(layer, 0f);
            int entry = _animator.GetLayerIndex(EntryLayer);
            if (entry >= 0) _animator.SetLayerWeight(entry, 0f);
        }

        private void Apply()
        {
            if (_animator == null || _animations == null || !_animations.IsValid) { Restore(); return; }
            var current = _animator.runtimeAnimatorController;
            if (current == null || current == _applied) return;
            if (!_controllers.TryGetValue(current, out var composed))
            {
                var chain = new List<AnimatorOverrideController>();
                RuntimeAnimatorController root = current;
                while (root is AnimatorOverrideController overrides) { chain.Add(overrides); root = overrides.runtimeAnimatorController; }
                composed = new AnimatorOverrideController(root) { name = current.name + " + Slide", hideFlags = HideFlags.DontSave };
                var clips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                for (int i = chain.Count - 1; i >= 0; i--)
                {
                    clips.Clear(); chain[i].GetOverrides(clips);
                    foreach (var pair in clips) if (pair.Value != null) composed[pair.Key] = pair.Value;
                }
                if (!_animations.TryApply(composed)) { Release(composed); composed = null; }
                _controllers.Add(current, composed);
            }
            if (composed == null) return;
            _source = current; _applied = composed; _animator.runtimeAnimatorController = composed;
        }
        private void Restore()
        {
            if (_animator != null && _applied != null && _animator.runtimeAnimatorController == _applied)
                _animator.runtimeAnimatorController = _source;
            _applied = null; _source = null;
        }
        private void Unbind()
        {
            if (_subscribed && _combat != null) _combat.EquipmentChanged -= OnEquipment;
            _subscribed = false;
        }
        private void ClearControllers()
        {
            foreach (var item in _controllers.Values) if (item != null) Release(item);
            _controllers.Clear();
        }
        private static void Release(Object value)
        { if (Application.isPlaying) Destroy(value); else DestroyImmediate(value); }
        // Temporary suspension must not rebind an Animator while its seat pose is being captured.
        // Reinitialization/destruction restores ownership; resuming reuses the same controller.
        private void OnDisable() { Unbind(); ResetLowerBody(); }
        private void OnDestroy() { Unbind(); ResetLowerBody(); Restore(); ClearControllers(); }
    }
}
