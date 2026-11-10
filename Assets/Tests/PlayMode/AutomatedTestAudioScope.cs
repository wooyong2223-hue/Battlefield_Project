#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Battlefield.Tests.PlayMode
{
    // Editor command-line flags are not the mute verification. Scope the master
    // mute to non-audio batch tests and restore the process's original value.
    [InitializeOnLoad]
    internal static class AutomatedTestAudioScope
    {
        private const string Key = "Battlefield.AutomatedTestAudioScope.";
        private static bool _restoring;
        static AutomatedTestAudioScope()
        {
            var args = Environment.GetCommandLineArgs();
            if (!Application.isBatchMode || !args.Contains("-runTests") || !args.Contains("-noaudio")) return;
            if (!SessionState.GetBool(Key + "Captured", false))
            {
                SessionState.SetBool(Key + "Previous", EditorUtility.audioMasterMute);
                SessionState.SetBool(Key + "Captured", true);
                Debug.Log("TEST_AUDIO captured previous=" + EditorUtility.audioMasterMute);
            }
            EnsureMuted();
            EditorApplication.update += EnsureMuted;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.quitting += Restore;
        }
        private static void EnsureMuted()
        {
            if (!_restoring && !EditorUtility.audioMasterMute) EditorUtility.audioMasterMute = true;
        }
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            EnsureMuted();
            Debug.Log("TEST_AUDIO " + state + " masterMuted=" + EditorUtility.audioMasterMute);
        }
        private static void Restore()
        {
            _restoring = true;
            EditorApplication.update -= EnsureMuted;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.quitting -= Restore;
            EditorUtility.audioMasterMute = SessionState.GetBool(Key + "Previous", false);
            Debug.Log("TEST_AUDIO restored=" + EditorUtility.audioMasterMute);
        }
    }
}
#endif
