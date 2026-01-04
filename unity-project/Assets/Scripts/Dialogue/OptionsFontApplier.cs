using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Watches for OptionItem instances created by Yarn Spinner and applies the global font.
///
/// OptionItems are instantiated from a prefab in Packages that has a hardcoded font.
/// GlobalFontOverride runs at scene load before options exist, so this component
/// catches them when they become active and applies the correct font.
///
/// Attach to: Options Presenter or Dialogue System
/// </summary>
public class OptionsFontApplier : MonoBehaviour
{
    // Standard dialogue font size (matches LinePresenter and other dialogue text)
    private const float DialogueFontSize = 60f;

    // Cache the text field from OptionItem (it's private in the Yarn Spinner package)
    private static FieldInfo optionItemTextField;
    // Cache the lastLineText field from OptionsPresenter
    private static FieldInfo lastLineTextField;
    private static FieldInfo lastLineCharacterNameTextField;
    private static bool fieldInfoCached;

    // Track which OptionItems we've already processed to avoid redundant work
    private HashSet<int> processedInstanceIds = new HashSet<int>();

    // Track if we've processed the OptionsPresenter's last line text
    private bool processedLastLineText;

    // Check frequency - balance between responsiveness and performance
    private float checkInterval = 0.05f; // 50ms
    private float nextCheckTime;

    private void Awake()
    {
        CacheFieldInfo();
    }

    private void OnEnable()
    {
        // Clear processed list when re-enabled (scene changes, etc.)
        processedInstanceIds.Clear();
        processedLastLineText = false;
        nextCheckTime = 0f;
    }

    private void Update()
    {
        if (Time.time < nextCheckTime)
        {
            return;
        }
        nextCheckTime = Time.time + checkInterval;

        ApplyFontToActiveOptions();
    }

    private static void CacheFieldInfo()
    {
        if (fieldInfoCached)
        {
            return;
        }

        fieldInfoCached = true;

        // Get the private 'text' field from OptionItem
        optionItemTextField = typeof(OptionItem).GetField(
            "text",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (optionItemTextField == null)
        {
            Debug.LogWarning("OptionsFontApplier: Could not find 'text' field on OptionItem. Font override will not work.");
        }

        // Get the lastLineText and lastLineCharacterNameText fields from OptionsPresenter
        lastLineTextField = typeof(OptionsPresenter).GetField(
            "lastLineText",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        lastLineCharacterNameTextField = typeof(OptionsPresenter).GetField(
            "lastLineCharacterNameText",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
    }

    private void ApplyFontToActiveOptions()
    {
        if (optionItemTextField == null)
        {
            return;
        }

        // Apply font to OptionsPresenter's last line text (only once per options display)
        if (!processedLastLineText)
        {
            ApplyFontToLastLineText();
        }

        // Find all active OptionItems
        OptionItem[] optionItems = FindObjectsByType<OptionItem>(FindObjectsSortMode.None);

        for (int i = 0; i < optionItems.Length; i++)
        {
            OptionItem item = optionItems[i];
            if (item == null || !item.isActiveAndEnabled)
            {
                continue;
            }

            int instanceId = item.GetInstanceID();
            if (processedInstanceIds.Contains(instanceId))
            {
                continue;
            }

            // Get the TMP_Text component via reflection
            TMP_Text textComponent = optionItemTextField.GetValue(item) as TMP_Text;
            if (textComponent != null)
            {
                GlobalFontOverride.ApplyFontToComponent(textComponent);
                textComponent.fontSize = DialogueFontSize;
            }

            processedInstanceIds.Add(instanceId);
        }
    }

    private void ApplyFontToLastLineText()
    {
        OptionsPresenter presenter = FindFirstObjectByType<OptionsPresenter>();
        if (presenter == null)
        {
            return;
        }

        // Check if any options are actually active (meaning we're showing options)
        OptionItem[] optionItems = FindObjectsByType<OptionItem>(FindObjectsSortMode.None);
        bool hasActiveOptions = false;
        for (int i = 0; i < optionItems.Length; i++)
        {
            if (optionItems[i] != null && optionItems[i].isActiveAndEnabled)
            {
                hasActiveOptions = true;
                break;
            }
        }

        if (!hasActiveOptions)
        {
            return;
        }

        // Apply font to lastLineText
        if (lastLineTextField != null)
        {
            TMP_Text lastLineText = lastLineTextField.GetValue(presenter) as TMP_Text;
            if (lastLineText != null)
            {
                GlobalFontOverride.ApplyFontToComponent(lastLineText);
                lastLineText.fontSize = DialogueFontSize;
            }
        }

        // Apply font to lastLineCharacterNameText
        if (lastLineCharacterNameTextField != null)
        {
            TMP_Text nameText = lastLineCharacterNameTextField.GetValue(presenter) as TMP_Text;
            if (nameText != null)
            {
                GlobalFontOverride.ApplyFontToComponent(nameText);
                nameText.fontSize = DialogueFontSize;
            }
        }

        processedLastLineText = true;
    }

    /// <summary>
    /// Called when options are dismissed - clear the processed list
    /// so we re-apply fonts if the same OptionItems are reused.
    /// </summary>
    public void OnOptionsCleared()
    {
        processedInstanceIds.Clear();
        processedLastLineText = false;
    }
}
