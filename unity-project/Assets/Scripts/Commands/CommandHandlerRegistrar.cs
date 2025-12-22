using UnityEngine;
using Yarn.Unity;
using System.Reflection;
using System.Collections;

/// <summary>
/// Ensures command handlers are properly registered with the DialogueRunner.
/// This script manually registers [YarnCommand] methods to ensure they're discovered.
/// </summary>
public class CommandHandlerRegistrar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("DialogueRunner to register commands with")]
    public DialogueRunner dialogueRunner;

    [Header("Command Handlers")]
    [Tooltip("BackgroundCommandHandler component")]
    public BackgroundCommandHandler backgroundHandler;

    [Tooltip("AudioCommandHandler component")]
    public AudioCommandHandler audioHandler;

    [Tooltip("CheckpointCommandHandler component")]
    public CheckpointCommandHandler checkpointHandler;

    [Tooltip("CompanyStore component")]
    public CompanyStore companyStore;

    [Tooltip("FMODAudioManager component")]
    public FMODAudioManager fmodHandler;

    private void Awake()
    {
        Debug.Log("CommandHandlerRegistrar: Awake() starting - finding and registering command handlers");

        // Find DialogueRunner - check multiple locations
        if (dialogueRunner == null)
        {
            dialogueRunner = GetComponent<DialogueRunner>();
            if (dialogueRunner == null) dialogueRunner = GetComponentInParent<DialogueRunner>();
            if (dialogueRunner == null) dialogueRunner = GetComponentInChildren<DialogueRunner>();
            if (dialogueRunner == null) dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        if (dialogueRunner != null)
        {
            Debug.Log($"CommandHandlerRegistrar: Found DialogueRunner on '{dialogueRunner.gameObject.name}'");
        }
        else
        {
            Debug.LogError("CommandHandlerRegistrar: DialogueRunner NOT FOUND - commands will not work!");
        }

        // Find command handlers - check own GameObject first, then search scene, then create
        backgroundHandler = FindOrCreateHandler<BackgroundCommandHandler>(ref backgroundHandler, "BackgroundCommandHandler");
        audioHandler = FindOrCreateHandler<AudioCommandHandler>(ref audioHandler, "AudioCommandHandler");
        checkpointHandler = FindOrCreateHandler<CheckpointCommandHandler>(ref checkpointHandler, "CheckpointCommandHandler");
        fmodHandler = FindOrCreateHandler<FMODAudioManager>(ref fmodHandler, "FMODAudioManager");

        // Find CompanyStore if not assigned (include inactive objects since panel starts hidden)
        if (companyStore == null)
        {
            companyStore = FindFirstObjectByType<CompanyStore>(FindObjectsInactive.Include);
        }

        // Register commands immediately in Awake() to ensure they're available before dialogue starts
        // This is critical for WebGL where dialogue may begin very early
        RegisterCommands();

        Debug.Log("CommandHandlerRegistrar: Awake() complete - commands registered");
    }

    /// <summary>
    /// Find a handler component, checking multiple locations, or create if not found.
    /// </summary>
    private T FindOrCreateHandler<T>(ref T handler, string handlerName) where T : Component
    {
        // If already assigned via Inspector, use it
        if (handler != null)
        {
            Debug.Log($"CommandHandlerRegistrar: {handlerName} already assigned via Inspector on '{handler.gameObject.name}'");
            return handler;
        }

        // Check own GameObject first (most reliable for WebGL)
        handler = GetComponent<T>();
        if (handler != null)
        {
            Debug.Log($"CommandHandlerRegistrar: Found {handlerName} on same GameObject");
            return handler;
        }

        // Check parent hierarchy
        handler = GetComponentInParent<T>();
        if (handler != null)
        {
            Debug.Log($"CommandHandlerRegistrar: Found {handlerName} in parent hierarchy on '{handler.gameObject.name}'");
            return handler;
        }

        // Check children
        handler = GetComponentInChildren<T>();
        if (handler != null)
        {
            Debug.Log($"CommandHandlerRegistrar: Found {handlerName} in children on '{handler.gameObject.name}'");
            return handler;
        }

        // Search entire scene (less reliable in WebGL)
        handler = FindFirstObjectByType<T>();
        if (handler != null)
        {
            Debug.Log($"CommandHandlerRegistrar: Found {handlerName} via scene search on '{handler.gameObject.name}'");
            return handler;
        }

        // Not found anywhere - create on this GameObject
        Debug.LogWarning($"CommandHandlerRegistrar: {handlerName} not found anywhere, creating on this GameObject");
        handler = gameObject.AddComponent<T>();
        return handler;
    }

    private void Start()
    {
        // Commands should already be registered in Awake(), but re-register as safety net
        // This handles edge cases where component was added at runtime after Awake()
        RegisterCommands();
    }

    /// <summary>
    /// Manually register command handlers with the DialogueRunner
    /// </summary>
    private void RegisterCommands()
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("CommandHandlerRegistrar: DialogueRunner not found!");
            return;
        }

        // Ensure DialogueRunner has YarnProject (needed for command dispatcher initialization)
        if (dialogueRunner.YarnProject == null)
        {
            Debug.LogWarning("CommandHandlerRegistrar: DialogueRunner.YarnProject is null. Commands may not work correctly.");
        }

        bool allRegistered = true;

        // Register BackgroundCommandHandler method via bound delegate
        if (backgroundHandler != null)
        {
            try
            {
                // Remove if already registered (safe to call)
                try { dialogueRunner.RemoveCommandHandler("bg"); } catch { }
                dialogueRunner.AddCommandHandler("bg", new System.Action<string>(backgroundHandler.ChangeBackground));
                Debug.Log("CommandHandlerRegistrar: Registered 'bg' command");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CommandHandlerRegistrar: Failed to register 'bg': {e.Message}");
                allRegistered = false;
            }
        }
        else
        {
            Debug.LogWarning("CommandHandlerRegistrar: BackgroundCommandHandler not found!");
            allRegistered = false;
        }

        // Register AudioCommandHandler methods via bound delegates
        if (audioHandler != null)
        {
            try
            {
                // Remove if already registered (safe to call)
                try { dialogueRunner.RemoveCommandHandler("bgm"); } catch { }
                try { dialogueRunner.RemoveCommandHandler("sfx"); } catch { }
                dialogueRunner.AddCommandHandler("bgm", new System.Action<string>(audioHandler.PlayBGM));
                dialogueRunner.AddCommandHandler("sfx", new System.Action<string>(audioHandler.PlaySFX));
                Debug.Log("CommandHandlerRegistrar: Registered 'bgm' and 'sfx' commands");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CommandHandlerRegistrar: Failed to register audio commands: {e.Message}");
                allRegistered = false;
            }
        }
        else
        {
            Debug.LogWarning("CommandHandlerRegistrar: AudioCommandHandler not found!");
            allRegistered = false;
        }

        // Register CheckpointCommandHandler method via bound delegate
        // Note: The [YarnCommand] attribute should auto-discover this, but explicit registration ensures it works
        if (checkpointHandler != null)
        {
            try
            {
                // Remove if already registered (safe to call)
                try { dialogueRunner.RemoveCommandHandler("checkpoint"); } catch { }
                dialogueRunner.AddCommandHandler("checkpoint", new System.Action<string>(checkpointHandler.Checkpoint));
                Debug.Log("CommandHandlerRegistrar: Registered 'checkpoint' command");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CommandHandlerRegistrar: Failed to register 'checkpoint': {e.Message}");
                allRegistered = false;
            }
        }
        else
        {
            Debug.LogWarning("CommandHandlerRegistrar: CheckpointCommandHandler not found!");
            // Don't mark as failed - checkpoint is optional for backwards compatibility
        }

        // Register CompanyStore method via bound delegate
        // Note: OpenStore returns IEnumerator, so we use Func<IEnumerator> instead of Action
        if (companyStore != null)
        {
            try
            {
                // Create the delegate bound to the specific instance
                System.Func<IEnumerator> storeCommand = companyStore.OpenStore;
                dialogueRunner.AddCommandHandler("store", storeCommand);

                Debug.Log($"CommandHandlerRegistrar: Registered 'store' command with CompanyStore on {companyStore.gameObject.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CommandHandlerRegistrar: Failed to register 'store': {e.Message}");
                allRegistered = false;
            }
        }
        else
        {
            Debug.LogWarning("CommandHandlerRegistrar: CompanyStore not found! Store command will not be available. Run Tools > Setup Company Store.");
        }

        // Register FMODAudioManager commands
        if (fmodHandler != null)
        {
            try
            {
                try { dialogueRunner.RemoveCommandHandler("music"); } catch { }
                try { dialogueRunner.RemoveCommandHandler("music_stop"); } catch { }
                try { dialogueRunner.RemoveCommandHandler("fmod"); } catch { }
                try { dialogueRunner.RemoveCommandHandler("fmod_stop"); } catch { }
                try { dialogueRunner.RemoveCommandHandler("fmod_loop"); } catch { }
                try { dialogueRunner.RemoveCommandHandler("fmod_param"); } catch { }

                dialogueRunner.AddCommandHandler("music", new System.Action<string, int>(fmodHandler.YarnPlayTheme));
                dialogueRunner.AddCommandHandler("music_stop", new System.Action<bool>(fmodHandler.YarnStopTheme));
                dialogueRunner.AddCommandHandler("fmod", new System.Action<string, int>(fmodHandler.YarnPlayMusic));
                dialogueRunner.AddCommandHandler("fmod_stop", new System.Action<bool>(fmodHandler.YarnStopMusic));
                dialogueRunner.AddCommandHandler("fmod_loop", new System.Action<string>(fmodHandler.YarnSetLoop));
                dialogueRunner.AddCommandHandler("fmod_param", new System.Action<string, float>(fmodHandler.YarnSetParameter));
                Debug.Log("CommandHandlerRegistrar: Registered FMOD commands (music, music_stop, fmod, fmod_stop, fmod_loop, fmod_param)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"CommandHandlerRegistrar: Failed to register FMOD commands: {e.Message}");
                allRegistered = false;
            }
        }
        else
        {
            Debug.LogWarning("CommandHandlerRegistrar: FMODAudioManager not found. FMOD commands will not be available.");
        }

        if (allRegistered)
        {
            Debug.Log("CommandHandlerRegistrar: All commands registered successfully!");
        }
    }

    // Note: using bound delegates above ensures Yarn does not interpret the first
    // parameter as a GameObject target; instead, the string is passed to the
    // instance methods directly.
}

