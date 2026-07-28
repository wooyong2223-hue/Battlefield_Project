using System.Collections.Generic;
using UnityEngine;
using Battlefield.Pool;

namespace Battlefield.VFX.Hit
{
    public class BulletHolePool : ObjectPool<BulletHole>
    {
        [SerializeField] private BulletHole _prefab;
        [SerializeField, Min(1)] private int _maxSize = 20;

        private readonly LinkedList<BulletHole> _activeHoles = new();
        private readonly Dictionary<BulletHole, LinkedListNode<BulletHole>>
            _activeHoleNodes = new();
        private int _createdCount;

        protected override BulletHole Create()
        {
            BulletHole bulletHole = Instantiate(_prefab, transform);
            _createdCount++;

            return Register(bulletHole);
        }

        public override BulletHole Get()
        {
            if (_pool.Count == 0 &&
                _createdCount >= _maxSize &&
                _activeHoles.First != null)
            {
                Return(_activeHoles.First.Value);
            }

            BulletHole bulletHole = base.Get();
            LinkedListNode<BulletHole> node = _activeHoles.AddLast(bulletHole);
            _activeHoleNodes[bulletHole] = node;

            return bulletHole;
        }

        public override void Return(BulletHole bulletHole)
        {
            if (_activeHoleNodes.Remove(bulletHole, out LinkedListNode<BulletHole> node))
            {
                _activeHoles.Remove(node);
            }

            base.Return(bulletHole);
        }
    }
}
