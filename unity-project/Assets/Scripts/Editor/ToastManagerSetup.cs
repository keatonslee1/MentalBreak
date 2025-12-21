using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor script to set up the ToastManager in the scene.
/// </summary>
public class ToastManagerSetup : EditorWindow
{
    [MenuItem("Tools/Setup ToastManager in Scene")]
    public static void SetupToastManager()
    {
        // Check if already exists
        ToastManager existing = Object.FindFirstObjectByType<ToastManager>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists",
                $"ToastManager already exists on: {existing.gameObject.name}\n\n" +
                "The ToastManager is a singleton and only one instance is needed.",
                "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        // Try to find Dialogue System to add it there (persists across scenes)
        GameObject dialogueSystem = GameObject.Find("Dialogue System");

        GameObject targetObj;
        if (dialogueSystem != null)
        {
            targetObj = dialogueSystem;
            Debug.Log("ToastManagerSetup: Adding ToastManager to existing Dialogue System object");
        }
        else
        {
            // Create new GameObject
            targetObj = new GameObject("ToastManager");
            Undo.RegisterCreatedObjectUndo(targetObj, "Create ToastManager");
            Debug.Log("ToastManagerSetup: Created new ToastManager GameObject");
        }

        ToastManager manager = Undo.AddComponent<ToastManager>(targetObj);

        // Configure default settings
        manager.defaultDuration = 2.5f;
        manager.fadeDuration = 0.3f;
        manager.maxToasts = 3;
        manager.bottomOffset = 100f;
        manager.toastSpacing = 60f;

        EditorUtility.SetDirty(targetObj);
        Selection.activeGameObject = targetObj;

        EditorUtility.DisplayDialog("Success",
            $"ToastManager added to: {targetObj.name}\n\n" +
            "Usage in code:\n" +
            "  ToastManager.Show(\"Message\");\n" +
            "  ToastManager.ShowSuccess(\"Saved!\");\n" +
            "  ToastManager.ShowError(\"Failed!\");\n\n" +
            "Remember to save the scene!", "OK");
    }

    [MenuItem("Tools/Test Toast Notifications")]
    public static void TestToastNotifications()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Play Mode Required",
                "Toast notifications can only be tested in Play Mode.\n\n" +
                "Enter Play Mode and run this command again.", "OK");
            return;
        }

        if (ToastManager.Instance == null)
        {
            EditorUtility.DisplayDialog("ToastManager Not Found",
                "ToastManager is not in the scene.\n\n" +
                "Run 'Tools > Setup ToastManager in Scene' first.", "OK");
            return;
        }

        // Show test toasts
        ToastManager.ShowInfo("This is an info toast");

        // Delay subsequent toasts slightly for visual effect
        EditorApplication.delayCall += () =>
        {
            if (Application.isPlaying && ToastManager.Instance != null)
            {
                ToastManager.ShowSuccess("Game saved successfully!");
            }
        };

        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying && ToastManager.Instance != null)
                {
                    ToastManager.ShowWarning("Save slot will be overwritten");
                }
            };
        };

        EditorApplication.delayCall += () =>
        {
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    if (Application.isPlaying && ToastManager.Instance != null)
                    {
                        ToastManager.ShowError("Failed to load save");
                    }
                };
            };
        };
    }
}
