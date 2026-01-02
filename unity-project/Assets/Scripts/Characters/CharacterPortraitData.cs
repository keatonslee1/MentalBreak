using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Data container for multi-frame character portrait assets.
/// Supports base portrait, talking animation frames (1-32), and eye blink frames (3+).
/// </summary>
[System.Serializable]
public class CharacterPortraitData
{
    public string characterName;

    [Header("Portrait Frames")]
    [Tooltip("Single static sprite shown when character is idle")]
    public Sprite baseIdleSprite;

    [Tooltip("1-32 frames for talking animation, cycled at 10fps")]
    public Sprite[] talkingFrames;

    [Tooltip("3+ frames for eye blink animation")]
    public Sprite[] eyeBlinkFrames;

    /// <summary>
    /// Validates that all required frames are loaded
    /// </summary>
    public bool IsValid => baseIdleSprite != null &&
                           talkingFrames != null && talkingFrames.Length > 0 &&
                           eyeBlinkFrames != null && eyeBlinkFrames.Length >= 3;

    /// <summary>
    /// Gets total frame count for debugging
    /// </summary>
    public int TotalFrameCount => (baseIdleSprite != null ? 1 : 0) +
                                  (talkingFrames?.Length ?? 0) +
                                  (eyeBlinkFrames?.Length ?? 0);

    /// <summary>
    /// Loads character portrait data from folder structure.
    /// Expected structure: Characters/{characterName}/Base/, Talking/, Eyes/
    /// </summary>
    public static CharacterPortraitData LoadFromFolder(string characterName, string basePath = "Graphics/Characters")
    {
        CharacterPortraitData data = new CharacterPortraitData();
        data.characterName = characterName;

#if UNITY_EDITOR
        // Editor: Use AssetDatabase
        data.LoadFromAssetDatabase(characterName, basePath);
#else
        // Runtime: Use Resources.Load
        data.LoadFromResources(characterName, basePath);
#endif

        return data;
    }

#if UNITY_EDITOR
    private void LoadFromAssetDatabase(string characterName, string basePath)
    {
        string characterPath = $"Assets/{basePath}/{characterName}";

        if (!AssetDatabase.IsValidFolder(characterPath))
        {
            Debug.LogWarning($"CharacterPortraitData: Character folder not found: {characterPath}");
            return;
        }

        // Load base sprite
        string baseFolderPath = $"{characterPath}/Base";
        if (AssetDatabase.IsValidFolder(baseFolderPath))
        {
            baseIdleSprite = LoadFirstSpriteFromFolder(baseFolderPath);
            if (baseIdleSprite != null)
            {
                Debug.Log($"CharacterPortraitData: Loaded base sprite for {characterName}: {baseIdleSprite.name}");
            }
        }
        else
        {
            Debug.LogWarning($"CharacterPortraitData: Base folder not found: {baseFolderPath}");
        }

        // Load talking frames
        string talkingFolderPath = $"{characterPath}/Talking";
        if (AssetDatabase.IsValidFolder(talkingFolderPath))
        {
            talkingFrames = LoadSpritesFromFolder(talkingFolderPath);
            Debug.Log($"CharacterPortraitData: Loaded {talkingFrames.Length} talking frames for {characterName}");
        }
        else
        {
            Debug.LogWarning($"CharacterPortraitData: Talking folder not found: {talkingFolderPath}");
            talkingFrames = new Sprite[0];
        }

        // Load eye blink frames
        string eyesFolderPath = $"{characterPath}/Eyes";
        if (AssetDatabase.IsValidFolder(eyesFolderPath))
        {
            eyeBlinkFrames = LoadSpritesFromFolder(eyesFolderPath);
            Debug.Log($"CharacterPortraitData: Loaded {eyeBlinkFrames.Length} eye blink frames for {characterName}");
        }
        else
        {
            Debug.LogWarning($"CharacterPortraitData: Eyes folder not found: {eyesFolderPath}");
            eyeBlinkFrames = new Sprite[0];
        }
    }

    private Sprite LoadFirstSpriteFromFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        if (guids.Length == 0)
            return null;

        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        return LoadSpriteFromTexture(assetPath);
    }

    private Sprite[] LoadSpritesFromFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        if (guids.Length == 0)
            return new Sprite[0];

        List<SpriteWithNumber> spritesWithNumbers = new List<SpriteWithNumber>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = LoadSpriteFromTexture(assetPath);

            if (sprite != null)
            {
                int number = ExtractNumberFromFilename(sprite.name);
                spritesWithNumbers.Add(new SpriteWithNumber { sprite = sprite, number = number });
            }
        }

        // Sort numerically by extracted number
        spritesWithNumbers.Sort((a, b) => a.number.CompareTo(b.number));

        return spritesWithNumbers.Select(s => s.sprite).ToArray();
    }

    private Sprite LoadSpriteFromTexture(string assetPath)
    {
        // Try loading directly as Sprite
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

        if (sprite != null)
            return sprite;

        // Try loading as Texture2D and get sprites from it
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (texture != null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object asset in assets)
            {
                if (asset is Sprite spriteAsset)
                    return spriteAsset;
            }

            // Create sprite from texture if none exists
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f));
        }

        return sprite;
    }
#endif

    private void LoadFromResources(string characterName, string basePath)
    {
        // Remove "Assets/" prefix if present
        string resourcesPath = basePath.Replace("Assets/", "");

        // Remove "Resources/" prefix if present
        if (resourcesPath.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
        {
            resourcesPath = resourcesPath.Substring("Resources/".Length);
        }

        string characterPath = $"{resourcesPath}/{characterName}";

        // Load base sprite
        Sprite[] baseSprites = Resources.LoadAll<Sprite>($"{characterPath}/Base");
        if (baseSprites != null && baseSprites.Length > 0)
        {
            baseIdleSprite = baseSprites[0];
            Debug.Log($"CharacterPortraitData: Loaded base sprite for {characterName}: {baseIdleSprite.name}");
        }
        else
        {
            Debug.LogWarning($"CharacterPortraitData: No base sprite found in Resources/{characterPath}/Base");
        }

        // Load talking frames
        Sprite[] talkingSprites = Resources.LoadAll<Sprite>($"{characterPath}/Talking");
        if (talkingSprites != null && talkingSprites.Length > 0)
        {
            talkingFrames = SortSpritesByNumber(talkingSprites);
            Debug.Log($"CharacterPortraitData: Loaded {talkingFrames.Length} talking frames for {characterName}");
        }
        else
        {
            Debug.LogWarning($"CharacterPortraitData: No talking frames found in Resources/{characterPath}/Talking");
            talkingFrames = new Sprite[0];
        }

        // Load eye blink frames
        Sprite[] eyeSprites = Resources.LoadAll<Sprite>($"{characterPath}/Eyes");
        if (eyeSprites != null && eyeSprites.Length > 0)
        {
            eyeBlinkFrames = SortSpritesByNumber(eyeSprites);
            Debug.Log($"CharacterPortraitData: Loaded {eyeBlinkFrames.Length} eye blink frames for {characterName}");
        }
        else
        {
            Debug.LogWarning($"CharacterPortraitData: No eye frames found in Resources/{characterPath}/Eyes");
            eyeBlinkFrames = new Sprite[0];
        }
    }

    private Sprite[] SortSpritesByNumber(Sprite[] sprites)
    {
        List<SpriteWithNumber> spritesWithNumbers = new List<SpriteWithNumber>();

        foreach (Sprite sprite in sprites)
        {
            int number = ExtractNumberFromFilename(sprite.name);
            spritesWithNumbers.Add(new SpriteWithNumber { sprite = sprite, number = number });
        }

        spritesWithNumbers.Sort((a, b) => a.number.CompareTo(b.number));

        return spritesWithNumbers.Select(s => s.sprite).ToArray();
    }

    /// <summary>
    /// Extracts numeric suffix from filename (e.g., "alice talking15" -> 15, "alice blinking2" -> 2)
    /// Handles filenames with spaces, underscores, and various formats
    /// </summary>
    private static int ExtractNumberFromFilename(string filename)
    {
        // Match one or more digits at the end of the filename
        Match match = Regex.Match(filename, @"(\d+)$");

        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }

        // Fallback: no number found, return 0
        return 0;
    }

    /// <summary>
    /// Helper struct for sorting sprites by numeric suffix
    /// </summary>
    private struct SpriteWithNumber
    {
        public Sprite sprite;
        public int number;
    }
}
