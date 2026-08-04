using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.Dummy
{
    public class DummyDebug : MonoBehaviour
    {
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        public void LogDamaged()
        {
            Debug.Log(
                $"{name} 피격 남은 체력: {_health.CurrentHealth}/{_health.MaxHealth}");
        }

        public void LogDeath()
        {
            Debug.Log($"{name} 사망");
        }
    }
}
