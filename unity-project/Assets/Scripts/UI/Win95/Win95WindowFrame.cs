using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Main Windows 95 window frame that wraps the entire game UI.
    /// Creates the window chrome including title bar, menu bar, content area, and status bar.
    /// </summary>
    public class Win95WindowFrame : MonoBehaviour
    {
        [Header("Window Settings")]
        [SerializeField] private string windowTitle = "Bigger Tech Corp. Employee Onboarding System";
        [SerializeField] private bool autoReparentSiblings = true;

        [Header("Sprites")]
        [SerializeField] private Sprite panelRaisedSprite;
        [SerializeField] private Sprite titleBarActiveSprite;
        [SerializeField] private Sprite titleBarInactiveSprite;
        [SerializeField] private Sprite windowIconSprite;

        [Header("References (Auto-created if null)")]
        [SerializeField] private RectTransform titleBar;
        [SerializeField] private RectTransform menuBar;
        [SerializeField] private RectTransform contentArea;
        [SerializeField] private RectTransform statusBar;

        [Header("Runtime References")]
        public TMP_Text TitleText { get; private set; }
        public Win95MenuBar MenuBarComponent { get; private set; }
        public Win95StatusBar StatusBarComponent { get; private set; }

        // Events
        public event Action OnMinimizeClicked;
        public event Action OnMaximizeClicked;
        public event Action OnCloseClicked;

        private Image frameBackground;
        private Image titleBarImage;
        private Canvas chromeCanvas;
        private RectTransform chromeContainer;
        [SerializeField] private bool isInitialized = false;

        private void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        private void Start()
        {
            if (autoReparentSiblings)
            {
                ReparentSiblingsToContentArea();
            }
        }

        /// <summary>
        /// Moves all sibling UI elements into the ContentArea.
        /// This allows the Win95 frame to wrap around all game UI.
        /// </summary>
        public void ReparentSiblingsToContentArea()
        {
            if (contentArea == null)
            {
                Debug.LogError("Win95WindowFrame: Cannot reparent - ContentArea is null");
                return;
            }

            Transform parentTransform = transform.parent;
            if (parentTransform == null)
            {
                Debug.LogError("Win95WindowFrame: Cannot reparent - no parent transform");
                return;
            }

            // Collect siblings to reparent (must collect first, then reparent to avoid modifying collection while iterating)
            var siblingsToReparent = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                Transform sibling = parentTransform.GetChild(i);
                if (sibling != transform) // Don't reparent ourselves
                {
                    siblingsToReparent.Add(sibling);
                }
            }

            if (siblingsToReparent.Count == 0)
            {
                Debug.Log("Win95WindowFrame: No siblings to reparent");
                return;
            }

            // Reparent all siblings into ContentArea, preserving their anchoredPosition
            foreach (Transform sibling in siblingsToReparent)
            {
                RectTransform rect = sibling.GetComponent<RectTransform>();
                Vector2 savedAnchoredPosition = rect != null ? rect.anchoredPosition : Vector2.zero;

                sibling.SetParent(contentArea, false); // worldPositionStays = false

                // Restore the anchoredPosition so elements maintain their intended position
                if (rect != null)
                {
                    rect.anchoredPosition = savedAnchoredPosition;
                }
            }

            // Make sure Win95WindowFrame is at the back (renders first as background)
            transform.SetAsFirstSibling();

            Debug.Log($"Win95WindowFrame: Reparented {siblingsToReparent.Count} siblings into ContentArea");
        }

        public void Initialize()
        {
            if (isInitialized) return;

            CreateWindowFrame();
            isInitialized = true;
        }

        private void CreateWindowFrame()
        {
            // Main frame background (raised panel)
            frameBackground = GetComponent<Image>();
            if (frameBackground == null)
            {
                frameBackground = gameObject.AddComponent<Image>();
            }

            frameBackground.sprite = panelRaisedSprite;
            frameBackground.type = Image.Type.Sliced;
            frameBackground.color = Win95Theme.WindowBackground;

            // Create chrome canvas with high sorting order (always on top)
            CreateChromeCanvas();

            // Create content area (stays on main canvas, renders behind chrome)
            CreateContentArea();

            // Create chrome elements ON the chrome canvas (always on top)
            CreateTitleBar();
            CreateMenuBar();
            CreateStatusBar();
        }

        private void CreateChromeCanvas()
        {
            // Check if ChromeCanvas already exists
            Transform existingChrome = transform.Find("ChromeCanvas");
            if (existingChrome != null)
            {
                chromeContainer = existingChrome.GetComponent<RectTransform>();
                chromeCanvas = existingChrome.GetComponent<Canvas>();
                return;
            }

            // Create container for all chrome elements
            GameObject chromeObj = new GameObject("ChromeCanvas");
            chromeObj.transform.SetParent(transform, false);

            chromeContainer = chromeObj.AddComponent<RectTransform>();
            chromeContainer.anchorMin = Vector2.zero;
            chromeContainer.anchorMax = Vector2.one;
            chromeContainer.offsetMin = Vector2.zero;
            chromeContainer.offsetMax = Vector2.zero;

            // Add nested Canvas with high sorting order so chrome is always on top
            chromeCanvas = chromeObj.AddComponent<Canvas>();
            chromeCanvas.overrideSorting = true;
            chromeCanvas.sortingOrder = 10000;  // Always on top of everything

            // Required for nested Canvas to receive raycasts (for button clicks)
            chromeObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateTitleBar()
        {
            if (titleBar == null)
            {
                GameObject titleBarObj = new GameObject("TitleBar");
                titleBarObj.transform.SetParent(chromeContainer, false);  // Parent to chrome canvas
                titleBar = titleBarObj.AddComponent<RectTransform>();
            }

            // Position at top
            titleBar.anchorMin = new Vector2(0, 1);
            titleBar.anchorMax = new Vector2(1, 1);
            titleBar.pivot = new Vector2(0.5f, 1);
            titleBar.anchoredPosition = new Vector2(0, -Win95Theme.BorderWidth);
            titleBar.sizeDelta = new Vector2(-Win95Theme.BorderWidth * 2, Win95Theme.TitleBarHeight);

            // Title bar background
            titleBarImage = titleBar.GetComponent<Image>();
            if (titleBarImage == null)
            {
                titleBarImage = titleBar.gameObject.AddComponent<Image>();
            }
            titleBarImage.sprite = titleBarActiveSprite;
            titleBarImage.type = Image.Type.Sliced;
            titleBarImage.color = Win95Theme.TitleBarActive;

            // Add horizontal layout (check if exists first to avoid duplicate component error)
            HorizontalLayoutGroup layout = titleBar.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = titleBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            }
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Window icon (check if exists)
            if (windowIconSprite != null)
            {
                Transform existingIcon = titleBar.Find("WindowIcon");
                if (existingIcon == null)
                {
                    GameObject iconObj = new GameObject("WindowIcon");
                    iconObj.transform.SetParent(titleBar, false);
                    Image iconImage = iconObj.AddComponent<Image>();
                    iconImage.sprite = windowIconSprite;
                    iconImage.preserveAspect = true;

                    LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
                    iconLayout.preferredWidth = 40;
                    iconLayout.preferredHeight = 40;
                }
            }

            // Title text (check if exists)
            Transform existingTitleText = titleBar.Find("TitleText");
            if (existingTitleText == null)
            {
                GameObject titleTextObj = new GameObject("TitleText");
                titleTextObj.transform.SetParent(titleBar, false);
                TitleText = titleTextObj.AddComponent<TextMeshProUGUI>();
                TitleText.text = windowTitle;
                TitleText.color = Win95Theme.TitleBarText;
                TitleText.fontSize = 36;
                TitleText.fontStyle = FontStyles.Bold;
                TitleText.alignment = TextAlignmentOptions.MidlineLeft;

                LayoutElement textLayout = titleTextObj.AddComponent<LayoutElement>();
                textLayout.flexibleWidth = 1;
            }
            else
            {
                TitleText = existingTitleText.GetComponent<TextMeshProUGUI>();
            }

            // Window buttons (destroy and recreate to ensure correct positioning)
            Transform existingButtons = titleBar.Find("WindowButtons");
            if (existingButtons != null)
            {
                DestroyImmediate(existingButtons.gameObject);
            }
            CreateWindowButtons();
        }

        private void CreateWindowButtons()
        {
            // Load the buttons sprite strip from Resources
            Sprite buttonsSprite = Resources.Load<Sprite>("Graphics/UI/Win95/win95_buttons");

            // Button container - positioned absolutely at right edge
            GameObject buttonContainer = new GameObject("WindowButtons");
            buttonContainer.transform.SetParent(titleBar, false);

            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1, 0.5f);
            containerRect.anchorMax = new Vector2(1, 0.5f);
            containerRect.pivot = new Vector2(1, 0.5f);
            containerRect.anchoredPosition = new Vector2(-4, 0);

            // Ignore parent layout - this container is positioned absolutely
            LayoutElement containerLayout = buttonContainer.AddComponent<LayoutElement>();
            containerLayout.ignoreLayout = true;

            // Add the buttons sprite image
            Image buttonsImage = buttonContainer.AddComponent<Image>();
            buttonsImage.sprite = buttonsSprite;
            buttonsImage.preserveAspect = true;
            buttonsImage.raycastTarget = false;

            // Fixed size for the buttons sprite (scaled to fit title bar, 2x scale)
            containerRect.sizeDelta = new Vector2(91.8f, 36f);
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

        private void CreateMenuBar()
        {
            if (menuBar == null)
            {
                GameObject menuBarObj = new GameObject("MenuBar");
                menuBarObj.transform.SetParent(chromeContainer, false);  // Parent to chrome canvas
                menuBar = menuBarObj.AddComponent<RectTransform>();
            }

            // Position below title bar
            float topOffset = Win95Theme.BorderWidth + Win95Theme.TitleBarHeight;
            menuBar.anchorMin = new Vector2(0, 1);
            menuBar.anchorMax = new Vector2(1, 1);
            menuBar.pivot = new Vector2(0.5f, 1);
            menuBar.anchoredPosition = new Vector2(0, -topOffset);
            menuBar.sizeDelta = new Vector2(-Win95Theme.BorderWidth * 2, Win95Theme.MenuBarHeight);

            // Menu bar background
            Image menuBgImage = menuBar.GetComponent<Image>();
            if (menuBgImage == null)
            {
                menuBgImage = menuBar.gameObject.AddComponent<Image>();
            }
            menuBgImage.color = Win95Theme.WindowBackground;

            // Add menu bar component
            MenuBarComponent = menuBar.gameObject.GetComponent<Win95MenuBar>();
            if (MenuBarComponent == null)
            {
                MenuBarComponent = menuBar.gameObject.AddComponent<Win95MenuBar>();
            }
        }

        private void CreateContentArea()
        {
            if (contentArea == null)
            {
                GameObject contentObj = new GameObject("ContentArea");
                contentObj.transform.SetParent(transform, false);
                contentArea = contentObj.AddComponent<RectTransform>();
            }

            // Position between menu bar and status bar
            float topOffset = Win95Theme.BorderWidth + Win95Theme.TitleBarHeight + Win95Theme.MenuBarHeight;
            float bottomOffset = 34;

            contentArea.anchorMin = new Vector2(0, 0);
            contentArea.anchorMax = new Vector2(1, 1);
            contentArea.offsetMin = new Vector2(Win95Theme.BorderWidth, bottomOffset);
            contentArea.offsetMax = new Vector2(-Win95Theme.BorderWidth, -topOffset);

            // Ensure ContentArea renders behind TitleBar/MenuBar/StatusBar (sibling index 0)
            contentArea.SetAsFirstSibling();

            // Add RectMask2D to clip children to ContentArea bounds
            if (contentArea.GetComponent<RectMask2D>() == null)
            {
                contentArea.gameObject.AddComponent<RectMask2D>();
            }

            // Add 2px sunken border around ContentArea
            CreateContentAreaSunkenBorder();
        }

        /// <summary>
        /// Creates a 2-pixel sunken border around the ContentArea.
        /// Figma CSS: box-shadow: inset -1px -1px 0px #FFFFFF, inset 1px 1px 0px #808080,
        ///            inset -2px -2px 0px #C1C1C1, inset 2px 2px 0px #000000;
        /// Top/Left: Black outer, Gray inner | Bottom/Right: Light gray outer, White inner
        /// </summary>
        private void CreateContentAreaSunkenBorder()
        {
            // The border is created on the ChromeCanvas so it renders on top of content
            GameObject borderContainer = new GameObject("ContentAreaBorder");
            borderContainer.transform.SetParent(chromeContainer, false);

            RectTransform borderRect = borderContainer.AddComponent<RectTransform>();
            // Match ContentArea position
            float topOffset = Win95Theme.BorderWidth + Win95Theme.TitleBarHeight + Win95Theme.MenuBarHeight;
            float bottomOffset = 34;
            borderRect.anchorMin = new Vector2(0, 0);
            borderRect.anchorMax = new Vector2(1, 1);
            borderRect.offsetMin = new Vector2(Win95Theme.BorderWidth, bottomOffset);
            borderRect.offsetMax = new Vector2(-Win95Theme.BorderWidth, -topOffset);

            // === TOP BORDER (dark) ===
            // Outer (gray) - 2px
            CreateSunkenBorderLine(borderContainer, "TopOuter",
                new Vector2(0, 1), new Vector2(1, 1),  // anchors: full width at top
                new Vector2(0, -2), new Vector2(0, 0),  // offset: 2px height at top
                Win95Theme.SunkenBorderDarkInner);

            // Inner (black) - 2px below outer
            CreateSunkenBorderLine(borderContainer, "TopInner",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(2, -4), new Vector2(-2, -2),  // inset 2px from sides, 2px below top
                Win95Theme.SunkenBorderDarkOuter);

            // === LEFT BORDER (dark) ===
            // Outer (gray) - 2px
            CreateSunkenBorderLine(borderContainer, "LeftOuter",
                new Vector2(0, 0), new Vector2(0, 1),  // anchors: full height at left
                new Vector2(0, 0), new Vector2(2, 0),  // offset: 2px width at left
                Win95Theme.SunkenBorderDarkInner);

            // Inner (black) - 2px right of outer
            CreateSunkenBorderLine(borderContainer, "LeftInner",
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(2, 2), new Vector2(4, -2),  // inset 2px from top/bottom, 2px from left
                Win95Theme.SunkenBorderDarkOuter);

            // === BOTTOM BORDER (light) ===
            // Outer (white) - 2px
            CreateSunkenBorderLine(borderContainer, "BottomOuter",
                new Vector2(0, 0), new Vector2(1, 0),  // anchors: full width at bottom
                new Vector2(0, 0), new Vector2(0, 2),  // offset: 2px height at bottom
                Win95Theme.SunkenBorderLightInner);

            // Inner (light gray) - 2px above outer
            CreateSunkenBorderLine(borderContainer, "BottomInner",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(2, 2), new Vector2(-2, 4),  // inset 2px from sides, 2px above bottom
                Win95Theme.SunkenBorderLightOuter);

            // === RIGHT BORDER (light) ===
            // Outer (white) - 2px
            CreateSunkenBorderLine(borderContainer, "RightOuter",
                new Vector2(1, 0), new Vector2(1, 1),  // anchors: full height at right
                new Vector2(-2, 0), new Vector2(0, 0),  // offset: 2px width at right
                Win95Theme.SunkenBorderLightInner);

            // Inner (light gray) - 2px left of outer
            CreateSunkenBorderLine(borderContainer, "RightInner",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-4, 2), new Vector2(-2, -2),  // inset 2px from top/bottom, 2px from right
                Win95Theme.SunkenBorderLightOuter);
        }

        private void CreateSunkenBorderLine(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
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

        private void CreateStatusBar()
        {
            if (statusBar == null)
            {
                GameObject statusBarObj = new GameObject("StatusBar");
                statusBarObj.transform.SetParent(chromeContainer, false);  // Parent to chrome canvas
                statusBar = statusBarObj.AddComponent<RectTransform>();
            }

            // Position at bottom
            statusBar.anchorMin = new Vector2(0, 0);
            statusBar.anchorMax = new Vector2(1, 0);
            statusBar.pivot = new Vector2(0.5f, 0);
            statusBar.anchoredPosition = new Vector2(0, 2);
            statusBar.sizeDelta = new Vector2(-Win95Theme.BorderWidth * 2, Win95Theme.StatusBarHeight);

            // Status bar background
            Image statusBgImage = statusBar.GetComponent<Image>();
            if (statusBgImage == null)
            {
                statusBgImage = statusBar.gameObject.AddComponent<Image>();
            }
            statusBgImage.color = Win95Theme.WindowBackground;

            // Add status bar component
            StatusBarComponent = statusBar.gameObject.GetComponent<Win95StatusBar>();
            if (StatusBarComponent == null)
            {
                StatusBarComponent = statusBar.gameObject.AddComponent<Win95StatusBar>();
            }
        }

        /// <summary>
        /// Get the content area RectTransform where game UI should be placed.
        /// </summary>
        public RectTransform GetContentArea()
        {
            return contentArea;
        }

        /// <summary>
        /// Set the window title.
        /// </summary>
        public void SetTitle(string title)
        {
            windowTitle = title;
            if (TitleText != null)
            {
                TitleText.text = title;
            }
        }

        /// <summary>
        /// Set window active/inactive state (changes title bar color).
        /// </summary>
        public void SetActive(bool active)
        {
            if (titleBarImage != null)
            {
                titleBarImage.color = active ? Win95Theme.TitleBarActive : Win95Theme.TitleBarInactive;
            }
        }
    }
}
