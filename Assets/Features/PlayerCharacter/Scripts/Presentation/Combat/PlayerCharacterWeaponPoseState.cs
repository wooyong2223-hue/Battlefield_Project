using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public enum CharacterWeaponPoseMode { Hidden, FirstPerson, Ready, Carry, Sprint }

    // Owns presentation transitions only; input, aim and attack authority stay with gameplay.
    public sealed class PlayerCharacterWeaponPoseState
    {
        private float _readyTime;
        public float CarryBlend { get; private set; }
        public float SprintBlend { get; private set; }
        public float UpperLayerWeight => 1f;
        public float SupportHandWeight => 1f;
        public float ForwardScale => Mathf.Lerp(1f, Mathf.Lerp(.8f, .6f, SprintBlend), CarryBlend);
        public float VerticalOffset => -CarryBlend * Mathf.Lerp(.1f, .2f, SprintBlend);

        public void Reset() { CarryBlend = SprintBlend = _readyTime = 0f; }
        public void RequireReady() { CarryBlend = SprintBlend = 0f; _readyTime = .35f; }

        public void Tick(CharacterWeaponPoseMode mode, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (mode == CharacterWeaponPoseMode.Hidden) { Reset(); return; }
            _readyTime = Mathf.Max(0f, _readyTime - deltaTime);
            if (mode == CharacterWeaponPoseMode.FirstPerson) { CarryBlend = SprintBlend = 0f; return; }
            if (_readyTime > 0f) mode = CharacterWeaponPoseMode.Ready;
            float response = 1f - Mathf.Exp(-15f * deltaTime);
            bool carry = mode == CharacterWeaponPoseMode.Carry || mode == CharacterWeaponPoseMode.Sprint;
            CarryBlend = Mathf.Lerp(CarryBlend, carry ? 1f : 0f, response);
            SprintBlend = Mathf.Lerp(SprintBlend, mode == CharacterWeaponPoseMode.Sprint ? 1f : 0f, response);
        }

        // Carry affects reach and height; weapon orientation always follows resolved aim.
        public Quaternion Rotation(Quaternion aim) => aim;
    }
}
