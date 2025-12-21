using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Self-contained Company Store for purchasing items.
/// Called via <<store>> Yarn command.
/// </summary>
public class CompanyStore : MonoBehaviour
{
    [System.Serializable]
    public class StoreItemData
    {
        public string id;
        public string displayName;
        public int cost;
        [TextArea(2, 3)]
        public string description;
        public Sprite icon;
    }

    [System.Serializable]
    public class StoreItemRow
    {
        public string itemId;
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI descriptionText;
        public Button buyButton;
        public CanvasGroup canvasGroup;
    }

    [Header("Store Items")]
    public List<StoreItemData> items = new List<StoreItemData>
    {
        new StoreItemData { id = "item_mental_break", displayName = "Mental Break", cost = 10, description = "Give Timmy a breather. +10 Sanity immediately." },
        new StoreItemData { id = "item_blackout_curtains", displayName = "Blackout Curtains", cost = 14, description = "Send Timmy blackout curtains. +14 Sanity now; -6 Engagement on the next node." },
        new StoreItemData { id = "item_blue_light_filter", displayName = "Blue-Light Filter", cost = 16, description = "Warm Timmy's screens. +15 Sanity tomorrow night; -1 Engagement each node tomorrow." },
        new StoreItemData { id = "item_screen_protector", displayName = "Screen Protector", cost = 12, description = "Be less interesting to supervisors. Adds heat damping for the rest of the run." },
        new StoreItemData { id = "item_priority_shipping", displayName = "Priority Shipping Label", cost = 18, description = "Parcel gets waved through. Unlock a dual escape during the mailroom scene." },
        new StoreItemData { id = "item_bow_for_alice", displayName = "Bow for Alice", cost = 11, description = "Cute accessory. +1 Engagement each time you choose a pro-engagement option." },
        new StoreItemData { id = "item_corporate_bond", displayName = "Corporate Bond", cost = 10, description = "Earn 10% interest in one day. Not available going into Run 4." }
    };

    [Header("UI References")]
    public GameObject storePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI creditsText;
    public Button closeButton;
    public Button leaveButton;
    public TextMeshProUGUI feedbackText;

    [Header("Item Rows")]
    public List<StoreItemRow> itemRows = new List<StoreItemRow>();

    [Header("References")]
    public DialogueRunner dialogueRunner;

    private VariableStorageBehaviour variableStorage;
    private Dictionary<string, StoreItemData> itemLookup = new Dictionary<string, StoreItemData>();
    private bool isOpen = false;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        // Build lookup dictionary
        itemLookup.Clear();
        foreach (var item in items)
        {
            if (item != null && !string.IsNullOrEmpty(item.id))
            {
                itemLookup[item.id] = item;
            }
        }

        // Find DialogueRunner if not assigned
        if (dialogueRunner == null)
        {
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        if (dialogueRunner != null)
        {
            variableStorage = dialogueRunner.VariableStorage;
        }

        // Hide store panel initially
        if (storePanel != null)
        {
            storePanel.SetActive(false);
        }

        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseStore);
        }
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(CloseStore);
        }

        // Setup buy button listeners for each item row
        for (int i = 0; i < itemRows.Count; i++)
        {
            var row = itemRows[i];
            if (row.buyButton != null && !string.IsNullOrEmpty(row.itemId))
            {
                string itemId = row.itemId;
                row.buyButton.onClick.AddListener(() => PurchaseItem(itemId));
            }
        }
    }

    /// <summary>
    /// Opens the store UI. Called by Yarn <<store>> command.
    /// Returns IEnumerator so dialogue waits for store to close.
    /// Registered via CommandHandlerRegistrar (not via attribute to avoid target conflicts).
    /// </summary>
    public IEnumerator OpenStore()
    {
        Debug.Log($"[CompanyStore] Opening store on '{gameObject.name}'");

        isOpen = true;

        // Activate the panel - use storePanel if set, otherwise use this gameObject
        GameObject panelToActivate = (storePanel != null) ? storePanel : gameObject;
        Debug.Log($"[CompanyStore] Before SetActive: activeSelf={panelToActivate.activeSelf}, activeInHierarchy={panelToActivate.activeInHierarchy}");
        Debug.Log($"[CompanyStore] Panel instanceID={panelToActivate.GetInstanceID()}, scene={panelToActivate.scene.name}");

        panelToActivate.SetActive(true);

        Debug.Log($"[CompanyStore] After SetActive: activeSelf={panelToActivate.activeSelf}, activeInHierarchy={panelToActivate.activeInHierarchy}");
        Debug.Log($"[CompanyStore] Activated panel: {panelToActivate.name}");

        // Boost canvas sorting order to render on top of dialogue UI
        Canvas canvas = panelToActivate.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 5000;
            Debug.Log("[CompanyStore] Boosted canvas sorting order to 5000");
        }

        RefreshDisplay();

        // Wait until store is closed
        while (isOpen)
        {
            yield return null;
        }

        Debug.Log("[CompanyStore] Store closed, resuming dialogue");
    }

    /// <summary>
    /// Closes the store UI.
    /// </summary>
    public void CloseStore()
    {
        Debug.Log("[CompanyStore] Closing store");

        isOpen = false;

        // Deactivate the panel - use storePanel if set, otherwise use this gameObject
        GameObject panelToDeactivate = (storePanel != null) ? storePanel : gameObject;
        panelToDeactivate.SetActive(false);

        // Clear feedback
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }

    /// <summary>
    /// Refreshes the store display with current credits and item states.
    /// </summary>
    private void RefreshDisplay()
    {
        float cash = GetCash();

        // Update credits display
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {cash:F0}";
        }

        // Update each item row
        for (int i = 0; i < itemRows.Count; i++)
        {
            var row = itemRows[i];
            if (string.IsNullOrEmpty(row.itemId)) continue;

            if (!itemLookup.TryGetValue(row.itemId, out StoreItemData item))
            {
                continue;
            }

            bool isOwned = IsItemOwned(item.id);
            bool isAvailable = IsItemAvailable(item.id);
            bool canAfford = cash >= item.cost;

            // Update text
            if (row.nameText != null)
            {
                row.nameText.text = item.displayName;
            }
            if (row.costText != null)
            {
                if (isOwned)
                {
                    row.costText.text = "OWNED";
                }
                else
                {
                    row.costText.text = $"{item.cost} credits";
                }
            }
            if (row.descriptionText != null)
            {
                row.descriptionText.text = item.description;
            }
            if (row.iconImage != null && item.icon != null)
            {
                row.iconImage.sprite = item.icon;
            }

            // Update buy button
            if (row.buyButton != null)
            {
                bool canBuy = !isOwned && isAvailable && canAfford;
                row.buyButton.interactable = canBuy;

                // Update button text
                var buttonText = row.buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (isOwned)
                    {
                        buttonText.text = "OWNED";
                    }
                    else if (!isAvailable)
                    {
                        buttonText.text = "N/A";
                    }
                    else if (!canAfford)
                    {
                        buttonText.text = "BUY";
                    }
                    else
                    {
                        buttonText.text = "BUY";
                    }
                }
            }

            // Gray out owned or unavailable items
            if (row.canvasGroup != null)
            {
                if (isOwned)
                {
                    row.canvasGroup.alpha = 0.5f;
                }
                else if (!isAvailable)
                {
                    row.canvasGroup.alpha = 0.4f;
                }
                else
                {
                    row.canvasGroup.alpha = 1f;
                }
            }
        }
    }

    /// <summary>
    /// Attempts to purchase an item.
    /// </summary>
    public void PurchaseItem(string itemId)
    {
        if (variableStorage == null)
        {
            ShowFeedback("Store not initialized!", Color.red);
            return;
        }

        if (!itemLookup.TryGetValue(itemId, out StoreItemData item))
        {
            ShowFeedback($"Item not found: {itemId}", Color.red);
            return;
        }

        // Check if already owned
        if (IsItemOwned(itemId))
        {
            ShowFeedback($"{item.displayName} is already owned!", Color.yellow);
            return;
        }

        // Check availability
        if (!IsItemAvailable(itemId))
        {
            ShowFeedback($"{item.displayName} is not available!", Color.yellow);
            return;
        }

        // Check affordability
        float cash = GetCash();
        if (cash < item.cost)
        {
            float shortfall = item.cost - cash;
            ShowFeedback($"Need {shortfall:F0} more credits!", Color.yellow);
            return;
        }

        // Deduct cost
        SetFloat("$rapid_feedback_cash", cash - item.cost);

        // Set ownership flag
        SetBool($"${itemId}", true);

        // Apply item effects
        ApplyItemEffects(item);

        Debug.Log($"[CompanyStore] Purchased {item.displayName} for {item.cost} credits");
        ShowFeedback($"Purchased {item.displayName}!", Color.green);

        // Refresh display to show updated state
        RefreshDisplay();
    }

    /// <summary>
    /// Applies immediate effects when an item is purchased.
    /// </summary>
    private void ApplyItemEffects(StoreItemData item)
    {
        int currentRun = GetCurrentRun();
        int currentDay = GetCurrentDay();

        switch (item.id)
        {
            case "item_mental_break":
                AdjustSanity(10f);
                break;

            case "item_blackout_curtains":
                AdjustSanity(14f);
                SetBool("$store_blackout_pending", true);
                break;

            case "item_blue_light_filter":
                SetBool("$store_blue_filter_active", true);
                int targetRun = currentRun;
                int targetDay = currentDay + 1;
                if (targetDay > 4)
                {
                    targetDay = 1;
                    targetRun += 1;
                }
                SetFloat("$store_blue_filter_target_run", targetRun);
                SetFloat("$store_blue_filter_target_day", targetDay);
                SetFloat("$store_blue_filter_penalties_applied", 0f);
                SetBool("$store_blue_filter_bonus_applied", false);
                break;

            case "item_screen_protector":
                SetFloat("$store_screen_protector_heat_modifier", -1f);
                break;

            case "item_priority_shipping":
                // Effect handled via ownership flag
                break;

            case "item_bow_for_alice":
                // Effect handled by engagement tracking system
                break;

            case "item_corporate_bond":
                SetBool("$store_corporate_bond_active", true);
                SetFloat("$store_corporate_bond_principal", item.cost);
                int bondTargetRun = currentRun;
                int bondTargetDay = currentDay + 1;
                if (bondTargetDay > 4)
                {
                    bondTargetDay = 1;
                    bondTargetRun += 1;
                }
                SetFloat("$store_corporate_bond_mature_run", bondTargetRun);
                SetFloat("$store_corporate_bond_mature_day", bondTargetDay);
                break;
        }
    }

    /// <summary>
    /// Shows feedback message to the player.
    /// </summary>
    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = color;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(ClearFeedbackAfterDelay(3f));
    }

    private IEnumerator ClearFeedbackAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
        feedbackCoroutine = null;
    }

    #region Yarn Variable Helpers

    private float GetCash()
    {
        return GetFloat("$rapid_feedback_cash", 0f);
    }

    private bool IsItemOwned(string itemId)
    {
        return GetBool($"${itemId}");
    }

    private bool IsItemAvailable(string itemId)
    {
        if (!itemLookup.ContainsKey(itemId)) return false;

        // Already owned items cannot be repurchased
        if (IsItemOwned(itemId)) return false;

        // Corporate Bond unavailable when entering Run 4 or if already active
        if (itemId == "item_corporate_bond")
        {
            int currentRun = GetCurrentRun();
            if (currentRun >= 4) return false;
            if (GetBool("$store_corporate_bond_active")) return false;
        }

        return true;
    }

    private int GetCurrentRun()
    {
        return Mathf.Max(1, Mathf.RoundToInt(GetFloat("$current_run", 1f)));
    }

    private int GetCurrentDay()
    {
        return Mathf.Max(1, Mathf.RoundToInt(GetFloat("$current_day", 1f)));
    }

    private float GetFloat(string variableName, float defaultValue = 0f)
    {
        if (variableStorage != null && variableStorage.TryGetValue<float>(variableName, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    private bool GetBool(string variableName)
    {
        if (variableStorage != null && variableStorage.TryGetValue<bool>(variableName, out var value))
        {
            return value;
        }
        return false;
    }

    private void SetFloat(string variableName, float value)
    {
        variableStorage?.SetValue(variableName, value);
    }

    private void SetBool(string variableName, bool value)
    {
        variableStorage?.SetValue(variableName, value);
    }

    private void AdjustSanity(float delta)
    {
        float sanity = GetFloat("$sanity", 0f);
        SetFloat("$sanity", sanity + delta);
    }

    #endregion
}
