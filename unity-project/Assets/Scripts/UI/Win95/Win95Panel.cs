using UnityEngine;
using UnityEngine.UI;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 style panel with raised or sunken appearance.
    /// </summary>
    public class Win95Panel : MonoBehaviour
    {
        public enum PanelStyle
        {
            Raised,     // Standard raised panel (like window background)
            Sunken,     // Sunken/inset panel (like text fields, progress bars)
            Flat        // No 3D effect, just background color
        }

        [Header("Panel Settings")]
        [SerializeField] private PanelStyle style = PanelStyle.Raised;
        [SerializeField] private int borderWidth = 4;
        [SerializeField] private Color backgroundColor = default;

        [Header("Sprites (Optional)")]
        [SerializeField] private Sprite panelSprite;

        private Image backgroundImage;

        // Border elements
        private Image topBorder1, topBorder2;
        private Image leftBorder1, leftBorder2;
        private Image bottomBorder1, bottomBorder2;
        private Image rightBorder1, rightBorder2;

        private bool isInitialized = false;

        private void Awake()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            // Set default background color if not set
            if (backgroundColor == default)
            {
                backgroundColor = Win95Theme.WindowBackground;
            }

            SetupPanel();
            isInitialized = true;
        }

        private void SetupPanel()
        {
            // Background
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            if (panelSprite != null)
            {
                backgroundImage.sprite = panelSprite;
                backgroundImage.type = Image.Type.Sliced;
            }

            backgroundImage.color = backgroundColor;

            // Create borders based on style
            if (style != PanelStyle.Flat)
            {
                CreateBorders();
                ApplyStyle();
            }
        }

        private void CreateBorders()
        {
            // Outer borders (2px each)
            topBorder1 = CreateBorderLine("TopBorder1", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, -2));
            leftBorder1 = CreateBorderLine("LeftBorder1", new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(2, 0));
            bottomBorder1 = CreateBorderLine("BottomBorder1", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 2));
            rightBorder1 = CreateBorderLine("RightBorder1", new Vector2(1, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-2, 0));

            if (borderWidth >= 4)
            {
                // Inner borders (2px each, inset by 2px)
                topBorder2 = CreateBorderLine("TopBorder2", new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, -2), new Vector2(-2, -4));
                leftBorder2 = CreateBorderLine("LeftBorder2", new Vector2(0, 0), new Vector2(0, 1), new Vector2(2, 2), new Vector2(4, -2));
                bottomBorder2 = CreateBorderLine("BottomBorder2", new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 2), new Vector2(-2, 4));
                rightBorder2 = CreateBorderLine("RightBorder2", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-2, 2), new Vector2(-4, -2));
            }
        }

        private Image CreateBorderLine(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject borderObj = new GameObject(name);
            borderObj.transform.SetParent(transform, false);
            borderObj.transform.SetAsFirstSibling(); // Borders behind content

            RectTransform rect = borderObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image img = borderObj.AddComponent<Image>();
            img.raycastTarget = false;

            return img;
        }

        private void ApplyStyle()
        {
            Color topLeftOuter, topLeftInner, bottomRightOuter, bottomRightInner;

            if (style == PanelStyle.Raised)
            {
                // Raised: light on top-left, dark on bottom-right
                topLeftOuter = Win95Theme.ButtonHighlight;
                topLeftInner = Win95Theme.ColorLightHighlight;
                bottomRightOuter = Win95Theme.ColorDark;
                bottomRightInner = Win95Theme.ButtonShadow;
            }
            else // Sunken
            {
                // Sunken: dark on top-left, light on bottom-right
                topLeftOuter = Win95Theme.ButtonShadow;
                topLeftInner = Win95Theme.ColorDark;
                bottomRightOuter = Win95Theme.ButtonHighlight;
                bottomRightInner = Win95Theme.ColorLightHighlight;
            }

            // Apply colors
            if (topBorder1 != null) topBorder1.color = topLeftOuter;
            if (leftBorder1 != null) leftBorder1.color = topLeftOuter;
            if (bottomBorder1 != null) bottomBorder1.color = bottomRightOuter;
            if (rightBorder1 != null) rightBorder1.color = bottomRightOuter;

            if (topBorder2 != null) topBorder2.color = topLeftInner;
            if (leftBorder2 != null) leftBorder2.color = topLeftInner;
            if (bottomBorder2 != null) bottomBorder2.color = bottomRightInner;
            if (rightBorder2 != null) rightBorder2.color = bottomRightInner;
        }

        /// <summary>
        /// Change the panel style at runtime.
        /// </summary>
        public void SetStyle(PanelStyle newStyle)
        {
            style = newStyle;

            if (style == PanelStyle.Flat)
            {
                // Remove borders
                DestroyBorders();
            }
            else
            {
                if (topBorder1 == null)
                {
                    CreateBorders();
                }
                ApplyStyle();
            }
        }

        private void DestroyBorders()
        {
            if (topBorder1 != null) Destroy(topBorder1.gameObject);
            if (leftBorder1 != null) Destroy(leftBorder1.gameObject);
            if (bottomBorder1 != null) Destroy(bottomBorder1.gameObject);
            if (rightBorder1 != null) Destroy(rightBorder1.gameObject);
            if (topBorder2 != null) Destroy(topBorder2.gameObject);
            if (leftBorder2 != null) Destroy(leftBorder2.gameObject);
            if (bottomBorder2 != null) Destroy(bottomBorder2.gameObject);
            if (rightBorder2 != null) Destroy(rightBorder2.gameObject);

            topBorder1 = leftBorder1 = bottomBorder1 = rightBorder1 = null;
            topBorder2 = leftBorder2 = bottomBorder2 = rightBorder2 = null;
        }

        /// <summary>
        /// Set the background color.
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }

        /// <summary>
        /// Create a Win95Panel on the given GameObject.
        /// </summary>
        public static Win95Panel Create(GameObject target, PanelStyle style = PanelStyle.Raised)
        {
            Win95Panel panel = target.GetComponent<Win95Panel>();
            if (panel == null)
            {
                panel = target.AddComponent<Win95Panel>();
            }

            panel.style = style;
            panel.Initialize();

            return panel;
        }

        /// <summary>
        /// Create a new GameObject with Win95Panel attached.
        /// </summary>
        public static Win95Panel CreateNew(string name, Transform parent, PanelStyle style = PanelStyle.Raised)
        {
            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(parent, false);

            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Win95Panel panel = panelObj.AddComponent<Win95Panel>();
            panel.style = style;

            return panel;
        }
    }
}
