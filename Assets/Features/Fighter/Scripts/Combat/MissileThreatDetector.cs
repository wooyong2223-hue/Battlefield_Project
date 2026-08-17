using System;
using System.Collections.Generic;
using Battlefield.Features.Targeting;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(AirTarget))]
    public sealed class MissileThreatDetector :
        MonoBehaviour,
        IMissileWarningReceiver
    {
        private readonly Dictionary<UnityEngine.Object, MissileWarningState>
            _reportedThreatStates = new();
        private readonly Dictionary<UnityEngine.Object, float>
            _reportedLockProgress = new();

        public bool HasIncomingMissile => IncomingMissileCount > 0;
        public int IncomingMissileCount { get; private set; }
        public float ClosestThreatDistance { get; private set; } =
            float.PositiveInfinity;
        public MissileWarningState WarningState { get; private set; }
        public float LockProgress { get; private set; }
        public event Action<MissileWarningState> WarningStateChanged;
        public event Action<float> LockProgressChanged;

        private void Update()
        {
            RefreshIncomingThreats();
        }

        private void OnDisable()
        {
            _reportedThreatStates.Clear();
            _reportedLockProgress.Clear();
            ClearThreats();
            SetWarningState(MissileWarningState.None);
            SetLockProgress(0f);
        }

        public void ReportThreatState(
            UnityEngine.Object source,
            MissileWarningState state)
        {
            if (source == null ||
                state == MissileWarningState.None)
            {
                return;
            }

            _reportedThreatStates[source] = state;
            RefreshIncomingThreats();
            RefreshWarningState();
        }

        public void ReportThreatProgress(
            UnityEngine.Object source,
            float progress)
        {
            if (source == null ||
                !_reportedThreatStates.TryGetValue(
                    source,
                    out MissileWarningState state) ||
                state != MissileWarningState.Locking)
            {
                return;
            }

            _reportedLockProgress[source] = Mathf.Clamp01(progress);
            RefreshLockProgress();
        }

        public void ClearThreatState(UnityEngine.Object source)
        {
            if (source == null || !_reportedThreatStates.Remove(source))
            {
                return;
            }

            _reportedLockProgress.Remove(source);
            RefreshIncomingThreats();
            RefreshWarningState();
        }

        private void RefreshIncomingThreats()
        {
            int incomingMissileCount = 0;
            float closestThreatDistance = float.PositiveInfinity;

            foreach (KeyValuePair<UnityEngine.Object, MissileWarningState>
                     report in _reportedThreatStates)
            {
                if (report.Value != MissileWarningState.Incoming ||
                    report.Key is not Component threat)
                {
                    continue;
                }

                incomingMissileCount++;
                float distance = Vector3.Distance(
                    transform.position,
                    threat.transform.position);
                closestThreatDistance = Mathf.Min(
                    closestThreatDistance,
                    distance);
            }

            IncomingMissileCount = incomingMissileCount;
            ClosestThreatDistance = closestThreatDistance;
        }

        private void ClearThreats()
        {
            IncomingMissileCount = 0;
            ClosestThreatDistance = float.PositiveInfinity;
        }

        private void RefreshWarningState()
        {
            MissileWarningState state = MissileWarningState.None;
            foreach (MissileWarningState reportedState in
                     _reportedThreatStates.Values)
            {
                if (reportedState > state)
                {
                    state = reportedState;
                }
            }

            SetWarningState(state);
            RefreshLockProgress();
        }

        private void RefreshLockProgress()
        {
            float progress = 0f;

            if (WarningState == MissileWarningState.Locking)
            {
                foreach (KeyValuePair<UnityEngine.Object, float> report in
                         _reportedLockProgress)
                {
                    if (_reportedThreatStates.TryGetValue(
                            report.Key,
                            out MissileWarningState state) &&
                        state == MissileWarningState.Locking)
                    {
                        progress = Mathf.Max(progress, report.Value);
                    }
                }
            }

            SetLockProgress(progress);
        }

        private void SetWarningState(MissileWarningState state)
        {
            if (WarningState == state)
            {
                return;
            }

            WarningState = state;
            WarningStateChanged?.Invoke(WarningState);
        }

        private void SetLockProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (Mathf.Approximately(LockProgress, progress))
            {
                return;
            }

            LockProgress = progress;
            LockProgressChanged?.Invoke(LockProgress);
        }
    }
}
