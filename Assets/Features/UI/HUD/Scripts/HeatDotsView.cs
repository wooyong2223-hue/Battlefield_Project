using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlefield.Features.Weapon;

namespace Battlefield.Features.UI
{
    public class HeatDotsView : MonoBehaviour
    {
        private const float WarningHeatPercent = 90f;
        private const float MaximumHeatPercent = 100f;

        [Header("Dot Images")]
        [SerializeField] private List<Image> _dots = new();
        [SerializeField] private WeaponBase _weapon;

        [Header("Dot Colors")]
        [SerializeField] private Color _offColor = new(1f, 1f, 1f, 0.15f);
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warnColor = new(1.0f, 0.65f, 0.0f);     // Orange
        [SerializeField] private Color _hotColor = new(0.9f, 0.2f, 0.2f);      // Red

        [Header("Dot Alpha")]
        [SerializeField] private float _activeAlpha = 1f;
        [SerializeField] private float _inactiveAlpha = 0.15f;

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
            if (_weapon == null || _dots.Count == 0) return;

            float maxHeat = Mathf.Max(1f, _weapon.MaxHeat);
            float heatPercent = Mathf.Clamp01(_weapon.Heat / maxHeat) * 100f;

            int litCount = Mathf.RoundToInt((heatPercent / 100f) * _dots.Count);

            Color stateColor = _weapon.IsOverheated
                ? ColorForCooling(heatPercent, maxHeat)
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
            return _normalColor;
        }

        private Color ColorForCooling(float heatPercent, float maxHeat)
        {
            float recoverHeatPercent =
                (_weapon.RecoverHeat / maxHeat) * MaximumHeatPercent;
            float coolingColorRatio = Mathf.InverseLerp(
                recoverHeatPercent,
                MaximumHeatPercent,
                heatPercent);

            return Color.Lerp(
                _normalColor,
                _hotColor,
                coolingColorRatio);
        }
    }
}
