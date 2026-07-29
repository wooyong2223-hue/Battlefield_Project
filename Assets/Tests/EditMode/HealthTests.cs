using System.Reflection;
using Battlefield.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Battlefield.Tests
{
    public class HealthTests
    {
        private GameObject _gameObject;
        private Health _health;

        [SetUp]
        public void SetUp()
        {
            _gameObject = new GameObject("Health Test");
            _health = _gameObject.AddComponent<Health>();
            typeof(Health)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(_health, null);
            _health.OnDamaged = new UnityEvent();
            _health.OnHealed = new UnityEvent();
            _health.OnDeath = new UnityEvent();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void StartsAtMaximumHealth()
        {
            Assert.That(_health.CurrentHealth, Is.EqualTo(_health.MaxHealth));
            Assert.That(_health.IsDead, Is.False);
        }

        [Test]
        public void TakeDamageClampsAtZeroAndInvokesDeathOnce()
        {
            int damagedCount = 0;
            int deathCount = 0;
            _health.OnDamaged.AddListener(() => damagedCount++);
            _health.OnDeath.AddListener(() => deathCount++);

            _health.TakeDamage(_health.MaxHealth * 2f);
            _health.TakeDamage(10f);

            Assert.That(_health.CurrentHealth, Is.Zero);
            Assert.That(_health.IsDead, Is.True);
            Assert.That(damagedCount, Is.EqualTo(1));
            Assert.That(deathCount, Is.EqualTo(1));
        }

        [Test]
        public void HealClampsAtMaximumHealth()
        {
            int healedCount = 0;
            _health.OnHealed.AddListener(() => healedCount++);

            _health.TakeDamage(40f);
            _health.Heal(100f);

            Assert.That(_health.CurrentHealth, Is.EqualTo(_health.MaxHealth));
            Assert.That(healedCount, Is.EqualTo(1));
        }

        [Test]
        public void NonPositiveDamageAndHealingAreIgnored()
        {
            int damagedCount = 0;
            int healedCount = 0;
            _health.OnDamaged.AddListener(() => damagedCount++);
            _health.OnHealed.AddListener(() => healedCount++);

            _health.TakeDamage(0f);
            _health.TakeDamage(-10f);
            _health.Heal(0f);
            _health.Heal(-10f);

            Assert.That(_health.CurrentHealth, Is.EqualTo(_health.MaxHealth));
            Assert.That(damagedCount, Is.Zero);
            Assert.That(healedCount, Is.Zero);
        }
    }
}
