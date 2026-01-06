using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Editor utility to fix sprite cropping issues for all animated characters.
/// Unity's default "Tight" mesh type crops transparent pixels from each sprite,
/// causing misalignment between base, talking, and eye blink frames.
/// This script sets all character sprites to "Full Rect" mode to preserve the full canvas.
///
/// Unity 6 Note: Uses direct .meta file editing due to deprecated TextureImporter API.
///
/// Usage: Tools > Fix All Character Sprite Cropping
/// </summary>
public class FixCharacterSpriteCropping
{
    private const string CharactersBasePath = "Assets/Graphics/Characters";

    [MenuItem("Tools/Fix All Character Sprite Cropping")]
    static void FixAllCharacterSprites()
    {
        if (!AssetDatabase.IsValidFolder(CharactersBasePath))
        {
            Debug.LogError($"FixCharacterSpriteCropping: Characters folder not found at {CharactersBasePath}");
            return;
        }

        int totalFixed = 0;
        int charactersProcessed = 0;

        // Find all character folders
        string[] characterFolders = AssetDatabase.GetSubFolders(CharactersBasePath);

        foreach (string characterFolder in characterFolders)
        {
            string characterName = Path.GetFileName(characterFolder);

            // Check if this folder has the animated structure (Base/, Talking/, Eyes/)
            string basePath = $"{characterFolder}/Base";
            string talkingPath = $"{characterFolder}/Talking";
            string eyesPath = $"{characterFolder}/Eyes";

            bool hasAnimatedStructure = AssetDatabase.IsValidFolder(basePath) ||
                                        AssetDatabase.IsValidFolder(talkingPath) ||
                                        AssetDatabase.IsValidFolder(eyesPath);

            if (!hasAnimatedStructure)
            {
                Debug.Log($"FixCharacterSpriteCropping: Skipping '{characterName}' - no animated structure found");
                continue;
            }

            Debug.Log($"<color=cyan>Processing character: {characterName}</color>");
            charactersProcessed++;

            // Process each subfolder
            string[] subfolders = { basePath, talkingPath, eyesPath };

            foreach (string subfolder in subfolders)
            {
                if (!AssetDatabase.IsValidFolder(subfolder))
                    continue;

                int fixedInFolder = FixSpritesInFolder(subfolder);
                totalFixed += fixedInFolder;
            }
        }

        AssetDatabase.Refresh();

        if (totalFixed > 0)
        {
            Debug.Log($"<color=green>Finished fixing sprite cropping! Processed {charactersProcessed} characters, fixed {totalFixed} textures.</color>");
        }
        else
        {
            Debug.LogWarning($"FixCharacterSpriteCropping: No textures were fixed. Processed {charactersProcessed} characters.");
        }
    }

    static int FixSpritesInFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        if (guids.Length == 0)
        {
            return 0;
        }

        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string metaPath = assetPath + ".meta";

            if (!File.Exists(metaPath))
            {
                Debug.LogWarning($"FixCharacterSpriteCropping: No .meta file found for {assetPath}");
                continue;
            }

            // Get texture to determine actual size
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                Debug.LogWarning($"FixCharacterSpriteCropping: Could not load texture at {assetPath}");
                continue;
            }

            int width = texture.width;
            int height = texture.height;

            // Read .meta file
            string metaContent = File.ReadAllText(metaPath);

            // Change spriteMeshType from 1 (Tight) to 0 (Full Rect)
            metaContent = Regex.Replace(metaContent, @"spriteMeshType:\s*1", "spriteMeshType: 0");

            // Fix sprite rect to use full canvas
            // Pattern matches the rect section and replaces x, y, width, height
            metaContent = Regex.Replace(metaContent,
                @"rect:\s*\n\s*serializedVersion:\s*\d+\s*\n\s*x:\s*[\d.]+\s*\n\s*y:\s*[\d.]+\s*\n\s*width:\s*[\d.]+\s*\n\s*height:\s*[\d.]+",
                $"rect:\n        serializedVersion: 2\n        x: 0\n        y: 0\n        width: {width}\n        height: {height}");

            // Fix pivot to bottom-center (0.5, 0)
            metaContent = Regex.Replace(metaContent,
                @"pivot:\s*\{x:\s*[\d.]+,\s*y:\s*[\d.]+\}",
                "pivot: {x: 0.5, y: 0}");

            // Fix alignment to 7 (BottomCenter)
            metaContent = Regex.Replace(metaContent,
                @"alignment:\s*\d+",
                "alignment: 7");

            // Write back to .meta file
            File.WriteAllText(metaPath, metaContent);

            // Force reimport
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"Fixed sprite cropping: {assetPath} ({width}x{height})");
            fixedCount++;
        }

        return fixedCount;
    }
}
