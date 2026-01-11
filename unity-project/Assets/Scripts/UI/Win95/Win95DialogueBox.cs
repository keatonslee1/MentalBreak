using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled dialogue box.
    /// Creates a Win95-themed container for dialogue text with proper borders and styling.
    /// Can be attached to the LinePresenter to restyle the dialogue display.
    /// </summary>
    public class Win95DialogueBox : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private float boxWidth = 1200f;
        [SerializeField] private float minHeight = 200f;
        [SerializeField] private float bottomMargin = 240f;
        [SerializeField] private float padding = 24f;

        [Header("Settings")]
        [SerializeField] private int fontSize = 44;
        [SerializeField] private int nameFontSize = 40;
        [SerializeField] private bool showCharacterName = true;

        private GameObject boxContainer;
        private GameObject nameContainer;
        private TMP_Text nameText;
        private GameObject textContainer;
        private TMP_Text dialogueText;
        private Image backgroundImage;

        private bool isInitialized = false;

        private void OnEnable()
        {
            if (!isInitialized)
            {
                CreateUI();
                isInitialized = true;
            }
        }

        private void CreateUI()
        {
            // Get or create the dialogue box container
            boxContainer = gameObject;

            RectTransform boxRect = GetComponent<RectTransform>();
            if (boxRect == null)
            {
                boxRect = gameObject.AddComponent<RectTransform>();
            }

            // Position at bottom center
            boxRect.anchorMin = new Vector2(0.5f, 0);
            boxRect.anchorMax = new Vector2(0.5f, 0);
            boxRect.pivot = new Vector2(0.5f, 0);
            boxRect.anchoredPosition = new Vector2(0, bottomMargin);
            boxRect.sizeDelta = new Vector2(boxWidth, minHeight);

            // Win95 panel background
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }
            backgroundImage.color = Win95Theme.WindowBackground;

            // Add content size fitter for auto-height
            ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Vertical layout
            VerticalLayoutGroup layout = GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            layout.spacing = 8;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperLeft;

            // Create character name display
            CreateCharacterNameDisplay();

            // Create dialogue text area
            CreateDialogueTextArea();

            // Add Win95 raised border
            Win95Panel.Create(boxContainer, Win95Panel.PanelStyle.Raised);
        }

        private void CreateCharacterNameDisplay()
        {
            nameContainer = new GameObject("CharacterNameContainer");
            nameContainer.transform.SetParent(transform, false);

            RectTransform nameRect = nameContainer.AddComponent<RectTransform>();

            // Name panel with title bar styling
            Image nameBg = nameContainer.AddComponent<Image>();
            nameBg.color = Win95Theme.ColorTitleActive;

            HorizontalLayoutGroup nameLayout = nameContainer.AddComponent<HorizontalLayoutGroup>();
            nameLayout.padding = new RectOffset(12, 12, 4, 4);
            nameLayout.childAlignment = TextAnchor.MiddleLeft;
            nameLayout.childControlWidth = true;
            nameLayout.childControlHeight = true;
            nameLayout.childForceExpandWidth = true;
            nameLayout.childForceExpandHeight = false;

            LayoutElement nameLayoutElement = nameContainer.AddComponent<LayoutElement>();
            nameLayoutElement.minHeight = nameFontSize + 8;
            nameLayoutElement.preferredHeight = nameFontSize + 8;

            // Name text
            GameObject nameTextObj = new GameObject("CharacterName");
            nameTextObj.transform.SetParent(nameContainer.transform, false);

            nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.fontSize = nameFontSize;
            nameText.color = Win95Theme.TitleBarText;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;

            nameContainer.SetActive(showCharacterName);
        }

        private void CreateDialogueTextArea()
        {
            textContainer = new GameObject("DialogueTextContainer");
            textContainer.transform.SetParent(transform, false);

            RectTransform textContainerRect = textContainer.AddComponent<RectTransform>();

            // Sunken text area (like a text box)
            Image textBg = textContainer.AddComponent<Image>();
            textBg.color = Color.white;

            // Add sunken border
            CreateSunkenBorder(textContainer);

            VerticalLayoutGroup textLayout = textContainer.AddComponent<VerticalLayoutGroup>();
            textLayout.padding = new RectOffset(16, 16, 12, 12);
            textLayout.childAlignment = TextAnchor.UpperLeft;
            textLayout.childControlWidth = true;
            textLayout.childControlHeight = true;
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;

            LayoutElement textLayoutElement = textContainer.AddComponent<LayoutElement>();
            textLayoutElement.minHeight = minHeight - 40;
            textLayoutElement.flexibleHeight = 1;

            // Dialogue text
            GameObject textObj = new GameObject("DialogueText");
            textObj.transform.SetParent(textContainer.transform, false);

            dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            dialogueText.fontSize = fontSize;
            dialogueText.color = Win95Theme.WindowText;
            dialogueText.alignment = TextAlignmentOptions.TopLeft;

            LayoutElement dialogueLayout = textObj.AddComponent<LayoutElement>();
            dialogueLayout.minHeight = fontSize + 10;
            dialogueLayout.flexibleHeight = 1;
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
        /// Set the character name display.
        /// </summary>
        public void SetCharacterName(string characterName)
        {
            if (nameText != null)
            {
                nameText.text = characterName;
            }

            if (nameContainer != null)
            {
                nameContainer.SetActive(!string.IsNullOrEmpty(characterName));
            }
        }

        /// <summary>
        /// Set the dialogue text.
        /// </summary>
        public void SetDialogueText(string text)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text;
            }
        }

        /// <summary>
        /// Get the dialogue text component for typewriter effects.
        /// </summary>
        public TMP_Text GetDialogueText()
        {
            return dialogueText;
        }

        /// <summary>
        /// Get the character name text component.
        /// </summary>
        public TMP_Text GetCharacterNameText()
        {
            return nameText;
        }

        /// <summary>
        /// Show or hide the entire dialogue box.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// Apply the Win95 style to an existing LinePresenter.
        /// This method can be called to restyle the default Yarn Spinner LinePresenter.
        /// </summary>
        public static void ApplyToLinePresenter(GameObject linePresenterObject)
        {
            if (linePresenterObject == null) return;

            // Find the background image and restyle it
            Image bgImage = linePresenterObject.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = Win95Theme.WindowBackground;
            }

            // Add Win95 panel styling
            Win95Panel panel = linePresenterObject.GetComponent<Win95Panel>();
            if (panel == null)
            {
                Win95Panel.Create(linePresenterObject, Win95Panel.PanelStyle.Raised);
            }

            // Find and restyle text components
            TMP_Text[] texts = linePresenterObject.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                text.color = Win95Theme.WindowText;
            }

            Debug.Log("Win95DialogueBox: Applied Win95 styling to LinePresenter.");
        }

        /// <summary>
        /// Create a Win95DialogueBox on the given GameObject.
        /// </summary>
        public static Win95DialogueBox Create(GameObject target, float width = 1200f, int fontSize = 44)
        {
            Win95DialogueBox box = target.GetComponent<Win95DialogueBox>();
            if (box == null)
            {
                box = target.AddComponent<Win95DialogueBox>();
            }

            box.boxWidth = width;
            box.fontSize = fontSize;

            return box;
        }
    }
}
