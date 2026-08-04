using System;
using UnityEngine;

namespace Battlefield.Features.VFX
{
    public class HitEffectDatabase : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public PhysicsMaterial PhysicsMaterial;
            public HitEffectPool EffectPool;
            public BulletHolePool BulletHolePool;
        }

        [SerializeField] private Entry[] _entries;

        public Entry GetEntry(PhysicsMaterial material)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.PhysicsMaterial == material)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
