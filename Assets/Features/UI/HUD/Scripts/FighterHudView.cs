using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Battlefield.Framework.Core;
using Battlefield.Features.Fighter;

namespace Battlefield.Features.UI
{
    public class FighterHudView : MonoBehaviour
    {
        [Header("Fighter")]
        [SerializeField] private JetMovement _movement;
        [SerializeField] private Health _health;

        [Header("Text")]
        [SerializeField] private TMP_Text _speedText;

        [Header("Health")]
        [SerializeField] private Image[] _healthSegments;

        private int _displayedSpeed = -1;
        private float _displayedHealth = -1f;
        private float _displayedMaxHealth = -1f;

        private void LateUpdate()
        {
            UpdateSpeed();
            UpdateHealth();
        }

        private void UpdateSpeed()
        {
            if (_movement == null || _speedText == null) return;

            int speed = Mathf.RoundToInt(_movement.CurrentSpeed);
            if (speed == _displayedSpeed) return;

            _displayedSpeed = speed;
            _speedText.SetText($"SPD {speed:000}");
        }

        private void UpdateHealth()
        {
            if (_health == null ||
                _healthSegments == null ||
                _healthSegments.Length == 0)
            {
                return;
            }

            float health = _health.CurrentHealth;
            float maxHealth = _health.MaxHealth;
            if (Mathf.Approximately(health, _displayedHealth) &&
                Mathf.Approximately(maxHealth, _displayedMaxHealth))
            {
                return;
            }

            _displayedHealth = health;
            _displayedMaxHealth = maxHealth;
            float healthRatio = maxHealth > 0f
                ? Mathf.Clamp01(health / maxHealth)
                : 0f;

            for (int i = 0; i < _healthSegments.Length; i++)
            {
                Image segment = _healthSegments[i];
                if (segment == null) continue;

                segment.fillAmount = Mathf.Clamp01(
                    healthRatio * _healthSegments.Length - i);
            }
        }
    }
}
