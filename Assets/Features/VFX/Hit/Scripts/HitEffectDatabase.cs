using System;
using UnityEngine;

namespace Battlefield.VFX.Hit
{
    public class HitEffectDatabase : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public PhysicsMaterial PhysicsMaterial;
            public ParticleSystem EffectPrefab;
        }

        [SerializeField] private Entry[] _entries;

        public ParticleSystem GetEffect(PhysicsMaterial material)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.PhysicsMaterial == material)
                {
                    return entry.EffectPrefab;
                }
            }

            return null;
        }
    }
}