using UnityEngine;

namespace Battlefield.Features.Audio
{
    [CreateAssetMenu(menuName = "Battlefield/Audio/Missile Launch Profile")]
    public sealed class MissileLaunchAudioProfile : AudioProfile
    {
        [SerializeField] private AudioClip _launch;
        [SerializeField] private float _volume = 0.65f;

        public AudioClip Launch => _launch;
        public float Volume => _volume;
    }
}
