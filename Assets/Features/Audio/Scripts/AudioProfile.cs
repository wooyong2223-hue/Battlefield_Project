using UnityEngine;
using UnityEngine.Audio;

namespace Battlefield.Features.Audio
{
    public abstract class AudioProfile : ScriptableObject
    {
        [SerializeField] private AudioMixerGroup _outputGroup;

        public AudioMixerGroup OutputGroup => _outputGroup;
    }
}
