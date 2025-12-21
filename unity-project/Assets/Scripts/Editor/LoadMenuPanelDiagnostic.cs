using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Text;

/// <summary>
/// Diagnostic script to analyze the current LoadMenuPanel structure
/// and rebuild it properly for the new save system.
/// </summary>
public class LoadMenuPanelDiagnostic : EditorWindow
{
    private static StringBuilder log = new StringBuilder();

    [MenuItem("Tools/Save System/1. Diagnose Load Menu Panel")]
    public static void DiagnosePanel()
    {
        log.Clear();
        log.AppendLine("=== LOAD MENU PANEL DIAGNOSTIC ===\n");

        // Find SaveSlotSelectionUI
        SaveSlotSelectionUI saveSlotUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        if (saveSlotUI == null)
        {
            log.AppendLine("ERROR: SaveSlotSelectionUI not found in scene!");
            ShowLog();
            return;
        }
        log.AppendLine($"Found SaveSlotSelectionUI on: {GetFullPath(saveSlotUI.gameObject)}");

        // Check selectionPanel reference
        if (saveSlotUI.selectionPanel == null)
        {
            log.AppendLine("ERROR: selectionPanel is NULL!");
            ShowLog();
            return;
        }
        log.AppendLine($"selectionPanel: {GetFullPath(saveSlotUI.selectionPanel)}");
        log.AppendLine($"  - Active: {saveSlotUI.selectionPanel.activeSelf}");
        log.AppendLine($"  - ActiveInHierarchy: {saveSlotUI.selectionPanel.activeInHierarchy}");

        // List all children
        Transform panel = saveSlotUI.selectionPanel.transform;
        log.AppendLine($"\nChildren of selectionPanel ({panel.childCount} total):");
        for (int i = 0; i < panel.childCount; i++)
        {
            Transform child = panel.GetChild(i);
            Button btn = child.GetComponent<Button>();
            log.AppendLine($"  [{i}] {child.name} - Active:{child.gameObject.activeSelf}, Button:{btn != null}");
        }

        // Check current button references
        log.AppendLine("\n=== CURRENT BUTTON REFERENCES ===");
        LogButtonRef("slot1Button", saveSlotUI.slot1Button);
        LogButtonRef("slot2Button", saveSlotUI.slot2Button);
        LogButtonRef("slot3Button", saveSlotUI.slot3Button);
        LogButtonRef("slot4Button", saveSlotUI.slot4Button);
        LogButtonRef("slot5Button", saveSlotUI.slot5Button);
        LogButtonRef("autosave0Button", saveSlotUI.autosave0Button);
        LogButtonRef("autosave1Button", saveSlotUI.autosave1Button);
        LogButtonRef("autosave2Button", saveSlotUI.autosave2Button);
        LogButtonRef("importButton", saveSlotUI.importButton);
        LogButtonRef("cancelButton", saveSlotUI.cancelButton);

        // Check for buttons by various names
        log.AppendLine("\n=== SEARCHING FOR BUTTONS BY NAME ===");
        string[] searchNames = {
            "Slot1Button", "Slot2Button", "Slot3Button", "Slot4Button", "Slot5Button",
            "Autosave0Button", "Autosave1Button", "Autosave2Button",
            "ImportButton", "CancelButton",
            // Old possible names
            "Button", "SaveButton", "LoadButton", "AutosaveButton",
            "Slot 1", "Slot 2", "Slot 3", "Slot 4", "Slot 5",
            "Cancel", "Back", "Close"
        };

        foreach (string name in searchNames)
        {
            Transform found = panel.Find(name);
            if (found != null)
            {
                log.AppendLine($"  Found: '{name}' -> {GetFullPath(found.gameObject)}");
            }
        }

        // Also search recursively
        log.AppendLine("\n=== ALL BUTTONS IN HIERARCHY ===");
        Button[] allButtons = saveSlotUI.selectionPanel.GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            log.AppendLine($"  {GetFullPath(btn.gameObject)} - Active:{btn.gameObject.activeSelf}");
        }

        ShowLog();
    }

    [MenuItem("Tools/Save System/2. Rebuild Load Menu Panel (DESTRUCTIVE)")]
    public static void RebuildPanel()
    {
        if (!EditorUtility.DisplayDialog("Rebuild Load Menu Panel",
            "This will DELETE all existing children of the LoadMenuPanel and create new buttons.\n\nAre you sure?",
            "Yes, Rebuild", "Cancel"))
        {
            return;
        }

        SaveSlotSelectionUI saveSlotUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        if (saveSlotUI == null || saveSlotUI.selectionPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "SaveSlotSelectionUI or selectionPanel not found!", "OK");
            return;
        }

        GameObject panel = saveSlotUI.selectionPanel;
        Undo.RecordObject(saveSlotUI, "Rebuild Load Menu Panel");
        Undo.RecordObject(panel, "Rebuild Load Menu Panel");

        // Delete all children
        while (panel.transform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(panel.transform.GetChild(0).gameObject);
        }

        // Clear all button references
        saveSlotUI.slot1Button = null;
        saveSlotUI.slot2Button = null;
        saveSlotUI.slot3Button = null;
        saveSlotUI.slot4Button = null;
        saveSlotUI.slot5Button = null;
        saveSlotUI.autosave0Button = null;
        saveSlotUI.autosave1Button = null;
        saveSlotUI.autosave2Button = null;
        saveSlotUI.importButton = null;
        saveSlotUI.cancelButton = null;

        // Setup panel components
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(220, 500);

        // Remove any existing layout components and re-add
        VerticalLayoutGroup existingLayout = panel.GetComponent<VerticalLayoutGroup>();
        if (existingLayout != null) Object.DestroyImmediate(existingLayout);

        ContentSizeFitter existingFitter = panel.GetComponent<ContentSizeFitter>();
        if (existingFitter != null) Object.DestroyImmediate(existingFitter);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 5;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Set panel background
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = panel.AddComponent<Image>();
        }
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        // Create all elements in order
        CreateHeader(panel.transform, "AUTOSAVES (Load Only)");
        saveSlotUI.autosave0Button = CreateButton(panel.transform, "Autosave0Button", "AUTO (NEWEST)\n(Empty)", new Color(0.15f, 0.25f, 0.15f));
        saveSlotUI.autosave1Button = CreateButton(panel.transform, "Autosave1Button", "AUTO (MIDDLE)\n(Empty)", new Color(0.15f, 0.25f, 0.15f));
        saveSlotUI.autosave2Button = CreateButton(panel.transform, "Autosave2Button", "AUTO (OLDEST)\n(Empty)", new Color(0.15f, 0.25f, 0.15f));

        CreateHeader(panel.transform, "MANUAL SAVES");
        saveSlotUI.slot1Button = CreateButton(panel.transform, "Slot1Button", "SLOT 1\n(Empty)", new Color(0.2f, 0.2f, 0.25f));
        saveSlotUI.slot2Button = CreateButton(panel.transform, "Slot2Button", "SLOT 2\n(Empty)", new Color(0.2f, 0.2f, 0.25f));
        saveSlotUI.slot3Button = CreateButton(panel.transform, "Slot3Button", "SLOT 3\n(Empty)", new Color(0.2f, 0.2f, 0.25f));
        saveSlotUI.slot4Button = CreateButton(panel.transform, "Slot4Button", "SLOT 4\n(Empty)", new Color(0.2f, 0.2f, 0.25f));
        saveSlotUI.slot5Button = CreateButton(panel.transform, "Slot5Button", "SLOT 5\n(Empty)", new Color(0.2f, 0.2f, 0.25f));

        CreateSpacer(panel.transform, 15);
        saveSlotUI.importButton = CreateButton(panel.transform, "ImportButton", "IMPORT FROM CLIPBOARD", new Color(0.2f, 0.35f, 0.2f));
        saveSlotUI.cancelButton = CreateButton(panel.transform, "CancelButton", "CANCEL", new Color(0.35f, 0.2f, 0.2f));

        // Mark dirty
        EditorUtility.SetDirty(saveSlotUI);
        EditorUtility.SetDirty(panel);

        // Force layout rebuild
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        Debug.Log("Load Menu Panel rebuilt successfully!");
        EditorUtility.DisplayDialog("Success",
            "Load Menu Panel rebuilt with:\n" +
            "- 3 Autosave buttons\n" +
            "- 5 Manual slot buttons\n" +
            "- Import button\n" +
            "- Cancel button\n\n" +
            "IMPORTANT: Save the scene now!", "OK");
    }

    [MenuItem("Tools/Save System/3. Force Activate Panel (Debug)")]
    public static void ForceActivatePanel()
    {
        SaveSlotSelectionUI saveSlotUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        if (saveSlotUI == null || saveSlotUI.selectionPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "Panel not found!", "OK");
            return;
        }

        // Activate entire hierarchy
        Transform current = saveSlotUI.selectionPanel.transform;
        while (current != null)
        {
            current.gameObject.SetActive(true);
            current = current.parent;
        }

        saveSlotUI.selectionPanel.SetActive(true);

        Selection.activeGameObject = saveSlotUI.selectionPanel;
        SceneView.FrameLastActiveSceneView();

        Debug.Log($"Panel activated and selected: {saveSlotUI.selectionPanel.name}");
    }

    private static Button CreateButton(Transform parent, string name, string text, Color bgColor)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 45);

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.8f;
        colors.selectedColor = bgColor * 1.1f;
        btn.colors = colors;

        LayoutElement layoutElem = btnObj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 45;
        layoutElem.preferredHeight = 45;

        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 2);
        textRect.offsetMax = new Vector2(-5, -2);

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.fontSize = 12;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Undo.RegisterCreatedObjectUndo(btnObj, "Create Button");

        return btn;
    }

    private static void CreateHeader(Transform parent, string text)
    {
        GameObject headerObj = new GameObject("Header_" + text.Replace(" ", ""));
        headerObj.transform.SetParent(parent, false);

        RectTransform rect = headerObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 20);

        LayoutElement layoutElem = headerObj.AddComponent<LayoutElement>();
        layoutElem.minHeight = 20;
        layoutElem.preferredHeight = 20;

        Text textComp = headerObj.AddComponent<Text>();
        textComp.text = text;
        textComp.fontSize = 11;
        textComp.fontStyle = FontStyle.Bold;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = new Color(0.8f, 0.8f, 0.6f);
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Undo.RegisterCreatedObjectUndo(headerObj, "Create Header");
    }

    private static void CreateSpacer(Transform parent, float height)
    {
        GameObject spacerObj = new GameObject("Spacer");
        spacerObj.transform.SetParent(parent, false);

        RectTransform rect = spacerObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, height);

        LayoutElement layoutElem = spacerObj.AddComponent<LayoutElement>();
        layoutElem.minHeight = height;
        layoutElem.preferredHeight = height;

        Undo.RegisterCreatedObjectUndo(spacerObj, "Create Spacer");
    }

    private static void LogButtonRef(string name, Button btn)
    {
        if (btn == null)
        {
            log.AppendLine($"  {name}: NULL");
        }
        else
        {
            log.AppendLine($"  {name}: {GetFullPath(btn.gameObject)} - Active:{btn.gameObject.activeSelf}");
        }
    }

    private static string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private static void ShowLog()
    {
        Debug.Log(log.ToString());
        EditorUtility.DisplayDialog("Diagnostic Results", log.ToString(), "OK");
    }
}
