using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Automatically fixes the FMOD cache when Unity reloads.
/// This prevents the "UI.bank.bank" bug from recurring.
///
/// The root cause is that the UI bank in FMOD Studio is named "UI.bank"
/// instead of just "UI". This script fixes the cache after FMOD regenerates it.
/// </summary>
[InitializeOnLoad]
public static class FMODCacheFixer
{
    private const string CachePath = "Assets/Plugins/FMOD/Cache/Editor/FMODStudioCache.asset";
    private const string SettingsPath = "Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset";

    static FMODCacheFixer()
    {
        // Run on editor load and after scripts recompile
        EditorApplication.delayCall += FixFMODCache;
    }

    [MenuItem("Tools/FMOD/Fix UI.bank Cache Issue")]
    public static void FixFMODCache()
    {
        bool fixedAnything = false;

        // Fix the cache file
        if (File.Exists(CachePath))
        {
            string content = File.ReadAllText(CachePath);
            string originalContent = content;

            // Fix the corrupt UI.bank.bank references
            // Pattern: bank:/UI.bank.bank -> bank:/UI
            content = Regex.Replace(content, @"bank:/UI\.bank\.bank", "bank:/UI");

            // Pattern: Path with UI.bank.bank -> UI.bank
            content = Regex.Replace(content, @"Path: Assets/StreamingAssets/UI\.bank\.bank", "Path: Assets/StreamingAssets/UI.bank");

            // Pattern: Name: UI.bank -> Name: UI (but not in paths)
            content = Regex.Replace(content, @"(\s+Name: )UI\.bank(\s*\n)", "$1UI$2");

            // Pattern: StudioPath: bank:/UI.bank -> bank:/UI
            content = Regex.Replace(content, @"StudioPath: bank:/UI\.bank(\s*\n)", "StudioPath: bank:/UI$1");

            if (content != originalContent)
            {
                File.WriteAllText(CachePath, content);
                fixedAnything = true;
                Debug.Log("[FMODCacheFixer] Fixed UI.bank.bank references in FMODStudioCache.asset");
            }
        }

        // Fix the settings file
        if (File.Exists(SettingsPath))
        {
            string content = File.ReadAllText(SettingsPath);
            string originalContent = content;

            // Fix Banks list: "- UI.bank" -> "- UI"
            content = Regex.Replace(content, @"(\s+- )UI\.bank(\s*\n)", "$1UI$2");

            if (content != originalContent)
            {
                File.WriteAllText(SettingsPath, content);
                fixedAnything = true;
                Debug.Log("[FMODCacheFixer] Fixed UI.bank reference in FMODStudioSettings.asset");
            }
        }

        if (fixedAnything)
        {
            AssetDatabase.Refresh();
            Debug.Log("[FMODCacheFixer] FMOD cache/settings fixed successfully!");
        }
    }

    // Also run after FMOD refreshes banks
    [InitializeOnLoadMethod]
    private static void RegisterCallbacks()
    {
        // Hook into asset import to catch FMOD cache regeneration
        EditorApplication.projectChanged += OnProjectChanged;
    }

    private static void OnProjectChanged()
    {
        // Delay to let FMOD finish its work first
        EditorApplication.delayCall += FixFMODCache;
    }
}
