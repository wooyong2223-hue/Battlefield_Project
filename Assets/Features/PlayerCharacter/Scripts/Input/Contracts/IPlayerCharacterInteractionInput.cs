namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterInteractionInput
    {
        bool HasControl { get; }
        bool InteractPressed { get; }
    }
}
