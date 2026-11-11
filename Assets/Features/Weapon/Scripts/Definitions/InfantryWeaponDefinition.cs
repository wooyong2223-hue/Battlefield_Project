using UnityEngine;
namespace Battlefield.Features.Weapon
{
    public enum InfantryFireMode
    {
        DefaultForSlot = 0,
        Single = 1,
        Automatic = 2,
        Selectable = 3
    }

    [CreateAssetMenu(menuName = "Battlefield/Weapon/Infantry weapon")]
    public sealed class InfantryWeaponDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [Header("Fire (zero rate keeps the character's existing settings)")]
        [SerializeField] private InfantryFireMode _fireMode;
        [SerializeField, Min(0f)] private float _fireRate;
        [SerializeField, Min(0f)] private float _damage = 10f;
        [Header("Ammunition (unless an existing magazine is supplied)")]
        [SerializeField, Min(1)] private int _capacity = 30;
        [SerializeField, Min(0)] private int _reserve = 180;
        [SerializeField, Min(.01f)] private float _reloadDuration = 1.6f;
        [Header("Presentation (empty model uses the existing pistol)")]
        [SerializeField] private WeaponModel _modelPrefab;
        [SerializeField] private AnimatorOverrideController _animations;
        [Tooltip("Optional standing and crouching reload clip. Leave empty to use the shared reload presentation.")]
        [SerializeField] private AnimationClip _reloadClip;
        [SerializeField] private bool _customGrip;
        [SerializeField] private Vector3 _gripPosition, _gripEuler;
        [SerializeField] private Vector3 _firstPersonEuler, _firstPersonOffset, _firstPersonAdsOffset;
        [SerializeField, Min(.01f)] private float _firstPersonScale = 1f;
        [SerializeField, Min(0f)] private float _recoilDegrees = 1.5f;
        [SerializeField, Min(0f)] private float _recoilDistance = .01f;
        public string DisplayName => _displayName;
        public bool OverridesFire => _fireRate > 0f;
        public InfantryFireMode FireMode => _fireMode;
        public float FireRate => _fireRate;
        public float Damage => _damage;
        public int Capacity => _capacity;
        public int Reserve => _reserve;
        public float ReloadDuration => _reloadDuration;
        public WeaponModel ModelPrefab => _modelPrefab;
        public AnimatorOverrideController Animations => _animations;
        public AnimationClip ReloadClip => _reloadClip;
        public bool CustomGrip => _customGrip;
        public Vector3 GripPosition => _gripPosition;
        public Quaternion GripRotation => Quaternion.Euler(_gripEuler);
        public Vector3 FirstPersonEuler => _firstPersonEuler;
        public Vector3 FirstPersonOffset => _firstPersonOffset;
        public Vector3 FirstPersonAdsOffset => _firstPersonAdsOffset;
        public float FirstPersonScale => _firstPersonScale;
        public float RecoilDegrees => _recoilDegrees;
        public float RecoilDistance => _recoilDistance;
    }
}
