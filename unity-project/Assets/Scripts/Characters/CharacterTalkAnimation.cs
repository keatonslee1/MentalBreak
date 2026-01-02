using UnityEngine;
using UnityEngine.Serialization;

public class CharacterTalkAnimation : MonoBehaviour
{
    [Header("Breathing Motion")]
    [Tooltip("Vertical movement amplitude in UI pixels.")]
    [FormerlySerializedAs("distance")]
    [SerializeField] private float amplitude = 3f;

    [Tooltip("Seconds per full cycle (up + down).")]
    [FormerlySerializedAs("duration")]
    [SerializeField] private float period = 10f;

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Targets (set at runtime by CharacterSpriteManager)")]
    [SerializeField] private RectTransform[] targets = new RectTransform[0];

    private Vector2[] startAnchoredPositions = new Vector2[0];
    private float t;

    void Awake()
    {
        CaptureStartPositions();
    }

    public void SetTargets(RectTransform[] newTargets, bool resetPhase = true)
    {
        targets = newTargets;
        CaptureStartPositions();
        if (resetPhase)
        {
            t = 0f;
        }
    }

    // Backwards compatibility: accept two individual targets
    public void SetTargets(RectTransform left, RectTransform right, bool resetPhase = true)
    {
        targets = new RectTransform[] { left, right };
        CaptureStartPositions();
        if (resetPhase)
        {
            t = 0f;
        }
    }

    public void Configure(float newAmplitude, float newPeriod)
    {
        amplitude = Mathf.Max(0f, newAmplitude);
        period = Mathf.Max(0.01f, newPeriod);
    }

    void CaptureStartPositions()
    {
        if (targets == null)
        {
            startAnchoredPositions = new Vector2[0];
            return;
        }

        startAnchoredPositions = new Vector2[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                startAnchoredPositions[i] = targets[i].anchoredPosition;
            }
        }
    }

    void Update()
    {
        // If not wired yet, do nothing (CharacterSpriteManager will call SetTargets).
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        t += dt;

        // Smooth breathing motion (sine wave).
        float omega = (2f * Mathf.PI) / Mathf.Max(0.01f, period);
        float offset = Mathf.Sin(t * omega) * amplitude;

        // Apply breathing offset to all targets
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null && i < startAnchoredPositions.Length)
            {
                Vector2 startPos = startAnchoredPositions[i];
                targets[i].anchoredPosition = new Vector2(startPos.x, startPos.y + offset);
            }
        }
    }
}