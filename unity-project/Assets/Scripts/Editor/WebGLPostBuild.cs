using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

/// <summary>
/// Post-build processor for WebGL builds.
/// After Unity builds, this script restores the custom website index.html from backup.
/// It does NOT touch play.html (which is a separate custom file).
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
        string backupPath = Path.Combine(buildPath, BACKUP_FILENAME);

        Debug.Log($"[WebGLPostBuild] Processing build at: {buildPath}");

        // Restore the website index.html from backup (Unity overwrites it with its template)
        if (File.Exists(backupPath))
        {
            File.Copy(backupPath, indexPath, true);
            Debug.Log($"[WebGLPostBuild] Restored website index.html from backup");
        }
        else
        {
            Debug.LogWarning($"[WebGLPostBuild] No backup found at {backupPath}. " +
                "Run 'Tools > WebGL > Backup Website Index.html' before building to preserve your website.\n" +
                "Unity's default index.html will be used (you may want to restore manually).");
        }

        // NOTE: We do NOT touch play.html - it's a separate custom file that references
        // the Build/ folder files. Update play.html manually if build filenames change.

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
