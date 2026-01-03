using TMPro;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor script to automatically set up the pause menu UI in the current scene.
/// Uses TextMeshPro for all text (font applied by GlobalFontOverride).
/// Usage: Unity Menu -> Tools -> Setup Pause Menu UI
/// </summary>
public class PauseMenuSetup : EditorWindow
{
    [MenuItem("Tools/Setup Pause Menu UI")]
    public static void SetupPauseMenu()
    {
        // Find or create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if it doesn't exist
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            Debug.Log("Created Canvas and EventSystem");
        }

        // Check for existing pause menu elements
        Transform existingHint = canvas.transform.Find("PauseHintText");
        Transform existingPanel = canvas.transform.Find("PauseMenuPanel");

        if (existingHint != null || existingPanel != null)
        {
            bool recreate = EditorUtility.DisplayDialog("Pause Menu Exists",
                "PauseMenuPanel and/or PauseHintText already exist. Delete and recreate them with updated settings?",
                "Yes, Recreate", "Cancel");
            if (!recreate)
            {
                if (existingPanel != null)
                    Selection.activeGameObject = existingPanel.gameObject;
                return;
            }

            // Delete existing elements
            if (existingHint != null)
            {
                Undo.DestroyObjectImmediate(existingHint.gameObject);
                Debug.Log("Deleted existing PauseHintText");
            }
            if (existingPanel != null)
            {
                Undo.DestroyObjectImmediate(existingPanel.gameObject);
                Debug.Log("Deleted existing PauseMenuPanel");
            }
        }

        // Create Pause Hint Text
        GameObject hintTextObj = CreateHintText(canvas.transform);
        Debug.Log("Created PauseHintText");

        // Create Pause Menu Panel
        GameObject panelObj = CreatePausePanel(canvas.transform);
        Debug.Log("Created PauseMenuPanel");

        // Create buttons
        CreateMenuButtons(panelObj.transform);

        // Setup PauseMenuManager
        SetupPauseMenuManager(hintTextObj, panelObj);

        Debug.Log("Pause Menu UI setup complete! Check the Canvas in the Hierarchy.");
        
        // Check if LoadMenuPanel exists
        bool loadMenuExists = false;
        Transform loadMenuPanel = canvas.transform.Find("LoadMenuPanel");
        if (loadMenuPanel == null)
        {
            // Also check if it's a child of PauseMenuPanel
            loadMenuPanel = panelObj.transform.Find("LoadMenuPanel");
        }
        loadMenuExists = loadMenuPanel != null;
        
        string dialogMessage = "Pause Menu UI has been created successfully!\n\n" +
            "Next steps:\n" +
            "1. Check the Canvas in Hierarchy\n" +
            "2. Verify PauseMenuManager component has all references assigned\n" +
            "3. Adjust button positions/sizes as needed\n";
        
        if (!loadMenuExists)
        {
            dialogMessage += "4. Run 'Tools -> Setup Load Menu UI' to create the Load Menu Panel\n";
            dialogMessage += "5. Test by pressing ESC in Play mode";
        }
        else
        {
            dialogMessage += "4. Test by pressing ESC in Play mode";
        }
        
        EditorUtility.DisplayDialog("Pause Menu Setup", dialogMessage, "OK");
    }

    private static GameObject CreateHintText(Transform parent)
    {
        GameObject hintObj = new GameObject("PauseHintText");
        hintObj.transform.SetParent(parent, false);

        RectTransform rectTransform = hintObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(1f, 1f);
        rectTransform.anchoredPosition = new Vector2(-20f, -20f);
        rectTransform.sizeDelta = new Vector2(500f, 60f);

        TextMeshProUGUI text = hintObj.AddComponent<TextMeshProUGUI>();
        text.text = "Press ESC to pause";
        text.fontSize = 48;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopRight;

        return hintObj;
    }

    private static GameObject CreatePausePanel(Transform parent)
    {
        GameObject panelObj = new GameObject("PauseMenuPanel");
        panelObj.transform.SetParent(parent, false);

        // RectTransform - full screen
        RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // Image - semi-transparent background
        Image image = panelObj.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 200f / 255f);

        // Vertical Layout Group with better spacing
        VerticalLayoutGroup layoutGroup = panelObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(50, 50, 80, 80);
        layoutGroup.spacing = 15f; // Spacing between buttons
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Content Size Fitter (optional, helps with layout)
        ContentSizeFitter fitter = panelObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Set inactive by default
        panelObj.SetActive(false);

        return panelObj;
    }

    private static void CreateMenuButtons(Transform panelParent)
    {
        string[] buttonNames = { "ResumeButton", "SaveGameButton", "LoadGameButton", "SettingsButton", "MainMenuButton", "ExitButton", "SkipDayButton", "RestartDayButton" };
        // NOTE: Exit is repurposed in-game as a Debug Menu entry.
        string[] buttonTexts = { "Resume", "Save Game", "Load Game", "Settings", "Main Menu", "Debug Menu", "Skip Day", "Restart Day" };

        for (int i = 0; i < buttonNames.Length; i++)
        {
            // Check if button already exists
            Transform existingButton = panelParent.Find(buttonNames[i]);
            if (existingButton != null)
            {
                Debug.Log($"Button {buttonNames[i]} already exists, skipping");
                continue;
            }

            GameObject buttonObj = new GameObject(buttonNames[i]);
            buttonObj.transform.SetParent(panelParent, false);

            // RectTransform - larger buttons for 60px font
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400f, 80f);

            // Image (button background)
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            // Button component
            Button button = buttonObj.AddComponent<Button>();

            // Set button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.35f, 1f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.2f, 1f);
            colors.selectedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            button.colors = colors;

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = buttonTexts[i];
            text.fontSize = 60;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            Debug.Log($"Created button: {buttonNames[i]}");
        }
    }

    private static void SetupPauseMenuManager(GameObject hintTextObj, GameObject panelObj)
    {
        // Find existing PauseMenuManager
        PauseMenuManager manager = FindFirstObjectByType<PauseMenuManager>();
        if (manager == null)
        {
            // Create new GameObject for PauseMenuManager
            GameObject managerObj = new GameObject("PauseMenuManager");
            manager = managerObj.AddComponent<PauseMenuManager>();
            Debug.Log("Created PauseMenuManager GameObject");
        }

        // Assign references using SerializedObject for proper undo support
        SerializedObject serializedManager = new SerializedObject(manager);
        
        // Assign hint text
        SerializedProperty hintProp = serializedManager.FindProperty("pauseHintText");
        if (hintProp != null)
        {
            TextMeshProUGUI hintText = hintTextObj.GetComponent<TextMeshProUGUI>();
            if (hintText != null)
            {
                hintProp.objectReferenceValue = hintText;
            }
        }

        // Assign panel
        SerializedProperty panelProp = serializedManager.FindProperty("pauseMenuPanel");
        if (panelProp != null)
        {
            panelProp.objectReferenceValue = panelObj;
        }

        // Assign buttons
        AssignButtonReference(serializedManager, "resumeButton", "ResumeButton", panelObj.transform);
        AssignButtonReference(serializedManager, "saveGameButton", "SaveGameButton", panelObj.transform);
        AssignButtonReference(serializedManager, "loadGameButton", "LoadGameButton", panelObj.transform);
        AssignButtonReference(serializedManager, "settingsButton", "SettingsButton", panelObj.transform);
        AssignButtonReference(serializedManager, "mainMenuButton", "MainMenuButton", panelObj.transform);
        AssignButtonReference(serializedManager, "exitButton", "ExitButton", panelObj.transform);
        AssignButtonReference(serializedManager, "skipDayButton", "SkipDayButton", panelObj.transform);
        AssignButtonReference(serializedManager, "restartDayButton", "RestartDayButton", panelObj.transform);

        serializedManager.ApplyModifiedProperties();

        // Auto-find SaveLoadManager and DialogueRunner (they'll be found at runtime)
        Debug.Log("PauseMenuManager references assigned. SaveLoadManager and DialogueRunner will be auto-found at runtime.");
    }

    private static void AssignButtonReference(SerializedObject serializedManager, string propertyName, string buttonName, Transform panelParent)
    {
        SerializedProperty prop = serializedManager.FindProperty(propertyName);
        if (prop != null)
        {
            Transform buttonTransform = panelParent.Find(buttonName);
            if (buttonTransform != null)
            {
                Button button = buttonTransform.GetComponent<Button>();
                if (button != null)
                {
                    prop.objectReferenceValue = button;
                }
            }
        }
    }

    [MenuItem("Tools/Add Settings Button to Pause Menu")]
    public static void AddSettingsButton()
    {
        // Find PauseMenuManager
        PauseMenuManager pauseMenu = FindFirstObjectByType<PauseMenuManager>();
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

        // Check if Settings button already exists
        Transform existingButton = panelTransform.Find("SettingsButton");
        if (existingButton != null)
        {
            // Button exists, just make sure it's assigned
            Button btn = existingButton.GetComponent<Button>();
            if (btn != null && pauseMenu.settingsButton == null)
            {
                pauseMenu.settingsButton = btn;
                EditorUtility.SetDirty(pauseMenu);
                EditorUtility.DisplayDialog("Fixed", "Settings button was already present. Assigned it to PauseMenuManager.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Already Exists", "SettingsButton already exists in the pause menu.", "OK");
            }
            Selection.activeGameObject = existingButton.gameObject;
            return;
        }

        // Find the position after LoadGameButton
        Transform loadGameButton = panelTransform.Find("LoadGameButton");
        int insertIndex = 3; // Default position
        if (loadGameButton != null)
        {
            insertIndex = loadGameButton.GetSiblingIndex() + 1;
        }

        // Create Settings button - larger size to match other buttons
        GameObject buttonObj = new GameObject("SettingsButton");
        Undo.RegisterCreatedObjectUndo(buttonObj, "Create Settings Button");
        buttonObj.transform.SetParent(panelTransform, false);
        buttonObj.transform.SetSiblingIndex(insertIndex);

        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400f, 80f); // Larger button for 60px font

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        colors.selectedColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        button.colors = colors;

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Settings";
        text.fontSize = 60;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        // Assign to PauseMenuManager
        pauseMenu.settingsButton = button;
        EditorUtility.SetDirty(pauseMenu);
        EditorUtility.SetDirty(pauseMenu.pauseMenuPanel);

        Selection.activeGameObject = buttonObj;

        EditorUtility.DisplayDialog("Success",
            "Settings button added to pause menu!\n\n" +
            "Next steps:\n" +
            "1. Run 'Tools > Setup Settings Panel in Pause Menu'\n" +
            "2. Run 'Tools > Setup SettingsManager in Scene'\n" +
            "3. Save the scene", "OK");
    }
}

