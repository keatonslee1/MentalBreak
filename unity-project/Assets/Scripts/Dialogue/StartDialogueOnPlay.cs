using UnityEngine;
using Yarn.Unity;

public class StartDialogueOnPlay : MonoBehaviour {
	public string startNode = "R1_Start";
	public DialogueRunner dialogueRunner;

	// PlayerPrefs key for autosave slot 0 (matches SaveLoadManager)
	private const string AutosaveKey = "MB_Save_v2_Auto_0_Json";

	private void Awake() {
		if (dialogueRunner == null) {
			dialogueRunner = FindAnyObjectByType<DialogueRunner>();
		}
	}

	private void Start() {
		// Check if returning player with autosave data
		if (HasAutosave()) {
			Debug.Log("StartDialogueOnPlay: Returning player detected, loading autosave...");
			LoadAutosave();
			return;
		}

		// New player - start dialogue from beginning
		Debug.Log($"StartDialogueOnPlay: New player, starting dialogue at {startNode}");
		if (dialogueRunner != null && dialogueRunner.YarnProject != null) {
			dialogueRunner.StartDialogue(startNode);
		} else {
			Debug.LogError("StartDialogueOnPlay: DialogueRunner or YarnProject is missing.");
		}
	}

	/// <summary>
	/// Check if player has autosave data
	/// </summary>
	private bool HasAutosave() {
		return PlayerPrefs.HasKey(AutosaveKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(AutosaveKey, ""));
	}

	/// <summary>
	/// Load autosave slot 0
	/// </summary>
	private void LoadAutosave() {
		SaveLoadManager saveManager = FindAnyObjectByType<SaveLoadManager>();
		if (saveManager != null) {
			saveManager.LoadGame(0);
		} else if (SaveLoadManager.Instance != null) {
			SaveLoadManager.Instance.LoadGame(0);
		} else {
			Debug.LogError("StartDialogueOnPlay: SaveLoadManager not found, cannot load autosave!");
			// Fallback: start fresh
			if (dialogueRunner != null && dialogueRunner.YarnProject != null) {
				dialogueRunner.StartDialogue(startNode);
			}
		}
	}
}
