#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.VFX;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class InfantryTrainingGroundTests
    {
        private static IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/SampleScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            foreach(var s in Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include)) s.enabled=false;
            foreach(var s in Object.FindObjectsByType<DummyMissileShooter>()) s.enabled=false;
        }

        [UnityTest]
        public IEnumerator AlliedDummyHitsNewRangeTargetsAndBlocksFriendlyDamage()
        {
            yield return Load();
            var shooter=Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include).First(s=>s.GetComponent<Team>().CurrentTeam==TeamType.Player);
            shooter.GetComponent<PlayerCharacterController>().enabled=false;
            shooter.GetComponent<PlayerCharacterMovement>().enabled=false;
            var body=shooter.GetComponent<CharacterController>();
            var root=GameObject.Find("InfantryTrainingGround").transform;
            var targets=root.GetComponentsInChildren<Health>();
            Assert.That(targets.Length,Is.EqualTo(9));
            foreach(var target in targets)
            {
                body.enabled=false;
                shooter.transform.SetPositionAndRotation(new Vector3(target.transform.position.x,root.position.y+2,root.position.z-40),Quaternion.identity);
                body.enabled=true;
                typeof(DummyShooter).GetField("_target",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(shooter,target.transform);
                Physics.SyncTransforms(); float before=target.CurrentHealth;
                typeof(DummyShooter).GetMethod("Fire",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(shooter,null);
                yield return new WaitForSeconds(.3f);
                float expected=target.GetComponent<Team>().CurrentTeam==TeamType.Player?before:before-10;
                Assert.That(target.CurrentHealth,Is.EqualTo(expected),target.name);
            }
        }

        [UnityTest]
        public IEnumerator AlliedDummyHitsEachMaterialAndSpawnsItsEffectAndBulletHole()
        {
            yield return Load();
            var shooter=Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include).First(s=>s.GetComponent<Team>().CurrentTeam==TeamType.Player);
            shooter.GetComponent<PlayerCharacterController>().enabled=false;
            shooter.GetComponent<PlayerCharacterMovement>().enabled=false;
            var body=shooter.GetComponent<CharacterController>();
            var root=GameObject.Find("InfantryTrainingGround").transform;
            var area=root.Find("MaterialImpactRange");
            Assert.That(area,Is.Not.Null);
            Assert.That(area.childCount,Is.EqualTo(5));
            Assert.That(area.GetComponentsInChildren<BoxCollider>().Length,Is.EqualTo(5));
            var database=Object.FindAnyObjectByType<HitEffectDatabase>();
            string[] names={"Concreat","Dirt","Metal","Sand","Wood"};
            foreach(string name in names)
            {
                var target=area.Find("MaterialTarget_"+name);
                Assert.That(target,Is.Not.Null);
                var material=target.GetComponent<Collider>().sharedMaterial;
                Assert.That(GameObject.Find("Dummy_"+name),Is.Null,name+" obsolete dummy");
                var original=AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Art/PhysicMaterial/"+name+".physicMaterial");
                Assert.That(original,Is.Not.Null,name);
                Assert.That(material,Is.SameAs(original),name);
                var entry=database.GetEntry(material);
                Assert.That(entry,Is.Not.Null,name);
                Assert.That(entry.EffectPool,Is.Not.Null,name);
                Assert.That(entry.BulletHolePool,Is.Not.Null,name);
                int effects=entry.EffectPool.GetComponentsInChildren<HitEffect>().Length;
                int holes=entry.BulletHolePool.GetComponentsInChildren<BulletHole>().Length;
                body.enabled=false;
                shooter.transform.SetPositionAndRotation(new Vector3(target.position.x,root.position.y+3,target.position.z-10.5f),Quaternion.identity);
                body.enabled=true;
                typeof(DummyShooter).GetField("_target",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(shooter,target);
                Physics.SyncTransforms();
                Assert.That(Physics.Raycast(shooter.transform.position,Vector3.forward,out var hit,20),Is.True,name);
                Assert.That(hit.collider,Is.SameAs(target.GetComponent<Collider>()),name);
                typeof(DummyShooter).GetMethod("Fire",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(shooter,null);
                yield return new WaitForSeconds(.15f);
                Assert.That(entry.EffectPool.GetComponentsInChildren<HitEffect>().Length,Is.EqualTo(effects+1),name+" effect");
                Assert.That(entry.BulletHolePool.GetComponentsInChildren<BulletHole>().Length,Is.EqualTo(holes+1),name+" bullet hole");
                Assert.That(entry.BulletHolePool.GetComponentsInChildren<BulletHole>().Any(h=>Vector3.Distance(h.transform.position,hit.point)<.3f),Is.True,name+" impact position");
            }
        }

        [UnityTest]
        public IEnumerator AuthoredRangeFloorSupportsCharacterJumpAndLanding()
        {
            yield return Load();
            var motor=Object.FindObjectsByType<PlayerCharacterMovement>().Single();
            motor.GetComponent<PlayerCharacterController>().enabled=false;
            typeof(PlayerCharacterMovement).GetField("_input",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(motor,null);
            var body=motor.GetComponent<CharacterController>();
            var root=GameObject.Find("InfantryTrainingGround").transform;
            body.enabled=false; motor.transform.position=root.position+new Vector3(0,2.1f,-45); body.enabled=true;
            motor.Stop(); Physics.SyncTransforms();
            for(int i=0;i<20;i++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
            Assert.That(motor.IsGrounded,Is.True);
            float start=motor.transform.position.y,peak=start;
            for(int i=0;i<80;i++)
            {
                motor.Tick(Vector2.zero,false,i==0,.02f);
                peak=Mathf.Max(peak,motor.transform.position.y);yield return null;
            }
            Assert.That(peak-start,Is.EqualTo(1.2f).Within(.15f));
            Assert.That(motor.IsGrounded,Is.True);
            Assert.That(motor.transform.position.y,Is.EqualTo(start).Within(.06f));
        }
    }
}
#endif
