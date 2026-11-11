#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Training;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Projectile;
using Battlefield.Features.Weapon;
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
    public class BulletVisualTests
    {
        const string Folder="Assets/Features/Projectile/Models/Temporary/";
        static Mesh Infantry=>AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"InfantryBullet.asset");
        static Mesh Cannon=>AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"CannonBullet.asset");
        static void Set(object target,string name,object value)
        {
            for(var type=target.GetType();type!=null;type=type.BaseType)
            {
                var field=type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);
                if(field==null)continue;field.SetValue(target,value);return;
            }
            Assert.Fail("Missing field "+name);
        }
        static IEnumerator Load()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/SampleScene.unity",new LoadSceneParameters(LoadSceneMode.Single));
            foreach(var s in Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include))s.enabled=false;
            foreach(var s in Object.FindObjectsByType<DummyMissileShooter>())s.enabled=false;
        }
        [UnityTest] public IEnumerator PoolReuseRestoresInfantryMeshAndKeepsProjectileSettings()
        {
            yield return Load();
            Assert.That(Infantry.bounds.size.x,Is.EqualTo(.02f).Within(.00001f));
            Assert.That(Infantry.bounds.size.z,Is.EqualTo(.08f).Within(.00001f));
            Assert.That(Cannon.bounds.size.x,Is.EqualTo(.04f).Within(.00001f));
            Assert.That(Cannon.bounds.size.z,Is.EqualTo(.16f).Within(.00001f));
            var pool=Object.FindAnyObjectByType<BulletPool>();var first=pool.Get();
            first.GetComponent<BulletVisual>().SetMesh(Cannon);pool.Return(first);
            var acquired=new List<Bullet>();
            try{
                for(int i=0;i<256;i++){var b=pool.Get();acquired.Add(b);if(b==first)break;}
                Assert.That(acquired.Last(),Is.SameAs(first));
                Assert.That(first.GetComponent<MeshFilter>().sharedMesh,Is.SameAs(Infantry));
                Assert.That(first.transform.localScale,Is.EqualTo(Vector3.one));
                Assert.That(first.Speed,Is.EqualTo(1000));Assert.That(first.LifeTime,Is.EqualTo(1));Assert.That(first.GravityScale,Is.Zero);
            }finally{foreach(var bullet in acquired)pool.Return(bullet);}
        }
        [UnityTest] public IEnumerator BothVisualsPreserveAlliedDummyOwnerDamageRules()
        {
            yield return Load();
            var shooter=Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include).First(s=>s.GetComponent<Team>().CurrentTeam==TeamType.Player);
            shooter.GetComponent<PlayerCharacterCombatController>().enabled=false;shooter.GetComponent<PlayerCharacterMovement>().enabled=false;
            var body=shooter.GetComponent<CharacterController>();var pool=Object.FindAnyObjectByType<BulletPool>();
            var gun=shooter.gameObject.AddComponent<MachineGun>();Set(gun,"_overheat",new Overheat());Set(gun,"_fireRate",10f);Set(gun,"_damage",10f);Set(gun,"_bulletPool",pool);
            var point=new GameObject("Temporary test muzzle").transform;point.SetParent(shooter.transform,false);point.localPosition=Vector3.forward*2;Set(gun,"_firePoint",point);
            string[] names={"Dummy_Enemy","Dummy_Neutral","Ally_10m"};
            foreach(var mesh in new[]{Infantry,Cannon})for(int i=0;i<names.Length;i++)
            {
                var target=Object.FindObjectsByType<Health>().Single(h=>h.name==names[i]);
                body.enabled=false;shooter.transform.SetPositionAndRotation(new Vector3(10000+i*100,10000,10000),Quaternion.identity);body.enabled=true;
                target.transform.SetParent(null,true);target.transform.position=shooter.transform.position+Vector3.forward*20;Physics.SyncTransforms();
                float before=target.CurrentHealth;Set(gun,"_projectileMesh",mesh);gun.TryFire();
                Assert.That(pool.GetComponentsInChildren<Bullet>().Any(b=>b.GetComponent<MeshFilter>().sharedMesh==mesh),Is.True);
                yield return new WaitForSeconds(.15f);
                Assert.That(target.CurrentHealth,Is.EqualTo(before-(i==2?0:10)),names[i]);
            }
        }
        [UnityTest] public IEnumerator AuthoredFighterSelectsCannonMeshFromSharedPool()
        {
            yield return Load();
            var gun=Object.FindObjectsByType<MachineGun>().Single();
            var data=new SerializedObject(gun);
            Assert.That(data.FindProperty("_projectileMesh").objectReferenceValue,Is.SameAs(Cannon));
            Assert.That(gun.ProjectilePrefab,Is.SameAs(Object.FindAnyObjectByType<BulletPool>().ProjectilePrefab));
        }
    }
}
#endif
