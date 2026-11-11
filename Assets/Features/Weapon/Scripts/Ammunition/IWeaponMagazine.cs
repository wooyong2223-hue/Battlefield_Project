using System;
namespace Battlefield.Features.Weapon
{
    public interface IWeaponMagazine
    {
        int Rounds { get; }
        int Reserve { get; }
        int Capacity { get; }
        float ReloadDuration { get; }
        bool IsReloading { get; }
        bool CanFire { get; }
        bool TryConsumeRound();
        bool TryReload();
        void CancelReload();
        void ResetAmmo();
        event Action ReloadStarted;
        event Action ReloadCompleted;
    }
}
