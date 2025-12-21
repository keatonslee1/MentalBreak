#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to select (and if needed create) a 16:9 Game View aspect preset.
/// Unity does not provide a stable public API for this; this uses reflection with safe fallbacks.
/// </summary>
public static class GameViewAspect16x9
{
    [MenuItem("Tools/Mental Break/Set GameView 16:9")]
    public static void SetGameViewTo16x9()
    {
        try
        {
            Ensure16x9ExistsAndSelect();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"GameViewAspect16x9: Could not set Game View via reflection ({ex.GetType().Name}: {ex.Message}). " +
                             "You can manually set Game View aspect to 16:9 using the Game View dropdown.");
        }
    }

    private static void Ensure16x9ExistsAndSelect()
    {
        // Internal types
        Assembly editorAsm = typeof(Editor).Assembly;
        Type gameViewType = editorAsm.GetType("UnityEditor.GameView");
        Type gameViewSizesType = editorAsm.GetType("UnityEditor.GameViewSizes");
        Type gameViewSizeType = editorAsm.GetType("UnityEditor.GameViewSize");
        Type gameViewSizeTypeEnum = editorAsm.GetType("UnityEditor.GameViewSizeType");
        Type scriptableSingletonType = editorAsm.GetType("UnityEditor.ScriptableSingleton`1");
        Type sizeGroupTypeEnum = editorAsm.GetType("UnityEditor.GameViewSizeGroupType");

        if (gameViewType == null || gameViewSizesType == null || gameViewSizeType == null || gameViewSizeTypeEnum == null || scriptableSingletonType == null || sizeGroupTypeEnum == null)
        {
            throw new InvalidOperationException("Unity editor internals changed; required GameView reflection types not found.");
        }

        // ScriptableSingleton<GameViewSizes>.instance
        Type singletonClosed = scriptableSingletonType.MakeGenericType(gameViewSizesType);
        PropertyInfo instanceProp = singletonClosed.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
        object sizesInstance = instanceProp?.GetValue(null);
        if (sizesInstance == null)
        {
            throw new InvalidOperationException("Could not get GameViewSizes instance.");
        }

        // sizesInstance.GetGroup(GameViewSizeGroupType.Standalone)
        int standaloneGroup = (int)Enum.Parse(sizeGroupTypeEnum, "Standalone");
        MethodInfo getGroup = gameViewSizesType.GetMethod("GetGroup", BindingFlags.Public | BindingFlags.Instance);
        object group = getGroup?.Invoke(sizesInstance, new object[] { standaloneGroup });
        if (group == null)
        {
            throw new InvalidOperationException("Could not get GameView size group.");
        }

        // group.GetDisplayTexts() to find existing "16:9"
        MethodInfo getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts", BindingFlags.Public | BindingFlags.Instance);
        string[] display = getDisplayTexts?.Invoke(group, null) as string[];
        if (display == null)
        {
            display = Array.Empty<string>();
        }

        int index = Array.FindIndex(display, s => s != null && (s.Contains("16:9") || s.Contains("16: 9")));
        if (index < 0)
        {
            // Create new custom aspect size.
            int aspectEnumValue = (int)Enum.Parse(gameViewSizeTypeEnum, "AspectRatio");
            object newSize = Activator.CreateInstance(gameViewSizeType, aspectEnumValue, 16, 9, "16:9");

            MethodInfo addCustomSize = group.GetType().GetMethod("AddCustomSize", BindingFlags.Public | BindingFlags.Instance);
            addCustomSize?.Invoke(group, new object[] { newSize });

            // Re-fetch display texts and find index again.
            display = getDisplayTexts?.Invoke(group, null) as string[] ?? Array.Empty<string>();
            index = Array.FindIndex(display, s => s != null && (s.Contains("16:9") || s.Contains("16: 9")));
        }

        if (index < 0)
        {
            throw new InvalidOperationException("Failed to create/find 16:9 Game View preset.");
        }

        // Select in GameView window
        EditorWindow gv = EditorWindow.GetWindow(gameViewType);
        if (gv == null)
        {
            throw new InvalidOperationException("Could not open GameView window.");
        }

        PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.Instance);
        if (selectedSizeIndex == null)
        {
            // Some Unity versions expose it as a field.
            FieldInfo selectedField = gameViewType.GetField("m_SelectedSizeIndex", BindingFlags.NonPublic | BindingFlags.Instance);
            if (selectedField == null)
            {
                throw new InvalidOperationException("Could not set GameView selected size index.");
            }
            selectedField.SetValue(gv, index);
        }
        else
        {
            selectedSizeIndex.SetValue(gv, index);
        }

        gv.Repaint();
    }
}
#endif



