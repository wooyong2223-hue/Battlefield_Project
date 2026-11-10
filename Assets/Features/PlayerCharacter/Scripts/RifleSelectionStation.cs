using Battlefield.Features.Weapon;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    public sealed class RifleSelectionStation : MonoBehaviour
    {
        [SerializeField] private InfantryWeaponDefinition _definition;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField, Min(.01f)] private float _range = 3f;
        public InfantryWeaponDefinition Definition => _definition;
        public Vector3 InteractionPoint => _interactionPoint != null ? _interactionPoint.position : transform.position;
        public bool IsInRange(Transform actor) => isActiveAndEnabled && actor != null && _definition != null &&
            (actor.position - InteractionPoint).sqrMagnitude <= _range * _range;
    }
}
