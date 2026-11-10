using UnityEngine;
using UnityEngine.Rendering;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(50)]
    public sealed class PlayerCharacterVisibility : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterView _view;
        [SerializeField] private PlayerCharacterCamera _camera;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField, Min(0f)] private float _hideDistance = 0.8f;

        private ShadowCastingMode[] _originalModes;
        private bool _hidden;

        private void Awake()
        {
            if (_view == null || _camera == null || _renderers == null || _renderers.Length == 0)
            {
                Debug.LogError("Player character visibility references are missing.", this);
                enabled = false;
                return;
            }
            _originalModes = new ShadowCastingMode[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    Debug.LogError("Player character renderer is missing.", this);
                    enabled = false;
                    return;
                }
                _originalModes[i] = _renderers[i].shadowCastingMode;
            }
        }

        private void LateUpdate()
        {
            float distance = _camera.DistanceFromCharacter;
            bool hide = _view.IsFirstPersonView || distance >= 0f && distance < _hideDistance;
            if (_hidden == hide) return;
            _hidden = hide;
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].shadowCastingMode = hide ? ShadowCastingMode.ShadowsOnly : _originalModes[i];
        }

        private void OnDisable()
        {
            if (!_hidden || _originalModes == null) return;
            for (int i = 0; i < _renderers.Length; i++)
                if (_renderers[i] != null) _renderers[i].shadowCastingMode = _originalModes[i];
            _hidden = false;
        }
    }
}
