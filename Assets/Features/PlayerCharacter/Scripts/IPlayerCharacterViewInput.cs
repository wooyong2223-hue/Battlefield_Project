using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterViewInput
    {
        bool HasControl { get; }
        Vector2 Look { get; }
        bool TogglePerspectivePressed { get; }
        bool FreeLookHeld { get; }
        bool AimHeld { get; }
        bool AimPressed { get; }
        bool AimReleased { get; }
    }
}
