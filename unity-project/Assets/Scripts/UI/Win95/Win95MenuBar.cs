using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Windows 95 style menu bar with clickable menu items.
    /// Menu items: Save | Load | Sound | Debug | Feedback
    /// </summary>
    public class Win95MenuBar : MonoBehaviour
    {
        [Header("Menu Items")]
        [SerializeField] private List<string> menuItems = new List<string> { "Back", "Save", "Load", "Sound", "Debug", "Feedback" };

        // Fixed settings (constants to avoid Unity serialization override, 2x scale)
        private const float MenuItemPaddingHorizontal = 16f;
        private const float MenuItemPaddingVertical = 1f;
        private const float MenuItemSpacing = 8f;
        private const int FontSize = 32;  // Visible size for menu bar (2x scale)

        // Events for each menu item
        public event Action OnBackClicked;
        public event Action OnSaveClicked;
        public event Action OnLoadClicked;
        public event Action OnSoundClicked;
        public event Action OnDebugClicked;
        public event Action OnFeedbackClicked;

        private List<Button> menuButtons = new List<Button>();
        private HorizontalLayoutGroup layoutGroup;

        private void Awake()
        {
            Debug.Log($"[Win95MenuBar] Awake() called. Child count BEFORE: {transform.childCount}");
            CreateMenuBar();
            Debug.Log($"[Win95MenuBar] After CreateMenuBar(). Child count AFTER: {transform.childCount}");
        }

        private void CreateMenuBar()
        {
            // Clear existing menu items and borders to prevent duplicates
            ClearExistingMenuItems();

            // Add horizontal layout group
            layoutGroup = GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layoutGroup.padding = new RectOffset(8, 8, 2, 2);
            layoutGroup.spacing = MenuItemSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = true;

            // Create menu items
            foreach (string menuItem in menuItems)
            {
                CreateMenuItem(menuItem);
            }

            // Add bottom border (separator line)
            CreateBottomBorder();
        }

        private void ClearExistingMenuItems()
        {
            Debug.Log($"[Win95MenuBar] ClearExistingMenuItems: childCount = {transform.childCount}");
            // Destroy all children to prevent duplicates when scene has old serialized items
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Debug.Log($"[Win95MenuBar] Destroying child: {transform.GetChild(i).name}");
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            Debug.Log($"[Win95MenuBar] After destroying: childCount = {transform.childCount}");

            // Also destroy any border lines that were created in parent
            if (transform.parent != null)
            {
                Transform bottomShadow = transform.parent.Find("BottomShadow");
                Transform bottomHighlight = transform.parent.Find("BottomHighlight");
                if (bottomShadow != null) DestroyImmediate(bottomShadow.gameObject);
                if (bottomHighlight != null) DestroyImmediate(bottomHighlight.gameObject);
            }

            menuButtons.Clear();
        }

        private void CreateMenuItem(string itemName)
        {
            Debug.Log($"[Win95MenuBar] CreateMenuItem: {itemName}, FontSize constant = {FontSize}, Padding = {MenuItemPaddingHorizontal}");

            GameObject itemObj = new GameObject(itemName + "MenuItem");
            itemObj.transform.SetParent(transform, false);

            RectTransform rect = itemObj.AddComponent<RectTransform>();

            // Background image - FLAT style (transparent normally, no 3D border)
            Image bgImage = itemObj.AddComponent<Image>();
            bgImage.color = Color.clear;  // Transparent by default (flat appearance)
            bgImage.raycastTarget = true;

            // Button component - disable color transitions (we handle hover manually)
            Button btn = itemObj.AddComponent<Button>();
            btn.targetGraphic = bgImage;
            btn.transition = Selectable.Transition.None;  // No automatic color changes

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(MenuItemPaddingHorizontal, MenuItemPaddingVertical);
            textRect.offsetMax = new Vector2(-MenuItemPaddingHorizontal, -MenuItemPaddingVertical);

            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = itemName;
            text.color = Win95Theme.WindowText;
            text.fontSize = FontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = false;  // Ensure no auto-sizing overrides fontSize

            Debug.Log($"[Win95MenuBar] {itemName}: fontSize AFTER set = {text.fontSize}, enableAutoSizing = {text.enableAutoSizing}");

            // Force TMP to calculate its metrics before getting preferredWidth
            text.ForceMeshUpdate();

            // Calculate preferred width based on text + padding
            LayoutElement layout = itemObj.AddComponent<LayoutElement>();
            layout.preferredWidth = text.preferredWidth + (MenuItemPaddingHorizontal * 2);
            layout.preferredHeight = Win95Theme.MenuBarHeight - 4;

            Debug.Log($"[Win95MenuBar] {itemName}: preferredWidth = {layout.preferredWidth}, spacing = {MenuItemSpacing}");

            // Wire up click events
            WireMenuItemClick(btn, itemName, text);

            menuButtons.Add(btn);
        }

        private void WireMenuItemClick(Button btn, string itemName, TMP_Text text)
        {
            btn.onClick.AddListener(() =>
            {
                switch (itemName)
                {
                    case "Back":
                        OnBackClicked?.Invoke();
                        break;
                    case "Save":
                        OnSaveClicked?.Invoke();
                        break;
                    case "Load":
                        OnLoadClicked?.Invoke();
                        break;
                    case "Sound":
                        OnSoundClicked?.Invoke();
                        break;
                    case "Debug":
                        OnDebugClicked?.Invoke();
                        break;
                    case "Feedback":
                        OnFeedbackClicked?.Invoke();
                        break;
                }
            });

            // Track state
            bool isHovering = false;
            bool isPressed = false;

            // Event trigger for hover and press states
            var eventTrigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // Pointer enter - just track hover state (no visual change)
            var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) =>
            {
                isHovering = true;
            });
            eventTrigger.triggers.Add(entryEnter);

            // Pointer exit - normal (flat/transparent)
            var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) =>
            {
                isHovering = false;
                if (!isPressed)
                {
                    text.color = Win95Theme.WindowText;
                    btn.image.color = Color.clear;  // Back to transparent (flat)
                }
                RemoveSunkenBorder(btn.gameObject);
            });
            eventTrigger.triggers.Add(entryExit);

            // Pointer down - pressed state (light gray + sunken border)
            var entryDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) =>
            {
                isPressed = true;
                text.color = Win95Theme.WindowText;  // Black text when pressed
                btn.image.color = Win95Theme.ColorLightGray;  // Light gray background
                CreateSunkenBorder(btn.gameObject);
            });
            eventTrigger.triggers.Add(entryDown);

            // Pointer up - return to normal state
            var entryUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) =>
            {
                isPressed = false;
                RemoveSunkenBorder(btn.gameObject);
                text.color = Win95Theme.WindowText;
                btn.image.color = Color.clear;
            });
            eventTrigger.triggers.Add(entryUp);
        }

        private void CreateSunkenBorder(GameObject button)
        {
            // Remove existing border first
            RemoveSunkenBorder(button);

            // Create border container
            GameObject borderContainer = new GameObject("SunkenBorder");
            borderContainer.transform.SetParent(button.transform, false);

            RectTransform borderRect = borderContainer.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;

            // Top border (dark gray) - 2px
            CreateBorderLine(borderContainer, "Top",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, -2),
                Win95Theme.SunkenBorderDarkInner);

            // Left border (dark gray) - 2px
            CreateBorderLine(borderContainer, "Left",
                new Vector2(0, 0), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(2, 0),
                Win95Theme.SunkenBorderDarkInner);

            // Bottom border (white) - 2px
            CreateBorderLine(borderContainer, "Bottom",
                new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 2),
                Win95Theme.SunkenBorderLightInner);

            // Right border (white) - 2px
            CreateBorderLine(borderContainer, "Right",
                new Vector2(1, 0), new Vector2(1, 1),
                new Vector2(-2, 0), new Vector2(0, 0),
                Win95Theme.SunkenBorderLightInner);
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

        private void RemoveSunkenBorder(GameObject button)
        {
            Transform existingBorder = button.transform.Find("SunkenBorder");
            if (existingBorder != null)
            {
                Destroy(existingBorder.gameObject);
            }
        }

        private void CreateBottomBorder()
        {
            // Shadow line at bottom
            GameObject shadowLine = new GameObject("BottomShadow");
            shadowLine.transform.SetParent(transform.parent, false);

            RectTransform shadowRect = shadowLine.AddComponent<RectTransform>();
            shadowRect.anchorMin = new Vector2(0, 0);
            shadowRect.anchorMax = new Vector2(1, 0);
            shadowRect.pivot = new Vector2(0.5f, 1);
            shadowRect.anchoredPosition = Vector2.zero;
            shadowRect.sizeDelta = new Vector2(0, 2);

            Image shadowImage = shadowLine.AddComponent<Image>();
            shadowImage.color = Win95Theme.ButtonShadow;
            shadowImage.raycastTarget = false;

            // Highlight line below shadow
            GameObject highlightLine = new GameObject("BottomHighlight");
            highlightLine.transform.SetParent(transform.parent, false);

            RectTransform highlightRect = highlightLine.AddComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0, 0);
            highlightRect.anchorMax = new Vector2(1, 0);
            highlightRect.pivot = new Vector2(0.5f, 1);
            highlightRect.anchoredPosition = new Vector2(0, -2);
            highlightRect.sizeDelta = new Vector2(0, 2);

            Image highlightImage = highlightLine.AddComponent<Image>();
            highlightImage.color = Win95Theme.ButtonHighlight;
            highlightImage.raycastTarget = false;
        }

        /// <summary>
        /// Enable or disable a specific menu item.
        /// </summary>
        public void SetMenuItemEnabled(string itemName, bool enabled)
        {
            int index = menuItems.IndexOf(itemName);
            if (index >= 0 && index < menuButtons.Count)
            {
                menuButtons[index].interactable = enabled;
            }
        }

        /// <summary>
        /// Get a reference to a menu button by name.
        /// </summary>
        public Button GetMenuButton(string itemName)
        {
            int index = menuItems.IndexOf(itemName);
            if (index >= 0 && index < menuButtons.Count)
            {
                return menuButtons[index];
            }
            return null;
        }
    }
}
