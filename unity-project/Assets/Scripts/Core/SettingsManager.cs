using UnityEngine;

/// <summary>
/// Centralized settings manager for game preferences.
/// Handles persistence via PlayerPrefs and provides events for settings changes.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // PlayerPrefs keys
    private const string PREF_MASTER_VOLUME = "MasterVolume";
    private const string PREF_MUSIC_VOLUME = "MusicVolume";
    private const string PREF_SFX_VOLUME = "SFXVolume";
    private const string PREF_TEXT_SPEED = "TextSpeed";

    // Default values
    private const float DEFAULT_MASTER_VOLUME = 1.0f;
    private const float DEFAULT_MUSIC_VOLUME = 0.7f;
    private const float DEFAULT_SFX_VOLUME = 1.0f;
    private const float DEFAULT_TEXT_SPEED = 1.0f;

    // Cached values
    private float masterVolume;
    private float musicVolume;
    private float sfxVolume;
    private float textSpeed;

    // Events for when settings change
    public System.Action<float> OnMasterVolumeChanged;
    public System.Action<float> OnMusicVolumeChanged;
    public System.Action<float> OnSFXVolumeChanged;
    public System.Action<float> OnTextSpeedChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load all settings from PlayerPrefs.
    /// </summary>
    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(PREF_MASTER_VOLUME, DEFAULT_MASTER_VOLUME);
        musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC_VOLUME, DEFAULT_MUSIC_VOLUME);
        sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOLUME, DEFAULT_SFX_VOLUME);
        textSpeed = PlayerPrefs.GetFloat(PREF_TEXT_SPEED, DEFAULT_TEXT_SPEED);

        Debug.Log($"[Settings] Loaded: Master={masterVolume:F2}, Music={musicVolume:F2}, SFX={sfxVolume:F2}, TextSpeed={textSpeed:F2}");

        // Apply settings immediately
        ApplyAllSettings();
    }

    /// <summary>
    /// Save all settings to PlayerPrefs.
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(PREF_MASTER_VOLUME, masterVolume);
        PlayerPrefs.SetFloat(PREF_MUSIC_VOLUME, musicVolume);
        PlayerPrefs.SetFloat(PREF_SFX_VOLUME, sfxVolume);
        PlayerPrefs.SetFloat(PREF_TEXT_SPEED, textSpeed);
        PlayerPrefs.Save();

        Debug.Log("[Settings] Saved to PlayerPrefs");
    }

    /// <summary>
    /// Apply all settings to their respective systems.
    /// </summary>
    private void ApplyAllSettings()
    {
        // Apply master volume to AudioListener
        AudioListener.volume = masterVolume;

        // Notify listeners
        OnMasterVolumeChanged?.Invoke(masterVolume);
        OnMusicVolumeChanged?.Invoke(musicVolume);
        OnSFXVolumeChanged?.Invoke(sfxVolume);
        OnTextSpeedChanged?.Invoke(textSpeed);
    }

    #region Master Volume

    /// <summary>
    /// Get the current master volume (0.0 to 1.0).
    /// </summary>
    public float GetMasterVolume() => masterVolume;

    /// <summary>
    /// Set the master volume (0.0 to 1.0).
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (Mathf.Approximately(masterVolume, volume)) return;

        masterVolume = volume;
        AudioListener.volume = volume;
        OnMasterVolumeChanged?.Invoke(volume);

        Debug.Log($"[Settings] Master volume: {volume:F2}");
    }

    #endregion

    #region Music Volume

    /// <summary>
    /// Get the current music volume (0.0 to 1.0).
    /// </summary>
    public float GetMusicVolume() => musicVolume;

    /// <summary>
    /// Set the music volume (0.0 to 1.0).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (Mathf.Approximately(musicVolume, volume)) return;

        musicVolume = volume;
        OnMusicVolumeChanged?.Invoke(volume);

        Debug.Log($"[Settings] Music volume: {volume:F2}");
    }

    /// <summary>
    /// Get the effective music volume (music * master).
    /// </summary>
    public float GetEffectiveMusicVolume() => musicVolume * masterVolume;

    #endregion

    #region SFX Volume

    /// <summary>
    /// Get the current SFX volume (0.0 to 1.0).
    /// </summary>
    public float GetSFXVolume() => sfxVolume;

    /// <summary>
    /// Set the SFX volume (0.0 to 1.0).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (Mathf.Approximately(sfxVolume, volume)) return;

        sfxVolume = volume;
        OnSFXVolumeChanged?.Invoke(volume);

        Debug.Log($"[Settings] SFX volume: {volume:F2}");
    }

    /// <summary>
    /// Get the effective SFX volume (sfx * master).
    /// </summary>
    public float GetEffectiveSFXVolume() => sfxVolume * masterVolume;

    #endregion

    #region Text Speed

    /// <summary>
    /// Get the current text speed multiplier (0.5 to 2.0).
    /// </summary>
    public float GetTextSpeed() => textSpeed;

    /// <summary>
    /// Set the text speed multiplier (0.5 to 2.0).
    /// </summary>
    public void SetTextSpeed(float speed)
    {
        speed = Mathf.Clamp(speed, 0.5f, 2.0f);
        if (Mathf.Approximately(textSpeed, speed)) return;

        textSpeed = speed;
        OnTextSpeedChanged?.Invoke(speed);

        Debug.Log($"[Settings] Text speed: {speed:F2}x");
    }

    #endregion

    #region Reset

    /// <summary>
    /// Reset all settings to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        SetMasterVolume(DEFAULT_MASTER_VOLUME);
        SetMusicVolume(DEFAULT_MUSIC_VOLUME);
        SetSFXVolume(DEFAULT_SFX_VOLUME);
        SetTextSpeed(DEFAULT_TEXT_SPEED);
        SaveSettings();

        Debug.Log("[Settings] Reset to defaults");
    }

    #endregion

    void OnDestroy()
    {
        // Auto-save when destroyed
        if (Instance == this)
        {
            SaveSettings();
            Instance = null;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Save settings when app is paused (mobile/WebGL)
        if (pauseStatus)
        {
            SaveSettings();
        }
    }

    void OnApplicationQuit()
    {
        SaveSettings();
    }
}
