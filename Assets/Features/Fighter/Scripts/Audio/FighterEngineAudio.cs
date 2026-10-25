using Battlefield.Features.Audio;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [DisallowMultipleComponent]
    public sealed class FighterEngineAudio : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private JetMovement _movement;
        [SerializeField] private Afterburner _afterburner;
        [SerializeField] private Health _health;
        [SerializeField] private FighterEngineAudioProfile _profile;

        [Header("Loop Sources")]
        [SerializeField] private AudioSource _engineSource;
        [SerializeField] private AudioSource _afterburnerSource;

        private bool _configured;
        private bool _playing;
        private bool _paused;
        private float _afterburnerBlend;
        private float _engineVolume;

        private void Awake()
        {
            _configured = _profile != null &&
                          _profile.EngineLoop != null &&
                          _profile.AfterburnerLoop != null &&
                          _movement != null && _afterburner != null &&
                          _health != null && _engineSource != null &&
                          _afterburnerSource != null &&
                          _engineSource != _afterburnerSource;

            if (!_configured)
            {
                Debug.LogError("Fighter engine audio requires a profile, state references, and two distinct loop sources.", this);
                enabled = false;
                return;
            }

            _engineSource.clip = _profile.EngineLoop;
            _afterburnerSource.clip = _profile.AfterburnerLoop;
            _engineSource.outputAudioMixerGroup = _profile.OutputGroup;
            _afterburnerSource.outputAudioMixerGroup = _profile.OutputGroup;
            _engineSource.playOnAwake = false;
            _afterburnerSource.playOnAwake = false;
            _engineSource.loop = true;
            _afterburnerSource.loop = true;
            StopPlayback();
        }

        private void LateUpdate()
        {
            // Observe state after movement has ticked the afterburner, including depletion.
            if (!_configured) return;
            if (_health.IsDead || !_movement.isActiveAndEnabled)
            {
                StopPlayback();
                return;
            }

            if (Time.timeScale <= 0f)
            {
                if (_playing && !_paused)
                {
                    _engineSource.Pause();
                    _afterburnerSource.Pause();
                    _paused = true;
                }
                return;
            }

            if (_paused)
            {
                _engineSource.UnPause();
                _afterburnerSource.UnPause();
                _paused = false;
            }

            if (!_playing)
            {
                _engineSource.Play();
                _afterburnerSource.Play();
                _playing = true;
            }

            float step = Time.deltaTime /
                         Mathf.Max(0.01f, _profile.TransitionSeconds);
            float speed = _movement.IsUnpowered ? 0f : Mathf.Clamp01(
                _movement.EffectiveForwardSpeed / Mathf.Max(1f, _movement.MaxSpeed));
            float engineTarget = Mathf.Lerp(
                Mathf.Clamp01(_profile.IdleVolume),
                Mathf.Clamp01(_profile.FlightVolume), speed);
            _engineVolume = Mathf.MoveTowards(_engineVolume, engineTarget, step);
            float boostTarget = _afterburner.isActiveAndEnabled && _afterburner.IsActive ? 1f : 0f;
            _afterburnerBlend = Mathf.MoveTowards(_afterburnerBlend, boostTarget, step);

            // Both recordings contain engine sound: crossfade instead of adding two full engines.
            _engineSource.volume = _engineVolume * (1f - _afterburnerBlend);
            _afterburnerSource.volume =
                Mathf.Clamp01(_profile.AfterburnerVolume) * _afterburnerBlend;
            _engineSource.pitch = Mathf.Lerp(
                Mathf.Clamp(_profile.IdlePitch, 0.1f, 3f),
                Mathf.Clamp(_profile.FlightPitch, 0.1f, 3f), speed);
            _afterburnerSource.pitch = 1f;
        }

        private void OnDisable()
        {
            StopPlayback();
        }

        private void StopPlayback()
        {
            if (_engineSource != null)
            {
                _engineSource.Stop();
                _engineSource.volume = 0f;
            }
            if (_afterburnerSource != null)
            {
                _afterburnerSource.Stop();
                _afterburnerSource.volume = 0f;
            }
            _playing = false;
            _paused = false;
            _afterburnerBlend = 0f;
            _engineVolume = 0f;
        }
    }
}
