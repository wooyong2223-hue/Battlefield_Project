using UnityEngine;

namespace Battlefield.Framework.Core
{
    public class Team : MonoBehaviour
    {
        [SerializeField] private TeamType _team = TeamType.Neutral;
        public TeamType CurrentTeam => _team;
    }
}
