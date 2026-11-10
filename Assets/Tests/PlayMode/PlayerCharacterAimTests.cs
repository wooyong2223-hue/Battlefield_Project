#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.Dummy;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class PlayerCharacterAimTests
    {
        [UnityTest]
        public IEnumerator AlliedDummyAimsAboveBodyForwardAndHonorsEnemyNeutralAndAlliedRules()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            Assert.That(context.Shooter.GetComponent<DummyShooter>(), Is.Not.Null);
            Assert.That(context.Shooter.GetComponent<Team>().CurrentTeam, Is.EqualTo(TeamType.Player));
            context.TogglePerspective();
            context.Input.Look = new Vector2(0f, Mathf.Atan2(4.4f, 30f) * Mathf.Rad2Deg / 3f);
            context.View.Tick(context.Input, 0.02f);
            context.Input.Look = Vector2.zero;
            FireInput fire = new();
            CharacterViewTestContext.SetField(context.Shooter, "_input", fire);
            context.Shooter.enabled = true;
            string[] names = { "Dummy_Enemy", "Dummy_Neutral", "Ally_10m" };
            for (int i = 0; i < names.Length; i++)
            {
                Health target = Object.FindObjectsByType<Health>().Single(item => item.name == names[i]);
                if (i == 2) CharacterViewTestContext.SetField(target.GetComponent<Team>(), "_team", TeamType.Player);
                context.Shooter.GetComponent<CharacterController>().enabled = false;
                context.Actor.position = new Vector3(10000f + i * 100f, 10002f, 10000f);
                context.Shooter.GetComponent<CharacterController>().enabled = true;
                target.transform.SetParent(null, true);
                target.transform.position = context.Actor.position + new Vector3(0f, 6f, 30f);
                Physics.SyncTransforms();
                float before = target.CurrentHealth;
                fire.FireWeapon = true;
                yield return null;
                fire.FireWeapon = false;
                yield return new WaitForSeconds(0.2f);
                Assert.That(target.CurrentHealth, Is.EqualTo(before - (i == 2 ? 0f : 10f)), names[i]);
            }
        }

        [UnityTest]
        public IEnumerator ACameraLookingOverCoverCannotSpawnTheBulletPastTheWall()
        {
            yield return CharacterViewTestContext.Load();
            var context = new CharacterViewTestContext();
            context.TogglePerspective();
            Health target = Object.FindObjectsByType<Health>().Single(item => item.name == "Dummy_Enemy");
            target.transform.SetParent(null, true);
            target.transform.position = context.Actor.position + Vector3.forward * 20f;
            GameObject cover = new("Muzzle Test Cover");
            cover.transform.position = context.Actor.position + Vector3.forward * 0.8f;
            cover.AddComponent<BoxCollider>().size = new Vector3(10f, 1f, 0.1f);
            Physics.SyncTransforms();
            try
            {
                float before = target.CurrentHealth;
                FireInput fire = new() { FireWeapon = true };
                CharacterViewTestContext.SetField(context.Shooter, "_input", fire);
                context.Shooter.enabled = true;
                yield return null;
                fire.FireWeapon = false;
                yield return new WaitForSeconds(0.2f);
                Assert.That(target.CurrentHealth, Is.EqualTo(before));
            }
            finally { Object.Destroy(cover); }
        }

        private sealed class FireInput : IWeaponInput
        {
            public bool FireWeapon { get; set; }
            public int WeaponSlotSelection => 0;
        }
    }
}
#endif
