using System.Collections.Generic;
using UnityEngine;

namespace Battlefield.Pool
{
    public class ObjectPool<T> : MonoBehaviour, IPool where T : PoolableBehaviour
    {
        [Header("Pool")]
        [SerializeField] protected T _prefab;
        [SerializeField] protected int _initialSize = 20;

        protected readonly Queue<T> _pool = new();

        protected virtual void Awake()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                T obj = Create();
                Return(obj);
            }
        }

        protected virtual T Create()
        {
            T obj = Instantiate(_prefab, transform);
            obj.SetPool(this);
            obj.gameObject.SetActive(false);

            return obj;
        }

        public virtual T Get()
        {
            T obj;

            if (_pool.Count == 0)
            {
                obj = Create();
            }
            else
            {
                obj = _pool.Dequeue();
            }

            obj.gameObject.SetActive(true);
            obj.OnSpawn();

            return obj;
        }

        public virtual void Return(T obj)
        {
            obj.OnDespawn();
            obj.gameObject.SetActive(false);

            _pool.Enqueue(obj);
        }
        void IPool.Return(IPoolable poolable)
        {
            if (poolable is T obj)
            {
                Return(obj);
            }
        }
    }
}