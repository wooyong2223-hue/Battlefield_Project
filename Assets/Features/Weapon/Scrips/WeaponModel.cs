using UnityEngine;

namespace Battlefield.Features.Weapon
{
    public sealed class WeaponModel : MonoBehaviour
    {
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _supportHandGrip;
        [SerializeField] private Transform _magazine;
        [SerializeField] private Transform _reloadGrip;
        [SerializeField] private Vector3 _reloadTravel = new(0f, -.4f, -.08f);
        [SerializeField] private Vector3 _reloadTilt;
        public Transform Muzzle => _muzzle;
        public Transform SupportHandGrip => _supportHandGrip;
        public Transform Magazine => _magazine;
        public Transform ReloadGrip => _reloadGrip;
        public Vector3 ReloadTravel => _reloadTravel;
        public Vector3 ReloadTilt => _reloadTilt;
    }
}
