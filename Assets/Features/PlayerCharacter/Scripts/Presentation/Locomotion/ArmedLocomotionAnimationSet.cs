using System;
using UnityEngine;
namespace Battlefield.Features.PlayerCharacter
{
    [CreateAssetMenu(menuName = "Battlefield/Animation/Armed Locomotion Set")]
    public sealed class ArmedLocomotionAnimationSet : ScriptableObject
    {
        [Serializable] private struct Entry
        {
            [SerializeField] private RuntimeAnimatorController _equipment;
            [SerializeField] private RuntimeAnimatorController _movement;
            public RuntimeAnimatorController Equipment => _equipment;
            public RuntimeAnimatorController Movement => _movement;
        }
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        public RuntimeAnimatorController Find(RuntimeAnimatorController equipment)
        {
            if (equipment == null) return null;
            foreach (var entry in _entries) if (entry.Equipment == equipment) return entry.Movement;
            return null;
        }
    }
}
