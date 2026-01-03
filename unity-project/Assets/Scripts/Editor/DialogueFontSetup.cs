using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Reflection;
using Yarn.Unity;
using Yarn.Unity.Addons.DialogueWheel;
using Yarn.Unity.Addons.SpeechBubbles;

/// <summary>
/// Editor script to set font sizes for dialogue and choice text.
/// Sets all dialogue-related TMP_Text to 60px.
/// Usage: Unity Menu -> Tools -> Setup Dialogue Font Sizes
/// </summary>
public class DialogueFontSetup : EditorWindow
{
    private const int DialogueFontSize = 60;

    [MenuItem("Tools/Setup Dialogue Font Sizes")]
    public static void SetupDialogueFonts()
    {
        int updated = 0;

        // Find and update LinePresenter text fields
        var linePresenters = FindObjectsByType<LinePresenter>(FindObjectsSortMode.None);
        foreach (var presenter in linePresenters)
        {
            if (presenter.lineText != null)
            {
                presenter.lineText.fontSize = DialogueFontSize;
                EditorUtility.SetDirty(presenter.lineText);
                updated++;
                Debug.Log($"Updated LinePresenter.lineText: {presenter.lineText.gameObject.name}");
            }

            if (presenter.characterNameText != null)
            {
                presenter.characterNameText.fontSize = DialogueFontSize;
                EditorUtility.SetDirty(presenter.characterNameText);
                updated++;
                Debug.Log($"Updated LinePresenter.characterNameText: {presenter.characterNameText.gameObject.name}");
            }
        }

        // Find and update WheelOptionView text fields (protected, need reflection)
        var wheelOptions = FindObjectsByType<WheelOptionView>(FindObjectsSortMode.None);
        foreach (var option in wheelOptions)
        {
            // Use reflection to access protected optionText field
            var field = typeof(WheelOptionView).GetField("optionText",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field != null)
            {
                var textField = field.GetValue(option) as TMP_Text;
                if (textField != null)
                {
                    textField.fontSize = DialogueFontSize;
                    EditorUtility.SetDirty(textField);
                    updated++;
                    Debug.Log($"Updated WheelOptionView.optionText: {textField.gameObject.name}");
                }
            }

            // Also check for serialized field via SerializedObject
            var so = new SerializedObject(option);
            var prop = so.FindProperty("optionText");
            if (prop != null && prop.objectReferenceValue is TMP_Text tmpText)
            {
                tmpText.fontSize = DialogueFontSize;
                EditorUtility.SetDirty(tmpText);
                // Don't double-count
            }
        }

        // Find and update BubbleContentView text fields
        var bubbleViews = FindObjectsByType<BubbleContentView>(FindObjectsSortMode.None);
        foreach (var bubble in bubbleViews)
        {
            if (bubble.textField != null)
            {
                bubble.textField.fontSize = DialogueFontSize;
                EditorUtility.SetDirty(bubble.textField);
                updated++;
                Debug.Log($"Updated BubbleContentView.textField: {bubble.textField.gameObject.name}");
            }
        }

        // Find any TMP_Text with dialogue-related names that might have been missed
        var allTmpTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (var tmp in allTmpTexts)
        {
            string name = tmp.gameObject.name.ToLower();
            string parentName = tmp.transform.parent?.name.ToLower() ?? "";

            // Look for dialogue-related text objects by naming convention
            bool isDialogueText =
                name.Contains("linetext") ||
                name.Contains("line text") ||
                name.Contains("dialoguetext") ||
                name.Contains("dialogue text") ||
                parentName.Contains("linepresenter") ||
                parentName.Contains("line presenter");

            if (isDialogueText && tmp.fontSize != DialogueFontSize)
            {
                tmp.fontSize = DialogueFontSize;
                EditorUtility.SetDirty(tmp);
                updated++;
                Debug.Log($"Updated dialogue text by name pattern: {tmp.gameObject.name}");
            }
        }

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"DialogueFontSetup: Updated {updated} text components to {DialogueFontSize}px");

        EditorUtility.DisplayDialog("Dialogue Font Setup",
            $"Updated {updated} text components to {DialogueFontSize}px.\n\n" +
            "Components updated:\n" +
            "- LinePresenter.lineText\n" +
            "- LinePresenter.characterNameText\n" +
            "- WheelOptionView.optionText\n" +
            "- BubbleContentView.textField\n\n" +
            "Remember to save the scene!",
            "OK");
    }
}
