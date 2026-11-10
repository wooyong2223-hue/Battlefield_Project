using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterInput
    {
        Vector2 Move { get; }
        bool SprintHeld { get; }
        bool JumpPressed { get; }
    }
}
