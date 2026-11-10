#if UNITY_EDITOR
using System.Collections;
using System.Reflection;
using Battlefield.Features.Dummy;
using Battlefield.Features.PlayerCharacter;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Battlefield.Tests.PlayMode
{
 public class PlayerCharacterNativeAimInputTests : InputTestFixture
 {
  [SetUp] public void ManualInput()=>InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
  [TearDown] public void StopInputBeforeReset()
  {
   foreach(var input in Object.FindObjectsByType<PlayerCharacterInput>()) input.enabled=false;
   foreach(var input in Object.FindObjectsByType<Battlefield.Features.Fighter.KeyboardJetInput>()) input.enabled=false;
  }
  static void CaptureInput(PlayerCharacterInput input)
  {
   typeof(PlayerCharacterInput).GetField("_captured",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(input,true);
   typeof(PlayerCharacterInput).GetField("_captureFrame",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(input,-1);
  }
  static IEnumerator MouseDelta(Mouse mouse,Vector2 delta)
  {
   InputSystem.QueueStateEvent(mouse,new MouseState{delta=delta});InputSystem.Update();yield return null;
   InputSystem.QueueStateEvent(mouse,new MouseState());InputSystem.Update();yield return null;
  }
  [UnityTest]
  public IEnumerator NativeMouseMovesTorsoAndHeadWithTheLiveAnimatorInBothPerspectives()
  {
   foreach(var path in new[]{"Assets/Scenes/SampleScene.unity"})
   {
    yield return EditorSceneManager.LoadSceneAsyncInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
    foreach(var s in Object.FindObjectsByType<DummyShooter>())s.enabled=false;
    foreach(var s in Object.FindObjectsByType<DummyMissileShooter>())s.enabled=false;
    var input=Object.FindAnyObjectByType<PlayerCharacterInput>();
    var view=input.GetComponent<PlayerCharacterView>();var motor=input.GetComponent<PlayerCharacterMovement>();
    var combat=input.GetComponent<PlayerCharacterController>();var aim=input.GetComponentInChildren<PlayerCharacterWeaponAimView>();
    var animator=input.GetComponentInChildren<Animator>();
    var chest=animator.GetBoneTransform(HumanBodyBones.UpperChest);var head=animator.GetBoneTransform(HumanBodyBones.Head);
    var mouse=InputSystem.AddDevice<Mouse>();var keyboard=InputSystem.AddDevice<Keyboard>();
    yield return new WaitForSeconds(.8f);CaptureInput(input);
    for(int perspective=0;perspective<2;perspective++)
    {
     float current=Mathf.DeltaAngle(0f,view.AimRotation.eulerAngles.x);
     yield return MouseDelta(mouse,new Vector2(0f,current/.3f));yield return new WaitForSeconds(.25f);
     Quaternion neutralChest=chest.rotation,neutralHead=head.rotation;
     var skin=animator.GetComponentInChildren<SkinnedMeshRenderer>();var baked=new Mesh();skin.BakeMesh(baked);
     Vector3[] neutralVertices=baked.vertices;var weights=skin.sharedMesh.boneWeights;var bones=skin.bones;
     var spine=animator.GetBoneTransform(HumanBodyBones.Spine);var chestBone=animator.GetBoneTransform(HumanBodyBones.Chest);
     bool IsTorso(int index)=>bones[index]==spine||bones[index]==chestBone||bones[index]==chest;

     yield return MouseDelta(mouse,new Vector2(0f,200f));yield return new WaitForSeconds(.35f);
     skin.BakeMesh(baked);Vector3[] aimedVertices=baked.vertices;float torsoDelta=0f;int torsoCount=0;
     for(int i=0;i<weights.Length;i++)
     {
      var w=weights[i];float weight=(IsTorso(w.boneIndex0)?w.weight0:0f)+(IsTorso(w.boneIndex1)?w.weight1:0f)+(IsTorso(w.boneIndex2)?w.weight2:0f)+(IsTorso(w.boneIndex3)?w.weight3:0f);
      if(weight<.5f)continue;torsoCount++;torsoDelta=Mathf.Max(torsoDelta,Vector3.Distance(neutralVertices[i],aimedVertices[i]));
     }
     Object.Destroy(baked);Debug.Log($"NATIVE_SKIN torsoVertices={torsoCount} maximumTorsoVertexDisplacement={torsoDelta}");
     Assert.That(torsoCount,Is.GreaterThan(0));Assert.That(torsoDelta,Is.GreaterThan(.03f),"The skinned torso surface must move, not only the Transform bones.");
     float chestAngle=Quaternion.Angle(neutralChest,chest.rotation),headAngle=Quaternion.Angle(neutralHead,head.rotation);
     Debug.Log($"NATIVE_AIM scene={path} first={view.IsFirstPersonView} pitch={Mathf.DeltaAngle(0f,view.AimRotation.eulerAngles.x)} chest={chestAngle} head={headAngle} enabled={aim.enabled} armed={combat.IsArmed} usable={combat.CanUseCombat} ground={motor.IsGrounded} base={animator.GetCurrentAnimatorStateInfo(0).fullPathHash} upperWeight={animator.GetLayerWeight(animator.GetLayerIndex("UpperCombat"))}");
     Assert.That(Mathf.DeltaAngle(0f,view.AimRotation.eulerAngles.x),Is.EqualTo(-60f).Within(.1f));
     Assert.That(chestAngle,Is.GreaterThan(8f),"Native mouse input must move the torso: "+path+" first="+view.IsFirstPersonView);
     Assert.That(headAngle,Is.GreaterThan(15f));
     InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.V));InputSystem.Update();yield return null;
     InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();yield return null;
    }
    InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keyboard);
   }
  }
 }
}
#endif
