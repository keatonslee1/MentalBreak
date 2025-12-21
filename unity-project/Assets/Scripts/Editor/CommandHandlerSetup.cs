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

        // Check/Add CommandHandlerRegistrar
        if (dialogueSystem.GetComponent<CommandHandlerRegistrar>() == null)
        {
            Undo.AddComponent<CommandHandlerRegistrar>(dialogueSystem);
            Debug.Log("Added CommandHandlerRegistrar to Dialogue System");
            addedCount++;
        }

        // Check/Add CheckpointCommandHandler
        if (Object.FindFirstObjectByType<CheckpointCommandHandler>() == null)
        {
            Undo.AddComponent<CheckpointCommandHandler>(dialogueSystem);
            Debug.Log("Added CheckpointCommandHandler to Dialogue System");
            addedCount++;
        }

        // Check/Add FMODAudioManager
        if (Object.FindFirstObjectByType<FMODAudioManager>() == null)
        {
            Undo.AddComponent<FMODAudioManager>(dialogueSystem);
            Debug.Log("Added FMODAudioManager to Dialogue System");
            addedCount++;
        }

        // Check/Add BackgroundCommandHandler
        if (Object.FindFirstObjectByType<BackgroundCommandHandler>() == null)
        {
            Undo.AddComponent<BackgroundCommandHandler>(dialogueSystem);
            Debug.Log("Added BackgroundCommandHandler to Dialogue System");
            addedCount++;
        }

        // Check/Add AudioCommandHandler
        if (Object.FindFirstObjectByType<AudioCommandHandler>() == null)
        {
            Undo.AddComponent<AudioCommandHandler>(dialogueSystem);
            Debug.Log("Added AudioCommandHandler to Dialogue System");
            addedCount++;
        }

        EditorUtility.SetDirty(dialogueSystem);

        string message;
        if (addedCount > 0)
        {
            message = $"Added {addedCount} command handler(s) to Dialogue System.\n\n" +
                "Remember to SAVE THE SCENE!\n\n" +
                "Components added:\n";

            if (dialogueSystem.GetComponent<CommandHandlerRegistrar>() != null)
                message += "- CommandHandlerRegistrar\n";
            if (dialogueSystem.GetComponent<CheckpointCommandHandler>() != null)
                message += "- CheckpointCommandHandler\n";
            if (dialogueSystem.GetComponent<FMODAudioManager>() != null)
                message += "- FMODAudioManager\n";
            if (dialogueSystem.GetComponent<BackgroundCommandHandler>() != null)
                message += "- BackgroundCommandHandler\n";
            if (dialogueSystem.GetComponent<AudioCommandHandler>() != null)
                message += "- AudioCommandHandler\n";
        }
        else
        {
            message = "All command handlers are already present in the scene.";
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
