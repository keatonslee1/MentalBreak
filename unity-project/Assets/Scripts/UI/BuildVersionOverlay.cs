using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a small build identifier in the bottom-left corner of the screen.
/// </summary>
public class BuildVersionOverlay : MonoBehaviour
{
    // Disabled: this label conflicts with the unified HUD layout (Feedback button occupies bottom-left).
    private static readonly bool Enabled = false;
    private const string LabelText = "alpha 3.5";
    private const float HorizontalPadding = 24f;
    private const float VerticalPadding = 24f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureOverlayExists()
    {
        if (!Enabled)
        {
            return;
        }

        if (FindFirstObjectByType<BuildVersionOverlay>() != null)
        {
            return;
        }

        GameObject overlayObject = new GameObject(nameof(BuildVersionOverlay));
        DontDestroyOnLoad(overlayObject);

        BuildVersionOverlay overlay = overlayObject.AddComponent<BuildVersionOverlay>();
        overlay.Initialize();
    }

    private void Initialize()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue; // Ensure the label stays on top of other UI.

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        gameObject.AddComponent<GraphicRaycaster>();

        GameObject labelObject = new GameObject("BuildLabel");
        labelObject.transform.SetParent(transform, false);

        Text textComponent = labelObject.AddComponent<Text>();
        textComponent.text = LabelText;
        textComponent.fontSize = 20;
        textComponent.color = new Color(1f, 1f, 1f, 0.8f);
        textComponent.alignment = TextAnchor.LowerLeft;
        textComponent.raycastTarget = false;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.zero;
        rectTransform.anchoredPosition = new Vector2(HorizontalPadding, VerticalPadding);
    }
}

