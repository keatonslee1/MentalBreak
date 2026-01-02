using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates a single character portrait with talking and eye blink animations.
/// Manages two layers: base (static/talking) and eyes (blink overlay).
/// </summary>
public class CharacterPortraitAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Talking animation speed: frames per second")]
    [SerializeField] private float talkingFPS = 10f;

    [Tooltip("Minimum seconds between eye blinks")]
    [SerializeField] private float blinkIntervalMin = 2f;

    [Tooltip("Maximum seconds between eye blinks")]
    [SerializeField] private float blinkIntervalMax = 6f;

    [Tooltip("Seconds per eye blink frame")]
    [SerializeField] private float blinkFrameDuration = 0.1f;

    // Portrait data
    private CharacterPortraitData portraitData;
    private Image baseLayer;
    private Image talkingLayer;
    private Image eyesLayer;

    // Animation state
    private bool isTalking = false;
    private Coroutine talkingCoroutine;
    private Coroutine blinkCoroutine;

    private bool isInitialized = false;

    /// <summary>
    /// Initializes the animator with portrait data and layer references.
    /// </summary>
    public void Initialize(CharacterPortraitData data, Image baseLayer, Image talkingLayer, Image eyesLayer)
    {
        this.portraitData = data;
        this.baseLayer = baseLayer;
        this.talkingLayer = talkingLayer;
        this.eyesLayer = eyesLayer;

        if (data == null || !data.IsValid)
        {
            Debug.LogWarning($"CharacterPortraitAnimator: Invalid portrait data provided for {gameObject.name}");
            isInitialized = false;
            return;
        }

        // Set initial state
        SetIdleSprite();
        ResetTalkingLayer();
        ResetEyesLayer();

        // Mark as initialized FIRST
        isInitialized = true;

        // Start eye blink coroutine (runs continuously)
        // Only start if not already running (OnEnable may have started it)
        if (blinkCoroutine == null && gameObject.activeInHierarchy)
        {
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }

        Debug.Log($"CharacterPortraitAnimator: Initialized for {data.characterName} (Talking: {data.talkingFrames.Length} frames, Eyes: {data.eyeBlinkFrames.Length} frames)");
    }

    /// <summary>
    /// Sets the talking state - starts or stops talking animation.
    /// </summary>
    public void SetTalkingState(bool talking)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"CharacterPortraitAnimator: Cannot set talking state - not initialized");
            return;
        }

        if (isTalking == talking)
            return; // No change

        isTalking = talking;

        // Stop existing talking coroutine
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
        }

        if (isTalking)
        {
            // Start talking animation
            talkingCoroutine = StartCoroutine(TalkingCoroutine());
        }
        else
        {
            // Return to idle (hide talking layer)
            ResetTalkingLayer();
        }
    }

    /// <summary>
    /// Stops all animations and resets to idle state.
    /// </summary>
    public void StopAllAnimations()
    {
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        isTalking = false;
        SetIdleSprite();
        ResetTalkingLayer();
        ResetEyesLayer();

        isInitialized = false;
    }

    void OnDisable()
    {
        // Stop all coroutines when portrait is hidden
        if (talkingCoroutine != null)
        {
            StopCoroutine(talkingCoroutine);
            talkingCoroutine = null;
        }

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;  // Clear the reference
        }
    }

    void OnEnable()
    {
        // Restart blinking when portrait is shown (if already initialized)
        if (isInitialized && blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkCoroutine());
        }
    }

    void OnDestroy()
    {
        StopAllAnimations();
    }

    /// <summary>
    /// Talking animation coroutine - cycles through talking frames at 10fps.
    /// </summary>
    private IEnumerator TalkingCoroutine()
    {
        if (portraitData == null || portraitData.talkingFrames == null || portraitData.talkingFrames.Length == 0)
        {
            Debug.LogWarning($"CharacterPortraitAnimator: No talking frames available");
            yield break;
        }

        float frameDuration = 1f / talkingFPS;
        int frameIndex = 0;

        while (isTalking)
        {
            // Set current talking frame on talking layer
            if (talkingLayer != null && frameIndex < portraitData.talkingFrames.Length)
            {
                talkingLayer.sprite = portraitData.talkingFrames[frameIndex];
                talkingLayer.color = Color.white; // Ensure visible
            }

            // Advance to next frame (loop back to start)
            frameIndex = (frameIndex + 1) % portraitData.talkingFrames.Length;

            yield return new WaitForSeconds(frameDuration);
        }

        // Talking stopped, hide talking layer
        ResetTalkingLayer();
    }

    /// <summary>
    /// Eye blink coroutine - plays blink animation at random intervals.
    /// </summary>
    private IEnumerator BlinkCoroutine()
    {
        // Check if we have valid blink frames (must have at least 3 frames for actual animation)
        if (portraitData == null || portraitData.eyeBlinkFrames == null || portraitData.eyeBlinkFrames.Length < 3)
        {
            // No blink animation (character has dummy frames or insufficient frames)
            yield break;
        }

        // Check if blink frames are all the same sprite (dummy blink)
        bool isDummyBlink = true;
        Sprite firstFrame = portraitData.eyeBlinkFrames[0];
        for (int i = 1; i < portraitData.eyeBlinkFrames.Length; i++)
        {
            if (portraitData.eyeBlinkFrames[i] != firstFrame)
            {
                isDummyBlink = false;
                break;
            }
        }

        if (isDummyBlink)
        {
            // All blink frames are the same (single-frame character), skip blinking
            yield break;
        }

        while (isInitialized)
        {
            // Wait random interval before next blink
            float waitTime = Random.Range(blinkIntervalMin, blinkIntervalMax);
            yield return new WaitForSeconds(waitTime);

            // Play blink animation
            if (eyesLayer != null)
            {
                for (int i = 0; i < portraitData.eyeBlinkFrames.Length; i++)
                {
                    eyesLayer.sprite = portraitData.eyeBlinkFrames[i];
                    eyesLayer.color = Color.white; // Ensure visible

                    yield return new WaitForSeconds(blinkFrameDuration);
                }

                // Reset eyes to transparent after blink
                ResetEyesLayer();
            }
        }
    }

    /// <summary>
    /// Sets base layer to idle sprite.
    /// </summary>
    private void SetIdleSprite()
    {
        if (baseLayer != null && portraitData != null && portraitData.baseIdleSprite != null)
        {
            baseLayer.sprite = portraitData.baseIdleSprite;
        }
    }

    /// <summary>
    /// Resets talking layer to transparent (hides talking animation).
    /// </summary>
    private void ResetTalkingLayer()
    {
        if (talkingLayer != null)
        {
            talkingLayer.sprite = null;
            talkingLayer.color = new Color(1f, 1f, 1f, 0f); // Transparent
        }
    }

    /// <summary>
    /// Resets eyes layer to transparent (no blink overlay).
    /// </summary>
    private void ResetEyesLayer()
    {
        if (eyesLayer != null)
        {
            eyesLayer.sprite = null;
            eyesLayer.color = new Color(1f, 1f, 1f, 0f); // Transparent
        }
    }
}
