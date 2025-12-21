using UnityEngine;
using UnityEngine.Serialization;

public class CharacterTalkAnimation : MonoBehaviour
{
    [Header("Breathing Motion")]
    [Tooltip("Vertical movement amplitude in UI pixels.")]
    [FormerlySerializedAs("distance")]
    [SerializeField] private float amplitude = 6f;

    [Tooltip("Seconds per full cycle (up + down).")]
    [FormerlySerializedAs("duration")]
    [SerializeField] private float period = 5f;

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Targets (set at runtime by CharacterSpriteManager)")]
    [SerializeField] private RectTransform leftTarget;
    [SerializeField] private RectTransform rightTarget;

    private Vector2 leftStartAnchoredPos;
    private Vector2 rightStartAnchoredPos;
    private float t;

    void Awake()
    {
        CaptureStartPositions();
    }

    public void SetTargets(RectTransform left, RectTransform right, bool resetPhase = true)
    {
        leftTarget = left;
        rightTarget = right;
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
        if (leftTarget != null)
        {
            leftStartAnchoredPos = leftTarget.anchoredPosition;
        }
        if (rightTarget != null)
        {
            rightStartAnchoredPos = rightTarget.anchoredPosition;
        }
    }

    void Update()
    {
        // If not wired yet, do nothing (CharacterSpriteManager will call SetTargets).
        if (leftTarget == null || rightTarget == null)
        {
            return;
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        t += dt;

        // Smooth breathing motion (sine wave).
        float omega = (2f * Mathf.PI) / Mathf.Max(0.01f, period);
        float offset = Mathf.Sin(t * omega) * amplitude;

        leftTarget.anchoredPosition = new Vector2(leftStartAnchoredPos.x, leftStartAnchoredPos.y + offset);
        rightTarget.anchoredPosition = new Vector2(rightStartAnchoredPos.x, rightStartAnchoredPos.y + offset);
    }
}