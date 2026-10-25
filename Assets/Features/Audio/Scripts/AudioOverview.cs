using UnityEngine;

namespace Battlefield.Features.Audio
{
    [DisallowMultipleComponent]
    public sealed class AudioOverview : MonoBehaviour
    {
        [SerializeField] private GameObject[] _relatedPrefabs;
    }
}
