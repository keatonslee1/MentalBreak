using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to clear all save data.
/// Usage: Unity Menu -> Tools -> Clear All Saves
/// </summary>
public class SaveDataCleaner : EditorWindow
{
    // PlayerPrefs keys used by SaveLoadManager
    private static readonly string[] SaveKeys = {
        "MB_Save_v2_Auto_0_Json",   // Autosave slot 0 (newest)
        "MB_Save_v2_Auto_1_Json",   // Autosave slot -1 (middle)
        "MB_Save_v2_Auto_2_Json",   // Autosave slot -2 (oldest)
        "MB_Save_v2_Slot_1_Json",   // Manual slot 1
        "MB_Save_v2_Slot_2_Json",   // Manual slot 2
        "MB_Save_v2_Slot_3_Json",   // Manual slot 3
        "MB_Save_v2_Slot_4_Json",   // Manual slot 4
        "MB_Save_v2_Slot_5_Json",   // Manual slot 5
        "MB_Save_v2_LastSaveSlot"   // Last save slot metadata
    };

    // Save file names for desktop builds
    private static readonly string[] SaveFileNames = {
        "autosave_0.json",
        "autosave_1.json",
        "autosave_2.json",
        "slot1.json",
        "slot2.json",
        "slot3.json",
        "slot4.json",
        "slot5.json"
    };

    [MenuItem("Tools/Clear All Saves")]
    public static void ClearAllSaves()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Clear All Saves",
            "This will delete ALL save data:\n\n" +
            "- 3 Autosave slots\n" +
            "- 5 Manual save slots\n\n" +
            "This action cannot be undone!",
            "Delete All",
            "Cancel"
        );

        if (!confirmed)
        {
            Debug.Log("[SaveDataCleaner] Operation cancelled by user.");
            return;
        }

        int prefsCleared = 0;
        int filesDeleted = 0;

        // Clear PlayerPrefs keys
        foreach (string key in SaveKeys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                prefsCleared++;
                Debug.Log($"[SaveDataCleaner] Deleted PlayerPrefs key: {key}");
            }
        }
        PlayerPrefs.Save();

        // Clear save files from persistentDataPath
        string savesDirectory = Path.Combine(Application.persistentDataPath, "saves");
        if (Directory.Exists(savesDirectory))
        {
            foreach (string fileName in SaveFileNames)
            {
                string filePath = Path.Combine(savesDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    filesDeleted++;
                    Debug.Log($"[SaveDataCleaner] Deleted file: {filePath}");
                }
            }
        }

        // Summary
        string summary = $"Save data cleared!\n\n" +
            $"PlayerPrefs keys deleted: {prefsCleared}\n" +
            $"Save files deleted: {filesDeleted}";

        Debug.Log($"[SaveDataCleaner] {summary.Replace("\n", " ")}");

        EditorUtility.DisplayDialog("Saves Cleared", summary, "OK");
    }

    [MenuItem("Tools/Show Save Data Info")]
    public static void ShowSaveDataInfo()
    {
        string info = "=== Save Data Info ===\n\n";

        // Check PlayerPrefs
        info += "PlayerPrefs Keys:\n";
        int prefsFound = 0;
        foreach (string key in SaveKeys)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key, "");
                int length = value.Length;
                info += $"  {key}: {length} chars\n";
                prefsFound++;
            }
        }
        if (prefsFound == 0)
        {
            info += "  (none found)\n";
        }

        // Check save files
        info += "\nSave Files:\n";
        string savesDirectory = Path.Combine(Application.persistentDataPath, "saves");
        info += $"  Directory: {savesDirectory}\n";

        int filesFound = 0;
        if (Directory.Exists(savesDirectory))
        {
            foreach (string fileName in SaveFileNames)
            {
                string filePath = Path.Combine(savesDirectory, fileName);
                if (File.Exists(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);
                    info += $"  {fileName}: {fileInfo.Length} bytes\n";
                    filesFound++;
                }
            }
        }
        if (filesFound == 0)
        {
            info += "  (none found)\n";
        }

        info += $"\nTotal: {prefsFound} PlayerPrefs keys, {filesFound} save files";

        Debug.Log(info);

        EditorUtility.DisplayDialog("Save Data Info", info, "OK");
    }
}
