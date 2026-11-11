using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public sealed class SeatedCharacterPose : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationClip _seatedAnimation;
        private SeatedAnimationPlayback _animationPlayback;
        [SerializeField] private float _torsoRecline = 12f;
        [SerializeField] private Vector3 _thighDirection = new Vector3(0f, 0.28f, 1f);
        [SerializeField] private Vector3 _lowerLegDirection = new Vector3(0f, -0.45f, 1f);
        [SerializeField] private Vector3 _upperArmDirection = new Vector3(0f, -0.65f, 0.45f);
        [SerializeField] private Vector3 _forearmDirection = new Vector3(0f, 0.1f, 1f);

        private const float MinimumDirectionLength = 0.000001f;
        private Transform[] _bones;
        private BonePose[] _standingPose;
        private Animator _cachedAnimator;
        private Transform _seatPoint;
        private bool _animatorWasEnabled;

        public bool IsSeated { get; private set; }
        public bool IsUsingAnimation => IsSeated && _animationPlayback != null && _animationPlayback.IsValid;
        public bool CanPose => CacheRig() && TryGetPelvisFrame(out _) &&
                               (_seatedAnimation == null || (_seatedAnimation.humanMotion && _seatedAnimation.length > 0f));

        private struct BonePose
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
        }

        private void Awake() => CacheRig();

        public bool BeginPose(Transform seatPoint)
        {
            if (seatPoint == null || seatPoint == transform || seatPoint.IsChildOf(transform))
                return false;
            if (IsSeated) return _seatPoint == seatPoint;
            if (!CanPose) return false;

            CaptureBonePose();
            _animatorWasEnabled = _animator.enabled;
            if (_seatedAnimation != null)
                _animationPlayback = new SeatedAnimationPlayback(_animator, _seatedAnimation);
            else
                _animator.enabled = false;
            _seatPoint = seatPoint;
            IsSeated = true;
            ApplyPose(0f);
            return true;
        }

        public void EndPose()
        {
            if (!IsSeated) return;

            IsSeated = false;
            _seatPoint = null;
            _animationPlayback?.Dispose();
            _animationPlayback = null;
            RestoreBonePose();
            if (_cachedAnimator != null) _cachedAnimator.enabled = _animatorWasEnabled;
        }

        private void LateUpdate()
        {
            if (!IsSeated) return;
            if (_seatPoint == null || _cachedAnimator == null)
            {
                EndPose();
                return;
            }

            float deltaTime = _cachedAnimator.updateMode == AnimatorUpdateMode.UnscaledTime
                ? Time.unscaledDeltaTime : Time.deltaTime;
            ApplyPose(deltaTime);
        }

        private void OnDisable() => EndPose();

        private bool CacheRig()
        {
            if (_animator == null || _animator.avatar == null || !_animator.avatar.isValid
                || !_animator.isHuman || !_animator.transform.IsChildOf(transform))
                return false;
            if (_cachedAnimator == _animator) return HasRequiredBones();
            if (IsSeated) return false;

            _cachedAnimator = _animator;
            _bones = new Transform[(int)HumanBodyBones.LastBone];
            _standingPose = new BonePose[_bones.Length];
            for (int i = 0; i < _bones.Length; i++)
                _bones[i] = _animator.GetBoneTransform((HumanBodyBones)i);
            return HasRequiredBones();
        }

        private bool HasRequiredBones()
        {
            return Bone(HumanBodyBones.Hips) != null
                && Bone(HumanBodyBones.Spine) != null
                && Bone(HumanBodyBones.Neck) != null
                && Bone(HumanBodyBones.Head) != null
                && Bone(HumanBodyBones.LeftUpperLeg) != null
                && Bone(HumanBodyBones.LeftLowerLeg) != null
                && Bone(HumanBodyBones.LeftFoot) != null
                && Bone(HumanBodyBones.RightUpperLeg) != null
                && Bone(HumanBodyBones.RightLowerLeg) != null
                && Bone(HumanBodyBones.RightFoot) != null
                && Bone(HumanBodyBones.LeftUpperArm) != null
                && Bone(HumanBodyBones.LeftLowerArm) != null
                && Bone(HumanBodyBones.LeftHand) != null
                && Bone(HumanBodyBones.RightUpperArm) != null
                && Bone(HumanBodyBones.RightLowerArm) != null
                && Bone(HumanBodyBones.RightHand) != null;
        }

        private void CaptureBonePose()
        {
            for (int i = 0; i < _bones.Length; i++)
            {
                Transform bone = _bones[i];
                if (bone == null) continue;
                _standingPose[i] = new BonePose
                {
                    LocalPosition = bone.localPosition,
                    LocalRotation = bone.localRotation
                };
            }
        }

        private void RestoreBonePose()
        {
            for (int i = 0; i < _bones.Length; i++)
            {
                Transform bone = _bones[i];
                if (bone == null) continue;
                bone.localPosition = _standingPose[i].LocalPosition;
                bone.localRotation = _standingPose[i].LocalRotation;
            }
        }

        private void ApplyPose(float deltaTime)
        {
            // Follow the seat spatially without parenting the character to a destructible vehicle.
            transform.rotation = _seatPoint.rotation;
            // Resample before fitting so neither authored motion nor fallback accumulates rotations.
            if (IsUsingAnimation) _animationPlayback.Sample(deltaTime);
            else RestoreBonePose();
            Transform hips = Bone(HumanBodyBones.Hips);
            if (hips == null || !TryGetPelvisFrame(out Quaternion pelvisFrame)) return;

            hips.rotation = _seatPoint.rotation * Quaternion.Inverse(pelvisFrame) * hips.rotation;

            float reclineRadians = _torsoRecline * Mathf.Deg2Rad;
            AlignBone(HumanBodyBones.Spine, HumanBodyBones.Neck,
                new Vector3(0f, Mathf.Cos(reclineRadians), -Mathf.Sin(reclineRadians)));
            // Keep the pilot looking ahead while the torso rests against the seat back.
            Transform head = Bone(HumanBodyBones.Head);
            head.rotation = Quaternion.AngleAxis(_torsoRecline, _seatPoint.right) * head.rotation;

            // A valid seated source owns limb motion. The geometric pose remains a
            // fallback for missing optional clips, not an overwrite of the sampled animation.
            if (!IsUsingAnimation)
            {
                PoseLeg(HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes);
                PoseLeg(HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
                    HumanBodyBones.RightFoot, HumanBodyBones.RightToes);
                PoseArm(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,
                    HumanBodyBones.LeftHand);
                PoseArm(HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm,
                    HumanBodyBones.RightHand);
            }

            // The mount belongs to the hips, not the standing capsule or model origin.
            // Only the actor's position changes; authored model offsets and scales survive.
            transform.position += _seatPoint.position - hips.position;
        }

        private bool TryGetPelvisFrame(out Quaternion frame)
        {
            frame = Quaternion.identity;
            Transform hips = Bone(HumanBodyBones.Hips);
            Transform spine = Bone(HumanBodyBones.Spine);
            Transform leftLeg = Bone(HumanBodyBones.LeftUpperLeg);
            Transform rightLeg = Bone(HumanBodyBones.RightUpperLeg);
            if (hips == null || spine == null || leftLeg == null || rightLeg == null)
                return false;

            Vector3 up = spine.position - hips.position;
            Vector3 right = rightLeg.position - leftLeg.position;
            Vector3 forward = Vector3.Cross(right, up);
            if (up.sqrMagnitude < MinimumDirectionLength
                || forward.sqrMagnitude < MinimumDirectionLength)
                return false;

            frame = Quaternion.LookRotation(forward, up);
            return true;
        }

        private void PoseLeg(HumanBodyBones upperLeg, HumanBodyBones lowerLeg,
            HumanBodyBones foot, HumanBodyBones toes)
        {
            AlignBone(upperLeg, lowerLeg, _thighDirection);
            AlignBone(lowerLeg, foot, _lowerLegDirection);
            AlignBone(foot, toes, Vector3.forward);
        }

        private void PoseArm(HumanBodyBones upperArm, HumanBodyBones lowerArm,
            HumanBodyBones hand)
        {
            AlignBone(upperArm, lowerArm, _upperArmDirection);
            AlignBone(lowerArm, hand, _forearmDirection);
        }

        private void AlignBone(HumanBodyBones boneId, HumanBodyBones childId,
            Vector3 seatDirection)
        {
            Transform bone = Bone(boneId);
            Transform child = Bone(childId);
            if (bone == null || child == null) return;

            Vector3 currentDirection = child.position - bone.position;
            if (currentDirection.sqrMagnitude < MinimumDirectionLength
                || seatDirection.sqrMagnitude < MinimumDirectionLength)
                return;

            Vector3 desiredDirection = _seatPoint.TransformDirection(seatDirection);
            bone.rotation = Quaternion.FromToRotation(currentDirection, desiredDirection) * bone.rotation;
        }

        private Transform Bone(HumanBodyBones bone)
        {
            return _bones != null ? _bones[(int)bone] : null;
        }
    }
}
