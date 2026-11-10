#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class InfantryLifeStateTests
    {
        private readonly List<GameObject> _objects = new();
        private readonly Vector3 _origin = new(70000f, 10000f, 70000f);
        private static void Set(object item, string field, object value) => CharacterViewTestContext.SetField(item, field, value);
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var item in _objects) if (item != null) Object.Destroy(item);
            _objects.Clear(); yield return null;
        }
        private InfantryLifeState Soldier(Vector3 offset, TeamType team = TeamType.Player)
        {
            var actor = new GameObject("Life test soldier"); _objects.Add(actor); actor.transform.position = _origin + offset;
            actor.AddComponent<BoxCollider>().size = Vector3.one;
            var member = actor.AddComponent<Team>(); Set(member, "_team", team);
            actor.AddComponent<Health>(); return actor.AddComponent<InfantryLifeState>();
        }
        private PlayerCharacterRevive Rescuer(InfantryLifeState soldier)
        {
            var revive = soldier.gameObject.AddComponent<PlayerCharacterRevive>();
            Set(revive, "_health", soldier.GetComponent<Health>()); Set(revive, "_team", soldier.GetComponent<Team>());
            return revive;
        }
        [Test]
        public void FatalDamageDownsFinishingDamageEliminatesAndOrdinaryHealingCannotRevive()
        {
            var soldier = Soldier(Vector3.zero); var health = soldier.GetComponent<Health>();
            health.TakeDamage(health.MaxHealth); Assert.That(soldier.IsDowned, Is.True);
            health.Heal(100f); Assert.That(health.IsDead, Is.True);
            health.TakeDamage(20f); Assert.That(soldier.IsDowned, Is.True);
            health.TakeDamage(20f); Assert.That(soldier.IsEliminated, Is.True);
            Assert.That(soldier.TryRevive(), Is.False);
        }
        [Test]
        public void ReviveRestoresAFractionAndBleedOutAndRespawnResetTheLifecycle()
        {
            var soldier = Soldier(Vector3.zero); var health = soldier.GetComponent<Health>();
            health.TakeDamage(health.MaxHealth); Assert.That(soldier.TryRevive(), Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth * .35f).Within(.001f));
            health.TakeDamage(health.MaxHealth); soldier.Tick(30f); Assert.That(soldier.IsEliminated, Is.True);
            soldier.transform.position += Vector3.right * 10f; Assert.That(soldier.TryRespawn(), Is.True);
            Assert.That(soldier.transform.position, Is.EqualTo(_origin)); Assert.That(health.CurrentHealth, Is.EqualTo(health.MaxHealth));
            Assert.That(soldier.State, Is.EqualTo(InfantryLife.Alive));
            health.TakeDamage(health.MaxHealth); Assert.That(soldier.IsDowned, Is.True);
        }
        [Test]
        public void ReviveRequiresAnAllyRangeAndAnUnobstructedPathAndCanBeCancelled()
        {
            var own = Soldier(Vector3.zero); var target = Soldier(Vector3.forward * 2f);
            var revive = Rescuer(own); target.GetComponent<Health>().TakeDamage(100f);
            Assert.That(revive.TryBegin(target), Is.True); revive.Tick(true, false, 1f); Assert.That(revive.Progress, Is.GreaterThan(0f));
            revive.Tick(false, false, .1f); Assert.That(revive.IsReviving, Is.False); Assert.That(target.IsDowned, Is.True);
            var wall = new GameObject("Revive blocking wall"); _objects.Add(wall);
            wall.transform.position = _origin + Vector3.forward; wall.AddComponent<BoxCollider>().size = new Vector3(5f, 5f, .1f); Physics.SyncTransforms();
            Assert.That(revive.TryBegin(target), Is.False); wall.GetComponent<Collider>().enabled = false;
            Set(target.GetComponent<Team>(), "_team", TeamType.Enemy); Assert.That(revive.TryBegin(target), Is.False);
            Set(target.GetComponent<Team>(), "_team", TeamType.Player); target.transform.position += Vector3.forward * 4f; Physics.SyncTransforms(); Assert.That(revive.TryBegin(target), Is.False);
            target.transform.position = _origin + Vector3.forward * 2f; Physics.SyncTransforms();
            Assert.That(revive.TryBegin(target), Is.True); revive.Tick(true, false, 4f); Assert.That(target.State, Is.EqualTo(InfantryLife.Alive));
        }
        [Test]
        public void OnlyOneRescuerCanOwnAReviveAndDisableReleasesTheClaim()
        {
            var own = Soldier(Vector3.zero); var other = Soldier(Vector3.right); var target = Soldier(Vector3.forward * 2f);
            target.GetComponent<Health>().TakeDamage(100f); var first = Rescuer(own); var second = Rescuer(other);
            Assert.That(first.TryBegin(target), Is.True); Assert.That(second.TryBegin(target), Is.False);
            first.enabled = false; Assert.That(second.TryBegin(target), Is.True);
        }
        [Test]
        public void HealthWithoutAnInfantryPolicyKeepsTheExistingDeadAndHealRules()
        {
            var actor = new GameObject("Existing health consumer"); _objects.Add(actor); var health = actor.AddComponent<Health>();
            health.TakeDamage(10f); health.Heal(5f); Assert.That(health.CurrentHealth, Is.EqualTo(95f));
            health.TakeDamage(100f); health.Heal(100f); health.TakeDamage(100f); Assert.That(health.CurrentHealth, Is.Zero);
        }
    }
}
#endif
