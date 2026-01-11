using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using MentalBreak.UI.Win95;

/// <summary>
/// Windows 95 styled metrics panel displaying Engagement, Sanity, and Suspicion.
/// This is a Win95-themed version of MetricsPanelUI.
/// </summary>
public class Win95MetricsPanel : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The DialogueRunner to read variables from")]
    public DialogueRunner dialogueRunner;

    [Header("Settings")]
    [Tooltip("Update interval in seconds")]
    public float updateInterval = 0.1f;

    [Tooltip("Font size for metric labels")]
    public int fontSize = 36;

    [Header("Layout")]
    public float panelWidth = 560f;
    public float panelHeight = 72f;
    public float metricSpacing = 8f;
    public float rightMargin = 16f;
    public float topMargin = 16f;
    public float barHeight = 28f;

    // Colors
    private readonly Color engagementColor = new Color(0f, 0f, 0.5f, 1f); // Navy blue (Win95 style)
    private readonly Color sanityColor = new Color(0f, 0.5f, 0.5f, 1f);   // Teal
    private readonly Color suspicionColorLow = new Color(0f, 0.5f, 0f, 1f);   // Green
    private readonly Color suspicionColorMid = new Color(0.5f, 0.5f, 0f, 1f); // Yellow-ish
    private readonly Color suspicionColorHigh = new Color(0.5f, 0f, 0f, 1f);  // Dark red

    private VariableStorageBehaviour variableStorage;
    private DialogueRuntimeWatcher runtimeWatcher;
    private float lastUpdateTime = 0f;

    // UI References
    private GameObject metricsRoot;
    private GameObject engagementPanel;
    private GameObject sanityPanel;
    private GameObject suspicionPanel;
    private Canvas canvas;

    private TMP_Text engagementLabelText;
    private TMP_Text engagementValueText;
    private Image engagementFillImage;
    private Image engagementTrackImage;

    private TMP_Text sanityLabelText;
    private TMP_Text sanityValueText;
    private Image sanityFillImage;

    private TMP_Text suspicionLabelText;
    private TMP_Text suspicionValueText;
    private Image suspicionFillImage;

    private bool isInitialized = false;

    #region Public Accessors

    public GameObject EngagementPanel => engagementPanel;
    public GameObject SanityPanel => sanityPanel;
    public GameObject SuspicionPanel => suspicionPanel;
    public Image EngagementFillImage => engagementFillImage;
    public Image SanityFillImage => sanityFillImage;
    public Image SuspicionFillImage => suspicionFillImage;
    public TMP_Text EngagementValueText => engagementValueText;
    public TMP_Text SanityValueText => sanityValueText;
    public TMP_Text SuspicionValueText => suspicionValueText;
    public VariableStorageBehaviour VariableStorage => variableStorage;

    #endregion

    private void OnEnable()
    {
        if (!isInitialized)
        {
            CreateUI();
            isInitialized = true;
        }

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
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        }

        if (dialogueRunner != null)
        {
            variableStorage = dialogueRunner.VariableStorage;
        }

        if (!isInitialized)
        {
            CreateUI();
            isInitialized = true;
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

    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateMetrics();
            lastUpdateTime = Time.time;
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

    private void CreateUI()
    {
        // Use deterministic overlay canvas
        canvas = OverlayCanvasProvider.GetCanvas();
        if (canvas == null)
        {
            Debug.LogError("Win95MetricsPanel: OverlayCanvasProvider returned null canvas.");
            return;
        }

        // Create root container
        metricsRoot = new GameObject("Win95MetricsPanelRoot");
        metricsRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = metricsRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = new Vector2(-rightMargin, -topMargin);

        // Vertical layout
        VerticalLayoutGroup rootLayout = metricsRoot.AddComponent<VerticalLayoutGroup>();
        rootLayout.childControlWidth = true;
        rootLayout.childControlHeight = false;
        rootLayout.childForceExpandWidth = true;
        rootLayout.childForceExpandHeight = false;
        rootLayout.childAlignment = TextAnchor.UpperRight;
        rootLayout.spacing = metricSpacing;

        ContentSizeFitter rootFitter = metricsRoot.AddComponent<ContentSizeFitter>();
        rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rootFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Create metric panels
        engagementPanel = CreateWin95MetricPanel("Engagement", "Engagement", engagementColor,
            out engagementLabelText, out engagementValueText, out engagementFillImage);

        sanityPanel = CreateWin95MetricPanel("Sanity", "Sanity", sanityColor,
            out sanityLabelText, out sanityValueText, out sanityFillImage);

        suspicionPanel = CreateWin95MetricPanel("Suspicion", "Suspicion", suspicionColorLow,
            out suspicionLabelText, out suspicionValueText, out suspicionFillImage);
        suspicionPanel.SetActive(false); // Hidden until $suspicion_hud_active

        // Add overall Win95 raised panel border to root
        Win95Panel.Create(metricsRoot, Win95Panel.PanelStyle.Raised);
    }

    private GameObject CreateWin95MetricPanel(string name, string label, Color fillColor,
        out TMP_Text labelText, out TMP_Text valueText, out Image fillImage)
    {
        GameObject panel = new GameObject(name + "Panel");
        panel.transform.SetParent(metricsRoot.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        LayoutElement layoutElement = panel.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = panelWidth;
        layoutElement.preferredHeight = panelHeight;

        // Win95 gray background
        Image bgImage = panel.AddComponent<Image>();
        bgImage.color = Win95Theme.WindowBackground;

        // Horizontal layout: Label | Progress Bar | Value
        HorizontalLayoutGroup hlg = panel.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 8, 8);
        hlg.spacing = 8;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleLeft;

        // Label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(panel.transform, false);

        labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = fontSize;
        labelText.color = Win95Theme.WindowText;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 160;
        labelLayout.flexibleWidth = 0;

        // Progress bar container (sunken)
        GameObject barContainer = new GameObject("BarContainer");
        barContainer.transform.SetParent(panel.transform, false);

        RectTransform barContainerRect = barContainer.AddComponent<RectTransform>();

        LayoutElement barContainerLayout = barContainer.AddComponent<LayoutElement>();
        barContainerLayout.flexibleWidth = 1;
        barContainerLayout.preferredHeight = barHeight;

        // Sunken border effect for progress bar
        Image barBg = barContainer.AddComponent<Image>();
        barBg.color = Color.white; // White background for Win95 progress bars

        // Add sunken borders
        CreateSunkenBorder(barContainer);

        // Fill bar (inside the container)
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(barContainer.transform, false);

        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = new Vector2(4, 4);
        fillRect.offsetMax = new Vector2(4, -4);

        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.raycastTarget = false;

        // Value text
        GameObject valueObj = new GameObject("Value");
        valueObj.transform.SetParent(panel.transform, false);

        valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = "0%";
        valueText.fontSize = fontSize;
        valueText.color = Win95Theme.WindowText;
        valueText.alignment = TextAlignmentOptions.MidlineRight;

        LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 96;
        valueLayout.flexibleWidth = 0;

        return panel;
    }

    private void CreateSunkenBorder(GameObject parent)
    {
        // Top shadow - 2px
        CreateBorderLine(parent, "TopShadow", new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, 0), new Vector2(0, -2), Win95Theme.ButtonShadow);

        // Left shadow - 2px
        CreateBorderLine(parent, "LeftShadow", new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(2, 0), Win95Theme.ButtonShadow);

        // Bottom highlight - 2px
        CreateBorderLine(parent, "BottomHighlight", new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 0), new Vector2(0, 2), Win95Theme.ButtonHighlight);

        // Right highlight - 2px
        CreateBorderLine(parent, "RightHighlight", new Vector2(1, 0), new Vector2(1, 1),
            new Vector2(0, 0), new Vector2(-2, 0), Win95Theme.ButtonHighlight);
    }

    private void CreateBorderLine(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(parent.transform, false);

        RectTransform rect = lineObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image img = lineObj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    private void UpdateMetrics()
    {
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
        UpdateMetricDisplay(engagementValueText, engagementFillImage, engagement, engagementColor);

        // Get Sanity value
        float sanity = 0f;
        if (variableStorage.TryGetValue<float>("$sanity", out var sanityValue))
        {
            sanity = sanityValue;
        }
        UpdateMetricDisplay(sanityValueText, sanityFillImage, sanity, sanityColor);

        // Update Suspicion panel
        UpdateSuspicionPanel();
    }

    private void UpdateMetricDisplay(TMP_Text valueText, Image fillImage, float value, Color color)
    {
        if (valueText != null)
        {
            valueText.text = $"{value:F0}%";
        }

        if (fillImage != null)
        {
            // Get parent width for calculating fill width
            RectTransform parent = fillImage.transform.parent as RectTransform;
            if (parent != null)
            {
                float parentWidth = parent.rect.width - 8; // Account for border (2x scale)
                float fillWidth = parentWidth * Mathf.Clamp01(value / 100f);

                RectTransform fillRect = fillImage.rectTransform;
                fillRect.sizeDelta = new Vector2(fillWidth, fillRect.sizeDelta.y);
            }

            fillImage.color = color;
        }
    }

    private void UpdateSuspicionPanel()
    {
        if (suspicionPanel == null || variableStorage == null)
        {
            return;
        }

        bool showSuspicion = false;
        if (variableStorage.TryGetValue<bool>("$suspicion_hud_active", out var hudActive))
        {
            showSuspicion = hudActive;
        }

        suspicionPanel.SetActive(showSuspicion);

        if (showSuspicion)
        {
            float alertLevel = 0f;
            if (variableStorage.TryGetValue<float>("$alert_level", out var alertValue))
            {
                alertLevel = alertValue;
            }

            // Determine color based on level
            Color suspicionColor;
            if (alertLevel >= 76)
            {
                suspicionColor = suspicionColorHigh;
            }
            else if (alertLevel >= 51)
            {
                suspicionColor = Color.Lerp(suspicionColorMid, suspicionColorHigh, (alertLevel - 51f) / 25f);
            }
            else if (alertLevel >= 26)
            {
                suspicionColor = Color.Lerp(suspicionColorLow, suspicionColorMid, (alertLevel - 26f) / 25f);
            }
            else
            {
                suspicionColor = suspicionColorLow;
            }

            UpdateMetricDisplay(suspicionValueText, suspicionFillImage, alertLevel, suspicionColor);
        }
    }

    private void ApplyLoadingState()
    {
        if (engagementPanel != null) engagementPanel.SetActive(true);
        if (sanityPanel != null) sanityPanel.SetActive(true);
        if (suspicionPanel != null) suspicionPanel.SetActive(false);

        if (engagementValueText != null) engagementValueText.text = "--%";
        if (sanityValueText != null) sanityValueText.text = "--%";

        if (engagementFillImage != null)
        {
            engagementFillImage.rectTransform.sizeDelta = new Vector2(0, engagementFillImage.rectTransform.sizeDelta.y);
        }
        if (sanityFillImage != null)
        {
            sanityFillImage.rectTransform.sizeDelta = new Vector2(0, sanityFillImage.rectTransform.sizeDelta.y);
        }
    }

    /// <summary>
    /// Force an immediate update.
    /// </summary>
    public void ForceUpdate()
    {
        UpdateMetrics();
    }
}
