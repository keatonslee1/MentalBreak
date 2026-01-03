using UnityEditor;
using UnityEngine;

/// <summary>
/// Ensures Unity Audio remains enabled despite FMOD's recommendation to disable it.
/// FMOD Setup Wizard can automatically disable Unity audio, but we need it for Mumble Generator.
/// This script runs on editor load and re-enables Unity audio if it was disabled.
/// </summary>
[InitializeOnLoad]
public static class ForceEnableUnityAudio
{
    static ForceEnableUnityAudio()
    {
        // Use delayCall to ensure Unity is fully initialized
        EditorApplication.delayCall += CheckAndEnableUnityAudio;
    }

    private static void CheckAndEnableUnityAudio()
    {
        var audioManager = AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/AudioManager.asset");
        if (audioManager == null)
        {
            Debug.LogWarning("ForceEnableUnityAudio: Could not find AudioManager.asset");
            return;
        }

        var serializedManager = new SerializedObject(audioManager);
        var disableAudioProp = serializedManager.FindProperty("m_DisableAudio");

        if (disableAudioProp == null)
        {
            Debug.LogWarning("ForceEnableUnityAudio: Could not find m_DisableAudio property");
            return;
        }

        if (disableAudioProp.boolValue)
        {
            disableAudioProp.boolValue = false;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log("ForceEnableUnityAudio: Re-enabled Unity Audio (was disabled, likely by FMOD Setup Wizard)");
        }
    }
}
