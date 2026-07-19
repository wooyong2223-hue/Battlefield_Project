using UnityEngine;

namespace Battlefield.Fighter.Controller
{
    public interface IJetInput
    {
        float Throttle { get; }
        float Pitch { get; }
        float Roll { get; }
        float Yaw { get; }

        bool ChangeCamera { get; }
        bool RearView { get; }
    }
}