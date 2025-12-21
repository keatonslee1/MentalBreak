using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Provides a deterministic, always-on modal overlay Canvas for runtime-created modal UI.
/// Intended to sit ABOVE OverlayUIRoot and all other UI.
/// </summary>
public sealed class ModalCanvasProvider : MonoBehaviour
{
    private const string ModalObjectName = "ModalUIRoot";
    private const int ModalSortingOrder = 3000;

    private static ModalCanvasProvider instance;
    private Canvas modalCanvas;

    [Header("Diagnostics")]
    [SerializeField] private bool enableVerboseLogging = false;

    /// <summary>
    /// Returns the modal Canvas, creating it if necessary.
    /// </summary>
    public static Canvas GetCanvas()
    {
        EnsureExists();
        return instance != null ? instance.modalCanvas : null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (instance != null)
        {
            return;
        }

        instance = FindFirstObjectByType<ModalCanvasProvider>();
        if (instance != null)
        {
            instance.EnsureInitialized();
            return;
        }

        GameObject go = GameObject.Find(ModalObjectName);
        if (go == null)
        {
            go = new GameObject(ModalObjectName);
        }

        instance = go.GetComponent<ModalCanvasProvider>();
        if (instance == null)
        {
            instance = go.AddComponent<ModalCanvasProvider>();
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

        modalCanvas = GetComponent<Canvas>();
        if (modalCanvas == null)
        {
            modalCanvas = gameObject.AddComponent<Canvas>();
        }

        modalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        modalCanvas.sortingOrder = ModalSortingOrder;

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
            Debug.Log($"ModalCanvasProvider: Ready on '{gameObject.name}' (renderMode={modalCanvas.renderMode}, sortingOrder={modalCanvas.sortingOrder}).", this);
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


