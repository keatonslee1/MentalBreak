using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled save/load dialog.
    /// Provides a modal interface for selecting save slots.
    /// </summary>
    public class Win95SaveLoadDialog : MonoBehaviour
    {
        public enum DialogMode
        {
            Save,
            Load
        }

        [Header("Dialog Settings")]
        [SerializeField] private float dialogWidth = 900f;
        [SerializeField] private float dialogHeight = 760f;
        [SerializeField] private int fontSize = 36;
        [SerializeField] private int slotCount = 5;
        [SerializeField] private float slotHeight = 96f;

        // Events
        public event Action<int> OnSlotSelected;
        public event Action OnDialogClosed;

        // UI Elements
        private GameObject dialogRoot;
        private GameObject dimBackground;
        private GameObject titleBar;
        private TMP_Text titleText;
        private GameObject slotContainer;
        private Button[] slotButtons;
        private TMP_Text[] slotLabels;
        private TMP_Text[] slotInfoTexts;
        private Image[] slotSelectedIndicators;
        private Button confirmButton;
        private Button cancelButton;
        private CanvasGroup canvasGroup;

        private DialogMode currentMode = DialogMode.Save;
        private int selectedSlotIndex = -1;
        private bool isInitialized = false;

        private void Awake()
        {
            if (!isInitialized)
            {
                CreateUI();
                isInitialized = true;
            }
        }

        private void CreateUI()
        {
            // Create dim background
            dimBackground = new GameObject("DimBackground");
            dimBackground.transform.SetParent(transform, false);

            RectTransform dimRect = dimBackground.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;

            Image dimImage = dimBackground.AddComponent<Image>();
            dimImage.color = new Color(0, 0, 0, 0.5f);
            dimImage.raycastTarget = true;

            Button dimButton = dimBackground.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(Close);

            // Create dialog window
            dialogRoot = new GameObject("DialogWindow");
            dialogRoot.transform.SetParent(transform, false);

            RectTransform rootRect = dialogRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(dialogWidth, dialogHeight);

            Image bgImage = dialogRoot.AddComponent<Image>();
            bgImage.color = Win95Theme.WindowBackground;
            bgImage.raycastTarget = true;

            canvasGroup = dialogRoot.AddComponent<CanvasGroup>();

            // Add Win95 raised border
            Win95Panel.Create(dialogRoot, Win95Panel.PanelStyle.Raised);

            // Vertical layout
            VerticalLayoutGroup layout = dialogRoot.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Create title bar
            CreateTitleBar();

            // Create slot container
            CreateSlotContainer();

            // Create button row
            CreateButtonRow();

            // Start hidden
            gameObject.SetActive(false);
        }

        private void CreateTitleBar()
        {
            titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(dialogRoot.transform, false);

            LayoutElement titleLayout = titleBar.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = 56;

            Image titleBg = titleBar.AddComponent<Image>();
            titleBg.color = Win95Theme.ColorTitleActive;

            HorizontalLayoutGroup titleHlg = titleBar.AddComponent<HorizontalLayoutGroup>();
            titleHlg.padding = new RectOffset(8, 8, 4, 4);
            titleHlg.spacing = 8;
            titleHlg.childAlignment = TextAnchor.MiddleLeft;
            titleHlg.childControlWidth = true;
            titleHlg.childControlHeight = true;
            titleHlg.childForceExpandWidth = true;
            titleHlg.childForceExpandHeight = true;

            // Title text
            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(titleBar.transform, false);

            titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Save Game";
            titleText.fontSize = fontSize;
            titleText.color = Win95Theme.TitleBarText;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement titleTextLayout = titleTextObj.AddComponent<LayoutElement>();
            titleTextLayout.flexibleWidth = 1;

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(titleBar.transform, false);

            LayoutElement closeLayout = closeObj.AddComponent<LayoutElement>();
            closeLayout.preferredWidth = 44;
            closeLayout.preferredHeight = 44;
            closeLayout.flexibleWidth = 0;

            Button closeButton = closeObj.AddComponent<Button>();
            closeButton.onClick.AddListener(Close);

            Win95Button.Create(closeObj, "X", fontSize - 2);
        }

        private void CreateSlotContainer()
        {
            slotContainer = new GameObject("SlotContainer");
            slotContainer.transform.SetParent(dialogRoot.transform, false);

            LayoutElement containerLayout = slotContainer.AddComponent<LayoutElement>();
            containerLayout.flexibleHeight = 1;

            // Sunken list area
            Image containerBg = slotContainer.AddComponent<Image>();
            containerBg.color = Color.white;

            // Add sunken border
            CreateSunkenBorder(slotContainer);

            VerticalLayoutGroup containerVlg = slotContainer.AddComponent<VerticalLayoutGroup>();
            containerVlg.padding = new RectOffset(8, 8, 8, 8);
            containerVlg.spacing = 4;
            containerVlg.childControlWidth = true;
            containerVlg.childControlHeight = false;
            containerVlg.childForceExpandWidth = true;
            containerVlg.childForceExpandHeight = false;

            // Create slot buttons
            slotButtons = new Button[slotCount];
            slotLabels = new TMP_Text[slotCount];
            slotInfoTexts = new TMP_Text[slotCount];
            slotSelectedIndicators = new Image[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                CreateSlotButton(i);
            }
        }

        private void CreateSlotButton(int index)
        {
            GameObject slotObj = new GameObject($"Slot_{index + 1}");
            slotObj.transform.SetParent(slotContainer.transform, false);

            RectTransform slotRect = slotObj.AddComponent<RectTransform>();

            LayoutElement slotLayout = slotObj.AddComponent<LayoutElement>();
            slotLayout.preferredHeight = slotHeight;
            slotLayout.minHeight = slotHeight;

            // Slot background (changes on selection)
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = Color.white;
            slotSelectedIndicators[index] = slotBg;

            // Button
            Button btn = slotObj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            int slotIndex = index;
            btn.onClick.AddListener(() => SelectSlot(slotIndex));
            slotButtons[index] = btn;

            // Horizontal layout for slot content
            HorizontalLayoutGroup slotHlg = slotObj.AddComponent<HorizontalLayoutGroup>();
            slotHlg.padding = new RectOffset(16, 16, 8, 8);
            slotHlg.spacing = 16;
            slotHlg.childAlignment = TextAnchor.MiddleLeft;
            slotHlg.childControlWidth = true;
            slotHlg.childControlHeight = true;
            slotHlg.childForceExpandWidth = false;
            slotHlg.childForceExpandHeight = true;

            // Slot label (e.g., "Slot 1")
            GameObject labelObj = new GameObject("SlotLabel");
            labelObj.transform.SetParent(slotObj.transform, false);

            TMP_Text labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = $"Slot {index + 1}";
            labelText.fontSize = fontSize;
            labelText.color = Win95Theme.WindowText;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            slotLabels[index] = labelText;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 160;
            labelLayout.flexibleWidth = 0;

            // Slot info (e.g., save date, run/day)
            GameObject infoObj = new GameObject("SlotInfo");
            infoObj.transform.SetParent(slotObj.transform, false);

            TMP_Text infoText = infoObj.AddComponent<TextMeshProUGUI>();
            infoText.text = "Empty";
            infoText.fontSize = fontSize - 2;
            infoText.color = Win95Theme.ColorMidGray;
            infoText.alignment = TextAlignmentOptions.MidlineLeft;
            slotInfoTexts[index] = infoText;

            LayoutElement infoLayout = infoObj.AddComponent<LayoutElement>();
            infoLayout.flexibleWidth = 1;
        }

        private void CreateButtonRow()
        {
            GameObject buttonRow = new GameObject("ButtonRow");
            buttonRow.transform.SetParent(dialogRoot.transform, false);

            LayoutElement buttonLayout = buttonRow.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 96;

            HorizontalLayoutGroup buttonHlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            buttonHlg.padding = new RectOffset(8, 8, 8, 8);
            buttonHlg.spacing = 16;
            buttonHlg.childAlignment = TextAnchor.MiddleRight;
            buttonHlg.childControlWidth = false;
            buttonHlg.childControlHeight = true;
            buttonHlg.childForceExpandWidth = false;
            buttonHlg.childForceExpandHeight = true;

            // Confirm button
            GameObject confirmObj = new GameObject("ConfirmButton");
            confirmObj.transform.SetParent(buttonRow.transform, false);

            RectTransform confirmRect = confirmObj.AddComponent<RectTransform>();
            confirmRect.sizeDelta = new Vector2(200, 64);

            LayoutElement confirmLayout = confirmObj.AddComponent<LayoutElement>();
            confirmLayout.preferredWidth = 200;
            confirmLayout.preferredHeight = 64;

            confirmButton = confirmObj.AddComponent<Button>();
            confirmButton.onClick.AddListener(ConfirmSelection);

            Win95Button.Create(confirmObj, "OK", fontSize);

            // Cancel button
            GameObject cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(buttonRow.transform, false);

            RectTransform cancelRect = cancelObj.AddComponent<RectTransform>();
            cancelRect.sizeDelta = new Vector2(200, 64);

            LayoutElement cancelLayout = cancelObj.AddComponent<LayoutElement>();
            cancelLayout.preferredWidth = 200;
            cancelLayout.preferredHeight = 64;

            cancelButton = cancelObj.AddComponent<Button>();
            cancelButton.onClick.AddListener(Close);

            Win95Button.Create(cancelObj, "Cancel", fontSize);
        }

        private void CreateSunkenBorder(GameObject parent)
        {
            CreateBorderLine(parent, "TopShadow", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -2), Win95Theme.ButtonShadow);
            CreateBorderLine(parent, "LeftShadow", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(2, 0), Win95Theme.ButtonShadow);
            CreateBorderLine(parent, "BottomHighlight", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 2), Win95Theme.ButtonHighlight);
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

        /// <summary>
        /// Show the dialog in save mode.
        /// </summary>
        public void ShowSaveDialog()
        {
            currentMode = DialogMode.Save;
            if (titleText != null) titleText.text = "Save Game";
            RefreshSlotDisplay();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Show the dialog in load mode.
        /// </summary>
        public void ShowLoadDialog()
        {
            currentMode = DialogMode.Load;
            if (titleText != null) titleText.text = "Load Game";
            RefreshSlotDisplay();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Close the dialog.
        /// </summary>
        public void Close()
        {
            selectedSlotIndex = -1;
            gameObject.SetActive(false);
            OnDialogClosed?.Invoke();
        }

        private void SelectSlot(int index)
        {
            selectedSlotIndex = index;
            UpdateSlotSelection();
        }

        private void UpdateSlotSelection()
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (slotSelectedIndicators[i] != null)
                {
                    slotSelectedIndicators[i].color = (i == selectedSlotIndex)
                        ? Win95Theme.ColorTitleActive
                        : Color.white;
                }

                if (slotLabels[i] != null)
                {
                    slotLabels[i].color = (i == selectedSlotIndex)
                        ? Color.white
                        : Win95Theme.WindowText;
                }

                if (slotInfoTexts[i] != null)
                {
                    slotInfoTexts[i].color = (i == selectedSlotIndex)
                        ? new Color(0.8f, 0.8f, 1f, 1f)
                        : Win95Theme.ColorMidGray;
                }
            }
        }

        private void ConfirmSelection()
        {
            if (selectedSlotIndex >= 0)
            {
                OnSlotSelected?.Invoke(selectedSlotIndex + 1); // Slots are 1-indexed for user
                Close();
            }
        }

        /// <summary>
        /// Refresh the slot display with current save data.
        /// </summary>
        public void RefreshSlotDisplay()
        {
            selectedSlotIndex = -1;
            UpdateSlotSelection();

            // Update slot info texts (would connect to SaveLoadManager)
            for (int i = 0; i < slotCount; i++)
            {
                if (slotInfoTexts[i] != null)
                {
                    // TODO: Connect to SaveLoadManager to get actual save info
                    slotInfoTexts[i].text = "Empty";
                }
            }
        }

        /// <summary>
        /// Set the info text for a specific slot.
        /// </summary>
        public void SetSlotInfo(int slotIndex, string info)
        {
            if (slotIndex >= 0 && slotIndex < slotCount && slotInfoTexts[slotIndex] != null)
            {
                slotInfoTexts[slotIndex].text = info;
            }
        }

        /// <summary>
        /// Create a Win95SaveLoadDialog.
        /// </summary>
        public static Win95SaveLoadDialog Create(Transform parent)
        {
            GameObject dialogObj = new GameObject("Win95SaveLoadDialog");
            dialogObj.transform.SetParent(parent, false);

            RectTransform dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.offsetMin = Vector2.zero;
            dialogRect.offsetMax = Vector2.zero;

            return dialogObj.AddComponent<Win95SaveLoadDialog>();
        }
    }
}
