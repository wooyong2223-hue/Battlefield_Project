#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
    public class PlayerEquipmentTests
    {
        [UnityTest]
        public IEnumerator SceneStartsWithAk47AndExplicitEquipmentObjects()
        {
            yield return CharacterViewTestContext.Load();
            var combat=Object.FindAnyObjectByType<PlayerCharacterController>();var loadout=combat.GetComponent<PlayerCharacterLoadout>();
            Assert.That(loadout.CurrentSlot,Is.EqualTo(1));Assert.That(combat.WeaponName,Is.EqualTo("AK-47"));
            var rifle=combat.transform.Find("Equipment/Rifle").GetComponent<PlayerCharacterWeaponEquipment>();
            var pistol=combat.transform.Find("Equipment/Pistol").GetComponent<PlayerCharacterWeaponEquipment>();
            Assert.That(loadout.GetEquipment(1),Is.SameAs(rifle));Assert.That(loadout.GetEquipment(2),Is.SameAs(pistol));
            Assert.That(rifle.Definition,Is.SameAs(AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/AK47.asset")));
            Assert.That(pistol.Magazine.Capacity,Is.EqualTo(15));Assert.That(pistol.Magazine.Reserve,Is.EqualTo(90));
            Assert.That(pistol.Magazine.ReloadDuration,Is.EqualTo(1.666667f).Within(.000001f));
            loadout.TryEquip(2);Assert.That(combat.WeaponName,Is.EqualTo("PISTOL"));
            loadout.TryEquip(3);Assert.That(combat.IsArmed,Is.False);Assert.That(combat.Magazine,Is.Null);
        }
        [UnityTest]
        public IEnumerator RifleVariantsRetainIndependentAmmoAndRefreshTheSelectedPresentation()
        {
            yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();c.Shooter.enabled=true;
            var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();loadout.TryEquip(1);
            var equipment=loadout.GetEquipment(1);var ak=equipment.Definition;
            var m4=AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/M4A1.asset");
            var view=c.Actor.GetComponentInChildren<PlayerCharacterCombatView>();var akAmmo=equipment.Magazine;
            akAmmo.TryConsumeRound();akAmmo.TryConsumeRound();Assert.That(akAmmo.TryReload(),Is.True);
            int changes=0;c.Shooter.EquipmentChanged+=_=>changes++;
            Assert.That(equipment.TryEquip(m4),Is.True);yield return null;
            Assert.That(changes,Is.EqualTo(1));Assert.That(akAmmo.IsReloading,Is.False);
            Assert.That(view.ActiveDefinition,Is.SameAs(m4));Assert.That(c.Shooter.Muzzle,Is.SameAs(view.ActiveWeapon.Muzzle));
            var m4Ammo=equipment.Magazine;Assert.That(m4Ammo,Is.Not.SameAs(akAmmo));m4Ammo.TryConsumeRound();
            Assert.That(equipment.TryEquip(ak),Is.True);Assert.That(equipment.Magazine.Rounds,Is.EqualTo(28));
            var serialized=new SerializedObject(equipment);serialized.FindProperty("_definition").objectReferenceValue=m4;serialized.ApplyModifiedPropertiesWithoutUndo();yield return null;
            Assert.That(equipment.Definition,Is.SameAs(m4));Assert.That(equipment.Magazine.Rounds,Is.EqualTo(29));Assert.That(view.ActiveDefinition,Is.SameAs(m4));
            c.Shooter.enabled=false;loadout.TryEquip(2);c.Shooter.enabled=true;yield return null;
            Assert.That(view.ActiveWeapon,Is.SameAs(view.Pistol),"Re-enabling combat must refresh equipment selected while inactive.");
            c.Shooter.ResetForRespawn();Assert.That(loadout.CurrentSlot,Is.EqualTo(1));Assert.That(m4Ammo.Rounds,Is.EqualTo(30));Assert.That(akAmmo.Rounds,Is.EqualTo(30));
        }
    }
}
#endif
