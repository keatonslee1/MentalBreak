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
    private static readonly Color SliderBackgroundColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    private static readonly Color SliderFillColor = new Color(0.4f, 0.6f, 0.8f, 1f);
    private static readonly Color ButtonColor = new Color(0.25f, 0.25f, 0.3f, 1f);
    private static readonly Color ButtonHighlightColor = new Color(0.35f, 0.35f, 0.4f, 1f);

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

        // Check if settings panel already exists
        Transform existing = panelTransform.Find("SettingsPanel");
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists", "SettingsPanel already exists in the pause menu.", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
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
            "Settings Panel created in pause menu!\n\n" +
            "Features:\n" +
            "- Master Volume slider\n" +
            "- Music Volume slider\n" +
            "- SFX Volume slider\n" +
            "- Soundtrack Toggle (Nela/Franco)\n" +
            "- Reset to Defaults button\n" +
            "- Back button\n\n" +
            "Remember to save the scene!", "OK");
    }

    private static GameObject CreateSettingsPanel(Transform parent)
    {
        // Main panel
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

        // Add SettingsPanel component
        SettingsPanel settingsPanel = panelObj.AddComponent<SettingsPanel>();

        // Create content container with vertical layout
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(panelObj.transform, false);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(500f, 450f);
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20f;
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Title
        CreateLabel(contentObj.transform, "Settings", 36, TextAnchor.MiddleCenter, 50f);

        // Master Volume
        var masterSlider = CreateSliderRow(contentObj.transform, "Master Volume", out Component masterLabel);
        settingsPanel.masterVolumeSlider = masterSlider;
        settingsPanel.masterVolumeLabel = masterLabel;

        // Music Volume
        var musicSlider = CreateSliderRow(contentObj.transform, "Music Volume", out Component musicLabel);
        settingsPanel.musicVolumeSlider = musicSlider;
        settingsPanel.musicVolumeLabel = musicLabel;

        // SFX Volume
        var sfxSlider = CreateSliderRow(contentObj.transform, "SFX Volume", out Component sfxLabel);
        settingsPanel.sfxVolumeSlider = sfxSlider;
        settingsPanel.sfxVolumeLabel = sfxLabel;

        // Soundtrack Toggle
        var soundtrackButton = CreateSoundtrackToggle(contentObj.transform, out Component soundtrackLabel);
        settingsPanel.soundtrackToggleButton = soundtrackButton;
        settingsPanel.soundtrackLabel = soundtrackLabel;

        // Spacer
        CreateSpacer(contentObj.transform, 20f);

        // Button row
        GameObject buttonRow = new GameObject("ButtonRow");
        buttonRow.transform.SetParent(contentObj.transform, false);

        RectTransform buttonRowRect = buttonRow.AddComponent<RectTransform>();
        buttonRowRect.sizeDelta = new Vector2(0f, 50f);

        HorizontalLayoutGroup buttonLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
        buttonLayout.spacing = 20f;
        buttonLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonLayout.childControlWidth = false;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = false;
        buttonLayout.childForceExpandHeight = true;

        // Reset button
        var resetButton = CreateButton(buttonRow.transform, "Reset Defaults", 150f);
        settingsPanel.resetButton = resetButton;

        // Back button
        var backButton = CreateButton(buttonRow.transform, "Back", 150f);
        settingsPanel.backButton = backButton;

        // Start hidden
        panelObj.SetActive(false);

        return panelObj;
    }

    private static void CreateLabel(Transform parent, string text, int fontSize, TextAnchor alignment, float height)
    {
        GameObject labelObj = new GameObject("Label_" + text.Replace(" ", ""));
        labelObj.transform.SetParent(parent, false);

        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, height);

        LayoutElement layoutElement = labelObj.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = height;

        Text uiText = labelObj.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.alignment = alignment;
        uiText.color = Color.white;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static Slider CreateSliderRow(Transform parent, string labelText, out Component valueLabel)
    {
        GameObject rowObj = new GameObject("SliderRow_" + labelText.Replace(" ", ""));
        rowObj.transform.SetParent(parent, false);

        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, 40f);

        LayoutElement rowLayout = rowObj.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 40f;

        HorizontalLayoutGroup hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10f;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rowObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(150f, 40f);

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 150f;

        Text label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.fontSize = 20;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(rowObj.transform, false);

        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(220f, 30f);

        LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
        sliderLayout.preferredWidth = 220f;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.7f;

        // Slider background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);

        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.25f);
        bgRect.anchorMax = new Vector2(1f, 0.75f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = SliderBackgroundColor;

        // Slider fill area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5f, 0f);
        fillAreaRect.offsetMax = new Vector2(-5f, 0f);

        // Slider fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = SliderFillColor;

        slider.fillRect = fillRect;

        // Slider handle area
        GameObject handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(sliderObj.transform, false);

        RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        // Slider handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);

        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20f, 0f);

        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.handleRect = handleRect;

        // Value label
        GameObject valueLabelObj = new GameObject("Value");
        valueLabelObj.transform.SetParent(rowObj.transform, false);

        RectTransform valueRect = valueLabelObj.AddComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(60f, 40f);

        LayoutElement valueLayout = valueLabelObj.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 60f;

        Text valueText = valueLabelObj.AddComponent<Text>();
        valueText.text = "70%";
        valueText.fontSize = 20;
        valueText.alignment = TextAnchor.MiddleRight;
        valueText.color = Color.white;
        valueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        valueLabel = valueText;

        return slider;
    }

    private static Button CreateSoundtrackToggle(Transform parent, out Component label)
    {
        GameObject rowObj = new GameObject("SoundtrackToggle");
        rowObj.transform.SetParent(parent, false);

        RectTransform rowRect = rowObj.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, 50f);

        LayoutElement rowLayout = rowObj.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = 50f;

        HorizontalLayoutGroup hLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10f;
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(rowObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(150f, 50f);

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 150f;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = "Soundtrack";
        labelText.fontSize = 20;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Toggle button
        GameObject buttonObj = new GameObject("Button");
        buttonObj.transform.SetParent(rowObj.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(200f, 40f);

        LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = 200f;

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.25f, 0.2f, 0.35f, 0.9f); // Purple tint

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.25f, 0.2f, 0.35f, 0.9f);
        colors.highlightedColor = new Color(0.35f, 0.3f, 0.45f, 1f);
        colors.pressedColor = new Color(0.2f, 0.15f, 0.3f, 1f);
        colors.selectedColor = new Color(0.3f, 0.25f, 0.4f, 1f);
        button.colors = colors;

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);

        Text text = textObj.AddComponent<Text>();
        text.text = "Nela's Score";
        text.fontSize = 18;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        label = text;

        return button;
    }

    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacerObj = new GameObject("Spacer");
        spacerObj.transform.SetParent(parent, false);

        RectTransform rect = spacerObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, height);

        LayoutElement layout = spacerObj.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
    }

    private static Button CreateButton(Transform parent, string text, float width)
    {
        GameObject buttonObj = new GameObject("Button_" + text.Replace(" ", ""));
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, 50f);

        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredWidth = width;

        Image image = buttonObj.AddComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = ButtonHighlightColor;
        colors.pressedColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        colors.selectedColor = ButtonHighlightColor;
        button.colors = colors;

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text uiText = textObj.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = 20;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.color = Color.white;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

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
