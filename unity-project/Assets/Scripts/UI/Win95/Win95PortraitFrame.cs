using UnityEngine;
using UnityEngine.UI;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled portrait frame.
    /// Adds a Win95 raised panel border around character portraits.
    /// Can be added to existing portrait frame GameObjects to apply Win95 styling.
    /// </summary>
    public class Win95PortraitFrame : MonoBehaviour
    {
        [Header("Style")]
        [SerializeField] private PanelStyle style = PanelStyle.Raised;
        [SerializeField] private int borderWidth = 2;

        public enum PanelStyle
        {
            Raised,
            Sunken
        }

        // Border elements
        private Image topBorder1, topBorder2;
        private Image leftBorder1, leftBorder2;
        private Image bottomBorder1, bottomBorder2;
        private Image rightBorder1, rightBorder2;
        private Image backgroundImage;

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

            // Get or set background color
            backgroundImage = GetComponent<Image>();
            if (backgroundImage != null)
            {
                backgroundImage.color = Win95Theme.WindowBackground;
            }

            // Create borders
            CreateBorders();
            ApplyStyle();

            isInitialized = true;
        }

        private void CreateBorders()
        {
            // Outer borders
            topBorder1 = CreateBorderLine("TopBorder1", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, -1));
            leftBorder1 = CreateBorderLine("LeftBorder1", new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(1, 0));
            bottomBorder1 = CreateBorderLine("BottomBorder1", new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, 1));
            rightBorder1 = CreateBorderLine("RightBorder1", new Vector2(1, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-1, 0));

            if (borderWidth >= 2)
            {
                // Inner borders
                topBorder2 = CreateBorderLine("TopBorder2", new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, -2));
                leftBorder2 = CreateBorderLine("LeftBorder2", new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, -1));
                bottomBorder2 = CreateBorderLine("BottomBorder2", new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(-1, 2));
                rightBorder2 = CreateBorderLine("RightBorder2", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-2, -1));
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
                topLeftOuter = Win95Theme.ButtonHighlight;
                topLeftInner = Win95Theme.ColorLightHighlight;
                bottomRightOuter = Win95Theme.ColorDark;
                bottomRightInner = Win95Theme.ButtonShadow;
            }
            else // Sunken
            {
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
        /// Set the panel style.
        /// </summary>
        public void SetStyle(PanelStyle newStyle)
        {
            style = newStyle;
            ApplyStyle();
        }

        /// <summary>
        /// Apply Win95 portrait frame styling to an existing portrait GameObject.
        /// </summary>
        public static Win95PortraitFrame ApplyTo(GameObject portraitFrame, PanelStyle style = PanelStyle.Raised)
        {
            if (portraitFrame == null) return null;

            Win95PortraitFrame frame = portraitFrame.GetComponent<Win95PortraitFrame>();
            if (frame == null)
            {
                frame = portraitFrame.AddComponent<Win95PortraitFrame>();
            }

            frame.style = style;
            frame.Initialize();

            return frame;
        }

        /// <summary>
        /// Find and style all portrait frames in the scene.
        /// </summary>
        public static void StyleAllPortraitFrames()
        {
            // Find portrait frames by name pattern
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int styledCount = 0;

            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("CharacterPortraitFrame") || obj.name.Contains("PortraitFrame"))
                {
                    ApplyTo(obj, PanelStyle.Raised);
                    styledCount++;
                    Debug.Log($"Win95PortraitFrame: Applied styling to '{obj.name}'");
                }
            }

            Debug.Log($"Win95PortraitFrame: Styled {styledCount} portrait frame(s).");
        }
    }
}
