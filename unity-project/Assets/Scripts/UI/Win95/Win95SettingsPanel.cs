using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 styled settings panel.
    /// Provides volume controls, soundtrack selection, and other settings.
    /// </summary>
    public class Win95SettingsPanel : MonoBehaviour
    {
        [Header("Dialog Settings")]
        [SerializeField] private float dialogWidth = 800f;
        [SerializeField] private float dialogHeight = 700f;
        [SerializeField] private int fontSize = 36;
        [SerializeField] private float sliderWidth = 400f;

        // Events
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<bool> OnSoundtrackSideChanged; // true = Side A, false = Side B
        public event Action OnDialogClosed;
        public event Action OnResetDefaults;

        // UI Elements
        private GameObject dialogRoot;
        private GameObject dimBackground;
        private TMP_Text titleText;
        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private TMP_Text masterValueText;
        private TMP_Text musicValueText;
        private TMP_Text sfxValueText;
        private Toggle soundtrackToggle;
        private TMP_Text soundtrackLabel;
        private CanvasGroup canvasGroup;

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

            // Create content area
            CreateContentArea();

            // Create button row
            CreateButtonRow();

            // Start hidden
            gameObject.SetActive(false);
        }

        private void CreateTitleBar()
        {
            GameObject titleBar = new GameObject("TitleBar");
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
            titleText.text = "Sound Settings";
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

            Button closeButton = closeObj.AddComponent<Button>();
            closeButton.onClick.AddListener(Close);

            Win95Button.Create(closeObj, "X", fontSize - 2);
        }

        private void CreateContentArea()
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(dialogRoot.transform, false);

            LayoutElement contentLayout = contentArea.AddComponent<LayoutElement>();
            contentLayout.flexibleHeight = 1;

            VerticalLayoutGroup contentVlg = contentArea.AddComponent<VerticalLayoutGroup>();
            contentVlg.padding = new RectOffset(32, 32, 32, 32);
            contentVlg.spacing = 24;
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = false;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;

            // Master Volume
            CreateVolumeSlider(contentArea.transform, "Master Volume", 1f,
                out masterSlider, out masterValueText, OnMasterSliderChanged);

            // Music Volume
            CreateVolumeSlider(contentArea.transform, "Music Volume", 0.7f,
                out musicSlider, out musicValueText, OnMusicSliderChanged);

            // SFX Volume
            CreateVolumeSlider(contentArea.transform, "SFX Volume", 1f,
                out sfxSlider, out sfxValueText, OnSFXSliderChanged);

            // Separator
            CreateSeparator(contentArea.transform);

            // Soundtrack Toggle
            CreateSoundtrackToggle(contentArea.transform);
        }

        private void CreateVolumeSlider(Transform parent, string label, float defaultValue,
            out Slider slider, out TMP_Text valueText, Action<float> onValueChanged)
        {
            GameObject row = new GameObject(label.Replace(" ", "") + "Row");
            row.transform.SetParent(parent, false);

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 80;

            VerticalLayoutGroup rowVlg = row.AddComponent<VerticalLayoutGroup>();
            rowVlg.spacing = 8;
            rowVlg.childControlWidth = true;
            rowVlg.childControlHeight = false;
            rowVlg.childForceExpandWidth = true;
            rowVlg.childForceExpandHeight = false;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);

            TMP_Text labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = fontSize;
            labelText.color = Win95Theme.WindowText;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredHeight = fontSize + 4;

            // Slider row
            GameObject sliderRow = new GameObject("SliderRow");
            sliderRow.transform.SetParent(row.transform, false);

            HorizontalLayoutGroup sliderHlg = sliderRow.AddComponent<HorizontalLayoutGroup>();
            sliderHlg.spacing = 16;
            sliderHlg.childAlignment = TextAnchor.MiddleLeft;
            sliderHlg.childControlWidth = false;
            sliderHlg.childControlHeight = true;
            sliderHlg.childForceExpandWidth = false;
            sliderHlg.childForceExpandHeight = true;

            LayoutElement sliderRowLayout = sliderRow.AddComponent<LayoutElement>();
            sliderRowLayout.preferredHeight = 48;

            // Create Win95 styled slider
            slider = CreateWin95Slider(sliderRow.transform, sliderWidth);
            slider.value = defaultValue;
            slider.onValueChanged.AddListener((value) => onValueChanged(value));

            // Value text
            GameObject valueObj = new GameObject("ValueText");
            valueObj.transform.SetParent(sliderRow.transform, false);

            valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = $"{defaultValue * 100:F0}%";
            valueText.fontSize = fontSize - 2;
            valueText.color = Win95Theme.WindowText;
            valueText.alignment = TextAlignmentOptions.MidlineRight;

            LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 100;
        }

        private Slider CreateWin95Slider(Transform parent, float width)
        {
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(parent, false);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(width, 40);

            LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = width;
            sliderLayout.preferredHeight = 40;

            // Background track (sunken)
            GameObject trackObj = new GameObject("Track");
            trackObj.transform.SetParent(sliderObj.transform, false);

            RectTransform trackRect = trackObj.AddComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(0, 0.5f);
            trackRect.anchorMax = new Vector2(1, 0.5f);
            trackRect.pivot = new Vector2(0.5f, 0.5f);
            trackRect.anchoredPosition = Vector2.zero;
            trackRect.sizeDelta = new Vector2(0, 16);

            Image trackImage = trackObj.AddComponent<Image>();
            trackImage.color = Color.white;

            // Add sunken border to track
            CreateSunkenBorder(trackObj);

            // Fill area
            GameObject fillAreaObj = new GameObject("FillArea");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);

            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1, 0.5f);
            fillAreaRect.pivot = new Vector2(0, 0.5f);
            fillAreaRect.anchoredPosition = new Vector2(16, 0);
            fillAreaRect.sizeDelta = new Vector2(-32, 12);

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);

            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Win95Theme.ColorTitleActive;

            // Handle
            GameObject handleAreaObj = new GameObject("HandleArea");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);

            RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(16, 0);
            handleAreaRect.offsetMax = new Vector2(-16, 0);

            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);

            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(32, 40);

            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Win95Theme.ButtonFace;

            // Add raised border to handle
            Win95Panel handlePanel = handleObj.AddComponent<Win95Panel>();

            // Create slider component
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            return slider;
        }

        private void CreateSeparator(Transform parent)
        {
            GameObject sepObj = new GameObject("Separator");
            sepObj.transform.SetParent(parent, false);

            LayoutElement sepLayout = sepObj.AddComponent<LayoutElement>();
            sepLayout.preferredHeight = 4;

            Image sepImage = sepObj.AddComponent<Image>();
            sepImage.color = Win95Theme.ButtonShadow;
        }

        private void CreateSoundtrackToggle(Transform parent)
        {
            GameObject row = new GameObject("SoundtrackRow");
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup rowHlg = row.AddComponent<HorizontalLayoutGroup>();
            rowHlg.spacing = 16;
            rowHlg.childAlignment = TextAnchor.MiddleLeft;
            rowHlg.childControlWidth = true;
            rowHlg.childControlHeight = true;
            rowHlg.childForceExpandWidth = false;
            rowHlg.childForceExpandHeight = true;

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 56;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);

            TMP_Text labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Soundtrack:";
            labelText.fontSize = fontSize;
            labelText.color = Win95Theme.WindowText;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 200;

            // Toggle
            GameObject toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(row.transform, false);

            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(40, 40);

            LayoutElement toggleLayout = toggleObj.AddComponent<LayoutElement>();
            toggleLayout.preferredWidth = 40;
            toggleLayout.preferredHeight = 40;

            Image toggleBg = toggleObj.AddComponent<Image>();
            toggleBg.color = Color.white;

            // Checkmark
            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(toggleObj.transform, false);

            RectTransform checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;

            Image checkImage = checkObj.AddComponent<Image>();
            checkImage.color = Win95Theme.WindowText;

            soundtrackToggle = toggleObj.AddComponent<Toggle>();
            soundtrackToggle.graphic = checkImage;
            soundtrackToggle.targetGraphic = toggleBg;
            soundtrackToggle.isOn = true; // Default to Side A
            soundtrackToggle.onValueChanged.AddListener(OnSoundtrackToggleChanged);

            // Soundtrack label
            GameObject soundtrackLabelObj = new GameObject("SoundtrackLabel");
            soundtrackLabelObj.transform.SetParent(row.transform, false);

            soundtrackLabel = soundtrackLabelObj.AddComponent<TextMeshProUGUI>();
            soundtrackLabel.text = "Nela's Score";
            soundtrackLabel.fontSize = fontSize;
            soundtrackLabel.color = Win95Theme.WindowText;
            soundtrackLabel.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement soundtrackLabelLayout = soundtrackLabelObj.AddComponent<LayoutElement>();
            soundtrackLabelLayout.flexibleWidth = 1;
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

            // Reset Defaults button
            CreateButton(buttonRow.transform, "Reset", 200, () => {
                ResetToDefaults();
                OnResetDefaults?.Invoke();
            });

            // OK button
            CreateButton(buttonRow.transform, "OK", 160, Close);
        }

        private void CreateButton(Transform parent, string text, float width, Action onClick)
        {
            GameObject btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(width, 64);

            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredWidth = width;
            btnLayout.preferredHeight = 64;

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            Win95Button.Create(btnObj, text, fontSize);
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

        // Event handlers
        private void OnMasterSliderChanged(float value)
        {
            if (masterValueText != null) masterValueText.text = $"{value * 100:F0}%";
            OnMasterVolumeChanged?.Invoke(value);
        }

        private void OnMusicSliderChanged(float value)
        {
            if (musicValueText != null) musicValueText.text = $"{value * 100:F0}%";
            OnMusicVolumeChanged?.Invoke(value);
        }

        private void OnSFXSliderChanged(float value)
        {
            if (sfxValueText != null) sfxValueText.text = $"{value * 100:F0}%";
            OnSFXVolumeChanged?.Invoke(value);
        }

        private void OnSoundtrackToggleChanged(bool isOn)
        {
            if (soundtrackLabel != null)
            {
                soundtrackLabel.text = isOn ? "Nela's Score" : "Franco's Score";
            }
            OnSoundtrackSideChanged?.Invoke(isOn);
        }

        /// <summary>
        /// Show the settings panel.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Close the settings panel.
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            OnDialogClosed?.Invoke();
        }

        /// <summary>
        /// Reset all settings to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            if (masterSlider != null) masterSlider.value = 1f;
            if (musicSlider != null) musicSlider.value = 0.7f;
            if (sfxSlider != null) sfxSlider.value = 1f;
            if (soundtrackToggle != null) soundtrackToggle.isOn = true;
        }

        /// <summary>
        /// Set the current values (call when showing to reflect saved settings).
        /// </summary>
        public void SetValues(float master, float music, float sfx, bool sideA)
        {
            if (masterSlider != null) masterSlider.value = master;
            if (musicSlider != null) musicSlider.value = music;
            if (sfxSlider != null) sfxSlider.value = sfx;
            if (soundtrackToggle != null) soundtrackToggle.isOn = sideA;
        }

        /// <summary>
        /// Create a Win95SettingsPanel.
        /// </summary>
        public static Win95SettingsPanel Create(Transform parent)
        {
            GameObject panelObj = new GameObject("Win95SettingsPanel");
            panelObj.transform.SetParent(parent, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            return panelObj.AddComponent<Win95SettingsPanel>();
        }
    }
}
