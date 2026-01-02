using UnityEngine;
using Yarn.Unity;

public class StartDialogueOnPlay : MonoBehaviour {
	public string startNode = "R1_Start";
	public DialogueRunner dialogueRunner;

	private void Awake() {
		if (dialogueRunner == null) {
			dialogueRunner = FindAnyObjectByType<DialogueRunner>();
		}
	}

	private void Start() {
		// Skip if WelcomeOverlay is waiting for user interaction
		// WelcomeOverlay will start dialogue after it's dismissed
		if (WelcomeOverlay.IsWaitingForDismissal)
		{
			Debug.Log("StartDialogueOnPlay: Waiting for WelcomeOverlay to be dismissed");
			return;
		}

		if (dialogueRunner != null && dialogueRunner.YarnProject != null) {
			dialogueRunner.StartDialogue(startNode);
		} else {
			Debug.LogError("StartDialogueOnPlay: DialogueRunner or YarnProject is missing.");
		}
	}
}