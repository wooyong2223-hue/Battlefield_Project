using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlefield.Weapon;

namespace Battlefield.UI.HUD
{
    public class HeatDotsView : MonoBehaviour
    {
        [Header("Dot Images")]
        [SerializeField] private List<Image> _dots = new();

        [Header("Dot Colors")]
        [SerializeField] private Color _offColor = new(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _coolColor = new(0.25f, 0.85f, 0.25f);   // Green
        [SerializeField] private Color _warnColor = new(1.0f, 0.65f, 0.0f);     // Orange
        [SerializeField] private Color _hotColor = new(0.9f, 0.2f, 0.2f);      // Red

        [Header("Dot Alpha")]
        [SerializeField, Range(0f, 1f)] private float _activeAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float _inactiveAlpha = 0.15f;

        [Header("Heat Threshold")]
        [SerializeField] private float _warnStartPercent = -1f;
        [SerializeField] private float _hotStartPercent = 70;

        private Overheat _overheat;
        private float _maxHeat;
        private float _warnStart;

        public void Bind(Overheat overheat)
        {
            _overheat = overheat;
            _maxHeat = Mathf.Max(1f, overheat.MaxHeat);

            _warnStart = _warnStartPercent >= 0f
                ? _warnStartPercent
                : (overheat.RecoverHeat / _maxHeat) * 100f;
        }

        [ContextMenu("Collect Dots From Children")]
        private void CollectDotsFromChildren()
        {
            _dots.Clear();

            foreach (Transform child in transform)
            {
                Image image = child.GetComponent<Image>();

                if (image != null)
                {
                    _dots.Add(image);
                }
            }
        }

        private void LateUpdate()
        {
            if (_overheat == null || _dots.Count == 0) return;
            float heatPercent = Mathf.Clamp01(_overheat.Heat / _maxHeat) * 100f;
            bool overheated = _overheat.IsOverheated;

            int litCount = Mathf.RoundToInt((heatPercent / 100f) * _dots.Count);

            Color stateColor = overheated
                ? _hotColor
                : ColorForHeat(heatPercent);

            for (int i = 0; i < _dots.Count; i++)
            {
                Color color = i < litCount
                    ? stateColor
                    : _offColor;

                color.a = i < litCount
                    ? _activeAlpha
                    : _inactiveAlpha;

                _dots[i].color = color;
            }
        }

        private Color ColorForHeat(float heatPercent)
        {
            if (heatPercent <= _warnStart) return _coolColor;
            if (heatPercent >= 100f) return _hotColor;

            if (heatPercent < _hotStartPercent)
            {
                float t = Mathf.InverseLerp(_warnStart, _hotStartPercent, heatPercent);
                return Color.Lerp(_coolColor, _warnColor, t);
            }

            float hotT = Mathf.InverseLerp(_hotStartPercent, 100f, heatPercent);
            return Color.Lerp(_warnColor, _hotColor, hotT);
        }
    }
}