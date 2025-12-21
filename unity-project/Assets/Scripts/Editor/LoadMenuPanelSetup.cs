using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// Editor script to set up the LoadMenuPanel with the new save system structure.
/// Run via Tools > Setup Load Menu Panel
/// </summary>
public class LoadMenuPanelSetup : EditorWindow
{
    [MenuItem("Tools/Setup Load Menu Panel")]
    public static void SetupLoadMenuPanel()
    {
        // Find SaveSlotSelectionUI in scene
        SaveSlotSelectionUI saveSlotUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        if (saveSlotUI == null)
        {
            EditorUtility.DisplayDialog("Error", "SaveSlotSelectionUI not found in scene. Make sure the Dialogue System is in the scene.", "OK");
            return;
        }

        // Find or get the selection panel
        GameObject loadMenuPanel = saveSlotUI.selectionPanel;
        if (loadMenuPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "selectionPanel is not assigned in SaveSlotSelectionUI.", "OK");
            return;
        }

        Undo.RecordObject(saveSlotUI, "Setup Load Menu Panel");
        Undo.RecordObject(loadMenuPanel, "Setup Load Menu Panel");

        // Get or add VerticalLayoutGroup
        VerticalLayoutGroup layoutGroup = loadMenuPanel.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = Undo.AddComponent<VerticalLayoutGroup>(loadMenuPanel);
        }
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        // Get or add ContentSizeFitter
        ContentSizeFitter sizeFitter = loadMenuPanel.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = Undo.AddComponent<ContentSizeFitter>(loadMenuPanel);
        }
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create section headers and buttons
        // First, create autosave section
        CreateSectionHeader(loadMenuPanel.transform, "--- AUTOSAVES ---");

        // Create autosave buttons if they don't exist
        if (saveSlotUI.autosave0Button == null)
        {
            saveSlotUI.autosave0Button = CreateSlotButton(loadMenuPanel.transform, "Autosave0Button", "AUTO (NEW)");
        }
        if (saveSlotUI.autosave1Button == null)
        {
            saveSlotUI.autosave1Button = CreateSlotButton(loadMenuPanel.transform, "Autosave1Button", "AUTO (MID)");
        }
        if (saveSlotUI.autosave2Button == null)
        {
            saveSlotUI.autosave2Button = CreateSlotButton(loadMenuPanel.transform, "Autosave2Button", "AUTO (OLD)");
        }

        // Create manual slots section
        CreateSectionHeader(loadMenuPanel.transform, "--- MANUAL SAVES ---");

        // Check if slot buttons exist, create if not
        if (saveSlotUI.slot1Button == null)
        {
            saveSlotUI.slot1Button = CreateSlotButton(loadMenuPanel.transform, "Slot1Button", "SLOT 1");
        }
        if (saveSlotUI.slot2Button == null)
        {
            saveSlotUI.slot2Button = CreateSlotButton(loadMenuPanel.transform, "Slot2Button", "SLOT 2");
        }
        if (saveSlotUI.slot3Button == null)
        {
            saveSlotUI.slot3Button = CreateSlotButton(loadMenuPanel.transform, "Slot3Button", "SLOT 3");
        }
        if (saveSlotUI.slot4Button == null)
        {
            saveSlotUI.slot4Button = CreateSlotButton(loadMenuPanel.transform, "Slot4Button", "SLOT 4");
        }
        if (saveSlotUI.slot5Button == null)
        {
            saveSlotUI.slot5Button = CreateSlotButton(loadMenuPanel.transform, "Slot5Button", "SLOT 5");
        }

        // Create spacer
        CreateSpacer(loadMenuPanel.transform);

        // Create import button
        if (saveSlotUI.importButton == null)
        {
            saveSlotUI.importButton = CreateSlotButton(loadMenuPanel.transform, "ImportButton", "IMPORT FROM CLIPBOARD");
            // Style it differently
            Image importBg = saveSlotUI.importButton.GetComponent<Image>();
            if (importBg != null)
            {
                importBg.color = new Color(0.2f, 0.4f, 0.2f, 1f); // Green tint
            }
        }

        // Create cancel button if not exists
        if (saveSlotUI.cancelButton == null)
        {
            saveSlotUI.cancelButton = CreateSlotButton(loadMenuPanel.transform, "CancelButton", "CANCEL");
            // Style it differently
            Image cancelBg = saveSlotUI.cancelButton.GetComponent<Image>();
            if (cancelBg != null)
            {
                cancelBg.color = new Color(0.4f, 0.2f, 0.2f, 1f); // Red tint
            }
        }

        // Reorder children to ensure proper layout
        ReorderChildren(loadMenuPanel.transform, saveSlotUI);

        // Mark as dirty
        EditorUtility.SetDirty(saveSlotUI);
        EditorUtility.SetDirty(loadMenuPanel);

        Debug.Log("LoadMenuPanel setup complete!");
        EditorUtility.DisplayDialog("Success", "LoadMenuPanel has been set up with:\n- 3 Autosave buttons\n- 5 Manual slot buttons\n- Import button\n- Cancel button\n\nRemember to save the scene!", "OK");
    }

    private static Button CreateSlotButton(Transform parent, string name, string labelText)
    {
        // Check if already exists
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.GetComponent<Button>();
        }

        // Create button GameObject
        GameObject buttonObj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(buttonObj, "Create Button");
        buttonObj.transform.SetParent(parent, false);

        // Add RectTransform
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 40);

        // Add Image (background)
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // Add Button
        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.selectedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        button.colors = colors;

        // Add LayoutElement for sizing
        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 40;
        layoutElement.preferredHeight = 50;

        // Create Text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);

        // Try to use TextMeshPro if available, otherwise use legacy Text
#if USE_TMP
        TMPro.TextMeshProUGUI tmpText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmpText.text = labelText;
        tmpText.fontSize = 14;
        tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        tmpText.color = Color.white;
#else
        Text text = textObj.AddComponent<Text>();
        text.text = labelText;
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        // Try to find a font
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
#endif

        return button;
    }

    private static void CreateSectionHeader(Transform parent, string headerText)
    {
        string name = "Header_" + headerText.Replace(" ", "").Replace("-", "");

        // Check if already exists
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return;
        }

        GameObject headerObj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(headerObj, "Create Header");
        headerObj.transform.SetParent(parent, false);

        RectTransform rectTransform = headerObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 25);

        LayoutElement layoutElement = headerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 25;
        layoutElement.preferredHeight = 25;

#if USE_TMP
        TMPro.TextMeshProUGUI tmpText = headerObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmpText.text = headerText;
        tmpText.fontSize = 12;
        tmpText.alignment = TMPro.TextAlignmentOptions.Center;
        tmpText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
#else
        Text text = headerObj.AddComponent<Text>();
        text.text = headerText;
        text.fontSize = 12;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
#endif
    }

    private static void CreateSpacer(Transform parent)
    {
        string name = "Spacer";

        // Check if already exists
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return;
        }

        GameObject spacerObj = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(spacerObj, "Create Spacer");
        spacerObj.transform.SetParent(parent, false);

        RectTransform rectTransform = spacerObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 10);

        LayoutElement layoutElement = spacerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 10;
        layoutElement.preferredHeight = 10;
        layoutElement.flexibleHeight = 0;
    }

    private static void ReorderChildren(Transform parent, SaveSlotSelectionUI ui)
    {
        // Define the desired order
        string[] desiredOrder = {
            "Header_AUTOSAVES",
            "Autosave0Button",
            "Autosave1Button",
            "Autosave2Button",
            "Header_MANUALSAVES",
            "Slot1Button",
            "Slot2Button",
            "Slot3Button",
            "Slot4Button",
            "Slot5Button",
            "Spacer",
            "ImportButton",
            "CancelButton"
        };

        int index = 0;
        foreach (string childName in desiredOrder)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                child.SetSiblingIndex(index);
                index++;
            }
        }
    }

    [MenuItem("Tools/Wire Save Slot UI References")]
    public static void WireSaveSlotUIReferences()
    {
        SaveSlotSelectionUI saveSlotUI = Object.FindFirstObjectByType<SaveSlotSelectionUI>();
        if (saveSlotUI == null)
        {
            EditorUtility.DisplayDialog("Error", "SaveSlotSelectionUI not found in scene.", "OK");
            return;
        }

        GameObject panel = saveSlotUI.selectionPanel;
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Error", "selectionPanel is not assigned.", "OK");
            return;
        }

        Undo.RecordObject(saveSlotUI, "Wire Save Slot UI References");

        // Find and wire buttons
        Transform t = panel.transform;

        // Autosave buttons
        saveSlotUI.autosave0Button = FindButton(t, "Autosave0Button");
        saveSlotUI.autosave1Button = FindButton(t, "Autosave1Button");
        saveSlotUI.autosave2Button = FindButton(t, "Autosave2Button");

        // Manual slot buttons
        saveSlotUI.slot1Button = FindButton(t, "Slot1Button");
        saveSlotUI.slot2Button = FindButton(t, "Slot2Button");
        saveSlotUI.slot3Button = FindButton(t, "Slot3Button");
        saveSlotUI.slot4Button = FindButton(t, "Slot4Button");
        saveSlotUI.slot5Button = FindButton(t, "Slot5Button");

        // Other buttons
        saveSlotUI.importButton = FindButton(t, "ImportButton");
        saveSlotUI.cancelButton = FindButton(t, "CancelButton");

        EditorUtility.SetDirty(saveSlotUI);

        int wiredCount = 0;
        if (saveSlotUI.autosave0Button != null) wiredCount++;
        if (saveSlotUI.autosave1Button != null) wiredCount++;
        if (saveSlotUI.autosave2Button != null) wiredCount++;
        if (saveSlotUI.slot1Button != null) wiredCount++;
        if (saveSlotUI.slot2Button != null) wiredCount++;
        if (saveSlotUI.slot3Button != null) wiredCount++;
        if (saveSlotUI.slot4Button != null) wiredCount++;
        if (saveSlotUI.slot5Button != null) wiredCount++;
        if (saveSlotUI.importButton != null) wiredCount++;
        if (saveSlotUI.cancelButton != null) wiredCount++;

        Debug.Log($"Wired {wiredCount}/10 button references");
        EditorUtility.DisplayDialog("Done", $"Wired {wiredCount}/10 button references.\n\nRemember to save the scene!", "OK");
    }

    private static Button FindButton(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child.GetComponent<Button>();
        }
        return null;
    }
}
