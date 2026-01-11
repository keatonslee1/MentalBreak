using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled options presenter.
    /// Displays dialogue options as Win95 buttons in a vertical layout.
    /// Replaces the dialogue wheel with a cleaner, more Windows 95 appropriate interface.
    /// </summary>
    public class Win95OptionsPresenter : DialoguePresenterBase
    {
        [Header("Layout")]
        [SerializeField] private float panelWidth = 1000f;
        [SerializeField] private float buttonHeight = 72f;
        [SerializeField] private float buttonSpacing = 8f;
        [SerializeField] private float panelPadding = 16f;
        [SerializeField] private int fontSize = 36;

        [Header("Settings")]
        [SerializeField] private bool showsLastLine = true;
        [SerializeField] private bool showUnavailableOptions = false;

        [Header("Fade")]
        [SerializeField] private bool useFadeEffect = true;
        [SerializeField] private float fadeUpDuration = 0.15f;
        [SerializeField] private float fadeDownDuration = 0.1f;

        private CanvasGroup canvasGroup;
        private GameObject optionsPanel;
        private GameObject lastLineContainer;
        private TMP_Text lastLineText;
        private TMP_Text lastLineCharacterText;
        private GameObject optionsContainer;

        private List<Win95OptionItem> optionViews = new List<Win95OptionItem>();
        private LocalizedLine lastSeenLine;
        private bool isInitialized = false;

        private void Awake()
        {
            CreateUI();
        }

        private void Start()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private void CreateUI()
        {
            if (isInitialized) return;

            // Create main options panel
            optionsPanel = new GameObject("Win95OptionsPanel");
            optionsPanel.transform.SetParent(transform, false);

            RectTransform panelRect = optionsPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0);
            panelRect.anchorMax = new Vector2(0.5f, 0);
            panelRect.pivot = new Vector2(0.5f, 0);
            panelRect.anchoredPosition = new Vector2(0, 40);
            panelRect.sizeDelta = new Vector2(panelWidth, 200); // Will be adjusted by content

            // Win95 raised panel background
            Image bgImage = optionsPanel.AddComponent<Image>();
            bgImage.color = Win95Theme.WindowBackground;

            canvasGroup = optionsPanel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Vertical layout
            VerticalLayoutGroup layout = optionsPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)panelPadding, (int)panelPadding, (int)panelPadding, (int)panelPadding);
            layout.spacing = buttonSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = optionsPanel.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add Win95 raised border
            Win95Panel.Create(optionsPanel, Win95Panel.PanelStyle.Raised);

            // Create last line display (shows the dialogue prompt before options)
            CreateLastLineDisplay();

            // Create options container
            optionsContainer = new GameObject("OptionsContainer");
            optionsContainer.transform.SetParent(optionsPanel.transform, false);

            RectTransform containerRect = optionsContainer.AddComponent<RectTransform>();

            VerticalLayoutGroup containerLayout = optionsContainer.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = buttonSpacing;
            containerLayout.childControlWidth = true;
            containerLayout.childControlHeight = false;
            containerLayout.childForceExpandWidth = true;
            containerLayout.childForceExpandHeight = false;

            ContentSizeFitter containerFitter = optionsContainer.AddComponent<ContentSizeFitter>();
            containerFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            containerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            isInitialized = true;
        }

        private void CreateLastLineDisplay()
        {
            lastLineContainer = new GameObject("LastLineContainer");
            lastLineContainer.transform.SetParent(optionsPanel.transform, false);

            RectTransform containerRect = lastLineContainer.AddComponent<RectTransform>();

            // Sunken panel for last line
            Image containerBg = lastLineContainer.AddComponent<Image>();
            containerBg.color = Color.white;

            // Add sunken border
            CreateSunkenBorder(lastLineContainer);

            VerticalLayoutGroup containerLayout = lastLineContainer.AddComponent<VerticalLayoutGroup>();
            containerLayout.padding = new RectOffset(16, 16, 8, 8);
            containerLayout.spacing = 4;
            containerLayout.childControlWidth = true;
            containerLayout.childControlHeight = true;
            containerLayout.childForceExpandWidth = true;
            containerLayout.childForceExpandHeight = false;

            ContentSizeFitter containerFitter = lastLineContainer.AddComponent<ContentSizeFitter>();
            containerFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            containerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement containerLayoutElement = lastLineContainer.AddComponent<LayoutElement>();
            containerLayoutElement.minHeight = 80;

            // Character name text
            GameObject charNameObj = new GameObject("CharacterName");
            charNameObj.transform.SetParent(lastLineContainer.transform, false);

            lastLineCharacterText = charNameObj.AddComponent<TextMeshProUGUI>();
            lastLineCharacterText.fontSize = fontSize - 2;
            lastLineCharacterText.color = Win95Theme.ColorTitleActive;
            lastLineCharacterText.fontStyle = FontStyles.Bold;
            lastLineCharacterText.alignment = TextAlignmentOptions.TopLeft;

            LayoutElement charLayout = charNameObj.AddComponent<LayoutElement>();
            charLayout.minHeight = fontSize;

            // Dialogue text
            GameObject textObj = new GameObject("LastLineText");
            textObj.transform.SetParent(lastLineContainer.transform, false);

            lastLineText = textObj.AddComponent<TextMeshProUGUI>();
            lastLineText.fontSize = fontSize;
            lastLineText.color = Win95Theme.WindowText;
            lastLineText.alignment = TextAlignmentOptions.TopLeft;

            LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.minHeight = fontSize + 4;

            lastLineContainer.SetActive(false);
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

        public override YarnTask OnDialogueStartedAsync()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            return YarnTask.CompletedTask;
        }

        public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            if (showsLastLine)
            {
                lastSeenLine = line;
            }
            return YarnTask.CompletedTask;
        }

        public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
        {
            if (!isInitialized)
            {
                CreateUI();
            }

            // Ensure we have enough option views
            while (dialogueOptions.Length > optionViews.Count)
            {
                var optionView = CreateNewOptionView();
                optionViews.Add(optionView);
            }

            // Completion source for selected option
            YarnTaskCompletionSource<DialogueOption> selectedOptionSource = new YarnTaskCompletionSource<DialogueOption>();
            var completionCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Handle cancellation
            async YarnTask CancelSourceWhenDialogueCancelled()
            {
                await YarnTask.WaitUntilCanceled(completionCancellationSource.Token);
                if (cancellationToken.IsCancellationRequested)
                {
                    selectedOptionSource.TrySetResult(null);
                }
            }
            CancelSourceWhenDialogueCancelled().Forget();

            // Configure option views
            int visibleCount = 0;
            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                var optionView = optionViews[i];
                var option = dialogueOptions[i];

                if (!option.IsAvailable && !showUnavailableOptions)
                {
                    optionView.gameObject.SetActive(false);
                    continue;
                }

                optionView.gameObject.SetActive(true);
                optionView.Option = option;
                optionView.OnOptionSelected = selectedOptionSource;
                optionView.completionToken = completionCancellationSource.Token;
                visibleCount++;
            }

            // Hide unused option views
            for (int i = dialogueOptions.Length; i < optionViews.Count; i++)
            {
                optionViews[i].gameObject.SetActive(false);
            }

            // Select first available option
            for (int i = 0; i < optionViews.Count; i++)
            {
                if (optionViews[i].isActiveAndEnabled && optionViews[i].IsInteractable())
                {
                    optionViews[i].Select();
                    break;
                }
            }

            // Update last line display
            if (lastLineContainer != null && showsLastLine && lastSeenLine != null)
            {
                lastLineContainer.SetActive(true);

                if (string.IsNullOrWhiteSpace(lastSeenLine.CharacterName))
                {
                    lastLineCharacterText.gameObject.SetActive(false);
                    lastLineText.text = lastSeenLine.TextWithoutCharacterName.Text;
                }
                else
                {
                    lastLineCharacterText.gameObject.SetActive(true);
                    lastLineCharacterText.text = lastSeenLine.CharacterName + ":";
                    lastLineText.text = lastSeenLine.TextWithoutCharacterName.Text;
                }
            }
            else if (lastLineContainer != null)
            {
                lastLineContainer.SetActive(false);
            }

            // Fade in
            if (useFadeEffect && canvasGroup != null)
            {
                await Effects.FadeAlphaAsync(canvasGroup, 0, 1, fadeUpDuration, cancellationToken);
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
            }

            // Enable interaction
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            // Wait for selection
            var selectedOption = await selectedOptionSource.Task;
            completionCancellationSource.Cancel();

            // Disable interaction
            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // Fade out
            if (useFadeEffect && canvasGroup != null)
            {
                await Effects.FadeAlphaAsync(canvasGroup, 1, 0, fadeDownDuration, cancellationToken);
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }

            // Hide all option views
            foreach (var optionView in optionViews)
            {
                optionView.gameObject.SetActive(false);
            }

            await YarnTask.Yield();

            if (cancellationToken.IsCancellationRequested)
            {
                return await DialogueRunner.NoOptionSelected;
            }

            return selectedOption;
        }

        private Win95OptionItem CreateNewOptionView()
        {
            GameObject optionObj = new GameObject("Win95Option");
            optionObj.transform.SetParent(optionsContainer.transform, false);

            RectTransform optionRect = optionObj.AddComponent<RectTransform>();
            optionRect.sizeDelta = new Vector2(0, buttonHeight);

            LayoutElement layout = optionObj.AddComponent<LayoutElement>();
            layout.preferredHeight = buttonHeight;
            layout.minHeight = buttonHeight;

            Win95OptionItem optionItem = Win95OptionItem.Create(optionObj, fontSize);
            optionObj.SetActive(false);

            return optionItem;
        }

        /// <summary>
        /// Set the panel width.
        /// </summary>
        public void SetPanelWidth(float width)
        {
            panelWidth = width;
            if (optionsPanel != null)
            {
                RectTransform rect = optionsPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(panelWidth, rect.sizeDelta.y);
                }
            }
        }
    }
}
