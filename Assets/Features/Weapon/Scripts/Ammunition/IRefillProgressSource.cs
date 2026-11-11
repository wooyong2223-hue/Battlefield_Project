namespace Battlefield.Features.Weapon
{
    public enum RefillProgressType
    {
        Active,
        Reload,
        Reserve
    }

    public interface IRefillProgressSource
    {
        bool TryGetRefillProgress(
            RefillProgressType progressType,
            out float progress);
    }
}
