using UnityEngine;
using UnityEngine.UI;

#if USE_TMP
using TMPro;
#endif

/// <summary>
/// UI component for toggling between Nela's Score (Side A) and Franco's Score (Side B).
/// Can be added to any button to create a soundtrack toggle.
/// </summary>
public class SoundtrackToggleUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Button that toggles the soundtrack. If null, uses GetComponent<Button>().")]
    public Button toggleButton;

    [Tooltip("Text component showing current selection. If null, searches in children.")]
    public Component labelText;

    [Header("Labels")]
    [Tooltip("Text shown when Side A (Nela's Score) is active")]
    public string sideALabel = "Nela's Score";

    [Tooltip("Text shown when Side B (Franco's Score) is active")]
    public string sideBLabel = "Franco's Score";

    private void Awake()
    {
        // Auto-find button if not assigned
        if (toggleButton == null)
        {
            toggleButton = GetComponent<Button>();
        }

        // Auto-find label text if not assigned
        if (labelText == null)
        {
#if USE_TMP
            labelText = GetComponentInChildren<TextMeshProUGUI>(true);
#endif
            if (labelText == null)
            {
                labelText = GetComponentInChildren<Text>(true);
            }
        }
    }

    private void Start()
    {
        // Wire up button click
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(OnToggleClicked);
        }

        // Update label to show current state
        UpdateLabel();
    }

    private void OnEnable()
    {
        // Update label when becoming visible
        UpdateLabel();
    }

    /// <summary>
    /// Called when the toggle button is clicked.
    /// </summary>
    public void OnToggleClicked()
    {
        if (FMODAudioManager.Instance == null)
        {
            Debug.LogWarning("[SoundtrackToggle] FMODAudioManager not found!");
            return;
        }

        FMODAudioManager.Instance.ToggleSoundtrackSide();
        UpdateLabel();
    }

    /// <summary>
    /// Update the label text to reflect current soundtrack side.
    /// </summary>
    public void UpdateLabel()
    {
        if (labelText == null) return;

        string currentSide = "A";
        if (FMODAudioManager.Instance != null)
        {
            currentSide = FMODAudioManager.Instance.GetSoundtrackSide();
        }

        string label = currentSide == "A" ? sideALabel : sideBLabel;

#if USE_TMP
        if (labelText is TextMeshProUGUI tmpText)
        {
            tmpText.text = label;
            return;
        }
#endif
        if (labelText is Text uiText)
        {
            uiText.text = label;
        }
    }

    /// <summary>
    /// Programmatically set to Side A.
    /// </summary>
    public void SetSideA()
    {
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.SetSoundtrackSide("A");
            UpdateLabel();
        }
    }

    /// <summary>
    /// Programmatically set to Side B.
    /// </summary>
    public void SetSideB()
    {
        if (FMODAudioManager.Instance != null)
        {
            FMODAudioManager.Instance.SetSoundtrackSide("B");
            UpdateLabel();
        }
    }
}
