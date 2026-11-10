using UnityEngine;
using Battlefield.Features.Weapon;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(45)]
    public sealed class PlayerCharacterWeaponAimView : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterController _combat;
        [SerializeField] private PlayerCharacterCombatView _presentation;
        [SerializeField] private PlayerCharacterView _view;
        [SerializeField] private PlayerCharacterCamera _camera;
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerCharacterPostureView _posture;
        [SerializeField] private PlayerCharacterWeaponMount _mount;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private Vector3 _rightHandOffset = new(.35f, -.4f, .9f);
        [SerializeField] private Vector3 _leftHandOffset;
        [SerializeField] private Vector3 _leftHandEuler;
        [Header("Upper body aiming")]
        [SerializeField] private float _bodyAimWeight = .3f;
        [SerializeField] private float _headAimWeight = .85f;
        [SerializeField] private float _lookAtClamp = .2f;
        [SerializeField, Min(.01f)] private float _poseFollowSpeed = 12f;
        [SerializeField, Min(0f)] private float _minimumDownwardReach = .35f;

        private Transform _rightHand, _leftHand;
        private Transform _rightUpperArm, _leftUpperArm;
        private Transform _rightElbow, _leftElbow;
        private float _rightReach, _leftReach;
        private int _layer, _meleeLayer;
        private Transform _meleeSpine, _meleeChest;
        private Transform _locomotionHead;
        private Quaternion _chestReference;
        private Vector3 _reloadEyeToTorso, _reloadGripCorrection;
        private InfantryWeaponDefinition _reloadReferenceDefinition;
        private Quaternion _reloadReferenceRotation;
        private CharacterController _body;
        private PlayerCharacterLean _lean;
        private Vector3 _rightGripShoulder, _leftGripShoulder;
        private const float ArmedTorsoPitchLimit = 3f;
        private const float LocomotionTorsoYawLimit = 3f;
        private const float LocomotionTorsoRollLimit = 3f;
        private static readonly int GaitSpeed = Animator.StringToHash("Speed");
        private HumanBodyBones _meleeChestId;
        private Quaternion _sampleRightGoal, _sampleLeftGoal;
        private Quaternion _rightGoalOffset, _leftGoalOffset;
        private bool _captureMapping, _mappingReady;
        private Quaternion _aimRotation;
        private float _bodyBlend;

        [SerializeField] private PlayerCharacterProneAnimationView _prone;
        [SerializeField] private PlayerCharacterTacticalAnimationView _tactical;
        public void SetTacticalPresentation(PlayerCharacterTacticalAnimationView view) => _tactical = view;
        public void SetPronePresentation(PlayerCharacterProneAnimationView view) => _prone = view;
        private bool _wasProne;
        private Quaternion _proneGripCorrection;

        private void Awake()
        {
            if (_combat == null || _presentation == null || _view == null || _camera == null ||
                _animator == null || !_animator.isHuman || (_layer = _animator.GetLayerIndex("UpperCombat")) < 0)
            {
                Debug.LogError("Player weapon aim references are missing.", this);
                enabled = false;
                return;
            }
            _lean = _combat.GetComponent<PlayerCharacterLean>();
            _meleeLayer = _animator.GetLayerIndex("MeleeCombat");
            _meleeSpine = _animator.GetBoneTransform(HumanBodyBones.Spine);
            _meleeChestId = _animator.GetBoneTransform(HumanBodyBones.UpperChest) != null ? HumanBodyBones.UpperChest : HumanBodyBones.Chest;
            _meleeChest = _animator.GetBoneTransform(_meleeChestId);
            _chestReference = _meleeChest != null
                ? Quaternion.Inverse(_combat.transform.rotation) * _meleeChest.rotation : Quaternion.identity;
            _body = _combat.GetComponent<CharacterController>();
            _reloadReferenceRotation = _chestReference;
            _reloadEyeToTorso = _combat.transform.InverseTransformVector(_camera.EyePosition - _meleeChest.position);
            _locomotionHead = _animator.GetBoneTransform(HumanBodyBones.Head);
            _rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _rightUpperArm = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            _leftUpperArm = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Transform rightElbow = _rightElbow = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform leftElbow = _leftElbow = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            _rightReach = Vector3.Distance(_rightUpperArm.position, rightElbow.position) + Vector3.Distance(rightElbow.position, _rightHand.position);
            _leftReach = Vector3.Distance(_leftUpperArm.position, leftElbow.position) + Vector3.Distance(leftElbow.position, _leftHand.position);
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (_tactical != null && _tactical.IsPresenting)
            { SetWeight(0f); if (layerIndex == 0) _animator.SetLookAtWeight(0f); return; }
            if (layerIndex == _meleeLayer)
            {
                // Include intended view pitch before measuring the final shoulder
                // line: pitch can otherwise reintroduce yaw/roll on a turned torso.
                bool addPitch = _combat.CanUseCombat && !_view.IsFirstPersonView && _presentation.MeleePoseWeight > .01f;
                float meleePitch = addPitch
                    ? Mathf.DeltaAngle(0f, _aimRotation.eulerAngles.x) * (_posture != null ? _posture.UpperBodyAimMultiplier : 1f)
                    : 0f;
                Vector2 gaitCorrection = !_combat.IsArmed && _presentation.MeleePoseWeight > .01f ? LocomotionTorsoCorrection(meleePitch) : Vector2.zero;
                if (addPitch || gaitCorrection.sqrMagnitude > .000001f)
                    ApplyTorsoRotation(meleePitch, gaitCorrection);
                return;
            }
            if (layerIndex == 0)
            {
                if (_posture != null) _posture.Apply(_animator);
                // Capture once: the torso and both hands must use the same aim this evaluation.
                _aimRotation = _view.AimRotation;
                ApplyUpperBodyAim();
                return;
            }
            if (layerIndex != _layer) return;
            _rightGripShoulder = _rightUpperArm.position;
            _leftGripShoulder = _leftUpperArm.position;
            if (CanStabilizeArmedTorso) StabilizeArmedTorso();
            else
            {
                if (_combat.IsArmed || _presentation.MeleePoseWeight <= .01f) StabilizeLocomotionTorso();
                _rightGripShoulder = _rightUpperArm.position;
                _leftGripShoulder = _leftUpperArm.position;
            }
            SetWeight(0f);
            if (!_mappingReady)
            {
                // Unity's IK hand rotations include an avatar-specific bone-axis offset.
                // Apply a known goal rotation once and pair it with the evaluated bones in LateUpdate.
                _sampleRightGoal = Quaternion.identity;
                _sampleLeftGoal = Quaternion.identity;
                _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
                _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                _animator.SetIKRotation(AvatarIKGoal.RightHand, _sampleRightGoal);
                _animator.SetIKRotation(AvatarIKGoal.LeftHand, _sampleLeftGoal);
                _captureMapping = true;
                return;
            }
            if (!_presentation.UsesItemGrip) return;
            // Rifles use the same calibrated source grip in either perspective.
            // The eye-relative copy still owns their rendered first-person placement.
            if (_view.IsFirstPersonView && (_presentation.ActiveDefinition == null || !_presentation.ActiveDefinition.CustomGrip)) return;

            CalculateGrip(_presentation.WeaponPoseRotation(_aimRotation), _presentation.WeaponForwardScale,
                _presentation.WeaponVerticalOffset, _presentation.RecoilAmount, out var rightPosition, out var rightRotation, out var reachScale);
            ApplyGrip(rightPosition, rightRotation, reachScale);
        }

        // Use the existing aim offsets with a non-jump torso reference, not a transient arm pose.
        public void GetReloadGrip(out Vector3 positionInTorso, out Quaternion rotationInTorso)
        {
            CalculateGrip(_presentation.WeaponPoseRotation(_view.AimRotation), 1f, 0f, 0f,
                out var position, out var rotation, out _);
            if (_reloadReferenceDefinition == _presentation.ActiveDefinition)
                position += _combat.transform.TransformVector(_reloadGripCorrection);
            Vector3 torsoPosition = _camera.EyePosition - _combat.transform.TransformVector(_reloadEyeToTorso);
            Quaternion torsoRotation = _combat.transform.rotation * _reloadReferenceRotation;
            var reference = Matrix4x4.TRS(torsoPosition, torsoRotation, _meleeChest.lossyScale);
            positionInTorso = reference.inverse.MultiplyPoint3x4(position);
            rotationInTorso = Quaternion.Inverse(torsoRotation) * rotation;
        }

        private void CalculateGrip(Quaternion aim, float forwardScale, float verticalOffset, float recoil,
            out Vector3 rightPosition, out Quaternion rightRotation, out float reachScale)
        {
            float pitch = Mathf.DeltaAngle(0f, aim.eulerAngles.x);
            float steepness = Mathf.InverseLerp(45f, 80f, Mathf.Abs(pitch));
            Vector3 offset = new Vector3(_rightHandOffset.x, 0f,
                _rightHandOffset.z * forwardScale * Mathf.Lerp(1f, .75f, steepness) - recoil * .03f);
            Vector3 scale = _combat.transform.lossyScale;
            // Pivot around shoulder height: rotating the vertical eye offset would put hands behind the body when looking down.
            rightPosition = _camera.EyePosition + _combat.transform.up * ((_rightHandOffset.y + verticalOffset) * scale.y) +
                                    aim * Vector3.Scale(offset, scale);
            if (pitch > 0f)
            {
                Vector3 forward = _combat.transform.forward;
                float reach = Vector3.Dot(rightPosition - _combat.transform.position, forward);
                rightPosition += forward * Mathf.Max(0f, _minimumDownwardReach * scale.z - reach);
            }
            rightRotation = aim * Quaternion.Inverse(_presentation.GripRotation);
            if (_mount != null && _mount.IsMounted)
                rightPosition = _mount.Contact + Vector3.up * .15f - aim * Vector3.forward * .35f;
            reachScale = Mathf.Lerp(1f, .92f, Mathf.InverseLerp(0f, 80f, Mathf.Abs(pitch)));
        }

        private void GetSupportPose(out Vector3 offset, out Quaternion rotation)
        {
            if (_presentation.TryGetSupportHandPose(out offset, out rotation)) return;
            offset = _leftHandOffset; rotation = Quaternion.Euler(_leftHandEuler);
        }
        private void ApplyGrip(Vector3 rightPosition, Quaternion rightRotation, float reachScale)
        {
            GetSupportPose(out var supportHandOffset, out var supportHandRotation);
            Vector3 supportOffset = rightRotation * Vector3.Scale(supportHandOffset, _rightHand.lossyScale);
            // One shared grip must fit both arms; independent unreachable goals separate the hands.
            for (int i = 0; i < 16; i++)
            {
                rightPosition = WithinReach(rightPosition, _rightGripShoulder, _rightReach * reachScale);
                rightPosition = WithinReach(rightPosition, _leftGripShoulder - supportOffset, _leftReach * reachScale);
            }
            SetWeight(1f);
            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1f);
            _animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1f);
            _animator.SetIKHintPosition(AvatarIKHint.RightElbow, _rightGripShoulder + _combat.transform.right * .5f - _combat.transform.up * .6f);
            _animator.SetIKHintPosition(AvatarIKHint.LeftElbow, _leftGripShoulder - _combat.transform.right * .5f - _combat.transform.up * .6f);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, rightPosition);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, rightRotation * _rightGoalOffset);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, rightPosition + supportOffset);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand,
                rightRotation * supportHandRotation * _leftGoalOffset);
        }

        private bool CanStabilizeArmedTorso => _presentation.UsesItemGrip &&
            (_posture == null || _posture.UpperBodyAimMultiplier > .99f) &&
            _animator.GetInteger("TraversalMode") == 0;

        private void StabilizeArmedTorso()
        {
            if (_meleeSpine == null || _meleeChest == null) return;
            Quaternion root = _combat.transform.rotation;
            Quaternion local = Quaternion.Inverse(root) * _meleeChest.rotation * Quaternion.Inverse(_chestReference);
            Vector3 angles = local.eulerAngles;
            float pitch = Mathf.DeltaAngle(0f, angles.x);
            float intendedPitch = Mathf.DeltaAngle(0f, _aimRotation.eulerAngles.x) * Mathf.Clamp01(_bodyAimWeight);
            float targetPitch = intendedPitch + CompressGaitAngle(pitch - intendedPitch, ArmedTorsoPitchLimit);
            Vector3 up = _combat.transform.up;
            Vector3 shoulderLine = _rightGripShoulder - _leftGripShoulder;
            Vector3 planar = Vector3.ProjectOnPlane(shoulderLine, up);
            if (planar.sqrMagnitude < .0001f) return;
            float shoulderYaw = Vector3.SignedAngle(_combat.transform.right, planar, up);
            float shoulderRoll = Mathf.Atan2(Vector3.Dot(shoulderLine, up), planar.magnitude) * Mathf.Rad2Deg;
            float targetYaw = CompressGaitAngle(shoulderYaw, LocomotionTorsoYawLimit);
            float intendedRoll = _lean != null ? _lean.Roll : 0f;
            float targetRoll = intendedRoll + CompressGaitAngle(shoulderRoll - intendedRoll, LocomotionTorsoRollLimit);
            Quaternion yawFrame = Quaternion.AngleAxis(targetYaw, up);
            Vector3 targetRight = Quaternion.AngleAxis(targetRoll, yawFrame * _combat.transform.forward) *
                (yawFrame * _combat.transform.right);
            Quaternion align = Quaternion.FromToRotation(shoulderLine, targetRight);
            Quaternion anatomical = _meleeChest.rotation * Quaternion.Inverse(_chestReference);
            Vector3 alignedForward = align * (anatomical * Vector3.forward);
            Vector3 alignedUp = align * (anatomical * Vector3.up);
            // After aligning the shoulder line, twist around that line to reach the
            // desired aim pitch without undoing shoulder yaw/roll. Rodrigues' formula
            // gives A*cos(t)+B*sin(t)+C for the forward vector's vertical component.
            float projection = Vector3.Dot(targetRight, alignedForward);
            float c = Vector3.Dot(targetRight, up) * projection;
            float a = Vector3.Dot(alignedForward, up) - c;
            float b = Vector3.Dot(Vector3.Cross(targetRight, alignedForward), up);
            float amplitude = Mathf.Sqrt(a * a + b * b);
            Quaternion twist = Quaternion.identity;
            if (amplitude > .0001f)
            {
                float phase = Mathf.Atan2(b, a);
                float arc = Mathf.Acos(Mathf.Clamp((-Mathf.Sin(targetPitch * Mathf.Deg2Rad) - c) / amplitude, -1f, 1f));
                Quaternion first = Quaternion.AngleAxis((phase + arc) * Mathf.Rad2Deg, targetRight);
                Quaternion second = Quaternion.AngleAxis((phase - arc) * Mathf.Rad2Deg, targetRight);
                twist = Vector3.Dot(first * alignedUp, up) >= Vector3.Dot(second * alignedUp, up) ? first : second;
            }
            Quaternion correction = twist * align;
            Quaternion half = Quaternion.Slerp(Quaternion.identity, correction, .5f);
            Vector3 chestPivot = _meleeSpine.position + half * (_meleeChest.position - _meleeSpine.position);
            // IK runs after these bone rotations. Fit goals to the resulting shoulder
            // positions rather than the pre-stabilization animation pose.
            _rightGripShoulder = chestPivot + correction * (_rightGripShoulder - _meleeChest.position);
            _leftGripShoulder = chestPivot + correction * (_leftGripShoulder - _meleeChest.position);
            RotateBoneWorld(HumanBodyBones.Spine, _meleeSpine, half);
            RotateBoneWorld(_meleeChestId, _meleeChest, half);
            // Keep the existing look-at result while stabilizing the torso underneath it.
            RotateBoneWorld(HumanBodyBones.Head, _locomotionHead, Quaternion.Inverse(correction));
        }

        private void StabilizeLocomotionTorso()
        {
            Vector2 correction = LocomotionTorsoCorrection();
            if (correction.sqrMagnitude > .000001f) ApplyTorsoRotation(0f, correction);
        }

        private Vector2 LocomotionTorsoCorrection(float intendedPitch = 0f)
        {
            if (!_combat.CanUseCombat || _combat.IsAttacking ||
                (_combat.Magazine != null && _combat.Magazine.IsReloading) ||
                (_actions != null && !_actions.CanAttack)) return Vector2.zero;
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (!state.IsName("Locomotion") && !state.IsName("CrouchLocomotion")) return Vector2.zero;
            if (!_animator.GetBool("Grounded") || (_posture != null && _posture.UpperBodyAimMultiplier < .99f)) return Vector2.zero;
            float weight = Mathf.InverseLerp(.05f, .3f, _animator.GetFloat(GaitSpeed));
            if (weight <= 0f || _meleeSpine == null || _meleeChest == null) return Vector2.zero;
            Vector3 up = _combat.transform.up;
            Vector3 shoulderRight = Quaternion.AngleAxis(intendedPitch, _combat.transform.right) *
                                    (_rightUpperArm.position - _leftUpperArm.position);
            Vector3 planar = Vector3.ProjectOnPlane(shoulderRight, up);
            if (planar.sqrMagnitude < .0001f) return Vector2.zero;
            float yaw = Vector3.SignedAngle(_combat.transform.right, planar, up);
            float roll = Mathf.Atan2(Vector3.Dot(shoulderRight, up), planar.magnitude) * Mathf.Rad2Deg;
            // Keep a small continuous gait motion on both axes. These are project
            // tuning targets, not joint angles measured from reference footage.
            float targetYaw = CompressGaitAngle(yaw, LocomotionTorsoYawLimit);
            float targetRoll = CompressGaitAngle(roll, LocomotionTorsoRollLimit);
            return new Vector2(targetYaw - yaw, targetRoll - roll) * weight;
        }

        private static float CompressGaitAngle(float angle, float limit) =>
            limit * (2f / Mathf.PI) * Mathf.Atan(angle / limit);

        private void ApplyTorsoRotation(float pitch, Vector2 correction)
        {
            Quaternion pitchRotation = Quaternion.AngleAxis(pitch, _combat.transform.right);
            Quaternion stabilization = Quaternion.AngleAxis(correction.y, _combat.transform.forward) *
                                       Quaternion.AngleAxis(correction.x, _combat.transform.up);
            // The same world-space half rotation on both joints composes to the
            // full correction without one axis overwriting another in this IK pass.
            Quaternion half = Quaternion.Slerp(Quaternion.identity, stabilization * pitchRotation, .5f);
            RotateBoneWorld(HumanBodyBones.Spine, _meleeSpine, half);
            RotateBoneWorld(_meleeChestId, _meleeChest, half);
            if (correction.sqrMagnitude <= .000001f) return;
            // Cancel stabilization at the head while retaining the intended pitch.
            Quaternion preserveGaze = Quaternion.Inverse(pitchRotation) * Quaternion.Inverse(stabilization) * pitchRotation;
            RotateBoneWorld(HumanBodyBones.Head, _locomotionHead, preserveGaze);
        }

        private void RotateBoneWorld(HumanBodyBones id, Transform bone, Quaternion rotation)
        {
            if (bone == null) return;
            Quaternion parent = bone.parent.rotation;
            _animator.SetBoneLocalRotation(id, Quaternion.Inverse(parent) * rotation * parent * bone.localRotation);
        }

        private static Vector3 WithinReach(Vector3 target, Vector3 shoulder, float reach) =>
            shoulder + Vector3.ClampMagnitude(target - shoulder, reach);

        // The world body shares the aimed third-person pose in both perspectives.
        // First-person grip is solved in the source pose before the eye-relative copy.
        private bool CanTrackBody => _combat.CanUseCombat && (_combat.IsArmed || _presentation.MeleePoseWeight <= .01f) &&
                                     !_combat.IsAttacking && (_combat.Magazine == null || !_combat.Magazine.IsReloading) &&
                                     (_actions == null || _actions.CanAttack);

        private void ApplyUpperBodyAim()
        {
            if (!_combat.CanUseCombat)
            {
                _bodyBlend = 0f;
                _animator.SetLookAtWeight(0f);
                return;
            }
            _bodyBlend = Mathf.MoveTowards(_bodyBlend, CanTrackBody ? 1f : 0f, Time.deltaTime * _poseFollowSpeed);
            float bodyWeight = Mathf.Clamp01(_bodyAimWeight) * (_posture != null ? _posture.UpperBodyAimMultiplier : 1f);
            _animator.SetLookAtWeight(_bodyBlend, bodyWeight, Mathf.Clamp01(_headAimWeight), 0f, Mathf.Clamp01(_lookAtClamp));
            _animator.SetLookAtPosition(_camera.EyePosition + _aimRotation * Vector3.forward * 100f);
        }

        private void LateUpdate()
        {
            if (_tactical != null && _tactical.IsPresenting) return;
            if (_prone != null && _prone.IsPresenting)
            {
                if (_prone.MeleePoseWeight > .001f)
                {
                    if (_prone.RetargetStandingMelee && _meleeChest != null)
                    {
                        // Retarget the authored arm chains from torso space into aimed space.
                        // Elbow/wrist motion and the prone torso/legs remain clip-driven.
                        Quaternion targetFrame = _view.AimRotation * _chestReference;
                        Quaternion correction = Quaternion.Slerp(Quaternion.identity,
                            targetFrame * Quaternion.Inverse(_meleeChest.rotation), _prone.MeleePoseWeight);
                        _rightUpperArm.rotation = correction * _rightUpperArm.rotation;
                        _leftUpperArm.rotation = correction * _leftUpperArm.rotation;
                    }
                    return;
                }
                FitProneItemContact(); return;
            }
            _wasProne = false;
            var state = _animator.GetCurrentAnimatorStateInfo(0);
            if (_mappingReady && _body != null && _body.isGrounded && _presentation.UsesItemGrip &&
                !_animator.IsInTransition(0) && (state.IsName("Base Layer.Locomotion") || state.IsName("Base Layer.CrouchLocomotion")))
            {
                _reloadEyeToTorso = _combat.transform.InverseTransformVector(_camera.EyePosition - _meleeChest.position);
                _reloadReferenceRotation = Quaternion.Inverse(_combat.transform.rotation) * _meleeChest.rotation;
                if (_presentation.WeaponForwardScale > .999f && Mathf.Abs(_presentation.WeaponVerticalOffset) < .001f && _presentation.RecoilAmount < .001f)
                {
                    CalculateGrip(_view.AimRotation, 1f, 0f, 0f, out var ready, out _, out _);
                    _reloadGripCorrection = _combat.transform.InverseTransformVector(_rightHand.position - ready);
                    _reloadReferenceDefinition = _presentation.ActiveDefinition;
                }
            }
            if (_captureMapping)
            {
                _rightGoalOffset = Quaternion.Inverse(_rightHand.rotation) * _sampleRightGoal;
                _leftGoalOffset = Quaternion.Inverse(_leftHand.rotation) * _sampleLeftGoal;
                _mappingReady = true;
                _captureMapping = false;
                return;
            }
            if (!_mappingReady || !_view.IsFirstPersonView || !_presentation.UsesItemGrip) return;
            // Fit the source arms before the eye-relative copy. Rifles share the
            // calibrated world grip above; pistols retain their animated source pose.
            Quaternion rightRotation = _rightHand.rotation;
            GetSupportPose(out var supportHandOffset, out var supportHandRotation);
            Vector3 offset = rightRotation * Vector3.Scale(supportHandOffset, _rightHand.lossyScale);
            Vector3 rightPosition = _rightHand.position;
            for (int i = 0; i < 12; i++)
            {
                rightPosition = WithinReach(rightPosition, _rightUpperArm.position, _rightReach * .99f);
                rightPosition = WithinReach(rightPosition, _leftUpperArm.position - offset, _leftReach * .99f);
            }
            HumanoidArmSolver.Solve(_rightUpperArm, _rightElbow, _rightHand, rightPosition, rightRotation);
            HumanoidArmSolver.Solve(_leftUpperArm, _leftElbow, _leftHand, rightPosition + offset,
                rightRotation * supportHandRotation);
        }

        private void FitProneItemContact()
        {
            if (!_combat.IsArmed || !_combat.CanUseCombat || _combat.IsAttacking || _presentation.ActiveWeapon == null) return;
            Quaternion native = _rightHand.rotation;
            Quaternion aimed = _view.AimRotation * Quaternion.Inverse(_presentation.GripRotation);
            if (!_wasProne || _prone.CanUseItemGrip)
                _proneGripCorrection = Quaternion.Inverse(native) * aimed;
            _wasProne = true;
            Quaternion rotation = _prone.CanUseItemGrip ? aimed : native * _proneGripCorrection;
            GetSupportPose(out var offset, out var supportRotation);
            Vector3 delta = rotation * Vector3.Scale(offset, _rightHand.lossyScale);
            Vector3 position = _rightHand.position;
            for (int i = 0; i < 12; i++)
            {
                position = WithinReach(position, _rightUpperArm.position, _rightReach * .99f);
                position = WithinReach(position, _leftUpperArm.position - delta, _leftReach * .99f);
            }
            HumanoidArmSolver.Solve(_rightUpperArm, _rightElbow, _rightHand, position, rotation);
            if (!(_prone.IsReloading))
                HumanoidArmSolver.Solve(_leftUpperArm, _leftElbow, _leftHand, position + delta, rotation * supportRotation);
        }

        private void SetWeight(float weight)
        {
            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0f);
            _animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0f);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);
        }

        private void OnDisable()
        {
            _captureMapping = false;
            _bodyBlend = 0f;
            _aimRotation = Quaternion.identity;
        }
    }
}
