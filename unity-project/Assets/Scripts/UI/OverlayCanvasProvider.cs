using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a deterministic, always-on overlay Canvas for runtime-created UI.
/// Avoids nondeterministic Canvas selection in builds (notably WebGL/IL2CPP).
/// </summary>
public sealed class OverlayCanvasProvider : MonoBehaviour
{
    private const string OverlayObjectName = "OverlayUIRoot";
    private const int OverlaySortingOrder = 2000;

    private static OverlayCanvasProvider instance;
    private Canvas overlayCanvas;

    [Header("Diagnostics")]
    [SerializeField] private bool enableVerboseLogging = false;

    /// <summary>
    /// Returns the overlay Canvas, creating it if necessary.
    /// </summary>
    public static Canvas GetCanvas()
    {
        EnsureExists();
        return instance != null ? instance.overlayCanvas : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<OverlayCanvasProvider>();
        if (instance != null)
        {
            instance.EnsureInitialized();
            return;
        }

        GameObject go = GameObject.Find(OverlayObjectName);
        if (go == null)
        {
            go = new GameObject(OverlayObjectName);
        }

        instance = go.GetComponent<OverlayCanvasProvider>();
        if (instance == null)
        {
            instance = go.AddComponent<OverlayCanvasProvider>();
        }

        instance.EnsureInitialized();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        DontDestroyOnLoad(gameObject);

        overlayCanvas = GetComponent<Canvas>();
        if (overlayCanvas == null)
        {
            overlayCanvas = gameObject.AddComponent<Canvas>();
        }

        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = OverlaySortingOrder;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (enableVerboseLogging)
        {
            Debug.Log($"OverlayCanvasProvider: Ready on '{gameObject.name}' (renderMode={overlayCanvas.renderMode}, sortingOrder={overlayCanvas.sortingOrder}).", this);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}


