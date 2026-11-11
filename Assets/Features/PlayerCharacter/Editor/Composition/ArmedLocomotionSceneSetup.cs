using Battlefield.Features.PlayerCharacter;
using UnityEditor;
using UnityEngine.SceneManagement;
namespace Battlefield.Editor.PlayerCharacter
{
    public static class ArmedLocomotionSceneSetup
    {
        public const string ProfilePath = MixamoArmedLocomotionImporter.ProfilePath;
        public static void Apply(Scene scene)
        {
            var profile=AssetDatabase.LoadAssetAtPath<ArmedLocomotionAnimationSet>(ProfilePath);
            if(profile==null||!scene.IsValid()||!scene.isLoaded)return;
            foreach(var root in scene.GetRootGameObjects())foreach(var view in root.GetComponentsInChildren<PlayerCharacterCombatView>(true))view.SetLocomotionAnimations(profile);
        }
    }
}
