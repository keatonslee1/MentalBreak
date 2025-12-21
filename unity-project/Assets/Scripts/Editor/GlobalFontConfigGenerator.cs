#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates/updates a Resources/GlobalFontConfig asset that points at the Montserrat fonts already present in the repo.
/// </summary>
[InitializeOnLoad]
public static class GlobalFontConfigGenerator
{
    private const string ResourcesDir = "Assets/Resources";
    private const string ConfigAssetPath = ResourcesDir + "/GlobalFontConfig.asset";

    // Prefer the Yarn Spinner sample assets that are already in the repo.
    private const string MontserratRegularSdfSearch = "Montserrat-Regular SDF t:TMP_FontAsset";
    // Font type search can be flaky across Packages vs Assets; we'll also fall back to filename search.
    private const string MontserratMediumTtfSearch = "Montserrat-Medium t:Font";
    private const string MontserratMediumTtfFilenameSearch = "Montserrat-Medium.ttf";

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

        if (config.montserratRegularSdf == null)
        {
            string guid = FindFirstGuid(MontserratRegularSdfSearch);
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                config.montserratRegularSdf = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                changed = true;
            }
        }

        if (config.montserratMediumTtf == null)
        {
            string guid = FindFirstGuid(MontserratMediumTtfSearch);
            if (string.IsNullOrEmpty(guid))
            {
                // Fallback: locate by filename (works reliably in Packages).
                string[] ttfGuids = AssetDatabase.FindAssets(MontserratMediumTtfFilenameSearch);
                if (ttfGuids != null && ttfGuids.Length > 0)
                {
                    guid = ttfGuids[0];
                }
            }

            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                config.montserratMediumTtf = AssetDatabase.LoadAssetAtPath<Font>(path);
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
                      $"- TMP: {(config.montserratRegularSdf != null ? config.montserratRegularSdf.name : "<missing>")}\n" +
                      $"- UI.Text: {(config.montserratMediumTtf != null ? config.montserratMediumTtf.name : "<missing>")}");
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


