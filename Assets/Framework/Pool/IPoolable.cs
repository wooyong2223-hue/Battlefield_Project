namespace Battlefield.Pool
{
    public interface IPoolable
    {
        IPool Pool { get; }
        void OnSpawn();
        void OnDespawn();
    }
}
