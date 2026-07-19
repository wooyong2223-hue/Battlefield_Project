using UnityEngine;

namespace Battlefield.Core
{
    public interface ITargetable
    {
        Transform TargetPoint { get; }
    }
}
