namespace Battlefield.Features.PlayerCharacter
{
    public interface IPlayerCharacterThrowInput
    {
        bool EquipGrenadePressed { get; }
        bool ThrowGrenadePressed { get; }
        bool ThrowGrenadeHeld { get; }
        bool ThrowGrenadeReleased { get; }
        bool CanControlGrenade { get; }
        bool ToggleGrenadeThrowModePressed { get; }
        int WeaponSlotSelection { get; }
        void ConsumePrimaryUntilRelease();
        void ConsumeAimUntilRelease();
    }
}
