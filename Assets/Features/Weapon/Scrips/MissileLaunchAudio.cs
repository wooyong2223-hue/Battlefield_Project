using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    [DisallowMultipleComponent]
    public sealed class MissileLaunchAudio : MonoBehaviour
    {
        [SerializeField] private AirToAirMissileLauncher _launcher;
        [SerializeField] private Health _health;
        [SerializeField] private AudioSource _source;

        private bool _pendingLaunch;
        private bool _paused;

        private void Awake()
        {
            if (_launcher == null || _source == null || _source.clip == null)
            {
                Debug.LogError("Missile launch audio requires a launcher, source, and clip.", this);
                enabled = false;
                return;
            }

            _source.playOnAwake = false;
            _source.loop = false;
        }

        private void OnEnable()
        {
            _pendingLaunch = false;
            if (_launcher != null)
            {
                _launcher.Fired += HandleFired;
            }
        }

        private void LateUpdate()
        {
            if ((_health != null && _health.IsDead) || !_launcher.isActiveAndEnabled)
            {
                _source.Stop();
                _paused = false;
                _pendingLaunch = false;
                return;
            }

            if (Time.timeScale <= 0f)
            {
                if (!_paused && _source.isPlaying)
                {
                    _source.Pause();
                    _paused = true;
                }
                return;
            }

            if (_paused)
            {
                _source.UnPause();
                _paused = false;
            }

            if (!_pendingLaunch) return;
            _pendingLaunch = false;
            _source.PlayOneShot(_source.clip);
        }

        private void OnDisable()
        {
            if (_launcher != null)
            {
                _launcher.Fired -= HandleFired;
            }
            if (_source != null) _source.Stop();
            _pendingLaunch = false;
            _paused = false;
        }

        private void HandleFired()
        {
            _pendingLaunch = true;
        }
    }
}
