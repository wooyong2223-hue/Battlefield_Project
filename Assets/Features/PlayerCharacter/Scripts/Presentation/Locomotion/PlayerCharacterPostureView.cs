using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Fit authored traversal poses and preserve the existing procedural prone presentation.
    public sealed class PlayerCharacterPostureView : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private Transform _root;
        [SerializeField] private CharacterController _body;
        [SerializeField] private InfantryLifeState _life;
        [SerializeField] private PlayerCharacterLean _lean;
        [SerializeField] private PlayerCharacterRevive _revive;
        [SerializeField] private PlayerCharacterTraversal _traversal;
        [SerializeField] private SkinnedMeshRenderer _skin;
        [SerializeField] private SkinnedPoseContactData _slideContactData;
        [SerializeField, Min(0f)] private float _bodyGroundClearance = .3f;
        [SerializeField, Min(0f)] private float _eliminatedGroundClearance = .25f;
        [SerializeField, Min(0f)] private float _reviveGroundClearance = 1.3f;
        [SerializeField, Min(.01f)] private float _blendSpeed = 8f;
        private float _blend;
        private float _reviveBlend;
        private Animator _animator;
        private Transform _spine;
        private Transform _hips;
        private SkinnedPoseContactData.Sampler _slideContact;
        [SerializeField] private PlayerCharacterProneAnimationView _authoredProne;
        public void SetPronePresentation(PlayerCharacterProneAnimationView view) => _authoredProne = view;
        private bool HasAuthoredProne
        {
            get { return _authoredProne != null && _authoredProne.IsPresenting; }
        }
        public float UpperBodyAimMultiplier => HasAuthoredProne ? 0f : 1f - _blend;
        private void Awake()
        {
            var animator = _animator = GetComponent<Animator>();
            _spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            _hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (_slideContactData != null) _slideContact = _slideContactData.CreateSampler(_skin);
        }
        public void Apply(Animator animator)
        {
            if (!isActiveAndEnabled || _stance == null || _root == null || _body == null) return;
            _blend = Mathf.MoveTowards(_blend, _stance.IsProne && !HasAuthoredProne && (_life == null || !_life.IsEliminated) ? 1f : 0f, _blendSpeed * Time.deltaTime);
            bool reviving = _revive != null && _revive.IsReviving && !_stance.IsProne && !HasAuthoredProne && (_life == null || _life.State == InfantryLife.Alive);
            _reviveBlend = Mathf.MoveTowards(_reviveBlend, reviving ? 1f : 0f, _blendSpeed * Time.deltaTime);
            if (_blend > 0f)
            {
                Vector3 position = animator.bodyPosition;
                float feet = _root.TransformPoint(_body.center).y - _body.height * Mathf.Abs(_root.lossyScale.y) * .5f;
                position.y = Mathf.Lerp(position.y, feet + _bodyGroundClearance * Mathf.Abs(_root.lossyScale.y), _blend);
                animator.bodyPosition = position;
                animator.bodyRotation = Quaternion.AngleAxis(90f * _blend, _root.right) * animator.bodyRotation;
            }
            if (_life != null && _life.IsEliminated)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("Base Layer.Eliminated"))
                {
                    Vector3 position = animator.bodyPosition;
                    float feet = _root.TransformPoint(_body.center).y - _body.height * Mathf.Abs(_root.lossyScale.y) * .5f;
                    float amount = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.1f, .8f, state.normalizedTime));
                    position.y = Mathf.Lerp(position.y, feet + _eliminatedGroundClearance * Mathf.Abs(_root.lossyScale.y), amount);
                    animator.bodyPosition = position;
                }
            }
            else if (_reviveBlend > 0f && !_stance.IsProne && !HasAuthoredProne && (_life == null || _life.State == InfantryLife.Alive))
            {
                Vector3 position = animator.bodyPosition;
                float feet = _root.TransformPoint(_body.center).y - _body.height * Mathf.Abs(_root.lossyScale.y) * .5f;
                position.y = Mathf.Lerp(position.y, feet + _reviveGroundClearance * Mathf.Abs(_root.lossyScale.y), _reviveBlend);
                animator.bodyPosition = position;
            }
            if (_lean != null && _spine != null && Mathf.Abs(_lean.Roll) > .001f)
            {
                Vector3 axis = _spine.parent.InverseTransformDirection(_root.forward);
                animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.AngleAxis(_lean.Roll, axis) * _spine.localRotation);
            }
        }
        private void LateUpdate()
        {
            if (_traversal == null || _hips == null || _slideContact == null ||
                _root == null || _body == null) return;
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            int lowerLayer = _animator.GetLayerIndex("LowerBodySlide");
            int entryLayer = _animator.GetLayerIndex("LowerBodySlideEntry");
            bool fadingLowerBody = (lowerLayer >= 0 && _animator.GetLayerWeight(lowerLayer) > .001f) ||
                (entryLayer >= 0 && _animator.GetLayerWeight(entryLayer) > .001f);
            if (!_traversal.IsSliding && !fadingLowerBody && !state.IsName("SlideStart") &&
                !state.IsName("SlideLoop") && !state.IsName("SlideExit")) return;
            float scale = Mathf.Abs(_root.lossyScale.y);
            float ground = _root.TransformPoint(_body.center).y - _body.height * scale * .5f;
            // Fit evaluated bones after IK; never move the character/model transforms.
            if (!_slideContact.TryGetLowestHeight(out float contact)) return;
            _hips.position += Vector3.up * (ground - contact);
        }
        private void OnDisable() { _blend = 0f; _reviveBlend = 0f; }
    }
}
