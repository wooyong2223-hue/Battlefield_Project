using UnityEngine;
namespace Battlefield.Features.Projectile
{
    [DisallowMultipleComponent]
    public sealed class MissileFlightAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        private bool _pendingStart;
        private bool _paused;
        private void Awake()
        {
            if (_source == null || _source.clip == null)
            {
                Debug.LogError("Missile flight audio requires a source and clip.", this);
                enabled = false;
                return;
            }
            _source.playOnAwake = false;
            _source.loop = true;
        }
        private void OnEnable() => _pendingStart = true;
        private void LateUpdate()
        {
            if (Time.timeScale <= 0f)
            {
                if (!_paused && _source.isPlaying) { _source.Pause(); _paused = true; }
                return;
            }
            if (_paused) { _source.UnPause(); _paused = false; }
            // Pool activation precedes launch positioning; wait until LateUpdate.
            if (!_pendingStart) return;
            _pendingStart = false;
            _source.Play();
        }
        private void OnDisable()
        {
            if (_source != null) _source.Stop();
            _pendingStart = false;
            _paused = false;
        }
    }
}
