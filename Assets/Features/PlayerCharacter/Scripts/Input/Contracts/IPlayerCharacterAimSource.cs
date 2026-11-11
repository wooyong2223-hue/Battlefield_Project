using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterAimSource
    {
        Ray GetAimRay();
    }
}
