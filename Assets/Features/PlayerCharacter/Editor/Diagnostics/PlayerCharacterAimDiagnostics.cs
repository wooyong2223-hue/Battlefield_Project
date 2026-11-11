using System;
using System.IO;
using System.Reflection;
using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace Battlefield.Editor.PlayerCharacter
{
    // Editor-only observation. Never changes a scene object, input or Animator pose.
    [InitializeOnLoad]
    public static class PlayerCharacterAimDiagnostics
    {
        private const string Folder = "Library/PlayerCharacterAimDiagnostics";
        private static double _until, _nextSample;
        private static bool _lastPlaying;
        private static readonly FieldInfo BodyBlend = typeof(PlayerCharacterWeaponAimView).GetField("_bodyBlend", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo TrackBody = typeof(PlayerCharacterWeaponAimView).GetProperty("CanTrackBody", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo BodyWeight = typeof(PlayerCharacterWeaponAimView).GetField("_bodyAimWeight", BindingFlags.Instance | BindingFlags.NonPublic);

        [Serializable]
        private sealed class Sample
        {
            public string utc, scene, actor, stance;
            public bool playing, firstPerson, freeLook, inputCaptured, inputHasControl;
            public bool aimEnabled, animatorEnabled, combatEnabled, armed, reloading, attacking, trackBody;
            public bool postureEnabled, bodyRendererEnabled, meshUsesChest;
            public float aimPitch, lookPitch, inputLookY, bodyBlend, bodyWeight, postureMultiplier, upperWeight;
            public int frame, actorId, baseState, upperState;
            public Quaternion rootRotation, chestRotation, headRotation;
        }

        static PlayerCharacterAimDiagnostics()
        {
            EditorApplication.update += Observe;
            EditorApplication.playModeStateChanged += OnPlayMode;
            _lastPlaying = EditorApplication.isPlaying;
            WriteStatus();
        }
        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode) { _until = 0; WriteStatus(); }
        }
        [MenuItem("Tools/Battlefield/Diagnostics/Record Upper Body Aim (2 minutes)")]
        public static void Begin()
        {
            _until = EditorApplication.timeSinceStartup + 120d;
            _nextSample = 0d;
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, "live.jsonl"), "");
            WriteStatus();
        }
        [MenuItem("Tools/Battlefield/Diagnostics/Stop Upper Body Aim Recording")]
        public static void Stop() { _until = 0d; WriteStatus(); }
        private static void WriteStatus()
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, "status.json"), JsonUtility.ToJson(new Sample {
                utc=DateTime.UtcNow.ToString("O"), playing=EditorApplication.isPlaying,
                scene=SceneManager.GetActiveScene().path
            }, true));
        }
        private static void Observe()
        {
            if (_lastPlaying != EditorApplication.isPlaying) { _lastPlaying = EditorApplication.isPlaying; WriteStatus(); }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup >= _until || EditorApplication.timeSinceStartup < _nextSample) return;
            _nextSample = EditorApplication.timeSinceStartup + .2d;
            foreach (var aim in Object.FindObjectsByType<PlayerCharacterWeaponAimView>())
            {
                var animator=aim.GetComponent<Animator>(); var combat=aim.GetComponentInParent<PlayerCharacterCombatController>();
                if(animator==null || !animator.isHuman || combat==null) continue;
                var view=combat.GetComponent<PlayerCharacterLookController>(); var input=combat.GetComponent<PlayerCharacterInput>();
                var stance=combat.GetComponent<PlayerCharacterStance>(); var posture=aim.GetComponent<PlayerCharacterPostureView>();
                var chest=animator.GetBoneTransform(HumanBodyBones.UpperChest); var head=animator.GetBoneTransform(HumanBodyBones.Head);
                var renderer=animator.GetComponentInChildren<SkinnedMeshRenderer>();
                bool bound=false;
                if(renderer!=null) foreach(var bone in renderer.bones) if(bone==chest) {bound=true;break;}
                var sample=new Sample {
                    utc=DateTime.UtcNow.ToString("O"), playing=true, frame=Time.frameCount, scene=aim.gameObject.scene.path,
                    actor=combat.name, actorId=combat.GetInstanceID(), stance=stance!=null?stance.Current.ToString():"None",
                    firstPerson=view.IsFirstPersonView, freeLook=view.IsFreeLooking,
                    inputCaptured=input!=null&&input.HasPointerCapture, inputHasControl=input!=null&&input.HasControl, inputLookY=input!=null?input.Look.y:0f,
                    aimPitch=Mathf.DeltaAngle(0f,view.AimRotation.eulerAngles.x), lookPitch=Mathf.DeltaAngle(0f,view.LookRotation.eulerAngles.x),
                    aimEnabled=aim.isActiveAndEnabled, animatorEnabled=animator.isActiveAndEnabled,
                    combatEnabled=combat.CanUseCombat, armed=combat.IsArmed, reloading=combat.Magazine!=null&&combat.Magazine.IsReloading, attacking=combat.IsAttacking,
                    trackBody=TrackBody!=null&&(bool)TrackBody.GetValue(aim), bodyBlend=BodyBlend!=null?(float)BodyBlend.GetValue(aim):-1f,
                    bodyWeight=BodyWeight!=null?(float)BodyWeight.GetValue(aim):-1f,
                    postureEnabled=posture!=null&&posture.isActiveAndEnabled, postureMultiplier=posture!=null?posture.UpperBodyAimMultiplier:1f,
                    bodyRendererEnabled=renderer!=null&&renderer.enabled, meshUsesChest=bound,
                    upperWeight=animator.GetLayerIndex("UpperCombat")>=0?animator.GetLayerWeight(animator.GetLayerIndex("UpperCombat")):0f,
                    baseState=animator.GetCurrentAnimatorStateInfo(0).fullPathHash, upperState=animator.GetLayerIndex("UpperCombat")>=0?animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("UpperCombat")).fullPathHash:0,
                    rootRotation=combat.transform.rotation, chestRotation=chest!=null?chest.rotation:Quaternion.identity, headRotation=head!=null?head.rotation:Quaternion.identity
                };
                File.AppendAllText(Path.Combine(Folder,"live.jsonl"),JsonUtility.ToJson(sample)+Environment.NewLine);
            }
        }
    }
}
