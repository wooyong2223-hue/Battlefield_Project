#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.Training;
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
            var fire = context.EnableCombatInput();
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
                fire.PressFire();
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
            var fire = context.EnableCombatInput();
            yield return null;
            yield return null;
            Health target = Object.FindObjectsByType<Health>().Single(item => item.name == "Dummy_Enemy");
            target.transform.SetParent(null, true);
            target.transform.position = context.Actor.position + Vector3.forward * 20f;
            GameObject cover = new("Muzzle Test Cover");
            try
            {
                Vector3 anchor = context.Actor.GetComponent<CharacterController>().bounds.center;
                Vector3 muzzleOffset = context.Shooter.Muzzle.position - anchor;
                Vector3 wallMidpoint = anchor + muzzleOffset * .5f;
                Ray aim = context.Follow.GetAimRay();
                float wallTop = (wallMidpoint.y + aim.origin.y) * .5f;
                float wallBottom = wallMidpoint.y - .5f;
                cover.transform.position = new Vector3(wallMidpoint.x, (wallTop + wallBottom) * .5f, wallMidpoint.z);
                var collider = cover.AddComponent<BoxCollider>();
                collider.size = new Vector3(3f, wallTop - wallBottom, .04f);
                Physics.SyncTransforms();
                Assert.That(collider.Raycast(aim, out _, 20f), Is.False, "The camera must see past the cover.");
                Assert.That(collider.Raycast(new Ray(anchor, muzzleOffset), out _, muzzleOffset.magnitude), Is.True,
                    "The cover must intersect the path from the body to the muzzle.");
                float before = target.CurrentHealth;
                int shots = 0;
                context.Shooter.Fired += () => shots++;
                fire.PressFire();
                yield return null;
                fire.FireWeapon = false;
                Assert.That(shots, Is.EqualTo(1), "The cover test must fire a shot.");
                yield return new WaitForSeconds(0.2f);
                Assert.That(target.CurrentHealth, Is.EqualTo(before));
            }
            finally { Object.Destroy(cover); }
        }

    }
}
#endif
