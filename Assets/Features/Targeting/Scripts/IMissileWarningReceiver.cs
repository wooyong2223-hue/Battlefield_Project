using UnityEngine;

namespace Battlefield.Features.Targeting
{
    public interface IMissileWarningReceiver
    {
        void ReportThreatState(Object source, MissileWarningState state);
        void ReportThreatProgress(Object source, float progress);
        void ClearThreatState(Object source);
    }
}
