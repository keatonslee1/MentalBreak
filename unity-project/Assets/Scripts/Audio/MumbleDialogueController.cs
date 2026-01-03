using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using TMPro;
using MumbleGenerator;

/// <summary>
/// Integrates Mumble Generator with Yarn Spinner dialogue to play character-specific
/// mumble sounds when dialogue lines are displayed.
/// Follows the same pattern as PortraitTalkingStateController.
/// </summary>
public class MumbleDialogueController : ActionMarkupHandler
{
    [Header("Mumble Configuration")]
    [Tooltip("MumblePlayer component that will play the sounds")]
    [SerializeField] private MumblePlayer mumblePlayer;

    [Tooltip("Fallback mumble for characters without a specific voice")]
    [SerializeField] private Mumble defaultMumble;

    [Header("Character Voices")]
    [Tooltip("Map character names to their specific mumble voices")]
    [SerializeField] private List<CharacterMumbleEntry> characterMumbles = new List<CharacterMumbleEntry>();

    [System.Serializable]
    public class CharacterMumbleEntry
    {
        [Tooltip("Character name as it appears in Yarn dialogue (e.g., 'Alice', 'Supervisor')")]
        public string characterName;

        [Tooltip("Mumble ScriptableObject for this character's voice")]
        public Mumble mumble;
    }

    private Dictionary<string, Mumble> mumbleLookup;

    // Characters that should be silent (no mumble sounds)
    private HashSet<string> silentCharacters = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
    {
        "Player",
        "DarkFigure",
        "Dark Figure"
    };

    void Awake()
    {
        BuildLookup();

        // Try to find MumblePlayer if not assigned
        if (mumblePlayer == null)
        {
            mumblePlayer = GetComponent<MumblePlayer>();
            if (mumblePlayer == null)
            {
                Debug.LogError("MumbleDialogueController: MumblePlayer not assigned and not found on this GameObject!");
            }
        }
    }

    private void BuildLookup()
    {
        mumbleLookup = new Dictionary<string, Mumble>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var entry in characterMumbles)
        {
            if (!string.IsNullOrEmpty(entry.characterName) && entry.mumble != null)
            {
                mumbleLookup[entry.characterName] = entry.mumble;

                // Also add alias for Arthur -> Supervisor mapping
                if (entry.characterName.Equals("Supervisor", System.StringComparison.OrdinalIgnoreCase))
                {
                    mumbleLookup["Arthur"] = entry.mumble;
                }
            }
        }
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
        // No preparation needed
    }

    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
        if (mumblePlayer == null)
        {
            Debug.LogWarning("MumbleDialogueController: MumblePlayer is null, skipping mumble");
            return;
        }

        string characterName = GetCharacterName(line);

        // Skip if no character name found
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.Log("MumbleDialogueController: No character name in line, skipping mumble");
            return;
        }

        // Skip silent characters (Player, DarkFigure, etc.)
        if (silentCharacters.Contains(characterName))
        {
            Debug.Log($"MumbleDialogueController: Character '{characterName}' is silent, skipping mumble");
            return;
        }

        // Get appropriate mumble for this character
        Mumble mumble = GetMumbleForCharacter(characterName);
        if (mumble == null)
        {
            Debug.LogWarning($"MumbleDialogueController: No mumble found for '{characterName}' and no default set");
            return;
        }

        // Set the dialogue text and play
        mumble.Text = text.text;
        mumblePlayer.Mumble = mumble;
        mumblePlayer.PlayMumble();

        Debug.Log($"MumbleDialogueController: Playing mumble for '{characterName}'");
    }

    public override YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, System.Threading.CancellationToken cancellationToken)
    {
        // No per-character action needed - mumble plays on line start
        return YarnTask.CompletedTask;
    }

    public override void OnLineDisplayComplete()
    {
        // Stop mumble when text finishes displaying
        StopMumble();
    }

    public override void OnLineWillDismiss()
    {
        // Also stop mumble when line dismisses (e.g., player advances early)
        StopMumble();
    }

    /// <summary>
    /// Stops the currently playing mumble audio.
    /// </summary>
    private void StopMumble()
    {
        if (mumblePlayer == null)
            return;

        // Stop the mumble coroutine
        mumblePlayer.StopAllCoroutines();

        // Stop any currently playing audio on the AudioSources
        foreach (var audioSource in mumblePlayer.GetComponents<AudioSource>())
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Extracts character name from Yarn line markup.
    /// </summary>
    private string GetCharacterName(MarkupParseResult line)
    {
        if (line.TryGetAttributeWithName("character", out var characterAttribute))
        {
            if (characterAttribute.Properties.TryGetValue("name", out var nameProperty))
            {
                return nameProperty.StringValue;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the Mumble ScriptableObject for a character, falling back to default.
    /// </summary>
    private Mumble GetMumbleForCharacter(string characterName)
    {
        if (mumbleLookup.TryGetValue(characterName, out var mumble))
        {
            return mumble;
        }
        return defaultMumble;
    }

    /// <summary>
    /// Adds a character to the silent list at runtime.
    /// </summary>
    public void AddSilentCharacter(string characterName)
    {
        silentCharacters.Add(characterName);
    }

    /// <summary>
    /// Removes a character from the silent list at runtime.
    /// </summary>
    public void RemoveSilentCharacter(string characterName)
    {
        silentCharacters.Remove(characterName);
    }
}
