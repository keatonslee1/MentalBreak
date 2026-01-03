using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using FMODUnity;
#endif

/// <summary>
/// Unlocks FMOD audio on WebGL by calling mixerSuspend/mixerResume on first user gesture.
/// Auto-initializes on WebGL builds only.
/// </summary>
public class FMODAudioUnlocker : MonoBehaviour
{
    private static bool audioUnlocked = false;
    private static FMODAudioUnlocker instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (instance != null) return;

        #if UNITY_WEBGL && !UNITY_EDITOR
        var go = new GameObject("FMODAudioUnlocker");
        instance = go.AddComponent<FMODAudioUnlocker>();
        DontDestroyOnLoad(go);
        Debug.Log("[FMODAudioUnlocker] Initialized, waiting for user gesture...");
        #endif
    }

    private void Update()
    {
        if (audioUnlocked) return;

        // Check for any user gesture (click, touch, or key)
        bool hasGesture = Input.anyKeyDown ||
                          Input.GetMouseButtonDown(0) ||
                          Input.GetMouseButtonDown(1) ||
                          Input.touchCount > 0;

        if (hasGesture)
        {
            UnlockAudio();
        }
    }

    private void UnlockAudio()
    {
        if (audioUnlocked) return;

        #if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            var result = RuntimeManager.CoreSystem.mixerSuspend();
            Debug.Log($"[FMODAudioUnlocker] mixerSuspend: {result}");

            result = RuntimeManager.CoreSystem.mixerResume();
            Debug.Log($"[FMODAudioUnlocker] mixerResume: {result}");

            audioUnlocked = true;
            Debug.Log("[FMODAudioUnlocker] Audio unlocked successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODAudioUnlocker] Failed to unlock audio: {e}");
        }
        #else
        audioUnlocked = true;
        #endif
    }
}
