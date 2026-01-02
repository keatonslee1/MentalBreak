using UnityEngine;
using Yarn.Unity;
using System.Linq;

public class StartDialogueOnPlay : MonoBehaviour {
	public string startNode = "R1_Start";
	public DialogueRunner dialogueRunner;

	private void Awake() {
		if (dialogueRunner == null) {
			dialogueRunner = FindAnyObjectByType<DialogueRunner>();
		}
	}

	private void Start() {
		// Find most recent save across all slots (manual + autosave)
		var mostRecentSave = FindMostRecentSave();

		if (mostRecentSave != null) {
			Debug.Log($"StartDialogueOnPlay: Returning player detected, loading slot {mostRecentSave.slot} ({mostRecentSave.timestamp})");
			LoadSave(mostRecentSave.slot);
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
	/// Find the most recent save across all slots (manual 1-5 and autosave 0, -1, -2)
	/// </summary>
	private SaveSlotData FindMostRecentSave() {
		SaveLoadManager saveManager = FindAnyObjectByType<SaveLoadManager>();
		if (saveManager == null) saveManager = SaveLoadManager.Instance;
		if (saveManager == null) return null;

		var allSaves = saveManager.GetAllSaveSlots();
		if (allSaves == null || allSaves.Count == 0) return null;

		// Find most recent by timestamp (ISO 8601 format is string-sortable)
		return allSaves
			.Where(s => !string.IsNullOrEmpty(s.timestamp))
			.OrderByDescending(s => s.timestamp)
			.FirstOrDefault();
	}

	/// <summary>
	/// Load a save from the specified slot
	/// </summary>
	private void LoadSave(int slot) {
		SaveLoadManager saveManager = FindAnyObjectByType<SaveLoadManager>();
		if (saveManager != null) {
			saveManager.LoadGame(slot);
		} else if (SaveLoadManager.Instance != null) {
			SaveLoadManager.Instance.LoadGame(slot);
		} else {
			Debug.LogError("StartDialogueOnPlay: SaveLoadManager not found!");
			// Fallback: start fresh
			if (dialogueRunner != null && dialogueRunner.YarnProject != null) {
				dialogueRunner.StartDialogue(startNode);
			}
		}
	}
}
