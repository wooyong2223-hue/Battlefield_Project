using UnityEngine;
using Battlefield.Fighter.Controller;

namespace Battlefield.Weapon
{
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon")]
        [SerializeField] private WeaponBase _weapon;

        private IJetInput _input;

        private void Awake()
        {
            _input = GetComponent<IJetInput>();

            if (_input == null)
            {
                Debug.LogError($"{nameof(IJetInput)} Missing.", this);
            }
        }

        private void Update()
        {
            if (_input == null)
            {
                return;
            }

            if (_input.FireWeapon)
            {
                _weapon?.TryFire();
            }
        }
    }
}