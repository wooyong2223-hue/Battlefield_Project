namespace Battlefield.Pool
{
    public interface IPool
    {
        void Return(IPoolable poolable);
    }
}