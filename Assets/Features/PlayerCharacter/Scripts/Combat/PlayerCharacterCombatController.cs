using System;
using Battlefield.Framework.Core;
using Battlefield.Features.Projectile;
using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterCombatController : MonoBehaviour
    {
        [Header("Fire")]
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField, Min(0.01f)] private float _fireRate = 10f;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0f)] private float _muzzleOffset = 0.6f;
        [SerializeField] private MonoBehaviour _aimSourceBehaviour;
        [SerializeField, Min(1f)] private float _aimDistance = 1000f;
        [SerializeField] private LayerMask _aimMask = ~0;
        [Header("Personal combat")]
        [SerializeField] private WeaponMagazine _magazine;
        [SerializeField] private PlayerCharacterLoadout _loadout;
        [SerializeField] private MeleeAttack _fistAttack;
        [SerializeField] private MeleeAttack _pistolAttack;
        [SerializeField] private PlayerCharacterActionPolicy _actions;

        private IWeaponInput _input;
        private IPlayerCharacterCombatInput _combatInput;
        private Health _health;
        private Transform _muzzle;
        private IPlayerCharacterAimSource _aimSource;
        private CharacterController _body;
        private float _nextFireTime;
        private readonly PlayerCharacterShotPath _shotPath = new();

        private bool _legacyArmed = true;
        public bool IsArmed => _loadout != null ? _loadout.IsArmed : _legacyArmed;
        public InfantryWeaponDefinition WeaponDefinition => _loadout != null ? _loadout.Definition : null;
        public int WeaponSlot => _loadout != null ? _loadout.CurrentSlot : IsArmed ? PlayerCharacterLoadout.PistolSlot : PlayerCharacterLoadout.UnarmedSlot;
        public bool IsRifleSingleFire => _loadout != null && _loadout.IsRifleSingleFire;
        public bool CanToggleFireMode => IsArmed && _loadout != null && _loadout.CanToggleFireMode;
        public bool IsSingleFire => _loadout == null || _loadout.UsesSingleFire;
        public string WeaponName => !IsArmed ? "FISTS" : WeaponDefinition != null ? WeaponDefinition.DisplayName : "PISTOL";
        private float FireRate => WeaponDefinition != null && WeaponDefinition.OverridesFire ? WeaponDefinition.FireRate : _fireRate;
        private float Damage => WeaponDefinition != null && WeaponDefinition.OverridesFire ? WeaponDefinition.Damage : _damage;
        public bool CanUseCombat => isActiveAndEnabled && (_health == null || !_health.IsDead);
        public bool IsAttacking => (_fistAttack != null && _fistAttack.IsAttacking) ||
                                   (_pistolAttack != null && _pistolAttack.IsAttacking);
        public IWeaponMagazine Magazine => _loadout != null ? _loadout.Magazine : _magazine;
        public Transform Muzzle => _muzzle;
        public float ShotInterval => 1f / Mathf.Max(.01f, FireRate);
        public float MeleeDuration => (IsArmed ? _pistolAttack : _fistAttack)?.Duration ?? 0f;
        public float MeleeWindup => (IsArmed ? _pistolAttack : _fistAttack)?.Windup ?? 0f;
        public event Action Fired;
        public event Action<bool> EquipmentChanged;
        public event Action<bool> MeleeStarted;

        public void BindMuzzle(Transform muzzle) => _muzzle = muzzle;
        public void ResetForRespawn()
        {
            CancelActions(); _legacyArmed = true; _nextFireTime = 0f;
            if (_loadout != null)
            {
                _loadout.ResetForRespawn();
                if (!isActiveAndEnabled) EquipmentChanged?.Invoke(IsArmed);
            }
            else { _magazine?.ResetAmmo(); EquipmentChanged?.Invoke(true); }
        }

        private void Awake()
        {
            _input = GetComponent<IWeaponInput>();
            _combatInput = GetComponent<IPlayerCharacterCombatInput>();
            _health = GetComponent<Health>();
            _body = GetComponent<CharacterController>();
            _aimSource = _aimSourceBehaviour as IPlayerCharacterAimSource;
            if (_aimSourceBehaviour != null && _aimSource == null)
            {
                Debug.LogError("Player character aim source must implement IPlayerCharacterAimSource.", this);
                enabled = false;
            }
            if (_input == null)
            {
                Debug.LogError("Player character requires an input component.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _nextFireTime = 0f;
            if (_loadout != null) _loadout.Changed += OnLoadoutChanged;
            EquipmentChanged?.Invoke(IsArmed);
        }
        private void OnLoadoutChanged()
        {
            CancelActions();
            EquipmentChanged?.Invoke(IsArmed);
        }

        private void Update()
        {
            if (!CanUseCombat)
            {
                CancelActions();
                return;
            }
            int slot = _input.WeaponSlotSelection;
            if (slot != 0) Equip(slot);
            if (_actions != null && !_actions.CanAttack) { CancelActions(); return; }
            if (_combatInput != null && IsArmed && !IsAttacking)
            {
                var magazine = Magazine;
                bool clickedOnEmpty = _combatInput.FirePressed && magazine?.Rounds == 0;

                if(_combatInput.ReloadPressed || clickedOnEmpty)
                    magazine?.TryReload();
            }
            if (_combatInput != null && _combatInput.MeleePressed) TryMelee();

            if (!IsArmed)
            {
                if (_input.FireWeapon) TryMelee();
                return;
            }

            if (_combatInput?.ToggleFireModePressed == true)
                _loadout?.ToggleFireMode();

            bool shouldFire = IsSingleFire && _combatInput != null ? _combatInput.FirePressed : _input.FireWeapon;
            if (shouldFire) TryFire();
        }

        private void Equip(int slot)
        {
            if (_loadout != null) { _loadout.TryEquip(slot); return; }
            if (slot != PlayerCharacterLoadout.PistolSlot && slot != PlayerCharacterLoadout.UnarmedSlot) return;
            bool armed = slot == PlayerCharacterLoadout.PistolSlot;
            if (IsArmed == armed) return;
            CancelActions();
            _legacyArmed = armed;
            EquipmentChanged?.Invoke(armed);
        }

        private void TryMelee()
        {
            if (IsAttacking || Time.time < _nextFireTime || (Magazine != null && Magazine.IsReloading)) return;
            MeleeAttack attack = IsArmed ? _pistolAttack : _fistAttack;
            Vector3 direction = _aimSource != null ? _aimSource.GetAimRay().direction : transform.forward;
            if (attack == null || !attack.TryAttack(transform, _body.bounds.center + Vector3.up * .35f, direction)) return;
            _nextFireTime = Time.time + attack.Duration;
            MeleeStarted?.Invoke(IsArmed);
        }

        private void CancelActions()
        {
            Magazine?.CancelReload();
            _fistAttack?.Cancel();
            _pistolAttack?.Cancel();
        }

        private void OnDisable()
        {
            CancelActions();
            if (_loadout != null) _loadout.Changed -= OnLoadoutChanged;
        }

        private void TryFire()
        {
            if (_bulletPool == null || Time.time < _nextFireTime || IsAttacking ||
                (Magazine != null && !Magazine.CanFire)) return;

            var (origin, direction) = _shotPath.Calculate(_body, _aimSource, _muzzle, _muzzleOffset, _aimDistance, _aimMask);

            Bullet bullet = _bulletPool.Get();
            if (bullet == null) return;
            if (Magazine != null && !Magazine.TryConsumeRound()) return;
            _nextFireTime = Time.time + ShotInterval;
            bullet.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
            bullet.Initialize(new ProjectileData(transform, Damage));
            Fired?.Invoke();
        }
    }
}
