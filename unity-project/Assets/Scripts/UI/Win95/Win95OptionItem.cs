using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Yarn.Unity;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled dialogue option button.
    /// Replaces the dialogue wheel option view with a proper Win95 button.
    /// </summary>
    public class Win95OptionItem : Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("References")]
        [SerializeField] private TMP_Text optionText;

        [Header("Settings")]
        [SerializeField] private int fontSize = 36;
        [SerializeField] private bool disabledStrikeThrough = true;

        // Win95 button visual elements
        private Image backgroundImage;
        private Image topHighlight;
        private Image leftHighlight;
        private Image bottomShadow;
        private Image rightShadow;
        private Image topInnerHighlight;
        private Image leftInnerHighlight;
        private Image bottomInnerShadow;
        private Image rightInnerShadow;

        public YarnTaskCompletionSource<DialogueOption> OnOptionSelected;
        public System.Threading.CancellationToken completionToken;

        private bool hasSubmittedOptionSelection = false;
        private DialogueOption _option;
        private bool isPressed = false;

        public DialogueOption Option
        {
            get => _option;
            set
            {
                _option = value;
                hasSubmittedOptionSelection = false;

                if (optionText == null) return;

                string line = value.Line.TextWithoutCharacterName.Text;
                if (disabledStrikeThrough && !value.IsAvailable)
                {
                    line = $"<s>{line}</s>";
                }

                optionText.text = line;
                interactable = value.IsAvailable;

                UpdateVisualState();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SetupButton();
        }

        private void SetupButton()
        {
            // Get or create background
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            backgroundImage.color = Win95Theme.ButtonFace;

            // Disable Unity's default transition
            transition = Transition.None;

            // Create 3D borders
            CreateBorders();

            // Setup text if not already present
            if (optionText == null)
            {
                optionText = GetComponentInChildren<TMP_Text>();
            }

            if (optionText == null)
            {
                GameObject textObj = new GameObject("OptionText");
                textObj.transform.SetParent(transform, false);

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(16, 8);
                textRect.offsetMax = new Vector2(-16, -8);

                optionText = textObj.AddComponent<TextMeshProUGUI>();
            }

            optionText.fontSize = fontSize;
            optionText.color = Win95Theme.WindowText;
            optionText.alignment = TextAlignmentOptions.MidlineLeft;
        }

        private void CreateBorders()
        {
            // Outer borders (2px each)
            topHighlight = CreateBorderImage("TopHighlight", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, -2));
            leftHighlight = CreateBorderImage("LeftHighlight", new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(2, 0));
            bottomShadow = CreateBorderImage("BottomShadow", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 2));
            rightShadow = CreateBorderImage("RightShadow", new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-2, 0));

            // Inner borders (2px each, inset by 2)
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

        private void UpdateVisualState()
        {
            if (!interactable)
            {
                // Disabled state
                SetBorderColors(Win95Theme.ColorLightGray, Win95Theme.ColorLightGray,
                    Win95Theme.ColorMidGray, Win95Theme.ColorMidGray);
                if (optionText != null) optionText.color = Win95Theme.ColorMidGray;
            }
            else if (isPressed)
            {
                // Pressed state - sunken
                SetBorderColors(Win95Theme.ColorDark, Win95Theme.ButtonShadow,
                    Win95Theme.ButtonHighlight, Win95Theme.ColorLightHighlight);
                if (optionText != null) optionText.color = Win95Theme.WindowText;
            }
            else if (IsHighlighted)
            {
                // Selected/highlighted state - keep raised but with focus indicator
                SetBorderColors(Win95Theme.ButtonHighlight, Win95Theme.ColorLightHighlight,
                    Win95Theme.ColorDark, Win95Theme.ButtonShadow);
                if (optionText != null) optionText.color = Win95Theme.WindowText;

                // Add dotted focus rectangle effect (using background color)
                backgroundImage.color = new Color(0.85f, 0.85f, 0.95f, 1f); // Slight blue tint
            }
            else
            {
                // Normal state - raised
                SetBorderColors(Win95Theme.ButtonHighlight, Win95Theme.ColorLightHighlight,
                    Win95Theme.ColorDark, Win95Theme.ButtonShadow);
                if (optionText != null) optionText.color = Win95Theme.WindowText;
                backgroundImage.color = Win95Theme.ButtonFace;
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

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            UpdateVisualState();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            isPressed = false;
            UpdateVisualState();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (interactable)
            {
                isPressed = true;
                UpdateVisualState();
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            isPressed = false;
            UpdateVisualState();
        }

        public new bool IsHighlighted => EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;

        public void OnSubmit(BaseEventData eventData)
        {
            InvokeOptionSelected();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InvokeOptionSelected();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            Select();
        }

        private void InvokeOptionSelected()
        {
            if (!IsInteractable()) return;

            if (!hasSubmittedOptionSelection && !completionToken.IsCancellationRequested)
            {
                hasSubmittedOptionSelection = true;
                OnOptionSelected?.TrySetResult(_option);
            }
        }

        /// <summary>
        /// Set the button text directly.
        /// </summary>
        public void SetText(string text)
        {
            if (optionText != null)
            {
                optionText.text = text;
            }
        }

        /// <summary>
        /// Create a Win95OptionItem on the given GameObject.
        /// </summary>
        public static Win95OptionItem Create(GameObject target, int fontSize = 36)
        {
            Win95OptionItem item = target.GetComponent<Win95OptionItem>();
            if (item == null)
            {
                item = target.AddComponent<Win95OptionItem>();
            }

            item.fontSize = fontSize;
            return item;
        }
    }
}
