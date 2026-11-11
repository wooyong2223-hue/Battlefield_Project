namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterLifeInput
    {
        bool InteractHeld { get; }
        bool GiveUpPressed { get; }
        bool RespawnPressed { get; }
    }
}
