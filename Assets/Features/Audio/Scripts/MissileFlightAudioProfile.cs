using UnityEngine;

namespace Battlefield.Features.Audio
{
    [CreateAssetMenu(menuName = "Battlefield/Audio/Missile Flight Profile")]
    public sealed class MissileFlightAudioProfile : AudioProfile
    {
        [SerializeField] private AudioClip _flightLoop;
        [SerializeField] private float _volume = 0.4f;

        public AudioClip FlightLoop => _flightLoop;
        public float Volume => _volume;
    }
}
