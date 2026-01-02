using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

#if USE_TMP
using TMPro;
#endif

/// <summary>
/// Welcome overlay that appears on game start.
/// Shows different messages for new vs returning players.
/// Ensures audio unlock via pointer click before game starts.
/// </summary>
public class WelcomeOverlay : MonoBehaviour, IPointerClickHandler
{
    private static WelcomeOverlay instance;
    private CanvasGroup canvasGroup;
    private bool isReturningPlayer = false;
    private bool isClicked = false;

    // Welcome messages
    private const string NEW_PLAYER_MESSAGE = "Welcome to Bigger Tech Corp.\n\n" +
        "If you have questions, please refer to the\n" +
        "employee handbook or email onboarding@biggertech.com.\n\n" +
        "< click anywhere to proceed >";

    private const string RETURNING_PLAYER_MESSAGE = "Welcome Back!\n\n" +
        "< Press anywhere to continue >";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (instance == null)
        {
            var go = new GameObject("WelcomeOverlay");
            instance = go.AddComponent<WelcomeOverlay>();
            DontDestroyOnLoad(go);
            Debug.Log("[WelcomeOverlay] Auto-created");
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        CreateOverlayUI();
    }

    private void CreateOverlayUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("WelcomeOverlayCanvas");
        canvasObj.transform.SetParent(transform);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it's on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create overlay panel (black background)
        GameObject panelObj = new GameObject("OverlayPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f); // 80% opacity black
        panelImage.raycastTarget = true; // Enable clicking

        // Add CanvasGroup for fade animation
        canvasGroup = panelObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        // Check if returning player
        isReturningPlayer = CheckIfReturningPlayer();

        // Create text
        GameObject textObj = new GameObject("WelcomeText");
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(1200, 600);

#if USE_TMP
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = isReturningPlayer ? RETURNING_PLAYER_MESSAGE : NEW_PLAYER_MESSAGE;
        textComponent.fontSize = isReturningPlayer ? 40 : 28;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.enableWordWrapping = true;
#else
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = isReturningPlayer ? RETURNING_PLAYER_MESSAGE : NEW_PLAYER_MESSAGE;
        textComponent.fontSize = isReturningPlayer ? 40 : 28;
        textComponent.color = Color.white;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
        textComponent.verticalOverflow = VerticalWrapMode.Truncate;

        // Try to find Arial font
        Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (arial != null)
        {
            textComponent.font = arial;
        }
#endif

        // Add click handler to panel
        panelObj.AddComponent<WelcomeOverlayClickHandler>().overlay = this;

        Debug.Log($"[WelcomeOverlay] Created overlay - Returning player: {isReturningPlayer}");
    }

    private bool CheckIfReturningPlayer()
    {
        // Check if autosave slot 0 exists
        SaveLoadManager saveManager = FindFirstObjectByType<SaveLoadManager>();
        if (saveManager == null)
        {
            Debug.LogWarning("[WelcomeOverlay] SaveLoadManager not found, assuming new player");
            return false;
        }

        bool hasSave = saveManager.HasSaveData(0);
        Debug.Log($"[WelcomeOverlay] Autosave slot 0 exists: {hasSave}");
        return hasSave;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // This is for direct clicks on this component
        HandleClick();
    }

    public void HandleClick()
    {
        if (isClicked) return;
        isClicked = true;

        Debug.Log("[WelcomeOverlay] Clicked! Starting fade out...");

        // Start fade out
        StartCoroutine(FadeOutAndProceed());
    }

    private IEnumerator FadeOutAndProceed()
    {
        float fadeDuration = 0.4f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // If returning player, load the autosave
        if (isReturningPlayer)
        {
            Debug.Log("[WelcomeOverlay] Loading autosave slot 0...");
            SaveLoadManager saveManager = FindFirstObjectByType<SaveLoadManager>();
            if (saveManager != null)
            {
                saveManager.LoadGame(0);
            }
            else
            {
                Debug.LogError("[WelcomeOverlay] SaveLoadManager not found, cannot load save!");
            }
        }
        else
        {
            Debug.Log("[WelcomeOverlay] New player - letting game start normally");
        }

        // Destroy overlay
        Destroy(gameObject);
    }
}

/// <summary>
/// Helper component to handle clicks on the overlay panel
/// </summary>
public class WelcomeOverlayClickHandler : MonoBehaviour, IPointerClickHandler
{
    public WelcomeOverlay overlay;

    public void OnPointerClick(PointerEventData eventData)
    {
        overlay?.HandleClick();
    }
}
