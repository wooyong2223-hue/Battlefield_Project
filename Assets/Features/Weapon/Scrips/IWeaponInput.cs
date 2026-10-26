namespace Battlefield.Features.Weapon
{
    public interface IWeaponInput
    {
        bool FireWeapon { get; }
        int WeaponSlotSelection { get; }
    }
}
