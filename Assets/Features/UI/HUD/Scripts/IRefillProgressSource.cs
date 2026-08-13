namespace Battlefield.Features.UI
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
