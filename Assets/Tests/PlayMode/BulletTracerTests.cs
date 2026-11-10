#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Projectile;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Battlefield.Tests.PlayMode
{
    public class BulletTracerTests
    {
        static BulletTracer Tracer(Bullet bullet)=> (BulletTracer)new SerializedObject(bullet).FindProperty("_tracer").objectReferenceValue;
        static IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/SampleScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            foreach(var s in Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include))s.enabled=false;
            foreach(var s in Object.FindObjectsByType<DummyMissileShooter>())s.enabled=false;
        }
        [UnityTest] public IEnumerator AlliedDummyTracerStopsAtActualImpactAndFadesAfterPoolReturn()
        {
            yield return Load();
            var shooter=Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include).First(s=>s.GetComponent<Team>().CurrentTeam==TeamType.Player);
            shooter.GetComponent<PlayerCharacterController>().enabled=false;shooter.GetComponent<PlayerCharacterMovement>().enabled=false;
            var body=shooter.GetComponent<CharacterController>();body.enabled=false;
            shooter.transform.SetPositionAndRotation(new Vector3(10000,10000,10000),Quaternion.identity);body.enabled=true;
            var target=GameObject.Find("Dummy_Enemy");target.transform.SetParent(null,true);target.transform.position=shooter.transform.position+Vector3.forward*15;
            typeof(DummyShooter).GetField("_target",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(shooter,target.transform);Physics.SyncTransforms();
            Assert.That(Physics.Raycast(shooter.transform.position,Vector3.forward,out var hit,20),Is.True);
            float health=target.GetComponent<Health>().CurrentHealth;
            typeof(DummyShooter).GetMethod("Fire",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(shooter,null);
            var bullet=Object.FindAnyObjectByType<BulletPool>().GetComponentsInChildren<Bullet>().Last();var tracer=Tracer(bullet);var line=tracer.GetComponent<LineRenderer>();
            yield return new WaitForFixedUpdate();
            Assert.That(bullet.gameObject.activeSelf,Is.False);
            Assert.That(tracer.gameObject.activeSelf,Is.True);
            Assert.That(line.positionCount,Is.EqualTo(2));
            Assert.That(Vector3.Distance(line.GetPosition(1),hit.point),Is.LessThan(.03f));
            Assert.That(Vector3.Distance(line.GetPosition(0),line.GetPosition(1)),Is.LessThanOrEqualTo(4.001f));
            Assert.That(target.GetComponent<Health>().CurrentHealth,Is.EqualTo(health-10));
            yield return new WaitForSeconds(.08f);
            Assert.That(tracer.gameObject.activeSelf,Is.False);Assert.That(line.positionCount,Is.Zero);
        }
        [UnityTest] public IEnumerator ReuseClearsOldSegmentAndUsesWeaponDiameter()
        {
            yield return Load();var pool=Object.FindAnyObjectByType<BulletPool>();var bullet=pool.Get();var tracer=Tracer(bullet);var line=tracer.GetComponent<LineRenderer>();
            tracer.Begin(Vector3.zero,.04f);tracer.Advance(Vector3.forward*20);tracer.Stop();
            tracer.Begin(new Vector3(100,5,20),.02f);
            Assert.That(line.GetPosition(0),Is.EqualTo(new Vector3(100,5,20)));Assert.That(line.GetPosition(1),Is.EqualTo(line.GetPosition(0)));
            Assert.That(line.widthMultiplier,Is.EqualTo(.12f).Within(.00001f));
            tracer.Begin(Vector3.zero,.04f);Assert.That(line.widthMultiplier,Is.EqualTo(.24f).Within(.00001f));
            pool.Return(bullet);yield return new WaitForSeconds(.08f);Assert.That(line.positionCount,Is.Zero);
        }
        [UnityTest] public IEnumerator DestroyedBulletCleansUpDetachedTracer()
        {
            yield return Load();var bullet=Object.FindAnyObjectByType<BulletPool>().Get();var tracer=Tracer(bullet);
            bullet.Initialize(new ProjectileData(null,10));Assert.That(tracer.transform.parent,Is.Null);
            Object.Destroy(bullet.gameObject);yield return null;yield return null;Assert.That(tracer==null,Is.True);
        }
        [UnityTest] public IEnumerator FighterLengthMultiplierResetsForInfantryReuse()
        {
            yield return Load();var pool=Object.FindAnyObjectByType<BulletPool>();var bullet=pool.Get();var tracer=Tracer(bullet);var line=tracer.GetComponent<LineRenderer>();
            bullet.InitializeWithTracer(new ProjectileData(null,10),3f);tracer.Advance(bullet.transform.position+Vector3.forward*100);
            Assert.That(Vector3.Distance(line.GetPosition(0),line.GetPosition(1)),Is.EqualTo(12).Within(.001f));
            var first=bullet;pool.Return(first);var acquired=new System.Collections.Generic.List<Bullet>();
            for(int i=0;i<256;i++){bullet=pool.Get();acquired.Add(bullet);if(bullet==first)break;}
            Assert.That(bullet,Is.SameAs(first));tracer=Tracer(bullet);line=tracer.GetComponent<LineRenderer>();
            bullet.Initialize(new ProjectileData(null,10));tracer.Advance(bullet.transform.position+Vector3.forward*100);
            Assert.That(Vector3.Distance(line.GetPosition(0),line.GetPosition(1)),Is.EqualTo(4).Within(.001f));
            var gun=Object.FindObjectsByType<Battlefield.Features.Weapon.MachineGun>().Single();
            Assert.That(new SerializedObject(gun).FindProperty("_tracerLengthMultiplier").floatValue,Is.EqualTo(3));
            foreach(var item in acquired)pool.Return(item);yield return null;
        }
    }
}
#endif
