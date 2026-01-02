using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using FMOD.Studio;
using FMODUnity;

/// <summary>
/// Manages FMOD audio events for Mental Break.
/// Handles both Side A (Nela) and Side B (Franco) soundtrack systems.
///
/// Side A Events: Use EndFade parameter (0=play, 1=fade out)
/// Side B Events: Use LoopChange + EndSection/EndFade parameters
/// </summary>
public class FMODAudioManager : MonoBehaviour
{
    // Retry settings for when banks aren't loaded yet
    private const int MAX_RETRY_ATTEMPTS = 10;
    private const float RETRY_DELAY_SECONDS = 0.5f;
    public static FMODAudioManager Instance { get; private set; }

    [Header("Event References")]
    [Tooltip("Currently playing music event instance")]
    private EventInstance currentMusicInstance;
    private string currentEventPath;

    // Event path constants
    private const string EVENT_PATH_PREFIX = "event:/";

    // PlayerPrefs key for soundtrack side preference
    private const string PREF_SOUNDTRACK_SIDE = "SoundtrackSide";

    // Parameter names
    private const string PARAM_END_FADE = "EndFade";
    private const string PARAM_END_SECTION = "EndSection";
    private const string PARAM_LOOP_CHANGE = "LoopChange";

    // Theme to event mapping (theme name -> (Side A event, Side B event))
    // null means the theme doesn't exist on that side (for future side-exclusive tracks)
    private static readonly Dictionary<string, (string sideA, string sideB)> ThemeMapping = new Dictionary<string, (string, string)>
    {
        { "MainTheme", ("ASIDE_MainTheme", "BSIDE_MainTheme") },
        { "AliceTheme", ("ASIDE_AliceTheme", "BSIDE_AliceTheme") },
        { "SupervisorTheme", ("ASIDE_Supervisor", "BSIDE_ArthurTheme") },
        // Aliases
        { "ArthurTheme", ("ASIDE_Supervisor", "BSIDE_ArthurTheme") },
    };

    // Event configurations
    private static readonly Dictionary<string, EventConfig> EventConfigs = new Dictionary<string, EventConfig>
    {
        // Side A (Nela) - All use EndFade
        { "ASIDE_MainTheme", new EventConfig(EventType.SideA) },
        { "ASIDE_AliceTheme", new EventConfig(EventType.SideA) },
        { "ASIDE_Supervisor", new EventConfig(EventType.SideA) },

        // Side B (Franco) - Different configurations
        { "BSIDE_MainTheme", new EventConfig(EventType.SideB_EndSection, 2) },      // LoopA/B only
        { "BSIDE_AliceTheme", new EventConfig(EventType.SideB_EndSection, 2) },     // LoopA/B only
        { "BSIDE_ArthurTheme", new EventConfig(EventType.SideB_EndFade, 4) },       // LoopA/B/C/D
    };

    private enum EventType
    {
        SideA,              // Uses EndFade only
        SideB_EndSection,   // Uses LoopChange + EndSection
        SideB_EndFade       // Uses LoopChange + EndFade
    }

    private struct EventConfig
    {
        public EventType Type;
        public int MaxLoops;

        public EventConfig(EventType type, int maxLoops = 0)
        {
            Type = type;
            MaxLoops = maxLoops;
        }
    }

    // Current music volume (0-1)
    private float musicVolume = 0.7f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Get initial volume from SettingsManager
        if (SettingsManager.Instance != null)
        {
            musicVolume = SettingsManager.Instance.GetMusicVolume();
            SettingsManager.Instance.OnMusicVolumeChanged += OnMusicVolumeChanged;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from volume changes
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= OnMusicVolumeChanged;
        }

        StopCurrentMusic(immediate: true);
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Called when music volume setting changes.
    /// </summary>
    private void OnMusicVolumeChanged(float volume)
    {
        musicVolume = volume;
        ApplyVolumeToCurrentInstance();
    }

    /// <summary>
    /// Apply current volume to the playing music instance.
    /// </summary>
    private void ApplyVolumeToCurrentInstance()
    {
        if (currentMusicInstance.isValid())
        {
            currentMusicInstance.setVolume(musicVolume);
        }
    }

    #region Public API - Soundtrack Side

    /// <summary>
    /// Get the current soundtrack side preference.
    /// </summary>
    /// <returns>"A" for Nela's Score, "B" for Franco's Score</returns>
    public string GetSoundtrackSide()
    {
        return PlayerPrefs.GetString(PREF_SOUNDTRACK_SIDE, "A");
    }

    /// <summary>
    /// Set the soundtrack side preference.
    /// </summary>
    /// <param name="side">"A" for Nela's Score, "B" for Franco's Score</param>
    public void SetSoundtrackSide(string side)
    {
        side = side.ToUpper();
        if (side != "A" && side != "B")
        {
            Debug.LogWarning($"[FMOD] Invalid soundtrack side: {side}. Use 'A' or 'B'.");
            return;
        }

        string oldSide = GetSoundtrackSide();
        PlayerPrefs.SetString(PREF_SOUNDTRACK_SIDE, side);
        PlayerPrefs.Save();

        Debug.Log($"[FMOD] Soundtrack side changed: {oldSide} -> {side}");

        // If music is playing, restart with the new side's version
        if (IsPlaying() && !string.IsNullOrEmpty(currentThemeName))
        {
            int currentLoop = GetCurrentLoop();
            PlayTheme(currentThemeName, currentLoop);
        }
    }

    /// <summary>
    /// Toggle between Side A and Side B.
    /// </summary>
    public void ToggleSoundtrackSide()
    {
        string current = GetSoundtrackSide();
        SetSoundtrackSide(current == "A" ? "B" : "A");
    }

    /// <summary>
    /// Check if currently on Side A (Nela's Score).
    /// </summary>
    public bool IsSideA() => GetSoundtrackSide() == "A";

    /// <summary>
    /// Check if currently on Side B (Franco's Score).
    /// </summary>
    public bool IsSideB() => GetSoundtrackSide() == "B";

    #endregion

    #region Public API - Theme Playback

    // Track current theme name for side-switching
    private string currentThemeName;

    /// <summary>
    /// Play a theme by name, automatically selecting the correct side based on preference.
    /// </summary>
    /// <param name="themeName">Theme name (e.g., "MainTheme", "AliceTheme", "SupervisorTheme")</param>
    /// <param name="startLoop">Starting loop for Side B events (0=LoopA, 1=LoopB, etc.)</param>
    public void PlayTheme(string themeName, int startLoop = 0)
    {
        if (!ThemeMapping.TryGetValue(themeName, out var mapping))
        {
            Debug.LogWarning($"[FMOD] Unknown theme: {themeName}. Playing as direct event name.");
            PlayMusic(themeName, startLoop);
            return;
        }

        string side = GetSoundtrackSide();
        string eventName = side == "A" ? mapping.sideA : mapping.sideB;

        // Handle side-exclusive tracks (for future use)
        if (string.IsNullOrEmpty(eventName))
        {
            // Fallback to the other side if this side doesn't have the track
            eventName = side == "A" ? mapping.sideB : mapping.sideA;
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning($"[FMOD] Theme {themeName} has no events defined.");
                return;
            }
            Debug.Log($"[FMOD] Theme {themeName} not available on Side {side}, using fallback.");
        }

        currentThemeName = themeName;
        PlayMusic(eventName, startLoop);
    }

    /// <summary>
    /// Stop the current theme.
    /// </summary>
    public void StopTheme(bool immediate = false)
    {
        currentThemeName = null;
        StopCurrentMusic(immediate);
    }

    /// <summary>
    /// Get the current loop index (for Side B events).
    /// </summary>
    private int GetCurrentLoop()
    {
        if (!currentMusicInstance.isValid()) return 0;

        currentMusicInstance.getParameterByName(PARAM_LOOP_CHANGE, out float value);
        return (int)value;
    }

    #endregion

    #region Public API - Direct Event Playback

    /// <summary>
    /// Play an FMOD music event by name.
    /// Will retry if banks aren't loaded yet.
    /// </summary>
    /// <param name="eventName">Event name (e.g., "ASIDE_MainTheme")</param>
    /// <param name="startLoop">Starting loop for Side B events (0=LoopA, 1=LoopB, etc.)</param>
    public void PlayMusic(string eventName, int startLoop = 0)
    {
        StartCoroutine(PlayMusicWithRetry(eventName, startLoop));
    }

    /// <summary>
    /// Internal coroutine that attempts to play music with retries.
    /// </summary>
    private IEnumerator PlayMusicWithRetry(string eventName, int startLoop)
    {
        string fullPath = EVENT_PATH_PREFIX + eventName;

        // Defensive check: Ensure Web Audio context is unlocked (WebGL only)
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!WebAudioUnlocker.IsUnlocked())
        {
            Debug.LogWarning($"[FMOD] Attempting to play {eventName} but Web Audio may be locked. Waiting for user interaction...");
        }
#endif

        // If same event is already playing, don't restart
        if (currentEventPath == fullPath && IsPlaying())
        {
            Debug.Log($"[FMOD] {eventName} already playing, skipping");
            yield break;
        }

        // Stop current music with proper ending
        StopCurrentMusic(immediate: false);

        // Try to create the event instance, with retries if banks aren't loaded yet
        currentEventPath = fullPath;
        bool success = false;
        bool shouldRetry = false;

        for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
        {
            shouldRetry = false;

            // Try to get the event (outside try-catch since we can't yield inside)
            FMOD.RESULT result = FMOD.RESULT.ERR_EVENT_NOTFOUND;
            EventDescription eventDesc = default;
            bool hadException = false;

            try
            {
                result = RuntimeManager.StudioSystem.getEvent(fullPath, out eventDesc);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FMOD] Exception on attempt {attempt + 1}: {e.Message}");
                hadException = true;
                shouldRetry = true;
            }

            if (!hadException)
            {
                if (result == FMOD.RESULT.OK && eventDesc.isValid())
                {
                    // Banks are loaded, create instance
                    try
                    {
                        currentMusicInstance = RuntimeManager.CreateInstance(fullPath);
                        if (currentMusicInstance.isValid())
                        {
                            success = true;
                            Debug.Log($"[FMOD] Event found on attempt {attempt + 1}: {eventName}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[FMOD] CreateInstance exception: {e.Message}");
                        shouldRetry = true;
                    }
                }
                else if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
                {
                    Debug.Log($"[FMOD] Event not found (attempt {attempt + 1}/{MAX_RETRY_ATTEMPTS}), banks may still be loading...");
                    shouldRetry = true;
                }
                else
                {
                    Debug.LogWarning($"[FMOD] getEvent returned {result} for {fullPath}");
                    shouldRetry = true;
                }
            }

            if (success)
            {
                break;
            }

            if (shouldRetry && attempt < MAX_RETRY_ATTEMPTS - 1)
            {
                yield return new WaitForSeconds(RETRY_DELAY_SECONDS);
            }
        }

        if (!success)
        {
            Debug.LogWarning($"[FMOD] Failed to create instance for '{fullPath}' after {MAX_RETRY_ATTEMPTS} attempts. " +
                "Banks may not have loaded correctly. Dialogue will continue without music.");
            currentEventPath = null;
            yield break;
        }

        // Set initial loop if Side B
        if (EventConfigs.TryGetValue(eventName, out EventConfig config))
        {
            if (config.Type == EventType.SideB_EndSection || config.Type == EventType.SideB_EndFade)
            {
                SetLoopInternal(startLoop, config.MaxLoops);
            }
        }

        // Apply current volume setting
        currentMusicInstance.setVolume(musicVolume);

        currentMusicInstance.start();
        Debug.Log($"[FMOD] Started: {eventName}" + (startLoop > 0 ? $" at Loop{(char)('A' + startLoop)}" : "") + $" (vol={musicVolume:F2})");
    }

    /// <summary>
    /// Stop current music with proper ending (fade/section based on event type).
    /// </summary>
    /// <param name="immediate">If true, stops immediately without fade</param>
    public void StopCurrentMusic(bool immediate = false)
    {
        if (!currentMusicInstance.isValid()) return;

        if (immediate)
        {
            currentMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            currentMusicInstance.release();
        }
        else
        {
            // Trigger proper ending based on event type
            string eventName = GetEventNameFromPath(currentEventPath);
            if (EventConfigs.TryGetValue(eventName, out EventConfig config))
            {
                switch (config.Type)
                {
                    case EventType.SideA:
                    case EventType.SideB_EndFade:
                        currentMusicInstance.setParameterByName(PARAM_END_FADE, 1f);
                        break;
                    case EventType.SideB_EndSection:
                        currentMusicInstance.setParameterByName(PARAM_END_SECTION, 1f);
                        break;
                }
            }
            else
            {
                // Default to EndFade
                currentMusicInstance.setParameterByName(PARAM_END_FADE, 1f);
            }

            // Release will happen when event ends naturally
            StartCoroutine(ReleaseWhenStopped());
        }

        Debug.Log($"[FMOD] Stopping: {currentEventPath} (immediate={immediate})");
        currentEventPath = null;
    }

    /// <summary>
    /// Change the current loop for Side B events.
    /// </summary>
    /// <param name="loopIndex">0=LoopA, 1=LoopB, 2=LoopC, 3=LoopD</param>
    public void SetLoop(int loopIndex)
    {
        if (!currentMusicInstance.isValid())
        {
            Debug.LogWarning("[FMOD] SetLoop called but no music playing");
            return;
        }

        string eventName = GetEventNameFromPath(currentEventPath);
        if (EventConfigs.TryGetValue(eventName, out EventConfig config))
        {
            if (config.Type == EventType.SideA)
            {
                Debug.LogWarning($"[FMOD] {eventName} is Side A event, doesn't support LoopChange");
                return;
            }
            SetLoopInternal(loopIndex, config.MaxLoops);
        }
    }

    /// <summary>
    /// Change loop by name (LoopA, LoopB, LoopC, LoopD).
    /// </summary>
    public void SetLoop(string loopName)
    {
        int index = loopName.ToUpper() switch
        {
            "LOOPA" => 0,
            "LOOPB" => 1,
            "LOOPC" => 2,
            "LOOPD" => 3,
            _ => -1
        };

        if (index < 0)
        {
            Debug.LogWarning($"[FMOD] Invalid loop name: {loopName}");
            return;
        }

        SetLoop(index);
    }

    /// <summary>
    /// Set a parameter on the current music event.
    /// </summary>
    public void SetParameter(string paramName, float value)
    {
        if (!currentMusicInstance.isValid())
        {
            Debug.LogWarning($"[FMOD] SetParameter({paramName}) called but no music playing");
            return;
        }

        currentMusicInstance.setParameterByName(paramName, value);
        Debug.Log($"[FMOD] Set {paramName} = {value}");
    }

    /// <summary>
    /// Check if music is currently playing.
    /// </summary>
    public bool IsPlaying()
    {
        if (!currentMusicInstance.isValid()) return false;

        currentMusicInstance.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING;
    }

    #endregion

    #region Yarn Commands

    /// <summary>
    /// Yarn command: <<music themeName [startLoop]>>
    /// Play a theme, automatically selecting the correct side (A/B) based on player preference.
    /// Themes: MainTheme, AliceTheme, SupervisorTheme
    /// </summary>
    [YarnCommand("music")]
    public void YarnPlayTheme(string themeName, int startLoop = 0)
    {
        PlayTheme(themeName, startLoop);
    }

    /// <summary>
    /// Yarn command: <<music_stop [immediate]>>
    /// Stop current music with proper ending.
    /// </summary>
    [YarnCommand("music_stop")]
    public void YarnStopTheme(bool immediate = false)
    {
        StopTheme(immediate);
    }

    /// <summary>
    /// Yarn command: <<fmod eventName [startLoop]>>
    /// Play an FMOD music event directly (bypasses side selection).
    /// </summary>
    [YarnCommand("fmod")]
    public void YarnPlayMusic(string eventName, int startLoop = 0)
    {
        PlayMusic(eventName, startLoop);
    }

    /// <summary>
    /// Yarn command: <<fmod_stop [immediate]>>
    /// Stop current music.
    /// </summary>
    [YarnCommand("fmod_stop")]
    public void YarnStopMusic(bool immediate = false)
    {
        StopCurrentMusic(immediate);
    }

    /// <summary>
    /// Yarn command: <<fmod_loop loopName>>
    /// Change loop (LoopA, LoopB, LoopC, LoopD).
    /// </summary>
    [YarnCommand("fmod_loop")]
    public void YarnSetLoop(string loopName)
    {
        SetLoop(loopName);
    }

    /// <summary>
    /// Yarn command: <<fmod_param paramName value>>
    /// Set a parameter on current music.
    /// </summary>
    [YarnCommand("fmod_param")]
    public void YarnSetParameter(string paramName, float value)
    {
        SetParameter(paramName, value);
    }

    #endregion

    #region Private Helpers

    private void SetLoopInternal(int loopIndex, int maxLoops)
    {
        if (maxLoops > 0 && loopIndex >= maxLoops)
        {
            Debug.LogWarning($"[FMOD] Loop index {loopIndex} exceeds max {maxLoops - 1}");
            loopIndex = maxLoops - 1;
        }

        currentMusicInstance.setParameterByName(PARAM_LOOP_CHANGE, loopIndex);
        Debug.Log($"[FMOD] LoopChange = {loopIndex} (Loop{(char)('A' + loopIndex)})");
    }

    private string GetEventNameFromPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        int lastSlash = path.LastIndexOf('/');
        return lastSlash >= 0 ? path.Substring(lastSlash + 1) : path;
    }

    private System.Collections.IEnumerator ReleaseWhenStopped()
    {
        var instance = currentMusicInstance;

        // Wait for event to finish
        while (instance.isValid())
        {
            instance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                instance.release();
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion
}
