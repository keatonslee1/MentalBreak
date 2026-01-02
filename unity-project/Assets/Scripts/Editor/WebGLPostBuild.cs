using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Pre and Post-build processor for WebGL builds.
///
/// BEFORE build: Backs up all custom website files to a temporary folder
/// AFTER build: Restores the custom website files that Unity would have deleted
///
/// This preserves your custom website (index.html, play.html, about.html, etc.)
/// while still allowing Unity to build the game files.
/// </summary>
public class WebGLPostBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    // Backup folder name (inside the build folder)
    private const string BACKUP_FOLDER = ".website-backup";

    // Character portrait paths for WebGL builds
    private const string CHARACTER_SOURCE_PATH = "Assets/Graphics/Characters";
    private const string CHARACTER_DEST_PATH = "Assets/Resources/Graphics/Characters";

    // Track copied character folders for cleanup
    private static List<string> copiedCharacterFolders = new List<string>();

    // Files to preserve (relative to build folder)
    private static readonly string[] WEBSITE_FILES = new string[]
    {
        "index.html",
        "play.html",
        "mobile-play.html",
        "about.html",
        "blog.html",
    };

    // Folders to preserve (relative to build folder)
    private static readonly string[] WEBSITE_FOLDERS = new string[]
    {
        "assets",  // CSS, JS, images for the website
    };

    /// <summary>
    /// Called BEFORE Unity builds - backup website files
    /// </summary>
    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        string buildPath = report.summary.outputPath;
        string backupPath = Path.Combine(buildPath, BACKUP_FOLDER);

        // Only backup if build folder exists (not first build)
        if (!Directory.Exists(buildPath))
        {
            Debug.Log("[WebGLPostBuild] First build - no files to backup");
            return;
        }

        Debug.Log($"[WebGLPostBuild] Backing up website files before build...");

        // Create backup folder
        if (Directory.Exists(backupPath))
        {
            Directory.Delete(backupPath, true);
        }
        Directory.CreateDirectory(backupPath);

        int backedUp = 0;

        // Backup individual files
        foreach (string file in WEBSITE_FILES)
        {
            string sourcePath = Path.Combine(buildPath, file);
            string destPath = Path.Combine(backupPath, file);

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, true);
                backedUp++;
                Debug.Log($"[WebGLPostBuild] Backed up: {file}");
            }
        }

        // Backup folders
        foreach (string folder in WEBSITE_FOLDERS)
        {
            string sourcePath = Path.Combine(buildPath, folder);
            string destPath = Path.Combine(backupPath, folder);

            if (Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, destPath);
                backedUp++;
                Debug.Log($"[WebGLPostBuild] Backed up folder: {folder}/");
            }
        }

        Debug.Log($"[WebGLPostBuild] Backed up {backedUp} items to {BACKUP_FOLDER}/");

        // Copy character portraits to Resources for WebGL builds
        Debug.Log("[WebGLPostBuild] Copying character portraits to Resources...");
        CopyCharacterPortraitsToResources();
    }

    /// <summary>
    /// Called AFTER Unity builds - restore website files
    /// </summary>
    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        string buildPath = report.summary.outputPath;
        string backupPath = Path.Combine(buildPath, BACKUP_FOLDER);

        Debug.Log($"[WebGLPostBuild] Restoring website files after build...");

        if (!Directory.Exists(backupPath))
        {
            Debug.LogWarning($"[WebGLPostBuild] No backup found at {backupPath}. " +
                "Website files may need to be restored manually from git.");
            return;
        }

        int restored = 0;

        // Restore individual files
        foreach (string file in WEBSITE_FILES)
        {
            string sourcePath = Path.Combine(backupPath, file);
            string destPath = Path.Combine(buildPath, file);

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, true);
                restored++;
                Debug.Log($"[WebGLPostBuild] Restored: {file}");
            }
        }

        // Restore folders
        foreach (string folder in WEBSITE_FOLDERS)
        {
            string sourcePath = Path.Combine(backupPath, folder);
            string destPath = Path.Combine(buildPath, folder);

            if (Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, destPath);
                restored++;
                Debug.Log($"[WebGLPostBuild] Restored folder: {folder}/");
            }
        }

        // Clean up backup folder
        try
        {
            Directory.Delete(backupPath, true);
            Debug.Log($"[WebGLPostBuild] Cleaned up backup folder");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[WebGLPostBuild] Could not delete backup folder: {e.Message}");
        }

        Debug.Log($"[WebGLPostBuild] Restored {restored} items. Build complete!");

        // Clean up copied character portraits
        Debug.Log("[WebGLPostBuild] Cleaning up character portraits...");
        CleanupCharacterPortraits();
    }

    /// <summary>
    /// Recursively copy a directory
    /// </summary>
    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destDir, fileName);
            File.Copy(file, destFile, true);
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, dirName);
            CopyDirectory(subDir, destSubDir);
        }
    }

    // ============ Character Portrait Copying for WebGL ============

    /// <summary>
    /// Detects characters with multi-frame portrait structure (Base/, Talking/, Eyes/ folders)
    /// </summary>
    private static List<string> DetectMultiFrameCharacters()
    {
        List<string> characters = new List<string>();

        if (!AssetDatabase.IsValidFolder(CHARACTER_SOURCE_PATH))
        {
            Debug.LogWarning($"[CharacterPortraitCopy] Source path not found: {CHARACTER_SOURCE_PATH}");
            return characters;
        }

        // Get all top-level folders in Graphics/Characters/
        string[] guids = AssetDatabase.FindAssets("t:Folder", new[] { CHARACTER_SOURCE_PATH });

        foreach (string guid in guids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(guid);

            // Skip if not direct child (e.g., skip Alice/Base/, Alice/Talking/)
            string relativePath = folderPath.Replace(CHARACTER_SOURCE_PATH + "/", "");
            if (relativePath.Contains("/")) continue;

            string folderName = Path.GetFileName(folderPath);

            // Skip utility folders
            if (folderName == "backup_old" || folderName == "frames to be implemented")
                continue;

            // Check for multi-frame structure
            if (IsMultiFrameCharacter(folderPath))
            {
                characters.Add(folderName);
                Debug.Log($"[CharacterPortraitCopy] Detected multi-frame character: {folderName}");
            }
        }

        return characters;
    }

    /// <summary>
    /// Checks if a character folder has multi-frame structure (any of Base/, Talking/, Eyes/)
    /// </summary>
    private static bool IsMultiFrameCharacter(string characterFolderPath)
    {
        bool hasBase = AssetDatabase.IsValidFolder($"{characterFolderPath}/Base");
        bool hasTalking = AssetDatabase.IsValidFolder($"{characterFolderPath}/Talking");
        bool hasEyes = AssetDatabase.IsValidFolder($"{characterFolderPath}/Eyes");

        // Character is multi-frame if it has ANY of these subfolders
        return hasBase || hasTalking || hasEyes;
    }

    /// <summary>
    /// Ensures a folder exists in the AssetDatabase, creating parent folders recursively if needed
    /// </summary>
    private static void EnsureFolderExists(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            // Create parent folders recursively
            string parentPath = Path.GetDirectoryName(folderPath).Replace("\\", "/");
            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolderExists(parentPath);
            }

            string folderName = Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parentPath, folderName);
            Debug.Log($"[CharacterPortraitCopy] Created folder: {folderPath}");
        }
    }

    /// <summary>
    /// Copies all sprites from source folder to destination folder, including .meta files
    /// </summary>
    private static void CopySpritesInFolder(string sourceFolder, string destFolder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { sourceFolder });

        int copiedCount = 0;
        foreach (string guid in guids)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(sourcePath);
            string destPath = Path.Combine(destFolder, fileName).Replace("\\", "/");

            // Copy sprite file
            AssetDatabase.CopyAsset(sourcePath, destPath);

            // CRITICAL: Copy .meta file to preserve Unity settings
            string sourceMetaPath = sourcePath + ".meta";
            string destMetaPath = destPath + ".meta";
            if (File.Exists(sourceMetaPath))
            {
                File.Copy(sourceMetaPath, destMetaPath, true);
            }

            copiedCount++;
        }

        Debug.Log($"[CharacterPortraitCopy] Copied {copiedCount} sprites from {sourceFolder}");
    }

    /// <summary>
    /// Copies a single character's portrait frames (Base/, Talking/, Eyes/ subfolders)
    /// </summary>
    private static void CopyCharacterFolder(string characterName)
    {
        string sourcePath = $"{CHARACTER_SOURCE_PATH}/{characterName}";
        string destPath = $"{CHARACTER_DEST_PATH}/{characterName}";

        // Ensure character folder exists in Resources
        EnsureFolderExists(destPath);

        // Copy each subfolder (Base, Talking, Eyes)
        string[] subfolders = { "Base", "Talking", "Eyes" };

        foreach (string subfolder in subfolders)
        {
            string sourceSubPath = $"{sourcePath}/{subfolder}";
            string destSubPath = $"{destPath}/{subfolder}";

            if (AssetDatabase.IsValidFolder(sourceSubPath))
            {
                // Ensure subfolder exists
                EnsureFolderExists(destSubPath);

                // Copy all sprite files
                CopySpritesInFolder(sourceSubPath, destSubPath);
            }
        }

        // Track for cleanup
        copiedCharacterFolders.Add(characterName);
        Debug.Log($"[CharacterPortraitCopy] Copied character: {characterName}");
    }

    /// <summary>
    /// Main method to copy all multi-frame character portraits to Resources (called in OnPreprocessBuild)
    /// </summary>
    private static void CopyCharacterPortraitsToResources()
    {
        copiedCharacterFolders.Clear();

        List<string> characters = DetectMultiFrameCharacters();

        if (characters.Count == 0)
        {
            Debug.Log("[CharacterPortraitCopy] No multi-frame characters detected");
            return;
        }

        Debug.Log($"[CharacterPortraitCopy] Copying {characters.Count} character(s) to Resources...");

        // Ensure destination folder exists
        EnsureFolderExists(CHARACTER_DEST_PATH);

        foreach (string characterName in characters)
        {
            CopyCharacterFolder(characterName);
        }

        // CRITICAL: Refresh AssetDatabase so Unity sees new files
        AssetDatabase.Refresh();

        Debug.Log($"[CharacterPortraitCopy] Copied {copiedCharacterFolders.Count} character(s) to Resources");
    }

    /// <summary>
    /// Cleans up copied character portraits from Resources (called in OnPostprocessBuild)
    /// </summary>
    private static void CleanupCharacterPortraits()
    {
        if (copiedCharacterFolders.Count == 0)
        {
            Debug.Log("[CharacterPortraitCopy] No portraits to clean up");
            return;
        }

        Debug.Log($"[CharacterPortraitCopy] Cleaning up {copiedCharacterFolders.Count} copied character(s)...");

        foreach (string characterName in copiedCharacterFolders)
        {
            string destPath = $"{CHARACTER_DEST_PATH}/{characterName}";

            if (AssetDatabase.IsValidFolder(destPath))
            {
                AssetDatabase.DeleteAsset(destPath);
                Debug.Log($"[CharacterPortraitCopy] Deleted: {destPath}");
            }
        }

        // Clean up empty parent folders
        if (AssetDatabase.IsValidFolder(CHARACTER_DEST_PATH))
        {
            string[] remainingFolders = AssetDatabase.FindAssets("t:Folder", new[] { CHARACTER_DEST_PATH });
            if (remainingFolders.Length == 0)
            {
                // No other characters, safe to delete Graphics/Characters
                AssetDatabase.DeleteAsset(CHARACTER_DEST_PATH);
                Debug.Log($"[CharacterPortraitCopy] Deleted empty folder: {CHARACTER_DEST_PATH}");

                // Check if Graphics folder is now empty
                string graphicsPath = "Assets/Resources/Graphics";
                if (AssetDatabase.IsValidFolder(graphicsPath))
                {
                    string[] remainingInGraphics = AssetDatabase.FindAssets("", new[] { graphicsPath });
                    if (remainingInGraphics.Length == 0)
                    {
                        AssetDatabase.DeleteAsset(graphicsPath);
                        Debug.Log($"[CharacterPortraitCopy] Deleted empty folder: {graphicsPath}");
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        copiedCharacterFolders.Clear();

        Debug.Log("[CharacterPortraitCopy] Cleanup complete");
    }

    // ============ Menu Items ============

    [MenuItem("Tools/WebGL/Show Protected Website Files")]
    public static void ShowProtectedFiles()
    {
        string message = "The following files/folders are automatically preserved during WebGL builds:\n\n";

        message += "FILES:\n";
        foreach (string file in WEBSITE_FILES)
        {
            message += $"  • {file}\n";
        }

        message += "\nFOLDERS:\n";
        foreach (string folder in WEBSITE_FOLDERS)
        {
            message += $"  • {folder}/\n";
        }

        message += "\nThese are backed up before build and restored after.";

        EditorUtility.DisplayDialog("Protected Website Files", message, "OK");
        Debug.Log("[WebGLPostBuild] " + message.Replace("\n", "\n[WebGLPostBuild] "));
    }

    [MenuItem("Tools/WebGL/Verify Website Files")]
    public static void VerifyWebsiteFiles()
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string buildPath = Path.Combine(projectPath, "webgl-build");

        if (!Directory.Exists(buildPath))
        {
            EditorUtility.DisplayDialog("Verify Failed",
                "webgl-build folder not found. Build the game first.", "OK");
            return;
        }

        string report = "Website Files Status:\n\n";
        bool allGood = true;

        foreach (string file in WEBSITE_FILES)
        {
            string filePath = Path.Combine(buildPath, file);
            if (File.Exists(filePath))
            {
                report += $"✓ {file}\n";
            }
            else
            {
                report += $"✗ {file} (MISSING)\n";
                allGood = false;
            }
        }

        report += "\n";
        foreach (string folder in WEBSITE_FOLDERS)
        {
            string folderPath = Path.Combine(buildPath, folder);
            if (Directory.Exists(folderPath))
            {
                int fileCount = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Length;
                report += $"✓ {folder}/ ({fileCount} files)\n";
            }
            else
            {
                report += $"✗ {folder}/ (MISSING)\n";
                allGood = false;
            }
        }

        if (allGood)
        {
            report += "\nAll website files are present!";
        }
        else
        {
            report += "\nSome files are missing. Restore from git or rebuild.";
        }

        Debug.Log("[WebGLPostBuild] " + report.Replace("\n", "\n[WebGLPostBuild] "));
        EditorUtility.DisplayDialog("Website Files Status", report, "OK");
    }

    [MenuItem("Tools/WebGL/List Multi-Frame Characters")]
    public static void ListMultiFrameCharacters()
    {
        List<string> characters = DetectMultiFrameCharacters();

        string message = characters.Count > 0
            ? $"Found {characters.Count} multi-frame character(s):\n\n" + string.Join("\n", characters.Select(c => $"• {c}"))
            : "No multi-frame characters detected.\n\nCharacters must have Base/, Talking/, or Eyes/ subfolders in:\n" + CHARACTER_SOURCE_PATH;

        EditorUtility.DisplayDialog("Multi-Frame Characters", message, "OK");
        Debug.Log("[CharacterPortraitCopy] " + message.Replace("\n", " "));
    }

    [MenuItem("Tools/WebGL/Copy Character Portraits to Resources")]
    public static void ManualCopyCharacterPortraits()
    {
        CopyCharacterPortraitsToResources();

        string message = copiedCharacterFolders.Count > 0
            ? $"Copied {copiedCharacterFolders.Count} character(s) to Resources:\n\n" +
              string.Join("\n", copiedCharacterFolders.Select(c => $"• {c}")) +
              $"\n\nLocation: {CHARACTER_DEST_PATH}\n\nTo clean up: Tools > WebGL > Clean Up Character Portraits"
            : "No multi-frame characters found to copy.";

        EditorUtility.DisplayDialog("Character Portraits Copied", message, "OK");
    }

    [MenuItem("Tools/WebGL/Clean Up Character Portraits")]
    public static void ManualCleanupCharacterPortraits()
    {
        // Manually populate list from existing folders
        copiedCharacterFolders.Clear();
        if (AssetDatabase.IsValidFolder(CHARACTER_DEST_PATH))
        {
            string[] guids = AssetDatabase.FindAssets("t:Folder", new[] { CHARACTER_DEST_PATH });
            foreach (string guid in guids)
            {
                string folderPath = AssetDatabase.GUIDToAssetPath(guid);
                string folderName = Path.GetFileName(folderPath);

                // Only add direct children
                string relativePath = folderPath.Replace(CHARACTER_DEST_PATH + "/", "");
                if (!relativePath.Contains("/"))
                {
                    copiedCharacterFolders.Add(folderName);
                }
            }
        }

        int cleanedCount = copiedCharacterFolders.Count;
        CleanupCharacterPortraits();

        string message = cleanedCount > 0
            ? $"Removed {cleanedCount} character(s) from Resources."
            : "No character portraits found in Resources to clean up.";

        EditorUtility.DisplayDialog("Character Portraits Cleaned", message, "OK");
    }
}
