using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Manages character sprite display based on Yarn node tags.
/// Displays up to 2 character sprites (bottom left priority, then bottom right).
/// Sprites render above background, below dialogue/choice box.
/// </summary>
public class CharacterSpriteManager : MonoBehaviour
{
    [Header("Character Sprites")]
    [Tooltip("Assign character sprites to their tags here (optional - will auto-load from Graphics/Characters if empty)")]
    public List<CharacterSpriteEntry> characterSprites = new List<CharacterSpriteEntry>();
    
    [Header("Auto-Load Settings")]
    [Tooltip("Path to character sprites folder (relative to Assets/)")]
    public string characterFolderPath = "Graphics/Characters";
    
    [Header("References")]
    [Tooltip("DialogueRunner reference (auto-found if null)")]
    public DialogueRunner dialogueRunner;
    
    [Header("UI Setup")]
    [Tooltip("Canvas to add character sprites to (auto-found if null)")]
    public Canvas targetCanvas;
    
    [Tooltip("Z-depth offset for character sprites (higher = closer to camera)")]
    public int spriteSortOrder = 10;
    
    [Tooltip("Character sprite size (width, height)")]
    public Vector2 spriteSize = new Vector2(200f, 300f);
    
    [Tooltip("Bottom-left position offset (x, y from bottom-left corner)")]
    public Vector2 bottomLeftOffset = new Vector2(20f, 250f);
    
    [Tooltip("Bottom-right position offset (x, y from bottom-right corner)")]
    public Vector2 bottomRightOffset = new Vector2(-20f, 250f);

    [Header("Portrait Frame")]
    [Tooltip("Background color for the portrait frame (translucent grey).")]
    public Color portraitFrameColor = new Color(0.25f, 0.25f, 0.25f, 0.40f);

    [Tooltip("Extra height above the portrait to ensure the head never clips at the top of the breath.")]
    public float portraitFrameHeadroom = 12f;

    [Header("Portrait Breathing")]
    [Tooltip("Seconds per full breath cycle (up + down).")]
    public float breathPeriodSeconds = 5f;

    [Tooltip("Vertical breathing amplitude in UI pixels.")]
    public float breathAmplitudePixels = 6f;
    
    // Cache dictionary for fast lookup (now supports multi-frame portraits)
    private Dictionary<string, CharacterPortraitData> portraitDataDictionary;
    
    // Character portrait frame GameObjects (containers with mask + background)
    private GameObject leftSpriteObject;
    private GameObject rightSpriteObject;

    // Child images (actual portraits) inside the frames - LAYERED STRUCTURE
    // Base layer: shows static portrait or talking frames
    private Image leftBaseImage;
    private Image rightBaseImage;
    private RectTransform leftBaseRect;
    private RectTransform rightBaseRect;

    // Talking overlay layer: shows talking frames
    private Image leftTalkingImage;
    private Image rightTalkingImage;
    private RectTransform leftTalkingRect;
    private RectTransform rightTalkingRect;

    // Eyes overlay layer: shows eye blink frames
    private Image leftEyesImage;
    private Image rightEyesImage;
    private RectTransform leftEyesRect;
    private RectTransform rightEyesRect;

    // Legacy references (for backwards compatibility, point to base layer)
    private Image leftPortraitImage => leftBaseImage;
    private Image rightPortraitImage => rightBaseImage;
    private RectTransform leftPortraitRect => leftBaseRect;
    private RectTransform rightPortraitRect => rightBaseRect;

    // Portrait animators (talking + blinking)
    private CharacterPortraitAnimator leftAnimator;
    private CharacterPortraitAnimator rightAnimator;

    // Track currently displayed characters
    private string currentLeftCharacterTag = null;
    private string currentRightCharacterTag = null;

    // Breathing animation driver
    private CharacterTalkAnimation talkAnimation;

    // Track current background for Alice auto-add logic
    private string currentBackground = "";
    
    // Reference to BackgroundCommandHandler to track background changes
    private BackgroundCommandHandler backgroundHandler;
    
    // Character tag to sprite name mapping
    private Dictionary<string, string> characterTagToSpriteName = new Dictionary<string, string>
    {
        { "char_Alice", "alice" },
        { "char_Supervisor", "supervisor" },
        { "char_Timmy", "timmy" },
        { "char_BTC", "btc" },
        { "char_Ari", "ari" },
        { "char_Charlotte", "charlotte" },
        { "char_Dina", "dina" },
        { "char_Noam", "noam" },
        { "char_Max", "max" },
        { "char_Player", null }, // Player doesn't have a sprite
        { "char_DarkFigure", null }, // Dark Figure doesn't have a sprite
    };
    
    [System.Serializable]
    public class CharacterSpriteEntry
    {
        public string characterTag; // e.g., "char_Alice"
        public Sprite sprite;
    }
    
    void Awake()
    {
        BuildDictionary();
        FindReferences();
        SetupCharacterSprites();
        
        // Subscribe to node start events
        if (dialogueRunner != null)
        {
            dialogueRunner.onNodeStart.AddListener(OnNodeStarted);
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (dialogueRunner != null)
        {
            dialogueRunner.onNodeStart.RemoveListener(OnNodeStarted);
        }
    }
    
    void FindReferences()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        }
        
        if (targetCanvas == null)
        {
            targetCanvas = FindDialogueCanvas();
        }
        
        // Find BackgroundCommandHandler to track background changes
        if (backgroundHandler == null)
        {
            backgroundHandler = FindAnyObjectByType<BackgroundCommandHandler>();
        }
    }
    
    Canvas FindDialogueCanvas()
    {
        // PRIORITY 1: Find Canvas in DontDestroyOnLoad (this is where dialogue system lives)
        GameObject dontDestroy = GameObject.Find("DontDestroyOnLoad");
        if (dontDestroy != null)
        {
            Canvas canvas = dontDestroy.GetComponentInChildren<Canvas>(true); // Include inactive
            if (canvas != null)
            {
                Debug.Log($"CharacterSpriteManager: Found Canvas in DontDestroyOnLoad: '{canvas.name}' on GameObject '{canvas.gameObject.name}' (InstanceID: {canvas.GetInstanceID()})");
                return canvas;
            }
        }
        
        // PRIORITY 2: Find via LinePresenter (but verify it's in DontDestroyOnLoad or dialogue system)
        LinePresenter linePresenter = FindAnyObjectByType<LinePresenter>();
        if (linePresenter != null)
        {
            Canvas canvas = linePresenter.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Verify this Canvas is in DontDestroyOnLoad hierarchy
                Transform current = canvas.transform;
                while (current != null && current.parent != null)
                {
                    if (current.parent.name == "DontDestroyOnLoad" || current.parent.name.Contains("Dialogue"))
                    {
                        Debug.Log($"CharacterSpriteManager: Found Canvas via LinePresenter in dialogue system: '{canvas.name}' on GameObject '{canvas.gameObject.name}' (InstanceID: {canvas.GetInstanceID()})");
                        return canvas;
                    }
                    current = current.parent;
                }
                Debug.LogWarning($"CharacterSpriteManager: LinePresenter Canvas '{canvas.name}' is NOT in DontDestroyOnLoad hierarchy, continuing search...");
            }
        }
        
        // PRIORITY 3: Try OptionsPresenter (but verify it's in dialogue system)
        OptionsPresenter optionsPresenter = FindAnyObjectByType<OptionsPresenter>();
        if (optionsPresenter != null)
        {
            Canvas canvas = optionsPresenter.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Verify this Canvas is in DontDestroyOnLoad hierarchy
                Transform current = canvas.transform;
                while (current != null && current.parent != null)
                {
                    if (current.parent.name == "DontDestroyOnLoad" || current.parent.name.Contains("Dialogue"))
                    {
                        Debug.Log($"CharacterSpriteManager: Found Canvas via OptionsPresenter in dialogue system: '{canvas.name}' on GameObject '{canvas.gameObject.name}' (InstanceID: {canvas.GetInstanceID()})");
                        return canvas;
                    }
                    current = current.parent;
                }
                Debug.LogWarning($"CharacterSpriteManager: OptionsPresenter Canvas '{canvas.name}' is NOT in DontDestroyOnLoad hierarchy, continuing search...");
            }
        }
        
        // Last resort: find any Canvas (but warn)
        Canvas anyCanvas = FindAnyObjectByType<Canvas>();
        Debug.LogWarning($"CharacterSpriteManager: Using fallback Canvas (may be wrong one!): {(anyCanvas != null ? $"'{anyCanvas.name}' on '{anyCanvas.gameObject.name}' (InstanceID: {anyCanvas.GetInstanceID()})" : "NULL")}");
        return anyCanvas;
    }
    
    void BuildDictionary()
    {
        portraitDataDictionary = new Dictionary<string, CharacterPortraitData>();

        // First, add manually assigned sprites (convert to CharacterPortraitData)
        int manualCount = 0;
        foreach (var entry in characterSprites)
        {
            if (entry.sprite != null && !string.IsNullOrEmpty(entry.characterTag))
            {
                // Wrap single sprite in CharacterPortraitData for backwards compatibility
                CharacterPortraitData data = new CharacterPortraitData();
                data.characterName = entry.characterTag;
                data.baseIdleSprite = entry.sprite;
                data.talkingFrames = new Sprite[] { entry.sprite }; // Use same sprite for talking
                data.eyeBlinkFrames = new Sprite[] { entry.sprite, entry.sprite, entry.sprite }; // Dummy blink (no animation)

                portraitDataDictionary[entry.characterTag] = data;
                manualCount++;
            }
        }
        Debug.Log($"CharacterSpriteManager: Loaded {manualCount} manually assigned sprites");

        // Then, try to auto-load from Resources or folder
        AutoLoadCharacters();

        // Log dictionary contents
        Debug.Log($"CharacterSpriteManager: Portrait data dictionary contains {portraitDataDictionary.Count} entries:");
        foreach (var kvp in portraitDataDictionary)
        {
            CharacterPortraitData data = kvp.Value;
            Debug.Log($"  - {kvp.Key}: Base={data.baseIdleSprite?.name ?? "NULL"}, Talking={data.talkingFrames?.Length ?? 0} frames, Eyes={data.eyeBlinkFrames?.Length ?? 0} frames");
        }
    }
    
    void AutoLoadCharacters()
    {
#if UNITY_EDITOR
        // In editor, load from folder path (works in both edit and play mode)
        LoadFromFolder();
        
        // Also try Resources as fallback
        LoadFromResources();
#else
        // Runtime: Only Resources.Load works
        LoadFromResources();
#endif
    }
    
    void LoadFromResources()
    {
        // Try Resources.Load with the folder path
        string resourcesPath = characterFolderPath.Replace("Assets/", "").Replace("\\", "/");

        // Remove "Resources/" prefix if present
        if (resourcesPath.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
        {
            resourcesPath = resourcesPath.Substring("Resources/".Length);
        }

        // First, try to load multi-frame characters (Alice/, Supervisor/, etc.)
        foreach (var kvp in characterTagToSpriteName)
        {
            if (kvp.Value == null) continue; // Skip characters with no sprite

            string characterName = CapitalizeFirst(kvp.Value);

            // Try loading multi-frame character data
            CharacterPortraitData data = CharacterPortraitData.LoadFromFolder(characterName, resourcesPath);

            // Multi-frame data ALWAYS takes precedence (overwrites manual single sprites)
            if (data.IsValid)
            {
                portraitDataDictionary[kvp.Key] = data;
                Debug.Log($"CharacterSpriteManager: Auto-loaded multi-frame character '{kvp.Key}' from Resources/{resourcesPath}/{characterName}");
            }
        }

        // Then, load single sprite files (legacy format)
        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcesPath);

        // Also try loading from Resources/Characters (standard location for builds)
        if (sprites == null || sprites.Length == 0)
        {
            sprites = Resources.LoadAll<Sprite>("Characters");
            if (sprites != null && sprites.Length > 0)
            {
                Debug.Log($"CharacterSpriteManager: Loading from Resources/Characters");
            }
        }

        if (sprites != null && sprites.Length > 0)
        {
            foreach (var sprite in sprites)
            {
                if (sprite != null)
                {
                    // Extract character name from sprite name (e.g., "alice" -> "char_Alice")
                    string spriteName = sprite.name.ToLower();
                    string charTag = "char_" + CapitalizeFirst(spriteName);

                    // Try to match with known character tags
                    foreach (var kvp in characterTagToSpriteName)
                    {
                        if (kvp.Value != null && spriteName.Contains(kvp.Value.ToLower()))
                        {
                            charTag = kvp.Key;
                            break;
                        }
                    }

                    // Only add if not already in dictionary (manual assignments and multi-frame take precedence)
                    if (!portraitDataDictionary.ContainsKey(charTag))
                    {
                        // Wrap single sprite in CharacterPortraitData
                        CharacterPortraitData data = new CharacterPortraitData();
                        data.characterName = charTag;
                        data.baseIdleSprite = sprite;
                        data.talkingFrames = new Sprite[] { sprite };
                        data.eyeBlinkFrames = new Sprite[] { sprite, sprite, sprite };

                        portraitDataDictionary[charTag] = data;
                        Debug.Log($"CharacterSpriteManager: Auto-loaded single-frame character '{charTag}' from Resources (sprite: {sprite.name})");
                    }
                }
            }
        }
    }
    
#if UNITY_EDITOR
    void LoadFromFolder()
    {
        string fullPath = "Assets/" + characterFolderPath;

        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            return;
        }

        // First, check for character subfolders with multi-frame structure (Alice/, Supervisor/, etc.)
        foreach (var kvp in characterTagToSpriteName)
        {
            if (kvp.Value == null) continue; // Skip characters with no sprite

            string characterName = CapitalizeFirst(kvp.Value);
            string characterFolderPath = $"{fullPath}/{characterName}";

            // Check if character has subfolder structure
            if (AssetDatabase.IsValidFolder(characterFolderPath))
            {
                // Check if it has Base/, Talking/, or Eyes/ subfolders (multi-frame structure)
                bool hasMultiFrameStructure = AssetDatabase.IsValidFolder($"{characterFolderPath}/Base") ||
                                               AssetDatabase.IsValidFolder($"{characterFolderPath}/Talking") ||
                                               AssetDatabase.IsValidFolder($"{characterFolderPath}/Eyes");

                if (hasMultiFrameStructure)
                {
                    // Load multi-frame character data
                    CharacterPortraitData data = CharacterPortraitData.LoadFromFolder(characterName, this.characterFolderPath);

                    // Multi-frame data ALWAYS takes precedence (overwrites manual single sprites)
                    if (data.IsValid)
                    {
                        portraitDataDictionary[kvp.Key] = data;
                        Debug.Log($"CharacterSpriteManager: Auto-loaded multi-frame character '{kvp.Key}' from {characterFolderPath}");
                    }
                    continue; // Skip single-file loading for this character
                }
            }
        }

        // Then, load single sprite files (legacy format for characters without animations)
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { fullPath });

        foreach (string guid in textureGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // Skip files inside subfolders (already handled above)
            string relativePath = assetPath.Replace(fullPath + "/", "");
            if (relativePath.Contains("/"))
                continue;

            // Extract character name from file name (e.g., "alice.png" -> "alice")
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower();
            string charTag = "char_" + CapitalizeFirst(fileName);

            // Try to match with known character tags
            foreach (var kvp in characterTagToSpriteName)
            {
                if (kvp.Value != null && fileName.Contains(kvp.Value.ToLower()))
                {
                    charTag = kvp.Key;
                    break;
                }
            }

            // Skip if already in dictionary
            if (portraitDataDictionary.ContainsKey(charTag))
            {
                continue;
            }

            // Load the sprite (Unity can load sprites from textures)
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            // If not a sprite, try loading as texture and getting sprites from it
            if (sprite == null)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture != null)
                {
                    // Try to get all sprites from the texture (for sprite sheets)
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (Object asset in assets)
                    {
                        if (asset is Sprite spriteAsset)
                        {
                            sprite = spriteAsset;
                            break; // Use the first sprite found
                        }
                    }

                    // If still no sprite, create one from the texture
                    if (sprite == null)
                    {
                        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f));
                    }
                }
            }

            if (sprite != null)
            {
                // Wrap single sprite in CharacterPortraitData for backwards compatibility
                CharacterPortraitData data = new CharacterPortraitData();
                data.characterName = charTag;
                data.baseIdleSprite = sprite;
                data.talkingFrames = new Sprite[] { sprite }; // Use same sprite for talking
                data.eyeBlinkFrames = new Sprite[] { sprite, sprite, sprite }; // Dummy blink (no animation)

                portraitDataDictionary[charTag] = data;
                Debug.Log($"CharacterSpriteManager: Auto-loaded single-frame character '{charTag}' from {assetPath}");
            }
        }
    }
#endif
    
    string CapitalizeFirst(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return char.ToUpper(str[0]) + str.Substring(1);
    }
    
    int FindBackgroundImageIndex()
    {
        if (targetCanvas == null)
            return -1;
        
        // Look for BackgroundCommandHandler's image component
        BackgroundCommandHandler bgHandler = FindAnyObjectByType<BackgroundCommandHandler>();
        if (bgHandler != null && bgHandler.backgroundImage != null)
        {
            Transform bgTransform = bgHandler.backgroundImage.transform;
            if (bgTransform.parent == targetCanvas.transform)
            {
                return bgTransform.GetSiblingIndex();
            }
        }
        
        // Fallback: look for any Image component with "background" in name
        for (int i = 0; i < targetCanvas.transform.childCount; i++)
        {
            Transform child = targetCanvas.transform.GetChild(i);
            Image img = child.GetComponent<Image>();
            if (img != null && child.name.ToLower().Contains("background"))
            {
                return i;
            }
        }
        
        return -1;
    }
    
    int FindDialogueUIIndex()
    {
        if (targetCanvas == null)
            return -1;
        
        int lowestDialogueIndex = int.MaxValue;
        
        // Search recursively through all children
        SearchForDialogueUI(targetCanvas.transform, 0, ref lowestDialogueIndex);
        
        return lowestDialogueIndex == int.MaxValue ? -1 : lowestDialogueIndex;
    }
    
    void SearchForDialogueUI(Transform parent, int currentIndex, ref int lowestIndex)
    {
        // Check current level for dialogue UI components
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            
            // Check for LinePresenter
            if (child.GetComponent<LinePresenter>() != null)
            {
                // Get sibling index relative to canvas root
                int siblingIndex = GetSiblingIndexRelativeToCanvas(child);
                if (siblingIndex >= 0 && siblingIndex < lowestIndex)
                {
                    lowestIndex = siblingIndex;
                }
            }
            
            // Check for OptionsPresenter
            if (child.GetComponent<OptionsPresenter>() != null)
            {
                // Get sibling index relative to canvas root
                int siblingIndex = GetSiblingIndexRelativeToCanvas(child);
                if (siblingIndex >= 0 && siblingIndex < lowestIndex)
                {
                    lowestIndex = siblingIndex;
                }
            }
            
            // Check for names containing "Line" or "Option" or "Dialogue"
            string name = child.name.ToLower();
            if (name.Contains("line") || name.Contains("option") || name.Contains("dialogue"))
            {
                // Get sibling index relative to canvas root
                int siblingIndex = GetSiblingIndexRelativeToCanvas(child);
                if (siblingIndex >= 0 && siblingIndex < lowestIndex)
                {
                    lowestIndex = siblingIndex;
                }
            }
            
            // Recursively search children
            if (child.childCount > 0)
            {
                SearchForDialogueUI(child, i, ref lowestIndex);
            }
        }
    }
    
    int GetSiblingIndexRelativeToCanvas(Transform transform)
    {
        // If transform is direct child of canvas, return its sibling index
        if (transform.parent == targetCanvas.transform)
        {
            return transform.GetSiblingIndex();
        }
        
        // Otherwise, find the parent that's a direct child of canvas
        Transform current = transform;
        while (current != null && current.parent != targetCanvas.transform)
        {
            current = current.parent;
        }
        
        if (current != null && current.parent == targetCanvas.transform)
        {
            return current.GetSiblingIndex();
        }
        
        return -1;
    }
    
    void LogCanvasHierarchy(string context = "")
    {
        if (targetCanvas == null)
        {
            Debug.LogWarning($"CharacterSpriteManager: Cannot log hierarchy - targetCanvas is null ({context})");
            return;
        }
        
        Debug.Log($"=== Canvas Hierarchy {context} ===");
        Debug.Log($"Canvas: '{targetCanvas.name}' on GameObject '{targetCanvas.gameObject.name}'");
        Debug.Log($"Total children: {targetCanvas.transform.childCount}");
        
        for (int i = 0; i < targetCanvas.transform.childCount; i++)
        {
            Transform child = targetCanvas.transform.GetChild(i);
            bool isActive = child.gameObject.activeSelf;
            string activeStr = isActive ? "ACTIVE" : "INACTIVE";
            
            // Check if this is one of our sprites
            string spriteMarker = "";
            if (child == leftSpriteObject?.transform) spriteMarker = " <-- LEFT SPRITE";
            if (child == rightSpriteObject?.transform) spriteMarker = " <-- RIGHT SPRITE";
            
            Debug.Log($"  [{i}] {child.name} ({activeStr}){spriteMarker}");
        }
        Debug.Log("=== End Hierarchy ===");
    }
    
    void SetupCharacterSprites()
    {
        // Force re-find the Canvas to ensure we have the correct one
        // This is critical because sprites must be in the same Canvas as dialogue UI
        Canvas correctCanvas = FindDialogueCanvas();
        if (correctCanvas == null)
        {
            Debug.LogError("CharacterSpriteManager: Cannot find dialogue Canvas. Cannot create character sprites.");
            return;
        }
        
        // Verify we're using the correct Canvas
        if (targetCanvas != correctCanvas)
        {
            Debug.LogWarning($"CharacterSpriteManager: Canvas mismatch! targetCanvas was '{targetCanvas?.name}' on '{targetCanvas?.gameObject.name}', but correct Canvas is '{correctCanvas.name}' on '{correctCanvas.gameObject.name}'. Updating...");
            targetCanvas = correctCanvas;
        }
        
        Debug.Log($"CharacterSpriteManager: Setting up sprites in Canvas '{targetCanvas.name}' on GameObject '{targetCanvas.gameObject.name}' (InstanceID: {targetCanvas.GetInstanceID()})");
        
        // Log hierarchy BEFORE making changes
        LogCanvasHierarchy("BEFORE sprite creation");
        
        // Find background and dialogue UI indices
        int backgroundIndex = FindBackgroundImageIndex();
        int dialogueUIIndex = FindDialogueUIIndex();
        
        Debug.Log($"CharacterSpriteManager: Background index: {backgroundIndex}, Dialogue UI index: {dialogueUIIndex}");
        
        // Calculate target index: right after background, but BEFORE dialogue UI
        int targetIndex;
        if (backgroundIndex >= 0)
        {
            // Start right after background
            targetIndex = backgroundIndex + 1;
            
            // If dialogue UI is found and would be before or at our target, adjust
            if (dialogueUIIndex >= 0 && targetIndex >= dialogueUIIndex)
            {
                // Sprites would be after dialogue - move before dialogue
                targetIndex = dialogueUIIndex - 1;
                // But ensure still after background
                if (targetIndex <= backgroundIndex)
                {
                    targetIndex = backgroundIndex + 1; // Place right after background
                }
            }
        }
        else
        {
            // No background found, place at index 0 or before dialogue UI
            if (dialogueUIIndex >= 0)
            {
                // Place at least 1 index before dialogue UI
                targetIndex = Mathf.Max(0, dialogueUIIndex - 1);
            }
            else
            {
                targetIndex = 0; // Fallback: place at start
            }
        }
        
        // Safety: Ensure target index is not negative
        targetIndex = Mathf.Max(0, targetIndex);
        
        Debug.Log($"CharacterSpriteManager: Calculated target sprite index: {targetIndex} (background: {backgroundIndex}, dialogue: {dialogueUIIndex})");
        
        // Destroy existing sprite objects if they exist (in case they were created in wrong Canvas)
        if (leftSpriteObject != null)
        {
            Debug.LogWarning($"CharacterSpriteManager: Destroying existing left sprite object (was in wrong Canvas)");
            DestroyImmediate(leftSpriteObject);
            leftSpriteObject = null;
            leftBaseImage = null;
            leftBaseRect = null;
            leftEyesImage = null;
            leftEyesRect = null;
        }
        if (rightSpriteObject != null)
        {
            Debug.LogWarning($"CharacterSpriteManager: Destroying existing right sprite object (was in wrong Canvas)");
            DestroyImmediate(rightSpriteObject);
            rightSpriteObject = null;
            rightBaseImage = null;
            rightBaseRect = null;
            rightEyesImage = null;
            rightEyesRect = null;
        }
        
        // Verify we have the correct Canvas before creating sprites
        if (targetCanvas == null)
        {
            Debug.LogError("CharacterSpriteManager: targetCanvas is null! Cannot create sprites.");
            return;
        }
        
        Debug.Log($"CharacterSpriteManager: Creating sprites as children of Canvas '{targetCanvas.name}' on '{targetCanvas.gameObject.name}'");

        // Ensure we have a reference to the breathing animation driver (exists on Yarn Dialogue System prefab)
        if (talkAnimation == null)
        {
            talkAnimation = FindAnyObjectByType<CharacterTalkAnimation>();
            if (talkAnimation == null)
            {
                Debug.LogWarning("CharacterSpriteManager: CharacterTalkAnimation not found. Portrait breathing will be disabled.");
            }
        }

        // Frame is taller than the portrait image so the head never clips at the inhale peak.
        float frameHeight = spriteSize.y + Mathf.Max(0f, breathAmplitudePixels) + Mathf.Max(0f, portraitFrameHeadroom) - 100f;
        Vector2 frameSize = new Vector2(spriteSize.x, frameHeight);
        
        // Create LEFT portrait frame (masked container + translucent background)
        leftSpriteObject = new GameObject("CharacterPortraitFrame_Left");
        leftSpriteObject.transform.SetParent(targetCanvas.transform, false);
        
        // Verify parent is correct
        if (leftSpriteObject.transform.parent != targetCanvas.transform)
        {
            Debug.LogError($"CharacterSpriteManager: Failed to set parent! Expected '{targetCanvas.gameObject.name}', got '{leftSpriteObject.transform.parent?.gameObject.name}'");
        }
        else
        {
            Debug.Log($"CharacterSpriteManager: Left sprite parent confirmed: '{leftSpriteObject.transform.parent.gameObject.name}'");
        }
        
        // Force sprite to render behind dialogue by setting sibling index
        // Use SetAsFirstSibling or SetSiblingIndex to ensure it's at the calculated position
        if (targetIndex == 0)
        {
            leftSpriteObject.transform.SetAsFirstSibling();
        }
        else
        {
            leftSpriteObject.transform.SetSiblingIndex(targetIndex);
        }
        
        // Verify final position
        int finalLeftIndex = leftSpriteObject.transform.GetSiblingIndex();
        Debug.Log($"CharacterSpriteManager: Left sprite placed at sibling index: {finalLeftIndex} in Canvas '{targetCanvas.name}'");
        
        // Re-check dialogue UI index after placing left sprite (indices may have shifted)
        int dialogueUIIndexAfter = FindDialogueUIIndex();
        Debug.Log($"CharacterSpriteManager: Dialogue UI index after left sprite placement: {dialogueUIIndexAfter}");
        
        // Safety check: If dialogue UI exists and is at same or lower index, move sprite lower
        if (dialogueUIIndexAfter >= 0 && finalLeftIndex >= dialogueUIIndexAfter)
        {
            Debug.LogWarning($"CharacterSpriteManager: Sprite index {finalLeftIndex} is not before dialogue UI index {dialogueUIIndexAfter}. Moving sprite to index {dialogueUIIndexAfter - 1}");
            int newIndex = Mathf.Max(backgroundIndex + 1, dialogueUIIndexAfter - 1);
            leftSpriteObject.transform.SetSiblingIndex(newIndex);
            finalLeftIndex = leftSpriteObject.transform.GetSiblingIndex();
            Debug.Log($"CharacterSpriteManager: Left sprite moved to index: {finalLeftIndex}");
        }
        
        RectTransform leftFrameRect = leftSpriteObject.AddComponent<RectTransform>();
        leftFrameRect.anchorMin = new Vector2(0f, 0f);
        leftFrameRect.anchorMax = new Vector2(0f, 0f);
        leftFrameRect.pivot = new Vector2(0f, 0f);
        leftFrameRect.anchoredPosition = bottomLeftOffset;
        leftFrameRect.sizeDelta = frameSize;

        Image leftFrameImage = leftSpriteObject.AddComponent<Image>();
        leftFrameImage.color = portraitFrameColor;
        leftFrameImage.raycastTarget = false;
        leftSpriteObject.AddComponent<RectMask2D>();

        // LAYERED PORTRAIT STRUCTURE: Base layer + Eyes overlay
        // Base Layer: Shows static portrait or talking frames
        GameObject leftBaseObject = new GameObject("PortraitBase_Left");
        leftBaseObject.transform.SetParent(leftSpriteObject.transform, false);
        leftBaseRect = leftBaseObject.AddComponent<RectTransform>();
        leftBaseRect.anchorMin = new Vector2(0f, 0f);
        leftBaseRect.anchorMax = new Vector2(1f, 0f);
        leftBaseRect.pivot = new Vector2(0.5f, 0f);
        leftBaseRect.anchoredPosition = Vector2.zero;
        leftBaseRect.sizeDelta = new Vector2(0f, spriteSize.y);

        leftBaseImage = leftBaseObject.AddComponent<Image>();
        leftBaseImage.preserveAspect = true;
        leftBaseImage.raycastTarget = false;

        // Talking Overlay Layer: Shows talking frames on top of base
        GameObject leftTalkingObject = new GameObject("PortraitTalking_Left");
        leftTalkingObject.transform.SetParent(leftSpriteObject.transform, false);
        leftTalkingRect = leftTalkingObject.AddComponent<RectTransform>();
        leftTalkingRect.anchorMin = new Vector2(0f, 0f);
        leftTalkingRect.anchorMax = new Vector2(1f, 0f);
        leftTalkingRect.pivot = new Vector2(0.5f, 0f);
        leftTalkingRect.anchoredPosition = Vector2.zero;
        leftTalkingRect.sizeDelta = new Vector2(0f, spriteSize.y);

        leftTalkingImage = leftTalkingObject.AddComponent<Image>();
        leftTalkingImage.preserveAspect = true;
        leftTalkingImage.raycastTarget = false;
        leftTalkingImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent (will be set by animator)

        // Eyes Overlay Layer: Shows eye blink frames on top of talking
        GameObject leftEyesObject = new GameObject("PortraitEyes_Left");
        leftEyesObject.transform.SetParent(leftSpriteObject.transform, false);
        leftEyesRect = leftEyesObject.AddComponent<RectTransform>();
        leftEyesRect.anchorMin = new Vector2(0f, 0f);
        leftEyesRect.anchorMax = new Vector2(1f, 0f);
        leftEyesRect.pivot = new Vector2(0.5f, 0f);
        leftEyesRect.anchoredPosition = Vector2.zero;
        leftEyesRect.sizeDelta = new Vector2(0f, spriteSize.y);

        leftEyesImage = leftEyesObject.AddComponent<Image>();
        leftEyesImage.preserveAspect = true;
        leftEyesImage.raycastTarget = false;
        leftEyesImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent (will be set by animator)

        // Add animator component to manage talking and blinking
        leftAnimator = leftSpriteObject.AddComponent<CharacterPortraitAnimator>();

        leftSpriteObject.SetActive(false);
        
        // Create RIGHT portrait frame (masked container + translucent background)
        rightSpriteObject = new GameObject("CharacterPortraitFrame_Right");
        rightSpriteObject.transform.SetParent(targetCanvas.transform, false);
        
        // Verify parent is correct
        if (rightSpriteObject.transform.parent != targetCanvas.transform)
        {
            Debug.LogError($"CharacterSpriteManager: Failed to set parent! Expected '{targetCanvas.gameObject.name}', got '{rightSpriteObject.transform.parent?.gameObject.name}'");
        }
        else
        {
            Debug.Log($"CharacterSpriteManager: Right sprite parent confirmed: '{rightSpriteObject.transform.parent.gameObject.name}'");
        }
        
        // Set sibling index to match left sprite (right after left)
        int leftIndex = leftSpriteObject.transform.GetSiblingIndex();
        rightSpriteObject.transform.SetSiblingIndex(leftIndex + 1);
        int finalRightIndex = rightSpriteObject.transform.GetSiblingIndex();
        Debug.Log($"CharacterSpriteManager: Right sprite placed at sibling index: {finalRightIndex} in Canvas '{targetCanvas.name}'");
        
        // Log hierarchy AFTER creating sprites to verify final order
        LogCanvasHierarchy("AFTER sprite creation");
        
        // Final verification: Check all indices one more time
        int finalBackgroundIndex = FindBackgroundImageIndex();
        int finalDialogueUIIndex = FindDialogueUIIndex();
        int finalLeftSpriteIndex = leftSpriteObject.transform.GetSiblingIndex();
        int finalRightSpriteIndex = rightSpriteObject.transform.GetSiblingIndex();
        
        Debug.Log($"CharacterSpriteManager: FINAL INDICES - Background: {finalBackgroundIndex}, Left Sprite: {finalLeftSpriteIndex}, Right Sprite: {finalRightSpriteIndex}, Dialogue UI: {finalDialogueUIIndex}");
        
        // Verify order is correct
        if (finalBackgroundIndex >= 0 && finalLeftSpriteIndex <= finalBackgroundIndex)
        {
            Debug.LogError($"CharacterSpriteManager: ERROR - Left sprite index {finalLeftSpriteIndex} is NOT after background index {finalBackgroundIndex}!");
        }
        if (finalDialogueUIIndex >= 0 && finalRightSpriteIndex >= finalDialogueUIIndex)
        {
            Debug.LogError($"CharacterSpriteManager: ERROR - Right sprite index {finalRightSpriteIndex} is NOT before dialogue UI index {finalDialogueUIIndex}!");
        }
        
        RectTransform rightFrameRect = rightSpriteObject.AddComponent<RectTransform>();
        rightFrameRect.anchorMin = new Vector2(1f, 0f);
        rightFrameRect.anchorMax = new Vector2(1f, 0f);
        rightFrameRect.pivot = new Vector2(1f, 0f);
        rightFrameRect.anchoredPosition = bottomRightOffset;
        rightFrameRect.sizeDelta = frameSize;

        Image rightFrameImage = rightSpriteObject.AddComponent<Image>();
        rightFrameImage.color = portraitFrameColor;
        rightFrameImage.raycastTarget = false;
        rightSpriteObject.AddComponent<RectMask2D>();

        // LAYERED PORTRAIT STRUCTURE: Base layer + Eyes overlay
        // Base Layer: Shows static portrait or talking frames
        GameObject rightBaseObject = new GameObject("PortraitBase_Right");
        rightBaseObject.transform.SetParent(rightSpriteObject.transform, false);
        rightBaseRect = rightBaseObject.AddComponent<RectTransform>();
        rightBaseRect.anchorMin = new Vector2(0f, 0f);
        rightBaseRect.anchorMax = new Vector2(1f, 0f);
        rightBaseRect.pivot = new Vector2(0.5f, 0f);
        rightBaseRect.anchoredPosition = Vector2.zero;
        rightBaseRect.sizeDelta = new Vector2(0f, spriteSize.y);

        rightBaseImage = rightBaseObject.AddComponent<Image>();
        rightBaseImage.preserveAspect = true;
        rightBaseImage.raycastTarget = false;

        // Talking Overlay Layer: Shows talking frames on top of base
        GameObject rightTalkingObject = new GameObject("PortraitTalking_Right");
        rightTalkingObject.transform.SetParent(rightSpriteObject.transform, false);
        rightTalkingRect = rightTalkingObject.AddComponent<RectTransform>();
        rightTalkingRect.anchorMin = new Vector2(0f, 0f);
        rightTalkingRect.anchorMax = new Vector2(1f, 0f);
        rightTalkingRect.pivot = new Vector2(0.5f, 0f);
        rightTalkingRect.anchoredPosition = Vector2.zero;
        rightTalkingRect.sizeDelta = new Vector2(0f, spriteSize.y);

        rightTalkingImage = rightTalkingObject.AddComponent<Image>();
        rightTalkingImage.preserveAspect = true;
        rightTalkingImage.raycastTarget = false;
        rightTalkingImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent (will be set by animator)

        // Eyes Overlay Layer: Shows eye blink frames on top of talking
        GameObject rightEyesObject = new GameObject("PortraitEyes_Right");
        rightEyesObject.transform.SetParent(rightSpriteObject.transform, false);
        rightEyesRect = rightEyesObject.AddComponent<RectTransform>();
        rightEyesRect.anchorMin = new Vector2(0f, 0f);
        rightEyesRect.anchorMax = new Vector2(1f, 0f);
        rightEyesRect.pivot = new Vector2(0.5f, 0f);
        rightEyesRect.anchoredPosition = Vector2.zero;
        rightEyesRect.sizeDelta = new Vector2(0f, spriteSize.y);

        rightEyesImage = rightEyesObject.AddComponent<Image>();
        rightEyesImage.preserveAspect = true;
        rightEyesImage.raycastTarget = false;
        rightEyesImage.color = new Color(1f, 1f, 1f, 0f); // Start transparent (will be set by animator)

        // Add animator component to manage talking and blinking
        rightAnimator = rightSpriteObject.AddComponent<CharacterPortraitAnimator>();

        rightSpriteObject.SetActive(false);

        // Wire breathing animation to ALL layers (base + talking + eyes for both portraits = 6 total)
        if (talkAnimation != null)
        {
            // Use new breathing parameters: 3px amplitude, 10s period (half speed/distance)
            talkAnimation.Configure(3f, 10f);
            RectTransform[] allLayers = { leftBaseRect, leftTalkingRect, leftEyesRect, rightBaseRect, rightTalkingRect, rightEyesRect };
            talkAnimation.SetTargets(allLayers, resetPhase: true);
        }
    }
    
    void OnNodeStarted(string nodeName)
    {
        Debug.Log($"CharacterSpriteManager: Node started: {nodeName}");
        
        if (dialogueRunner == null || dialogueRunner.Dialogue == null)
        {
            Debug.LogWarning("CharacterSpriteManager: DialogueRunner or Dialogue is null");
            return;
        }
        
        // Get tags for this node using the new API
        string tagsHeader = dialogueRunner.Dialogue.GetHeaderValue(nodeName, "tags");
        Debug.Log($"CharacterSpriteManager: Tags header: '{tagsHeader}'");
        
        if (string.IsNullOrEmpty(tagsHeader))
        {
            // No tags, hide all character sprites
            Debug.Log("CharacterSpriteManager: No tags found, hiding all characters");
            HideAllCharacters();
            return;
        }
        
        // Update current background from BackgroundCommandHandler
        UpdateCurrentBackground();
        
        // Split tags by spaces
        string[] tags = tagsHeader.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        // Extract character tags (starting with "char_")
        List<string> characterTags = new List<string>();
        foreach (string tag in tags)
        {
            // Remove # prefix if present
            string cleanTag = tag.StartsWith("#") ? tag.Substring(1) : tag;
            if (cleanTag.StartsWith("char_"))
            {
                characterTags.Add(cleanTag);
            }
        }
        
        Debug.Log($"CharacterSpriteManager: Extracted character tags (before auto-add): [{string.Join(", ", characterTags)}]");
        
        // Filter out characters without sprites (Player, DarkFigure)
        characterTags = characterTags.Where(tag => 
            characterTagToSpriteName.ContainsKey(tag) && 
            characterTagToSpriteName[tag] != null).ToList();
        
        // Auto-add Alice when appropriate
        bool aliceAdded = TryAutoAddAlice(characterTags);
        if (aliceAdded)
        {
            Debug.Log("CharacterSpriteManager: Auto-added Alice (player is present, no supervisor, not darkroom)");
        }
        else
        {
            Debug.Log("CharacterSpriteManager: Did not auto-add Alice - Supervisor present or darkroom or already in list");
        }
        
        Debug.Log($"CharacterSpriteManager: Final character tags: [{string.Join(", ", characterTags)}]");
        
        // Limit to 2 characters max
        if (characterTags.Count > 2)
        {
            characterTags = characterTags.Take(2).ToList();
            Debug.Log($"CharacterSpriteManager: Limited to 2 characters: [{string.Join(", ", characterTags)}]");
        }
        
        // Display characters (priority: left first, then right)
        DisplayCharacters(characterTags);
    }
    
    void UpdateCurrentBackground()
    {
        // Try to get current background from BackgroundCommandHandler
        if (backgroundHandler != null && backgroundHandler.backgroundImage != null && backgroundHandler.backgroundImage.sprite != null)
        {
            // Extract background name from sprite name (e.g., "bg_office" from sprite name)
            string spriteName = backgroundHandler.backgroundImage.sprite.name.ToLower();
            currentBackground = spriteName;
            Debug.Log($"CharacterSpriteManager: Current background updated to: {currentBackground}");
        }
    }
    
    bool TryAutoAddAlice(List<string> characterTags)
    {
        // Check if Alice is already in the list
        if (characterTags.Contains("char_Alice"))
        {
            return false; // Already present
        }
        
        // Check if Supervisor is present - if so, don't add Alice
        if (characterTags.Contains("char_Supervisor"))
        {
            return false; // Supervisor present, don't add Alice
        }
        
        // Check if we're in darkroom - if so, don't add Alice
        if (currentBackground.Contains("darkroom"))
        {
            return false; // Darkroom, don't add Alice
        }
        
        // Check if we have room (max 2 characters)
        if (characterTags.Count >= 2)
        {
            return false; // Already have 2 characters
        }
        
        // All conditions met - add Alice
        characterTags.Add("char_Alice");
        return true;
    }
    
    void DisplayCharacters(List<string> characterTags)
    {
        // Hide all first
        HideAllCharacters();

        if (characterTags == null || characterTags.Count == 0)
        {
            Debug.Log("CharacterSpriteManager: No characters to display");
            return;
        }

        // Display first character on left
        if (characterTags.Count >= 1)
        {
            string leftTag = characterTags[0];
            if (portraitDataDictionary.TryGetValue(leftTag, out CharacterPortraitData leftData))
            {
                if (leftSpriteObject != null && leftBaseImage != null)
                {
                    currentLeftCharacterTag = leftTag;

                    // IMPORTANT: Activate GameObject BEFORE initializing animator (coroutines need active GameObject)
                    leftSpriteObject.SetActive(true);

                    // Initialize animator with portrait data
                    if (leftAnimator != null)
                    {
                        leftAnimator.Initialize(leftData, leftBaseImage, leftTalkingImage, leftEyesImage);
                    }

                    Debug.Log($"CharacterSpriteManager: Displaying '{leftTag}' on left (base sprite: {leftData.baseIdleSprite?.name}, {leftData.talkingFrames.Length} talking frames, {leftData.eyeBlinkFrames.Length} eye frames)");
                }
            }
            else
            {
                Debug.LogWarning($"CharacterSpriteManager: No portrait data found for character tag '{leftTag}' in dictionary. Available keys: [{string.Join(", ", portraitDataDictionary.Keys)}]");
            }
        }

        // Display second character on right
        if (characterTags.Count >= 2)
        {
            string rightTag = characterTags[1];
            if (portraitDataDictionary.TryGetValue(rightTag, out CharacterPortraitData rightData))
            {
                if (rightSpriteObject != null && rightBaseImage != null)
                {
                    currentRightCharacterTag = rightTag;

                    // IMPORTANT: Activate GameObject BEFORE initializing animator (coroutines need active GameObject)
                    rightSpriteObject.SetActive(true);

                    // Initialize animator with portrait data
                    if (rightAnimator != null)
                    {
                        rightAnimator.Initialize(rightData, rightBaseImage, rightTalkingImage, rightEyesImage);
                    }

                    Debug.Log($"CharacterSpriteManager: Displaying '{rightTag}' on right (base sprite: {rightData.baseIdleSprite?.name}, {rightData.talkingFrames.Length} talking frames, {rightData.eyeBlinkFrames.Length} eye frames)");
                }
            }
            else
            {
                Debug.LogWarning($"CharacterSpriteManager: No portrait data found for character tag '{rightTag}' in dictionary. Available keys: [{string.Join(", ", portraitDataDictionary.Keys)}]");
            }
        }
    }

    /// <summary>
    /// Public API for PortraitTalkingStateController to get animator by character tag.
    /// Returns the animator for the character if currently displayed, null otherwise.
    /// </summary>
    public CharacterPortraitAnimator GetAnimatorForCharacter(string characterTag)
    {
        if (characterTag == currentLeftCharacterTag)
            return leftAnimator;
        if (characterTag == currentRightCharacterTag)
            return rightAnimator;

        return null; // Character not currently displayed
    }
    
    void HideAllCharacters()
    {
        if (leftSpriteObject != null)
        {
            leftSpriteObject.SetActive(false);
        }
        if (rightSpriteObject != null)
        {
            rightSpriteObject.SetActive(false);
        }

        // Clear current character tracking
        currentLeftCharacterTag = null;
        currentRightCharacterTag = null;
    }
    
    // Editor helper method
    void OnValidate()
    {
        // Rebuild dictionary when changes are made in the editor
        if (Application.isPlaying)
        {
            BuildDictionary();
        }
    }
}

