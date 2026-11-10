using UnityEngine;
using UnityEngine.Events;

namespace Battlefield.Framework.Core
{
    public class Health : MonoBehaviour, IDamageable, IHealable
    {
        [SerializeField] private float _maxHealth = 100f;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        private IHealthDamagePolicy _damagePolicy;

        public UnityEvent OnDamaged = new();
        public UnityEvent OnHealed = new();
        public UnityEvent OnDeath = new();

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (damage <= 0f) return;
            if (_damagePolicy != null && _damagePolicy.TryHandleDamage(damage)) return;
            if (IsDead) return;

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

        public void BindDamagePolicy(IHealthDamagePolicy policy) => _damagePolicy = policy;
        public void UnbindDamagePolicy(IHealthDamagePolicy policy)
        {
            if (ReferenceEquals(_damagePolicy, policy)) _damagePolicy = null;
        }

        public bool TryRevive(float amount)
        {
            if (!IsDead || amount <= 0f) return false;
            CurrentHealth = Mathf.Min(amount, _maxHealth);
            OnHealed?.Invoke();
            return true;
        }
    }
}
