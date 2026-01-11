using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically configures import settings for Win95 UI sprites.
/// Sets up 9-slice borders, point filtering, and proper texture settings.
/// </summary>
public class Win95SpriteImporter : AssetPostprocessor
{
    private const string Win95Path = "Assets/Graphics/UI/Win95/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Win95Path))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;

        // Basic settings for pixel art UI
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 1; // 1:1 pixel mapping
        importer.filterMode = FilterMode.Point; // No filtering for crisp pixels
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spriteImportMode = SpriteImportMode.Single;

        // Set 9-slice borders based on filename
        ConfigureSlicing(importer, assetPath);
    }

    private void ConfigureSlicing(TextureImporter importer, string path)
    {
        string filename = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();

        // Define border settings for each asset type
        // Border order: left, bottom, right, top
        Vector4 border = Vector4.zero;

        if (filename.Contains("button"))
        {
            // 4px border all around for buttons
            border = new Vector4(4, 4, 4, 4);
        }
        else if (filename.Contains("panel"))
        {
            // 4px border for panels
            border = new Vector4(4, 4, 4, 4);
        }
        else if (filename.Contains("progress_bg"))
        {
            // 2px border for progress bar background
            border = new Vector4(4, 4, 4, 4);
        }
        else if (filename.Contains("titlebar") && !filename.Contains("buttons"))
        {
            // Title bar stretches horizontally, keep ends intact
            border = new Vector4(4, 0, 4, 0);
        }
        else if (filename.Contains("menubar") || filename.Contains("statusbar"))
        {
            // Bars stretch horizontally
            border = new Vector4(4, 2, 4, 2);
        }
        else if (filename.Contains("checkbox"))
        {
            // Checkbox is a sprite sheet - handle as multiple
            importer.spriteImportMode = SpriteImportMode.Multiple;
            return;
        }
        else if (filename.Contains("titlebar_buttons"))
        {
            // Title bar buttons are a sprite sheet
            importer.spriteImportMode = SpriteImportMode.Multiple;
            return;
        }

        // Apply border for 9-slice
        if (border != Vector4.zero)
        {
            importer.spriteBorder = border;
        }
    }

    void OnPostprocessTexture(Texture2D texture)
    {
        if (!assetPath.StartsWith(Win95Path))
            return;

        string filename = System.IO.Path.GetFileNameWithoutExtension(assetPath).ToLower();

        // Set up sprite sheet slicing for multi-sprite assets
        if (filename.Contains("checkbox") || filename.Contains("titlebar_buttons"))
        {
            SetupSpriteSheet(filename);
        }
    }

    private void SetupSpriteSheet(string filename)
    {
        TextureImporter importer = (TextureImporter)assetImporter;

        if (filename.Contains("checkbox"))
        {
            // 32x16 with two 16x16 sprites
            SpriteMetaData[] spritesheet = new SpriteMetaData[2];

            spritesheet[0] = new SpriteMetaData
            {
                name = "win95_checkbox_unchecked",
                rect = new Rect(0, 0, 16, 16),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };

            spritesheet[1] = new SpriteMetaData
            {
                name = "win95_checkbox_checked",
                rect = new Rect(16, 0, 16, 16),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };

            importer.spritesheet = spritesheet;
        }
        else if (filename.Contains("titlebar_buttons"))
        {
            // 48x16 with three 16x16 sprites
            SpriteMetaData[] spritesheet = new SpriteMetaData[3];

            spritesheet[0] = new SpriteMetaData
            {
                name = "win95_btn_minimize",
                rect = new Rect(0, 0, 16, 16),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };

            spritesheet[1] = new SpriteMetaData
            {
                name = "win95_btn_maximize",
                rect = new Rect(16, 0, 16, 16),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };

            spritesheet[2] = new SpriteMetaData
            {
                name = "win95_btn_close",
                rect = new Rect(32, 0, 16, 16),
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };

            importer.spritesheet = spritesheet;
        }
    }
}

/// <summary>
/// Menu item to manually re-import Win95 sprites with correct settings.
/// </summary>
public static class Win95SpriteReimporter
{
    [MenuItem("Tools/Reimport Win95 Sprites")]
    public static void ReimportWin95Sprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Graphics/UI/Win95" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log($"Reimported {guids.Length} Win95 sprites.");
    }
}
