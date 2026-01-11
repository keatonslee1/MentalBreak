using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to generate Windows 95 style UI textures.
/// Run via Tools > Generate Win95 UI Assets
/// </summary>
public class Win95AssetGenerator : EditorWindow
{
    // Windows 95 Color Palette
    private static readonly Color32 ColorWhite = new Color32(255, 255, 255, 255);
    private static readonly Color32 ColorLightHighlight = new Color32(240, 240, 240, 255);
    private static readonly Color32 ColorLightGray = new Color32(223, 223, 223, 255);
    private static readonly Color32 ColorGray = new Color32(195, 195, 195, 255);       // #C3C3C3 - main background
    private static readonly Color32 ColorMidGray = new Color32(128, 128, 128, 255);    // #808080
    private static readonly Color32 ColorDarkGray = new Color32(126, 126, 126, 255);   // #7E7E7E
    private static readonly Color32 ColorDark = new Color32(38, 38, 38, 255);          // #262626
    private static readonly Color32 ColorBlack = new Color32(0, 0, 0, 255);
    private static readonly Color32 ColorTitleActive = new Color32(0, 0, 128, 255);    // Navy blue
    private static readonly Color32 ColorTitleInactive = new Color32(128, 128, 128, 255);
    private static readonly Color32 ColorTransparent = new Color32(0, 0, 0, 0);

    private const string OutputPath = "Assets/Graphics/UI/Win95";

    private static System.Collections.Generic.List<string> generatedFiles = new System.Collections.Generic.List<string>();

    [MenuItem("Tools/Generate Win95 UI Assets")]
    public static void GenerateAssets()
    {
        generatedFiles.Clear();

        // Ensure output directory exists
        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }

        GenerateButtonNormal();
        GenerateButtonPressed();
        GenerateButtonDisabled();
        GeneratePanelRaised();
        GeneratePanelInset();
        GenerateTitleBarActive();
        GenerateTitleBarInactive();
        GenerateTitleBarButtons();
        GenerateMenuBarBg();
        GenerateStatusBarBg();
        GenerateWindowBorder();
        GenerateProgressBarBg();
        GenerateProgressBarFill();
        GenerateCheckbox();
        GenerateWindowIcon();

        // Step 1: First refresh to import as default textures (avoids 2D Animation post-processor)
        foreach (string assetPath in generatedFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                // File not yet known to AssetDatabase, import it first as Default
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
                importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            }

            if (importer != null)
            {
                // Import as Default first to avoid 2D Animation post-processor
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // Step 2: Now convert to Sprite mode (2D Animation should handle this better)
        int successCount = 0;
        foreach (string assetPath in generatedFiles)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.spritePixelsPerUnit = 100;
                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
                successCount++;
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Debug.Log($"Win95 UI Assets generated successfully in {OutputPath} ({successCount}/{generatedFiles.Count} sprites configured)");
    }

    #region Button Textures

    private static void GenerateButtonNormal()
    {
        // 32x32 raised button for 9-slice (border: 4px each side)
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Fill with gray background
        FillRect(tex, 0, 0, size, size, ColorGray);

        // Raised 3D border (2px each layer)
        // Outer highlight (white) - top and left
        DrawHLine(tex, 0, size - 1, size, ColorWhite);
        DrawHLine(tex, 0, size - 2, size - 1, ColorWhite);
        DrawVLine(tex, 0, 0, size, ColorWhite);
        DrawVLine(tex, 1, 1, size - 1, ColorWhite);

        // Inner highlight (light gray)
        DrawHLine(tex, 2, size - 3, size - 2, ColorLightGray);
        DrawVLine(tex, 2, 2, size - 3, ColorLightGray);

        // Outer shadow (black) - bottom and right
        DrawHLine(tex, 0, 0, size, ColorBlack);
        DrawHLine(tex, 1, 1, size - 1, ColorBlack);
        DrawVLine(tex, size - 1, 0, size, ColorBlack);
        DrawVLine(tex, size - 2, 1, size - 1, ColorBlack);

        // Inner shadow (dark gray)
        DrawHLine(tex, 2, 2, size - 2, ColorMidGray);
        DrawVLine(tex, size - 3, 2, size - 2, ColorMidGray);

        tex.Apply();
        SaveTexture(tex, "win95_button_normal.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateButtonPressed()
    {
        // 32x32 pressed/sunken button for 9-slice
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Fill with slightly darker gray for pressed state
        FillRect(tex, 0, 0, size, size, ColorGray);

        // Sunken 3D border - reversed from raised
        // Outer shadow (black) - top and left
        DrawHLine(tex, 0, size - 1, size, ColorBlack);
        DrawHLine(tex, 0, size - 2, size - 1, ColorMidGray);
        DrawVLine(tex, 0, 0, size, ColorBlack);
        DrawVLine(tex, 1, 1, size - 1, ColorMidGray);

        // Outer highlight (white) - bottom and right
        DrawHLine(tex, 0, 0, size, ColorWhite);
        DrawHLine(tex, 1, 1, size - 1, ColorLightGray);
        DrawVLine(tex, size - 1, 0, size, ColorWhite);
        DrawVLine(tex, size - 2, 1, size - 1, ColorLightGray);

        tex.Apply();
        SaveTexture(tex, "win95_button_pressed.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateButtonDisabled()
    {
        // 32x32 disabled button - flat, no 3D effect
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Fill with gray background
        FillRect(tex, 0, 0, size, size, ColorGray);

        // Subtle raised border (less contrast)
        DrawHLine(tex, 0, size - 1, size, ColorLightGray);
        DrawVLine(tex, 0, 0, size, ColorLightGray);
        DrawHLine(tex, 0, 0, size, ColorMidGray);
        DrawVLine(tex, size - 1, 0, size, ColorMidGray);

        tex.Apply();
        SaveTexture(tex, "win95_button_disabled.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Panel Textures

    private static void GeneratePanelRaised()
    {
        // 32x32 raised panel for 9-slice
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        FillRect(tex, 0, 0, size, size, ColorGray);

        // 2px raised border
        // Top-left highlight
        DrawHLine(tex, 0, size - 1, size, ColorWhite);
        DrawHLine(tex, 1, size - 2, size - 1, ColorLightHighlight);
        DrawVLine(tex, 0, 0, size, ColorWhite);
        DrawVLine(tex, 1, 1, size - 1, ColorLightHighlight);

        // Bottom-right shadow
        DrawHLine(tex, 0, 0, size, ColorBlack);
        DrawHLine(tex, 1, 1, size - 1, ColorDarkGray);
        DrawVLine(tex, size - 1, 0, size, ColorBlack);
        DrawVLine(tex, size - 2, 1, size - 1, ColorDarkGray);

        tex.Apply();
        SaveTexture(tex, "win95_panel_raised.png");
        Object.DestroyImmediate(tex);
    }

    private static void GeneratePanelInset()
    {
        // 32x32 sunken/inset panel for 9-slice
        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        FillRect(tex, 0, 0, size, size, ColorGray);

        // 2px sunken border (reversed from raised)
        // Top-left shadow
        DrawHLine(tex, 0, size - 1, size, ColorMidGray);
        DrawHLine(tex, 1, size - 2, size - 1, ColorDark);
        DrawVLine(tex, 0, 0, size, ColorMidGray);
        DrawVLine(tex, 1, 1, size - 1, ColorDark);

        // Bottom-right highlight
        DrawHLine(tex, 0, 0, size, ColorWhite);
        DrawHLine(tex, 1, 1, size - 1, ColorLightHighlight);
        DrawVLine(tex, size - 1, 0, size, ColorWhite);
        DrawVLine(tex, size - 2, 1, size - 1, ColorLightHighlight);

        tex.Apply();
        SaveTexture(tex, "win95_panel_inset.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Title Bar Textures

    private static void GenerateTitleBarActive()
    {
        // 64x32 active title bar (navy blue gradient simulation)
        int width = 64;
        int height = 32;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Solid navy blue (Win95 didn't have gradients in the basic theme)
        FillRect(tex, 0, 0, width, height, ColorTitleActive);

        tex.Apply();
        SaveTexture(tex, "win95_titlebar_active.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateTitleBarInactive()
    {
        // 64x32 inactive title bar (gray)
        int width = 64;
        int height = 32;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        FillRect(tex, 0, 0, width, height, ColorTitleInactive);

        tex.Apply();
        SaveTexture(tex, "win95_titlebar_inactive.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateTitleBarButtons()
    {
        // 64x16 sprite sheet: minimize, maximize, close (each 16x16 with 4px padding = 20px, but we'll do 16x16 tight)
        // Actually let's do 48x16 with three 16x16 buttons
        int btnSize = 16;
        int width = btnSize * 3;
        int height = btnSize;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Draw three buttons side by side
        for (int i = 0; i < 3; i++)
        {
            int xOffset = i * btnSize;

            // Button background with raised border
            FillRect(tex, xOffset, 0, btnSize, btnSize, ColorGray);

            // Raised border (1px)
            DrawHLine(tex, xOffset, height - 1, xOffset + btnSize - 1, ColorWhite);
            DrawVLine(tex, xOffset, 0, height, ColorWhite);
            DrawHLine(tex, xOffset, 0, xOffset + btnSize, ColorBlack);
            DrawVLine(tex, xOffset + btnSize - 1, 0, height, ColorBlack);

            // Draw icon
            int cx = xOffset + btnSize / 2;
            int cy = height / 2;

            if (i == 0) // Minimize - horizontal line at bottom
            {
                DrawHLine(tex, xOffset + 4, 3, xOffset + btnSize - 4, ColorBlack);
                DrawHLine(tex, xOffset + 4, 4, xOffset + btnSize - 4, ColorBlack);
            }
            else if (i == 1) // Maximize - square outline
            {
                // Outer rectangle
                DrawHLine(tex, xOffset + 3, height - 4, xOffset + btnSize - 3, ColorBlack);
                DrawHLine(tex, xOffset + 3, height - 5, xOffset + btnSize - 3, ColorBlack);
                DrawHLine(tex, xOffset + 3, 3, xOffset + btnSize - 3, ColorBlack);
                DrawVLine(tex, xOffset + 3, 3, height - 3, ColorBlack);
                DrawVLine(tex, xOffset + btnSize - 4, 3, height - 3, ColorBlack);
            }
            else // Close - X
            {
                // Draw X with 2px thick lines
                for (int d = 0; d < 8; d++)
                {
                    tex.SetPixel(xOffset + 4 + d, 4 + d, ColorBlack);
                    tex.SetPixel(xOffset + 5 + d, 4 + d, ColorBlack);
                    tex.SetPixel(xOffset + 4 + d, 5 + d, ColorBlack);
                    tex.SetPixel(xOffset + btnSize - 5 - d, 4 + d, ColorBlack);
                    tex.SetPixel(xOffset + btnSize - 6 - d, 4 + d, ColorBlack);
                    tex.SetPixel(xOffset + btnSize - 5 - d, 5 + d, ColorBlack);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, "win95_titlebar_buttons.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Bar Textures

    private static void GenerateMenuBarBg()
    {
        // Simple gray bar with bottom border
        int width = 32;
        int height = 24;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        FillRect(tex, 0, 0, width, height, ColorGray);

        // Bottom separator line (sunken)
        DrawHLine(tex, 0, 0, width, ColorMidGray);
        DrawHLine(tex, 0, 1, width, ColorWhite);

        tex.Apply();
        SaveTexture(tex, "win95_menubar_bg.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateStatusBarBg()
    {
        // Status bar with top border (sunken sections)
        int width = 32;
        int height = 24;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        FillRect(tex, 0, 0, width, height, ColorGray);

        // Top highlight line
        DrawHLine(tex, 0, height - 1, width, ColorWhite);

        tex.Apply();
        SaveTexture(tex, "win95_statusbar_bg.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateWindowBorder()
    {
        // 8x8 corner piece for window frame
        int size = 8;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        FillRect(tex, 0, 0, size, size, ColorGray);

        // Outer raised border
        DrawHLine(tex, 0, size - 1, size, ColorWhite);
        DrawVLine(tex, 0, 0, size, ColorWhite);
        DrawHLine(tex, 0, 0, size, ColorBlack);
        DrawVLine(tex, size - 1, 0, size, ColorBlack);

        tex.Apply();
        SaveTexture(tex, "win95_window_border.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Progress Bar Textures

    private static void GenerateProgressBarBg()
    {
        // Sunken progress bar background
        int width = 32;
        int height = 20;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // White/light fill (progress bars have white bg in Win95)
        FillRect(tex, 0, 0, width, height, ColorWhite);

        // Sunken border
        DrawHLine(tex, 0, height - 1, width, ColorMidGray);
        DrawHLine(tex, 1, height - 2, width - 1, ColorDark);
        DrawVLine(tex, 0, 0, height, ColorMidGray);
        DrawVLine(tex, 1, 1, height - 1, ColorDark);

        DrawHLine(tex, 0, 0, width, ColorWhite);
        DrawHLine(tex, 1, 1, width - 1, ColorLightHighlight);
        DrawVLine(tex, width - 1, 0, height, ColorWhite);
        DrawVLine(tex, width - 2, 1, height - 1, ColorLightHighlight);

        tex.Apply();
        SaveTexture(tex, "win95_progress_bg.png");
        Object.DestroyImmediate(tex);
    }

    private static void GenerateProgressBarFill()
    {
        // Blue segmented fill for progress bar
        int width = 12;
        int height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Navy blue fill
        FillRect(tex, 0, 0, width, height, ColorTitleActive);

        tex.Apply();
        SaveTexture(tex, "win95_progress_fill.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Checkbox

    private static void GenerateCheckbox()
    {
        // 32x16: unchecked (left 16x16), checked (right 16x16)
        int size = 16;
        int width = size * 2;
        Texture2D tex = new Texture2D(width, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Both checkboxes have white interior with sunken border
        for (int i = 0; i < 2; i++)
        {
            int xOffset = i * size;

            // White fill
            FillRect(tex, xOffset + 2, 2, size - 4, size - 4, ColorWhite);

            // Sunken border
            DrawHLine(tex, xOffset, size - 1, xOffset + size - 1, ColorMidGray);
            DrawHLine(tex, xOffset + 1, size - 2, xOffset + size - 2, ColorDark);
            DrawVLine(tex, xOffset, 0, size, ColorMidGray);
            DrawVLine(tex, xOffset + 1, 1, size - 1, ColorDark);

            DrawHLine(tex, xOffset, 0, xOffset + size, ColorWhite);
            DrawHLine(tex, xOffset + 1, 1, xOffset + size - 1, ColorLightHighlight);
            DrawVLine(tex, xOffset + size - 1, 0, size, ColorWhite);
            DrawVLine(tex, xOffset + size - 2, 1, size - 1, ColorLightHighlight);

            // Checkmark for second one
            if (i == 1)
            {
                // Draw checkmark
                int[] checkX = { 4, 5, 6, 7, 8, 9, 10, 11 };
                int[] checkY = { 7, 8, 9, 10, 9, 8, 7, 6 };
                for (int j = 0; j < checkX.Length; j++)
                {
                    tex.SetPixel(xOffset + checkX[j], checkY[j], ColorBlack);
                    tex.SetPixel(xOffset + checkX[j], checkY[j] - 1, ColorBlack);
                }
            }
        }

        tex.Apply();
        SaveTexture(tex, "win95_checkbox.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Window Icon

    private static void GenerateWindowIcon()
    {
        // 16x16 small computer/app icon
        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // Transparent background
        FillRect(tex, 0, 0, size, size, ColorTransparent);

        // Simple monitor shape
        // Screen (blue rectangle)
        FillRect(tex, 2, 5, 12, 8, ColorTitleActive);

        // Screen border (gray)
        FillRect(tex, 1, 4, 14, 10, ColorGray);
        FillRect(tex, 2, 5, 12, 8, ColorTitleActive);

        // Monitor base
        FillRect(tex, 5, 2, 6, 2, ColorGray);
        FillRect(tex, 4, 1, 8, 1, ColorMidGray);

        // Screen shine (white pixel in corner)
        tex.SetPixel(3, 11, ColorWhite);
        tex.SetPixel(4, 11, ColorLightGray);

        tex.Apply();
        SaveTexture(tex, "win95_window_icon.png");
        Object.DestroyImmediate(tex);
    }

    #endregion

    #region Helper Methods

    private static void FillRect(Texture2D tex, int x, int y, int width, int height, Color32 color)
    {
        for (int px = x; px < x + width && px < tex.width; px++)
        {
            for (int py = y; py < y + height && py < tex.height; py++)
            {
                tex.SetPixel(px, py, color);
            }
        }
    }

    private static void DrawHLine(Texture2D tex, int x1, int y, int x2, Color32 color)
    {
        for (int x = x1; x < x2 && x < tex.width; x++)
        {
            if (y >= 0 && y < tex.height)
                tex.SetPixel(x, y, color);
        }
    }

    private static void DrawVLine(Texture2D tex, int x, int y1, int y2, Color32 color)
    {
        for (int y = y1; y < y2 && y < tex.height; y++)
        {
            if (x >= 0 && x < tex.width)
                tex.SetPixel(x, y, color);
        }
    }

    private static void SaveTexture(Texture2D tex, string filename)
    {
        byte[] bytes = tex.EncodeToPNG();
        string fullPath = Path.Combine(OutputPath, filename);
        File.WriteAllBytes(fullPath, bytes);
        generatedFiles.Add(fullPath);
        Debug.Log($"Saved: {fullPath}");
    }

    #endregion
}
