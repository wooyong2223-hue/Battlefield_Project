#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Training;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    public class PersonalCombatMechanicsTests
    {
        [UnityTest]
        public IEnumerator MagazineSupportsDryFirePartialReloadAndCancellation()
        {
            var item = new GameObject("Magazine test"); item.SetActive(false);
            var magazine = item.AddComponent<WeaponMagazine>();Set(magazine,"_capacity",3);Set(magazine,"_startingReserve",2);Set(magazine,"_reloadDuration",.05f);item.SetActive(true);
            try
            {
                for(int i=0;i<3;i++)Assert.That(magazine.TryConsumeRound(),Is.True);
                Assert.That(magazine.TryConsumeRound(),Is.False);
                Assert.That(magazine.TryReload(),Is.True);Assert.That(magazine.TryConsumeRound(),Is.False);
                magazine.CancelReload();yield return new WaitForSeconds(.08f);
                Assert.That(magazine.Rounds,Is.Zero);Assert.That(magazine.Reserve,Is.EqualTo(2));
                Assert.That(magazine.TryReload(),Is.True);yield return new WaitForSeconds(.08f);
                Assert.That(magazine.Rounds,Is.EqualTo(2));Assert.That(magazine.Reserve,Is.Zero);
                Assert.That(magazine.TryReload(),Is.False);
            }
            finally{Object.Destroy(item);}
        }

        [UnityTest]
        public IEnumerator AlliedDummyMeleeDamagesEnemyAndNeutralButStopsOnAllies()
        {
            yield return BoardingTestContext.Load();
            Transform owner = AlliedOwner();var attack=owner.gameObject.AddComponent<MeleeAttack>();Set(attack,"_windup",.01f);Set(attack,"_recovery",.02f);
            string[] names={"Dummy_Enemy","Dummy_Neutral","Ally_10m"};
            for(int i=0;i<names.Length;i++)
            {
                Health target=Object.FindObjectsByType<Health>().Single(h=>h.name==names[i]);target.transform.SetParent(null,true);
                target.transform.position=owner.position+Vector3.forward*3;
                if(i==2)Set(target.GetComponent<Team>(),"_team",TeamType.Player);
                Physics.SyncTransforms();float before=target.CurrentHealth;
                Assert.That(attack.TryAttack(owner,owner.position,Vector3.forward),Is.True);
                yield return new WaitForSeconds(.08f);
                Assert.That(target.CurrentHealth,Is.EqualTo(before-(i==2?0:20)),names[i]);
                target.transform.position=owner.position+Vector3.right*(50+i*10);
            }
        }

        [UnityTest]
        public IEnumerator MeleeCannotHitThroughWallsAndCancelledAttacksCauseNoDelayedDamage()
        {
            yield return BoardingTestContext.Load();Transform owner=AlliedOwner();
            var attack=owner.gameObject.AddComponent<MeleeAttack>();Set(attack,"_windup",.05f);Set(attack,"_recovery",.05f);
            Health target=Object.FindObjectsByType<Health>().Single(h=>h.name=="Dummy_Enemy");target.transform.SetParent(null,true);target.transform.position=owner.position+Vector3.forward*3;
            var wall=new GameObject("Melee blocking wall");wall.transform.position=owner.position+Vector3.forward*1.5f;wall.AddComponent<BoxCollider>().size=new Vector3(4,4,.2f);Physics.SyncTransforms();
            float before=target.CurrentHealth;Assert.That(attack.TryAttack(owner,owner.position,Vector3.forward),Is.True);
            yield return new WaitForSeconds(.15f);Assert.That(target.CurrentHealth,Is.EqualTo(before));Object.Destroy(wall);yield return null;
            Assert.That(attack.TryAttack(owner,owner.position,Vector3.forward),Is.True);attack.enabled=false;
            yield return new WaitForSeconds(.15f);Assert.That(target.CurrentHealth,Is.EqualTo(before));
            attack.enabled=true;Assert.That(attack.TryAttack(owner,owner.position,Vector3.forward),Is.True);
            yield return new WaitForSeconds(.15f);Assert.That(target.CurrentHealth,Is.EqualTo(before-20));
            yield return new WaitForSeconds(.1f);Assert.That(target.CurrentHealth,Is.EqualTo(before-20));
        }
        private static Transform AlliedOwner()
        {
            foreach(var shooter in Object.FindObjectsByType<DummyShooter>())shooter.enabled=false;
            var owner=Object.FindObjectsByType<PlayerVehicleOccupant>().Single().transform;
            Assert.That(owner.GetComponent<DummyShooter>(),Is.Not.Null);
            Assert.That(owner.GetComponent<Team>().CurrentTeam,Is.EqualTo(TeamType.Player));
            owner.GetComponent<PlayerCharacterCombatController>().enabled=false;owner.GetComponent<PlayerCharacterMovement>().enabled=false;
            var body=owner.GetComponent<CharacterController>();body.enabled=false;owner.position=new Vector3(30000,12000,30000);body.enabled=true;Physics.SyncTransforms();return owner;
        }
        private static void Set(object target,string name,object value)=>target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
    }
}
#endif
