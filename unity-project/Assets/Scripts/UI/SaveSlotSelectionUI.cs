using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Save/Load slot selection UI with store-style design.
/// Creates UI dynamically at runtime for maximum flexibility.
/// </summary>
public class SaveSlotSelectionUI : MonoBehaviour
{
    public enum Mode { Save, Load }

    [System.Serializable]
    public class SlotRowData
    {
        public int slotNumber;
        public GameObject rowObject;
        public TextMeshProUGUI slotLabel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI infoText;
        public Button actionButton;
        public Button exportButton;
        public Button deleteButton;
        public CanvasGroup canvasGroup;
    }

    [Header("UI References")]
    public GameObject selectionPanel;

    [Header("References")]
    public SaveLoadManager saveLoadManager;
    public PauseMenuManager pauseMenuManager;

    // Runtime UI elements
    private TextMeshProUGUI titleText;
    private Button closeButton;
    private Button importButton;
    private Button backButton;
    private TextMeshProUGUI feedbackText;
    private ScrollRect scrollRect;
    private Transform contentTransform;

    // Slot rows
    private List<SlotRowData> slotRows = new List<SlotRowData>();

    // Confirmation dialog
    private GameObject confirmDialog;
    private TextMeshProUGUI confirmText;
    private Button confirmYesButton;
    private Button confirmNoButton;
    private int pendingDeleteSlot = -999;

    // State
    private Mode currentMode = Mode.Load;
    private bool uiCreated = false;
    private Coroutine feedbackCoroutine;

    // Colors (match CompanyStore)
    private static readonly Color PanelBgColor = new Color(0.08f, 0.08f, 0.12f, 0.98f);
    private static readonly Color HeaderBgColor = new Color(0.12f, 0.12f, 0.18f, 1f);
    private static readonly Color RowBgColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
    private static readonly Color PrimaryButtonColor = new Color(0.2f, 0.5f, 0.2f, 1f);
    private static readonly Color DangerButtonColor = new Color(0.7f, 0.2f, 0.2f, 1f);
    private static readonly Color NeutralButtonColor = new Color(0.3f, 0.3f, 0.4f, 1f);
    private static readonly Color TextSecondaryColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    private static readonly Color TextAccentColor = new Color(0.4f, 0.9f, 0.4f, 1f);

    private void Awake()
    {
        if (saveLoadManager == null)
        {
            saveLoadManager = FindFirstObjectByType<SaveLoadManager>();
            if (saveLoadManager == null && SaveLoadManager.Instance != null)
                saveLoadManager = SaveLoadManager.Instance;
        }

        if (pauseMenuManager == null)
        {
            pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
            if (pauseMenuManager == null && PauseMenuManager.Instance != null)
                pauseMenuManager = PauseMenuManager.Instance;
        }
    }

    private void Start()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(false);
    }

    private void Update()
    {
        if (selectionPanel != null && selectionPanel.activeSelf)
        {
            if (ModalInputLock.IsLocked) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (confirmDialog != null && confirmDialog.activeSelf)
                    OnCancelDelete();
                else
                    OnCancel();
            }
        }
    }

    /// <summary>
    /// Show UI in Save mode
    /// </summary>
    public void ShowSelectionUIForSave()
    {
        currentMode = Mode.Save;
        ShowSelectionUI();
    }

    /// <summary>
    /// Show UI in Load mode
    /// </summary>
    public void ShowSelectionUIForLoad()
    {
        currentMode = Mode.Load;
        ShowSelectionUI();
    }

    /// <summary>
    /// Show the selection UI
    /// </summary>
    public void ShowSelectionUI()
    {
        if (selectionPanel == null)
        {
            Debug.LogError("[SaveSlotSelectionUI] selectionPanel is null!");
            return;
        }

        // Create UI if not already created
        if (!uiCreated)
        {
            CreateUI();
            uiCreated = true;
        }

        // Update title
        if (titleText != null)
            titleText.text = currentMode == Mode.Save ? "SAVE GAME" : "LOAD GAME";

        // Show/hide import button (Load mode only)
        if (importButton != null)
            importButton.gameObject.SetActive(currentMode == Mode.Load);

        // Update all slot rows
        UpdateAllSlotRows();

        // Show panel
        selectionPanel.SetActive(true);

        // Ensure proper layering
        Canvas canvas = selectionPanel.GetComponentInParent<Canvas>();
        if (canvas != null)
            canvas.sortingOrder = 5000;

        Debug.Log($"[SaveSlotSelectionUI] Showing UI in {currentMode} mode");
    }

    /// <summary>
    /// Hide the selection UI
    /// </summary>
    public void HideSelectionUI()
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (confirmDialog != null)
            confirmDialog.SetActive(false);

        ClearFeedback();
    }

    /// <summary>
    /// Create the entire UI programmatically
    /// </summary>
    private void CreateUI()
    {
        // Clear any existing children
        foreach (Transform child in selectionPanel.transform)
            Destroy(child.gameObject);

        slotRows.Clear();

        // REMOVE any layout components from the old UI that could override our sizing
        var layoutGroup = selectionPanel.GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            Debug.Log("[SaveSlotSelectionUI] Removing old LayoutGroup from panel");
            Destroy(layoutGroup);
        }

        var contentFitter = selectionPanel.GetComponent<ContentSizeFitter>();
        if (contentFitter != null)
        {
            Debug.Log("[SaveSlotSelectionUI] Removing old ContentSizeFitter from panel");
            Destroy(contentFitter);
        }

        var layoutElement = selectionPanel.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            Debug.Log("[SaveSlotSelectionUI] Removing old LayoutElement from panel");
            Destroy(layoutElement);
        }

        RectTransform panelRect = selectionPanel.GetComponent<RectTransform>();
        if (panelRect == null)
            panelRect = selectionPanel.AddComponent<RectTransform>();

        // Ensure panel is center-anchored with fixed size
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(480, 580);
        panelRect.anchoredPosition = Vector2.zero;

        // Panel background
        Image panelImage = selectionPanel.GetComponent<Image>();
        if (panelImage == null)
            panelImage = selectionPanel.AddComponent<Image>();
        panelImage.color = PanelBgColor;

        // Header
        CreateHeader();

        // Scroll view for slots
        CreateScrollView();

        // Create slot rows
        CreateSlotRows();

        // Footer
        CreateFooter();

        // Confirmation dialog (hidden initially)
        CreateConfirmDialog();

        Debug.Log("[SaveSlotSelectionUI] UI created successfully");
    }

    private void CreateHeader()
    {
        // Header background
        GameObject headerBg = CreateUIElement(selectionPanel.transform, "HeaderBackground",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -30), new Vector2(0, 60));
        Image headerImage = headerBg.AddComponent<Image>();
        headerImage.color = HeaderBgColor;

        // Title text
        GameObject titleObj = CreateUIElement(headerBg.transform, "TitleText",
            new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-60, 0));
        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "LOAD GAME";
        titleText.fontSize = 26;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Close button (X)
        GameObject closeObj = CreateUIElement(headerBg.transform, "CloseButton",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(48, 48));
        Image closeImage = closeObj.AddComponent<Image>();
        closeImage.color = DangerButtonColor;
        closeButton = closeObj.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(OnCancel);

        SetButtonColors(closeButton, DangerButtonColor);

        // X text
        GameObject closeText = CreateUIElement(closeObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI closeTMP = closeText.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 24;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;
    }

    private void CreateScrollView()
    {
        // Scroll view container
        GameObject scrollObj = CreateUIElement(selectionPanel.transform, "ScrollView",
            new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, -10), new Vector2(-20, -130));
        RectTransform scrollRect = scrollObj.GetComponent<RectTransform>();
        scrollRect.anchoredPosition = new Vector2(0, -10);

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0);

        this.scrollRect = scrollObj.AddComponent<ScrollRect>();
        this.scrollRect.horizontal = false;
        this.scrollRect.vertical = true;
        this.scrollRect.movementType = ScrollRect.MovementType.Clamped;
        this.scrollRect.scrollSensitivity = 30f;

        // Viewport
        GameObject viewport = CreateUIElement(scrollObj.transform, "Viewport",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        viewport.AddComponent<RectMask2D>();
        this.scrollRect.viewport = viewport.GetComponent<RectTransform>();

        // Content
        GameObject content = CreateUIElement(viewport.transform, "Content",
            new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.pivot = new Vector2(0.5f, 1);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        this.scrollRect.content = contentRect;
        contentTransform = content.transform;
    }

    private void CreateSlotRows()
    {
        // Manual slots 1-5
        for (int i = 1; i <= 5; i++)
            CreateSlotRow(i, false);

        // Autosave slots 0, -1, -2
        CreateSlotRow(0, true);
        CreateSlotRow(-1, true);
        CreateSlotRow(-2, true);
    }

    private void CreateSlotRow(int slotNumber, bool isAutosave)
    {
        GameObject row = new GameObject($"SlotRow_{slotNumber}");
        row.transform.SetParent(contentTransform, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 80);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = RowBgColor;

        CanvasGroup canvasGroup = row.AddComponent<CanvasGroup>();

        // Slot indicator (left side)
        GameObject slotIndicator = CreateUIElement(row.transform, "SlotIndicator",
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(40, 60));
        Image slotIndBg = slotIndicator.AddComponent<Image>();
        slotIndBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject slotLabelObj = CreateUIElement(slotIndicator.transform, "Label",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI slotLabel = slotLabelObj.AddComponent<TextMeshProUGUI>();
        if (isAutosave)
        {
            slotLabel.text = slotNumber == 0 ? "A0" : slotNumber == -1 ? "A1" : "A2";
        }
        else
        {
            slotLabel.text = $"#{slotNumber}";
        }
        slotLabel.fontSize = 16;
        slotLabel.fontStyle = FontStyles.Bold;
        slotLabel.alignment = TextAlignmentOptions.Center;
        slotLabel.color = TextAccentColor;

        // Slot name
        GameObject nameObj = CreateUIElement(row.transform, "NameText",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(60, -8), new Vector2(200, 28));
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.pivot = new Vector2(0, 1);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = isAutosave ? GetAutosaveLabel(slotNumber) : $"SLOT {slotNumber}";
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = Color.white;

        // Info text (run/day, timestamp)
        GameObject infoObj = CreateUIElement(row.transform, "InfoText",
            new Vector2(0, 0), new Vector2(0, 0), new Vector2(60, 8), new Vector2(280, 36));
        RectTransform infoRect = infoObj.GetComponent<RectTransform>();
        infoRect.pivot = new Vector2(0, 0);
        TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "(Empty)";
        infoText.fontSize = 14;
        infoText.alignment = TextAlignmentOptions.Left;
        infoText.color = TextSecondaryColor;
        infoText.enableWordWrapping = true;

        // Action button (Load/Save)
        GameObject actionObj = CreateUIElement(row.transform, "ActionButton",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-100, 0), new Vector2(70, 40));
        Image actionImage = actionObj.AddComponent<Image>();
        actionImage.color = PrimaryButtonColor;
        Button actionButton = actionObj.AddComponent<Button>();
        actionButton.targetGraphic = actionImage;
        SetButtonColors(actionButton, PrimaryButtonColor);

        int slot = slotNumber; // Capture for lambda
        actionButton.onClick.AddListener(() => OnSlotSelected(slot));

        GameObject actionText = CreateUIElement(actionObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI actionTMP = actionText.AddComponent<TextMeshProUGUI>();
        actionTMP.text = "LOAD";
        actionTMP.fontSize = 14;
        actionTMP.fontStyle = FontStyles.Bold;
        actionTMP.alignment = TextAlignmentOptions.Center;
        actionTMP.color = Color.white;

        // Export button
        GameObject exportObj = CreateUIElement(row.transform, "ExportButton",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-55, 0), new Vector2(36, 36));
        Image exportImage = exportObj.AddComponent<Image>();
        exportImage.color = NeutralButtonColor;
        Button exportButton = exportObj.AddComponent<Button>();
        exportButton.targetGraphic = exportImage;
        SetButtonColors(exportButton, NeutralButtonColor);
        exportButton.onClick.AddListener(() => OnExportSlot(slot));

        GameObject exportText = CreateUIElement(exportObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI exportTMP = exportText.AddComponent<TextMeshProUGUI>();
        exportTMP.text = "\u2197"; // Arrow symbol
        exportTMP.fontSize = 18;
        exportTMP.alignment = TextAlignmentOptions.Center;
        exportTMP.color = Color.white;

        // Delete button
        GameObject deleteObj = CreateUIElement(row.transform, "DeleteButton",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(36, 36));
        Image deleteImage = deleteObj.AddComponent<Image>();
        deleteImage.color = DangerButtonColor;
        Button deleteButton = deleteObj.AddComponent<Button>();
        deleteButton.targetGraphic = deleteImage;
        SetButtonColors(deleteButton, DangerButtonColor);
        deleteButton.onClick.AddListener(() => ShowConfirmDelete(slot));

        GameObject deleteText = CreateUIElement(deleteObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI deleteTMP = deleteText.AddComponent<TextMeshProUGUI>();
        deleteTMP.text = "X";
        deleteTMP.fontSize = 16;
        deleteTMP.fontStyle = FontStyles.Bold;
        deleteTMP.alignment = TextAlignmentOptions.Center;
        deleteTMP.color = Color.white;

        // Store row data
        SlotRowData rowData = new SlotRowData
        {
            slotNumber = slotNumber,
            rowObject = row,
            slotLabel = slotLabel,
            nameText = nameText,
            infoText = infoText,
            actionButton = actionButton,
            exportButton = exportButton,
            deleteButton = deleteButton,
            canvasGroup = canvasGroup
        };
        slotRows.Add(rowData);
    }

    private void CreateFooter()
    {
        // Feedback text
        GameObject feedbackObj = CreateUIElement(selectionPanel.transform, "FeedbackText",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 65), new Vector2(-20, 25));
        feedbackText = feedbackObj.AddComponent<TextMeshProUGUI>();
        feedbackText.text = "";
        feedbackText.fontSize = 16;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.color = TextAccentColor;

        // Footer buttons container
        GameObject footerObj = CreateUIElement(selectionPanel.transform, "Footer",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 25), new Vector2(-20, 40));

        HorizontalLayoutGroup footerLayout = footerObj.AddComponent<HorizontalLayoutGroup>();
        footerLayout.spacing = 20;
        footerLayout.childAlignment = TextAnchor.MiddleCenter;
        footerLayout.childControlWidth = false;
        footerLayout.childControlHeight = false;

        // Import button (Load mode only)
        GameObject importObj = new GameObject("ImportButton");
        importObj.transform.SetParent(footerObj.transform, false);
        RectTransform importRect = importObj.AddComponent<RectTransform>();
        importRect.sizeDelta = new Vector2(140, 40);
        Image importImage = importObj.AddComponent<Image>();
        importImage.color = NeutralButtonColor;
        importButton = importObj.AddComponent<Button>();
        importButton.targetGraphic = importImage;
        SetButtonColors(importButton, NeutralButtonColor);
        importButton.onClick.AddListener(OnImportClicked);

        GameObject importText = CreateUIElement(importObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI importTMP = importText.AddComponent<TextMeshProUGUI>();
        importTMP.text = "IMPORT";
        importTMP.fontSize = 16;
        importTMP.fontStyle = FontStyles.Bold;
        importTMP.alignment = TextAlignmentOptions.Center;
        importTMP.color = Color.white;

        // Back button
        GameObject backObj = new GameObject("BackButton");
        backObj.transform.SetParent(footerObj.transform, false);
        RectTransform backRect = backObj.AddComponent<RectTransform>();
        backRect.sizeDelta = new Vector2(140, 40);
        Image backImage = backObj.AddComponent<Image>();
        backImage.color = NeutralButtonColor;
        backButton = backObj.AddComponent<Button>();
        backButton.targetGraphic = backImage;
        SetButtonColors(backButton, NeutralButtonColor);
        backButton.onClick.AddListener(OnCancel);

        GameObject backText = CreateUIElement(backObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI backTMP = backText.AddComponent<TextMeshProUGUI>();
        backTMP.text = "BACK";
        backTMP.fontSize = 16;
        backTMP.fontStyle = FontStyles.Bold;
        backTMP.alignment = TextAlignmentOptions.Center;
        backTMP.color = Color.white;
    }

    private void CreateConfirmDialog()
    {
        // Overlay background
        confirmDialog = CreateUIElement(selectionPanel.transform, "ConfirmDialog",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image overlayImage = confirmDialog.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.7f);
        overlayImage.raycastTarget = true;

        // Dialog box
        GameObject dialogBox = CreateUIElement(confirmDialog.transform, "DialogBox",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320, 180));
        Image dialogImage = dialogBox.AddComponent<Image>();
        dialogImage.color = PanelBgColor;

        // Title
        GameObject titleObj = CreateUIElement(dialogBox.transform, "Title",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -20), new Vector2(-20, 35));
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "DELETE SAVE?";
        titleTMP.fontSize = 22;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;

        // Confirm text
        GameObject textObj = CreateUIElement(dialogBox.transform, "ConfirmText",
            new Vector2(0, 0.4f), new Vector2(1, 0.8f), Vector2.zero, new Vector2(-20, 0));
        confirmText = textObj.AddComponent<TextMeshProUGUI>();
        confirmText.text = "Delete save from Slot 1?";
        confirmText.fontSize = 16;
        confirmText.alignment = TextAlignmentOptions.Center;
        confirmText.color = TextSecondaryColor;

        // Buttons container
        GameObject buttonsObj = CreateUIElement(dialogBox.transform, "Buttons",
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 25), new Vector2(-20, 45));

        HorizontalLayoutGroup buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
        buttonsLayout.spacing = 30;
        buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
        buttonsLayout.childControlWidth = false;
        buttonsLayout.childControlHeight = false;

        // Cancel button
        GameObject cancelObj = new GameObject("CancelButton");
        cancelObj.transform.SetParent(buttonsObj.transform, false);
        RectTransform cancelRect = cancelObj.AddComponent<RectTransform>();
        cancelRect.sizeDelta = new Vector2(100, 40);
        Image cancelImage = cancelObj.AddComponent<Image>();
        cancelImage.color = NeutralButtonColor;
        confirmNoButton = cancelObj.AddComponent<Button>();
        confirmNoButton.targetGraphic = cancelImage;
        SetButtonColors(confirmNoButton, NeutralButtonColor);
        confirmNoButton.onClick.AddListener(OnCancelDelete);

        GameObject cancelText = CreateUIElement(cancelObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI cancelTMP = cancelText.AddComponent<TextMeshProUGUI>();
        cancelTMP.text = "CANCEL";
        cancelTMP.fontSize = 14;
        cancelTMP.fontStyle = FontStyles.Bold;
        cancelTMP.alignment = TextAlignmentOptions.Center;
        cancelTMP.color = Color.white;

        // Delete button
        GameObject deleteObj = new GameObject("DeleteButton");
        deleteObj.transform.SetParent(buttonsObj.transform, false);
        RectTransform deleteRect = deleteObj.AddComponent<RectTransform>();
        deleteRect.sizeDelta = new Vector2(100, 40);
        Image deleteImage = deleteObj.AddComponent<Image>();
        deleteImage.color = DangerButtonColor;
        confirmYesButton = deleteObj.AddComponent<Button>();
        confirmYesButton.targetGraphic = deleteImage;
        SetButtonColors(confirmYesButton, DangerButtonColor);
        confirmYesButton.onClick.AddListener(OnConfirmDelete);

        GameObject deleteText = CreateUIElement(deleteObj.transform, "Text",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        TextMeshProUGUI deleteTMP = deleteText.AddComponent<TextMeshProUGUI>();
        deleteTMP.text = "DELETE";
        deleteTMP.fontSize = 14;
        deleteTMP.fontStyle = FontStyles.Bold;
        deleteTMP.alignment = TextAlignmentOptions.Center;
        deleteTMP.color = Color.white;

        confirmDialog.SetActive(false);
    }

    private void UpdateAllSlotRows()
    {
        foreach (var row in slotRows)
        {
            UpdateSlotRow(row);
        }
    }

    private void UpdateSlotRow(SlotRowData row)
    {
        bool isAutosave = row.slotNumber <= 0;
        SaveSlotData slotData = saveLoadManager?.GetSaveSlotData(row.slotNumber);
        bool hasData = slotData != null;

        // Update name text
        if (isAutosave)
        {
            row.nameText.text = GetAutosaveLabel(row.slotNumber);
        }
        else
        {
            row.nameText.text = $"SLOT {row.slotNumber}";
        }

        // Update info text
        if (hasData)
        {
            string info = $"Run {slotData.run} - Day {slotData.day}";
            if (DateTime.TryParse(slotData.timestamp, out DateTime timestamp))
            {
                info += $"\n{timestamp:MMM dd, yyyy HH:mm}";
            }
            row.infoText.text = info;
        }
        else
        {
            row.infoText.text = "(Empty)";
        }

        // Update action button
        TextMeshProUGUI actionText = row.actionButton.GetComponentInChildren<TextMeshProUGUI>();
        if (currentMode == Mode.Save)
        {
            actionText.text = "SAVE";
            // Can always save to manual slots, but not to autosave slots
            row.actionButton.interactable = !isAutosave;
            row.rowObject.SetActive(!isAutosave); // Hide autosave rows in Save mode
        }
        else // Load mode
        {
            actionText.text = "LOAD";
            row.actionButton.interactable = hasData;
            row.rowObject.SetActive(true);
        }

        // Update export/delete buttons (only show if has data)
        row.exportButton.gameObject.SetActive(hasData);
        row.deleteButton.gameObject.SetActive(hasData);

        // Update row opacity
        if (currentMode == Mode.Load && !hasData)
        {
            row.canvasGroup.alpha = 0.5f;
        }
        else
        {
            row.canvasGroup.alpha = 1f;
        }
    }

    private string GetAutosaveLabel(int slot)
    {
        switch (slot)
        {
            case 0: return "AUTOSAVE (Newest)";
            case -1: return "AUTOSAVE (Middle)";
            case -2: return "AUTOSAVE (Oldest)";
            default: return "AUTOSAVE";
        }
    }

    #region Button Handlers

    public void OnSlotSelected(int slot)
    {
        if (saveLoadManager == null)
        {
            ShowFeedback("Save system not available!", Color.red);
            return;
        }

        bool success = false;

        if (currentMode == Mode.Save)
        {
            success = saveLoadManager.SaveGame(slot);
            if (success)
            {
                ShowFeedback($"Saved to Slot {slot}!", TextAccentColor);
                ToastManager.ShowSuccess($"Game saved to Slot {slot}");
                UpdateAllSlotRows();
            }
            else
            {
                ShowFeedback("Failed to save!", Color.red);
                ToastManager.ShowError("Failed to save game");
            }
        }
        else // Load mode
        {
            success = saveLoadManager.LoadGame(slot);
            if (success)
            {
                ToastManager.ShowSuccess("Game loaded");
                HideSelectionUI();
                if (pauseMenuManager != null)
                    pauseMenuManager.ResumeGame();
            }
            else
            {
                if (!saveLoadManager.HasSaveData(slot))
                    ShowFeedback("No save in this slot!", Color.yellow);
                else
                    ShowFeedback("Failed to load!", Color.red);
            }
        }
    }

    public void OnExportSlot(int slot)
    {
        if (saveLoadManager == null) return;

        string exportString = saveLoadManager.ExportSlotToString(slot);
        if (!string.IsNullOrEmpty(exportString))
        {
            GUIUtility.systemCopyBuffer = exportString;
            ShowFeedback("Save copied to clipboard!", TextAccentColor);
            ToastManager.ShowSuccess("Save copied to clipboard");
        }
        else
        {
            ShowFeedback("Export failed!", Color.red);
        }
    }

    public void ShowConfirmDelete(int slot)
    {
        pendingDeleteSlot = slot;

        SaveSlotData slotData = saveLoadManager?.GetSaveSlotData(slot);
        string slotName = slot <= 0 ? GetAutosaveLabel(slot) : $"Slot {slot}";
        string info = slotData != null ? $"\nRun {slotData.run} - Day {slotData.day}" : "";

        confirmText.text = $"Delete save from {slotName}?{info}";
        confirmDialog.SetActive(true);
    }

    public void OnConfirmDelete()
    {
        if (pendingDeleteSlot == -999) return;

        if (saveLoadManager != null && saveLoadManager.DeleteSave(pendingDeleteSlot))
        {
            ShowFeedback("Save deleted!", TextAccentColor);
            ToastManager.ShowSuccess("Save deleted");
            UpdateAllSlotRows();
        }
        else
        {
            ShowFeedback("Delete failed!", Color.red);
        }

        confirmDialog.SetActive(false);
        pendingDeleteSlot = -999;
    }

    public void OnCancelDelete()
    {
        confirmDialog.SetActive(false);
        pendingDeleteSlot = -999;
    }

    public void OnImportClicked()
    {
        string clipboardContent = GUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(clipboardContent))
        {
            ShowFeedback("Clipboard is empty!", Color.yellow);
            return;
        }

        if (!SaveExporter.ValidateSaveString(clipboardContent))
        {
            ShowFeedback("Invalid save string!", Color.red);
            return;
        }

        // Find first empty manual slot
        int targetSlot = -1;
        for (int i = 1; i <= 5; i++)
        {
            if (!saveLoadManager.HasSaveData(i))
            {
                targetSlot = i;
                break;
            }
        }

        if (targetSlot == -1)
        {
            targetSlot = 5; // Overwrite slot 5 if all full
            ShowFeedback("All slots full - using Slot 5", Color.yellow);
        }

        if (saveLoadManager.ImportFromString(clipboardContent, targetSlot))
        {
            ShowFeedback($"Imported to Slot {targetSlot}!", TextAccentColor);
            ToastManager.ShowSuccess($"Save imported to Slot {targetSlot}");
            UpdateAllSlotRows();
        }
        else
        {
            ShowFeedback("Import failed!", Color.red);
        }
    }

    public void OnCancel()
    {
        HideSelectionUI();

        if (pauseMenuManager != null)
            pauseMenuManager.ShowPauseMenu();
    }

    #endregion

    #region Utility Methods

    private GameObject CreateUIElement(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        return obj;
    }

    private void SetButtonColors(Button button, Color normalColor)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = normalColor * 1.2f;
        colors.pressedColor = normalColor * 0.8f;
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        button.colors = colors;
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = color;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(ClearFeedbackAfterDelay(3f));
    }

    private void ClearFeedback()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        if (feedbackText != null)
            feedbackText.text = "";
    }

    private IEnumerator ClearFeedbackAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (feedbackText != null)
            feedbackText.text = "";
        feedbackCoroutine = null;
    }

    #endregion
}
