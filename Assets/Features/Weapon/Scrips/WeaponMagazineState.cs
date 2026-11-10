using System;
namespace Battlefield.Features.Weapon
{
    // Ammunition state is independent of a visual model or a Unity object lifecycle.
    public sealed class WeaponMagazineState : IWeaponMagazine
    {
        private readonly Func<float> _clock;
        private readonly Func<bool> _available;
        private int _startingReserve;
        private float _reloadEndsAt;
        public int Rounds { get; private set; }
        public int Reserve { get; private set; }
        public int Capacity { get; private set; }
        public float ReloadDuration { get; private set; }
        public bool IsReloading { get; private set; }
        public bool CanFire => (_available == null || _available()) && !IsReloading && Rounds > 0;
        public event Action ReloadStarted;
        public event Action ReloadCompleted;
        public WeaponMagazineState(int capacity, int reserve, float duration, Func<float> clock, Func<bool> available = null)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _available = available;
            Configure(capacity, reserve, duration);
            ResetAmmo();
        }
        public void Configure(int capacity, int reserve, float duration)
        {
            Capacity = Math.Max(1, capacity);
            _startingReserve = Math.Max(0, reserve);
            ReloadDuration = Math.Max(.01f, duration);
        }
        public void ResetAmmo()
        {
            CancelReload(); Rounds = Capacity; Reserve = _startingReserve;
        }
        public void Tick()
        {
            if (!IsReloading || _clock() < _reloadEndsAt) return;
            int amount = Math.Min(Capacity - Rounds, Reserve);
            Rounds += amount; Reserve -= amount; IsReloading = false;
            ReloadCompleted?.Invoke();
        }
        public bool TryConsumeRound()
        {
            if (!CanFire) return false;
            Rounds--; return true;
        }
        public bool TryReload()
        {
            if ((_available != null && !_available()) || IsReloading || Rounds >= Capacity || Reserve <= 0) return false;
            IsReloading = true; _reloadEndsAt = _clock() + ReloadDuration;
            ReloadStarted?.Invoke(); return true;
        }
        public void CancelReload() => IsReloading = false;
    }
}
