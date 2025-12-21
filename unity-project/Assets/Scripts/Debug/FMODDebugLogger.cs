using UnityEngine;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Debug script to log FMOD bank loading status.
/// Add this to any GameObject in the scene.
/// </summary>
public class FMODDebugLogger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool logEveryFrame = false;
    [SerializeField] private float logInterval = 2f;

    private float lastLogTime;
    private bool hasLoggedInitial = false;

    void Start()
    {
        Debug.Log("[FMODDebug] FMODDebugLogger started");
        StartCoroutine(LogFMODStatus());
    }

    IEnumerator LogFMODStatus()
    {
        // Wait a frame for FMOD to initialize
        yield return null;

        Debug.Log("[FMODDebug] Checking FMOD status...");

        // Try to get the FMOD Studio System
        FMOD.Studio.System studioSystem;
        try
        {
            studioSystem = RuntimeManager.StudioSystem;
            Debug.Log("[FMODDebug] RuntimeManager.StudioSystem obtained successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODDebug] Failed to get StudioSystem: {e.Message}");
            yield break;
        }

        // Check system validity
        if (!studioSystem.isValid())
        {
            Debug.LogError("[FMODDebug] StudioSystem is not valid!");
            yield break;
        }

        Debug.Log("[FMODDebug] StudioSystem is valid");

        // Log loaded banks
        LogLoadedBanks(studioSystem);

        // Wait and log again to see if banks load asynchronously
        yield return new WaitForSeconds(2f);
        Debug.Log("[FMODDebug] Checking banks again after 2 seconds...");
        LogLoadedBanks(studioSystem);

        // Try to load a specific event to see what happens
        yield return new WaitForSeconds(1f);
        TryLoadEvent("event:/ASIDE_Supervisor");
        TryLoadEvent("event:/ASIDE_MainTheme");
    }

    void LogLoadedBanks(FMOD.Studio.System studioSystem)
    {
        int bankCount;
        FMOD.RESULT result = studioSystem.getBankCount(out bankCount);

        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError($"[FMODDebug] Failed to get bank count: {result}");
            return;
        }

        Debug.Log($"[FMODDebug] Number of loaded banks: {bankCount}");

        if (bankCount > 0)
        {
            Bank[] banks = new Bank[bankCount];
            result = studioSystem.getBankList(out banks);

            if (result == FMOD.RESULT.OK)
            {
                foreach (var bank in banks)
                {
                    if (bank.isValid())
                    {
                        string path;
                        bank.getPath(out path);

                        FMOD.Studio.LOADING_STATE loadingState;
                        bank.getLoadingState(out loadingState);

                        int eventCount;
                        bank.getEventCount(out eventCount);

                        Debug.Log($"[FMODDebug] Bank: {path} | State: {loadingState} | Events: {eventCount}");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("[FMODDebug] NO BANKS LOADED! This is the problem.");

            // Log StreamingAssets path
            Debug.Log($"[FMODDebug] Application.streamingAssetsPath = {Application.streamingAssetsPath}");

            // Log FMOD Settings
            var settings = Settings.Instance;
            if (settings != null)
            {
                Debug.Log($"[FMODDebug] Settings.ImportType = {settings.ImportType}");
                Debug.Log($"[FMODDebug] Settings.AutomaticEventLoading = {settings.AutomaticEventLoading}");
                Debug.Log($"[FMODDebug] Settings.BankLoadType = {settings.BankLoadType}");
                Debug.Log($"[FMODDebug] Settings.MasterBanks = [{string.Join(", ", settings.MasterBanks)}]");
                Debug.Log($"[FMODDebug] Settings.Banks = [{string.Join(", ", settings.Banks)}]");
            }
        }
    }

    void TryLoadEvent(string eventPath)
    {
        Debug.Log($"[FMODDebug] Attempting to get event description for: {eventPath}");

        try
        {
            EventDescription eventDesc;
            FMOD.RESULT result = RuntimeManager.StudioSystem.getEvent(eventPath, out eventDesc);

            if (result == FMOD.RESULT.OK && eventDesc.isValid())
            {
                Debug.Log($"[FMODDebug] SUCCESS! Event found: {eventPath}");
            }
            else
            {
                Debug.LogWarning($"[FMODDebug] Event not found: {eventPath} - Result: {result}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FMODDebug] Exception getting event {eventPath}: {e.Message}");
        }
    }

    void Update()
    {
        if (logEveryFrame || (Time.time - lastLogTime > logInterval && !hasLoggedInitial))
        {
            lastLogTime = Time.time;

            if (!hasLoggedInitial)
            {
                hasLoggedInitial = true;
            }
        }
    }
}
