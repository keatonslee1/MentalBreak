using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Handles animated metric changes with SFX and visual highlights.
/// Works alongside MetricsPanelUI - detects Yarn variable changes and animates the UI.
/// </summary>
public class MetricsAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the bar fill animation in seconds")]
    public float animationDuration = 1.0f;

    [Tooltip("Delay between starting each metric animation when multiple change")]
    public float sequenceDelay = 0.75f;

    [Tooltip("How much brighter the panel background becomes during highlight (0-1)")]
    [Range(0f, 1f)]
    public float highlightIntensity = 0.3f;

    [Header("SFX - Engagement")]
    [Tooltip("Sound when engagement increases")]
    [SerializeField] private AudioClip engagementUp;

    [Tooltip("Sound when engagement decreases")]
    [SerializeField] private AudioClip engagementDown;

    [Header("SFX - Sanity")]
    [Tooltip("Sound when sanity increases")]
    [SerializeField] private AudioClip sanityUp;

    [Tooltip("Sound when sanity decreases")]
    [SerializeField] private AudioClip sanityDown;

    [Header("SFX - Suspicion")]
    [Tooltip("Sound when suspicion increases")]
    [SerializeField] private AudioClip suspicionUp;

    [Tooltip("Sound when suspicion decreases")]
    [SerializeField] private AudioClip suspicionDown;

    [Header("References")]
    [Tooltip("Reference to MetricsPanelUI (auto-found if not set)")]
    public MetricsPanelUI metricsPanel;

    [Tooltip("Reference to AudioCommandHandler for SFX playback (auto-found if not set)")]
    public AudioCommandHandler audioHandler;

    [Header("Polling")]
    [Tooltip("How often to check for metric changes (seconds)")]
    public float pollInterval = 0.1f;

    // Previous values for change detection (-1 means uninitialized)
    private float prevEngagement = -1f;
    private float prevSanity = -1f;
    private float prevAlertLevel = -1f;

    // Animation state
    private Queue<MetricChange> pendingAnimations = new Queue<MetricChange>();
    private bool isAnimating = false;
    private Coroutine processQueueCoroutine = null;

    // Runtime references
    private VariableStorageBehaviour variableStorage;
    private DialogueRuntimeWatcher runtimeWatcher;
    private float lastPollTime = 0f;
    private bool isReady = false;

    private void OnEnable()
    {
        runtimeWatcher = DialogueRuntimeWatcher.Instance;
        runtimeWatcher.Register(OnRuntimeReady, OnRuntimeLost);

        if (runtimeWatcher.HasRuntime)
        {
            // Runtime already available
            var runner = Object.FindAnyObjectByType<DialogueRunner>();
            if (runner != null)
            {
                variableStorage = runner.VariableStorage;
                isReady = true;
            }
        }
    }

    private void OnDisable()
    {
        if (runtimeWatcher != null)
        {
            runtimeWatcher.Unregister(OnRuntimeReady, OnRuntimeLost);
            runtimeWatcher = null;
        }

        // Stop all animations
        StopAllCoroutines();
        pendingAnimations.Clear();
        isAnimating = false;
        processQueueCoroutine = null;
    }

    private void Start()
    {
        // Find references if not assigned
        if (metricsPanel == null)
        {
            metricsPanel = Object.FindAnyObjectByType<MetricsPanelUI>();
        }

        if (audioHandler == null)
        {
            audioHandler = Object.FindAnyObjectByType<AudioCommandHandler>();
        }

        // Enable animated updates on MetricsPanelUI
        if (metricsPanel != null)
        {
            metricsPanel.UseAnimatedUpdates = true;
        }
    }

    private void OnRuntimeReady(DialogueRunner runner, VariableStorageBehaviour storage)
    {
        variableStorage = storage;
        isReady = true;

        // Initialize previous values to current values (no animation on first load)
        InitializePreviousValues();
    }

    private void OnRuntimeLost()
    {
        variableStorage = null;
        isReady = false;

        // Stop any running animations
        StopAllCoroutines();
        pendingAnimations.Clear();
        isAnimating = false;
        processQueueCoroutine = null;

        // Reset previous values
        prevEngagement = -1f;
        prevSanity = -1f;
        prevAlertLevel = -1f;
    }

    private void Update()
    {
        if (!isReady || variableStorage == null)
        {
            return;
        }

        // Poll for changes at interval
        if (Time.time - lastPollTime >= pollInterval)
        {
            CheckForChanges();
            lastPollTime = Time.time;
        }
    }

    /// <summary>
    /// Initialize previous values to current values (prevents animation on first load).
    /// </summary>
    private void InitializePreviousValues()
    {
        if (variableStorage == null) return;

        if (variableStorage.TryGetValue<float>("$engagement", out float engagement))
        {
            prevEngagement = engagement;

            // Also set the UI to current value immediately
            if (metricsPanel != null && metricsPanel.EngagementFillImage != null)
            {
                metricsPanel.EngagementFillImage.fillAmount = Mathf.Clamp01(engagement / 100f);
            }
            if (metricsPanel != null && metricsPanel.EngagementValueText != null)
            {
                metricsPanel.EngagementValueText.text = $"{engagement:F0}%";
            }
        }

        if (variableStorage.TryGetValue<float>("$sanity", out float sanity))
        {
            prevSanity = sanity;

            if (metricsPanel != null && metricsPanel.SanityFillImage != null)
            {
                metricsPanel.SanityFillImage.fillAmount = Mathf.Clamp01(sanity / 100f);
            }
            if (metricsPanel != null && metricsPanel.SanityValueText != null)
            {
                metricsPanel.SanityValueText.text = $"{sanity:F0}%";
            }
        }

        if (variableStorage.TryGetValue<float>("$alert_level", out float alertLevel))
        {
            prevAlertLevel = alertLevel;

            if (metricsPanel != null && metricsPanel.SuspicionFillImage != null)
            {
                metricsPanel.SuspicionFillImage.fillAmount = Mathf.Clamp01(alertLevel / 100f);
            }
            if (metricsPanel != null && metricsPanel.SuspicionValueText != null)
            {
                metricsPanel.SuspicionValueText.text = $"{alertLevel:F0}%";
            }
        }
    }

    /// <summary>
    /// Check for metric changes and queue animations.
    /// </summary>
    private void CheckForChanges()
    {
        if (variableStorage == null) return;

        List<MetricChange> changes = new List<MetricChange>();

        // Check Engagement
        if (variableStorage.TryGetValue<float>("$engagement", out float engagement))
        {
            if (prevEngagement >= 0 && !Mathf.Approximately(engagement, prevEngagement))
            {
                changes.Add(new MetricChange(MetricType.Engagement, prevEngagement, engagement));
            }
            prevEngagement = engagement;
        }

        // Check Sanity
        if (variableStorage.TryGetValue<float>("$sanity", out float sanity))
        {
            if (prevSanity >= 0 && !Mathf.Approximately(sanity, prevSanity))
            {
                changes.Add(new MetricChange(MetricType.Sanity, prevSanity, sanity));
            }
            prevSanity = sanity;
        }

        // Check Suspicion (only if visible)
        bool suspicionActive = false;
        variableStorage.TryGetValue<bool>("$suspicion_hud_active", out suspicionActive);

        if (suspicionActive && variableStorage.TryGetValue<float>("$alert_level", out float alertLevel))
        {
            if (prevAlertLevel >= 0 && !Mathf.Approximately(alertLevel, prevAlertLevel))
            {
                changes.Add(new MetricChange(MetricType.Suspicion, prevAlertLevel, alertLevel));
            }
            prevAlertLevel = alertLevel;
        }

        // Queue changes in priority order (Engagement=0, Sanity=1, Suspicion=2)
        if (changes.Count > 0)
        {
            foreach (var change in changes.OrderBy(c => (int)c.Type))
            {
                pendingAnimations.Enqueue(change);
            }

            // Start processing if not already animating
            if (!isAnimating && processQueueCoroutine == null)
            {
                processQueueCoroutine = StartCoroutine(ProcessAnimationQueue());
            }
        }
    }

    /// <summary>
    /// Process the animation queue with staggered starts.
    /// </summary>
    private IEnumerator ProcessAnimationQueue()
    {
        isAnimating = true;

        while (pendingAnimations.Count > 0)
        {
            MetricChange change = pendingAnimations.Dequeue();

            // Start this animation (non-blocking - runs in parallel)
            StartCoroutine(AnimateMetric(change));

            // Wait for stagger delay before starting next animation
            if (pendingAnimations.Count > 0)
            {
                yield return new WaitForSeconds(sequenceDelay);
            }
        }

        // Wait for the last animation to complete
        yield return new WaitForSeconds(animationDuration);

        isAnimating = false;
        processQueueCoroutine = null;
    }

    /// <summary>
    /// Animate a single metric change.
    /// </summary>
    private IEnumerator AnimateMetric(MetricChange change)
    {
        // Get UI references
        Image fillImage = GetFillImage(change.Type);
        TextMeshProUGUI valueText = GetValueText(change.Type);
        Image background = GetBackground(change.Type);

        if (fillImage == null)
        {
            yield break;
        }

        // Play SFX at start
        PlayMetricSFX(change.Type, change.IsIncrease);

        // Start highlight effect (runs in parallel)
        if (background != null)
        {
            StartCoroutine(PulseHighlight(background));
        }

        // Animate fill and text over duration
        float startFill = Mathf.Clamp01(change.OldValue / 100f);
        float endFill = Mathf.Clamp01(change.NewValue / 100f);
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animationDuration);

            float currentFill = Mathf.Lerp(startFill, endFill, t);
            fillImage.fillAmount = currentFill;

            if (valueText != null)
            {
                float currentValue = Mathf.Lerp(change.OldValue, change.NewValue, t);
                valueText.text = $"{currentValue:F0}%";
            }

            yield return null;
        }

        // Ensure final values are exact
        fillImage.fillAmount = endFill;
        if (valueText != null)
        {
            valueText.text = $"{change.NewValue:F0}%";
        }
    }

    /// <summary>
    /// Pulse the background color for highlight effect.
    /// </summary>
    private IEnumerator PulseHighlight(Image background)
    {
        if (background == null) yield break;

        Color originalColor = background.color;
        Color highlightColor = new Color(
            Mathf.Min(1f, originalColor.r + highlightIntensity),
            Mathf.Min(1f, originalColor.g + highlightIntensity),
            Mathf.Min(1f, originalColor.b + highlightIntensity),
            originalColor.a
        );

        float fadeInDuration = animationDuration * 0.3f;
        float holdDuration = animationDuration * 0.4f;
        float fadeOutDuration = animationDuration * 0.3f;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            background.color = Color.Lerp(originalColor, highlightColor, t);
            yield return null;
        }

        // Hold
        background.color = highlightColor;
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            background.color = Color.Lerp(highlightColor, originalColor, t);
            yield return null;
        }

        // Ensure original color is restored
        background.color = originalColor;
    }

    /// <summary>
    /// Play the appropriate SFX for a metric change.
    /// </summary>
    private void PlayMetricSFX(MetricType type, bool isIncrease)
    {
        if (audioHandler == null) return;

        AudioClip clip = null;

        switch (type)
        {
            case MetricType.Engagement:
                clip = isIncrease ? engagementUp : engagementDown;
                break;
            case MetricType.Sanity:
                clip = isIncrease ? sanityUp : sanityDown;
                break;
            case MetricType.Suspicion:
                clip = isIncrease ? suspicionUp : suspicionDown;
                break;
        }

        if (clip != null)
        {
            audioHandler.PlaySFXClip(clip);
        }
    }

    #region UI Element Accessors

    private Image GetFillImage(MetricType type)
    {
        if (metricsPanel == null) return null;

        return type switch
        {
            MetricType.Engagement => metricsPanel.EngagementFillImage,
            MetricType.Sanity => metricsPanel.SanityFillImage,
            MetricType.Suspicion => metricsPanel.SuspicionFillImage,
            _ => null
        };
    }

    private TextMeshProUGUI GetValueText(MetricType type)
    {
        if (metricsPanel == null) return null;

        return type switch
        {
            MetricType.Engagement => metricsPanel.EngagementValueText,
            MetricType.Sanity => metricsPanel.SanityValueText,
            MetricType.Suspicion => metricsPanel.SuspicionValueText,
            _ => null
        };
    }

    private Image GetBackground(MetricType type)
    {
        if (metricsPanel == null) return null;

        return type switch
        {
            MetricType.Engagement => metricsPanel.EngagementBackground,
            MetricType.Sanity => metricsPanel.SanityBackground,
            MetricType.Suspicion => metricsPanel.SuspicionBackground,
            _ => null
        };
    }

    #endregion
}
