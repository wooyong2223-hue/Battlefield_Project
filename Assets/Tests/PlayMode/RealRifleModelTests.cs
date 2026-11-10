#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    public class RealRifleModelTests
    {
        internal static void Select(PlayerCharacterLoadout loadout,InfantryWeaponDefinition definition)
        {
            loadout.TryEquip(PlayerCharacterLoadout.UnarmedSlot);
            loadout.GetEquipment(PlayerCharacterLoadout.RifleSlot).TryEquip(definition);
            Assert.That(loadout.TryEquip(PlayerCharacterLoadout.RifleSlot),Is.True);
        }
        private sealed class Idle:IWeaponInput{public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
        [Test]
        public void BothRiflesHavePbrMapsSeparatedMagazinesAndForwardMuzzles()
        {
            foreach(string name in new[]{"M4A1","AK47"})
            {
                var definition=AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+name+".asset");
                Assert.That(definition,Is.Not.Null);var model=definition.ModelPrefab;Assert.That(model,Is.Not.Null);
                Assert.That(model.SupportHandGrip,Is.Not.Null);Assert.That(model.Muzzle,Is.Not.Null);
                Assert.That(Vector3.Dot(model.Muzzle.forward,Vector3.forward),Is.GreaterThan(.999f));
                Assert.That(model.GetComponentsInChildren<Transform>().Count(t=>t.name=="Magazine"),Is.EqualTo(1));
                Assert.That(model.GetComponentsInChildren<Collider>(),Is.Empty);
                var materials=model.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Distinct().ToArray();
                Assert.That(materials.Length,Is.EqualTo(1));var m=materials[0];Assert.That(m.shader.name,Is.EqualTo("Universal Render Pipeline/Lit"));
                foreach(string property in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap"})Assert.That(m.GetTexture(property),Is.Not.Null,name+property);
                Assert.That(m.IsKeywordEnabled("_NORMALMAP"),Is.True);Assert.That(m.IsKeywordEnabled("_METALLICSPECGLOSSMAP"),Is.True);
                Assert.That(((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m.GetTexture("_BumpMap")))).textureType,Is.EqualTo(TextureImporterType.NormalMap));
                Assert.That(((TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m.GetTexture("_MetallicGlossMap")))).sRGBTexture,Is.False);
            }
            var active=AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/AssaultRifle.asset");
            Assert.That(active.ModelPrefab,Is.SameAs(AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/M4A1.asset").ModelPrefab));
            Assert.That(active.Capacity,Is.EqualTo(30));Assert.That(active.Reserve,Is.EqualTo(180));Assert.That(active.FireRate,Is.EqualTo(10));Assert.That(active.ReloadDuration,Is.EqualTo(1.6f));
        }
        [UnityTest]
        public IEnumerator BothModelsRetainHandContactsAimAndAuthoredReloadInBothViews()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();
            CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
            var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();var a=c.Actor.GetComponentInChildren<Animator>();var p=a.GetComponent<PlayerCharacterCombatView>();
            foreach(string name in new[]{"M4A1","AK47"})
            {
                Select(loadout,AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+name+".asset"));
                foreach(bool fp in new[]{false,true})
                {
                    if(c.View.IsFirstPersonView!=fp)c.TogglePerspective();yield return new WaitForSeconds(.3f);
                    for(int i=0;i<30;i++)
                    {
                        c.Input.Look=new Vector2(0,i<15?.3f:-.3f);c.View.Tick(c.Input,.02f);yield return null;
                        var leftSource=a.GetBoneTransform(HumanBodyBones.LeftHand);
                        var left=fp?p.FirstPersonArms.bones.Single(b=>b.name==leftSource.name):leftSource;
                        Assert.That(Vector3.Distance(left.position,p.ActiveWeapon.SupportHandGrip.position),Is.LessThan(.05f),name+" support FP="+fp);
                        Assert.That(Vector3.Angle(p.ActiveWeapon.Muzzle.forward,c.View.AimRotation*Vector3.forward),Is.LessThan(50),name+" aim");
                    }
                    Assert.That(c.Shooter.Magazine.TryConsumeRound(),Is.True);Assert.That(c.Shooter.Magazine.TryReload(),Is.True);
                    yield return new WaitForSeconds(.2f);Assert.That(p.UsesItemGrip,Is.False);
                    yield return new WaitForSeconds(1.6f);Assert.That(c.Shooter.Magazine.Rounds,Is.EqualTo(30));Assert.That(p.UsesItemGrip,Is.True);
                }
            }
        }
    }
}
#endif
