using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using System.Collections.Generic;
using System.Linq;
using MentalBreak.UI.Win95;

/// <summary>
/// Windows 95 styled leaderboard displaying Employee of the Month rankings.
/// Shows top 3 fake entries plus the player's entry with their current rank.
/// </summary>
public class Win95Leaderboard : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The DialogueRunner to read variables from")]
    public DialogueRunner dialogueRunner;

    [Header("Settings")]
    [Tooltip("Update interval in seconds")]
    public float updateInterval = 0.1f;

    [Tooltip("Font size for leaderboard entries")]
    public int fontSize = 36;

    [Header("Layout")]
    public float panelWidth = 720f;
    public float panelHeight = 400f;
    public float topMargin = 16f;
    public float titleBarHeight = 48f;
    public float entryHeight = 44f;
    public float entrySpacing = 4f;

    private VariableStorageBehaviour variableStorage;
    private DialogueRuntimeWatcher runtimeWatcher;
    private float lastUpdateTime = 0f;

    // UI References
    private GameObject leaderboardRoot;
    private GameObject listArea;
    private Canvas canvas;
    private List<GameObject> entryObjects = new List<GameObject>();

    // Column widths (2x scale)
    private const float RankWidth = 100f;
    private const float NameWidth = 300f;
    private const float EngagementWidth = 120f;
    private const float SanityWidth = 120f;

    // Fake employee data
    private class LeaderboardEntry
    {
        public int rank;
        public string name;
        public float engagement;
        public float sanity;

        public LeaderboardEntry(int rank, string name, float engagement, float sanity)
        {
            this.rank = rank;
            this.name = name;
            this.engagement = engagement;
            this.sanity = sanity;
        }
    }

    private List<LeaderboardEntry> fakeEntries = new List<LeaderboardEntry>();
    private bool isInitialized = false;

    private void Awake()
    {
        GenerateFakeEntries();
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            CreateUI();
            isInitialized = true;
        }

        runtimeWatcher = DialogueRuntimeWatcher.Instance;
        runtimeWatcher.Register(OnRuntimeReady, OnRuntimeLost);

        if (!runtimeWatcher.HasRuntime)
        {
            ApplyLoadingState();
        }
    }

    private void OnDisable()
    {
        if (runtimeWatcher != null)
        {
            runtimeWatcher.Unregister(OnRuntimeReady, OnRuntimeLost);
            runtimeWatcher = null;
        }
    }

    private void Start()
    {
        if (dialogueRunner == null)
        {
            dialogueRunner = FindAnyObjectByType<DialogueRunner>();
        }

        if (dialogueRunner != null)
        {
            variableStorage = dialogueRunner.VariableStorage;
        }

        if (!isInitialized)
        {
            CreateUI();
            isInitialized = true;
        }

        if (variableStorage == null)
        {
            ApplyLoadingState();
        }
        else
        {
            UpdateLeaderboard();
        }
    }

    private void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateLeaderboard();
            lastUpdateTime = Time.time;
        }
    }

    private void OnRuntimeReady(DialogueRunner runner, VariableStorageBehaviour storage)
    {
        if (runner != null)
        {
            dialogueRunner = runner;
        }
        variableStorage = storage;
        UpdateLeaderboard();
    }

    private void OnRuntimeLost()
    {
        variableStorage = null;
        ApplyLoadingState();
    }

    private void GenerateFakeEntries()
    {
        fakeEntries.Clear();
        fakeEntries.Add(new LeaderboardEntry(1, "Sarah Chen", 92f, 45f));
        fakeEntries.Add(new LeaderboardEntry(2, "Marcus Smith", 88f, 38f));
        fakeEntries.Add(new LeaderboardEntry(3, "Calan Fields", 87f, 42f));

        fakeEntries = fakeEntries.OrderByDescending(e => e.engagement).ToList();
        for (int i = 0; i < fakeEntries.Count; i++)
        {
            fakeEntries[i].rank = i + 1;
        }
    }

    private void CreateUI()
    {
        // Use deterministic overlay canvas
        canvas = OverlayCanvasProvider.GetCanvas();
        if (canvas == null)
        {
            Debug.LogError("Win95Leaderboard: OverlayCanvasProvider returned null canvas.");
            return;
        }

        // Create root container
        leaderboardRoot = new GameObject("Win95LeaderboardRoot");
        leaderboardRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = leaderboardRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0, -topMargin);
        rootRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // Add Win95 raised panel background
        Image bgImage = leaderboardRoot.AddComponent<Image>();
        bgImage.color = Win95Theme.WindowBackground;
        bgImage.raycastTarget = false;

        // Create title bar (Win95 style)
        CreateTitleBar();

        // Create sunken list area
        CreateListArea();

        // Add Win95 raised border to root
        Win95Panel.Create(leaderboardRoot, Win95Panel.PanelStyle.Raised);
    }

    private void CreateTitleBar()
    {
        GameObject titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(leaderboardRoot.transform, false);

        RectTransform titleRect = titleBar.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -4);
        titleRect.sizeDelta = new Vector2(-8, titleBarHeight);

        // Title bar background (active blue)
        Image titleBg = titleBar.AddComponent<Image>();
        titleBg.color = Win95Theme.ColorTitleActive;
        titleBg.raycastTarget = false;

        // Title text
        GameObject titleTextObj = new GameObject("TitleText");
        titleTextObj.transform.SetParent(titleBar.transform, false);

        RectTransform textRect = titleTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8, 0);
        textRect.offsetMax = new Vector2(-8, 0);

        TMP_Text titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Employee of the Month";
        titleText.fontSize = fontSize;
        titleText.color = Win95Theme.TitleBarText;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;
    }

    private void CreateListArea()
    {
        listArea = new GameObject("ListArea");
        listArea.transform.SetParent(leaderboardRoot.transform, false);

        RectTransform listRect = listArea.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 1);
        listRect.offsetMin = new Vector2(8, 8);
        listRect.offsetMax = new Vector2(-8, -(titleBarHeight + 12));

        // White background for list area (Win95 list style)
        Image listBg = listArea.AddComponent<Image>();
        listBg.color = Color.white;
        listBg.raycastTarget = false;

        // Add sunken border effect
        CreateSunkenBorder(listArea);

        // Create header row
        CreateHeaderRow();
    }

    private void CreateSunkenBorder(GameObject parent)
    {
        // Top shadow - 2px
        CreateBorderLine(parent, "TopShadow", new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, 0), new Vector2(0, -2), Win95Theme.ButtonShadow);

        // Left shadow - 2px
        CreateBorderLine(parent, "LeftShadow", new Vector2(0, 0), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(2, 0), Win95Theme.ButtonShadow);

        // Bottom highlight - 2px
        CreateBorderLine(parent, "BottomHighlight", new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 0), new Vector2(0, 2), Win95Theme.ButtonHighlight);

        // Right highlight - 2px
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

    private void CreateHeaderRow()
    {
        GameObject header = new GameObject("Header");
        header.transform.SetParent(listArea.transform, false);

        RectTransform headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = new Vector2(0, -4);
        headerRect.sizeDelta = new Vector2(-8, entryHeight);

        // Header background (slightly gray)
        Image headerBg = header.AddComponent<Image>();
        headerBg.color = Win95Theme.WindowBackground;
        headerBg.raycastTarget = false;

        // Horizontal layout
        HorizontalLayoutGroup hlg = header.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 0, 0);
        hlg.spacing = 4;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Create header columns
        int headerFontSize = fontSize - 2;
        CreateColumnText(header.transform, "#", headerFontSize, Win95Theme.WindowText, TextAlignmentOptions.Center, RankWidth);
        CreateColumnText(header.transform, "Name", headerFontSize, Win95Theme.WindowText, TextAlignmentOptions.MidlineLeft, NameWidth, true);
        CreateColumnText(header.transform, "Eng.", headerFontSize, Win95Theme.WindowText, TextAlignmentOptions.MidlineRight, EngagementWidth);
        CreateColumnText(header.transform, "San.", headerFontSize, Win95Theme.WindowText, TextAlignmentOptions.MidlineRight, SanityWidth);
    }

    private void CreateColumnText(Transform parent, string text, int size, Color color,
        TextAlignmentOptions alignment, float width, bool flexible = false)
    {
        GameObject obj = new GameObject(text);
        obj.transform.SetParent(parent, false);

        obj.AddComponent<RectTransform>();

        LayoutElement layout = obj.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width * 0.8f;
        layout.flexibleWidth = flexible ? 1f : 0f;

        TMP_Text tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.raycastTarget = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void UpdateLeaderboard()
    {
        if (variableStorage == null)
        {
            ApplyLoadingState();
            return;
        }

        // Get player's current values
        float playerEngagement = 0f;
        float playerSanity = 0f;

        variableStorage.TryGetValue<float>("$engagement", out playerEngagement);
        variableStorage.TryGetValue<float>("$sanity", out playerSanity);

        // Create combined list with player entry
        List<LeaderboardEntry> allEntries = new List<LeaderboardEntry>(fakeEntries);
        allEntries.Add(new LeaderboardEntry(0, "You", playerEngagement, playerSanity));

        // Sort by engagement descending
        allEntries = allEntries.OrderByDescending(e => e.engagement).ToList();

        // Reassign ranks
        for (int i = 0; i < allEntries.Count; i++)
        {
            allEntries[i].rank = i + 1;
        }

        // Get top 3 + player entry
        List<LeaderboardEntry> displayEntries = new List<LeaderboardEntry>();

        // Add top 3
        for (int i = 0; i < Mathf.Min(3, allEntries.Count); i++)
        {
            displayEntries.Add(allEntries[i]);
        }

        // Find player entry and add if not in top 3
        LeaderboardEntry playerEntry = allEntries.FirstOrDefault(e => e.name == "You");
        if (playerEntry != null && playerEntry.rank > 3)
        {
            displayEntries.Add(playerEntry);
        }

        // Update UI
        UpdateUIEntries(displayEntries);
    }

    private void UpdateUIEntries(List<LeaderboardEntry> entries)
    {
        ClearEntryObjects();

        float yOffset = entryHeight + 8; // Start below header (2x scale)

        foreach (LeaderboardEntry entry in entries)
        {
            GameObject entryObj = CreateEntryObject(entry, yOffset);
            entryObjects.Add(entryObj);
            yOffset += entryHeight + entrySpacing;
        }
    }

    private void ClearEntryObjects()
    {
        foreach (GameObject entryObj in entryObjects)
        {
            if (entryObj != null)
            {
                Destroy(entryObj);
            }
        }
        entryObjects.Clear();
    }

    private GameObject CreateEntryObject(LeaderboardEntry entry, float yOffset)
    {
        GameObject entryObj = new GameObject($"Entry_{entry.rank}");
        entryObj.transform.SetParent(listArea.transform, false);

        RectTransform entryRect = entryObj.AddComponent<RectTransform>();
        entryRect.anchorMin = new Vector2(0, 1);
        entryRect.anchorMax = new Vector2(1, 1);
        entryRect.pivot = new Vector2(0.5f, 1);
        entryRect.anchoredPosition = new Vector2(0, -yOffset);
        entryRect.sizeDelta = new Vector2(-8, entryHeight);

        // Highlight player row
        bool isPlayer = entry.name == "You";
        if (isPlayer)
        {
            Image rowBg = entryObj.AddComponent<Image>();
            rowBg.color = Win95Theme.ColorTitleActive; // Blue highlight for player
            rowBg.raycastTarget = false;
        }

        // Horizontal layout
        HorizontalLayoutGroup hlg = entryObj.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 0, 0);
        hlg.spacing = 4;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Text color (white for player, black for others)
        Color textColor = isPlayer ? Color.white : Win95Theme.WindowText;

        // Create columns
        CreateColumnText(entryObj.transform, entry.rank.ToString(), fontSize, textColor, TextAlignmentOptions.Center, RankWidth);
        CreateColumnText(entryObj.transform, entry.name, fontSize, textColor, TextAlignmentOptions.MidlineLeft, NameWidth, true);
        CreateColumnText(entryObj.transform, entry.engagement.ToString("F0"), fontSize, textColor, TextAlignmentOptions.MidlineRight, EngagementWidth);
        CreateColumnText(entryObj.transform, entry.sanity.ToString("F0"), fontSize, textColor, TextAlignmentOptions.MidlineRight, SanityWidth);

        return entryObj;
    }

    private void ApplyLoadingState()
    {
        ClearEntryObjects();

        if (listArea == null) return;

        GameObject statusObj = new GameObject("LoadingStatus");
        statusObj.transform.SetParent(listArea.transform, false);

        RectTransform statusRect = statusObj.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.5f);
        statusRect.anchorMax = new Vector2(1, 0.5f);
        statusRect.pivot = new Vector2(0.5f, 0.5f);
        statusRect.anchoredPosition = Vector2.zero;
        statusRect.sizeDelta = new Vector2(0, entryHeight);

        TMP_Text statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Loading...";
        statusText.fontSize = fontSize;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Win95Theme.ColorMidGray;
        statusText.raycastTarget = false;

        entryObjects.Add(statusObj);
    }

    /// <summary>
    /// Force an immediate update.
    /// </summary>
    public void ForceUpdate()
    {
        UpdateLeaderboard();
    }

    private void OnDestroy()
    {
        if (runtimeWatcher != null)
        {
            runtimeWatcher.Unregister(OnRuntimeReady, OnRuntimeLost);
        }
        ClearEntryObjects();
    }
}
