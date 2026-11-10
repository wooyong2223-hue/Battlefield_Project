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
namespace Battlefield.Tests.PlayMode
{
 public class ReloadBodyFrameTests
 {
  sealed class Idle:IWeaponInput{public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
  static void Call(object target,string method)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);
  [UnityTest]public IEnumerator ReloadHandsFollowAnAnimatedTorsoTranslationInsteadOfStayingAtTheCamera()
  {
   yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
   var a=c.Actor.GetComponentInChildren<Animator>();var p=a.GetComponent<PlayerCharacterCombatView>();var rig=c.Actor.GetComponentInChildren<FirstPersonArmsView>();var reload=a.GetComponent<PlayerCharacterRifleReloadView>();var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();
   float worst=0;string label="";
   foreach(string weapon in new[]{"AK47","M4A1","Pistol"})foreach(bool fp in new[]{false,true})
   {
    bool rifle=weapon!="Pistol";
    if(rifle)RealRifleModelTests.Select(loadout,AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+weapon+".asset"));else loadout.TryEquip(2);if(c.View.IsFirstPersonView)c.TogglePerspective();yield return new WaitForSeconds(.3f);
    c.Shooter.Magazine.TryConsumeRound();Assert.That(c.Shooter.Magazine.TryReload(),Is.True);yield return new WaitForSeconds(.55f);a.speed=0;if(fp){c.TogglePerspective();yield return null;}
    var torso=a.GetBoneTransform(HumanBodyBones.Chest);var hand=a.GetBoneTransform(HumanBodyBones.RightHand);if(fp)Call(rig,"CopyPose");var shown=fp?rig.RightHand:hand;
    Vector3 before=shown.position;Vector3 old=torso.position;Vector3 shift=c.Actor.right*.12f;torso.position+=shift;
    Call(reload,"LateUpdate");if(fp)Call(rig,"CopyPose");
    float expected=fp&&rifle?p.ActiveDefinition.FirstPersonScale:1f;
    float error=Mathf.Abs(Vector3.Distance(before,shown.position)-shift.magnitude*expected);
    Debug.Log($"RELOAD_BODY rifle={rifle} FP={fp} translation={Vector3.Distance(before,shown.position):F4} expected={shift.magnitude*expected:F4} error={error:F4}");
    if(error>worst){worst=error;label=$"rifle={rifle} FP={fp}";}
    torso.position=old;Call(reload,"LateUpdate");if(fp)Call(rig,"CopyPose");
    var shownTorso=fp?p.FirstPersonArms.bones.Single(b=>b.name==torso.name):torso;
    Quaternion mapping=shownTorso.rotation*Quaternion.Inverse(torso.rotation);
    Vector3 pivot=shownTorso.position;before=shown.position;
    Quaternion original=torso.rotation;Quaternion turn=Quaternion.AngleAxis(12,c.Actor.forward);
    Vector3 expectedPosition=pivot+(mapping*turn*Quaternion.Inverse(mapping))*(before-pivot);
    torso.rotation=turn*original;Call(reload,"LateUpdate");if(fp)Call(rig,"CopyPose");
    error=Vector3.Distance(expectedPosition,shown.position);
    Debug.Log($"RELOAD_ROTATION {weapon} FP={fp} error={error:F4}");
    if(error>worst){worst=error;label=$"rotation {weapon} FP={fp}";}
    torso.rotation=original;a.speed=1;yield return new WaitForSeconds(1.3f);
   }
   Assert.That(worst,Is.LessThan(.015f),label+" hand must follow torso translation and rotation");
  }
 }
}
#endif
