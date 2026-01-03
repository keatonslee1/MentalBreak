#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates/updates a Resources/GlobalFontConfig asset that points at the project fonts.
/// </summary>
[InitializeOnLoad]
public static class GlobalFontConfigGenerator
{
    private const string ResourcesDir = "Assets/Resources";
    private const string ConfigAssetPath = ResourcesDir + "/GlobalFontConfig.asset";

    private const string PrimarySdfSearch = "monogram-extended SDF t:TMP_FontAsset";
    private const string PrimaryTtfSearch = "monogram-extended t:Font";
    private const string PrimaryTtfFilenameSearch = "monogram-extended.ttf";

    static GlobalFontConfigGenerator()
    {
        // Auto-generate on editor load if missing, but don't spam modifications every domain reload.
        if (!File.Exists(ConfigAssetPath))
        {
            TryGenerateOrUpdate(verbose: false);
        }
    }

    [MenuItem("Tools/Mental Break/Generate Global Font Config")]
    public static void GenerateMenu()
    {
        TryGenerateOrUpdate(verbose: true);
    }

    private static void TryGenerateOrUpdate(bool verbose)
    {
        if (!Directory.Exists(ResourcesDir))
        {
            Directory.CreateDirectory(ResourcesDir);
            AssetDatabase.Refresh();
        }

        GlobalFontConfig config = AssetDatabase.LoadAssetAtPath<GlobalFontConfig>(ConfigAssetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GlobalFontConfig>();
            AssetDatabase.CreateAsset(config, ConfigAssetPath);
        }

        bool changed = false;

        if (config.primarySdfFont == null)
        {
            string guid = FindFirstGuid(PrimarySdfSearch);
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                config.primarySdfFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                changed = true;
            }
        }

        if (config.primaryTtfFont == null)
        {
            string guid = FindFirstGuid(PrimaryTtfSearch);
            if (string.IsNullOrEmpty(guid))
            {
                // Fallback: locate by filename (works reliably in Packages).
                string[] ttfGuids = AssetDatabase.FindAssets(PrimaryTtfFilenameSearch);
                if (ttfGuids != null && ttfGuids.Length > 0)
                {
                    guid = ttfGuids[0];
                }
            }

            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                config.primaryTtfFont = AssetDatabase.LoadAssetAtPath<Font>(path);
                changed = true;
            }
        }

        if (changed)
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        if (verbose)
        {
            Debug.Log($"GlobalFontConfigGenerator: {(changed ? "Updated" : "Verified")} {ConfigAssetPath}\n" +
                      $"- TMP: {(config.primarySdfFont != null ? config.primarySdfFont.name : "<missing>")}\n" +
                      $"- UI.Text: {(config.primaryTtfFont != null ? config.primaryTtfFont.name : "<missing>")}");
        }
    }

    private static string FindFirstGuid(string searchFilter)
    {
        string[] guids = AssetDatabase.FindAssets(searchFilter);
        if (guids == null || guids.Length == 0)
        {
            return null;
        }
        return guids[0];
    }
}
#endif


