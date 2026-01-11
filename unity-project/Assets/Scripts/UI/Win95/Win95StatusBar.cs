using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 style status bar showing game information.
    /// Displays: Version | Run/Day | Credits
    /// </summary>
    public class Win95StatusBar : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private string versionPrefix = "Mental Break alpha";
        [SerializeField] private int fontSize = 22;  // 2x scale from 11px

        [Header("Sprites")]
        [SerializeField] private Sprite panelInsetSprite;

        [Header("References")]
        [SerializeField] private DialogueRunner dialogueRunner;

        // UI Elements
        private TMP_Text versionText;
        private TMP_Text runDayText;
        private TMP_Text creditsText;

        private RectTransform versionPanel;
        private RectTransform runDayPanel;
        private RectTransform creditsPanel;

        private float updateInterval = 0.5f;
        private float lastUpdateTime;

        private void Awake()
        {
            CreateStatusBar();
        }

        private void Start()
        {
            // Find DialogueRunner if not assigned
            if (dialogueRunner == null)
            {
                dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            }

            UpdateDisplays();
        }

        private void Update()
        {
            // Periodic update
            if (Time.time - lastUpdateTime > updateInterval)
            {
                UpdateDisplays();
                lastUpdateTime = Time.time;
            }
        }

        private void CreateStatusBar()
        {
            // Add horizontal layout
            HorizontalLayoutGroup layout = GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layout.padding = new RectOffset(4, 4, 4, 2);
            layout.spacing = 4;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Create three sunken panels
            versionPanel = CreateStatusPanel("VersionPanel", 0.4f);
            runDayPanel = CreateStatusPanel("RunDayPanel", 0.3f);
            creditsPanel = CreateStatusPanel("CreditsPanel", 0.3f);

            // Create text in each panel
            versionText = CreateStatusText(versionPanel, "VersionText");
            runDayText = CreateStatusText(runDayPanel, "RunDayText");
            creditsText = CreateStatusText(creditsPanel, "CreditsText");

            // Add top border for status bar
            CreateTopBorder();
        }

        private RectTransform CreateStatusPanel(string name, float flexibleWidth)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(transform, false);

            RectTransform rect = panelObj.AddComponent<RectTransform>();

            // Sunken panel background
            Image bgImage = panelObj.AddComponent<Image>();
            if (panelInsetSprite != null)
            {
                bgImage.sprite = panelInsetSprite;
                bgImage.type = Image.Type.Sliced;
            }
            bgImage.color = Win95Theme.WindowBackground;

            // Add sunken border effect
            CreateSunkenBorder(panelObj);

            // Layout element for flexible sizing
            LayoutElement layoutElement = panelObj.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = flexibleWidth;
            layoutElement.minWidth = 160;

            return rect;
        }

        private void CreateSunkenBorder(GameObject parent)
        {
            // Top shadow - 2px
            CreateBorderLine(parent, "TopShadow", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -2), Win95Theme.ButtonShadow);

            // Left shadow - 2px
            CreateBorderLine(parent, "LeftShadow", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(2, 0), Win95Theme.ButtonShadow);

            // Bottom highlight - 2px
            CreateBorderLine(parent, "BottomHighlight", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 2), Win95Theme.ButtonHighlight);

            // Right highlight - 2px
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

        private TMP_Text CreateStatusText(RectTransform parent, string name)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8, 0);
            textRect.offsetMax = new Vector2(-8, 0);

            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = Win95Theme.WindowText;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.overflowMode = TextOverflowModes.Ellipsis;

            return text;
        }

        private void CreateTopBorder()
        {
            // Highlight line at top of status bar area
            GameObject highlightLine = new GameObject("TopHighlight");
            highlightLine.transform.SetParent(transform.parent, false);
            highlightLine.transform.SetAsFirstSibling();

            RectTransform highlightRect = highlightLine.AddComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 1);
            highlightRect.anchorMax = new Vector2(1, 1);
            highlightRect.pivot = new Vector2(0.5f, 0);
            highlightRect.anchoredPosition = Vector2.zero;
            highlightRect.sizeDelta = new Vector2(0, 2);

            Image highlightImage = highlightLine.AddComponent<Image>();
            highlightImage.color = Win95Theme.ButtonHighlight;
            highlightImage.raycastTarget = false;
        }

        private void UpdateDisplays()
        {
            UpdateVersionDisplay();
            UpdateRunDayDisplay();
            UpdateCreditsDisplay();
        }

        private void UpdateVersionDisplay()
        {
            if (versionText == null) return;

            // Try to get version from GameManager or use default
            string version = "v4.0.0"; // Default

            // Try to read from Application version
            if (!string.IsNullOrEmpty(Application.version))
            {
                version = "v" + Application.version;
            }

            versionText.text = $"{versionPrefix} {version}";
        }

        private void UpdateRunDayDisplay()
        {
            if (runDayText == null) return;

            int currentRun = 1;
            int currentDay = 1;

            // Try to get from Yarn variables
            if (dialogueRunner != null && dialogueRunner.VariableStorage != null)
            {
                var storage = dialogueRunner.VariableStorage;

                if (storage.TryGetValue("$current_run", out float runValue))
                {
                    currentRun = Mathf.RoundToInt(runValue);
                }

                if (storage.TryGetValue("$current_day", out float dayValue))
                {
                    currentDay = Mathf.RoundToInt(dayValue);
                }
            }

            runDayText.text = $"Run {currentRun} Day {currentDay}";
        }

        private void UpdateCreditsDisplay()
        {
            if (creditsText == null) return;

            int credits = 0;

            // Try to get from Yarn variables
            if (dialogueRunner != null && dialogueRunner.VariableStorage != null)
            {
                var storage = dialogueRunner.VariableStorage;

                if (storage.TryGetValue("$credits", out float creditsValue))
                {
                    credits = Mathf.RoundToInt(creditsValue);
                }
            }

            creditsText.text = $"Credits: {credits}";
        }

        /// <summary>
        /// Force an immediate update of all displays.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateDisplays();
        }

        /// <summary>
        /// Set a custom version string.
        /// </summary>
        public void SetVersion(string version)
        {
            if (versionText != null)
            {
                versionText.text = $"{versionPrefix} {version}";
            }
        }

        /// <summary>
        /// Set run and day directly (bypasses Yarn variable lookup).
        /// </summary>
        public void SetRunDay(int run, int day)
        {
            if (runDayText != null)
            {
                runDayText.text = $"Run {run} Day {day}";
            }
        }

        /// <summary>
        /// Set credits directly (bypasses Yarn variable lookup).
        /// </summary>
        public void SetCredits(int credits)
        {
            if (creditsText != null)
            {
                creditsText.text = $"Credits: {credits}";
            }
        }
    }
}
