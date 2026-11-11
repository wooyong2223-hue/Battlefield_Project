using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace Battlefield.Editor.PlayerCharacter
{
    [InitializeOnLoad]
    public sealed class PlayerCharacterSceneComposition : IProcessSceneWithReport
    {
        static PlayerCharacterSceneComposition()
        {
            SceneManager.sceneLoaded += (scene, _) =>
            {
                if (EditorApplication.isPlaying) Apply(scene);
            };
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode) return;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    Apply(SceneManager.GetSceneAt(i));
            };
        }

        public int callbackOrder => 1000;

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report != null) Apply(scene);
        }

        private static void Apply(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            SlideAnimationSceneSetup.Apply(scene);
            ProneAnimationSceneSetup.Apply(scene);
            ArmedLocomotionSceneSetup.Apply(scene);
            TacticalActionSceneSetup.Apply(scene);
        }
    }
}
