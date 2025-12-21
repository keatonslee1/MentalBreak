using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Applies global font settings on startup and on each scene load.
/// - TMP uses Montserrat-Regular SDF (per project choice)
/// - Legacy UI.Text uses Montserrat-Medium.ttf
/// </summary>
public sealed class GlobalFontOverride : MonoBehaviour
{
    private const string ResourcesConfigName = "GlobalFontConfig";

    private static GlobalFontOverride instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(GlobalFontOverride));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<GlobalFontOverride>();
    }

    private GlobalFontConfig config;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        config = Resources.Load<GlobalFontConfig>(ResourcesConfigName);
        if (config == null)
        {
            Debug.LogWarning($"GlobalFontOverride: Could not load Resources/{ResourcesConfigName}.asset. Run Tools -> Mental Break -> Generate Global Font Config in the editor.");
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        ApplyNow();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyNow();
    }

    private void ApplyNow()
    {
        if (config == null)
        {
            return;
        }

        if (config.montserratRegularSdf != null)
        {
            // Set TMP global default.
            TMP_Settings.defaultFontAsset = config.montserratRegularSdf;
        }

        ApplyToAllTmpText();
        ApplyToAllLegacyText();
    }

    /// <summary>
    /// Call this after creating runtime UI (e.g. programmatic HUD buttons) to ensure fonts are applied.
    /// Safe to call even if config is missing.
    /// </summary>
    public static void RequestApply()
    {
        if (instance == null)
        {
            return;
        }

        instance.ApplyNow();
    }

    private void ApplyToAllTmpText()
    {
        if (config == null || config.montserratRegularSdf == null)
        {
            return;
        }

        TMP_Text[] allTmp = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        for (int i = 0; i < allTmp.Length; i++)
        {
            TMP_Text t = allTmp[i];
            if (t == null) continue;
            if (t.font == config.montserratRegularSdf) continue;
            t.font = config.montserratRegularSdf;
        }
    }

    private void ApplyToAllLegacyText()
    {
        if (config == null || config.montserratMediumTtf == null)
        {
            return;
        }

        Text[] allText = FindObjectsByType<Text>(FindObjectsSortMode.None);
        for (int i = 0; i < allText.Length; i++)
        {
            Text t = allText[i];
            if (t == null) continue;
            if (t.font == config.montserratMediumTtf) continue;
            t.font = config.montserratMediumTtf;
        }
    }
}


