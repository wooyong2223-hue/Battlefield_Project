#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
    public class RifleReloadContactTests
    {
        private sealed class Idle:IWeaponInput{public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
        [UnityTest]
        public IEnumerator BothRiflesKeepTheLeftHandOnTheRemovedMagazineAndRestoreItAfterReload()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
            var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();var presentation=c.Actor.GetComponentInChildren<PlayerCharacterCombatView>();var a=c.Actor.GetComponentInChildren<Animator>();
            foreach(string name in new[]{"AK47","M4A1"})foreach(bool fp in new[]{false,true})
            {
                RealRifleModelTests.Select(loadout,AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+name+".asset"));
                if(c.View.IsFirstPersonView!=fp)c.TogglePerspective();yield return new WaitForSeconds(.3f);
                var model=presentation.ActiveWeapon;Assert.That(model.Magazine,Is.Not.Null);Assert.That(model.ReloadGrip,Is.Not.Null);
                Vector3 home=model.Magazine.localPosition;Quaternion rotation=model.Magazine.localRotation;
                c.Shooter.Magazine.TryConsumeRound();Assert.That(c.Shooter.Magazine.TryReload(),Is.True);
                yield return new WaitForSeconds(.6f);
                var source=a.GetBoneTransform(HumanBodyBones.LeftHand);var left=fp?presentation.FirstPersonArms.bones.Single(t=>t.name==source.name):source;
                Assert.That(Vector3.Distance(left.position,model.ReloadGrip.position),Is.LessThan(.06f),name+" magazine contact FP="+fp);
                Assert.That(Vector3.Distance(home,model.Magazine.localPosition),Is.GreaterThan(.06f),name+" removable magazine must move");
                Assert.That(a.GetFloat("ReloadSpeed"),Is.EqualTo(1.666667f/1.6f).Within(.001f));
                Assert.That(c.Shooter.Magazine.CanFire,Is.False);
                yield return new WaitForSeconds(1.2f);
                Assert.That(c.Shooter.Magazine.Rounds,Is.EqualTo(30));Assert.That(Vector3.Distance(home,model.Magazine.localPosition),Is.LessThan(.0001f));
                Assert.That(Quaternion.Angle(rotation,model.Magazine.localRotation),Is.LessThan(.01f));Assert.That(presentation.UsesItemGrip,Is.True);
            }
        }
        [UnityTest]
        public IEnumerator SwitchingRiflesDuringReloadRestoresTheHiddenModelMagazine()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
            var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();loadout.TryEquip(1);yield return new WaitForSeconds(.3f);
            var view=c.Actor.GetComponentInChildren<PlayerCharacterCombatView>();var old=view.ActiveWeapon;Vector3 home=old.Magazine.localPosition;
            c.Shooter.Magazine.TryConsumeRound();Assert.That(c.Shooter.Magazine.TryReload(),Is.True);yield return new WaitForSeconds(.6f);
            loadout.GetEquipment(1).TryEquip(AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/M4A1.asset"));
            Assert.That(Vector3.Distance(home,old.Magazine.localPosition),Is.LessThan(.0001f));Assert.That(old.gameObject.activeSelf,Is.False);
            Assert.That(c.Shooter.Magazine.IsReloading,Is.False);
        }
    }
}
#endif
