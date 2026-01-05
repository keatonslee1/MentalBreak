using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

using TMPro;

/// <summary>
/// Displays Engagement, Sanity, and Suspicion percentages on the right side of the screen.
/// Engagement is displayed in fuchsia, Sanity in cyan, Suspicion in orange.
/// Updates in real-time from Yarn variables.
/// Suspicion panel only appears when $suspicion_hud_active is true (granted by Noam in Day 2).
/// </summary>
public class MetricsPanelUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The DialogueRunner to read variables from")]
    public DialogueRunner dialogueRunner;

    [Header("Settings")]
    [Tooltip("Update interval in seconds")]
    public float updateInterval = 0.1f;

    [Header("Diagnostics")]
    [Tooltip("Emit verbose logs about canvas selection and layout calculations.")]
    public bool enableVerboseLogging = false;

    [Tooltip("Minimum font size for metric text")]
    public int minFontSize = 48;

    [Tooltip("Font size for metric text (clamped by Min Font Size)")]
    public int fontSize = 48;

    [Tooltip("Spacing between metrics")]
    public float metricSpacing = 20f;

    [Tooltip("Preferred height for each metric panel")]
    public float panelPreferredHeight = 68f;

    [Tooltip("Minimum width for each metric panel")]
    public float panelMinWidth = 340f;

    [Tooltip("Maximum width for each metric panel")]
    public float panelMaxWidth = 520f;

    [Tooltip("Fraction of the safe-area width used by the metric panels")]
    [Range(0.15f, 0.5f)]
    public float widthFraction = 0.34f;

    [Tooltip("Distance from the safe area's right edge")]
    public float rightMargin = 20f;

    [Tooltip("Distance from the safe area's top edge")]
    public float topMargin = 20f;

    [Tooltip("Extra width (in pixels) added to the computed metrics root width to avoid text clipping")]
    public float widthBonus = 50f;

    [Tooltip("Padding inside each metric panel (x = left/right, y = top/bottom)")]
    public Vector2 panelPadding = new Vector2(20f, 10f);

    [Header("Bar Style")]
    [Tooltip("Height of the filled bar area (in pixels)")]
    public float barHeight = 20f;

    [Tooltip("Vertical spacing between header row and bar")]
    public float barSpacing = 10f;

    [Tooltip("Background color for the bar track (behind the fill)")]
    public Color barTrackColor = new Color(1f, 1f, 1f, 0.06f);

    [Tooltip("Background color used for metric panels")]
    public Color panelBackgroundColor = new Color(0f, 0f, 0f, 0.95f);

    [Header("Tuning")]
    [Tooltip("If enabled, the script will overwrite inspector values with the defaults defined in code, ensuring consistent layout in builds.")]
    public bool enforceRuntimeDefaults = true;

    // Colors
    private readonly Color engagementColor = new Color(1f, 0f, 1f, 1f); // Fuchsia #FF00FF
    private readonly Color sanityColor = new Color(0f, 1f, 1f, 1f); // Cyan #00FFFF
    private readonly Color suspicionColorLow = new Color(0.3f, 0.9f, 0.4f, 1f); // Green-ish for low suspicion
    private readonly Color suspicionColorMid = new Color(1f, 0.6f, 0f, 1f); // Orange for medium suspicion
    private readonly Color suspicionColorHigh = new Color(1f, 0.2f, 0.2f, 1f); // Red for high suspicion

    private VariableStorageBehaviour variableStorage;
    private DialogueRuntimeWatcher runtimeWatcher;
    private float lastUpdateTime = 0f;

    // Layout references
    private GameObject metricsRoot;
    private VerticalLayoutGroup rootLayoutGroup;
    private ContentSizeFitter rootFitter;
    private Vector2 lastScreenSize = Vector2.zero;
    private Rect lastSafeArea = new Rect();

    // UI References
    private GameObject engagementPanel;
    private GameObject sanityPanel;
    private GameObject suspicionPanel;
    private Canvas canvas;

    private TextMeshProUGUI engagementLabelText;
    private TextMeshProUGUI engagementValueText;
    private Image engagementFillImage;

    private TextMeshProUGUI sanityLabelText;
    private TextMeshProUGUI sanityValueText;
    private Image sanityFillImage;

    private TextMeshProUGUI suspicionLabelText;
    private TextMeshProUGUI suspicionValueText;
    private Image suspicionFillImage;

    // Background images for animation highlighting
    private Image engagementBackground;
    private Image sanityBackground;
    private Image suspicionBackground;

    private static Sprite runtimeWhiteSprite;

    // Flag to skip instant updates when MetricsAnimator handles animation
    private bool useAnimatedUpdates = false;

    #region Public Accessors for MetricsAnimator

    /// <summary>Gets or sets whether animated updates are being used (skips instant fill updates).</summary>
    public bool UseAnimatedUpdates
    {
        get => useAnimatedUpdates;
        set => useAnimatedUpdates = value;
    }

    /// <summary>Gets the Engagement panel GameObject.</summary>
    public GameObject EngagementPanel => engagementPanel;

    /// <summary>Gets the Sanity panel GameObject.</summary>
    public GameObject SanityPanel => sanityPanel;

    /// <summary>Gets the Suspicion panel GameObject.</summary>
    public GameObject SuspicionPanel => suspicionPanel;

    /// <summary>Gets the Engagement fill bar Image.</summary>
    public Image EngagementFillImage => engagementFillImage;

    /// <summary>Gets the Sanity fill bar Image.</summary>
    public Image SanityFillImage => sanityFillImage;

    /// <summary>Gets the Suspicion fill bar Image.</summary>
    public Image SuspicionFillImage => suspicionFillImage;

    /// <summary>Gets the Engagement value text.</summary>
    public TextMeshProUGUI EngagementValueText => engagementValueText;

    /// <summary>Gets the Sanity value text.</summary>
    public TextMeshProUGUI SanityValueText => sanityValueText;

    /// <summary>Gets the Suspicion value text.</summary>
    public TextMeshProUGUI SuspicionValueText => suspicionValueText;

    /// <summary>Gets the Engagement panel background Image (for highlight effects).</summary>
    public Image EngagementBackground => engagementBackground;

    /// <summary>Gets the Sanity panel background Image (for highlight effects).</summary>
    public Image SanityBackground => sanityBackground;

    /// <summary>Gets the Suspicion panel background Image (for highlight effects).</summary>
    public Image SuspicionBackground => suspicionBackground;

    /// <summary>Gets the variable storage for reading Yarn variables.</summary>
    public VariableStorageBehaviour VariableStorage => variableStorage;

    #endregion

    private void OnEnable()
    {
        EnsureUIReady();

        runtimeWatcher = DialogueRuntimeWatcher.Instance;
        runtimeWatcher.Register(OnRuntimeReady, OnRuntimeLost);

        if (!runtimeWatcher.HasRuntime)
        {
            ApplyLoadingState();
        }
    }

    private void OnDisable()
    {
        if (runtimeWatcher != null)
        {
            runtimeWatcher.Unregister(OnRuntimeReady, OnRuntimeLost);
            runtimeWatcher = null;
        }
    }

    private void Start()
    {
        ApplyRuntimeDefaults();

        // Find DialogueRunner if not assigned
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        }

        if (dialogueRunner != null)
        {
            variableStorage = dialogueRunner.VariableStorage;
        }

        // Create UI elements if they don't already exist
        if (metricsRoot == null)
        {
            CreateUI();
        }
        else
        {
            EnsureContainerLayout();
        }

        if (variableStorage == null)
        {
            ApplyLoadingState();
        }
        else
        {
            UpdateMetrics();
        }
    }

    private void OnRuntimeReady(DialogueRunner runner, VariableStorageBehaviour storage)
    {
        if (runner != null)
        {
            dialogueRunner = runner;
        }

        variableStorage = storage;
        UpdateMetrics();
    }

    private void OnRuntimeLost()
    {
        variableStorage = null;
        ApplyLoadingState();
    }

    private void Update()
    {
        EnsureContainerLayout();

        // Update metrics at intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMetrics();
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// Create the UI elements programmatically
    /// </summary>
    private void CreateUI()
    {
        ApplyRuntimeDefaults();

        // Use deterministic overlay canvas to avoid nondeterministic Canvas selection in builds (WebGL/IL2CPP).
        canvas = OverlayCanvasProvider.GetCanvas();
        if (canvas == null)
        {
            Debug.LogError("MetricsPanelUI: OverlayCanvasProvider returned null canvas; cannot create UI.");
            return;
        }

        if (enableVerboseLogging)
        {
            Debug.Log($"MetricsPanelUI: Using canvas '{canvas.name}' (renderMode={canvas.renderMode}, enabled={canvas.enabled}, sortingOrder={canvas.sortingOrder}).", canvas);
        }

        metricsRoot = new GameObject("MetricsPanelRoot");
        metricsRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = metricsRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = Vector2.zero;

        rootLayoutGroup = metricsRoot.AddComponent<VerticalLayoutGroup>();
        rootLayoutGroup.childControlWidth = true;
        rootLayoutGroup.childControlHeight = true;
        rootLayoutGroup.childForceExpandWidth = true;
        rootLayoutGroup.childForceExpandHeight = false;
        rootLayoutGroup.childAlignment = TextAnchor.UpperRight;
        rootLayoutGroup.spacing = metricSpacing;

        rootFitter = metricsRoot.AddComponent<ContentSizeFitter>();
        rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // Create Engagement panel
        engagementPanel = CreateMetricPanel("EngagementPanel", "Engagement", engagementColor, out engagementLabelText, out engagementValueText, out engagementFillImage, out engagementBackground);

        // Create Sanity panel
        sanityPanel = CreateMetricPanel("SanityPanel", "Sanity", sanityColor, out sanityLabelText, out sanityValueText, out sanityFillImage, out sanityBackground);

        // Create Suspicion panel (hidden by default until Noam grants the HUD)
        suspicionPanel = CreateMetricPanel("SuspicionPanel", "Suspicion", suspicionColorLow, out suspicionLabelText, out suspicionValueText, out suspicionFillImage, out suspicionBackground);
        suspicionPanel.SetActive(false); // Hidden until $suspicion_hud_active is true

        EnsureContainerLayout();
    }

    private void EnsureUIReady()
    {
        if (metricsRoot == null)
        {
            CreateUI();
        }
    }

    /// <summary>
    /// Create a metric panel
    /// </summary>
    private GameObject CreateMetricPanel(
        string name,
        string label,
        Color fillColor,
        out TextMeshProUGUI labelText,
        out TextMeshProUGUI valueText,
        out Image fillImage,
        out Image backgroundImage)
    {
        labelText = null;
        valueText = null;
        fillImage = null;
        backgroundImage = null;

        GameObject panel = new GameObject(name);
        panel.transform.SetParent(metricsRoot.transform, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        float panelHeight = GetPanelHeight();
        rect.sizeDelta = new Vector2(0f, panelHeight);

        LayoutElement layoutElement = panel.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = panelHeight;
        layoutElement.minHeight = panelHeight;
        layoutElement.flexibleHeight = 0f;
        layoutElement.flexibleWidth = 1f;

        // Background
        Image bgImage = panel.AddComponent<Image>();
        bgImage.color = panelBackgroundColor;
        bgImage.raycastTarget = false;
        backgroundImage = bgImage;

        // Layout inside panel
        VerticalLayoutGroup panelLayout = panel.AddComponent<VerticalLayoutGroup>();
        int padX = Mathf.RoundToInt(panelPadding.x);
        int padY = Mathf.RoundToInt(panelPadding.y);
        panelLayout.padding.left = padX;
        panelLayout.padding.right = padX;
        panelLayout.padding.top = padY;
        panelLayout.padding.bottom = padY;
        panelLayout.spacing = barSpacing;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;
        panelLayout.childAlignment = TextAnchor.UpperCenter;

        // Header row: label (left) + value (right)
        GameObject headerObj = new GameObject("Header");
        headerObj.transform.SetParent(panel.transform, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.5f);
        headerRect.anchorMax = new Vector2(1f, 0.5f);
        headerRect.pivot = new Vector2(0.5f, 0.5f);
        headerRect.anchoredPosition = Vector2.zero;

        // Prevent layout edge cases from collapsing the header row height to ~0.
        LayoutElement headerElement = headerObj.AddComponent<LayoutElement>();
        headerElement.preferredHeight = Mathf.Max(24f, GetFontSize() + 6f);
        headerElement.minHeight = headerElement.preferredHeight;
        headerElement.flexibleHeight = 0f;

        HorizontalLayoutGroup headerLayout = headerObj.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding.left = 0;
        headerLayout.padding.right = 0;
        headerLayout.padding.top = 0;
        headerLayout.padding.bottom = 0;
        headerLayout.spacing = 0f;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = true;
        headerLayout.childAlignment = TextAnchor.MiddleCenter;

        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(headerObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        // For layout groups, avoid stretch anchors on children; let the layout system drive size/position.
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = Vector2.zero;
        var labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        labelLayout.minWidth = 0f;

        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(headerObj.transform, false);
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0.5f, 0.5f);
        valueRect.anchorMax = new Vector2(0.5f, 0.5f);
        valueRect.pivot = new Vector2(0.5f, 0.5f);
        valueRect.anchoredPosition = Vector2.zero;
        valueRect.sizeDelta = Vector2.zero;
        var valueLayout = valueObj.AddComponent<LayoutElement>();
        valueLayout.flexibleWidth = 0f;
        valueLayout.minWidth = 80f;

        TextMeshProUGUI labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        ApplyTmpFontFallback(labelTmp);
        labelTmp.text = label;
        labelTmp.fontSize = GetFontSize();
        labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
        labelTmp.color = Color.white;
        labelTmp.textWrappingMode = TextWrappingModes.NoWrap;
        labelTmp.raycastTarget = false;

        TextMeshProUGUI valueTmp = valueObj.AddComponent<TextMeshProUGUI>();
        ApplyTmpFontFallback(valueTmp);
        valueTmp.text = "0%";
        valueTmp.fontSize = GetFontSize();
        valueTmp.alignment = TextAlignmentOptions.MidlineRight;
        valueTmp.color = Color.white;
        valueTmp.textWrappingMode = TextWrappingModes.NoWrap;
        valueTmp.raycastTarget = false;

        labelText = labelTmp;
        valueText = valueTmp;

        // Bar container (track + fill + subtle highlight)
        GameObject barObj = new GameObject("Bar");
        barObj.transform.SetParent(panel.transform, false);
        RectTransform barRect = barObj.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0.5f);
        barRect.anchorMax = new Vector2(1f, 0.5f);
        barRect.pivot = new Vector2(0.5f, 0.5f);
        barRect.anchoredPosition = Vector2.zero;
        barRect.sizeDelta = new Vector2(0f, Mathf.Max(8f, barHeight));

        LayoutElement barLayout = barObj.AddComponent<LayoutElement>();
        barLayout.preferredHeight = Mathf.Max(8f, barHeight);
        barLayout.minHeight = Mathf.Max(8f, barHeight);
        barLayout.flexibleHeight = 0f;
        barLayout.flexibleWidth = 1f;

        Sprite uiSprite = GetRuntimeWhiteSprite();

        Image track = barObj.AddComponent<Image>();
        track.sprite = uiSprite;
        track.type = Image.Type.Simple;
        track.color = barTrackColor;
        track.raycastTarget = false;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fill = fillObj.AddComponent<Image>();
        fill.sprite = uiSprite;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 0f;
        fill.color = fillColor;
        fill.raycastTarget = false;
        fillImage = fill;

        GameObject highlightObj = new GameObject("Highlight");
        highlightObj.transform.SetParent(barObj.transform, false);
        RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;

        Image highlight = highlightObj.AddComponent<Image>();
        highlight.sprite = uiSprite;
        highlight.type = Image.Type.Simple;
        highlight.color = new Color(1f, 1f, 1f, 0.04f);
        highlight.raycastTarget = false;

        // If the UI system ever overlaps these elements during resize, ensure the header draws on top.
        headerObj.transform.SetAsLastSibling();

        return panel;
    }

    private static Sprite GetRuntimeWhiteSprite()
    {
        if (runtimeWhiteSprite != null)
        {
            return runtimeWhiteSprite;
        }

        // Create a simple 1x1 sprite from Unity's built-in white texture.
        Texture2D tex = Texture2D.whiteTexture;
        if (tex == null)
        {
            return null;
        }

        runtimeWhiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        runtimeWhiteSprite.name = "RuntimeWhiteSprite";
        return runtimeWhiteSprite;
    }

    private static void ApplyTmpFontFallback(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        // Prefer the global TMP default (can be set by GlobalFontOverride).
        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        // Final fallback: built-in TMP font (ships with TextMeshPro).
        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        }

        if (font != null)
        {
            text.font = font;
        }
    }

    private void ApplyRuntimeDefaults()
    {
        if (!enforceRuntimeDefaults)
        {
            return;
        }

        // Font sizes
        minFontSize = 48;
        fontSize = 48;

        metricSpacing = 20f;
        panelPadding = new Vector2(20f, 10f);
        barHeight = 20f;
        barSpacing = 10f;

        // Match the button background opacity/feel.
        panelBackgroundColor = new Color(0f, 0f, 0f, 0.95f);

        // Reduce track/highlight so the bars don't feel more opaque than the HUD buttons.
        barTrackColor = new Color(1f, 1f, 1f, 0.06f);
    }

    /// <summary>
    /// Update the metric displays with current values
    /// </summary>
    private void UpdateMetrics()
    {
        EnsureUIReady();

        if (variableStorage == null)
        {
            ApplyLoadingState();
            return;
        }

        // Get Engagement value
        float engagement = 0f;
        if (variableStorage.TryGetValue<float>("$engagement", out var engagementValue))
        {
            engagement = engagementValue;
        }

        // Get Sanity value
        float sanity = 0f;
        if (variableStorage.TryGetValue<float>("$sanity", out var sanityValue))
        {
            sanity = sanityValue;
        }

        // Update text + fills (skip if MetricsAnimator is handling animation)
        if (!useAnimatedUpdates)
        {
            float engagement01 = Mathf.Clamp01(engagement / 100f);
            float sanity01 = Mathf.Clamp01(sanity / 100f);

            if (engagementValueText != null)
            {
                engagementValueText.text = $"{engagement:F0}%";
            }
            if (engagementFillImage != null)
            {
                engagementFillImage.fillAmount = engagement01;
                engagementFillImage.color = engagementColor;
            }

            if (sanityValueText != null)
            {
                sanityValueText.text = $"{sanity:F0}%";
            }
            if (sanityFillImage != null)
            {
                sanityFillImage.fillAmount = sanity01;
                sanityFillImage.color = sanityColor;
            }
        }

        // Update Suspicion panel visibility and value
        UpdateSuspicionPanel();
    }

    /// <summary>
    /// Update the suspicion panel based on $suspicion_hud_active and $alert_level
    /// </summary>
    private void UpdateSuspicionPanel()
    {
        if (suspicionPanel == null || variableStorage == null)
        {
            return;
        }

        // Check if the suspicion HUD should be visible
        bool showSuspicion = false;
        if (variableStorage.TryGetValue<bool>("$suspicion_hud_active", out var hudActive))
        {
            showSuspicion = hudActive;
        }

        suspicionPanel.SetActive(showSuspicion);

        if (showSuspicion)
        {
            // Get the alert level (suspicion score)
            float alertLevel = 0f;
            if (variableStorage.TryGetValue<float>("$alert_level", out var alertValue))
            {
                alertLevel = alertValue;
            }

            // Update color based on threat level (always update color, animator doesn't handle this)
            // 0-25: Clear (green), 26-50: Flagged (yellow-green), 51-75: Watched (orange), 76+: Critical (red)
            Color suspicionColor;
            if (alertLevel >= 76)
            {
                suspicionColor = suspicionColorHigh; // Red - Critical
            }
            else if (alertLevel >= 51)
            {
                suspicionColor = Color.Lerp(suspicionColorMid, suspicionColorHigh, (alertLevel - 51f) / 25f); // Orange to Red
            }
            else if (alertLevel >= 26)
            {
                suspicionColor = Color.Lerp(suspicionColorLow, suspicionColorMid, (alertLevel - 26f) / 25f); // Green to Orange
            }
            else
            {
                suspicionColor = suspicionColorLow; // Green - Clear
            }

            // Keep header text white
            if (suspicionLabelText != null)
            {
                suspicionLabelText.color = Color.white;
            }

            if (suspicionValueText != null)
            {
                suspicionValueText.color = Color.white;
            }

            // Update fill amount and text (skip if MetricsAnimator is handling animation)
            if (!useAnimatedUpdates)
            {
                if (suspicionValueText != null)
                {
                    suspicionValueText.text = $"{alertLevel:F0}%";
                }

                if (suspicionFillImage != null)
                {
                    suspicionFillImage.fillAmount = Mathf.Clamp01(alertLevel / 100f);
                }
            }

            // Always apply color (animator doesn't handle dynamic suspicion color)
            if (suspicionFillImage != null)
            {
                suspicionFillImage.color = suspicionColor;
            }
        }
    }

    private void ApplyLoadingState()
    {
        EnsureUIReady();

        if (engagementPanel != null)
        {
            engagementPanel.SetActive(true);
        }

        if (sanityPanel != null)
        {
            sanityPanel.SetActive(true);
        }

        // Suspicion panel stays hidden in loading state
        if (suspicionPanel != null)
        {
            suspicionPanel.SetActive(false);
        }

        if (engagementValueText != null)
        {
            engagementValueText.text = "--%";
        }

        if (sanityValueText != null)
        {
            sanityValueText.text = "--%";
        }

        if (engagementFillImage != null)
        {
            engagementFillImage.fillAmount = 0f;
            engagementFillImage.color = engagementColor;
        }

        if (sanityFillImage != null)
        {
            sanityFillImage.fillAmount = 0f;
            sanityFillImage.color = sanityColor;
        }

        if (suspicionFillImage != null)
        {
            suspicionFillImage.fillAmount = 0f;
            suspicionFillImage.color = suspicionColorLow;
        }
    }

    private void ConfigureCanvasScaler(CanvasScaler scaler)
    {
        if (scaler == null) return;
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void EnsureContainerLayout()
    {
        if (metricsRoot == null)
        {
            return;
        }

        RectTransform rootRect = metricsRoot.GetComponent<RectTransform>();
        if (rootRect == null)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;
        if (safeArea.width <= 0f || safeArea.height <= 0f)
        {
            safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
        }
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (screenSize != lastScreenSize || safeArea != lastSafeArea)
        {
            float safeWidth = Mathf.Max(safeArea.width, 200f);
            float availableWidth = Mathf.Max(safeWidth - (rightMargin * 2f), 200f);
            availableWidth = Mathf.Min(availableWidth, safeWidth);

            float clampedFraction = Mathf.Clamp01(widthFraction);
            float targetWidth = safeWidth * clampedFraction;

            float minWidth = Mathf.Min(panelMinWidth, availableWidth);
            float maxWidth = Mathf.Min(panelMaxWidth, availableWidth);
            if (maxWidth < minWidth)
            {
                maxWidth = minWidth;
            }

            targetWidth = Mathf.Clamp(targetWidth, minWidth, maxWidth);
            targetWidth = Mathf.Min(targetWidth + Mathf.Max(0f, widthBonus), availableWidth);

            rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

            if (rootLayoutGroup != null)
            {
                // Do not add additional padding at the root level; each panel already has internal padding.
                rootLayoutGroup.padding.left = 0;
                rootLayoutGroup.padding.right = 0;
                rootLayoutGroup.padding.top = 0;
                rootLayoutGroup.padding.bottom = 0;
                rootLayoutGroup.spacing = metricSpacing;
            }

            float safeTopPadding = Screen.height - safeArea.yMax;
            // Since anchor is (1f, 1f) and pivot is (1f, 1f), use simple negative offsets from top-right corner
            rootRect.anchoredPosition = new Vector2(-rightMargin, -(safeTopPadding + topMargin));

            // Editor Game View resizing can leave layout groups in a transient state; force a rebuild when size changes.
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
            if (engagementPanel != null) LayoutRebuilder.ForceRebuildLayoutImmediate(engagementPanel.GetComponent<RectTransform>());
            if (sanityPanel != null) LayoutRebuilder.ForceRebuildLayoutImmediate(sanityPanel.GetComponent<RectTransform>());
            if (suspicionPanel != null) LayoutRebuilder.ForceRebuildLayoutImmediate(suspicionPanel.GetComponent<RectTransform>());

            if (enableVerboseLogging)
            {
                Debug.Log($"MetricsPanelUI: Layout updated (safeArea={safeArea}, screen={screenSize}, anchoredPosition={rootRect.anchoredPosition}, width={rootRect.rect.width}).", metricsRoot);
            }

            lastScreenSize = screenSize;
            lastSafeArea = safeArea;
        }
    }

    private float GetPanelHeight()
    {
        int effectiveFontSize = Mathf.Max(fontSize, minFontSize);
        float headerHeight = effectiveFontSize + 6f;
        float barH = Mathf.Max(8f, barHeight);
        float internalPad = panelPadding.y * 2f;
        float total = internalPad + headerHeight + barSpacing + barH;
        return Mathf.Max(panelPreferredHeight, total);
    }

    private int GetFontSize()
    {
        return Mathf.Max(fontSize, minFontSize);
    }
}
