using UnityEngine;

namespace Battlefield.Features.UI
{
    public sealed class FighterGunAimReticleMover
    {
        private readonly RectTransform _reticleTransform;
        private readonly float _moveSpeed;

        public FighterGunAimReticleMover(
            RectTransform reticleTransform,
            float moveSpeed)
        {
            _reticleTransform = reticleTransform;
            _moveSpeed = Mathf.Max(0f, moveSpeed);
        }

        public void MoveTo(Vector2 targetPosition, float deltaTime)
        {
            if (_reticleTransform == null)
            {
                return;
            }

            float interpolation = 1f - Mathf.Exp(
                -_moveSpeed * Mathf.Max(0f, deltaTime));
            _reticleTransform.anchoredPosition = Vector2.Lerp(
                _reticleTransform.anchoredPosition,
                targetPosition,
                interpolation);
        }
    }
}
