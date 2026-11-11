using System.Collections.Generic;
using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(41)]
    public sealed class PlayerCharacterTacticalAnimationView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private TacticalAnimationSet _profile;
        [SerializeField] private PlayerCharacterMantle _mantle;
        [SerializeField] private PlayerCharacterGrenadeThrow _throw;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterLookController _view;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        private Transform _hand;
        private bool _wasClimbing;
        [SerializeField] private SkinnedPoseContactData _contactData;
        [SerializeField] private SkinnedMeshRenderer _skin;
        private Transform _hips;
        private CharacterController _body;
        private SkinnedPoseContactData.Sampler _contact;
        private readonly Dictionary<RuntimeAnimatorController, AnimatorOverrideController> _overrides = new();
        private RuntimeAnimatorController _source, _applied, _validated;
        private readonly Dictionary<AnimatorOverrideController, KeyValuePair<AnimationClip, AnimationClip>[]> _borrowed = new();
        private void Awake() => CachePose();
        private void CachePose()
        {
            if (_animator == null || _mantle == null) return;
            _hand = _animator.GetBoneTransform(HumanBodyBones.RightHand); _hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
            _body = _mantle.GetComponent<CharacterController>(); _contact = _contactData != null ? _contactData.CreateSampler(_skin) : null;
        }
        public bool IsGrenadeEquipped => _throw != null && _throw.IsEquipped;
        public bool IsThrowing => _throw != null && _throw.IsThrowing;
        public Vector3 FirstPersonThrowOffset => !_profile.Grenade.UseAnimations ? _profile.Grenade.HandlingFirstPersonOffset :
            Vector3.Lerp(_profile.Grenade.HandlingFirstPersonOffset, _profile.Grenade.FirstPersonOffset,
                _throw != null && _throw.State == GrenadeHandlingState.Throwing ? Mathf.SmoothStep(0f, 1f, _throw.Progress / .5f) : 0f);
        public bool IsPresenting => (_mantle != null && _mantle.IsBusy) || (_throw != null && _throw.IsEquipped);
        public void Initialize(Animator animator, TacticalAnimationSet profile, PlayerCharacterMantle mantle, PlayerCharacterGrenadeThrow grenade,
            PlayerCharacterStance stance, PlayerCharacterLookController view, PlayerCharacterActionPolicy actions, SkinnedPoseContactData contact, SkinnedMeshRenderer skin)
        { _animator = animator; _profile = profile; _mantle = mantle; _throw = grenade; _stance = stance; _view = view; _actions = actions; _contactData = contact; _skin = skin; CachePose(); }
        private void Update()
        {
            if (_animator == null || _profile == null || !_profile.IsValid) return;
            ApplyController();
            bool climbing = _mantle != null && _mantle.IsClimbing;
            int layer = _animator.GetLayerIndex("GrenadeUpperBody");
            if (layer >= 0) _animator.SetLayerWeight(layer, _throw != null ? _throw.PoseWeight : 0f);
            _animator.SetFloat("ThrowTime", _throw != null ? _throw.Progress : 0f);
            int leftLayer = _animator.GetLayerIndex("GrenadeLeftArm");
            if (leftLayer >= 0)
            {
                bool active = _profile.Grenade.UseAnimations && _profile.HasLeftThrow && _throw != null && _throw.State == GrenadeHandlingState.Throwing;
                float entry = active ? Mathf.SmoothStep(0f, 1f, _throw.Progress * _profile.Grenade.ThrowAnimation.length / .12f) : 0f;
                _animator.SetLayerWeight(leftLayer, active ? _throw.PoseWeight * entry : 0f);
            }
            _animator.SetFloat("GrenadeHandlingTime", _throw != null ? _throw.HandlingProgress : 0f);
            if (_profile.Grenade.UseAnimations && layer >= 0 && _throw != null && _throw.IsEquipped)
            {
                string state = _throw.State == GrenadeHandlingState.Equipping ? "Equip" :
                    _throw.State == GrenadeHandlingState.Ready ? "Idle" : "Throw";
                bool entering = _animator.IsInTransition(layer) && _animator.GetNextAnimatorStateInfo(layer).IsName(state);
                if (!_animator.GetCurrentAnimatorStateInfo(layer).IsName(state) && !entering)
                    _animator.CrossFadeInFixedTime("GrenadeUpperBody." + state, .12f, layer, 0f);
            }
            if (climbing) _animator.SetFloat("MantleTime", _mantle.AnimationProgress);
            if (climbing && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Mantle") &&
                (!_animator.IsInTransition(0) || !_animator.GetNextAnimatorStateInfo(0).IsName("Mantle")))
                _animator.CrossFadeInFixedTime("Base Layer.Mantle", .1f, 0, 0f);
            else if (!climbing && (_actions == null || _actions.IsAlive))
            {
                string target = _stance != null && _stance.IsCrouched ? "CrouchLocomotion" : "Locomotion";
                bool fadingToTarget = _animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).IsName(target);
                // Suspension clears our edge flag. Also inspect the state on resume so
                // an interrupted mantle cannot leave the Animator stuck after exiting a seat.
                bool mantlePose = _animator.GetCurrentAnimatorStateInfo(0).IsName("Mantle") ||
                    (_animator.IsInTransition(0) && _animator.GetNextAnimatorStateInfo(0).IsName("Mantle"));
                if ((_wasClimbing || mantlePose) && !fadingToTarget)
                    _animator.CrossFadeInFixedTime("Base Layer." + target, .12f, 0);
            }
            _wasClimbing = climbing;
        }
        private void ApplyController()
        {
            var current = _animator.runtimeAnimatorController;
            if (current == null || current == _validated || current == _applied) return;
            _validated = current;
            // Slide composition flattens controller chains: detect our clip mappings, not wrapper identity.
            if (_profile.IsApplied(current)) return;
            // Clip slots are disjoint from equipment/slide. Bind them in-place on an
            // existing disposable runtime controller to preserve its identity during suspension.
            if (current is AnimatorOverrideController runtime && (runtime.hideFlags & HideFlags.DontSave) != 0)
            {
                if (!_borrowed.ContainsKey(runtime)) _borrowed.Add(runtime, _profile.Read(runtime));
                _profile.Apply(runtime); return;
            }
            if (!_overrides.TryGetValue(current, out var composed))
            {
                RuntimeAnimatorController root = current; var chain = new List<AnimatorOverrideController>();
                while (root is AnimatorOverrideController existing) { chain.Add(existing); root = existing.runtimeAnimatorController; }
                composed = new AnimatorOverrideController(root) { name = current.name + " + Tactical", hideFlags = HideFlags.DontSave };
                var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                for (int i = chain.Count - 1; i >= 0; i--) { pairs.Clear(); chain[i].GetOverrides(pairs); foreach (var pair in pairs) if (pair.Value != null) composed[pair.Key] = pair.Value; }
                _profile.Apply(composed); _overrides.Add(current, composed);
            }
            _source = current; _applied = composed; _animator.runtimeAnimatorController = composed;
        }
        private void LateUpdate()
        {
            // Reuse contact fitting after native animation evaluation. Only the presentation
            // height moves; source limb rotations and the motor/capsule remain untouched.
            if (_mantle != null && _mantle.IsBusy && _hips != null && _body != null && _contact != null && _contact.TryGetLowestHeight(out float contact))
            {
                float ground = _body.transform.TransformPoint(_body.center).y - _body.height * Mathf.Abs(_body.transform.lossyScale.y) * .5f;
                _hips.position += Vector3.up * (ground - contact);
            }
            if (_view != null && !_view.IsFirstPersonView) UpdateHeldVisual(_hand);
        }
        public void UpdateHeldVisual(Transform hand) { if (_throw != null) _throw.AttachHeldVisual(hand); }
        private void OnDisable()
        {
            _wasClimbing = false;
            if (_animator == null) return;
            int layer = _animator.GetLayerIndex("GrenadeUpperBody"); if (layer >= 0) _animator.SetLayerWeight(layer, 0f);
            int leftLayer = _animator.GetLayerIndex("GrenadeLeftArm"); if (leftLayer >= 0) _animator.SetLayerWeight(leftLayer, 0f);
        }
        private void OnDestroy()
        {
            if (_animator != null && _animator.runtimeAnimatorController == _applied) _animator.runtimeAnimatorController = _source;
            foreach (var pair in _borrowed) if (pair.Key != null) _profile.Restore(pair.Key, pair.Value);
            _borrowed.Clear();
            foreach (var controller in _overrides.Values) Destroy(controller); _overrides.Clear();
        }
    }
}
