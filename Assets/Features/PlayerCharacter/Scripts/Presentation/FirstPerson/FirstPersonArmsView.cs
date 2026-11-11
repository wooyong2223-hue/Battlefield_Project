using System;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Copy the animated pose into a presentation rig without moving the world character or camera.
    [DefaultExecutionOrder(80)]
    public sealed class FirstPersonArmsView : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _source;
        [SerializeField] private SkinnedMeshRenderer _renderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _characterRoot;
        [SerializeField] private Transform _eye;
        [SerializeField] private Transform _poseAnchor;
        [SerializeField] private Vector3 _poseEuler;
        [SerializeField] private PlayerCharacterLookController _view;
        [SerializeField] private PlayerCharacterCombatController _combat;
        [SerializeField] private Vector3 _offset = new(.15f, .75f, .2f);
        [SerializeField] private Vector3 _adsOffset = new(.05f, .86f, .2f);
        [SerializeField] private PlayerCharacterCamera _motionSource;
        [SerializeField] private Vector3 _unarmedOffset = new(0f, -.7f, .25f);
        private PlayerCharacterCombatView _presentation;
        [SerializeField] private Mesh _meleeMesh;
        private Mesh _pistolMesh;
        private Transform[] _sourceBones, _bones;
        private Transform _sourceRightHand;
        [SerializeField] private PlayerCharacterRifleReloadView _reloadPose;
        [SerializeField] private Vector3 _adsShoulder = new(.45f, -.4f, -.1f);
        [SerializeField] private Vector3 _adsElbowHint = new(.65f, -.6f, .15f);
        private FirstPersonArmPose _rightArmPose, _leftArmPose;
        private Transform _reloadTorso;
        [SerializeField] private PlayerCharacterProneAnimationView _prone;
        [SerializeField] private PlayerCharacterTacticalAnimationView _tactical;
        public void SetTacticalPresentation(PlayerCharacterTacticalAnimationView view) => _tactical = view;
        public void SetPronePresentation(PlayerCharacterProneAnimationView view) => _prone = view;
        [SerializeField, Min(.05f)] private float _pronePistolAimDistance = 1.4f;
        private bool _hasReloadFrame;
        private int _reloadSequence;
        private Vector3 _reloadTorsoPosition;
        private Quaternion _reloadTorsoRotation, _reloadBasis;
        public Transform RightHand { get; private set; }

        private void Awake() => Initialize();

        public bool Initialize()
        {
            if (_bones != null) return RightHand != null;
            if (_source == null || _renderer == null || _animator == null || _characterRoot == null ||
                _eye == null || _poseAnchor == null || _view == null || _combat == null)
            {
                Debug.LogError("First person arms references are missing.", this); enabled = false; return false;
            }
            _pistolMesh = _renderer.sharedMesh;
            _presentation = _animator.GetComponent<PlayerCharacterCombatView>();
            _sourceBones = _source.bones;
            _bones = new Transform[_sourceBones.Length];
            for (int i = 0; i < _bones.Length; i++)
            {
                _bones[i] = new GameObject(_sourceBones[i].name).transform;
                _bones[i].SetParent(transform, false);
            }
            _renderer.bones = _bones;
            int rootIndex = Array.IndexOf(_sourceBones, _source.rootBone);
            _renderer.rootBone = rootIndex >= 0 ? _bones[rootIndex] : transform;
            int handIndex = Array.IndexOf(_sourceBones, _animator.GetBoneTransform(HumanBodyBones.RightHand));
            if (handIndex < 0)
            {
                Debug.LogError("First person arms require a right hand skin bone.", this); enabled = false; return false;
            }
            RightHand = _bones[handIndex];
            _sourceRightHand = _sourceBones[handIndex];
            _reloadTorso = _animator.GetBoneTransform(HumanBodyBones.UpperChest) ?? _animator.GetBoneTransform(HumanBodyBones.Chest);
            _rightArmPose = new FirstPersonArmPose(_animator, _sourceBones, _bones, true);
            _leftArmPose = new FirstPersonArmPose(_animator, _sourceBones, _bones, false);
            CopyPose();
            return true;
        }
        private void LateUpdate()
        {
            if (_bones != null && _combat.CanUseCombat && _view.IsFirstPersonView) { CopyPose(); if (_tactical != null) _tactical.UpdateHeldVisual(RightHand); }
            else _hasReloadFrame = false;
        }
        private void CopyPose()
        {
            Quaternion aim = _motionSource != null ? _motionSource.ResolvedAimRotation : _view.AimRotation;
            // Preserve the authored melee motion; pistol calibration is blended out
            // while the dedicated melee layer owns the upper-body pose.
            float melee = !_combat.IsArmed ? 1f : _presentation != null ? _presentation.MeleePoseWeight : _combat.IsAttacking ? 1f : 0f;
            if (_tactical != null && _tactical.IsPresenting) melee = 1f;
            bool nativeProne = _prone != null && _prone.IsPresenting;
            bool sourceGrip = _presentation.UsesItemGrip || (nativeProne && _prone.CanUseItemGrip);
            Mesh mesh = (melee > .01f || (nativeProne && !sourceGrip)) && _meleeMesh != null ? _meleeMesh : _pistolMesh;
            if (_renderer.sharedMesh != mesh) _renderer.sharedMesh = mesh;
            // Preserve authored melee torso motion in the character frame; camera aim adds the view pitch.
            var definition = _presentation != null ? _presentation.ActiveDefinition : null;
            bool custom = definition != null && definition.CustomGrip;
            Vector3 poseEuler = custom ? definition.FirstPersonEuler : _poseEuler;
            Quaternion weaponBasis = Quaternion.Euler(poseEuler) * Quaternion.Inverse(_poseAnchor.rotation);
            if ((nativeProne && sourceGrip) || (custom && (_presentation.UsesItemGrip || (_reloadPose != null && _reloadPose.IsActive))))
                weaponBasis = Quaternion.Inverse(_sourceRightHand.rotation * _presentation.GripRotation);
            Quaternion basis = Quaternion.Slerp(weaponBasis,
                Quaternion.Inverse(_characterRoot.rotation), melee);
            Quaternion rotation = aim * Quaternion.Euler(_motionSource != null ? _motionSource.FirstPersonWeaponEuler : Vector3.zero) * basis;
            float blend = _motionSource != null ? _motionSource.FirstPersonAimBlend : _view.IsAds ? 1f : 0f;
            Vector3 feedback = _motionSource != null ? _motionSource.FirstPersonWeaponOffset : Vector3.zero;
            Vector3 authoredOffset = Vector3.Lerp(Vector3.Lerp(custom ? definition.FirstPersonOffset : _offset, custom ? definition.FirstPersonAdsOffset : _adsOffset, blend), _unarmedOffset, melee);
            if (_tactical != null && _tactical.IsGrenadeEquipped) authoredOffset = _tactical.FirstPersonThrowOffset;
            Vector3 reloadOffset = custom && _reloadPose != null ? _reloadPose.ViewOffset : Vector3.zero;
            Vector3 offset = Vector3.Scale(authoredOffset + feedback + reloadOffset, _characterRoot.lossyScale);
            // _eye references the rendered camera, which already includes asset jump motion.
            Vector3 pivot = _eye.position + aim * offset;
            // Rifle data anchors its trigger hand in view space; the off hand still follows the authored action.
            Vector3 anchor = custom && melee <= .001f ? _sourceRightHand.position : _poseAnchor.position;
            float reloadWeight = _presentation != null ? _presentation.ReloadPoseWeight : 0f;
            Vector3 bodyOffset = Vector3.zero;
            Quaternion bodyRotation = Quaternion.identity;
            if (reloadWeight > 0f)
            {
                Quaternion torsoRotation = Quaternion.Inverse(_characterRoot.rotation) * _reloadTorso.rotation;
                Vector3 torsoPosition = _characterRoot.InverseTransformPoint(_reloadTorso.position);
                if (!_hasReloadFrame || _reloadSequence != _presentation.ReloadSequence)
                {
                    _reloadTorsoPosition = torsoPosition;
                    _reloadTorsoRotation = torsoRotation;
                    _reloadBasis = weaponBasis * _characterRoot.rotation;
                    _reloadSequence = _presentation.ReloadSequence;
                    _hasReloadFrame = true;
                }
                // Keep the calibrated view frame; carry both arms with a bounded common torso delta.
                Quaternion delta = _reloadBasis * torsoRotation * Quaternion.Inverse(_reloadTorsoRotation) * Quaternion.Inverse(_reloadBasis);
                delta = Quaternion.RotateTowards(Quaternion.identity, delta, 15f);
                bodyRotation = aim * Quaternion.Slerp(Quaternion.identity, delta, reloadWeight) * Quaternion.Inverse(aim);
                bodyOffset = aim * (_reloadBasis * Vector3.ClampMagnitude(torsoPosition - _reloadTorsoPosition, .2f)) * reloadWeight;
            }
            else _hasReloadFrame = false;
            Vector3 parentScale = transform.lossyScale;
            float viewScale = custom ? definition.FirstPersonScale : 1f;
            bodyOffset *= viewScale;
            Vector3 bodyPivot = pivot + rotation * ((_reloadTorso.position - anchor) * viewScale);
            if (custom && _reloadPose != null && _reloadPose.IsActive)
            {
                if (_reloadPose.IsLoweringRaisedHands && _reloadPose.Progress < .18f)
                {
                    Vector3 upward = aim * Vector3.up;
                    Vector3 bodyHand = bodyPivot + bodyRotation * (pivot - bodyPivot) + bodyOffset;
                    bodyOffset -= upward * Mathf.Max(0f, Vector3.Dot(bodyHand - pivot, upward));
                }
                bodyOffset += rotation * Vector3.ClampMagnitude(_reloadPose.HandSettleDelta, .2f) * viewScale;
            }
            if (nativeProne) { bodyRotation = Quaternion.identity; bodyOffset = Vector3.zero; }
            for (int i = 0; i < _bones.Length; i++)
            {
                Vector3 position = pivot + rotation * ((_sourceBones[i].position - anchor) * viewScale);
                position = bodyPivot + bodyRotation * (position - bodyPivot) + bodyOffset;
                _bones[i].SetPositionAndRotation(position, bodyRotation * rotation * _sourceBones[i].rotation);
                Vector3 scale = _sourceBones[i].lossyScale * viewScale;
                _bones[i].localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            }
            if (custom)
            {
                float weight = Mathf.Max(sourceGrip ? 1f : blend, _reloadPose != null ? _reloadPose.Blend : 0f) * (1f - melee);
                Vector3 scale = _characterRoot.lossyScale;
                Vector3 shoulder = Vector3.Scale(_adsShoulder, scale);
                Vector3 hint = Vector3.Scale(_adsElbowHint, scale);
                _rightArmPose.Apply(_eye.position + aim * shoulder, _eye.position + aim * hint, weight);
                shoulder.x = -shoulder.x; hint.x = -hint.x;
                _leftArmPose.Apply(_eye.position + aim * shoulder, _eye.position + aim * hint, weight);
            }
            if (nativeProne && sourceGrip && !custom && melee <= .001f)
                FitPronePistolAim(aim, blend);
        }
        private void FitPronePistolAim(Quaternion aim, float adsBlend)
        {
            var weapon = _presentation.ActiveWeapon;
            if (weapon == null || weapon.Muzzle == null) return;
            // Frame the current copied pose from the authored muzzle, not the prone torso anchor.
            // Use the resting grip transform so existing weapon recoil remains visible.
            Vector3 muzzleInModel = weapon.transform.InverseTransformPoint(weapon.Muzzle.position);
            Vector3 modelScale = Vector3.Scale(RightHand.lossyScale, weapon.transform.localScale);
            Vector3 restingMuzzle = RightHand.TransformPoint(_presentation.GripPosition) +
                (RightHand.rotation * _presentation.GripRotation) * Vector3.Scale(muzzleInModel, modelScale);
            Vector3 readyOffset = Vector3.Lerp(_offset - _adsOffset, Vector3.zero, adsBlend);
            Vector3 target = _eye.position + aim * Vector3.Scale(
                readyOffset + Vector3.forward * _pronePistolAimDistance, _characterRoot.lossyScale);
            Vector3 correction = target - restingMuzzle;
            for (int i = 0; i < _bones.Length; i++) _bones[i].position += correction;
        }
        private void OnDisable() { _hasReloadFrame = false; if (_renderer != null) _renderer.enabled = false; }
        private void OnDestroy()
        {
            if (_bones == null) return;
            foreach (Transform bone in _bones)
                if (bone != null) Destroy(bone.gameObject);
        }
    }
}







