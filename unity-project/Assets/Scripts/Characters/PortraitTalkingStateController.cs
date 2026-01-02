using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;

/// <summary>
/// Integrates with Yarn Spinner's dialogue system to trigger talking animations on character portraits.
/// Implements IActionMarkupHandler to detect when dialogue lines start and complete.
/// </summary>
public class PortraitTalkingStateController : ActionMarkupHandler
{
    [Header("References")]
    [Tooltip("CharacterSpriteManager to control portrait animators")]
    public CharacterSpriteManager spriteManager;

    // Mapping from Yarn character names to character tags
    private Dictionary<string, string> characterNameToTag = new Dictionary<string, string>
    {
        { "Alice", "char_Alice" },
        { "Supervisor", "char_Supervisor" },
        { "Arthur", "char_Supervisor" }, // Alias
        { "Timmy", "char_Timmy" },
        { "BTC", "char_BTC" },
        { "Ari", "char_Ari" },
        { "Charlotte", "char_Charlotte" },
        { "Dina", "char_Dina" },
        { "Noam", "char_Noam" },
        { "Max", "char_Max" },
        { "Player", null }, // Player has no portrait
        { "DarkFigure", null }, // Dark Figure has no portrait
        { "Dark Figure", null }, // Alias
    };

    // Track currently talking character
    private string currentTalkingCharacterTag = null;

    void Awake()
    {
        // Find CharacterSpriteManager if not assigned
        if (spriteManager == null)
        {
            spriteManager = FindAnyObjectByType<CharacterSpriteManager>();
            if (spriteManager == null)
            {
                Debug.LogError("PortraitTalkingStateController: CharacterSpriteManager not found!");
            }
        }
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
        // No preparation needed for portrait animations
    }

    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
        if (spriteManager == null)
            return;

        // Get character name from line attributes
        string characterName = null;
        if (line.TryGetAttributeWithName("character", out var characterAttribute))
        {
            // Character name is in the attribute's properties dictionary with key "name"
            if (characterAttribute.Properties.TryGetValue("name", out var nameProperty))
            {
                characterName = nameProperty.StringValue;
            }
        }

        if (string.IsNullOrEmpty(characterName))
        {
            Debug.Log("PortraitTalkingStateController: No character name in line, skipping talking animation");
            return;
        }

        // Map character name to tag
        string characterTag = GetCharacterTag(characterName);

        if (characterTag == null)
        {
            Debug.Log($"PortraitTalkingStateController: Character '{characterName}' has no portrait, skipping animation");
            return;
        }

        // Get animator for this character
        CharacterPortraitAnimator animator = spriteManager.GetAnimatorForCharacter(characterTag);

        if (animator == null)
        {
            Debug.Log($"PortraitTalkingStateController: Character '{characterName}' ({characterTag}) is not currently displayed, skipping animation");
            return;
        }

        // Start talking animation
        animator.SetTalkingState(true);
        currentTalkingCharacterTag = characterTag;

        Debug.Log($"PortraitTalkingStateController: Started talking animation for '{characterName}' ({characterTag})");
    }

    public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
    {
        // No per-character action needed for portrait animations
        return YarnTask.CompletedTask;
    }

    public override void OnLineDisplayComplete()
    {
        if (spriteManager == null)
            return;

        // Stop talking animation for currently talking character
        if (!string.IsNullOrEmpty(currentTalkingCharacterTag))
        {
            CharacterPortraitAnimator animator = spriteManager.GetAnimatorForCharacter(currentTalkingCharacterTag);

            if (animator != null)
            {
                animator.SetTalkingState(false);
                Debug.Log($"PortraitTalkingStateController: Stopped talking animation for '{currentTalkingCharacterTag}'");
            }

            currentTalkingCharacterTag = null;
        }
    }

    public override void OnLineWillDismiss()
    {
        // Ensure talking stops when line dismisses (redundant safety)
        OnLineDisplayComplete();
    }

    /// <summary>
    /// Maps Yarn character name to character tag.
    /// Returns null if character has no portrait.
    /// </summary>
    private string GetCharacterTag(string characterName)
    {
        if (characterNameToTag.TryGetValue(characterName, out string tag))
        {
            return tag;
        }

        // Fallback: Try case-insensitive match
        foreach (var kvp in characterNameToTag)
        {
            if (string.Equals(kvp.Key, characterName, System.StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value;
            }
        }

        // If not in dictionary, try constructing tag (char_ + name)
        string constructedTag = $"char_{characterName}";
        Debug.LogWarning($"PortraitTalkingStateController: Unknown character '{characterName}', trying constructed tag '{constructedTag}'");
        return constructedTag;
    }
}
