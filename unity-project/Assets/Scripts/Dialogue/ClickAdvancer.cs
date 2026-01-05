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

        // Handle mouse click
        var mouse = Mouse.current;
        bool mouseClicked = mouse != null && mouse.leftButton.wasPressedThisFrame;

        // Handle touch (mobile)
        var touchscreen = Touchscreen.current;
        bool touchBegan = touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame;

        if (mouseClicked || touchBegan)
        {
            // Don't advance if clicking/tapping on interactive UI elements
            if (!IsPointerOverInteractableUI())
            {
                lineAdvancer?.RequestLineHurryUp();
                return;
            }
        }

        // Handle Space and Enter keys (LineAdvancer's keyboard handling is disabled)
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                lineAdvancer?.RequestLineHurryUp();
                return;
            }
        }
    }

    /// <summary>
    /// Returns true if pointer is over an interactive UI element (Button, Toggle, OptionItem, etc.)
    /// These should NOT trigger dialogue advancement.
    /// </summary>
    private bool IsPointerOverInteractableUI()
    {
        if (EventSystem.current == null) return false;

        // Not over any UI? Allow click/tap to advance dialogue.
        if (!EventSystem.current.IsPointerOverGameObject()) return false;

        // Get pointer position from touch or mouse
        Vector2 pointerPosition;
        var touchscreen = Touchscreen.current;
        var mouse = Mouse.current;

        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            pointerPosition = touchscreen.primaryTouch.position.ReadValue();
        }
        else if (mouse != null)
        {
            pointerPosition = mouse.position.ReadValue();
        }
        else
        {
            return false; // No input device available
        }

        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
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
