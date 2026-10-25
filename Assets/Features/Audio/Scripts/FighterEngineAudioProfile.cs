using UnityEngine;

namespace Battlefield.Features.Audio
{
    [CreateAssetMenu(menuName = "Battlefield/Audio/Fighter Engine Profile")]
    public sealed class FighterEngineAudioProfile : AudioProfile
    {
        [SerializeField] private AudioClip _engineLoop;
        [SerializeField] private AudioClip _afterburnerLoop;
        [SerializeField] private float _idleVolume = 0.2f;
        [SerializeField] private float _flightVolume = 0.5f;
        [SerializeField] private float _afterburnerVolume = 0.5f;
        [SerializeField] private float _idlePitch = 0.9f;
        [SerializeField] private float _flightPitch = 1.1f;
        [SerializeField] private float _transitionSeconds = 0.2f;

        public AudioClip EngineLoop => _engineLoop;
        public AudioClip AfterburnerLoop => _afterburnerLoop;
        public float IdleVolume => _idleVolume;
        public float FlightVolume => _flightVolume;
        public float AfterburnerVolume => _afterburnerVolume;
        public float IdlePitch => _idlePitch;
        public float FlightPitch => _flightPitch;
        public float TransitionSeconds => _transitionSeconds;
    }
}
