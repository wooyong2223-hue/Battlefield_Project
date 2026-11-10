#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode {
public sealed class ProneFiringStabilityTests {
sealed class FireInput:IWeaponInput {public bool FireWeapon{get;set;}public int WeaponSlotSelection=>0;}
[UnityTest] public IEnumerator RealShotsKeepGripMeshAndAimInBothViews(){
 float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
 try{foreach(string weapon in new[]{"AK47","M4A1","Pistol"})foreach(bool fp in new[]{false,true}){
 yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var input=new FireInput();CharacterViewTestContext.SetField(c.Shooter,"_input",input);c.Shooter.enabled=true;
 // Separate weapon/hand stability from the intentional camera recoil.
 c.Actor.GetComponent<PlayerCharacterRecoil>().enabled=false;
 var loadout=c.Actor.GetComponent<PlayerCharacterLoadout>();if(weapon=="Pistol")loadout.TryEquip(2);else RealRifleModelTests.Select(loadout,AssetDatabase.LoadAssetAtPath<InfantryWeaponDefinition>("Assets/Features/Weapon/Definitions/"+weapon+".asset"));
 if(fp)c.TogglePerspective();var a=c.Actor.GetComponentInChildren<Animator>();var presentation=a.GetComponent<PlayerCharacterCombatView>();
 var prone=a.GetComponent<PlayerCharacterProneAnimationView>();if(prone==null){Assert.Ignore("Optional local prone originals are absent.");yield break;}
 var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(100,.5f,100);Physics.SyncTransforms();
 try{
 var motor=c.Actor.GetComponent<PlayerCharacterMovement>();CharacterViewTestContext.SetField(motor,"_input",null);motor.enabled=true;
 Assert.That(c.Actor.GetComponent<PlayerCharacterStance>().TrySetStance(CharacterStance.Prone),Is.True);
 for(int n=0;n<70;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
 Transform hand=a.GetBoneTransform(HumanBodyBones.RightHand);if(fp)hand=presentation.FirstPersonArms.bones.Single(b=>b.name==hand.name);
 int shots=0;c.Shooter.Fired+=()=>shots++;
 Vector3 start=c.Camera.transform.InverseTransformPoint(hand.position),previous=start;float rise=0,step=0,angle=0;
 var mesh=presentation.FirstPersonArms.sharedMesh;input.FireWeapon=true;
 for(int n=0;n<80;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;
 Vector3 current=c.Camera.transform.InverseTransformPoint(hand.position);rise=Mathf.Max(rise,current.y-start.y);step=Mathf.Max(step,Vector3.Distance(current,previous));previous=current;
 angle=Mathf.Max(angle,Vector3.Angle(c.Shooter.Muzzle.forward,c.View.AimRotation*Vector3.forward));
 if(fp)Assert.That(presentation.FirstPersonArms.sharedMesh,Is.SameAs(mesh),"A shot must not replace the gripping arm mesh.");
 }
 input.FireWeapon=false;Assert.That(shots,Is.GreaterThan(5));
 Assert.That(rise,Is.LessThan(.08f),weapon+" hand rise FP="+fp);
 Assert.That(step,Is.LessThan(.1f),weapon+" hand discontinuity FP="+fp);
 Assert.That(angle,Is.LessThan(8f),weapon+" muzzle/aim mismatch FP="+fp);
 Assert.That(prone.CanUseItemGrip,Is.True);
 }finally{Object.Destroy(floor);}
 }}finally{Time.captureDeltaTime=old;}
}
}}
#endif
