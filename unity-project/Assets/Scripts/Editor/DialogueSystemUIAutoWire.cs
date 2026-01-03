using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

/// <summary>
/// Editor-only utility to (re)wire Dialogue System UI references after prefab replacement.
/// This avoids relying on scene-only overrides that get lost when a prefab instance is replaced.
/// </summary>
public static class DialogueSystemUIAutoWire
{
    private const string DialogueSystemName = "Dialogue System";
    private const string CanvasName = "Canvas";

    private const string PauseMenuPanelName = "PauseMenuPanel";
    private const string PauseHintTextName = "PauseHintText";

    private const string LoadMenuPanelName = "LoadMenuPanel";

    private const string FeedbackFormPanelName = "Feedback Form";
    private const string OpenFeedbackFormButtonName = "OpenFeedbackFormButton";

    public static void AutoWireOnSceneInstance(GameObject dialogueSystemRoot)
    {
        if (dialogueSystemRoot == null)
        {
            Debug.LogError("DialogueSystemUIAutoWire: dialogueSystemRoot is null.");
            return;
        }

        Transform canvas = dialogueSystemRoot.transform.Find(CanvasName);
        if (canvas == null)
        {
            Debug.LogError($"DialogueSystemUIAutoWire: Could not find '{CanvasName}' under '{dialogueSystemRoot.name}'.");
            return;
        }

        AutoWire(dialogueSystemRoot, canvas);
    }

    public static void AutoWireOnPrefabInstanceRoot(GameObject dialogueSystemRoot)
    {
        // Same logic; separate method name to make call sites self-documenting.
        AutoWireOnSceneInstance(dialogueSystemRoot);
    }

    private static void AutoWire(GameObject dialogueSystemRoot, Transform canvasTransform)
    {
        AutoWirePauseMenu(dialogueSystemRoot, canvasTransform);
        AutoWireSaveSlotSelection(dialogueSystemRoot, canvasTransform);
        AutoWireFeedbackForm(dialogueSystemRoot, canvasTransform);
    }

    private static void AutoWirePauseMenu(GameObject dialogueSystemRoot, Transform canvasTransform)
    {
        // Prefer a PauseMenuManager that belongs to this Dialogue System instance/prefab.
        PauseMenuManager pauseMenu = dialogueSystemRoot.GetComponentInChildren<PauseMenuManager>(true);
        if (pauseMenu == null)
        {
            // As a fallback for scenes that keep the manager elsewhere, try a global lookup.
            // (Still safe: we only assign refs if they are currently null.)
            pauseMenu = Object.FindFirstObjectByType<PauseMenuManager>();
        }
        if (pauseMenu == null)
        {
            // Create one under the Dialogue System root to keep things tidy and self-contained.
            pauseMenu = dialogueSystemRoot.AddComponent<PauseMenuManager>();
            Debug.Log("DialogueSystemUIAutoWire: Added PauseMenuManager to Dialogue System root.");
        }

        Transform pauseMenuPanelTf = FindByNameOrPath(canvasTransform, PauseMenuPanelName, $"{PauseMenuPanelName}");
        Transform pauseHintTextTf = FindByNameOrPath(canvasTransform, PauseHintTextName, $"{PauseHintTextName}");

        SerializedObject so = new SerializedObject(pauseMenu);

        AssignIfNull(so, "pauseMenuPanel", pauseMenuPanelTf != null ? pauseMenuPanelTf.gameObject : null);

        // pauseHintText is a Component (TMP_Text or Text).
        if (so.FindProperty("pauseHintText") != null && so.FindProperty("pauseHintText").objectReferenceValue == null && pauseHintTextTf != null)
        {
            var tmp = pauseHintTextTf.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                so.FindProperty("pauseHintText").objectReferenceValue = tmp;
            }
            else
            {
                var uText = pauseHintTextTf.GetComponent<Text>();
                if (uText != null) so.FindProperty("pauseHintText").objectReferenceValue = uText;
            }
        }

        // Buttons live under PauseMenuPanel.
        if (pauseMenuPanelTf != null)
        {
            AssignButtonIfNull(so, "resumeButton", pauseMenuPanelTf, "ResumeButton");
            AssignButtonIfNull(so, "saveGameButton", pauseMenuPanelTf, "SaveGameButton");
            AssignButtonIfNull(so, "loadGameButton", pauseMenuPanelTf, "LoadGameButton");
            AssignButtonIfNull(so, "mainMenuButton", pauseMenuPanelTf, "MainMenuButton");
            AssignButtonIfNull(so, "exitButton", pauseMenuPanelTf, "ExitButton");
            AssignButtonIfNull(so, "skipDayButton", pauseMenuPanelTf, "SkipDayButton");
            AssignButtonIfNull(so, "restartDayButton", pauseMenuPanelTf, "RestartDayButton");
        }

        // Also wire SaveSlotSelectionUI reference on PauseMenuManager if present.
        SaveSlotSelectionUI selectionUI = dialogueSystemRoot.GetComponentInChildren<SaveSlotSelectionUI>(true);
        if (selectionUI == null)
        {
            selectionUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        }
        if (selectionUI != null)
        {
            AssignIfNull(so, "saveSlotSelectionUI", selectionUI);
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(pauseMenu);
    }

    private static void AutoWireSaveSlotSelection(GameObject dialogueSystemRoot, Transform canvasTransform)
    {
        // SaveSlotSelectionUI now creates its UI dynamically at runtime.
        // We only need to wire the selectionPanel reference if it exists.
        SaveSlotSelectionUI selectionUI = dialogueSystemRoot.GetComponentInChildren<SaveSlotSelectionUI>(true);
        if (selectionUI == null)
        {
            selectionUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        }

        if (selectionUI == null)
        {
            // No SaveSlotSelectionUI found - this is fine, it may be set up via Tools > Setup Save/Load Panel
            return;
        }

        // Find the panel - check SaveLoadPanel first (new name), then LoadMenuPanel (legacy)
        Transform panelTf =
            canvasTransform.Find("SaveLoadPanel") ??
            FindDeepChild(canvasTransform, "SaveLoadPanel") ??
            canvasTransform.Find(LoadMenuPanelName) ??
            FindDeepChild(canvasTransform, LoadMenuPanelName);

        if (panelTf == null)
        {
            // Panel not found - the new SaveSlotSelectionUI creates UI dynamically,
            // so the selectionPanel may be the component's own GameObject
            if (selectionUI.selectionPanel == null)
            {
                Debug.LogWarning("DialogueSystemUIAutoWire: SaveSlotSelectionUI.selectionPanel not set. Run Tools > Setup Save/Load Panel to create the UI.");
            }
            return;
        }

        SerializedObject so = new SerializedObject(selectionUI);
        AssignIfNull(so, "selectionPanel", panelTf.gameObject);
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(selectionUI);

        Debug.Log($"DialogueSystemUIAutoWire: Wired SaveSlotSelectionUI.selectionPanel to '{panelTf.name}'.", selectionUI);
    }

    private static void AutoWireFeedbackForm(GameObject dialogueSystemRoot, Transform canvasTransform)
    {
        // FeedbackForm component is expected to live on the Canvas in your setup.
        FeedbackForm feedback = canvasTransform.GetComponent<FeedbackForm>();
        if (feedback == null)
        {
            // Fallback: search anywhere under the dialogue system.
            feedback = dialogueSystemRoot.GetComponentInChildren<FeedbackForm>(true);
        }

        if (feedback == null)
        {
            Debug.LogWarning("DialogueSystemUIAutoWire: FeedbackForm component not found; skipping wiring.");
            return;
        }

        Transform feedbackPanelTf = FindDeepChild(canvasTransform, FeedbackFormPanelName);
        Transform openButtonTf = FindDeepChild(canvasTransform, OpenFeedbackFormButtonName);

        Transform cancelTf = feedbackPanelTf != null ? FindDeepChild(feedbackPanelTf, "CancelButton") : null;
        Transform submitTf = feedbackPanelTf != null ? FindDeepChild(feedbackPanelTf, "SubmitButton") : null;

        // FeedbackForm uses TMP_InputField unconditionally in runtime code, so always wire it here too.
        TMP_InputField input = feedbackPanelTf != null ? feedbackPanelTf.GetComponentInChildren<TMP_InputField>(true) : null;

        SerializedObject so = new SerializedObject(feedback);
        AssignIfNull(so, "feedbackForm", feedbackPanelTf != null ? feedbackPanelTf.gameObject : null);
        AssignIfNull(so, "openFeedbackFormButton", openButtonTf != null ? openButtonTf.GetComponent<Button>() : null);
        AssignIfNull(so, "cancelButton", cancelTf != null ? cancelTf.GetComponent<Button>() : null);
        AssignIfNull(so, "submitButton", submitTf != null ? submitTf.GetComponent<Button>() : null);
        AssignIfNull(so, "feedbackText", input);

        if (so.FindProperty("feedbackText") != null && so.FindProperty("feedbackText").objectReferenceValue == null)
        {
            Debug.LogWarning("DialogueSystemUIAutoWire: FeedbackForm.feedbackText is still null after wiring. Ensure a TMP_InputField exists under 'Feedback Form'.", feedback);
        }
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(feedback);
    }

    private static void AssignIfNull(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            return;
        }

        if (prop.objectReferenceValue == null && value != null)
        {
            prop.objectReferenceValue = value;
        }
    }

    private static void AssignButtonIfNull(SerializedObject so, string propertyName, Transform parent, string childName)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
        {
            return;
        }

        if (prop.objectReferenceValue != null)
        {
            return;
        }

        Transform buttonTf = parent.Find(childName) ?? FindDeepChild(parent, childName);
        if (buttonTf == null)
        {
            return;
        }

        Button btn = buttonTf.GetComponent<Button>();
        if (btn != null)
        {
            prop.objectReferenceValue = btn;
        }
    }

    private static Transform FindByNameOrPath(Transform root, string name, string relativePath)
    {
        Transform byPath = root.Find(relativePath);
        if (byPath != null)
        {
            return byPath;
        }

        return FindDeepChild(root, name);
    }

    private static Transform FindDeepChild(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            Transform found = FindDeepChild(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}


