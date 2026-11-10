#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.Weapon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode {
public sealed class PronePistolAdsTests {
sealed class Input:IWeaponInput {public bool FireWeapon{get;set;}public int WeaponSlotSelection=>0;}
static Vector3 Point(CharacterViewTestContext c)=>c.Camera.WorldToViewportPoint(c.Shooter.Muzzle.position);
static void AssertCentered(CharacterViewTestContext c){Vector3 p=Point(c);Assert.That(p.z,Is.GreaterThan(c.Camera.nearClipPlane));Assert.That(p.x,Is.EqualTo(.5f).Within(.015f));Assert.That(p.y,Is.EqualTo(.5f).Within(.015f));}
[UnityTest]public IEnumerator PronePistolAdsStaysVisibleThroughAimFireReloadAndPerspectiveChanges(){
 yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var input=new Input();CharacterViewTestContext.SetField(c.Shooter,"_input",input);c.Shooter.enabled=true;
 c.Actor.GetComponent<PlayerCharacterRecoil>().enabled=false;
 var a=c.Actor.GetComponentInChildren<Animator>();var prone=a.GetComponent<PlayerCharacterProneAnimationView>();if(prone==null){Assert.Ignore("Optional local prone originals are absent.");yield break;}
 var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(100,.5f,100);Physics.SyncTransforms();
 float previous=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
 try{
 var motor=c.Actor.GetComponent<PlayerCharacterMovement>();CharacterViewTestContext.SetField(motor,"_input",null);motor.enabled=true;var stance=c.Actor.GetComponent<PlayerCharacterStance>();
 c.TapAim();for(int n=0;n<60;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}AssertCentered(c);Vector3 standing=Point(c);
 c.TapAim();Assert.That(c.View.IsFirstPerson,Is.False);Assert.That(stance.TrySetStance(CharacterStance.Prone),Is.True);
 for(int n=0;n<60;n++){motor.Tick(Vector2.zero,false,false,.02f);yield return null;}
 // Exercise entry from the third-person prone view, not only V-selected FPP.
 c.TapAim();for(int n=0;n<35;n++)yield return null;Assert.That(c.View.IsFirstPersonView,Is.True);AssertCentered(c);
 Assert.That(Vector3.Distance(Point(c),standing),Is.LessThan(.02f),"Match the existing standing pistol ADS framing.");
 foreach(var look in new[]{new Vector2(0f,5f),new Vector2(5f,-5f),new Vector2(-5f,-5f)}){
 c.Input.Look=look;c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;for(int n=0;n<20;n++)yield return null;AssertCentered(c);
 }
 int shots=0;c.Shooter.Fired+=()=>shots++;input.FireWeapon=true;
 for(int n=0;n<35;n++){yield return null;var p=Point(c);Assert.That(p.x,Is.InRange(.35f,.65f));Assert.That(p.y,Is.InRange(.35f,.7f));Assert.That(p.z,Is.GreaterThan(.5f));}
 input.FireWeapon=false;Assert.That(shots,Is.GreaterThan(3));for(int n=0;n<20;n++)yield return null;AssertCentered(c);
 Assert.That(c.Shooter.Magazine.TryReload(),Is.True);yield return null;Assert.That(prone.CanUseItemGrip,Is.False,"Do not lock the reload animation to the sight.");
 for(int n=0;n<110;n++)yield return null;Assert.That(c.Shooter.Magazine.IsReloading,Is.False);AssertCentered(c);
 c.TapAim();c.TogglePerspective();for(int n=0;n<35;n++)yield return null;var hip=Point(c);Assert.That(hip.x,Is.InRange(0f,1f));Assert.That(hip.y,Is.InRange(.05f,.95f));
 c.TapAim();for(int n=0;n<35;n++)yield return null;AssertCentered(c);
 Assert.That(stance.TrySetStance(CharacterStance.Standing),Is.True);for(int n=0;n<60;n++)yield return null;AssertCentered(c);
 }finally{Time.captureDeltaTime=previous;Object.Destroy(floor);}
}
}}
#endif
