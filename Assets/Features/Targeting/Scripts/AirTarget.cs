using System.Collections.Generic;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Targeting
{
    [RequireComponent(typeof(Team))]
    public sealed class AirTarget : MonoBehaviour
    {
        private static readonly List<AirTarget> Active = new();
        private Team _team;
        private Health _health;

        internal static IReadOnlyList<AirTarget> ActiveTargets => Active;
        public TeamType Team => _team.CurrentTeam;
        public Vector3 AimPosition => transform.position;
        public bool IsAvailable => isActiveAndEnabled && (_health == null || !_health.IsDead);

        private void Awake()
        {
            _team = GetComponent<Team>();
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (!Active.Contains(this)) Active.Add(this);
        }

        private void OnDisable() => Active.Remove(this);
    }
}
