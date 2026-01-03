using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor script to create and setup the Settings Panel UI.
/// Creates a complete settings panel with volume sliders and soundtrack toggle.
/// Uses TextMeshPro for all text (font applied by GlobalFontOverride).
/// </summary>
public class SettingsPanelSetup : EditorWindow
{
    private static readonly Color PanelBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    private static readonly Color RowBackgroundColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
    private static readonly Color ButtonColor = new Color(0.25f, 0.25f, 0.3f, 1f);
    private static readonly Color ButtonHighlightColor = new Color(0.35f, 0.35f, 0.4f, 1f);
    private static readonly Color SliderBgColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    private static readonly Color SliderFillColor = new Color(0.4f, 0.6f, 0.9f, 1f);

    [MenuItem("Tools/Setup Settings Panel in Pause Menu")]
    public static void SetupSettingsPanel()
    {
        // Find PauseMenuManager
        PauseMenuManager pauseMenu = Object.FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenu == null)
        {
            EditorUtility.DisplayDialog("Error", "PauseMenuManager not found in scene.", "OK");
            return;
        }

        if (pauseMenu.pauseMenuPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "PauseMenuPanel not assigned in PauseMenuManager.", "OK");
            return;
        }

        Transform panelTransform = pauseMenu.pauseMenuPanel.transform;

        // Check if settings panel already exists - delete it to recreate
        Transform existing = panelTransform.Find("SettingsPanel");
        if (existing != null)
        {
            bool recreate = EditorUtility.DisplayDialog("Settings Panel Exists",
                "SettingsPanel already exists. Delete and recreate it?",
                "Yes, Recreate", "Cancel");
            if (!recreate)
            {
                Selection.activeGameObject = existing.gameObject;
                return;
            }
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // Create the settings panel
        GameObject settingsPanelObj = CreateSettingsPanel(panelTransform);

        // Get the SettingsPanel component
        SettingsPanel settingsPanel = settingsPanelObj.GetComponent<SettingsPanel>();

        // Assign to PauseMenuManager
        pauseMenu.settingsPanel = settingsPanel;
        EditorUtility.SetDirty(pauseMenu);

        Selection.activeGameObject = settingsPanelObj;

        EditorUtility.DisplayDialog("Success",
            "Settings Panel created!\n\n" +
            "Features:\n" +
            "- Master Volume slider\n" +
            "- Music Volume slider\n" +
            "- SFX Volume slider\n" +
            "- Soundtrack Toggle\n" +
            "- Back button\n\n" +
            "Remember to save the scene!", "OK");
    }

    private static GameObject CreateSettingsPanel(Transform parent)
    {
        // Main panel - full screen overlay
        GameObject panelObj = new GameObject("SettingsPanel");
        Undo.RegisterCreatedObjectUndo(panelObj, "Create Settings Panel");
        panelObj.transform.SetParent(parent, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = PanelBackgroundColor;
        panelImage.raycastTarget = true;

        // Add SettingsPanel component
        SettingsPanel settingsPanelComp = panelObj.AddComponent<SettingsPanel>();
        settingsPanelComp.settingsPanel = panelObj;

        // Create centered content box - larger size
        GameObject contentBox = new GameObject("ContentBox");
        contentBox.transform.SetParent(panelObj.transform, false);

        RectTransform contentRect = contentBox.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(700f, 600f); // Larger content box for 60px font
        contentRect.anchoredPosition = Vector2.zero;

        Image contentBg = contentBox.AddComponent<Image>();
        contentBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

        // Title
        CreateTitle(contentBox.transform);

        // Volume sliders
        float yPos = 140f;
        settingsPanelComp.masterVolumeSlider = CreateVolumeRow(contentBox.transform, "Master", yPos, out Component masterLabel);
        settingsPanelComp.masterVolumeLabel = masterLabel;

        yPos -= 80f;
        settingsPanelComp.musicVolumeSlider = CreateVolumeRow(contentBox.transform, "Music", yPos, out Component musicLabel);
        settingsPanelComp.musicVolumeLabel = musicLabel;

        yPos -= 80f;
        settingsPanelComp.sfxVolumeSlider = CreateVolumeRow(contentBox.transform, "SFX", yPos, out Component sfxLabel);
        settingsPanelComp.sfxVolumeLabel = sfxLabel;

        // Soundtrack toggle
        yPos -= 90f;
        settingsPanelComp.soundtrackToggleButton = CreateSoundtrackRow(contentBox.transform, yPos, out Component soundtrackLabel);
        settingsPanelComp.soundtrackLabel = soundtrackLabel;

        // Back button
        settingsPanelComp.backButton = CreateBackButton(contentBox.transform);

        // Start hidden
        panelObj.SetActive(false);

        return panelObj;
    }

    private static void CreateTitle(Transform parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent, false);

        RectTransform rect = titleObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -15f);
        rect.sizeDelta = new Vector2(400f, 70f);

        TextMeshProUGUI text = titleObj.AddComponent<TextMeshProUGUI>();
        text.text = "SETTINGS";
        text.fontSize = 60;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
    }

    private static Slider CreateVolumeRow(Transform parent, string label, float yPos, out Component valueLabel)
    {
        // Row container
        GameObject rowObj = new GameObject($"{label}VolumeRow");
        rowObj.transform.SetParent(parent, false);

        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 0.5f);
        rowRect.anchorMax = new Vector2(1f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = new Vector2(0f, yPos);
        rowRect.sizeDelta = new Vector2(-40f, 70f); // Taller rows for larger font
        rowRect.offsetMin = new Vector2(25f, rowRect.offsetMin.y);
        rowRect.offsetMax = new Vector2(-25f, rowRect.offsetMax.y);

        Image rowBg = rowObj.AddComponent<Image>();
        rowBg.color = RowBackgroundColor;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rowObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(15f, 0f);
        labelRect.sizeDelta = new Vector2(180f, 0f);

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 60;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.color = Color.white;

        // Slider using DefaultControls for proper setup
        GameObject sliderObj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderObj.name = $"{label}Slider";
        sliderObj.transform.SetParent(rowObj.transform, false);

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(20f, 0f);
        sliderRect.sizeDelta = new Vector2(-280f, 20f);
        sliderRect.offsetMin = new Vector2(200f, -10f);
        sliderRect.offsetMax = new Vector2(-100f, 10f);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;

        // Style the slider
        Image bgImage = sliderObj.transform.Find("Background").GetComponent<Image>();
        bgImage.color = SliderBgColor;

        Transform fillArea = sliderObj.transform.Find("Fill Area");
        Image fillImage = fillArea.Find("Fill").GetComponent<Image>();
        fillImage.color = SliderFillColor;

        Transform handleArea = sliderObj.transform.Find("Handle Slide Area");
        Image handleImage = handleArea.Find("Handle").GetComponent<Image>();
        handleImage.color = Color.white;

        // Value label
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(rowObj.transform, false);

        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(1f, 0f);
        valueRect.anchorMax = new Vector2(1f, 1f);
        valueRect.pivot = new Vector2(1f, 0.5f);
        valueRect.anchoredPosition = new Vector2(-15f, 0f);
        valueRect.sizeDelta = new Vector2(80f, 0f);

        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = "70%";
        valueText.fontSize = 48;
        valueText.alignment = TextAlignmentOptions.MidlineRight;
        valueText.color = Color.white;

        valueLabel = valueText;
        return slider;
    }

    private static Button CreateSoundtrackRow(Transform parent, float yPos, out Component label)
    {
        // Row container
        GameObject rowObj = new GameObject("SoundtrackRow");
        rowObj.transform.SetParent(parent, false);

        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 0.5f);
        rowRect.anchorMax = new Vector2(1f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = new Vector2(0f, yPos);
        rowRect.sizeDelta = new Vector2(-40f, 70f); // Taller row for larger font
        rowRect.offsetMin = new Vector2(25f, rowRect.offsetMin.y);
        rowRect.offsetMax = new Vector2(-25f, rowRect.offsetMax.y);

        Image rowBg = rowObj.AddComponent<Image>();
        rowBg.color = RowBackgroundColor;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rowObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(15f, 0f);
        labelRect.sizeDelta = new Vector2(280f, 0f);

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = "Soundtrack";
        labelText.fontSize = 60;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelText.color = Color.white;

        // Toggle button
        GameObject buttonObj = new GameObject("ToggleButton");
        buttonObj.transform.SetParent(rowObj.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(-15f, 0f);
        buttonRect.sizeDelta = new Vector2(280f, 56f); // Larger button

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.25f, 0.4f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.25f, 0.4f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.35f, 0.5f, 1f);
        colors.pressedColor = new Color(0.25f, 0.2f, 0.35f, 1f);
        button.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Nela's Score";
        text.fontSize = 48;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        label = text;
        return button;
    }

    private static Button CreateBackButton(Transform parent)
    {
        GameObject buttonObj = new GameObject("BackButton");
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 35f);
        rect.sizeDelta = new Vector2(220f, 70f); // Larger button for 60px font

        Image image = buttonObj.AddComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = ButtonHighlightColor;
        colors.pressedColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        button.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Back";
        text.fontSize = 60;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return button;
    }

    [MenuItem("Tools/Setup SettingsManager in Scene")]
    public static void SetupSettingsManager()
    {
        // Check if already exists
        SettingsManager existing = Object.FindFirstObjectByType<SettingsManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists",
                $"SettingsManager already exists on: {existing.gameObject.name}", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Try to find Dialogue System to add it there (persists across scenes)
        GameObject dialogueSystem = GameObject.Find("Dialogue System");

        GameObject targetObj;
        if (dialogueSystem != null)
        {
            targetObj = dialogueSystem;
        }
        else
        {
            // Create new GameObject
            targetObj = new GameObject("SettingsManager");
            Undo.RegisterCreatedObjectUndo(targetObj, "Create SettingsManager");
        }

        SettingsManager manager = Undo.AddComponent<SettingsManager>(targetObj);

        EditorUtility.SetDirty(targetObj);
        Selection.activeGameObject = targetObj;

        EditorUtility.DisplayDialog("Success",
            $"SettingsManager added to: {targetObj.name}\n\n" +
            "Remember to save the scene!", "OK");
    }
}
