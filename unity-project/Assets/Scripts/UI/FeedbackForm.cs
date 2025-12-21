using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections.Generic;

public class FeedbackForm : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject feedbackForm;
    [SerializeField] private Button openFeedbackFormButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private TMP_InputField feedbackText;
    [SerializeField] private int maxCharacters = 1000;

    private bool isOpen;
    private float previousTimeScale = 1f;
    private bool openedWhilePausedMenuOpen;

    private Transform originalParent;
    private int originalSiblingIndex;

    private GameObject modalBlocker;
    private IDisposable modalToken;
    private readonly List<LineAdvancer> disabledLineAdvancers = new List<LineAdvancer>();
    private readonly List<MonoBehaviour> disabledInputBehaviours = new List<MonoBehaviour>();
    private bool isSubmitting;

    [Serializable]
    private class FeedbackPayload
    {
        public string message;
        public int run;
        public int day;
        public string buildVersion;
        public string nodeName;
        public string pageUrl;
    }

    private void Start()
    {
        if (openFeedbackFormButton != null)
        {
            openFeedbackFormButton.onClick.AddListener(OpenFeedbackFormPressed);

            // Hide the prefab-provided feedback button; we use the unified HUD button instead.
            if (openFeedbackFormButton.gameObject != null)
            {
                openFeedbackFormButton.gameObject.SetActive(false);
            }
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelButtonPressed);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitButtonPressed);
        }

        if (feedbackText != null)
        {
            feedbackText.characterLimit = Mathf.Max(0, maxCharacters);
        }

        if (feedbackForm != null)
        {
            feedbackForm.SetActive(false);
        }
    }

    /// <summary>
    /// Public entry point for the unified HUD Feedback button.
    /// </summary>
    public void Open()
    {
        OpenFeedbackFormPressed();
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Prevent PauseMenuManager (and others) from responding to Escape in the same frame.
            ModalInputLock.ConsumeEscapeThisFrame();
            CloseFeedbackForm();
        }
    }

    private void OpenFeedbackFormPressed()
    {
        if (feedbackForm == null)
        {
            Debug.LogWarning("FeedbackForm: feedbackForm is not assigned.");
            return;
        }

        if (isOpen)
        {
            return;
        }

        // Track whether pause menu was already open so closing the form does not unpause.
        openedWhilePausedMenuOpen = PauseMenuManager.Instance != null && PauseMenuManager.Instance.IsPaused;

        // Acquire global modal lock (gates other input handlers).
        modalToken = ModalInputLock.Acquire(this);

        // Pause time while modal is open.
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Disable dialogue advance inputs so story cannot progress while feedback modal is open.
        DisableDialogueInput();

        // Cache original hierarchy and move to ModalUIRoot so it renders above OverlayUIRoot.
        originalParent = feedbackForm.transform.parent;
        originalSiblingIndex = feedbackForm.transform.GetSiblingIndex();

        Canvas modalCanvas = ModalCanvasProvider.GetCanvas();
        if (modalCanvas != null)
        {
            feedbackForm.transform.SetParent(modalCanvas.transform, false);
        }
        else
        {
            Debug.LogWarning("FeedbackForm: ModalCanvasProvider returned null canvas; modal may not render above overlay.");
        }

        EnsureModalBlocker();
        if (modalBlocker != null)
        {
            modalBlocker.transform.SetAsFirstSibling(); // behind the form
        }
        feedbackForm.transform.SetAsLastSibling();

        // Open + focus input
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        feedbackForm.SetActive(true);
        isOpen = true;

        if (feedbackText != null)
        {
            feedbackText.Select();
            feedbackText.ActivateInputField();
        }

        if (EventSystem.current != null && feedbackText != null)
        {
            EventSystem.current.SetSelectedGameObject(feedbackText.gameObject);
        }
    }

    private void CancelButtonPressed()
    {
        CloseFeedbackForm();
    }

    private void SubmitButtonPressed()
    {
        if (isSubmitting)
        {
            return;
        }

        if (feedbackText == null)
        {
            return;
        }

        string message = (feedbackText.text ?? string.Empty).Trim();
        if (message.Length == 0)
        {
            // Do not submit empty feedback.
            return;
        }

        int run = 1;
        int day = 1;
        string nodeName = null;

        try
        {
            var watcher = DialogueRuntimeWatcher.Instance;
            var storage = watcher != null ? watcher.CurrentVariableStorage : null;
            if (storage != null)
            {
                if (storage.TryGetValue<float>("$current_run", out var runValue))
                {
                    run = Mathf.Max(1, Mathf.RoundToInt(runValue));
                }
                if (storage.TryGetValue<float>("$current_day", out var dayValue))
                {
                    day = Mathf.Max(1, Mathf.RoundToInt(dayValue));
                }
            }

            var runner = watcher != null ? watcher.CurrentRunner : null;
            if (runner != null && runner.Dialogue != null)
            {
                nodeName = runner.Dialogue.CurrentNode;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"FeedbackForm: Could not read run/day from Yarn storage. Defaulting to Run 1 Day 1. ({ex.Message})");
        }

        var payload = new FeedbackPayload
        {
            message = message,
            run = run,
            day = day,
            buildVersion = Application.version,
            nodeName = nodeName,
            pageUrl = Application.absoluteURL
        };

        StartCoroutine(SendFeedback(payload));
    }

    private IEnumerator SendFeedback(FeedbackPayload payload)
    {
        isSubmitting = true;
        if (submitButton != null) submitButton.interactable = false;
        if (cancelButton != null) cancelButton.interactable = false;

        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        using (var req = new UnityWebRequest("/api/feedback", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            bool ok = req.result == UnityWebRequest.Result.Success || (req.responseCode >= 200 && req.responseCode < 300);
            if (ok)
            {
                isSubmitting = false;
                if (submitButton != null) submitButton.interactable = true;
                if (cancelButton != null) cancelButton.interactable = true;
                CloseFeedbackForm();
                yield break;
            }

            Debug.LogWarning($"FeedbackForm: Feedback submit failed. HTTP {req.responseCode}. Error='{req.error}'. Response='{req.downloadHandler?.text}'");
        }

        isSubmitting = false;
        if (submitButton != null) submitButton.interactable = true;
        if (cancelButton != null) cancelButton.interactable = true;
    }

    private void CloseFeedbackForm()
    {
        // Always reset submit state so the form doesn't get stuck disabled if it was closed mid-request.
        isSubmitting = false;
        if (submitButton != null) submitButton.interactable = true;
        if (cancelButton != null) cancelButton.interactable = true;

        if (!isOpen)
        {
            // Still ensure object is hidden if someone calls close while not open.
            if (feedbackForm != null)
            {
                feedbackForm.SetActive(false);
            }
            return;
        }

        isOpen = false;

        if (feedbackForm != null)
        {
            feedbackForm.SetActive(false);
        }

        if (modalBlocker != null)
        {
            Destroy(modalBlocker);
            modalBlocker = null;
        }

        // Restore original hierarchy
        if (feedbackForm != null && originalParent != null)
        {
            feedbackForm.transform.SetParent(originalParent, false);
            feedbackForm.transform.SetSiblingIndex(originalSiblingIndex);
        }

        // Restore time scale
        // If the pause menu was open when we opened the feedback form, timeScale should remain 0.
        if (openedWhilePausedMenuOpen)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
        }

        EnableDialogueInput();

        // Release modal lock
        if (modalToken != null)
        {
            modalToken.Dispose();
            modalToken = null;
        }
    }

    private void DisableDialogueInput()
    {
        disabledLineAdvancers.Clear();
        disabledInputBehaviours.Clear();

        LineAdvancer[] advancers = FindObjectsByType<LineAdvancer>(FindObjectsSortMode.None);
        foreach (LineAdvancer advancer in advancers)
        {
            if (advancer != null && advancer.enabled)
            {
                disabledLineAdvancers.Add(advancer);
                advancer.enabled = false;
            }
        }

        // Disable BubbleInput components without taking a compile-time dependency on the add-on assembly.
        MonoBehaviour[] allBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (MonoBehaviour behaviour in allBehaviours)
        {
            if (behaviour != null && behaviour.enabled)
            {
                string typeName = behaviour.GetType().Name;
                if (typeName == "BubbleInput")
                {
                    disabledInputBehaviours.Add(behaviour);
                    behaviour.enabled = false;
                }
            }
        }
    }

    private void EnableDialogueInput()
    {
        foreach (LineAdvancer advancer in disabledLineAdvancers)
        {
            if (advancer != null)
            {
                advancer.enabled = true;
            }
        }
        disabledLineAdvancers.Clear();

        foreach (MonoBehaviour behaviour in disabledInputBehaviours)
        {
            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }
        disabledInputBehaviours.Clear();
    }

    private void EnsureModalBlocker()
    {
        if (modalBlocker != null || feedbackForm == null)
        {
            return;
        }

        Transform parent = feedbackForm.transform.parent;
        if (parent == null)
        {
            return;
        }

        modalBlocker = new GameObject("ModalBlocker");
        modalBlocker.transform.SetParent(parent, false);

        RectTransform rect = modalBlocker.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = modalBlocker.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f); // invisible
        image.raycastTarget = true; // blocks clicks to underlying UI
    }
}