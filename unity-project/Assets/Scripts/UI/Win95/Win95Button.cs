using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 style button with 3D raised appearance.
    /// Automatically handles normal, hover, pressed, and disabled states.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class Win95Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Button Settings")]
        [SerializeField] private string buttonText = "Button";
        [SerializeField] private int fontSize = 36;
        [SerializeField] private bool useUnderline = false;
        [SerializeField] private int underlineIndex = 0; // Which character to underline (keyboard shortcut)

        [Header("Sprites (Optional - uses colors if null)")]
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField] private Sprite disabledSprite;

        private Button button;
        private Image backgroundImage;
        private TMP_Text label;

        // Border elements for 3D effect
        private Image topHighlight;
        private Image leftHighlight;
        private Image bottomShadow;
        private Image rightShadow;
        private Image topInnerHighlight;
        private Image leftInnerHighlight;
        private Image bottomInnerShadow;
        private Image rightInnerShadow;

        private bool isPressed = false;
        private bool isHovered = false;

        private void Awake()
        {
            button = GetComponent<Button>();

            SetupButton();
            UpdateVisualState();
        }

        private void SetupButton()
        {
            // Background
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            backgroundImage.color = Win95Theme.ButtonFace;

            // Disable Unity's default button transition
            button.transition = Selectable.Transition.None;

            // Create 3D border effect
            CreateBorders();

            // Create text label
            CreateLabel();
        }

        private void CreateBorders()
        {
            // Outer borders (2px each)
            topHighlight = CreateBorderImage("TopHighlight", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, -2));
            leftHighlight = CreateBorderImage("LeftHighlight", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(2, 0));
            bottomShadow = CreateBorderImage("BottomShadow", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 2));
            rightShadow = CreateBorderImage("RightShadow", new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-2, 0));

            // Inner borders (2px, inset by 2)
            topInnerHighlight = CreateBorderImage("TopInnerHighlight", new Vector2(0, 1), new Vector2(1, 1), new Vector2(2, -2), new Vector2(-2, -4));
            leftInnerHighlight = CreateBorderImage("LeftInnerHighlight", new Vector2(0, 0), new Vector2(0, 1), new Vector2(2, 2), new Vector2(4, -2));
            bottomInnerShadow = CreateBorderImage("BottomInnerShadow", new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 2), new Vector2(-2, 4));
            rightInnerShadow = CreateBorderImage("RightInnerShadow", new Vector2(1, 0), new Vector2(1, 1), new Vector2(-2, 2), new Vector2(-4, -2));
        }

        private Image CreateBorderImage(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject borderObj = new GameObject(name);
            borderObj.transform.SetParent(transform, false);

            RectTransform rect = borderObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image img = borderObj.AddComponent<Image>();
            img.raycastTarget = false;

            return img;
        }

        private void CreateLabel()
        {
            // Check for existing label
            label = GetComponentInChildren<TMP_Text>();

            if (label == null)
            {
                GameObject textObj = new GameObject("Label");
                textObj.transform.SetParent(transform, false);

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8, 4);
                textRect.offsetMax = new Vector2(-8, -4);

                label = textObj.AddComponent<TextMeshProUGUI>();
            }

            label.fontSize = fontSize;
            label.color = Win95Theme.WindowText;
            label.alignment = TextAlignmentOptions.Center;

            UpdateLabelText();
        }

        private void UpdateLabelText()
        {
            if (label == null) return;

            if (useUnderline && underlineIndex >= 0 && underlineIndex < buttonText.Length)
            {
                // Insert underline tags around the specified character
                string before = buttonText.Substring(0, underlineIndex);
                string underlined = buttonText.Substring(underlineIndex, 1);
                string after = buttonText.Substring(underlineIndex + 1);
                label.text = $"{before}<u>{underlined}</u>{after}";
            }
            else
            {
                label.text = buttonText;
            }
        }

        private void UpdateVisualState()
        {
            if (!button.interactable)
            {
                // Disabled state
                SetBorderColors(Win95Theme.ColorLightGray, Win95Theme.ColorLightGray,
                    Win95Theme.ColorMidGray, Win95Theme.ColorMidGray);
                if (label != null) label.color = Win95Theme.ColorMidGray;

                if (disabledSprite != null)
                {
                    backgroundImage.sprite = disabledSprite;
                    backgroundImage.type = Image.Type.Sliced;
                }
            }
            else if (isPressed)
            {
                // Pressed state - invert borders (sunken look)
                SetBorderColors(Win95Theme.ColorDark, Win95Theme.ButtonShadow,
                    Win95Theme.ButtonHighlight, Win95Theme.ColorLightHighlight);
                if (label != null) label.color = Win95Theme.WindowText;

                if (pressedSprite != null)
                {
                    backgroundImage.sprite = pressedSprite;
                    backgroundImage.type = Image.Type.Sliced;
                }
            }
            else
            {
                // Normal/Hover state - raised look
                SetBorderColors(Win95Theme.ButtonHighlight, Win95Theme.ColorLightHighlight,
                    Win95Theme.ColorDark, Win95Theme.ButtonShadow);
                if (label != null) label.color = Win95Theme.WindowText;

                if (normalSprite != null)
                {
                    backgroundImage.sprite = normalSprite;
                    backgroundImage.type = Image.Type.Sliced;
                }
            }
        }

        private void SetBorderColors(Color topLeft, Color topLeftInner, Color bottomRight, Color bottomRightInner)
        {
            if (topHighlight != null) topHighlight.color = topLeft;
            if (leftHighlight != null) leftHighlight.color = topLeft;
            if (topInnerHighlight != null) topInnerHighlight.color = topLeftInner;
            if (leftInnerHighlight != null) leftInnerHighlight.color = topLeftInner;

            if (bottomShadow != null) bottomShadow.color = bottomRight;
            if (rightShadow != null) rightShadow.color = bottomRight;
            if (bottomInnerShadow != null) bottomInnerShadow.color = bottomRightInner;
            if (rightInnerShadow != null) rightInnerShadow.color = bottomRightInner;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            // Win95 buttons don't change on hover, but we track it
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            isPressed = false;
            UpdateVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button.interactable)
            {
                isPressed = true;
                UpdateVisualState();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            UpdateVisualState();
        }

        /// <summary>
        /// Set the button text.
        /// </summary>
        public void SetText(string text)
        {
            buttonText = text;
            UpdateLabelText();
        }

        /// <summary>
        /// Set keyboard shortcut underline.
        /// </summary>
        public void SetUnderline(bool enable, int characterIndex = 0)
        {
            useUnderline = enable;
            underlineIndex = characterIndex;
            UpdateLabelText();
        }

        /// <summary>
        /// Create a Win95Button on the given GameObject.
        /// </summary>
        public static Win95Button Create(GameObject target, string text, int fontSize = 36)
        {
            Button btn = target.GetComponent<Button>();
            if (btn == null)
            {
                btn = target.AddComponent<Button>();
            }

            Win95Button win95Btn = target.GetComponent<Win95Button>();
            if (win95Btn == null)
            {
                win95Btn = target.AddComponent<Win95Button>();
            }

            win95Btn.buttonText = text;
            win95Btn.fontSize = fontSize;

            return win95Btn;
        }
    }
}
