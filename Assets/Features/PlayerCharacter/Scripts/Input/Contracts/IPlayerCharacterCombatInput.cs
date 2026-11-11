namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterCombatInput
    {
        bool ReloadPressed { get; }
        bool MeleePressed { get; }
        bool FirePressed { get; }
        bool ToggleFireModePressed { get; }
    }
}
