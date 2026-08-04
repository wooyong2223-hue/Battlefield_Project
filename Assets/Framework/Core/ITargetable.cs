using UnityEngine;

namespace Battlefield.Framework.Core
{
    public interface ITargetable
    {
        Transform TargetPoint { get; }
    }
}
