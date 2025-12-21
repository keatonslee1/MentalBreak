using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to ensure all Yarn command handlers are properly set up in the scene.
/// Run via Tools > Setup Command Handlers
/// </summary>
public class CommandHandlerSetup : EditorWindow
{
    [MenuItem("Tools/Setup Command Handlers")]
    public static void SetupCommandHandlers()
    {
        // Find or create target (Dialogue System or new GameObject)
        GameObject dialogueSystem = GameObject.Find("Dialogue System");
        if (dialogueSystem == null)
        {
            // Try to find DialogueRunner
            var runner = Object.FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
            if (runner != null)
            {
                dialogueSystem = runner.gameObject;
            }
        }

        if (dialogueSystem == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Dialogue System not found in scene.\n\n" +
                "Please ensure the Dialogue System is in the scene before running this setup.", "OK");
            return;
        }

        Undo.RecordObject(dialogueSystem, "Setup Command Handlers");

        int addedCount = 0;
        string report = "";

        // IMPORTANT: For WebGL reliability, ALL handlers should be on the SAME GameObject
        // This ensures GetComponent works reliably

        // Check/Add CommandHandlerRegistrar
        if (dialogueSystem.GetComponent<CommandHandlerRegistrar>() == null)
        {
            Undo.AddComponent<CommandHandlerRegistrar>(dialogueSystem);
            Debug.Log("Added CommandHandlerRegistrar to Dialogue System");
            report += "- Added CommandHandlerRegistrar\n";
            addedCount++;
        }
        else
        {
            report += "- CommandHandlerRegistrar: OK\n";
        }

        // Check/Add CheckpointCommandHandler - MUST be on same GameObject for WebGL
        if (dialogueSystem.GetComponent<CheckpointCommandHandler>() == null)
        {
            Undo.AddComponent<CheckpointCommandHandler>(dialogueSystem);
            Debug.Log("Added CheckpointCommandHandler to Dialogue System");
            report += "- Added CheckpointCommandHandler\n";
            addedCount++;
        }
        else
        {
            report += "- CheckpointCommandHandler: OK\n";
        }

        // Check/Add FMODAudioManager - MUST be on same GameObject for WebGL
        if (dialogueSystem.GetComponent<FMODAudioManager>() == null)
        {
            Undo.AddComponent<FMODAudioManager>(dialogueSystem);
            Debug.Log("Added FMODAudioManager to Dialogue System");
            report += "- Added FMODAudioManager\n";
            addedCount++;
        }
        else
        {
            report += "- FMODAudioManager: OK\n";
        }

        // Check/Add BackgroundCommandHandler - MUST be on same GameObject for WebGL
        if (dialogueSystem.GetComponent<BackgroundCommandHandler>() == null)
        {
            Undo.AddComponent<BackgroundCommandHandler>(dialogueSystem);
            Debug.Log("Added BackgroundCommandHandler to Dialogue System");
            report += "- Added BackgroundCommandHandler\n";
            addedCount++;
        }
        else
        {
            report += "- BackgroundCommandHandler: OK\n";
        }

        // Check/Add AudioCommandHandler - MUST be on same GameObject for WebGL
        if (dialogueSystem.GetComponent<AudioCommandHandler>() == null)
        {
            Undo.AddComponent<AudioCommandHandler>(dialogueSystem);
            Debug.Log("Added AudioCommandHandler to Dialogue System");
            report += "- Added AudioCommandHandler\n";
            addedCount++;
        }
        else
        {
            report += "- AudioCommandHandler: OK\n";
        }

        EditorUtility.SetDirty(dialogueSystem);

        string message;
        if (addedCount > 0)
        {
            message = $"Added {addedCount} command handler(s) to '{dialogueSystem.name}'.\n\n" +
                "IMPORTANT: Remember to SAVE THE SCENE!\n\n" +
                report;
        }
        else
        {
            message = $"All command handlers are already on '{dialogueSystem.name}'.\n\n" +
                report + "\n" +
                "If commands still don't work in WebGL, try:\n" +
                "1. Run 'Tools > Verify Command Handlers'\n" +
                "2. Check if handlers are on DIFFERENT GameObjects\n" +
                "3. Save scene and rebuild";
        }

        EditorUtility.DisplayDialog("Setup Complete", message, "OK");
        Selection.activeGameObject = dialogueSystem;
    }

    [MenuItem("Tools/Verify Command Handlers")]
    public static void VerifyCommandHandlers()
    {
        string report = "=== Command Handler Verification ===\n\n";
        bool allGood = true;

        // Check CommandHandlerRegistrar
        var registrar = Object.FindFirstObjectByType<CommandHandlerRegistrar>();
        if (registrar != null)
        {
            report += $"CommandHandlerRegistrar: Found on '{registrar.gameObject.name}'\n";
        }
        else
        {
            report += "CommandHandlerRegistrar: MISSING\n";
            allGood = false;
        }

        // Check CheckpointCommandHandler
        var checkpoint = Object.FindFirstObjectByType<CheckpointCommandHandler>();
        if (checkpoint != null)
        {
            report += $"CheckpointCommandHandler: Found on '{checkpoint.gameObject.name}'\n";
        }
        else
        {
            report += "CheckpointCommandHandler: MISSING\n";
            allGood = false;
        }

        // Check FMODAudioManager
        var fmod = Object.FindFirstObjectByType<FMODAudioManager>();
        if (fmod != null)
        {
            report += $"FMODAudioManager: Found on '{fmod.gameObject.name}'\n";
        }
        else
        {
            report += "FMODAudioManager: MISSING\n";
            allGood = false;
        }

        // Check BackgroundCommandHandler
        var bg = Object.FindFirstObjectByType<BackgroundCommandHandler>();
        if (bg != null)
        {
            report += $"BackgroundCommandHandler: Found on '{bg.gameObject.name}'\n";
        }
        else
        {
            report += "BackgroundCommandHandler: MISSING\n";
            allGood = false;
        }

        // Check AudioCommandHandler
        var audio = Object.FindFirstObjectByType<AudioCommandHandler>();
        if (audio != null)
        {
            report += $"AudioCommandHandler: Found on '{audio.gameObject.name}'\n";
        }
        else
        {
            report += "AudioCommandHandler: MISSING\n";
            allGood = false;
        }

        report += "\n";
        if (allGood)
        {
            report += "All command handlers are properly set up.";
        }
        else
        {
            report += "Some handlers are missing. Run 'Tools > Setup Command Handlers' to fix.";
        }

        Debug.Log(report);
        EditorUtility.DisplayDialog("Verification Result", report, "OK");
    }
}
