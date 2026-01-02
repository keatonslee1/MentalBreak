using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Editor utility to fix Alice sprite cropping issues.
/// Unity's default "Tight" mesh type crops transparent pixels from each sprite,
/// causing misalignment between base, talking, and eye blink frames.
/// This script sets all Alice sprites to "Full Rect" mode to preserve the full 640x720 canvas.
///
/// Unity 6 Note: Uses direct .meta file editing due to deprecated TextureImporter API.
///
/// Usage: Tools > Fix Alice Sprite Cropping
/// </summary>
public class FixAliceSpriteCropping
{
    [MenuItem("Tools/Fix Alice Sprite Cropping")]
    static void FixSprites()
    {
        string[] paths = {
            "Assets/Graphics/Characters/Alice/Base",
            "Assets/Graphics/Characters/Alice/Talking",
            "Assets/Graphics/Characters/Alice/Eyes"
        };

        int fixedCount = 0;

        foreach (string path in paths)
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });

            if (guids.Length == 0)
            {
                Debug.LogWarning($"FixAliceSpriteCropping: No textures found in {path}");
                continue;
            }

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string metaPath = assetPath + ".meta";

                if (!File.Exists(metaPath))
                {
                    Debug.LogWarning($"FixAliceSpriteCropping: No .meta file found for {assetPath}");
                    continue;
                }

                // Get texture to determine actual size
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture == null)
                {
                    Debug.LogWarning($"FixAliceSpriteCropping: Could not load texture at {assetPath}");
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
        }

        AssetDatabase.Refresh();
        Debug.Log($"<color=green>Finished fixing Alice sprite cropping! Fixed {fixedCount} textures.</color>");

        if (fixedCount == 0)
        {
            Debug.LogError("FixAliceSpriteCropping: No textures were fixed. Check that Alice frames exist in Assets/Graphics/Characters/Alice/");
        }
    }
}
