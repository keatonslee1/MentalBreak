using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// AGGRESSIVE WebGL bank loader that bypasses FMOD's automatic loading.
/// This script manually downloads and loads all FMOD banks with extensive logging.
/// Auto-creates itself on game start - no need to add to scene manually.
/// </summary>
public class FMODWebGLBankLoader : MonoBehaviour
{
    // Banks to load in order (strings bank must be first, then master)
    private static readonly string[] bankNames = new string[]
    {
        "Master.strings",
        "Master",
        "Music_A_Side",
        "Music_B_Side",
        "UI"
    };

    private static FMODWebGLBankLoader instance;
    private bool banksLoaded = false;
    private int banksLoadedCount = 0;

    public static bool AreBanksLoaded => instance != null && instance.banksLoaded;

    // Auto-create on game start
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        Debug.Log("[FMODWebGLBankLoader] AutoCreate called (RuntimeInitializeOnLoadMethod)");

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("[FMODWebGLBankLoader] WebGL detected - creating bank loader");
        var go = new GameObject("FMODWebGLBankLoader");
        go.AddComponent<FMODWebGLBankLoader>();
        DontDestroyOnLoad(go);
#else
        Debug.Log("[FMODWebGLBankLoader] Not WebGL - skipping custom loader");
#endif
    }

    void Awake()
    {
        if (instance != null)
        {
            Debug.Log("[FMODWebGLBankLoader] Instance already exists, destroying duplicate");
            Destroy(gameObject);
            return;
        }

        instance = this;
        Debug.Log("[FMODWebGLBankLoader] Awake - Instance created");
    }

    void Start()
    {
        Debug.Log("[FMODWebGLBankLoader] Start - Beginning bank loading process");
        Debug.Log($"[FMODWebGLBankLoader] Platform: {Application.platform}");
        Debug.Log($"[FMODWebGLBankLoader] StreamingAssetsPath: {Application.streamingAssetsPath}");
        Debug.Log($"[FMODWebGLBankLoader] Banks to load: {string.Join(", ", bankNames)}");

        // This script only exists in WebGL builds (see AutoCreate), so always load
        Debug.Log("[FMODWebGLBankLoader] Starting manual bank load coroutine");
        StartCoroutine(LoadAllBanksWebGL());
    }

    private IEnumerator LoadAllBanksWebGL()
    {
        Debug.Log("[FMODWebGLBankLoader] LoadAllBanksWebGL coroutine started");

        // Wait a frame to ensure FMOD system is initialized
        yield return null;
        Debug.Log("[FMODWebGLBankLoader] Waited one frame for FMOD initialization");

        // Check if FMOD is available
        try
        {
            var studioSystem = FMODUnity.RuntimeManager.StudioSystem;
            if (!studioSystem.isValid())
            {
                Debug.LogError("[FMODWebGLBankLoader] FMOD StudioSystem is not valid!");
                yield break;
            }
            Debug.Log("[FMODWebGLBankLoader] FMOD StudioSystem is valid");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODWebGLBankLoader] Failed to get FMOD StudioSystem: {e.Message}");
            yield break;
        }

        // Load each bank
        foreach (string bankName in bankNames)
        {
            yield return StartCoroutine(LoadBankWebGL(bankName));
        }

        Debug.Log($"[FMODWebGLBankLoader] All banks processed. Loaded: {banksLoadedCount}/{bankNames.Length}");

        if (banksLoadedCount == bankNames.Length)
        {
            banksLoaded = true;
            Debug.Log("[FMODWebGLBankLoader] SUCCESS - All banks loaded!");
        }
        else
        {
            Debug.LogError($"[FMODWebGLBankLoader] FAILED - Only {banksLoadedCount}/{bankNames.Length} banks loaded");
        }

        // Log bank status
        LogBankStatus();
    }

    private IEnumerator LoadBankWebGL(string bankName)
    {
        string bankFileName = bankName.EndsWith(".bank") ? bankName : bankName + ".bank";

        // Try relative URL first (what FMOD uses)
        string bankPath = Application.streamingAssetsPath + "/" + bankFileName;

        // Also construct absolute URL as fallback
        string absoluteUrl = Application.absoluteURL;
        if (!string.IsNullOrEmpty(absoluteUrl))
        {
            // Get base URL (remove the page name)
            int lastSlash = absoluteUrl.LastIndexOf('/');
            if (lastSlash > 0)
            {
                absoluteUrl = absoluteUrl.Substring(0, lastSlash);
            }
        }
        string absoluteBankPath = absoluteUrl + "/StreamingAssets/" + bankFileName;

        Debug.Log($"[FMODWebGLBankLoader] Loading bank: {bankName}");
        Debug.Log($"[FMODWebGLBankLoader] Application.absoluteURL: {Application.absoluteURL}");
        Debug.Log($"[FMODWebGLBankLoader] Application.streamingAssetsPath: {Application.streamingAssetsPath}");
        Debug.Log($"[FMODWebGLBankLoader] Relative URL: {bankPath}");
        Debug.Log($"[FMODWebGLBankLoader] Absolute URL: {absoluteBankPath}");

        // Try to download the bank file - first with relative URL, then absolute if that fails
        byte[] bankData = null;
        string urlUsed = bankPath;

        // First attempt: relative URL
        Debug.Log($"[FMODWebGLBankLoader] Attempt 1: Trying relative URL: {bankPath}");
        using (UnityWebRequest www = UnityWebRequest.Get(bankPath))
        {
            yield return www.SendWebRequest();

            Debug.Log($"[FMODWebGLBankLoader] Attempt 1 response code: {www.responseCode}");

#if UNITY_2020_1_OR_NEWER
            bool success = www.result == UnityWebRequest.Result.Success;
#else
            bool success = !www.isNetworkError && !www.isHttpError;
#endif
            if (success && www.downloadHandler.data != null && www.downloadHandler.data.Length > 0)
            {
                bankData = www.downloadHandler.data;
                Debug.Log($"[FMODWebGLBankLoader] Attempt 1 SUCCESS - got {bankData.Length} bytes");
            }
            else
            {
                Debug.LogWarning($"[FMODWebGLBankLoader] Attempt 1 FAILED: {www.error ?? "no data"}");
            }
        }

        // Second attempt: absolute URL (if first failed)
        if (bankData == null && !string.IsNullOrEmpty(absoluteBankPath))
        {
            Debug.Log($"[FMODWebGLBankLoader] Attempt 2: Trying absolute URL: {absoluteBankPath}");
            urlUsed = absoluteBankPath;

            using (UnityWebRequest www = UnityWebRequest.Get(absoluteBankPath))
            {
                yield return www.SendWebRequest();

                Debug.Log($"[FMODWebGLBankLoader] Attempt 2 response code: {www.responseCode}");

#if UNITY_2020_1_OR_NEWER
                bool success = www.result == UnityWebRequest.Result.Success;
#else
                bool success = !www.isNetworkError && !www.isHttpError;
#endif
                if (success && www.downloadHandler.data != null && www.downloadHandler.data.Length > 0)
                {
                    bankData = www.downloadHandler.data;
                    Debug.Log($"[FMODWebGLBankLoader] Attempt 2 SUCCESS - got {bankData.Length} bytes");
                }
                else
                {
                    Debug.LogError($"[FMODWebGLBankLoader] Attempt 2 FAILED: {www.error ?? "no data"}");
                }
            }
        }

        // Check if we got data
        if (bankData == null || bankData.Length == 0)
        {
            Debug.LogError($"[FMODWebGLBankLoader] FAILED to download {bankFileName} from any URL");
            yield break;
        }

        Debug.Log($"[FMODWebGLBankLoader] Downloaded {bankData.Length} bytes for {bankFileName} from {urlUsed}");

        // Load the bank into FMOD
        try
        {
            FMOD.Studio.Bank bank;
            FMOD.RESULT result = FMODUnity.RuntimeManager.StudioSystem.loadBankMemory(
                bankData,
                FMOD.Studio.LOAD_BANK_FLAGS.NORMAL,
                out bank
            );

            if (result != FMOD.RESULT.OK)
            {
                Debug.LogError($"[FMODWebGLBankLoader] FMOD loadBankMemory FAILED for {bankFileName}: {result}");
                yield break;
            }

            Debug.Log($"[FMODWebGLBankLoader] SUCCESS - Bank loaded: {bankFileName}");
            banksLoadedCount++;

            // Log bank info
            if (bank.isValid())
            {
                string path;
                bank.getPath(out path);
                int eventCount;
                bank.getEventCount(out eventCount);
                Debug.Log($"[FMODWebGLBankLoader] Bank path: {path}, Events: {eventCount}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODWebGLBankLoader] Exception loading {bankFileName}: {e.Message}");
            Debug.LogException(e);
        }
    }

    private void LogBankStatus()
    {
        Debug.Log("[FMODWebGLBankLoader] === BANK STATUS ===");

        try
        {
            var studioSystem = FMODUnity.RuntimeManager.StudioSystem;
            if (!studioSystem.isValid())
            {
                Debug.LogError("[FMODWebGLBankLoader] StudioSystem not valid for status check");
                return;
            }

            int bankCount;
            studioSystem.getBankCount(out bankCount);
            Debug.Log($"[FMODWebGLBankLoader] Total banks loaded in FMOD: {bankCount}");

            if (bankCount > 0)
            {
                FMOD.Studio.Bank[] banks;
                studioSystem.getBankList(out banks);

                foreach (var bank in banks)
                {
                    if (bank.isValid())
                    {
                        string path;
                        bank.getPath(out path);

                        FMOD.Studio.LOADING_STATE state;
                        bank.getLoadingState(out state);

                        int eventCount;
                        bank.getEventCount(out eventCount);

                        Debug.Log($"[FMODWebGLBankLoader] Bank: {path} | State: {state} | Events: {eventCount}");
                    }
                }
            }

            // Try to find a specific event
            Debug.Log("[FMODWebGLBankLoader] Testing event lookup...");
            FMOD.Studio.EventDescription eventDesc;
            FMOD.RESULT result = studioSystem.getEvent("event:/ASIDE_Supervisor", out eventDesc);
            if (result == FMOD.RESULT.OK && eventDesc.isValid())
            {
                Debug.Log("[FMODWebGLBankLoader] SUCCESS - event:/ASIDE_Supervisor found!");
            }
            else
            {
                Debug.LogWarning($"[FMODWebGLBankLoader] event:/ASIDE_Supervisor NOT found: {result}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODWebGLBankLoader] Exception in LogBankStatus: {e.Message}");
        }

        Debug.Log("[FMODWebGLBankLoader] === END BANK STATUS ===");
    }

    // Public method to manually trigger bank loading (for debugging)
    public void ReloadBanks()
    {
        Debug.Log("[FMODWebGLBankLoader] Manual reload triggered");
        banksLoadedCount = 0;
        banksLoaded = false;
        StartCoroutine(LoadAllBanksWebGL());
    }
}
