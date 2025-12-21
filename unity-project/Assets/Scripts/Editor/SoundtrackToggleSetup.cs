using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor script to add the Soundtrack Toggle button to the pause menu.
/// </summary>
public class SoundtrackToggleSetup : EditorWindow
{
    [MenuItem("Tools/Remove Soundtrack Toggle from Pause Menu")]
    public static void RemoveSoundtrackToggleFromPauseMenu()
    {
        // Find PauseMenuManager
        PauseMenuManager pauseMenu = Object.FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenu == null)
        {
            EditorUtility.DisplayDialog("Not Found", "PauseMenuManager not found in scene.", "OK");
            return;
        }

        if (pauseMenu.pauseMenuPanel == null)
        {
            EditorUtility.DisplayDialog("Not Found", "PauseMenuPanel not assigned in PauseMenuManager.", "OK");
            return;
        }

        Transform panelTransform = pauseMenu.pauseMenuPanel.transform;
        Transform existing = panelTransform.Find("SoundtrackToggleButton");

        if (existing == null)
        {
            EditorUtility.DisplayDialog("Not Found", "SoundtrackToggleButton not found in pause menu.\n\n(It may have already been removed, or the soundtrack toggle is now in the Settings panel.)", "OK");
            return;
        }

        Undo.DestroyObjectImmediate(existing.gameObject);
        EditorUtility.SetDirty(pauseMenu.pauseMenuPanel);

        EditorUtility.DisplayDialog("Removed",
            "SoundtrackToggleButton has been removed from the pause menu.\n\n" +
            "The soundtrack toggle is now available in the Settings panel.\n\n" +
            "Remember to save the scene!", "OK");
    }

    [MenuItem("Tools/Setup Soundtrack Toggle in Pause Menu")]
    public static void SetupSoundtrackToggle()
    {
        // Warn that this is deprecated
        bool proceed = EditorUtility.DisplayDialog("Deprecated",
            "The soundtrack toggle is now part of the Settings panel.\n\n" +
            "Use 'Tools > Setup Settings Panel in Pause Menu' instead.\n\n" +
            "Do you still want to add the standalone toggle to the pause menu?",
            "Yes, Add Anyway", "Cancel");

        if (!proceed) return;

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

        // Check if toggle already exists
        Transform existing = panelTransform.Find("SoundtrackToggleButton");
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists", "SoundtrackToggleButton already exists in the pause menu.", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Create the button
        GameObject buttonObj = new GameObject("SoundtrackToggleButton");
        Undo.RegisterCreatedObjectUndo(buttonObj, "Create Soundtrack Toggle");
        buttonObj.transform.SetParent(panelTransform, false);

        // Position it (will be ordered via sibling index)
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 50);

        // Add Image (background)
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.25f, 0.2f, 0.35f, 0.9f); // Purple tint for music

        // Add Button
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.25f, 0.2f, 0.35f, 0.9f);
        colors.highlightedColor = new Color(0.35f, 0.3f, 0.45f, 1f);
        colors.pressedColor = new Color(0.2f, 0.15f, 0.3f, 1f);
        colors.selectedColor = new Color(0.3f, 0.25f, 0.4f, 1f);
        button.colors = colors;

        // Add LayoutElement for sizing in layout groups
        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 50;
        layoutElement.preferredHeight = 50;

        // Create Text child
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

        // Add SoundtrackToggleUI component
        SoundtrackToggleUI toggleUI = buttonObj.AddComponent<SoundtrackToggleUI>();
        toggleUI.toggleButton = button;
        toggleUI.labelText = text;

        // Try to position after Resume button if it has a layout group
        // Find Resume button to position relative to it
        if (pauseMenu.resumeButton != null)
        {
            int resumeIndex = pauseMenu.resumeButton.transform.GetSiblingIndex();
            buttonObj.transform.SetSiblingIndex(resumeIndex + 1);
        }

        // Mark dirty
        EditorUtility.SetDirty(pauseMenu.pauseMenuPanel);

        Selection.activeGameObject = buttonObj;

        EditorUtility.DisplayDialog("Success",
            "Soundtrack Toggle button added to pause menu!\n\n" +
            "The button will toggle between:\n" +
            "- Nela's Score (Side A)\n" +
            "- Franco's Score (Side B)\n\n" +
            "Remember to save the scene!", "OK");
    }

    [MenuItem("Tools/Setup FMODAudioManager in Scene")]
    public static void SetupFMODAudioManager()
    {
        // Check if already exists
        FMODAudioManager existing = Object.FindFirstObjectByType<FMODAudioManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists",
                $"FMODAudioManager already exists on: {existing.gameObject.name}", "OK");
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
            targetObj = new GameObject("FMODAudioManager");
            Undo.RegisterCreatedObjectUndo(targetObj, "Create FMODAudioManager");
        }

        FMODAudioManager manager = Undo.AddComponent<FMODAudioManager>(targetObj);

        EditorUtility.SetDirty(targetObj);
        Selection.activeGameObject = targetObj;

        EditorUtility.DisplayDialog("Success",
            $"FMODAudioManager added to: {targetObj.name}\n\n" +
            "Remember to save the scene!", "OK");
    }
}
