#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode
{
 public class JumpReloadLoweringTests
 {
  sealed class Idle:IWeaponInput {public bool FireWeapon=>false;public int WeaponSlotSelection=>0;}
  static void Call(object target,string method)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,null);
  [UnityTest]public IEnumerator JumpReloadReturnsToTheGroundedBodyGripWithoutAnInitialTeleport()
  {
   yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();CharacterViewTestContext.SetField(c.Shooter,"_input",new Idle());c.Shooter.enabled=true;
   var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(100,.5f,100);Physics.SyncTransforms();
   var motor=c.Actor.GetComponent<PlayerCharacterMovement>();CharacterViewTestContext.SetField(motor,"_input",null);motor.enabled=true;
   var animator=c.Actor.GetComponentInChildren<Animator>();var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);var torso=animator.GetBoneTransform(HumanBodyBones.UpperChest);
   var reload=c.Actor.GetComponentInChildren<PlayerCharacterRifleReloadView>();var rig=c.Actor.GetComponentInChildren<FirstPersonArmsView>();var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();
   float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;float worst=0;bool allRaised=true;float worstJump=0;float minimumViewDrop=float.PositiveInfinity;
   try{
    foreach(string name in new[]{"AK47","M4A1"})foreach(bool fp in new[]{false,true})
    {
     RealRifleModelTests.Select(loadout,AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+name+".asset"));if(c.View.IsFirstPersonView!=fp)c.TogglePerspective();c.Input.AimHeld=true;c.View.Tick(c.Input,.02f);
     for(int i=0;i<70;i++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
     c.Shooter.Magazine.TryConsumeRound();Assert.That(c.Shooter.Magazine.TryReload(),Is.True);
     for(int i=0;i<22;i++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
     Vector3 ground=torso.InverseTransformPoint(hand.position);
     for(int i=0;i<90;i++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
     float referenceHeight=Vector3.Dot(hand.position-torso.position,c.Actor.up);bool raised=false;
     for(int i=0;i<35;i++){
      motor.Tick(Vector2.zero,false,i==0,.02f);yield return null;
      if(i>=2&&!motor.IsGrounded&&Vector3.Dot(hand.position-torso.position,c.Actor.up)>referenceHeight+.1f){raised=true;break;}
     }
     allRaised&=raised;Vector3 entry=hand.position;Vector3 raisedLocal=torso.InverseTransformPoint(hand.position);
     c.Shooter.Magazine.TryConsumeRound();Assert.That(c.Shooter.Magazine.TryReload(),Is.True);Call(reload,"LateUpdate");worstJump=Mathf.Max(worstJump,Vector3.Distance(entry,hand.position));
     if(fp)Call(rig,"CopyPose");float viewY=fp?c.Camera.transform.InverseTransformPoint(rig.RightHand.position).y:0;
     for(int i=0;i<22;i++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
     Vector3 settled=torso.InverseTransformPoint(hand.position);float error=Vector3.Distance(ground,settled);worst=Mathf.Max(worst,error);
     float shownDrop=fp?viewY-c.Camera.transform.InverseTransformPoint(rig.RightHand.position).y:0;
     if(fp)minimumViewDrop=Mathf.Min(minimumViewDrop,shownDrop);
     Debug.Log($"JUMP_RELOAD {name} FP={fp} raised={raised} entry={raisedLocal} ground={ground} settled={settled} error={error:F4} viewDrop={shownDrop:F4}");
     for(int i=0;i<100;i++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
    }
    Assert.That(allRaised,Is.True,"The real jump must reproduce the elevated-arm phase.");
    Assert.That(worstJump,Is.LessThan(.02f),"Reload must start continuously from the current hand pose.");
    Assert.That(minimumViewDrop,Is.GreaterThan(.03f),"The first-person hand must visibly lower too.");
    Assert.That(worst,Is.LessThan(.04f),"Jump entry must settle to the same torso-local grip as grounded reload.");
   }finally{Time.captureDeltaTime=old;}
  }
 }
}
#endif
