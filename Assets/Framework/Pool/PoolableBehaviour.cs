using UnityEngine;

namespace Battlefield.Pool
{
    public abstract class PoolableBehaviour : MonoBehaviour, IPoolable
    {
        public IPool Pool {  get; private set; }
        
        internal void SetPool(IPool pool)
        {
            Pool = pool;
        }
        public virtual void OnSpawn()
        {
        }

        public virtual void OnDespawn()
        {
        }

        protected void ReturnToPool()
        {
            Pool?.Return(this);
        }
    }
}