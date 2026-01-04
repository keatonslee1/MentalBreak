using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Yarn.Unity;

/// <summary>
/// Click-to-start overlay that unlocks audio and shows welcome message.
/// First-time players see "Welcome to Bigger Tech."
/// Returning players see "Welcome Back."
/// </summary>
public class WelcomeOverlay : MonoBehaviour {
	public static bool IsWaitingForDismissal { get; private set; } = false;
	public static event System.Action OnDismissed;

	private CanvasGroup canvasGroup;
	private bool isDismissing = false;
	private static WelcomeOverlay instance;
	private System.IDisposable inputLock;
	private readonly List<MonoBehaviour> disabledInputComponents = new List<MonoBehaviour>();

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void Initialize() {
		// Only create in MVPScene (the game scene), not MainMenu
		string sceneName = SceneManager.GetActiveScene().name;
		if (sceneName != "MVPScene") return;

		// Prevent duplicates
		if (instance != null) return;

		// Check if player has any saves (returning player)
		bool isReturningPlayer = HasAnySave();

		// Create the overlay
		CreateOverlay(isReturningPlayer);
		IsWaitingForDismissal = true;
	}

	private static bool HasAnySave() {
		SaveLoadManager saveManager = Object.FindAnyObjectByType<SaveLoadManager>();
		if (saveManager == null) saveManager = SaveLoadManager.Instance;
		if (saveManager == null) return false;

		var allSaves = saveManager.GetAllSaveSlots();
		return allSaves != null && allSaves.Count > 0;
	}

	private static void CreateOverlay(bool isReturningPlayer) {
		// Create root GameObject
		GameObject overlayGO = new GameObject("WelcomeOverlay");
		Object.DontDestroyOnLoad(overlayGO);
		instance = overlayGO.AddComponent<WelcomeOverlay>();

		// Canvas
		Canvas canvas = overlayGO.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 5000; // Above everything

		// Canvas Scaler
		CanvasScaler scaler = overlayGO.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920f, 1080f);
		scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
		scaler.matchWidthOrHeight = 0.5f;

		// Graphic Raycaster for click detection
		overlayGO.AddComponent<GraphicRaycaster>();

		// Canvas Group for fade
		instance.canvasGroup = overlayGO.AddComponent<CanvasGroup>();

		// Background (95% opaque black)
		GameObject bgGO = new GameObject("Background");
		bgGO.transform.SetParent(overlayGO.transform, false);
		Image bgImage = bgGO.AddComponent<Image>();
		bgImage.color = new Color(0f, 0f, 0f, 0.98f);
		bgImage.raycastTarget = true; // Catch clicks
		RectTransform bgRect = bgGO.GetComponent<RectTransform>();
		bgRect.anchorMin = Vector2.zero;
		bgRect.anchorMax = Vector2.one;
		bgRect.offsetMin = Vector2.zero;
		bgRect.offsetMax = Vector2.zero;

		// Top-left text: "Bigger Tech Corp - Employee Onboarding Terminal"
		CreateText(overlayGO.transform, "TopLeftText",
			"Bigger Tech Corp - Employee Onboarding Terminal",
			new Vector2(0, 1), new Vector2(0, 1), // anchor top-left
			new Vector2(30, -30), // position offset from top-left
			new Vector2(0, 1), // pivot top-left
			48, FontStyles.Normal, TextAlignmentOptions.TopLeft,
			new Color(0.7f, 0.7f, 0.7f, 1f));

		// Center welcome text
		string welcomeText = isReturningPlayer ? "Welcome Back." : "Welcome to Bigger Tech.";
		CreateText(overlayGO.transform, "WelcomeText",
			welcomeText,
			new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), // anchor center-ish
			Vector2.zero,
			new Vector2(0.5f, 0.5f), // pivot center
			96, FontStyles.Bold, TextAlignmentOptions.Center,
			Color.white);

		// "< Press to Continue >" text
		CreateText(overlayGO.transform, "ContinueText",
			"< Press to Continue >",
			new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), // below center
			Vector2.zero,
			new Vector2(0.5f, 0.5f), // pivot center
			60, FontStyles.Normal, TextAlignmentOptions.Center,
			new Color(0.8f, 0.8f, 0.8f, 1f));

		// Bottom center text
		CreateText(overlayGO.transform, "BottomText",
			"For questions, refer to your employee handbook,\nor e-mail employee_relations@biggertech.com.",
			new Vector2(0.5f, 0), new Vector2(0.5f, 0), // anchor bottom-center
			new Vector2(0, 177), // offset up from bottom
			new Vector2(0.5f, 0), // pivot bottom-center
			48, FontStyles.Normal, TextAlignmentOptions.Center,
			new Color(0.5f, 0.5f, 0.5f, 1f));

		// Request font application
		GlobalFontOverride.RequestApply();

		// Acquire input lock to block dialogue progression
		instance.inputLock = ModalInputLock.Acquire(instance);

		// Disable Yarn Spinner's built-in input components
		instance.DisableDialogueInput();

		Debug.Log($"[WelcomeOverlay] Created for {(isReturningPlayer ? "returning" : "new")} player");
	}

	private static void CreateText(Transform parent, string name, string content,
		Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 pivot,
		float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color) {

		GameObject textGO = new GameObject(name);
		textGO.transform.SetParent(parent, false);

		TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
		tmp.text = content;
		tmp.fontSize = fontSize;
		tmp.fontStyle = fontStyle;
		tmp.alignment = alignment;
		tmp.color = color;
		tmp.raycastTarget = false;

		RectTransform rect = textGO.GetComponent<RectTransform>();
		rect.pivot = pivot;
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.anchoredPosition = anchoredPosition;
		rect.sizeDelta = new Vector2(1800, 200); // Wide enough for text
	}

	private void DisableDialogueInput() {
		disabledInputComponents.Clear();

		// DON'T disable LineAdvancer - it needs to stay enabled to track line completion state
		// ModalInputLock will block input via ClickAdvancer (which handles clicks, Space, and Enter)

		// Only disable BubbleInput if using Speech Bubbles (it has its own keyboard handling)
		MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
		foreach (MonoBehaviour behaviour in allBehaviours) {
			if (behaviour != null && behaviour.enabled) {
				string typeName = behaviour.GetType().Name;
				if (typeName == "BubbleInput") {
					disabledInputComponents.Add(behaviour);
					behaviour.enabled = false;
				}
			}
		}

		Debug.Log($"[WelcomeOverlay] Disabled {disabledInputComponents.Count} input components (LineAdvancer stays enabled for state tracking)");
	}

	private void EnableDialogueInput() {
		foreach (MonoBehaviour component in disabledInputComponents) {
			if (component != null) {
				component.enabled = true;
			}
		}
		Debug.Log($"[WelcomeOverlay] Re-enabled {disabledInputComponents.Count} input components");
		disabledInputComponents.Clear();
	}

	private void Update() {
		if (isDismissing) return;

		// Only respond to mouse clicks, not keyboard
		var mouse = Mouse.current;
		if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)) {
			StartCoroutine(FadeAndDismiss());
		}
	}

	private IEnumerator FadeAndDismiss() {
		isDismissing = true;
		Debug.Log("[WelcomeOverlay] Dismissing...");

		float elapsed = 0f;
		const float fadeDuration = 0.5f;

		while (elapsed < fadeDuration) {
			elapsed += Time.deltaTime;
			canvasGroup.alpha = 1f - (elapsed / fadeDuration);
			yield return null;
		}

		canvasGroup.alpha = 0f;
		IsWaitingForDismissal = false;

		// Re-enable dialogue input components
		EnableDialogueInput();

		// Release input lock before destroying
		inputLock?.Dispose();
		inputLock = null;

		Debug.Log("[WelcomeOverlay] Dismissed, firing OnDismissed event");
		OnDismissed?.Invoke();

		instance = null;
		Destroy(gameObject);
	}

	private void OnDestroy() {
		// Ensure dialogue input is re-enabled
		EnableDialogueInput();

		// Ensure input lock is released
		inputLock?.Dispose();
		inputLock = null;

		if (instance == this) {
			instance = null;
		}
	}
}
