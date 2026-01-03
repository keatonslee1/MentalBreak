using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Yarn.Unity;
using System.Collections.Generic;

/// <summary>
/// Minimal click-to-advance handler for dialogue.
/// Delegates to LineAdvancer for actual advancement logic (two-stage: hurry up, then advance).
/// </summary>
public class ClickAdvancer : MonoBehaviour
{
    [Tooltip("The LineAdvancer component that handles dialogue progression")]
    [SerializeField] private LineAdvancer lineAdvancer;

    private void Start()
    {
        // Find LineAdvancer if not assigned
        if (lineAdvancer == null)
        {
            lineAdvancer = FindFirstObjectByType<LineAdvancer>();
            if (lineAdvancer == null)
            {
                Debug.LogWarning("ClickAdvancer: LineAdvancer not found. Click-to-advance will not work.");
                enabled = false;
            }
        }
    }

    private void Update()
    {
        // Block input when modal overlays are active
        if (ModalInputLock.IsLocked) return;

        // Check for mouse click (using new Input System)
        var mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

        // Don't advance if clicking on interactive UI elements
        if (IsPointerOverInteractableUI()) return;

        // Delegate to LineAdvancer for two-stage advancement
        // (first click completes text, second click advances to next line)
        lineAdvancer?.RequestLineHurryUp();
    }

    /// <summary>
    /// Returns true if pointer is over an interactive UI element (Button, Toggle, OptionItem, etc.)
    /// These should NOT trigger dialogue advancement.
    /// </summary>
    private bool IsPointerOverInteractableUI()
    {
        if (EventSystem.current == null) return false;

        // Not over any UI? Allow click to advance dialogue.
        if (!EventSystem.current.IsPointerOverGameObject()) return false;

        // Raycast to find what's under the pointer (using new Input System)
        var mouse = Mouse.current;
        if (mouse == null) return false;

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = mouse.position.ReadValue()
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Block if over any Selectable (Button, Toggle, Slider, InputField, etc.)
            if (result.gameObject.GetComponent<Selectable>() != null)
                return true;

            // Block if over Yarn option items
            if (result.gameObject.GetComponent<OptionItem>() != null)
                return true;
        }

        // Over non-interactive UI (like dialogue box background) - allow click to advance
        return false;
    }
}
