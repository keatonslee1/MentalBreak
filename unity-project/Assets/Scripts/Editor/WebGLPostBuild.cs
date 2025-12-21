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
}
