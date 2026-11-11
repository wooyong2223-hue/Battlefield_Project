using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    // Shared prone-only orbit and look limits; authored standing/crouch values remain on their views.
    [CreateAssetMenu(menuName = "Battlefield/Camera/Prone Camera Profile")]
    public sealed class ProneCameraProfile : ScriptableObject
    {
        [SerializeField] private Vector3 _pivotFromFeet = new(0f, 1.7f, .25f);
        [SerializeField] private Vector3 _thirdPersonOffset = new(.6f, .5f, -3.6f);
        [SerializeField] private Vector3 _aimOffset = new(.5f, .3f, -2.6f);
        [SerializeField, Min(0f)] private float _aimPitchUp = 30f;
        [SerializeField, Min(0f)] private float _aimPitchDown = 45f;
        [SerializeField, Min(0f)] private float _freePitchUp = 30f;
        [SerializeField, Min(0f)] private float _freePitchDown = 65f;
        [SerializeField, Min(0f)] private float _aimYawLimit = 35f;
        public float AimYawLimit => _aimYawLimit;
        public Vector3 PivotFromFeet => _pivotFromFeet;
        public Vector3 ThirdPersonOffset => _thirdPersonOffset;
        public Vector3 AimOffset => _aimOffset;
        public float AimPitchUp => _aimPitchUp;
        public float AimPitchDown => _aimPitchDown;
        public float FreePitchUp => _freePitchUp;
        public float FreePitchDown => _freePitchDown;
    }
}
