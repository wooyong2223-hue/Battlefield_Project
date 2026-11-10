using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [CreateAssetMenu(menuName = "Battlefield/Player Character/Motion Profile")]
    public sealed class PlayerCharacterMotionProfile : ScriptableObject
    {
        [Header("Gait (world units)")]
        [SerializeField, Min(.01f)] private float _walkStride = 5.6f;
        [SerializeField, Min(.01f)] private float _sprintStride = 7.5f;
        [SerializeField] private Vector2 _walkCameraBob = new(.012f, .02f);
        [SerializeField] private Vector2 _sprintCameraBob = new(.022f, .038f);
        [SerializeField, Min(0f)] private float _cameraRoll = .45f;
        [SerializeField, Min(0f)] private float _adsMotionMultiplier = .1f;
        [SerializeField] private Vector3 _weaponBob = new(.022f, .032f, .008f);
        [Header("Weapon response")]
        [SerializeField] private Vector3 _lookSway = new(.006f, .008f, .006f);
        [SerializeField, Min(0f)] private float _maximumSway = 2.5f;
        [SerializeField] private Vector3 _sprintOffset = new(.04f, -.05f, -.04f);
        [SerializeField] private Vector3 _sprintEuler = new(8f, -5f, -8f);
        [SerializeField, Min(.01f)] private float _response = 18f;
        [SerializeField, Min(.01f)] private float _aimResponse = 18f;
        [Header("Landing")]
        [SerializeField, Min(0f)] private float _landingImpulse = .12f;
        [SerializeField, Min(0f)] private float _maximumLandingImpulse = 2f;
        [SerializeField, Min(.01f)] private float _landingRecovery = 14f;

        [Header("Jump camera")]
        [SerializeField, Min(0f)] private float _jumpVerticalLag = .025f;
        [SerializeField, Min(0f)] private float _maximumJumpLag = .12f;
        [SerializeField, Min(.01f)] private float _jumpRecovery = 10f;
        [SerializeField, Min(1f)] private float _landingCameraMultiplier = 3f;
        [SerializeField, Min(0f)] private float _jumpPitchPerMeter = 8f;
        [SerializeField, Min(0f)] private float _landingPitchPerMeter = 40f;
        [SerializeField, Min(0f)] private float _thirdPersonJumpMultiplier = .6f;

        public float JumpVerticalLag => Mathf.Max(0f, _jumpVerticalLag);
        public float MaximumJumpLag => Mathf.Max(0f, _maximumJumpLag);
        public float JumpRecovery => Mathf.Max(.01f, _jumpRecovery);
        public float LandingCameraMultiplier => Mathf.Max(1f, _landingCameraMultiplier);
        public float JumpPitchPerMeter => Mathf.Max(0f, _jumpPitchPerMeter);
        public float LandingPitchPerMeter => Mathf.Max(0f, _landingPitchPerMeter);
        public float ThirdPersonJumpMultiplier => Mathf.Clamp01(_thirdPersonJumpMultiplier);

        public float WalkStride => Mathf.Max(.01f, _walkStride);
        public float SprintStride => Mathf.Max(.01f, _sprintStride);
        public Vector2 WalkCameraBob => _walkCameraBob;
        public Vector2 SprintCameraBob => _sprintCameraBob;
        public float CameraRoll => _cameraRoll;
        public float AdsMotionMultiplier => Mathf.Clamp01(_adsMotionMultiplier);
        public Vector3 WeaponBob => _weaponBob;
        public Vector3 LookSway => _lookSway;
        public float MaximumSway => Mathf.Max(0f, _maximumSway);
        public Vector3 SprintOffset => _sprintOffset;
        public Vector3 SprintEuler => _sprintEuler;
        public float Response => Mathf.Max(.01f, _response);
        public float AimResponse => Mathf.Max(.01f, _aimResponse);
        public float LandingImpulse => Mathf.Max(0f, _landingImpulse);
        public float MaximumLandingImpulse => Mathf.Max(0f, _maximumLandingImpulse);
        public float LandingRecovery => Mathf.Max(.01f, _landingRecovery);
    }
}
