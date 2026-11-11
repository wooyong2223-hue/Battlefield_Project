#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.Training;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using Battlefield.Framework.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    public class AssaultRifleTests
    {
        private sealed class FireInput:IWeaponInput
        { public bool FireWeapon {get;set;} public int WeaponSlotSelection=>0; }
        private static void Set(object item,string field,object value) => CharacterViewTestContext.SetField(item,field,value);
        [UnityTest]
        public IEnumerator RifleHeldFireHonorsRateAndStopsAtEmptyMagazine()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            var input=new FireInput{FireWeapon=true};Set(c.Shooter,"_input",input);c.Shooter.enabled=true;
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(PlayerCharacterLoadout.RifleSlot);
            int shots=0;float last=-100;float minimum=100;
            c.Shooter.Fired+=()=>{if(shots>0)minimum=Mathf.Min(minimum,Time.time-last);last=Time.time;shots++;};
            for(int i=0;i<29;i++)Assert.That(c.Shooter.Magazine.TryConsumeRound(),Is.True);
            yield return new WaitForSeconds(.4f);
            Assert.That(shots,Is.EqualTo(1));Assert.That(c.Shooter.Magazine.Rounds,Is.Zero);
            c.Shooter.Magazine.ResetAmmo();shots=0;
            yield return new WaitForSeconds(.65f);input.FireWeapon=false;
            Assert.That(shots,Is.InRange(4,7));Assert.That(minimum,Is.GreaterThanOrEqualTo(.0999f));
            Assert.That(c.Shooter.Magazine.Rounds,Is.EqualTo(30-shots));
        }
        [UnityTest]
        public IEnumerator RifleKeepsBothHandContactsAndFollowsVerticalAimInBothViews()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            Set(c.Shooter,"_input",new FireInput());c.Shooter.enabled=true;
            c.Actor.GetComponent<PlayerCharacterLoadout>().TryEquip(PlayerCharacterLoadout.RifleSlot);
            var a=c.Actor.GetComponentInChildren<Animator>();var p=a.GetComponent<PlayerCharacterCombatView>();
            var motor=c.Actor.GetComponent<PlayerCharacterMovement>();Set(motor,"_input",null);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(1000,.5f,1000);Physics.SyncTransforms();
            float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
            try
            {
                foreach(bool fp in new[]{false,true})
                {
                    if(c.View.IsFirstPersonView!=fp)c.TogglePerspective();
                    for(int i=0;i<100;i++)
                    {
                        c.Input.Look=new Vector2(0,i<30?.3f:i<60?-.6f:.3f);c.View.Tick(c.Input,.02f);
                        motor.Tick(Vector2.up,true,i==35,.02f);yield return null;
                        if(i<20)continue;
                        Transform left=fp?p.FirstPersonArms.bones.Single(b=>b.name==a.GetBoneTransform(HumanBodyBones.LeftHand).name):a.GetBoneTransform(HumanBodyBones.LeftHand);
                        Assert.That(Vector3.Distance(left.position,p.ActiveWeapon.SupportHandGrip.position),Is.LessThan(.05f),$"support grip FP={fp} frame={i}");
                        Assert.That(Vector3.Angle(p.ActiveWeapon.Muzzle.forward,c.View.AimRotation*Vector3.forward),Is.LessThan(50),$"aim FP={fp} frame={i}");
                        if(fp)
                        {
                            Transform source=a.GetBoneTransform(HumanBodyBones.RightHand);
                            Transform copied=p.FirstPersonArms.bones.Single(b=>b.name==source.name);
                            Quaternion presentation=copied.rotation*Quaternion.Inverse(source.rotation);
                            Vector3 viewForward=c.View.AimRotation*Vector3.forward;
                            Vector3 upright=Vector3.ProjectOnPlane(presentation*c.Actor.up,viewForward);
                            Assert.That(Vector3.Angle(upright,c.View.AimRotation*Vector3.up),Is.LessThan(35),
                                "Camera-plane arm roll must stay upright; pitch follows the authored vertical aim.");
                        }
                    }
                }
            }
            finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
        }
        [Test]
        public void MagazineStateCancelsReloadAndPreservesIndependentAmmunition()
        {
            float clock=0;
            var rifle=new WeaponMagazineState(30,180,1.6f,()=>clock);
            var pistol=new WeaponMagazineState(15,90,1.666667f,()=>clock);
            for(int i=0;i<8;i++)Assert.That(rifle.TryConsumeRound(),Is.True);
            Assert.That(rifle.TryReload(),Is.True); Assert.That(rifle.TryConsumeRound(),Is.False);
            rifle.CancelReload();clock=2;rifle.Tick();Assert.That(rifle.Rounds,Is.EqualTo(22));
            int complete=0;rifle.ReloadCompleted+=()=>complete++;
            Assert.That(rifle.TryReload(),Is.True);clock=3.59f;rifle.Tick();Assert.That(complete,Is.Zero);
            clock=3.61f;rifle.Tick();Assert.That(rifle.Rounds,Is.EqualTo(30));Assert.That(rifle.Reserve,Is.EqualTo(172));
            rifle.Tick();Assert.That(complete,Is.EqualTo(1));Assert.That(pistol.Rounds,Is.EqualTo(15));Assert.That(pistol.Reserve,Is.EqualTo(90));
        }
        [UnityTest]
        public IEnumerator RifleDefinitionDamagePassesAlliedDummyRangeBeforeEquipmentIntegration()
        {
            yield return CharacterViewTestContext.Load();
            var definition=AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/AssaultRifle.asset");
            Assert.That(definition,Is.Not.Null);Assert.That(definition.FireRate,Is.EqualTo(10));
            var shooter=Object.FindObjectsByType<DummyShooter>(FindObjectsInactive.Include).First(s=>s.GetComponent<Team>().CurrentTeam==TeamType.Player);
            shooter.GetComponent<PlayerCharacterCombatController>().enabled=false;
            shooter.GetComponent<PlayerCharacterMovement>().enabled=false;
            var body=shooter.GetComponent<CharacterController>();
            var root=GameObject.Find("InfantryTrainingGround").transform;
            Set(shooter,"_damage",definition.Damage);
            foreach(var target in root.GetComponentsInChildren<Health>())
            {
                body.enabled=false;shooter.transform.SetPositionAndRotation(new Vector3(target.transform.position.x,root.position.y+2,root.position.z-40),Quaternion.identity);body.enabled=true;
                Set(shooter,"_target",target.transform);Physics.SyncTransforms();float before=target.CurrentHealth;
                typeof(DummyShooter).GetMethod("Fire",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(shooter,null);
                yield return new WaitForSeconds(.3f);
                float expected=target.GetComponent<Team>().CurrentTeam==TeamType.Player?before:before-definition.Damage;
                Assert.That(target.CurrentHealth,Is.EqualTo(expected),target.name);
            }
        }
        [UnityTest]
        public IEnumerator AuthoredLoadoutSwitchesModelsAndRetainsAmmoAcrossReloadCancellation()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();Assert.That(loadout,Is.Not.Null);
            c.Shooter.enabled=true;yield return null;
            var view=c.Actor.GetComponentInChildren<PlayerCharacterCombatView>();
            var pistol=c.Shooter.Magazine;Assert.That(pistol.Rounds,Is.EqualTo(15));
            Assert.That(loadout.TryEquip(PlayerCharacterLoadout.RifleSlot),Is.True);yield return null;
            Assert.That(c.Shooter.WeaponName,Is.EqualTo("AK-47"));Assert.That(c.Shooter.ShotInterval,Is.EqualTo(.1f).Within(.0001f));
            Assert.That(view.ActiveWeapon,Is.Not.SameAs(view.Pistol));Assert.That(view.ActiveWeapon.SupportHandGrip,Is.Not.Null);
            Assert.That(c.Shooter.Muzzle,Is.SameAs(view.ActiveWeapon.Muzzle));
            var rifle=c.Shooter.Magazine;Assert.That(rifle.Rounds,Is.EqualTo(30));
            Assert.That(rifle.TryConsumeRound(),Is.True);Assert.That(rifle.TryReload(),Is.True);
            Assert.That(loadout.TryEquip(PlayerCharacterLoadout.PistolSlot),Is.True);yield return null;Assert.That(rifle.IsReloading,Is.False);
            Assert.That(c.Shooter.Magazine,Is.SameAs(pistol));Assert.That(view.ActiveWeapon,Is.SameAs(view.Pistol));
            Assert.That(loadout.TryEquip(PlayerCharacterLoadout.RifleSlot),Is.True);Assert.That(rifle.Rounds,Is.EqualTo(29));
            Assert.That(rifle.TryReload(),Is.True);yield return new WaitForSeconds(1.7f);
            Assert.That(rifle.Rounds,Is.EqualTo(30));Assert.That(rifle.Reserve,Is.EqualTo(179));
            Assert.That(loadout.TryEquip(PlayerCharacterLoadout.UnarmedSlot),Is.True);yield return null;Assert.That(c.Shooter.IsArmed,Is.False);Assert.That(view.ActiveWeapon,Is.Null);
            Assert.That(loadout.TryEquip(99),Is.False);Assert.That(c.Shooter.WeaponSlot,Is.EqualTo(PlayerCharacterLoadout.UnarmedSlot));
            int notifications=0;c.Shooter.EquipmentChanged+=_=>notifications++;
            c.Shooter.ResetForRespawn();Assert.That(notifications,Is.EqualTo(1));Assert.That(loadout.CurrentSlot,Is.EqualTo(1));
            loadout.TryEquip(PlayerCharacterLoadout.RifleSlot);Assert.That(rifle.Reserve,Is.EqualTo(180));
        }
    }
}
#endif
