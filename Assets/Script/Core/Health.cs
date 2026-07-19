using UnityEngine;
using UnityEngine.Events;

namespace Battlefield.Core
{
    public class Health : MonoBehaviour, IDamageable, IHealable
    {
        [SerializeField] private float _maxHealth = 100f;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public UnityEvent OnDamaged;
        public UnityEvent OnHealed;
        public UnityEvent OnDeath;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;
            if (damage <= 0f) return;

            CurrentHealth = Mathf.Max(CurrentHealth - damage, 0f);
            OnDamaged?.Invoke();

            if (IsDead) Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            if(amount <= 0f) return;

            CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
            OnHealed?.Invoke();
        }

        public void Die()
        {
            OnDeath?.Invoke();
        }
    }
}
