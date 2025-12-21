using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

/// <summary>
/// Post-build processor for WebGL builds.
/// After Unity builds, this script:
/// 1. Renames the generated index.html to play.html (the game page)
/// 2. Restores the original website index.html from backup
///
/// This prevents Unity from overwriting your custom website landing page.
/// </summary>
public class WebGLPostBuild : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    // Path to backup of the original website index.html
    private const string BACKUP_FILENAME = "index.html.website-backup";

    public void OnPostprocessBuild(BuildReport report)
    {
        // Only process WebGL builds
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        string buildPath = report.summary.outputPath;
        string indexPath = Path.Combine(buildPath, "index.html");
        string playPath = Path.Combine(buildPath, "play.html");
        string backupPath = Path.Combine(buildPath, BACKUP_FILENAME);

        Debug.Log($"[WebGLPostBuild] Processing build at: {buildPath}");

        // Step 1: If play.html exists and is the old version, back it up
        if (File.Exists(playPath))
        {
            string playBackup = Path.Combine(buildPath, "play.html.old");
            File.Copy(playPath, playBackup, true);
            Debug.Log($"[WebGLPostBuild] Backed up existing play.html to play.html.old");
        }

        // Step 2: Rename Unity's new index.html to play.html
        if (File.Exists(indexPath))
        {
            // Check if this is Unity's generated file (contains Unity template markers)
            string content = File.ReadAllText(indexPath);

            // If backup exists, restore the website index.html
            if (File.Exists(backupPath))
            {
                // First, move Unity's index.html to play.html
                File.Copy(indexPath, playPath, true);
                Debug.Log($"[WebGLPostBuild] Copied Unity's index.html to play.html");

                // Then restore the website backup
                File.Copy(backupPath, indexPath, true);
                Debug.Log($"[WebGLPostBuild] Restored website index.html from backup");
            }
            else
            {
                Debug.LogWarning($"[WebGLPostBuild] No backup found at {backupPath}. " +
                    "Run 'Tools > Backup Website Index.html' before building to preserve your website.");
            }
        }

        Debug.Log("[WebGLPostBuild] Build post-processing complete!");
    }

    [MenuItem("Tools/WebGL/Backup Website Index.html")]
    public static void BackupWebsiteIndex()
    {
        // Find the webgl-build folder
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string buildPath = Path.Combine(projectPath, "webgl-build");
        string indexPath = Path.Combine(buildPath, "index.html");
        string backupPath = Path.Combine(buildPath, BACKUP_FILENAME);

        if (!File.Exists(indexPath))
        {
            EditorUtility.DisplayDialog("Backup Failed",
                $"index.html not found at:\n{indexPath}", "OK");
            return;
        }

        File.Copy(indexPath, backupPath, true);
        Debug.Log($"[WebGLPostBuild] Backed up index.html to {backupPath}");
        EditorUtility.DisplayDialog("Backup Complete",
            $"Website index.html backed up to:\n{BACKUP_FILENAME}\n\n" +
            "This will be automatically restored after each WebGL build.", "OK");
    }

    [MenuItem("Tools/WebGL/Restore Website Index.html")]
    public static void RestoreWebsiteIndex()
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string buildPath = Path.Combine(projectPath, "webgl-build");
        string indexPath = Path.Combine(buildPath, "index.html");
        string backupPath = Path.Combine(buildPath, BACKUP_FILENAME);

        if (!File.Exists(backupPath))
        {
            EditorUtility.DisplayDialog("Restore Failed",
                $"Backup not found at:\n{backupPath}\n\n" +
                "Run 'Tools > WebGL > Backup Website Index.html' first.", "OK");
            return;
        }

        File.Copy(backupPath, indexPath, true);
        Debug.Log($"[WebGLPostBuild] Restored index.html from {backupPath}");
        EditorUtility.DisplayDialog("Restore Complete",
            "Website index.html has been restored from backup.", "OK");
    }
}
