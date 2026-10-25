using UnityEngine;

namespace Battlefield.Features.Audio
{
    [CreateAssetMenu(menuName = "Battlefield/Audio/Machine Gun Profile")]
    public sealed class MachineGunAudioProfile : AudioProfile
    {
        [SerializeField] private AudioClip _fireLoop;
        [SerializeField] private float _volume = 0.65f;
        [SerializeField] private float _fadeSeconds = 0.05f;

        public AudioClip FireLoop => _fireLoop;
        public float Volume => _volume;
        public float FadeSeconds => _fadeSeconds;
    }
}
