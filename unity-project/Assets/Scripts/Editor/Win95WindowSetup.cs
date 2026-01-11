using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using MentalBreak.UI.Win95;
using System.Collections.Generic;

/// <summary>
/// Editor script to set up the Windows 95 window frame in the scene.
/// Run via Tools > Setup Win95 Window Frame
/// </summary>
public class Win95WindowSetup : EditorWindow
{
    [MenuItem("Tools/Setup Win95 Window Frame")]
    public static void SetupWindowFrame()
    {
        // Find the Dialogue System canvas
        Canvas dialogueCanvas = null;
        GameObject dialogueSystem = GameObject.Find("Dialogue System");

        if (dialogueSystem != null)
        {
            dialogueCanvas = dialogueSystem.GetComponentInChildren<Canvas>();
        }

        if (dialogueCanvas == null)
        {
            // Try to find any canvas named "Canvas"
            dialogueCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        }

        if (dialogueCanvas == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find the Dialogue System canvas. Please ensure the Dialogue System is in the scene.",
                "OK");
            return;
        }

        // Check if Win95WindowFrame already exists
        Win95WindowFrame existingFrame = dialogueCanvas.GetComponentInChildren<Win95WindowFrame>();
        if (existingFrame != null)
        {
            if (!EditorUtility.DisplayDialog("Win95 Window Frame Exists",
                "A Win95 Window Frame already exists. Do you want to recreate it?",
                "Recreate", "Cancel"))
            {
                return;
            }

            Undo.DestroyObjectImmediate(existingFrame.gameObject);
        }

        // Create the window frame as a child of the canvas
        GameObject frameObj = new GameObject("Win95WindowFrame");
        Undo.RegisterCreatedObjectUndo(frameObj, "Create Win95 Window Frame");

        frameObj.transform.SetParent(dialogueCanvas.transform, false);
        frameObj.transform.SetAsFirstSibling(); // Put at bottom of hierarchy

        // Set up RectTransform to fill canvas
        RectTransform frameRect = frameObj.AddComponent<RectTransform>();
        frameRect.anchorMin = Vector2.zero;
        frameRect.anchorMax = Vector2.one;
        frameRect.offsetMin = Vector2.zero;
        frameRect.offsetMax = Vector2.zero;

        // Add Win95WindowFrame component
        Win95WindowFrame windowFrame = frameObj.AddComponent<Win95WindowFrame>();

        // Set default title
        SerializedObject so = new SerializedObject(windowFrame);
        so.FindProperty("windowTitle").stringValue = "Bigger Tech Corp. Employee Onboarding System";
        so.ApplyModifiedProperties();

        // Select the new frame
        Selection.activeGameObject = frameObj;

        Debug.Log("Win95 Window Frame created successfully. You may need to reparent existing UI elements to the ContentArea.");

        EditorUtility.DisplayDialog("Success",
            "Win95 Window Frame created!\n\n" +
            "Next steps:\n" +
            "1. Run 'Tools > Generate Win95 UI Assets' to create sprites\n" +
            "2. Run 'Tools > Wire Win95 Menu Bar' to connect menu actions\n" +
            "3. Move existing UI elements into the ContentArea child\n" +
            "4. Test in Play mode",
            "OK");
    }

    [MenuItem("Tools/Force Recreate Win95 Menu Bar")]
    public static void ForceRecreateMenuBar()
    {
        // Find the Win95WindowFrame
        Win95WindowFrame windowFrame = FindFirstObjectByType<Win95WindowFrame>();
        if (windowFrame == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No Win95 Window Frame found. Please run 'Tools > Setup Win95 Window Frame' first.",
                "OK");
            return;
        }

        // Find and destroy the existing MenuBar
        Transform chromeCanvas = windowFrame.transform.Find("ChromeCanvas");
        if (chromeCanvas != null)
        {
            Transform menuBar = chromeCanvas.Find("MenuBar");
            if (menuBar != null)
            {
                // Destroy all children of MenuBar first
                for (int i = menuBar.childCount - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(menuBar.GetChild(i).gameObject);
                }
                Debug.Log("Destroyed MenuBar children");

                // Remove the Win95MenuBar component
                Win95MenuBar menuBarComponent = menuBar.GetComponent<Win95MenuBar>();
                if (menuBarComponent != null)
                {
                    Undo.DestroyObjectImmediate(menuBarComponent);
                    Debug.Log("Destroyed Win95MenuBar component");
                }
            }
        }

        // Reset isInitialized so it will reinitialize
        SerializedObject windowFrameSO = new SerializedObject(windowFrame);
        windowFrameSO.FindProperty("isInitialized").boolValue = false;
        windowFrameSO.ApplyModifiedProperties();

        // Mark scene dirty
        EditorUtility.SetDirty(windowFrame);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Win95 Menu Bar reset. Press Play to see the recreated menu bar with new settings.");

        EditorUtility.DisplayDialog("Success",
            "Win95 Menu Bar has been reset!\n\n" +
            "The menu bar will be recreated fresh when you press Play.\n" +
            "New settings: FontSize=14, Spacing=2, Padding=3",
            "OK");
    }

    [MenuItem("Tools/Wire Win95 Menu Bar")]
    public static void WireMenuBar()
    {
        // Find the Win95WindowFrame
        Win95WindowFrame windowFrame = FindFirstObjectByType<Win95WindowFrame>();
        if (windowFrame == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No Win95 Window Frame found. Please run 'Tools > Setup Win95 Window Frame' first.",
                "OK");
            return;
        }

        // Find PauseMenuManager
        var pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenuManager == null)
        {
            Debug.LogWarning("PauseMenuManager not found. Menu bar events will not be wired.");
        }

        // Create a helper script to wire the events at runtime
        var connector = windowFrame.gameObject.GetComponent<Win95MenuConnector>();
        if (connector == null)
        {
            connector = windowFrame.gameObject.AddComponent<Win95MenuConnector>();
            Undo.RegisterCreatedObjectUndo(connector, "Add Win95 Menu Connector");
        }

        Debug.Log("Win95 Menu Connector added. It will wire events at runtime.");

        EditorUtility.DisplayDialog("Success",
            "Win95 Menu Connector added!\n\n" +
            "The menu bar events will be automatically connected to:\n" +
            "- Save: Opens save dialog\n" +
            "- Load: Opens load dialog\n" +
            "- Sound: Opens settings panel\n" +
            "- Debug: Opens debug menu\n" +
            "- Feedback: Opens feedback form",
            "OK");
    }

    [MenuItem("Tools/Create Win95 Test Panel")]
    public static void CreateTestPanel()
    {
        // Find a canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in scene.", "OK");
            return;
        }

        // Create test panel
        GameObject panelObj = new GameObject("Win95TestPanel");
        Undo.RegisterCreatedObjectUndo(panelObj, "Create Win95 Test Panel");

        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform rect = panelObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(300, 200);

        // Add Win95Panel
        Win95Panel panel = panelObj.AddComponent<Win95Panel>();

        // Add a test button inside
        GameObject btnObj = new GameObject("TestButton");
        btnObj.transform.SetParent(panelObj.transform, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -50);
        btnRect.sizeDelta = new Vector2(100, 30);

        Button btn = btnObj.AddComponent<Button>();
        Win95Button win95Btn = btnObj.AddComponent<Win95Button>();

        Selection.activeGameObject = panelObj;

        Debug.Log("Win95 Test Panel created with a test button inside.");
    }

    [MenuItem("Tools/Reparent UI to Win95 Frame")]
    public static void ReparentUIToWin95Frame()
    {
        // Find Win95WindowFrame
        Win95WindowFrame windowFrame = FindFirstObjectByType<Win95WindowFrame>();
        if (windowFrame == null)
        {
            EditorUtility.DisplayDialog("Error",
                "No Win95 Window Frame found. Please run 'Tools > Setup Win95 Window Frame' first.",
                "OK");
            return;
        }

        // Initialize the frame (creates TitleBar, MenuBar, ContentArea, StatusBar)
        // This is needed because Initialize() normally runs in Awake() which only happens at runtime
        windowFrame.Initialize();

        // Mark as initialized so it won't try to reinitialize at runtime
        SerializedObject windowFrameSO = new SerializedObject(windowFrame);
        windowFrameSO.FindProperty("isInitialized").boolValue = true;
        windowFrameSO.ApplyModifiedProperties();

        // Get ContentArea
        RectTransform contentArea = windowFrame.GetContentArea();
        if (contentArea == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Win95 Window Frame has no ContentArea. The frame may not be initialized properly.",
                "OK");
            return;
        }

        // Get parent canvas transform
        Transform canvasTransform = windowFrame.transform.parent;
        if (canvasTransform == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Win95 Window Frame has no parent Canvas.",
                "OK");
            return;
        }

        // Collect all siblings to reparent (exclude Win95WindowFrame itself)
        List<Transform> toReparent = new List<Transform>();
        for (int i = 0; i < canvasTransform.childCount; i++)
        {
            Transform child = canvasTransform.GetChild(i);
            if (child != windowFrame.transform)
            {
                toReparent.Add(child);
            }
        }

        if (toReparent.Count == 0)
        {
            EditorUtility.DisplayDialog("Info",
                "No UI elements to reparent. All existing UI may already be inside ContentArea.",
                "OK");
            return;
        }

        // Register undo for the canvas
        Undo.RecordObject(canvasTransform.gameObject, "Reparent UI to Win95 Frame");

        // Reparent all siblings to ContentArea
        foreach (Transform t in toReparent)
        {
            Undo.SetTransformParent(t, contentArea, "Reparent UI to Win95 Frame");
        }

        // Mark scene as dirty
        EditorUtility.SetDirty(canvasTransform.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"Reparented {toReparent.Count} UI elements to Win95WindowFrame ContentArea.");

        EditorUtility.DisplayDialog("Success",
            $"Reparented {toReparent.Count} UI elements to ContentArea!\n\n" +
            "The hierarchy is now:\n" +
            "Canvas\n" +
            "  └─ Win95WindowFrame\n" +
            "       ├─ TitleBar\n" +
            "       ├─ MenuBar\n" +
            "       ├─ ContentArea (your UI is now here)\n" +
            "       └─ StatusBar\n\n" +
            "Save the scene (Ctrl+S) and press Play to test.",
            "OK");
    }
}

/// <summary>
/// Runtime connector that wires Win95 menu bar events to game systems.
/// </summary>
public class Win95MenuConnector : MonoBehaviour
{
    private Win95WindowFrame windowFrame;
    private PauseMenuManager pauseMenuManager;

    private void Start()
    {
        windowFrame = GetComponent<Win95WindowFrame>();
        pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();

        WireEvents();
    }

    private void WireEvents()
    {
        if (windowFrame == null || windowFrame.MenuBarComponent == null)
        {
            Debug.LogError("Win95MenuConnector: WindowFrame or MenuBar not found.");
            return;
        }

        var menuBar = windowFrame.MenuBarComponent;

        // Wire Back
        menuBar.OnBackClicked += () =>
        {
            Debug.Log("Back clicked");
            var nav = FindFirstObjectByType<DialogueDebugNavigator>();
            if (nav != null && nav.GetHistoryCount() > 1)
            {
                nav.GoBack();
            }
        };

        // Wire Save
        menuBar.OnSaveClicked += () =>
        {
            Debug.Log("Save clicked");
            if (pauseMenuManager != null)
            {
                // Use reflection or public method to trigger save
                var method = pauseMenuManager.GetType().GetMethod("ShowSavePanel",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                method?.Invoke(pauseMenuManager, null);
            }
        };

        // Wire Load
        menuBar.OnLoadClicked += () =>
        {
            Debug.Log("Load clicked");
            if (pauseMenuManager != null)
            {
                var method = pauseMenuManager.GetType().GetMethod("ShowLoadPanel",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                method?.Invoke(pauseMenuManager, null);
            }
        };

        // Wire Sound (Settings)
        menuBar.OnSoundClicked += () =>
        {
            Debug.Log("Sound/Settings clicked");
            if (pauseMenuManager != null)
            {
                var method = pauseMenuManager.GetType().GetMethod("ShowSettingsPanel",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                method?.Invoke(pauseMenuManager, null);
            }
        };

        // Wire Debug
        menuBar.OnDebugClicked += () =>
        {
            Debug.Log("Debug clicked");
            if (pauseMenuManager != null)
            {
                var method = pauseMenuManager.GetType().GetMethod("ShowDebugMenu",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                method?.Invoke(pauseMenuManager, null);
            }
        };

        // Wire Feedback
        menuBar.OnFeedbackClicked += () =>
        {
            Debug.Log("Feedback clicked");
            var feedbackForm = FindFirstObjectByType<FeedbackForm>();
            if (feedbackForm != null)
            {
                feedbackForm.Open();
            }
        };

        Debug.Log("Win95 Menu events wired successfully.");
    }
}
