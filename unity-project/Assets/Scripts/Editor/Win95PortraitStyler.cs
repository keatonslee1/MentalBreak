using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using MentalBreak.UI.Win95;

/// <summary>
/// Editor tools for applying Win95 styling to portrait frames.
/// </summary>
public class Win95PortraitStyler : EditorWindow
{
    [MenuItem("Tools/Apply Win95 Portrait Frames")]
    public static void ApplyWin95PortraitFrames()
    {
        // Find CharacterSpriteManager
        var spriteManager = FindFirstObjectByType<CharacterSpriteManager>();
        if (spriteManager == null)
        {
            EditorUtility.DisplayDialog("Error",
                "CharacterSpriteManager not found in scene. Please ensure the Dialogue System is in the scene.",
                "OK");
            return;
        }

        // Apply styling to all portrait frames in the scene
        Win95PortraitFrame.StyleAllPortraitFrames();

        // Update the frame color in CharacterSpriteManager to match Win95 style
        spriteManager.portraitFrameColor = Win95Theme.WindowBackground;

        EditorUtility.SetDirty(spriteManager);

        EditorUtility.DisplayDialog("Success",
            "Win95 styling applied to portrait frames!\n\n" +
            "Note: Portrait frames are created at runtime, so the Win95 borders\n" +
            "will only appear when playing the game.\n\n" +
            "The frame background color has been updated in CharacterSpriteManager.",
            "OK");
    }

    [MenuItem("Tools/Update CharacterSpriteManager for Win95")]
    public static void UpdateCharacterSpriteManagerForWin95()
    {
        var spriteManager = FindFirstObjectByType<CharacterSpriteManager>();
        if (spriteManager == null)
        {
            EditorUtility.DisplayDialog("Error",
                "CharacterSpriteManager not found in scene.",
                "OK");
            return;
        }

        // Update portrait frame settings for Win95 style
        spriteManager.portraitFrameColor = Win95Theme.WindowBackground;

        // Optionally adjust frame headroom for Win95 borders
        spriteManager.portraitFrameHeadroom = 16f; // Slightly more headroom for borders

        EditorUtility.SetDirty(spriteManager);

        Debug.Log("CharacterSpriteManager updated with Win95 styling settings.");
        EditorUtility.DisplayDialog("Success",
            "CharacterSpriteManager settings updated for Win95 styling.\n\n" +
            "To apply Win95 borders at runtime, add the Win95PortraitStyler component\n" +
            "to the CharacterSpriteManager GameObject.",
            "OK");
    }
}

/// <summary>
/// Runtime component that automatically applies Win95 styling to portrait frames.
/// Add this to the CharacterSpriteManager GameObject.
/// </summary>
public class Win95PortraitFrameApplier : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Style to apply to portrait frames")]
    public Win95PortraitFrame.PanelStyle frameStyle = Win95PortraitFrame.PanelStyle.Raised;

    [Tooltip("Delay before applying styling (to allow portrait frames to be created)")]
    public float applyDelay = 0.5f;

    private bool hasApplied = false;

    private void Start()
    {
        if (!hasApplied)
        {
            Invoke(nameof(ApplyWin95Styling), applyDelay);
        }
    }

    private void ApplyWin95Styling()
    {
        if (hasApplied) return;

        // Find and style all portrait frames
        var portraitFrames = FindObjectsByType<RectMask2D>(FindObjectsSortMode.None);
        foreach (var frame in portraitFrames)
        {
            if (frame.gameObject.name.Contains("CharacterPortraitFrame") ||
                frame.gameObject.name.Contains("PortraitFrame"))
            {
                Win95PortraitFrame.ApplyTo(frame.gameObject, frameStyle);
                Debug.Log($"Win95PortraitFrameApplier: Applied styling to '{frame.gameObject.name}'");
            }
        }

        hasApplied = true;
    }

    /// <summary>
    /// Force re-application of styling (useful after portrait frames are recreated).
    /// </summary>
    public void ReapplyStyling()
    {
        hasApplied = false;
        ApplyWin95Styling();
    }
}
