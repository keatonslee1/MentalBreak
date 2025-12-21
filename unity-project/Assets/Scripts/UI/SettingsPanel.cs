using UnityEngine;
using UnityEngine.UI;

#if USE_TMP
using TMPro;
#endif

/// <summary>
/// Settings panel UI component for the pause menu.
/// Handles volume sliders, soundtrack toggle, and other settings.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("Panel Reference")]
    [Tooltip("The settings panel GameObject (this object or a child)")]
    public GameObject settingsPanel;

    [Header("Volume Sliders")]
    [Tooltip("Master volume slider")]
    public Slider masterVolumeSlider;

    [Tooltip("Music volume slider")]
    public Slider musicVolumeSlider;

    [Tooltip("SFX volume slider")]
    public Slider sfxVolumeSlider;

    [Header("Volume Labels")]
    [Tooltip("Master volume value label")]
    public Component masterVolumeLabel;

    [Tooltip("Music volume value label")]
    public Component musicVolumeLabel;

    [Tooltip("SFX volume value label")]
    public Component sfxVolumeLabel;

    [Header("Soundtrack Toggle")]
    [Tooltip("Soundtrack toggle button (uses SoundtrackToggleUI)")]
    public Button soundtrackToggleButton;

    [Tooltip("Soundtrack toggle label")]
    public Component soundtrackLabel;

    [Header("Navigation")]
    [Tooltip("Back button to return to pause menu")]
    public Button backButton;

    [Header("Reset")]
    [Tooltip("Reset to defaults button")]
    public Button resetButton;

    // Cached SoundtrackToggleUI if present
    private SoundtrackToggleUI soundtrackToggleUI;

    // Flag to prevent feedback loops when setting slider values
    private bool isUpdatingUI = false;

    void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }

        // Try to find SoundtrackToggleUI on the soundtrack button
        if (soundtrackToggleButton != null)
        {
            soundtrackToggleUI = soundtrackToggleButton.GetComponent<SoundtrackToggleUI>();
        }
    }

    void Start()
    {
        SetupSliders();
        SetupButtons();

        // Subscribe to settings changes
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMasterVolumeChanged += OnMasterVolumeChanged;
            SettingsManager.Instance.OnMusicVolumeChanged += OnMusicVolumeChanged;
            SettingsManager.Instance.OnSFXVolumeChanged += OnSFXVolumeChanged;
        }

        // Initialize UI with current values
        RefreshUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from settings changes
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMasterVolumeChanged -= OnMasterVolumeChanged;
            SettingsManager.Instance.OnMusicVolumeChanged -= OnMusicVolumeChanged;
            SettingsManager.Instance.OnSFXVolumeChanged -= OnSFXVolumeChanged;
        }
    }

    void OnEnable()
    {
        // Refresh UI when panel becomes visible
        RefreshUI();
    }

    /// <summary>
    /// Setup slider listeners.
    /// </summary>
    private void SetupSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    /// <summary>
    /// Setup button listeners.
    /// </summary>
    private void SetupButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetClicked);
        }

        // Soundtrack toggle button is handled by SoundtrackToggleUI if present
        // Otherwise, wire it up manually
        if (soundtrackToggleButton != null && soundtrackToggleUI == null)
        {
            soundtrackToggleButton.onClick.AddListener(OnSoundtrackToggleClicked);
        }
    }

    /// <summary>
    /// Refresh all UI elements to match current settings.
    /// </summary>
    public void RefreshUI()
    {
        isUpdatingUI = true;

        if (SettingsManager.Instance != null)
        {
            // Update sliders
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = SettingsManager.Instance.GetMasterVolume();
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = SettingsManager.Instance.GetMusicVolume();
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = SettingsManager.Instance.GetSFXVolume();
            }

            // Update labels
            UpdateVolumeLabel(masterVolumeLabel, SettingsManager.Instance.GetMasterVolume());
            UpdateVolumeLabel(musicVolumeLabel, SettingsManager.Instance.GetMusicVolume());
            UpdateVolumeLabel(sfxVolumeLabel, SettingsManager.Instance.GetSFXVolume());
        }

        // Update soundtrack label
        UpdateSoundtrackLabel();

        isUpdatingUI = false;
    }

    /// <summary>
    /// Update a volume label with percentage text.
    /// </summary>
    private void UpdateVolumeLabel(Component label, float volume)
    {
        if (label == null) return;

        string text = $"{Mathf.RoundToInt(volume * 100)}%";

#if USE_TMP
        if (label is TextMeshProUGUI tmpText)
        {
            tmpText.text = text;
            return;
        }
#endif
        if (label is Text uiText)
        {
            uiText.text = text;
        }
    }

    /// <summary>
    /// Update the soundtrack toggle label.
    /// </summary>
    private void UpdateSoundtrackLabel()
    {
        if (soundtrackLabel == null) return;

        string side = "A";
        if (FMODAudioManager.Instance != null)
        {
            side = FMODAudioManager.Instance.GetSoundtrackSide();
        }

        string text = side == "A" ? "Nela's Score" : "Franco's Score";

#if USE_TMP
        if (soundtrackLabel is TextMeshProUGUI tmpText)
        {
            tmpText.text = text;
            return;
        }
#endif
        if (soundtrackLabel is Text uiText)
        {
            uiText.text = text;
        }
    }

    #region Slider Callbacks

    private void OnMasterSliderChanged(float value)
    {
        if (isUpdatingUI) return;

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMasterVolume(value);
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        if (isUpdatingUI) return;

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXSliderChanged(float value)
    {
        if (isUpdatingUI) return;

        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SetSFXVolume(value);
        }
    }

    #endregion

    #region Settings Change Callbacks

    private void OnMasterVolumeChanged(float volume)
    {
        if (!isUpdatingUI)
        {
            isUpdatingUI = true;
            if (masterVolumeSlider != null) masterVolumeSlider.value = volume;
            isUpdatingUI = false;
        }
        UpdateVolumeLabel(masterVolumeLabel, volume);
    }

    private void OnMusicVolumeChanged(float volume)
    {
        if (!isUpdatingUI)
        {
            isUpdatingUI = true;
            if (musicVolumeSlider != null) musicVolumeSlider.value = volume;
            isUpdatingUI = false;
        }
        UpdateVolumeLabel(musicVolumeLabel, volume);
    }

    private void OnSFXVolumeChanged(float volume)
    {
        if (!isUpdatingUI)
        {
            isUpdatingUI = true;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = volume;
            isUpdatingUI = false;
        }
        UpdateVolumeLabel(sfxVolumeLabel, volume);
    }

    #endregion

    #region Button Callbacks

    private void OnBackClicked()
    {
        // Save settings when leaving
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.SaveSettings();
        }

        // Hide settings panel
        Hide();

        // Show pause menu buttons
        if (PauseMenuManager.Instance != null)
        {
            PauseMenuManager.Instance.ShowPauseMenu();
        }
    }

    private void OnResetClicked()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.ResetToDefaults();
            RefreshUI();
        }
    }

    private void OnSoundtrackToggleClicked()
    {
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.ToggleSoundtrackSide();
            UpdateSoundtrackLabel();
        }
    }

    #endregion

    #region Show/Hide

    /// <summary>
    /// Show the settings panel.
    /// </summary>
    public void Show()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            RefreshUI();
        }
    }

    /// <summary>
    /// Hide the settings panel.
    /// </summary>
    public void Hide()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Check if the settings panel is visible.
    /// </summary>
    public bool IsVisible()
    {
        return settingsPanel != null && settingsPanel.activeSelf;
    }

    #endregion
}
