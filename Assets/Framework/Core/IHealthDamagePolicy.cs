namespace Battlefield.Framework.Core
{
    public interface IHealthDamagePolicy
    {
        // Return true only when this policy owns the damage, e.g. a downed soldier's finishing damage.
        bool TryHandleDamage(float damage);
    }
}
