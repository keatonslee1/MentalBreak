using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// Creates a project-owned Dialogue System prefab (based on Yarn Spinner's package prefab)
/// and injects an OverlayUIRoot under its Canvas so the overlay hierarchy is visible/editable
/// in edit mode (not only during Play Mode).
/// </summary>
public static class CreateProjectDialogueSystemPrefab
{
    private const string YarnDialogueSystemPrefabPath = "Packages/dev.yarnspinner.unity/Prefabs/Dialogue System.prefab";
    private const string OutputPrefabPath = "Assets/Prefabs/Dialogue/Dialogue System (Project).prefab";

    private const string DialogueSystemRootName = "Dialogue System";
    private const string OverlayRootName = "OverlayUIRoot";

    [MenuItem("Tools/Yarn Spinner/Create/Install Project Dialogue System (with OverlayUIRoot)")]
    public static void CreateAndInstall()
    {
        GameObject projectPrefab = CreateOrUpdateProjectPrefab();
        if (projectPrefab == null)
        {
            return;
        }

        // Optionally replace the current scene's Dialogue System instance.
        GameObject existing = GameObject.Find(DialogueSystemRootName);
        if (existing == null)
        {
            Debug.Log($"CreateProjectDialogueSystemPrefab: No '{DialogueSystemRootName}' found in the current scene; prefab was created/updated at '{OutputPrefabPath}'.");
            return;
        }

        bool doReplace = EditorUtility.DisplayDialog(
            "Replace Dialogue System instance?",
            $"Found a '{DialogueSystemRootName}' in the current scene.\n\nReplace it with the project prefab?\n\nThis makes the hierarchy (including OverlayUIRoot) visible/editable outside Play Mode.",
            "Replace",
            "Cancel");

        if (!doReplace)
        {
            return;
        }

        ReplaceSceneDialogueSystem(existing, projectPrefab);
    }

    [MenuItem("Tools/Yarn Spinner/Create/Update Project Dialogue System Prefab (with OverlayUIRoot)")]
    public static GameObject CreateOrUpdateProjectPrefab()
    {
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(YarnDialogueSystemPrefabPath);
        if (basePrefab == null)
        {
            Debug.LogError($"CreateProjectDialogueSystemPrefab: Could not load Yarn Dialogue System prefab at '{YarnDialogueSystemPrefabPath}'. Is the package installed?");
            return null;
        }

        EnsureParentFolderExists(OutputPrefabPath);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        if (instance == null)
        {
            Debug.LogError("CreateProjectDialogueSystemPrefab: Failed to instantiate Yarn Dialogue System prefab.");
            return null;
        }

        try
        {
            InjectOverlayRoot(instance);

            // Wire scene/prefab UI references so replacement doesn't lose inspector overrides.
            DialogueSystemUIAutoWire.AutoWireOnPrefabInstanceRoot(instance);

            // Ensure SaveLoadManager exists on the project Dialogue System prefab (it exists in the package prefab,
            // but replacement can drop it, causing checkpoint/save UI to fail finding it).
            EnsureSaveLoadManager(instance);

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(instance, OutputPrefabPath);
            if (saved == null)
            {
                Debug.LogError($"CreateProjectDialogueSystemPrefab: Failed to save project prefab at '{OutputPrefabPath}'.");
                return null;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CreateProjectDialogueSystemPrefab: Created/updated project Dialogue System prefab at '{OutputPrefabPath}'.");
            return saved;
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }
    }

    private static void ReplaceSceneDialogueSystem(GameObject existingInstance, GameObject projectPrefab)
    {
        Transform oldTransform = existingInstance.transform;
        Transform oldParent = oldTransform.parent;
        int oldSiblingIndex = oldTransform.GetSiblingIndex();

        // Instantiate the project prefab into the current scene.
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject newInstance = (GameObject)PrefabUtility.InstantiatePrefab(projectPrefab, activeScene);
        if (newInstance == null)
        {
            Debug.LogError("CreateProjectDialogueSystemPrefab: Failed to instantiate project Dialogue System prefab into the scene.");
            return;
        }
        newInstance.name = DialogueSystemRootName;
        newInstance.transform.SetParent(oldParent, worldPositionStays: false);
        newInstance.transform.SetSiblingIndex(oldSiblingIndex);
        newInstance.transform.localPosition = oldTransform.localPosition;
        newInstance.transform.localRotation = oldTransform.localRotation;
        newInstance.transform.localScale = oldTransform.localScale;

        Object.DestroyImmediate(existingInstance);

        EditorSceneManager.MarkSceneDirty(activeScene);

        // If we're on MVPScene, run the existing configurator to restore assignments.
        if (activeScene.name == "MVPScene")
        {
            ConfigureNewDialogueSystem.Configure();
        }
        else
        {
            Debug.LogWarning("CreateProjectDialogueSystemPrefab: Scene is not MVPScene, so ConfigureNewDialogueSystem was not run automatically.");
        }

        Debug.Log("CreateProjectDialogueSystemPrefab: Replaced scene Dialogue System instance with project prefab.");
    }

    private static void EnsureSaveLoadManager(GameObject dialogueSystemRoot)
    {
        if (dialogueSystemRoot == null) return;

        DialogueRunner runner = dialogueSystemRoot.GetComponent<DialogueRunner>();
        SaveLoadManager slm = dialogueSystemRoot.GetComponent<SaveLoadManager>();
        if (slm == null)
        {
            slm = dialogueSystemRoot.AddComponent<SaveLoadManager>();
            Debug.Log("CreateProjectDialogueSystemPrefab: Added SaveLoadManager to project Dialogue System prefab.");
        }

        // Prefer explicit wiring for determinism; SaveLoadManager can still auto-find at runtime.
        var so = new SerializedObject(slm);
        var runnerProp = so.FindProperty("dialogueRunner");
        if (runnerProp != null && runnerProp.objectReferenceValue == null && runner != null)
        {
            runnerProp.objectReferenceValue = runner;
        }
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(slm);
    }

    private static void InjectOverlayRoot(GameObject dialogueSystemRoot)
    {
        Canvas dialogueCanvas = FindDialogueCanvas(dialogueSystemRoot);
        if (dialogueCanvas == null)
        {
            Debug.LogError("CreateProjectDialogueSystemPrefab: Could not find a Canvas under the Dialogue System prefab to attach OverlayUIRoot.");
            return;
        }

        Transform existingOverlay = dialogueCanvas.transform.Find(OverlayRootName);
        GameObject overlayRoot = existingOverlay != null ? existingOverlay.gameObject : new GameObject(OverlayRootName);
        overlayRoot.transform.SetParent(dialogueCanvas.transform, false);

        // OverlayUIRoot is a PANEL, not a Canvas. (The dialogue canvas is the only canvas.)
        // Remove any legacy components if present.
        Canvas existingCanvas = overlayRoot.GetComponent<Canvas>();
        if (existingCanvas != null)
        {
            Object.DestroyImmediate(existingCanvas);
        }
        CanvasScaler existingScaler = overlayRoot.GetComponent<CanvasScaler>();
        if (existingScaler != null)
        {
            Object.DestroyImmediate(existingScaler);
        }
        GraphicRaycaster existingRaycaster = overlayRoot.GetComponent<GraphicRaycaster>();
        if (existingRaycaster != null)
        {
            Object.DestroyImmediate(existingRaycaster);
        }

        // Ensure it fills the parent canvas by default (can be edited in prefab).
        RectTransform rt = overlayRoot.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = overlayRoot.AddComponent<RectTransform>();
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // Ensure it renders above other dialogue UI by default.
        overlayRoot.transform.SetAsLastSibling();

        // Optional scaffolding: create common overlay widget roots so they're visible/editable in edit mode.
        InjectOverlayWidgetRoots(overlayRoot);

        // Portrait placeholders: allow you to see/edit portrait rects in edit mode, and provide stable targets
        // for CharacterTalkAnimation (which looks for CharacterSprite_Left/Right by name).
        InjectPortraitPlaceholders(dialogueCanvas.transform);
    }

    private static void InjectOverlayWidgetRoots(GameObject overlayRoot)
    {
        if (overlayRoot == null)
        {
            return;
        }

        // Build version label root (no extra Canvas; the label will live under OverlayUIRoot).
        Transform buildOverlay = overlayRoot.transform.Find("BuildVersionOverlay");
        if (buildOverlay == null)
        {
            GameObject buildGo = new GameObject("BuildVersionOverlay");
            buildGo.transform.SetParent(overlayRoot.transform, false);
            buildGo.AddComponent<RectTransform>();

            GameObject labelGo = new GameObject("BuildLabel");
            labelGo.transform.SetParent(buildGo.transform, false);
            labelGo.AddComponent<RectTransform>();

            Text text = labelGo.AddComponent<Text>();
            text.text = "alpha";
            text.fontSize = 20;
            text.color = new Color(1f, 1f, 1f, 0.8f);
            text.alignment = TextAnchor.LowerLeft;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            if (labelRect == null)
            {
                labelRect = labelGo.AddComponent<RectTransform>();
            }
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.zero;
            labelRect.pivot = Vector2.zero;
            labelRect.anchoredPosition = new Vector2(24f, 24f);
        }

        // Create empty roots for panels that are generated/managed by scripts, so designers can author them in the prefab.
        if (overlayRoot.transform.Find("MetricsPanelRoot") == null)
        {
            GameObject metricsRoot = new GameObject("MetricsPanelRoot");
            metricsRoot.transform.SetParent(overlayRoot.transform, false);
            metricsRoot.AddComponent<RectTransform>();
            if (metricsRoot.GetComponent<MetricsPanelUI>() == null)
            {
                metricsRoot.AddComponent<MetricsPanelUI>();
            }
        }

        if (overlayRoot.transform.Find("LeaderboardPanel") == null)
        {
            GameObject leaderboardPanel = new GameObject("LeaderboardPanel");
            leaderboardPanel.transform.SetParent(overlayRoot.transform, false);
            leaderboardPanel.AddComponent<RectTransform>();
            if (leaderboardPanel.GetComponent<LeaderboardUI>() == null)
            {
                leaderboardPanel.AddComponent<LeaderboardUI>();
            }
        }
    }

    private static void InjectPortraitPlaceholders(Transform dialogueCanvasTransform)
    {
        if (dialogueCanvasTransform == null)
        {
            return;
        }

        EnsurePortraitPlaceholder(dialogueCanvasTransform, "CharacterSprite_Left", anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(0f, 0f), pivot: new Vector2(0f, 0f));
        EnsurePortraitPlaceholder(dialogueCanvasTransform, "CharacterSprite_Right", anchorMin: new Vector2(1f, 0f), anchorMax: new Vector2(1f, 0f), pivot: new Vector2(1f, 0f));
    }

    private static void EnsurePortraitPlaceholder(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = go.AddComponent<RectTransform>();
        }

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = new Vector2(400f, 600f);
        rt.anchoredPosition = new Vector2(pivot.x < 0.5f ? 100f : -100f, 100f);

        Image img = go.GetComponent<Image>();
        if (img == null)
        {
            img = go.AddComponent<Image>();
        }
        img.preserveAspect = true;
        img.raycastTarget = false;

        // Keep enabled so the rect is visible/editable in edit mode; runtime will set sprite + active state via CharacterSpriteManager.
        if (!go.activeSelf)
        {
            go.SetActive(true);
        }
    }

    private static Canvas FindDialogueCanvas(GameObject root)
    {
        // Prefer the standard named Canvas child if present; otherwise fall back to any Canvas.
        Transform canvasByName = root.transform.Find("Canvas");
        if (canvasByName != null)
        {
            Canvas c = canvasByName.GetComponent<Canvas>();
            if (c != null)
            {
                return c;
            }
        }

        Canvas any = root.GetComponentInChildren<Canvas>(true);
        return any;
    }

    private static void EnsureParentFolderExists(string assetPath)
    {
        // assetPath is like "Assets/Prefabs/Dialogue/Foo.prefab"
        string folder = Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
    }
}


