namespace Battlefield.Framework.Pool
{
    public interface IPool
    {
        void Return(IPoolable poolable);
    }
}