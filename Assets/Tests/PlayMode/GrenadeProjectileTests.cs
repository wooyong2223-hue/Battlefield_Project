#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.Training;
using Battlefield.Features.Projectile;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    public sealed class GrenadeProjectileTests
    {
        [UnityTest]
        public IEnumerator AlliedDummyGrenadeDamagesEnemyAndNeutralButNotAlliesAndBouncesBeforeFuse()
        {
            yield return CharacterViewTestContext.Load();
            var owner = Object.FindObjectsByType<DummyShooter>(FindObjectsSortMode.None)
                .First(x => x.GetComponent<Team>()?.CurrentTeam == TeamType.Player).transform;
            owner.SetParent(null,true); owner.position = new Vector3(70000,10000,70000);
            var targets = new[]{"Dummy_Enemy","Dummy_Neutral","Ally_10m"}.Select(name => Object.FindObjectsByType<Health>(FindObjectsSortMode.None).Single(x=>x.name==name)).ToArray();
            var team = owner.GetComponent<Team>(); Assert.That(team.CurrentTeam, Is.EqualTo(TeamType.Player));
            for(int i=0;i<3;i++){targets[i].transform.SetParent(null,true);targets[i].transform.position=owner.position+(i==0 ? new Vector3(-4,0,2) : i==1 ? new Vector3(0,0,6) : new Vector3(4,0,2));}
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position=owner.position+Vector3.down*2;
            floor.transform.localScale=new Vector3(30,1,30);
            var definition=ScriptableObject.CreateInstance<GrenadeDefinition>();
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Features/Projectile/Prefabs/FragGrenade.prefab");
            var grenade = Object.Instantiate(prefab).GetComponent<GrenadeProjectile>();
            grenade.GetComponent<SphereCollider>().radius=.12f;grenade.transform.position=owner.position+Vector3.up*2;
            float[] before=targets.Select(x=>x.CurrentHealth).ToArray();Physics.SyncTransforms();
            try
            {
                Assert.That(grenade.Arm(owner,Vector3.down*2,definition),Is.True);
                bool bounced=false;
                for(int n=0;n<45;n++){yield return new WaitForFixedUpdate();if(grenade.GetComponent<Rigidbody>().linearVelocity.y>.1f)bounced=true;}
                Assert.That(bounced,Is.True,"The gameplay prefab must bounce, not pass through or disappear at contact.");
                Assert.That(grenade!=null && grenade.IsArmed,Is.True,"Impact must not detonate a timed grenade.");
                Assert.That(grenade.GetComponent<Rigidbody>().position.y,Is.GreaterThan(floor.transform.position.y+.45f),"The sphere must collide with the floor.");
                grenade.transform.position=owner.position+Vector3.forward*2;grenade.GetComponent<Rigidbody>().position=grenade.transform.position;Physics.SyncTransforms();
                Assert.That(grenade.Detonate(),Is.True);Assert.That(grenade.Detonate(),Is.False);
                Assert.That(targets[0].CurrentHealth,Is.LessThan(before[0]));Assert.That(targets[1].CurrentHealth,Is.LessThan(before[1]));Assert.That(targets[2].CurrentHealth,Is.EqualTo(before[2]));
            }
            finally{if(grenade!=null)Object.Destroy(grenade.gameObject);Object.Destroy(floor);Object.Destroy(definition);}
        }
        [UnityTest]
        public IEnumerator AlliedDummyGrenadeRespectsCoverAndItsFuse()
        {
            yield return CharacterViewTestContext.Load();
            var owner=Object.FindObjectsByType<DummyShooter>(FindObjectsSortMode.None).First(x=>x.GetComponent<Team>()?.CurrentTeam==TeamType.Player).transform;
            owner.SetParent(null,true);owner.position=new Vector3(70000,10000,70000);
            var enemy=Object.FindObjectsByType<Health>(FindObjectsSortMode.None).Single(x=>x.name=="Dummy_Enemy");enemy.transform.SetParent(null,true);enemy.transform.position=owner.position+Vector3.forward*4;
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=owner.position+Vector3.forward*2;wall.transform.localScale=new Vector3(10,10,.3f);
            var definition=ScriptableObject.CreateInstance<GrenadeDefinition>();CharacterViewTestContext.SetField(definition,"_fuse",.12f);
            var grenade=new GameObject("Covered grenade").AddComponent<GrenadeProjectile>();grenade.transform.position=owner.position+Vector3.forward;
            float before=enemy.CurrentHealth;Physics.SyncTransforms();
            try{grenade.Arm(owner,Vector3.zero,definition);yield return new WaitForSeconds(.3f);Assert.That(grenade==null,Is.True);Assert.That(enemy.CurrentHealth,Is.EqualTo(before));}
            finally{if(grenade!=null)Object.Destroy(grenade.gameObject);Object.Destroy(wall);Object.Destroy(definition);}
        }
    }
}
#endif
