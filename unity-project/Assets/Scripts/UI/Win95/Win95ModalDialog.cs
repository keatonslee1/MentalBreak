using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled modal dialog.
    /// Provides a standard Win95 dialog window with title bar, content area, and button row.
    /// Can be used as a base for save/load, settings, and other modal dialogs.
    /// </summary>
    public class Win95ModalDialog : MonoBehaviour
    {
        [Header("Dialog Settings")]
        [SerializeField] private string dialogTitle = "Dialog";
        [SerializeField] private float dialogWidth = 400f;
        [SerializeField] private float dialogHeight = 300f;
        [SerializeField] private bool showCloseButton = true;
        [SerializeField] private int fontSize = 36;

        [Header("Layout")]
        [SerializeField] private float titleBarHeight = 56f;
        [SerializeField] private float buttonRowHeight = 96f;
        [SerializeField] private float contentPadding = 16f;

        // UI Elements
        private GameObject dialogRoot;
        private GameObject titleBar;
        private TMP_Text titleText;
        private Button closeButton;
        private GameObject contentArea;
        private GameObject buttonRow;
        private CanvasGroup canvasGroup;
        private Image dimBackground;

        // Events
        public event Action OnCloseClicked;
        public event Action OnDialogOpened;
        public event Action OnDialogClosed;

        // Public accessors
        public GameObject ContentArea => contentArea;
        public GameObject ButtonRow => buttonRow;
        public CanvasGroup CanvasGroup => canvasGroup;
        public string Title
        {
            get => dialogTitle;
            set
            {
                dialogTitle = value;
                if (titleText != null) titleText.text = value;
            }
        }

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
            // Create dim background (covers entire screen)
            GameObject dimObj = new GameObject("DimBackground");
            dimObj.transform.SetParent(transform, false);
            dimObj.transform.SetAsFirstSibling();

            RectTransform dimRect = dimObj.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;

            dimBackground = dimObj.AddComponent<Image>();
            dimBackground.color = new Color(0, 0, 0, 0.5f);
            dimBackground.raycastTarget = true;

            // Add button to close when clicking outside
            Button dimButton = dimObj.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(() => { if (showCloseButton) Close(); });

            // Create dialog root (centered window)
            dialogRoot = new GameObject("DialogWindow");
            dialogRoot.transform.SetParent(transform, false);

            RectTransform rootRect = dialogRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(dialogWidth, dialogHeight);

            // Win95 window background
            Image bgImage = dialogRoot.AddComponent<Image>();
            bgImage.color = Win95Theme.WindowBackground;
            bgImage.raycastTarget = true;

            // Canvas group for fade effects
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
            layout.childAlignment = TextAnchor.UpperCenter;

            // Create title bar
            CreateTitleBar();

            // Create content area
            CreateContentArea();

            // Create button row
            CreateButtonRow();

            // Start hidden
            gameObject.SetActive(false);
        }

        private void CreateTitleBar()
        {
            titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(dialogRoot.transform, false);

            RectTransform titleRect = titleBar.AddComponent<RectTransform>();

            LayoutElement titleLayout = titleBar.AddComponent<LayoutElement>();
            titleLayout.preferredHeight = titleBarHeight;
            titleLayout.minHeight = titleBarHeight;

            // Title bar background (active blue)
            Image titleBg = titleBar.AddComponent<Image>();
            titleBg.color = Win95Theme.ColorTitleActive;

            // Horizontal layout for title + close button
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
            titleText.text = dialogTitle;
            titleText.fontSize = fontSize;
            titleText.color = Win95Theme.TitleBarText;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement titleTextLayout = titleTextObj.AddComponent<LayoutElement>();
            titleTextLayout.flexibleWidth = 1;

            // Close button
            if (showCloseButton)
            {
                GameObject closeObj = new GameObject("CloseButton");
                closeObj.transform.SetParent(titleBar.transform, false);

                RectTransform closeRect = closeObj.AddComponent<RectTransform>();

                LayoutElement closeLayout = closeObj.AddComponent<LayoutElement>();
                closeLayout.preferredWidth = 44;
                closeLayout.preferredHeight = 44;
                closeLayout.flexibleWidth = 0;

                Image closeBg = closeObj.AddComponent<Image>();
                closeBg.color = Win95Theme.ButtonFace;

                closeButton = closeObj.AddComponent<Button>();
                closeButton.onClick.AddListener(Close);

                Win95Button.Create(closeObj, "X", fontSize - 2);
            }
        }

        private void CreateContentArea()
        {
            contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(dialogRoot.transform, false);

            RectTransform contentRect = contentArea.AddComponent<RectTransform>();

            LayoutElement contentLayout = contentArea.AddComponent<LayoutElement>();
            contentLayout.flexibleHeight = 1;

            // Sunken panel for content
            Image contentBg = contentArea.AddComponent<Image>();
            contentBg.color = Win95Theme.WindowBackground;

            // Content padding
            VerticalLayoutGroup contentVlg = contentArea.AddComponent<VerticalLayoutGroup>();
            contentVlg.padding = new RectOffset((int)contentPadding, (int)contentPadding, (int)contentPadding, (int)contentPadding);
            contentVlg.spacing = 8;
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = true;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            contentVlg.childAlignment = TextAnchor.UpperLeft;

            ContentSizeFitter contentFitter = contentArea.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private void CreateButtonRow()
        {
            buttonRow = new GameObject("ButtonRow");
            buttonRow.transform.SetParent(dialogRoot.transform, false);

            RectTransform buttonRect = buttonRow.AddComponent<RectTransform>();

            LayoutElement buttonLayout = buttonRow.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = buttonRowHeight;
            buttonLayout.minHeight = buttonRowHeight;

            // Horizontal layout for buttons (right-aligned)
            HorizontalLayoutGroup buttonHlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            buttonHlg.padding = new RectOffset(8, 8, 8, 8);
            buttonHlg.spacing = 16;
            buttonHlg.childAlignment = TextAnchor.MiddleRight;
            buttonHlg.childControlWidth = false;
            buttonHlg.childControlHeight = true;
            buttonHlg.childForceExpandWidth = false;
            buttonHlg.childForceExpandHeight = true;
        }

        /// <summary>
        /// Show the dialog.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            OnDialogOpened?.Invoke();
        }

        /// <summary>
        /// Close the dialog.
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            OnCloseClicked?.Invoke();
            OnDialogClosed?.Invoke();
        }

        /// <summary>
        /// Add a button to the button row.
        /// </summary>
        public Button AddButton(string text, Action onClick, float width = 200f)
        {
            if (buttonRow == null) return null;

            GameObject btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(buttonRow.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(width, 64);

            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredWidth = width;
            btnLayout.preferredHeight = 64;

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            Win95Button win95Btn = Win95Button.Create(btnObj, text, fontSize);

            return btn;
        }

        /// <summary>
        /// Add a text label to the content area.
        /// </summary>
        public TMP_Text AddLabel(string text, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
        {
            if (contentArea == null) return null;

            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(contentArea.transform, false);

            TMP_Text label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = Win95Theme.WindowText;
            label.alignment = alignment;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.minHeight = fontSize + 16;

            return label;
        }

        /// <summary>
        /// Add a sunken text area to the content area.
        /// </summary>
        public GameObject AddTextArea(float height = 100f)
        {
            if (contentArea == null) return null;

            GameObject areaObj = new GameObject("TextArea");
            areaObj.transform.SetParent(contentArea.transform, false);

            RectTransform areaRect = areaObj.AddComponent<RectTransform>();

            LayoutElement areaLayout = areaObj.AddComponent<LayoutElement>();
            areaLayout.preferredHeight = height;
            areaLayout.flexibleHeight = 0;

            // White background
            Image areaBg = areaObj.AddComponent<Image>();
            areaBg.color = Color.white;

            // Add sunken border
            CreateSunkenBorder(areaObj);

            return areaObj;
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
        /// Create a Win95 modal dialog.
        /// </summary>
        public static Win95ModalDialog Create(Transform parent, string title, float width = 800f, float height = 600f)
        {
            GameObject dialogObj = new GameObject("Win95Dialog_" + title.Replace(" ", ""));
            dialogObj.transform.SetParent(parent, false);

            RectTransform dialogRect = dialogObj.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.offsetMin = Vector2.zero;
            dialogRect.offsetMax = Vector2.zero;

            Win95ModalDialog dialog = dialogObj.AddComponent<Win95ModalDialog>();
            dialog.dialogTitle = title;
            dialog.dialogWidth = width;
            dialog.dialogHeight = height;

            return dialog;
        }
    }
}
