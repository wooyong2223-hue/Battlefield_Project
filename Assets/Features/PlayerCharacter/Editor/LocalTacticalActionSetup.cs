using System.Linq;
using Battlefield.Features.PlayerCharacter;
using Battlefield.Features.VFX;
using Battlefield.Features.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Battlefield.Editor.PlayerCharacter
{
    [InitializeOnLoad]
    public static class LocalTacticalActionSetup
    {
        static LocalTacticalActionSetup()
        {
            SceneManager.sceneLoaded+=(scene,mode)=>{if(EditorApplication.isPlaying)Apply(scene);};
            EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode)for(int i=0;i<SceneManager.sceneCount;i++)Apply(SceneManager.GetSceneAt(i));};
        }
        public static void Apply(Scene scene)
        {
            var profile=AssetDatabase.LoadAssetAtPath<TacticalAnimationSet>(TacticalActionImporter.ProfilePath);
            if(profile==null || !profile.IsValid || !scene.IsValid() || !scene.isLoaded)return;
            var roots=scene.GetRootGameObjects();var effect=roots.SelectMany(r=>r.GetComponentsInChildren<ExplosionEffectPool>(true)).FirstOrDefault();
            var camera=roots.SelectMany(r=>r.GetComponentsInChildren<Camera>(true)).FirstOrDefault();
            foreach(var root in roots)
            foreach(var combat in root.GetComponentsInChildren<PlayerCharacterController>(true))
            {
                var movement=combat.GetComponent<PlayerCharacterMovement>();var stance=combat.GetComponent<PlayerCharacterStance>();var policy=combat.GetComponent<PlayerCharacterActionPolicy>();
                var animator=combat.GetComponentInChildren<Animator>(true);var animation=combat.GetComponentInChildren<PlayerCharacterAnimationView>(true);
                if(movement==null || stance==null || animator==null || policy==null || animation==null || combat.GetComponent<PlayerCharacterMantle>()!=null)continue;
                var mantle=combat.gameObject.AddComponent<PlayerCharacterMantle>();var grenade=combat.gameObject.AddComponent<PlayerCharacterGrenadeThrow>();
                mantle.Initialize(profile.Mantle,stance,combat.GetComponent<PlayerCharacterTraversal>(),policy);
                var data=new SerializedObject(combat);var aim=data.FindProperty("_aimSourceBehaviour").objectReferenceValue as IPlayerCharacterAimSource;
                grenade.Initialize(profile.Grenade,combat,stance,movement,policy,aim,combat.GetComponent<IPlayerCharacterThrowInput>(),effect,combat.GetComponent<InfantryLifeState>());
                var trajectory=combat.gameObject.AddComponent<GrenadeTrajectoryView>();
                trajectory.Initialize(grenade,AssetDatabase.LoadAssetAtPath<Material>("Assets/Features/UI/GrenadeTrajectory.mat"),camera);
                foreach(var hud in roots.SelectMany(r=>r.GetComponentsInChildren<PlayerCombatHud>(true)))hud.SetTacticalActions(grenade,mantle);
                policy.SetTacticalActions(mantle,grenade);movement.SetMantle(mantle);animation.SetMantle(mantle);
                var presentation=animator.gameObject.AddComponent<PlayerCharacterTacticalAnimationView>();
                var posture=animator.GetComponent<PlayerCharacterPostureView>();
                var contact=posture!=null ? new SerializedObject(posture).FindProperty("_slideContactData").objectReferenceValue as SkinnedPoseContactData : null;
                var skin=animator.GetComponentInChildren<SkinnedMeshRenderer>(true);
                presentation.Initialize(animator,profile,mantle,grenade,stance,combat.GetComponent<PlayerCharacterView>(),policy,contact,skin);
                var combatView=animator.GetComponent<PlayerCharacterCombatView>();if(combatView!=null)combatView.SetTacticalPresentation(presentation);
                var aimView=animator.GetComponent<PlayerCharacterWeaponAimView>();if(aimView!=null)aimView.SetTacticalPresentation(presentation);
                var arms=animator.GetComponent<FirstPersonArmsView>();if(arms==null)arms=combat.GetComponentInChildren<FirstPersonArmsView>(true);if(arms!=null)arms.SetTacticalPresentation(presentation);
                var occupant=combat.GetComponent<PlayerVehicleOccupant>();
                if(occupant!=null)
                {
                    var serialized=new SerializedObject(occupant);var list=serialized.FindProperty("_onFootComponents");
                    foreach(var component in new Behaviour[]{mantle,grenade,presentation,trajectory}){int index=list.arraySize;list.arraySize++;list.GetArrayElementAtIndex(index).objectReferenceValue=component;}
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }
    }
    public sealed class LocalTacticalActionBuildProcessor:IProcessSceneWithReport
    {
        public int callbackOrder=>1003;
        public void OnProcessScene(Scene scene,BuildReport report){if(report!=null)LocalTacticalActionSetup.Apply(scene);}
    }
}
