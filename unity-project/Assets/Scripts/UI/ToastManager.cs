using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Toast notification types for different feedback scenarios.
/// </summary>
public enum ToastType
{
    Info,       // Neutral information (white/gray)
    Success,    // Positive feedback (green)
    Warning,    // Cautionary message (yellow/orange)
    Error       // Failure/error message (red)
}

/// <summary>
/// Manages toast notifications for user feedback.
/// Provides a simple API for showing temporary messages.
/// Usage: ToastManager.Show("Game saved!", ToastType.Success);
/// </summary>
public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("How long toasts are visible (seconds)")]
    public float defaultDuration = 2.5f;

    [Tooltip("Fade in/out duration (seconds)")]
    public float fadeDuration = 0.3f;

    [Tooltip("Maximum number of simultaneous toasts")]
    public int maxToasts = 3;

    [Header("Positioning")]
    [Tooltip("Vertical offset from bottom of screen")]
    public float bottomOffset = 100f;

    [Tooltip("Spacing between stacked toasts")]
    public float toastSpacing = 80f;

    [Header("Styling")]
    public Color infoColor = new Color(0.2f, 0.2f, 0.25f, 0.95f);
    public Color successColor = new Color(0.15f, 0.4f, 0.2f, 0.95f);
    public Color warningColor = new Color(0.5f, 0.4f, 0.1f, 0.95f);
    public Color errorColor = new Color(0.5f, 0.15f, 0.15f, 0.95f);

    public Color infoTextColor = Color.white;
    public Color successTextColor = new Color(0.7f, 1f, 0.7f);
    public Color warningTextColor = new Color(1f, 0.95f, 0.7f);
    public Color errorTextColor = new Color(1f, 0.7f, 0.7f);

    // Internal state
    private Canvas toastCanvas;
    private List<ToastInstance> activeToasts = new List<ToastInstance>();
    private Queue<QueuedToast> toastQueue = new Queue<QueuedToast>();

    private class ToastInstance
    {
        public GameObject gameObject;
        public CanvasGroup canvasGroup;
        public float createdTime;
        public float duration;
    }

    private class QueuedToast
    {
        public string message;
        public ToastType type;
        public float duration;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateToastCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Process toast queue if we have room
        while (toastQueue.Count > 0 && activeToasts.Count < maxToasts)
        {
            QueuedToast queued = toastQueue.Dequeue();
            CreateToast(queued.message, queued.type, queued.duration);
        }
    }

    /// <summary>
    /// Create the canvas for toast notifications.
    /// </summary>
    private void CreateToastCanvas()
    {
        // Create a separate canvas for toasts that renders on top of everything
        GameObject canvasObj = new GameObject("ToastCanvas");
        canvasObj.transform.SetParent(transform);

        toastCanvas = canvasObj.AddComponent<Canvas>();
        toastCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        toastCanvas.sortingOrder = 1000; // Render on top of other UI

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // No GraphicRaycaster - toasts shouldn't block input
    }

    /// <summary>
    /// Show a toast notification with default duration.
    /// </summary>
    public static void Show(string message, ToastType type = ToastType.Info)
    {
        if (Instance == null)
        {
            Debug.LogWarning($"ToastManager: Not initialized. Message: {message}");
            return;
        }

        Instance.ShowToast(message, type, Instance.defaultDuration);
    }

    /// <summary>
    /// Show a toast notification with custom duration.
    /// </summary>
    public static void Show(string message, ToastType type, float duration)
    {
        if (Instance == null)
        {
            Debug.LogWarning($"ToastManager: Not initialized. Message: {message}");
            return;
        }

        Instance.ShowToast(message, type, duration);
    }

    /// <summary>
    /// Convenience methods for specific toast types.
    /// </summary>
    public static void ShowSuccess(string message) => Show(message, ToastType.Success);
    public static void ShowError(string message) => Show(message, ToastType.Error);
    public static void ShowWarning(string message) => Show(message, ToastType.Warning);
    public static void ShowInfo(string message) => Show(message, ToastType.Info);

    /// <summary>
    /// Internal method to show or queue a toast.
    /// </summary>
    private void ShowToast(string message, ToastType type, float duration)
    {
        if (activeToasts.Count >= maxToasts)
        {
            // Queue the toast for later
            toastQueue.Enqueue(new QueuedToast
            {
                message = message,
                type = type,
                duration = duration
            });
            return;
        }

        CreateToast(message, type, duration);
    }

    /// <summary>
    /// Create and display a toast notification.
    /// </summary>
    private void CreateToast(string message, ToastType type, float duration)
    {
        // Create toast container
        GameObject toastObj = new GameObject("Toast");
        toastObj.transform.SetParent(toastCanvas.transform, false);

        RectTransform rect = toastObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(600f, 70f);

        // Position based on number of active toasts
        float yPos = bottomOffset + (activeToasts.Count * toastSpacing);
        rect.anchoredPosition = new Vector2(0f, yPos);

        // Add background image
        Image bg = toastObj.AddComponent<Image>();
        bg.color = GetBackgroundColor(type);
        bg.raycastTarget = false;

        // Add canvas group for fading
        CanvasGroup canvasGroup = toastObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(toastObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 8f);
        textRect.offsetMax = new Vector2(-20f, -8f);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }
        text.fontSize = 48;
        text.alignment = TextAlignmentOptions.Center;
        text.color = GetTextColor(type);
        text.raycastTarget = false;

        // Track the toast
        ToastInstance instance = new ToastInstance
        {
            gameObject = toastObj,
            canvasGroup = canvasGroup,
            createdTime = Time.unscaledTime,
            duration = duration
        };
        activeToasts.Add(instance);

        // Start fade in/out coroutine
        StartCoroutine(AnimateToast(instance));

        Debug.Log($"ToastManager: Showing {type} toast: \"{message}\"");
    }

    /// <summary>
    /// Animate toast fade in, wait, fade out.
    /// </summary>
    private IEnumerator AnimateToast(ToastInstance toast)
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (toast.canvasGroup != null)
            {
                toast.canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            }
            yield return null;
        }

        if (toast.canvasGroup != null)
        {
            toast.canvasGroup.alpha = 1f;
        }

        // Wait for display duration
        yield return new WaitForSecondsRealtime(toast.duration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (toast.canvasGroup != null)
            {
                toast.canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            }
            yield return null;
        }

        // Remove and destroy
        activeToasts.Remove(toast);
        if (toast.gameObject != null)
        {
            Destroy(toast.gameObject);
        }

        // Reposition remaining toasts
        RepositionToasts();
    }

    /// <summary>
    /// Reposition active toasts after one is removed.
    /// </summary>
    private void RepositionToasts()
    {
        for (int i = 0; i < activeToasts.Count; i++)
        {
            ToastInstance toast = activeToasts[i];
            if (toast.gameObject != null)
            {
                RectTransform rect = toast.gameObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    float targetY = bottomOffset + (i * toastSpacing);
                    StartCoroutine(AnimatePosition(rect, targetY));
                }
            }
        }
    }

    /// <summary>
    /// Smoothly animate toast position.
    /// </summary>
    private IEnumerator AnimatePosition(RectTransform rect, float targetY)
    {
        float startY = rect.anchoredPosition.y;
        float elapsed = 0f;
        float moveDuration = 0.15f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            float newY = Mathf.Lerp(startY, targetY, t);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, newY);
            yield return null;
        }

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, targetY);
    }

    /// <summary>
    /// Get background color for toast type.
    /// </summary>
    private Color GetBackgroundColor(ToastType type)
    {
        switch (type)
        {
            case ToastType.Success: return successColor;
            case ToastType.Warning: return warningColor;
            case ToastType.Error: return errorColor;
            default: return infoColor;
        }
    }

    /// <summary>
    /// Get text color for toast type.
    /// </summary>
    private Color GetTextColor(ToastType type)
    {
        switch (type)
        {
            case ToastType.Success: return successTextColor;
            case ToastType.Warning: return warningTextColor;
            case ToastType.Error: return errorTextColor;
            default: return infoTextColor;
        }
    }

    /// <summary>
    /// Clear all active and queued toasts.
    /// </summary>
    public static void ClearAll()
    {
        if (Instance == null) return;

        Instance.toastQueue.Clear();

        foreach (var toast in Instance.activeToasts)
        {
            if (toast.gameObject != null)
            {
                Destroy(toast.gameObject);
            }
        }
        Instance.activeToasts.Clear();
    }
}
