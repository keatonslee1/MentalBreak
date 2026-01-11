using UnityEngine;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Runtime connector that wires Win95 menu bar events to game systems.
    /// Attached to the Win95WindowFrame GameObject.
    /// </summary>
    public class Win95MenuConnector : MonoBehaviour
    {
        private Win95WindowFrame windowFrame;
        private PauseMenuManager pauseMenuManager;

        private void Start()
        {
            windowFrame = GetComponent<Win95WindowFrame>();
            pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
            WireEvents();
        }

        private void WireEvents()
        {
            if (windowFrame == null || windowFrame.MenuBarComponent == null)
            {
                Debug.LogError("Win95MenuConnector: WindowFrame or MenuBar not found.");
                return;
            }

            var menuBar = windowFrame.MenuBarComponent;

            // Back - DialogueDebugNavigator
            menuBar.OnBackClicked += () =>
            {
                Debug.Log("Back clicked");
                var nav = FindFirstObjectByType<DialogueDebugNavigator>();
                if (nav != null && nav.GetHistoryCount() > 1)
                    nav.GoBack();
            };

            // Save - direct call to PauseMenuManager
            menuBar.OnSaveClicked += () =>
            {
                Debug.Log("Save clicked");
                pauseMenuManager?.OnSaveGame();
            };

            // Load - direct call to PauseMenuManager
            menuBar.OnLoadClicked += () =>
            {
                Debug.Log("Load clicked");
                pauseMenuManager?.OnLoadGame();
            };

            // Sound/Settings - use Win95SettingsPanel directly
            menuBar.OnSoundClicked += () =>
            {
                Debug.Log("Sound/Settings clicked");
                var settingsPanel = FindFirstObjectByType<Win95SettingsPanel>();
                if (settingsPanel == null)
                {
                    // Find canvas to parent the settings panel
                    var canvas = FindFirstObjectByType<Canvas>();
                    if (canvas != null)
                    {
                        settingsPanel = Win95SettingsPanel.Create(canvas.transform);
                    }
                }
                settingsPanel?.Show();
            };

            // Debug - direct call to PauseMenuManager
            menuBar.OnDebugClicked += () =>
            {
                Debug.Log("Debug clicked");
                pauseMenuManager?.OnDebugMenu();
            };

            // Feedback - FeedbackForm
            menuBar.OnFeedbackClicked += () =>
            {
                Debug.Log("Feedback clicked");
                var feedbackForm = FindFirstObjectByType<FeedbackForm>();
                feedbackForm?.Open();
            };

            Debug.Log("Win95 Menu events wired successfully.");
        }
    }
}
