using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
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
        [SerializeField] private float dialogWidth = 630f;
        [SerializeField] private float dialogHeight = 570f;
        [SerializeField] private int fontSize = 36;
        [SerializeField] private float sliderWidth = 330f;

        // Events
        public event Action<float> OnMasterVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSFXVolumeChanged;
        public event Action<bool> OnSoundtrackSideChanged;
        public event Action OnDialogClosed;
        public event Action OnResetDefaults;

        // UI Elements
        private GameObject dialogRoot;
        private GameObject dimBackground;
        private RectTransform dialogRootRect;
        private Slider masterSlider;
        private Slider musicSlider;
        private Slider sfxSlider;
        private TMP_Text masterValueText;
        private TMP_Text musicValueText;
        private TMP_Text sfxValueText;

        // Radio buttons for soundtrack selection
        private ToggleGroup soundtrackToggleGroup;
        private Toggle nelaToggle;
        private Toggle francoToggle;

        private bool isInitialized = false;
        private IDisposable modalLock;

        private void Awake()
        {
            if (!isInitialized)
            {
                CreateUI();
                isInitialized = true;
            }
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

            // Handle ESC to close
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ModalInputLock.ConsumeEscapeThisFrame();
                Close();
                return;
            }

            // Handle click outside dialog to close
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (!IsPointerOverDialog())
                {
                    Close();
                }
            }
        }

        private bool IsPointerOverDialog()
        {
            if (dialogRootRect == null) return false;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            return RectTransformUtility.RectangleContainsScreenPoint(dialogRootRect, mousePos, null);
        }

        private void CreateUI()
        {
            // Create dim background (no Button - we handle click-outside in Update)
            dimBackground = new GameObject("DimBackground");
            dimBackground.transform.SetParent(transform, false);

            RectTransform dimRect = dimBackground.AddComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.offsetMin = Vector2.zero;
            dimRect.offsetMax = Vector2.zero;

            Image dimImage = dimBackground.AddComponent<Image>();
            dimImage.color = new Color(0, 0, 0, 0.8f);
            dimImage.raycastTarget = true; // Blocks clicks from going through

            // Create dialog window
            dialogRoot = new GameObject("DialogWindow");
            dialogRoot.transform.SetParent(transform, false);

            dialogRootRect = dialogRoot.AddComponent<RectTransform>();
            dialogRootRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRootRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRootRect.pivot = new Vector2(0.5f, 0.5f);
            dialogRootRect.anchoredPosition = Vector2.zero;
            dialogRootRect.sizeDelta = new Vector2(dialogWidth, dialogHeight);

            Image bgImage = dialogRoot.AddComponent<Image>();
            bgImage.color = Win95Theme.WindowBackground;
            bgImage.raycastTarget = true;

            // Add raised border manually (not as layout children)
            CreateRaisedBorder(dialogRoot);

            // Create title bar
            CreateTitleBar();

            // Create content area
            CreateContentArea();

            // Create button row
            CreateButtonRow();

            // Start hidden
            gameObject.SetActive(false);
        }

        private void CreateRaisedBorder(GameObject target)
        {
            // Outer highlight (top-left)
            CreateBorderLine(target, "TopHighlight", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -2), Win95Theme.ButtonHighlight, true);
            CreateBorderLine(target, "LeftHighlight", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(2, 0), Win95Theme.ButtonHighlight, true);

            // Outer shadow (bottom-right)
            CreateBorderLine(target, "BottomShadow", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 2), Win95Theme.ColorDark, true);
            CreateBorderLine(target, "RightShadow", new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-2, 0), new Vector2(0, 0), Win95Theme.ColorDark, true);

            // Inner highlight
            CreateBorderLine(target, "TopHighlight2", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(2, -2), new Vector2(-2, -4), Win95Theme.ColorLightHighlight, true);
            CreateBorderLine(target, "LeftHighlight2", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(2, 2), new Vector2(4, -2), Win95Theme.ColorLightHighlight, true);

            // Inner shadow
            CreateBorderLine(target, "BottomShadow2", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(2, 2), new Vector2(-2, 4), Win95Theme.ButtonShadow, true);
            CreateBorderLine(target, "RightShadow2", new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-4, 2), new Vector2(-2, -2), Win95Theme.ButtonShadow, true);
        }

        private void CreateBorderLine(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color, bool ignoreLayout = false)
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

            if (ignoreLayout)
            {
                LayoutElement le = lineObj.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
            }
        }

        private void CreateTitleBar()
        {
            GameObject titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(dialogRoot.transform, false);

            RectTransform titleRect = titleBar.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -6);
            titleRect.sizeDelta = new Vector2(-12, 42);

            Image titleBg = titleBar.AddComponent<Image>();
            titleBg.color = Win95Theme.ColorTitleActive;

            // Title text
            GameObject titleTextObj = new GameObject("TitleText");
            titleTextObj.transform.SetParent(titleBar.transform, false);

            RectTransform textRect = titleTextObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(12, 0);
            textRect.offsetMax = new Vector2(-48, 0);

            TMP_Text titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Sound Settings";
            titleText.fontSize = fontSize;
            titleText.color = Win95Theme.TitleBarText;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // Close button
            GameObject closeObj = new GameObject("CloseButton");
            closeObj.transform.SetParent(titleBar.transform, false);

            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 0.5f);
            closeRect.anchorMax = new Vector2(1, 0.5f);
            closeRect.pivot = new Vector2(1, 0.5f);
            closeRect.anchoredPosition = new Vector2(-6, 0);
            closeRect.sizeDelta = new Vector2(33, 33);

            Image closeBg = closeObj.AddComponent<Image>();
            closeBg.color = Win95Theme.ButtonFace;

            Button closeButton = closeObj.AddComponent<Button>();
            closeButton.targetGraphic = closeBg;
            closeButton.onClick.AddListener(Close);

            // X text
            GameObject xTextObj = new GameObject("X");
            xTextObj.transform.SetParent(closeObj.transform, false);

            RectTransform xRect = xTextObj.AddComponent<RectTransform>();
            xRect.anchorMin = Vector2.zero;
            xRect.anchorMax = Vector2.one;
            xRect.offsetMin = Vector2.zero;
            xRect.offsetMax = Vector2.zero;

            TMP_Text xText = xTextObj.AddComponent<TextMeshProUGUI>();
            xText.text = "X";
            xText.fontSize = 24;
            xText.color = Win95Theme.WindowText;
            xText.fontStyle = FontStyles.Bold;
            xText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateContentArea()
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(dialogRoot.transform, false);

            RectTransform contentRect = contentArea.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(16, 60);
            contentRect.offsetMax = new Vector2(-16, -54);

            VerticalLayoutGroup contentVlg = contentArea.AddComponent<VerticalLayoutGroup>();
            contentVlg.padding = new RectOffset(12, 12, 12, 12);
            contentVlg.spacing = 6;
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = false;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;

            // Master Volume
            masterSlider = CreateVolumeRow(contentArea.transform, "Master", 1f, out masterValueText);
            masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);

            // Music Volume
            musicSlider = CreateVolumeRow(contentArea.transform, "Music", 0.7f, out musicValueText);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);

            // SFX Volume
            sfxSlider = CreateVolumeRow(contentArea.transform, "SFX", 1f, out sfxValueText);
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

            // No separator - directly to soundtrack radio group

            // Soundtrack Radio Buttons
            CreateSoundtrackRadioGroup(contentArea.transform);
        }

        private Slider CreateVolumeRow(Transform parent, string label, float defaultValue, out TMP_Text valueText)
        {
            GameObject row = new GameObject(label + "Row");
            row.transform.SetParent(parent, false);

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 60;
            rowLayout.minHeight = 60;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);

            TMP_Text labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = fontSize;
            labelText.color = Win95Theme.WindowText;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 120;
            labelLayout.preferredHeight = fontSize + 4;

            // Create slider using Unity's DefaultControls for proper functionality
            GameObject sliderObj = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderObj.name = label + "Slider";
            sliderObj.transform.SetParent(row.transform, false);

            LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
            sliderLayout.preferredWidth = sliderWidth;
            sliderLayout.preferredHeight = 30;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = defaultValue;

            // Style the slider for Win95 look
            StyleSlider(sliderObj);

            // Value text
            GameObject valueObj = new GameObject("Value");
            valueObj.transform.SetParent(row.transform, false);

            valueText = valueObj.AddComponent<TextMeshProUGUI>();
            valueText.text = $"{defaultValue * 100:F0}%";
            valueText.fontSize = fontSize - 6;
            valueText.color = Win95Theme.WindowText;
            valueText.alignment = TextAlignmentOptions.MidlineRight;

            LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 75;

            return slider;
        }

        private void StyleSlider(GameObject sliderObj)
        {
            // Background
            Transform bg = sliderObj.transform.Find("Background");
            if (bg != null)
            {
                Image bgImage = bg.GetComponent<Image>();
                bgImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            }

            // Fill
            Transform fillArea = sliderObj.transform.Find("Fill Area");
            if (fillArea != null)
            {
                Transform fill = fillArea.Find("Fill");
                if (fill != null)
                {
                    Image fillImage = fill.GetComponent<Image>();
                    fillImage.color = Win95Theme.ColorTitleActive;
                }
            }

            // Handle - darker grey with 3D raised border
            Transform handleArea = sliderObj.transform.Find("Handle Slide Area");
            if (handleArea != null)
            {
                Transform handle = handleArea.Find("Handle");
                if (handle != null)
                {
                    Image handleImage = handle.GetComponent<Image>();
                    handleImage.color = Win95Theme.ButtonShadow; // Darker grey

                    RectTransform handleRect = handle.GetComponent<RectTransform>();
                    handleRect.sizeDelta = new Vector2(21, 30);

                    // Add 3D raised border to handle
                    CreateHandleBorder(handle.gameObject);
                }
            }
        }

        private void CreateHandleBorder(GameObject handle)
        {
            // Top highlight (white)
            CreateBorderLine(handle, "TopHL", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -1), Win95Theme.ButtonHighlight);
            // Left highlight (white)
            CreateBorderLine(handle, "LeftHL", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), Win95Theme.ButtonHighlight);
            // Bottom shadow (dark)
            CreateBorderLine(handle, "BottomSH", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 1), Win95Theme.ColorDark);
            // Right shadow (dark)
            CreateBorderLine(handle, "RightSH", new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-1, 0), new Vector2(0, 0), Win95Theme.ColorDark);
        }

        private void CreateSoundtrackRadioGroup(Transform parent)
        {
            // Container with VLG for the two radio options
            GameObject group = new GameObject("SoundtrackGroup");
            group.transform.SetParent(parent, false);

            LayoutElement groupLayout = group.AddComponent<LayoutElement>();
            groupLayout.preferredHeight = 76;
            groupLayout.minHeight = 76;

            VerticalLayoutGroup groupVlg = group.AddComponent<VerticalLayoutGroup>();
            groupVlg.spacing = 4;
            groupVlg.childControlWidth = true;
            groupVlg.childControlHeight = false;
            groupVlg.childForceExpandWidth = true;
            groupVlg.childForceExpandHeight = false;

            // Toggle group for mutual exclusivity
            soundtrackToggleGroup = group.AddComponent<ToggleGroup>();
            soundtrackToggleGroup.allowSwitchOff = false;

            // Create two radio options
            nelaToggle = CreateRadioOption(group.transform, "Nela's Score", true);
            francoToggle = CreateRadioOption(group.transform, "Franco's Score", false);

            nelaToggle.group = soundtrackToggleGroup;
            francoToggle.group = soundtrackToggleGroup;

            nelaToggle.onValueChanged.AddListener(OnNelaToggleChanged);
        }

        private Toggle CreateRadioOption(Transform parent, string labelText, bool isOn)
        {
            GameObject row = new GameObject(labelText.Replace("'", "") + "Row");
            row.transform.SetParent(parent, false);

            LayoutElement rowLayout = row.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = 32;
            rowLayout.minHeight = 32;

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Radio button (circle with dot)
            GameObject radioObj = CreateWin95RadioButton();
            radioObj.transform.SetParent(row.transform, false);

            Toggle toggle = radioObj.GetComponent<Toggle>();
            toggle.isOn = isOn;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(row.transform, false);

            TMP_Text label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = fontSize;
            label.color = Win95Theme.WindowText;
            label.alignment = TextAlignmentOptions.MidlineLeft;

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 300;

            return toggle;
        }

        private GameObject CreateWin95RadioButton()
        {
            GameObject radioObj = new GameObject("RadioButton");

            LayoutElement layout = radioObj.AddComponent<LayoutElement>();
            layout.preferredWidth = 24;
            layout.preferredHeight = 24;
            layout.minWidth = 24;
            layout.minHeight = 24;

            // White circle background (square for now - Win95 used square with rounded appearance)
            Image bg = radioObj.AddComponent<Image>();
            bg.color = Color.white;

            // Sunken border (dark top-left, light bottom-right - opposite of raised)
            // Top shadow (dark)
            CreateBorderLine(radioObj, "TopSH", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -1), Win95Theme.ButtonShadow);
            // Left shadow (dark)
            CreateBorderLine(radioObj, "LeftSH", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), Win95Theme.ButtonShadow);
            // Bottom highlight (white)
            CreateBorderLine(radioObj, "BottomHL", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 1), Win95Theme.ButtonHighlight);
            // Right highlight (white)
            CreateBorderLine(radioObj, "RightHL", new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-1, 0), new Vector2(0, 0), Win95Theme.ButtonHighlight);

            // Center dot (black, shown when selected)
            GameObject dot = new GameObject("Dot");
            dot.transform.SetParent(radioObj.transform, false);

            RectTransform dotRect = dot.AddComponent<RectTransform>();
            dotRect.anchorMin = new Vector2(0.3f, 0.3f);
            dotRect.anchorMax = new Vector2(0.7f, 0.7f);
            dotRect.offsetMin = Vector2.zero;
            dotRect.offsetMax = Vector2.zero;

            Image dotImage = dot.AddComponent<Image>();
            dotImage.color = Win95Theme.WindowText;

            // Toggle component
            Toggle toggle = radioObj.AddComponent<Toggle>();
            toggle.graphic = dotImage; // Dot shown/hidden based on isOn
            toggle.targetGraphic = bg;

            return radioObj;
        }

        private void CreateButtonRow()
        {
            GameObject buttonRow = new GameObject("ButtonRow");
            buttonRow.transform.SetParent(dialogRoot.transform, false);

            RectTransform buttonRect = buttonRow.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 0);
            buttonRect.anchorMax = new Vector2(1, 0);
            buttonRect.pivot = new Vector2(0.5f, 0);
            buttonRect.anchoredPosition = new Vector2(0, 12);
            buttonRect.sizeDelta = new Vector2(-24, 48);

            HorizontalLayoutGroup buttonHlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            buttonHlg.spacing = 18;
            buttonHlg.childAlignment = TextAnchor.MiddleRight;
            buttonHlg.childControlWidth = false;
            buttonHlg.childControlHeight = true;
            buttonHlg.childForceExpandWidth = false;
            buttonHlg.childForceExpandHeight = false;

            // Spacer to push buttons right
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(buttonRow.transform, false);
            LayoutElement spacerLayout = spacer.AddComponent<LayoutElement>();
            spacerLayout.flexibleWidth = 1;

            // Reset button
            CreateButton(buttonRow.transform, "Reset", 120, () => {
                ResetToDefaults();
                OnResetDefaults?.Invoke();
            });

            // OK button
            CreateButton(buttonRow.transform, "OK", 105, Close);
        }

        private void CreateButton(Transform parent, string text, float width, Action onClick)
        {
            GameObject btnObj = new GameObject(text + "Button");
            btnObj.transform.SetParent(parent, false);

            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredWidth = width;
            btnLayout.preferredHeight = 42;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = Win95Theme.ButtonFace;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Simple raised border
            CreateBorderLine(btnObj, "TopHL", new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -2), Win95Theme.ButtonHighlight);
            CreateBorderLine(btnObj, "LeftHL", new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(2, 0), Win95Theme.ButtonHighlight);
            CreateBorderLine(btnObj, "BottomSH", new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 2), Win95Theme.ColorDark);
            CreateBorderLine(btnObj, "RightSH", new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-2, 0), new Vector2(0, 0), Win95Theme.ColorDark);

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TMP_Text btnText = textObj.AddComponent<TextMeshProUGUI>();
            btnText.text = text;
            btnText.fontSize = fontSize - 6;
            btnText.color = Win95Theme.WindowText;
            btnText.alignment = TextAlignmentOptions.Center;
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

        private void OnNelaToggleChanged(bool isNelaSelected)
        {
            Debug.Log($"Soundtrack toggle changed: {(isNelaSelected ? "Nela (A)" : "Franco (B)")}");

            // Directly update FMOD audio manager
            if (FMODAudioManager.Instance != null)
            {
                FMODAudioManager.Instance.SetSoundtrackSide(isNelaSelected ? "A" : "B");
            }

            OnSoundtrackSideChanged?.Invoke(isNelaSelected);
        }

        /// <summary>
        /// Show the settings panel.
        /// </summary>
        public void Show()
        {
            modalLock = ModalInputLock.Acquire(this);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Close the settings panel.
        /// </summary>
        public void Close()
        {
            modalLock?.Dispose();
            modalLock = null;
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
            if (nelaToggle != null) nelaToggle.isOn = true;
        }

        /// <summary>
        /// Set the current values (call when showing to reflect saved settings).
        /// </summary>
        public void SetValues(float master, float music, float sfx, bool sideA)
        {
            if (masterSlider != null) masterSlider.value = master;
            if (musicSlider != null) musicSlider.value = music;
            if (sfxSlider != null) sfxSlider.value = sfx;
            if (nelaToggle != null && francoToggle != null)
            {
                nelaToggle.isOn = sideA;
                francoToggle.isOn = !sideA;
            }
        }

        /// <summary>
        /// Create a Win95SettingsPanel with its own Canvas for proper z-ordering.
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

            // Add Canvas with high sortingOrder to appear above overlay UI (2000)
            Canvas panelCanvas = panelObj.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 3000;

            // Add GraphicRaycaster for UI interaction
            panelObj.AddComponent<GraphicRaycaster>();

            return panelObj.AddComponent<Win95SettingsPanel>();
        }
    }
}
