using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor script to create and setup the Settings Panel UI.
/// Creates a complete settings panel with volume sliders and soundtrack toggle.
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
        contentRect.sizeDelta = new Vector2(500f, 480f); // Larger content box
        contentRect.anchoredPosition = Vector2.zero;

        Image contentBg = contentBox.AddComponent<Image>();
        contentBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

        // Title
        CreateTitle(contentBox.transform);

        // Volume sliders
        float yPos = 115f;
        settingsPanelComp.masterVolumeSlider = CreateVolumeRow(contentBox.transform, "Master", yPos, out Component masterLabel);
        settingsPanelComp.masterVolumeLabel = masterLabel;

        yPos -= 60f;
        settingsPanelComp.musicVolumeSlider = CreateVolumeRow(contentBox.transform, "Music", yPos, out Component musicLabel);
        settingsPanelComp.musicVolumeLabel = musicLabel;

        yPos -= 60f;
        settingsPanelComp.sfxVolumeSlider = CreateVolumeRow(contentBox.transform, "SFX", yPos, out Component sfxLabel);
        settingsPanelComp.sfxVolumeLabel = sfxLabel;

        // Soundtrack toggle
        yPos -= 70f;
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
        rect.sizeDelta = new Vector2(300f, 50f);

        Text text = titleObj.AddComponent<Text>();
        text.text = "SETTINGS";
        text.fontSize = 36; // Larger title
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        rowRect.sizeDelta = new Vector2(-40f, 50f); // Taller rows
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
        labelRect.sizeDelta = new Vector2(90f, 0f);

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = label;
        labelText.fontSize = 22; // Larger font
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Slider using DefaultControls for proper setup
        GameObject sliderObj = DefaultControls.CreateSlider(new DefaultControls.Resources());
        sliderObj.name = $"{label}Slider";
        sliderObj.transform.SetParent(rowObj.transform, false);

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(20f, 0f);
        sliderRect.sizeDelta = new Vector2(-180f, 20f);
        sliderRect.offsetMin = new Vector2(100f, -10f);
        sliderRect.offsetMax = new Vector2(-70f, 10f);

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
        valueRect.sizeDelta = new Vector2(50f, 0f);

        Text valueText = valueObj.AddComponent<Text>();
        valueText.text = "70%";
        valueText.fontSize = 20; // Larger font
        valueText.alignment = TextAnchor.MiddleRight;
        valueText.color = Color.white;
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
        rowRect.sizeDelta = new Vector2(-40f, 50f); // Taller row
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
        labelRect.sizeDelta = new Vector2(130f, 0f);

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "Soundtrack";
        labelText.fontSize = 22; // Larger font
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Toggle button
        GameObject buttonObj = new GameObject("ToggleButton");
        buttonObj.transform.SetParent(rowObj.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0.5f);
        buttonRect.anchorMax = new Vector2(1f, 0.5f);
        buttonRect.pivot = new Vector2(1f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(-15f, 0f);
        buttonRect.sizeDelta = new Vector2(200f, 40f); // Larger button

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

        Text text = textObj.AddComponent<Text>();
        text.text = "Nela's Score";
        text.fontSize = 18; // Larger font
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
        rect.sizeDelta = new Vector2(180f, 50f); // Larger button

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

        Text text = textObj.AddComponent<Text>();
        text.text = "Back";
        text.fontSize = 22; // Larger font
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
