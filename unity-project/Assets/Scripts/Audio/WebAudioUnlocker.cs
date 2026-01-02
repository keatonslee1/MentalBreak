using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// Unlocks Web Audio context on WebGL to enable audio playback from keyboard input.
/// Browser autoplay policies require a user gesture to unlock audio - mouse clicks work,
/// but keyboard input doesn't. This class bridges to JavaScript to force resume.
///
/// Usage: Call WebAudioUnlocker.TryResumeAudioContext() on any user input.
/// Auto-creates itself before scene load, only active on WebGL builds.
/// </summary>
public class WebAudioUnlocker : MonoBehaviour
{
    public static WebAudioUnlocker Instance { get; private set; }

    private bool hasAttemptedUnlock = false;
    private bool isUnlocked = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int WebAudio_TryResume();

    [DllImport("__Internal")]
    private static extern int WebAudio_GetState();
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var go = new GameObject("WebAudioUnlocker");
        go.AddComponent<WebAudioUnlocker>();
        DontDestroyOnLoad(go);
        Debug.Log("[WebAudioUnlocker] Auto-created for WebGL build");
#endif
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Attempt to resume the Web Audio context. Call on any user input (keyboard or mouse).
    /// Safe to call multiple times - will early return if already unlocked.
    /// </summary>
    public static void TryResumeAudioContext()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Instance == null) return;

        if (Instance.isUnlocked)
        {
            return; // Already unlocked, no need to retry
        }

        int result = WebAudio_TryResume();
        Instance.hasAttemptedUnlock = true;

        if (result == 2) // Already running
        {
            Instance.isUnlocked = true;
            Debug.Log("[WebAudioUnlocker] Audio context is running");
        }
        else if (result == 1) // Attempted resume
        {
            Debug.Log("[WebAudioUnlocker] Attempted to resume audio context");
            // Check again in a moment to confirm unlock
            Instance.Invoke(nameof(CheckUnlockStatus), 0.1f);
        }
        else if (result == 0) // Failed
        {
            Debug.LogWarning("[WebAudioUnlocker] Failed to resume audio context");
        }
#endif
    }

    private void CheckUnlockStatus()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        int state = WebAudio_GetState();
        if (state == 2) // Running
        {
            isUnlocked = true;
            Debug.Log("[WebAudioUnlocker] Audio context successfully unlocked!");
        }
        else if (state == 1) // Still suspended
        {
            Debug.LogWarning("[WebAudioUnlocker] Audio context still suspended after unlock attempt");
        }
#endif
    }

    /// <summary>
    /// Check if audio context is unlocked.
    /// </summary>
    public static bool IsUnlocked()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return Instance != null && Instance.isUnlocked;
#else
        return true; // Not WebGL, assume audio works
#endif
    }
}
