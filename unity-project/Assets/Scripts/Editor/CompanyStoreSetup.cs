using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Editor script to set up the Company Store UI.
/// Usage: Unity Menu -> Tools -> Setup Company Store
/// </summary>
public class CompanyStoreSetup : EditorWindow
{
    private static readonly string[] itemIds = {
        "item_mental_break",
        "item_blackout_curtains",
        "item_blue_light_filter",
        "item_screen_protector",
        "item_priority_shipping",
        "item_bow_for_alice",
        "item_corporate_bond"
    };

    private static readonly string[] itemNames = {
        "Mental Break",
        "Blackout Curtains",
        "Blue-Light Filter",
        "Screen Protector",
        "Priority Shipping Label",
        "Bow for Alice",
        "Corporate Bond"
    };

    private static readonly int[] itemCosts = { 10, 14, 16, 12, 18, 11, 10 };

    private static readonly string[] itemDescriptions = {
        "Give Timmy a breather. +10 Sanity immediately.",
        "Send Timmy blackout curtains. +14 Sanity now; -6 Engagement on the next node.",
        "Warm Timmy's screens. +15 Sanity tomorrow night; -1 Engagement each node tomorrow.",
        "Be less interesting to supervisors. Adds heat damping for the rest of the run.",
        "Parcel gets waved through. Unlock a dual escape during the mailroom scene.",
        "Cute accessory. +1 Engagement each time you choose a pro-engagement option.",
        "Earn 10% interest in one day. Not available going into Run 4."
    };

    [MenuItem("Tools/Setup Company Store")]
    public static void SetupCompanyStore()
    {
        // CRITICAL: Create our OWN Canvas to avoid being absorbed into DontDestroyOnLoad
        // The Dialogue System's Canvas moves to DontDestroyOnLoad at runtime, which breaks child panels
        // Using FindFirstObjectByType<Canvas>() was finding the Dialogue System's Canvas!

        Canvas storeCanvas = null;

        // Look for an existing "StoreCanvas" that we created previously
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name == "StoreCanvas")
            {
                storeCanvas = c;
                Debug.Log("Found existing StoreCanvas");
                break;
            }
        }

        // If not found, create a new dedicated canvas
        if (storeCanvas == null)
        {
            GameObject canvasObj = new GameObject("StoreCanvas");
            storeCanvas = canvasObj.AddComponent<Canvas>();
            storeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            storeCanvas.sortingOrder = 5000; // High sorting order to render on top of everything

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("Created dedicated StoreCanvas (won't be absorbed into DontDestroyOnLoad)");
        }

        // Ensure EventSystem exists
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem");
        }

        Canvas canvas = storeCanvas;

        // Check for existing StorePanel and remove it
        Transform existingPanel = canvas.transform.Find("CompanyStorePanel");
        if (existingPanel != null)
        {
            DestroyImmediate(existingPanel.gameObject);
            Debug.Log("Removed existing CompanyStorePanel");
        }

        // Create Store Panel
        GameObject panelObj = CreateStorePanel(canvas.transform);

        // Create all UI elements
        CreateStoreUI(panelObj.transform);

        // Setup CompanyStore component
        CompanyStore companyStore = panelObj.GetComponent<CompanyStore>();
        if (companyStore == null)
        {
            companyStore = panelObj.AddComponent<CompanyStore>();
        }

        // Wire up references
        WireReferences(panelObj, companyStore);

        // Set panel inactive initially
        panelObj.SetActive(false);

        Debug.Log("Company Store setup complete!");

        EditorUtility.DisplayDialog("Company Store Setup Complete",
            "Company Store UI has been created!\n\n" +
            "Next steps:\n" +
            "1. Check CompanyStorePanel in Canvas\n" +
            "2. Verify DialogueRunner is assigned\n" +
            "3. Add item icons (optional)\n" +
            "4. Test with <<store>> command",
            "OK");

        Selection.activeGameObject = panelObj;
    }

    private static GameObject CreateStorePanel(Transform parent)
    {
        GameObject panelObj = new GameObject("CompanyStorePanel");
        panelObj.transform.SetParent(parent, false);

        RectTransform rect = panelObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(700, 600);
        rect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

        // Add CanvasGroup for potential fading
        panelObj.AddComponent<CanvasGroup>();

        return panelObj;
    }

    private static void CreateStoreUI(Transform panelTransform)
    {
        // Header background
        GameObject headerBg = CreateUIElement(panelTransform, "HeaderBackground", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -50), new Vector2(0, 50));
        headerBg.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.18f, 1f);

        // Title text
        GameObject titleObj = CreateUIElement(panelTransform, "TitleText", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -25), new Vector2(-60, 40));
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "COMPANY STORE";
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // Close button (X)
        GameObject closeBtn = CreateUIElement(panelTransform, "CloseButton", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-25, -25), new Vector2(40, 40));
        Image closeBtnImage = closeBtn.AddComponent<Image>();
        closeBtnImage.color = new Color(0.7f, 0.2f, 0.2f, 1f);
        Button closeButton = closeBtn.AddComponent<Button>();
        closeButton.targetGraphic = closeBtnImage;

        GameObject closeText = new GameObject("CloseText");
        closeText.transform.SetParent(closeBtn.transform, false);
        RectTransform closeTextRect = closeText.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI closeTMP = closeText.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 24;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;

        // Credits text
        GameObject creditsObj = CreateUIElement(panelTransform, "CreditsText", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -70), new Vector2(-20, 30));
        TextMeshProUGUI creditsText = creditsObj.AddComponent<TextMeshProUGUI>();
        creditsText.text = "Credits: 0";
        creditsText.fontSize = 22;
        creditsText.alignment = TextAlignmentOptions.Center;
        creditsText.color = new Color(0.4f, 0.9f, 0.4f, 1f);

        // Scroll View for items
        GameObject scrollView = CreateScrollView(panelTransform);

        // Create item rows inside scroll content
        Transform content = scrollView.transform.Find("Viewport/Content");
        for (int i = 0; i < 7; i++)
        {
            CreateItemRow(content, i);
        }

        // Feedback text
        GameObject feedbackObj = CreateUIElement(panelTransform, "FeedbackText", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 70), new Vector2(-20, 30));
        TextMeshProUGUI feedbackText = feedbackObj.AddComponent<TextMeshProUGUI>();
        feedbackText.text = "";
        feedbackText.fontSize = 18;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.color = Color.green;

        // Leave Store button
        GameObject leaveBtn = CreateUIElement(panelTransform, "LeaveButton", new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(200, 45));
        Image leaveBtnImage = leaveBtn.AddComponent<Image>();
        leaveBtnImage.color = new Color(0.3f, 0.3f, 0.4f, 1f);
        Button leaveButton = leaveBtn.AddComponent<Button>();
        leaveButton.targetGraphic = leaveBtnImage;

        ColorBlock leaveColors = leaveButton.colors;
        leaveColors.highlightedColor = new Color(0.4f, 0.4f, 0.5f, 1f);
        leaveColors.pressedColor = new Color(0.25f, 0.25f, 0.35f, 1f);
        leaveButton.colors = leaveColors;

        GameObject leaveText = new GameObject("LeaveText");
        leaveText.transform.SetParent(leaveBtn.transform, false);
        RectTransform leaveTextRect = leaveText.AddComponent<RectTransform>();
        leaveTextRect.anchorMin = Vector2.zero;
        leaveTextRect.anchorMax = Vector2.one;
        leaveTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI leaveTMP = leaveText.AddComponent<TextMeshProUGUI>();
        leaveTMP.text = "LEAVE STORE";
        leaveTMP.fontSize = 18;
        leaveTMP.fontStyle = FontStyles.Bold;
        leaveTMP.alignment = TextAlignmentOptions.Center;
        leaveTMP.color = Color.white;
    }

    private static GameObject CreateScrollView(Transform parent)
    {
        // Scroll View container
        GameObject scrollView = CreateUIElement(parent, "ScrollView", new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0), new Vector2(-40, -160));
        RectTransform scrollRect = scrollView.GetComponent<RectTransform>();
        scrollRect.anchoredPosition = new Vector2(0, -15);

        Image scrollBg = scrollView.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0);

        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        GameObject viewport = CreateUIElement(scrollView.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        viewport.AddComponent<RectMask2D>();
        scroll.viewport = viewport.GetComponent<RectTransform>();

        // Content
        GameObject content = CreateUIElement(viewport.transform, "Content", new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 0));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.pivot = new Vector2(0.5f, 1);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;

        return scrollView;
    }

    private static void CreateItemRow(Transform parent, int index)
    {
        GameObject row = new GameObject($"ItemRow_{index}");
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0, 80);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        CanvasGroup canvasGroup = row.AddComponent<CanvasGroup>();

        // Icon placeholder
        GameObject iconObj = CreateUIElement(row.transform, "Icon", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(45, 0), new Vector2(60, 60));
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(0.4f, 0.4f, 0.5f, 1f);

        // Item name
        GameObject nameObj = CreateUIElement(row.transform, "NameText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(90, -8), new Vector2(250, 25));
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.pivot = new Vector2(0, 1);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = itemNames[index];
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.color = Color.white;

        // Cost text
        GameObject costObj = CreateUIElement(row.transform, "CostText", new Vector2(0, 1), new Vector2(0, 1), new Vector2(350, -8), new Vector2(120, 25));
        RectTransform costRect = costObj.GetComponent<RectTransform>();
        costRect.pivot = new Vector2(0, 1);
        TextMeshProUGUI costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.text = $"{itemCosts[index]} credits";
        costText.fontSize = 16;
        costText.alignment = TextAlignmentOptions.Left;
        costText.color = new Color(0.4f, 0.9f, 0.4f, 1f);

        // Description text
        GameObject descObj = CreateUIElement(row.transform, "DescriptionText", new Vector2(0, 0), new Vector2(1, 0), new Vector2(90, 8), new Vector2(-180, 40));
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.pivot = new Vector2(0, 0);
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(1, 0);
        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = itemDescriptions[index];
        descText.fontSize = 13;
        descText.alignment = TextAlignmentOptions.Left;
        descText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        descText.enableWordWrapping = true;

        // Buy button
        GameObject buyBtn = CreateUIElement(row.transform, "BuyButton", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(70, 35));
        Image buyBtnImage = buyBtn.AddComponent<Image>();
        buyBtnImage.color = new Color(0.2f, 0.5f, 0.2f, 1f);
        Button buyButton = buyBtn.AddComponent<Button>();
        buyButton.targetGraphic = buyBtnImage;

        ColorBlock buyColors = buyButton.colors;
        buyColors.highlightedColor = new Color(0.3f, 0.6f, 0.3f, 1f);
        buyColors.pressedColor = new Color(0.15f, 0.4f, 0.15f, 1f);
        buyColors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        buyButton.colors = buyColors;

        GameObject buyText = new GameObject("BuyText");
        buyText.transform.SetParent(buyBtn.transform, false);
        RectTransform buyTextRect = buyText.AddComponent<RectTransform>();
        buyTextRect.anchorMin = Vector2.zero;
        buyTextRect.anchorMax = Vector2.one;
        buyTextRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI buyTMP = buyText.AddComponent<TextMeshProUGUI>();
        buyTMP.text = "BUY";
        buyTMP.fontSize = 14;
        buyTMP.fontStyle = FontStyles.Bold;
        buyTMP.alignment = TextAlignmentOptions.Center;
        buyTMP.color = Color.white;
    }

    private static GameObject CreateUIElement(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        return obj;
    }

    private static void WireReferences(GameObject panelObj, CompanyStore companyStore)
    {
        Transform panel = panelObj.transform;

        // Store panel reference
        companyStore.storePanel = panelObj;

        // Title text
        Transform titleTransform = panel.Find("TitleText");
        if (titleTransform != null)
        {
            companyStore.titleText = titleTransform.GetComponent<TextMeshProUGUI>();
        }

        // Credits text
        Transform creditsTransform = panel.Find("CreditsText");
        if (creditsTransform != null)
        {
            companyStore.creditsText = creditsTransform.GetComponent<TextMeshProUGUI>();
        }

        // Close button
        Transform closeTransform = panel.Find("CloseButton");
        if (closeTransform != null)
        {
            companyStore.closeButton = closeTransform.GetComponent<Button>();
        }

        // Leave button
        Transform leaveTransform = panel.Find("LeaveButton");
        if (leaveTransform != null)
        {
            companyStore.leaveButton = leaveTransform.GetComponent<Button>();
        }

        // Feedback text
        Transform feedbackTransform = panel.Find("FeedbackText");
        if (feedbackTransform != null)
        {
            companyStore.feedbackText = feedbackTransform.GetComponent<TextMeshProUGUI>();
        }

        // Find DialogueRunner
        DialogueRunner dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        if (dialogueRunner != null)
        {
            companyStore.dialogueRunner = dialogueRunner;
            Debug.Log("Assigned DialogueRunner to CompanyStore");
        }
        else
        {
            Debug.LogWarning("DialogueRunner not found. Please assign it manually.");
        }

        // Wire up item rows
        companyStore.itemRows.Clear();
        Transform content = panel.Find("ScrollView/Viewport/Content");
        if (content != null)
        {
            for (int i = 0; i < 7; i++)
            {
                Transform rowTransform = content.Find($"ItemRow_{i}");
                if (rowTransform != null)
                {
                    CompanyStore.StoreItemRow row = new CompanyStore.StoreItemRow();
                    row.itemId = itemIds[i];
                    row.iconImage = rowTransform.Find("Icon")?.GetComponent<Image>();
                    row.nameText = rowTransform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                    row.costText = rowTransform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
                    row.descriptionText = rowTransform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
                    row.buyButton = rowTransform.Find("BuyButton")?.GetComponent<Button>();
                    row.canvasGroup = rowTransform.GetComponent<CanvasGroup>();

                    companyStore.itemRows.Add(row);
                }
            }
            Debug.Log($"Wired {companyStore.itemRows.Count} item rows");
        }

        EditorUtility.SetDirty(companyStore);
    }
}
