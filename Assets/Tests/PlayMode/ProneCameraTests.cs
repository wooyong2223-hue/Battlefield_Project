#if UNITY_EDITOR
using System.Collections;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Battlefield.Tests.PlayMode {
public sealed class ProneCameraTests {
[UnityTest]public IEnumerator ProneOrbitLookLimitsAndEyePivotStayConsistent(){
 yield return CharacterViewTestContext.Load();var c=new CharacterViewTestContext();var profile=c.View.ProneCamera;Assert.That(profile,Is.Not.Null);
 var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=c.Actor.position+Vector3.down*2.25f;floor.transform.localScale=new Vector3(100,.5f,100);Physics.SyncTransforms();
 float old=Time.captureDeltaTime;Time.captureDeltaTime=.02f;
 try{
 var stance=c.Actor.GetComponent<PlayerCharacterStance>();Assert.That(stance.TrySetStance(CharacterStance.Prone),Is.True);
 for(int n=0;n<50;n++)yield return null;
 float ground=c.Actor.GetComponent<CharacterController>().bounds.min.y;
 Assert.That(c.View.ProneBlend,Is.EqualTo(1f));
 Assert.That(c.Follow.DistanceFromCharacter,Is.EqualTo(profile.ThirdPersonOffset.magnitude).Within(.03f));
 c.Input.Look=new Vector2(0f,100f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;yield return null;yield return null;
 Assert.That(Mathf.DeltaAngle(0f,c.View.AimRotation.eulerAngles.x),Is.EqualTo(-profile.AimPitchUp).Within(.01f));
 Assert.That(c.Camera.transform.position.y-ground,Is.GreaterThan(.25f));
 Assert.That(c.Follow.DistanceFromCharacter,Is.GreaterThan(3f),"Looking up must not collapse the orbit into the prone body.");
 Assert.That(Vector3.Distance(c.Follow.GetAimRay().origin,c.Camera.transform.position),Is.LessThan(.02f));
 // A wall must still contract the authored prone orbit and restore it when removed.
 var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);
 c.Input.Look=new Vector2(0f,-100f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;
 c.Input.Look=new Vector2(0f,profile.AimPitchDown/3f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;
 Vector3 pivot=c.Actor.TransformPoint(stance.StandingCenter-Vector3.up*stance.StandingHeight*.5f+profile.PivotFromFeet);
 wall.transform.position=pivot+Vector3.back*1.5f;wall.transform.localScale=new Vector3(10f,10f,.2f);Physics.SyncTransforms();yield return null;yield return null;
 Assert.That(c.Follow.DistanceFromCharacter,Is.LessThan(2f));Object.Destroy(wall);
 for(int n=0;n<40;n++)yield return null;
 Assert.That(c.Follow.DistanceFromCharacter,Is.EqualTo(profile.ThirdPersonOffset.magnitude).Within(.04f));
 c.Input.AimHeld=true;c.View.Tick(c.Input,.02f);for(int n=0;n<40;n++)yield return null;
 Assert.That(c.Follow.DistanceFromCharacter,Is.EqualTo(profile.AimOffset.magnitude).Within(.04f));
 c.Input.AimHeld=false;c.View.Tick(c.Input,.02f);c.TogglePerspective();yield return null;yield return null;
 Vector3 eye=c.Camera.transform.position;
 Assert.That(eye.y-ground,Is.EqualTo(.7f).Within(.1f));
 c.Input.Look=new Vector2(0f,-100f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;yield return null;yield return null;
 Assert.That(Mathf.DeltaAngle(0f,c.View.AimRotation.eulerAngles.x),Is.EqualTo(profile.AimPitchDown).Within(.01f));
 Assert.That(Vector3.Distance(eye,c.Camera.transform.position),Is.LessThan(.02f),"First person pitch rotates at the eyes instead of orbiting behind the body.");
 Quaternion root=c.Actor.rotation;c.Input.Look=new Vector2(5f,0f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;yield return null;
 Assert.That(Quaternion.Angle(root,c.Actor.rotation),Is.LessThan(.01f));
 Assert.That(Vector3.Distance(eye,c.Camera.transform.position),Is.LessThan(.02f),"Small prone yaw must turn at the eye position.");
 c.Input.Look=new Vector2(100f,0f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;yield return null;
 Assert.That(Quaternion.Angle(root,c.Actor.rotation),Is.GreaterThan(1f));
 Vector3 aimFlat=Vector3.ProjectOnPlane(c.View.AimRotation*Vector3.forward,Vector3.up);
 Assert.That(Mathf.Abs(Vector3.SignedAngle(c.Actor.forward,aimFlat,Vector3.up)),Is.EqualTo(profile.AimYawLimit).Within(.02f));
 Quaternion aim=c.View.AimRotation;root=c.Actor.rotation;c.Input.FreeLookHeld=true;c.Input.Look=new Vector2(100f,-100f);c.View.Tick(c.Input,.02f);c.Input.Look=Vector2.zero;yield return null;
 Assert.That(Quaternion.Angle(aim,c.View.AimRotation),Is.LessThan(.01f));Assert.That(Quaternion.Angle(root,c.Actor.rotation),Is.LessThan(.01f));
 Assert.That(Mathf.DeltaAngle(0f,c.View.LookRotation.eulerAngles.x),Is.EqualTo(profile.FreePitchDown).Within(.02f));
 c.Input.FreeLookHeld=false;c.View.Tick(c.Input,.02f);Assert.That(Mathf.DeltaAngle(0f,c.View.LookRotation.eulerAngles.x),Is.GreaterThan(profile.AimPitchDown+1f),"Free-look return should ease through the larger look range instead of snapping to the aim clamp.");for(int n=0;n<45;n++)yield return null;
 Quaternion beforeExit=c.View.AimRotation;Assert.That(stance.TrySetStance(CharacterStance.Standing),Is.True);
 for(int n=0;n<40;n++)yield return null;
 Assert.That(c.View.ProneBlend,Is.EqualTo(0f));Assert.That(Quaternion.Angle(beforeExit,c.View.AimRotation),Is.LessThan(.02f),"Leaving prone must preserve the viewed heading.");
 c.TogglePerspective();for(int n=0;n<40;n++)yield return null;
 Assert.That(c.Follow.DistanceFromCharacter,Is.EqualTo(new Vector3(.8f,.25f,-5f).magnitude).Within(.05f));
 }finally{Time.captureDeltaTime=old;Object.Destroy(floor);}
}
}}
#endif
