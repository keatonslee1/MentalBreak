using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to set up the Save/Load Panel UI.
/// Usage: Unity Menu -> Tools -> Setup Save/Load Panel
/// </summary>
public class SaveLoadPanelSetup : EditorWindow
{
    [MenuItem("Tools/Setup Save/Load Panel")]
    public static void SetupSaveLoadPanel()
    {
        // Create dedicated canvas to avoid DontDestroyOnLoad issues
        Canvas saveLoadCanvas = null;

        // Look for existing SaveLoadCanvas
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name == "SaveLoadCanvas")
            {
                saveLoadCanvas = c;
                Debug.Log("[SaveLoadPanelSetup] Found existing SaveLoadCanvas");
                break;
            }
        }

        // Create new canvas if not found
        if (saveLoadCanvas == null)
        {
            GameObject canvasObj = new GameObject("SaveLoadCanvas");
            saveLoadCanvas = canvasObj.AddComponent<Canvas>();
            saveLoadCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            saveLoadCanvas.sortingOrder = 5000;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("[SaveLoadPanelSetup] Created SaveLoadCanvas");
        }

        // Ensure EventSystem exists
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[SaveLoadPanelSetup] Created EventSystem");
        }

        // Remove existing panel if present
        Transform existingPanel = saveLoadCanvas.transform.Find("SaveLoadPanel");
        if (existingPanel != null)
        {
            DestroyImmediate(existingPanel.gameObject);
            Debug.Log("[SaveLoadPanelSetup] Removed existing SaveLoadPanel");
        }

        // Create the panel
        GameObject panelObj = CreateSaveLoadPanel(saveLoadCanvas.transform);

        // Add and configure SaveSlotSelectionUI component
        SaveSlotSelectionUI saveSlotUI = panelObj.GetComponent<SaveSlotSelectionUI>();
        if (saveSlotUI == null)
        {
            saveSlotUI = panelObj.AddComponent<SaveSlotSelectionUI>();
        }

        // Wire references
        WireReferences(panelObj, saveSlotUI);

        // Set panel inactive initially
        panelObj.SetActive(false);

        Debug.Log("[SaveLoadPanelSetup] Save/Load Panel setup complete!");

        EditorUtility.DisplayDialog("Save/Load Panel Setup Complete",
            "Save/Load Panel UI has been created!\n\n" +
            "The panel will create its UI dynamically at runtime.\n" +
            "Test with the pause menu Save/Load buttons.",
            "OK");

        Selection.activeGameObject = panelObj;
    }

    private static GameObject CreateSaveLoadPanel(Transform parent)
    {
        GameObject panelObj = new GameObject("SaveLoadPanel");
        panelObj.transform.SetParent(parent, false);

        RectTransform rect = panelObj.AddComponent<RectTransform>();
        // Use percentage-based sizing for responsiveness
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(480, 580); // Slightly smaller for mobile
        rect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

        panelObj.AddComponent<CanvasGroup>();

        return panelObj;
    }

    private static void WireReferences(GameObject panelObj, SaveSlotSelectionUI saveSlotUI)
    {
        // The SaveSlotSelectionUI will create its UI dynamically
        // We just need to set the panel reference
        saveSlotUI.selectionPanel = panelObj;

        // Try to find SaveLoadManager
        SaveLoadManager saveLoadManager = FindFirstObjectByType<SaveLoadManager>();
        if (saveLoadManager != null)
        {
            saveSlotUI.saveLoadManager = saveLoadManager;
            Debug.Log("[SaveLoadPanelSetup] Assigned SaveLoadManager");
        }

        // Try to find PauseMenuManager
        PauseMenuManager pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
        if (pauseMenuManager != null)
        {
            saveSlotUI.pauseMenuManager = pauseMenuManager;
            Debug.Log("[SaveLoadPanelSetup] Assigned PauseMenuManager");
        }

        EditorUtility.SetDirty(saveSlotUI);
    }
}
