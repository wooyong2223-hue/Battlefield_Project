using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
namespace Battlefield.Editor.PlayerCharacter
{
    [InitializeOnLoad]
    public static class LocalArmedLocomotionSetup
    {
        public const string ProfilePath = MixamoArmedLocomotionImporter.ProfilePath;
        static LocalArmedLocomotionSetup()
        {
            SceneManager.sceneLoaded += (scene, mode) => { if (EditorApplication.isPlaying) Apply(scene); };
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                    for (int i=0;i<SceneManager.sceneCount;i++) Apply(SceneManager.GetSceneAt(i));
            };
        }
        public static void Apply(Scene scene)
        {
            var profile=AssetDatabase.LoadAssetAtPath<ArmedLocomotionAnimationSet>(ProfilePath);
            if(profile==null||!scene.IsValid()||!scene.isLoaded)return;
            foreach(var root in scene.GetRootGameObjects())foreach(var view in root.GetComponentsInChildren<PlayerCharacterCombatView>(true))view.SetLocomotionAnimations(profile);
        }
    }
    public sealed class LocalArmedLocomotionBuildProcessor:IProcessSceneWithReport
    {
        public int callbackOrder=>1002;
        public void OnProcessScene(Scene scene,BuildReport report){if(report!=null)LocalArmedLocomotionSetup.Apply(scene);}
    }
}
