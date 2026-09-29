using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Weapon
{
    [DisallowMultipleComponent]
    public sealed class MachineGunAudio : MonoBehaviour
    {
        [SerializeField] private MachineGun _gun;
        [SerializeField] private Health _health;
        [SerializeField] private AudioSource _source;
        [SerializeField] private float _volume = 0.65f;
        [SerializeField] private float _fadeSeconds = 0.05f;

        private float _lastShotTime = float.NegativeInfinity;
        private bool _paused;

        private void Awake()
        {
            if (_gun == null || _source == null || _source.clip == null)
            {
                Debug.LogError("Machine gun audio requires a gun, source, and clip.", this);
                enabled = false;
                return;
            }

            _source.loop = true;
            _source.playOnAwake = false;
            StopPlayback();
        }

        private void OnEnable()
        {
            _lastShotTime = float.NegativeInfinity;
            if (_gun != null)
            {
                _gun.Fired += HandleFired;
            }
        }

        private void LateUpdate()
        {
            if ((_health != null && _health.IsDead) || !_gun.isActiveAndEnabled)
            {
                StopPlayback();
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

            bool firing = Time.time - _lastShotTime <= _gun.ShotInterval * 1.3f;
            if (firing && !_source.isPlaying)
            {
                _source.Play();
            }

            float level = Mathf.Clamp01(_volume);
            _source.volume = Mathf.MoveTowards(
                _source.volume,
                firing ? level : 0f,
                level * Time.deltaTime / Mathf.Max(0.01f, _fadeSeconds));

            if (!firing && _source.volume <= 0f)
            {
                _source.Stop();
            }
        }

        private void OnDisable()
        {
            if (_gun != null)
            {
                _gun.Fired -= HandleFired;
            }
            StopPlayback();
        }

        private void HandleFired()
        {
            _lastShotTime = Time.time;
        }

        private void StopPlayback()
        {
            if (_source != null)
            {
                _source.Stop();
                _source.volume = 0f;
            }
            _paused = false;
        }
    }
}
