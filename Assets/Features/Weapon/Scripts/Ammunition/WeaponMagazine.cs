using System;
using UnityEngine;
namespace Battlefield.Features.Weapon
{
    [DisallowMultipleComponent]
    public sealed class WeaponMagazine : MonoBehaviour, IWeaponMagazine
    {
        [SerializeField, Min(1)] private int _capacity = 15;
        [SerializeField, Min(0)] private int _startingReserve = 90;
        [SerializeField, Min(.01f)] private float _reloadDuration = 1.666667f;
        private WeaponMagazineState _state;
        private WeaponMagazineState State
        {
            get
            {
                if (_state == null) _state = new WeaponMagazineState(_capacity, _startingReserve, _reloadDuration,
                    () => Time.time, () => isActiveAndEnabled);
                _state.Configure(_capacity, _startingReserve, _reloadDuration);
                return _state;
            }
        }
        public int Rounds => State.Rounds;
        public int Reserve => State.Reserve;
        public int Capacity => _capacity;
        public float ReloadDuration => _reloadDuration;
        public bool IsReloading => State.IsReloading;
        public bool CanFire => State.CanFire;
        public event Action ReloadStarted { add => State.ReloadStarted += value; remove => State.ReloadStarted -= value; }
        public event Action ReloadCompleted { add => State.ReloadCompleted += value; remove => State.ReloadCompleted -= value; }
        private void Awake() => ResetAmmo();
        private void Update() => State.Tick();
        public void ResetAmmo() => State.ResetAmmo();
        public bool TryConsumeRound() => State.TryConsumeRound();
        public bool TryReload() => State.TryReload();
        public void CancelReload() => State.CancelReload();
        private void OnDisable() => CancelReload();
    }
}
