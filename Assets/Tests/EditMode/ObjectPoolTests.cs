using System.Reflection;
using Battlefield.Framework.Pool;
using NUnit.Framework;
using UnityEngine;

namespace Battlefield.Tests.EditMode
{
    public sealed class TestPoolable : PoolableBehaviour
    {
        public int SpawnCount { get; private set; }
        public int DespawnCount { get; private set; }

        public override void OnSpawn()
        {
            SpawnCount++;
        }

        public override void OnDespawn()
        {
            DespawnCount++;
        }
    }

    public sealed class TestObjectPool : ObjectPool<TestPoolable>
    {
        public int CreatedCount { get; private set; }
        public int AvailableCount => _pool.Count;

        protected override TestPoolable Create()
        {
            GameObject obj = new GameObject($"Test Poolable {CreatedCount}");
            obj.transform.SetParent(transform);
            CreatedCount++;

            return Register(obj.AddComponent<TestPoolable>());
        }
    }

    public class ObjectPoolTests
    {
        private GameObject _poolObject;
        private TestObjectPool _pool;

        [SetUp]
        public void SetUp()
        {
            _poolObject = new GameObject("Test Object Pool");
            _pool = _poolObject.AddComponent<TestObjectPool>();
            typeof(ObjectPool<TestPoolable>)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(_pool, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_poolObject);
        }

        [Test]
        public void AwakeCreatesInitialInactiveObjects()
        {
            Assert.That(_pool.CreatedCount, Is.EqualTo(20));
            Assert.That(_pool.AvailableCount, Is.EqualTo(20));

            foreach (Transform child in _pool.transform)
            {
                Assert.That(child.gameObject.activeSelf, Is.False);
            }
        }

        [Test]
        public void GetActivatesObjectAndInvokesOnSpawn()
        {
            TestPoolable pooled = _pool.Get();

            Assert.That(pooled.gameObject.activeSelf, Is.True);
            Assert.That(pooled.SpawnCount, Is.EqualTo(1));
            Assert.That(_pool.AvailableCount, Is.EqualTo(19));
        }

        [Test]
        public void ReturnDeactivatesAndReusesObject()
        {
            TestPoolable first = _pool.Get();
            int createdBeforeReturn = _pool.CreatedCount;
            int despawnCountBeforeReturn = first.DespawnCount;

            _pool.Return(first);
            TestPoolable reused = null;

            for (int i = 0; i < _pool.CreatedCount; i++)
            {
                TestPoolable candidate = _pool.Get();

                if (candidate == first)
                {
                    reused = candidate;
                    break;
                }
            }

            Assert.That(first.gameObject.activeSelf, Is.True);
            Assert.That(first.DespawnCount, Is.EqualTo(despawnCountBeforeReturn + 1));
            Assert.That(reused, Is.SameAs(first));
            Assert.That(_pool.CreatedCount, Is.EqualTo(createdBeforeReturn));
        }
    }
}
