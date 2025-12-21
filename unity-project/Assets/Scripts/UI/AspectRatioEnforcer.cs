using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enforces a target aspect ratio at runtime by letterboxing/pillarboxing via Camera.rect.
/// Uses a dedicated background camera to clear the full screen to pure black.
/// </summary>
public sealed class AspectRatioEnforcer : MonoBehaviour
{
    private static AspectRatioEnforcer instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureExists()
    {
        if (instance != null)
        {
            return;
        }

        GameObject go = new GameObject(nameof(AspectRatioEnforcer));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AspectRatioEnforcer>();
    }

    [Tooltip("Target aspect ratio (width/height). 16:9 = 1.777...")]
    public float targetAspect = 16f / 9f;

    [Tooltip("If true, applies to all enabled cameras each time layout is updated.")]
    public bool affectAllCameras = true;

    [Tooltip("Optional explicit cameras to affect (ignored when Affect All Cameras is true).")]
    public Camera[] cameras;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    private Camera letterboxCamera;
    private readonly Dictionary<Camera, Rect> originalRects = new Dictionary<Camera, Rect>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        ApplyIfNeeded(force: true);
    }

    private void Update()
    {
        ApplyIfNeeded(force: false);
    }

    private void ApplyIfNeeded(bool force)
    {
        if (!force && Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
        {
            return;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (targetAspect <= 0.01f)
        {
            return;
        }

        EnsureLetterboxCamera();

        Rect targetRect = ComputeTargetRect(targetAspect);

        Camera[] targets = GetTargetCameras();
        for (int i = 0; i < targets.Length; i++)
        {
            Camera cam = targets[i];
            if (cam == null) continue;
            if (cam == letterboxCamera) continue;
            if (!cam.enabled) continue;

            if (!originalRects.ContainsKey(cam))
            {
                originalRects[cam] = cam.rect;
            }

            cam.rect = targetRect;
        }
    }

    private Camera[] GetTargetCameras()
    {
        if (affectAllCameras)
        {
            return Camera.allCameras;
        }

        return cameras ?? new Camera[0];
    }

    private void EnsureLetterboxCamera()
    {
        if (letterboxCamera != null)
        {
            return;
        }

        GameObject go = GameObject.Find("__LetterboxCamera");
        if (go == null)
        {
            go = new GameObject("__LetterboxCamera");
        }

        DontDestroyOnLoad(go);

        letterboxCamera = go.GetComponent<Camera>();
        if (letterboxCamera == null)
        {
            letterboxCamera = go.AddComponent<Camera>();
        }

        // Clear full screen to pure black.
        letterboxCamera.rect = new Rect(0f, 0f, 1f, 1f);
        letterboxCamera.clearFlags = CameraClearFlags.SolidColor;
        letterboxCamera.backgroundColor = Color.black;
        letterboxCamera.cullingMask = 0; // render nothing
        letterboxCamera.orthographic = true;
        letterboxCamera.depth = -10000f;
        letterboxCamera.useOcclusionCulling = false;
        letterboxCamera.allowHDR = false;
        letterboxCamera.allowMSAA = false;
    }

    private static Rect ComputeTargetRect(float desiredAspect)
    {
        float windowAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
        float scaleHeight = windowAspect / desiredAspect;

        // If the window is wider than desired, pillarbox. Otherwise, letterbox.
        if (scaleHeight < 1f)
        {
            // Letterbox: full width, reduced height
            float height = scaleHeight;
            float y = (1f - height) * 0.5f;
            return new Rect(0f, y, 1f, height);
        }
        else
        {
            // Pillarbox: full height, reduced width
            float scaleWidth = 1f / scaleHeight;
            float x = (1f - scaleWidth) * 0.5f;
            return new Rect(x, 0f, scaleWidth, 1f);
        }
    }
}


