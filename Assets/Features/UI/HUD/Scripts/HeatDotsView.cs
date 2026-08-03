using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlefield.Weapon;

namespace Battlefield.UI.HUD
{
    public class HeatDotsView : MonoBehaviour
    {
        private const float WarningHeatPercent = 90f;
        private const float MaximumHeatPercent = 100f;

        [Header("Dot Images")]
        [SerializeField] private List<Image> _dots = new();

        [Header("Dot Colors")]
        [SerializeField] private Color _offColor = new(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _warnColor = new(1.0f, 0.65f, 0.0f);     // Orange
        [SerializeField] private Color _hotColor = new(0.9f, 0.2f, 0.2f);      // Red

        [Header("Dot Alpha")]
        [SerializeField] private float _activeAlpha = 1f;
        [SerializeField] private float _inactiveAlpha = 0.15f;

        private Overheat _overheat;
        private float _maxHeat;

        public void Bind(Overheat overheat)
        {
            _overheat = overheat;
            _maxHeat = Mathf.Max(1f, overheat.MaxHeat);
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

            int litCount = Mathf.RoundToInt((heatPercent / 100f) * _dots.Count);

            Color stateColor = _overheat.IsOverheated
                ? ColorForCooling(heatPercent)
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
            if (heatPercent >= MaximumHeatPercent) return _hotColor;
            if (heatPercent >= WarningHeatPercent) return _warnColor;
            return Color.white;
        }

        private Color ColorForCooling(float heatPercent)
        {
            float recoverHeatPercent =
                (_overheat.RecoverHeat / _maxHeat) * MaximumHeatPercent;
            float coolingColorRatio = Mathf.InverseLerp(
                recoverHeatPercent,
                MaximumHeatPercent,
                heatPercent);

            return Color.Lerp(
                Color.white,
                _hotColor,
                coolingColorRatio);
        }
    }
}
