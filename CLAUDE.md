# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Mental Break is a Unity 6.2 WebGL narrative game deployed via Vercel. It uses Yarn Spinner for dialogue with custom dialogue wheel and speech bubble systems.

**Current Version**: 4.0.0 (Clean restructure from v3.5.3)

---

## IMPORTANT: Git Policy for Claude

**DO NOT push to git.** All pushing to GitHub must be done manually by the user via GitHub Desktop.

Claude is allowed to:
- `git add` (stage files)
- `git commit` (create commits)
- `git status`, `git log`, `git diff` (read operations)

Claude is **NOT allowed to**:
- `git push` (user will push manually via GitHub Desktop)
- `git push --force` or any force operations
- Any remote-modifying operations

This ensures the user maintains full control over what gets deployed to the remote repository.

---

## Directory Structure

```
mentalbreak/                               # Repository root
├── unity-project/                         # Unity project
│   ├── Assets/
│   │   ├── Dialogue/                      # .yarn files (numbered 01-99)
│   │   ├── Scripts/                       # C# game logic (organized)
│   │   │   ├── Core/                      # GameManager, SaveLoad, RunTransition
│   │   │   ├── Dialogue/                  # Dialogue handling, options, navigation
│   │   │   ├── UI/                        # All UI components
│   │   │   ├── Audio/                     # Audio command handler
│   │   │   ├── Characters/                # Portrait/sprite management
│   │   │   ├── Commands/                  # Yarn command handlers
│   │   │   └── Editor/                    # Editor-only utilities
│   │   ├── Graphics/                      # Backgrounds, portraits, UI
│   │   ├── Audio/                         # BGM and SFX assets
│   │   ├── Dialogue Wheel for Yarn Spinner/
│   │   ├── Speech Bubbles for Yarn Spinner/
│   │   ├── StreamingAssets/               # FMOD banks
│   │   └── Plugins/FMOD/                  # FMOD Unity integration
│   └── webgl-build/                       # Build output for Vercel
├── api/                                   # Vercel serverless functions
├── docs/                                  # Documentation
└── vercel.json                            # Deployment config
```

## Build & Deploy Commands

### Unity Build
1. Open Unity Hub, add project at `mentalbreak/unity-project/`
2. File > Build Settings > WebGL > Build
3. Output directly to `unity-project/webgl-build/`

### Deploy to Vercel
1. Claude stages and commits changes: `git add . && git commit -m "message"`
2. User pushes via **GitHub Desktop** (Claude does NOT push)
3. Vercel auto-deploys from main branch

### Local Testing
```powershell
cd C:\Users\epick\OneDrive\Documents\mentalbreak\unity-project\webgl-build
python -m http.server 8000
```

## Architecture

### Core Systems (Scripts/Core/)

**GameManager** (`Scripts/Core/GameManager.cs`)
- Central coordinator for game state and run transitions
- 4-run structure (R1_Start through R4_End)

**SaveLoadManager** (`Scripts/Core/SaveLoadManager.cs`)
- **5 Manual Slots** (indices 1-5): Player-controlled saves
- **3 Autosave Slots** (indices 0, -1, -2): FIFO rotation at checkpoints
  - Slot 0: Newest autosave
  - Slot -1: Middle autosave
  - Slot -2: Oldest autosave (overwritten on rotation)
- WebGL: PlayerPrefs (IndexedDB-backed)
- Desktop: JSON files in persistentDataPath
- **Export/Import**: Base64-encoded save strings for clipboard sharing

**SaveExporter** (`Scripts/Core/SaveExporter.cs`)
- Converts SaveData to/from Base64 strings
- Format: `MBSAVE_1_{base64data}`
- Used for cross-device save transfer

**CommandHandlerRegistrar** (`Scripts/Commands/CommandHandlerRegistrar.cs`)
- Registers Yarn commands with DialogueRunner
- Required for WebGL builds (attribute discovery fails)

### Script Organization

| Folder | Purpose | Key Files |
|--------|---------|-----------|
| Core/ | Game state, saves, settings | GameManager, SaveLoadManager, SaveExporter, SettingsManager |
| Dialogue/ | Dialogue flow | ClickAdvancer, OptionsInputHandler |
| UI/ | User interface | MenuManager, PauseMenuManager, SettingsPanel, StoreUI, ToastManager |
| Audio/ | Audio commands | AudioCommandHandler, FMODAudioManager, FMODWebGLBankLoader, MumbleDialogueController |
| Characters/ | Portraits | CharacterSpriteManager, PortraitTalkingStateController |
| Commands/ | Yarn handlers | BackgroundCommandHandler, CheckpointCommandHandler |
| Editor/ | Dev tools | DialogueSystemUIAutoWire, SettingsPanelSetup, DialogueFontSetup, PauseMenuSetup |

### Yarn Commands

| Command | Handler | Description |
|---------|---------|-------------|
| `<<bg key>>` | BackgroundCommandHandler | Background (video/static) |
| `<<bgm key>>` | AudioCommandHandler | Background music (legacy) |
| `<<sfx key>>` | AudioCommandHandler | Sound effect |
| `<<checkpoint id>>` | CheckpointCommandHandler | Autosave point |
| `<<store>>` | StoreUI | Open store UI |
| `<<music theme [loop]>>` | FMODAudioManager | Play theme (auto-selects A/B side) |
| `<<music_stop [immediate]>>` | FMODAudioManager | Stop music with fade |
| `<<fmod event [loop]>>` | FMODAudioManager | Play FMOD event directly |
| `<<fmod_stop [immediate]>>` | FMODAudioManager | Stop music (same as music_stop) |
| `<<fmod_loop loopName>>` | FMODAudioManager | Change loop (LoopA-D) |
| `<<fmod_param param value>>` | FMODAudioManager | Set FMOD parameter |

### Scenes
- **MainMenu.unity**: Title screen
- **MVPScene.unity**: Main game (Dialogue System in DontDestroyOnLoad)

## Dialogue Input System

Clean two-component architecture for dialogue advancement:

| Component | Location | Responsibility |
|-----------|----------|----------------|
| **LineAdvancer** (Yarn Spinner) | Line Advancer GameObject | Keyboard input (Space/Enter), two-stage advancement |
| **ClickAdvancer** (custom) | Dialogue System | Mouse click input, delegates to LineAdvancer |

### How It Works
- **Space/Enter**: Two-stage advancement (first completes text, second advances to next line)
- **Click anywhere** (except buttons): Same two-stage behavior via ClickAdvancer
- **ESC**: Disabled (prevents accidental skips)

### Key Components
- `LineAdvancer`: Yarn Spinner's built-in component, handles keyboard and tracks line completion state
- `ClickAdvancer`: Minimal 45-line script, checks `ModalInputLock` and `IsPointerOverInteractableUI()`, then calls `lineAdvancer.RequestLineHurryUp()`
- `OptionsInputHandler`: Handles Space key for selecting dialogue options

### Modal Blocking
- `ModalInputLock`: Static lock checked by ClickAdvancer
- `WelcomeOverlay`: Directly disables LineAdvancer when shown (LineAdvancer doesn't check ModalInputLock)

### ActionMarkupHandler Pattern
Line lifecycle callbacks (used by MumbleDialogueController, PortraitTalkingStateController):
- `OnLineDisplayBegin()` → Line starts displaying (start mumble/talking animation)
- `OnLineDisplayComplete()` → Text finished scrolling (stop mumble/talking animation)
- `OnLineWillDismiss()` → Line about to be dismissed

Register handlers in LinePresenter's **Event Handlers** list in the Inspector.

## Dialogue Files

Located in `Assets/Dialogue/`, numbered by content:
- `01-09`: Meta, globals, UI commands
- `10-19`: Run 1
- `20-29`: Run 2
- `30-39`: Run 3
- `40-49`: Run 4
- `90-99`: Epilogues, debug tools

Node naming: `R{run}_{context}` (e.g., `R1_Start`, `R2_Day1_Morning`)

## Audio System

See `docs/audio_guide.md` for comprehensive audio documentation including FMOD setup, theme mapping, WebGL troubleshooting, and usage examples.

### Quick Reference
```yarn
<<music MainTheme>>           // Play theme (auto-selects Side A/B)
<<music AliceTheme>>          // Alice's theme
<<music SupervisorTheme>>     // Arthur's theme
<<music_stop>>                // Stop with proper fade
<<bgm key>>                   // Legacy background music
<<sfx key>>                   // Sound effects
```

### Key Files
| File | Location | Purpose |
|------|----------|---------|
| `FMODAudioManager.cs` | `Scripts/Audio/` | Main FMOD manager, Yarn commands |
| `FMODWebGLBankLoader.cs` | `Scripts/Audio/` | WebGL bank loader |
| `AudioCommandHandler.cs` | `Scripts/Audio/` | Legacy bgm/sfx commands |
| `FMODCacheFixer.cs` | `Scripts/Editor/` | Editor script for cache bugs |

### Theme Mapping
| Theme | Side A (Nela) | Side B (Franco) |
|-------|---------------|-----------------|
| MainTheme | ASIDE_MainTheme | BSIDE_MainTheme |
| AliceTheme | ASIDE_AliceTheme | BSIDE_AliceTheme |
| SupervisorTheme | ASIDE_Supervisor | BSIDE_ArthurTheme |

## Settings System

**SettingsManager** (`Scripts/Core/SettingsManager.cs`)
- Centralized settings persistence via PlayerPrefs
- Volume controls: Master, Music, SFX (0.0-1.0)
- Events for settings changes (OnMasterVolumeChanged, etc.)

**SettingsPanel** (`Scripts/UI/SettingsPanel.cs`)
- In-game settings UI accessed from pause menu
- Volume sliders with percentage display
- Soundtrack toggle (Nela's Score / Franco's Score)
- Reset to Defaults button

### PlayerPrefs Keys
| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `MasterVolume` | float | 1.0 | Master volume multiplier |
| `MusicVolume` | float | 0.7 | Music volume (legacy + FMOD) |
| `SFXVolume` | float | 1.0 | Sound effects volume |
| `SoundtrackSide` | string | "A" | Soundtrack preference ("A" or "B") |

### Editor Setup
- `Tools > Setup Settings Panel in Pause Menu` - Creates complete settings UI
- `Tools > Setup SettingsManager in Scene` - Adds SettingsManager component

## UI Canvas Architecture

The game uses **two separate ScreenSpaceOverlay canvases** with different sorting orders:

| Canvas | sortingOrder | Contents |
|--------|--------------|----------|
| Dialogue System Canvas ("Canvas") | 0 | Dialogue UI, pause menu, save/load panel, character sprites |
| OverlayUIRoot | 2000 | Metrics bars (Engagement/Sanity), Leaderboard, runtime overlays |

### OverlayCanvasProvider (`Scripts/UI/OverlayCanvasProvider.cs`)
- Singleton that provides a deterministic overlay canvas for runtime-created UI
- Creates "OverlayUIRoot" with `sortingOrder = 2000` and `DontDestroyOnLoad`
- Used by: `MetricsPanelUI`, `LeaderboardUI`, `ToastManager`
- Avoids nondeterministic canvas selection in WebGL/IL2CPP builds

### Best Practices
1. **Never change the Dialogue System Canvas sortingOrder** - This breaks OverlayUIRoot visibility
2. **Use OverlayCanvasProvider.GetCanvas()** for runtime UI that should appear above dialogue
3. **For modal dialogs within the Dialogue Canvas**, use sibling ordering (SetAsLastSibling) instead of sortingOrder changes

### Known Pitfall (Fixed)
`SaveSlotSelectionUI` previously changed the parent canvas `sortingOrder = 5000`, which caused metrics/leaderboard to disappear behind the dialogue UI. The fix was to remove this sortingOrder change entirely - the save/load panel is already visible within its parent canvas.

---

## UI Font Standardization

All UI uses **TextMeshPro** with the **monogram-extended** pixel font. The `GlobalFontOverride` system automatically applies this font to all TMP_Text components.

### Standardized Font Sizes
| Size | Usage |
|------|-------|
| **60px** | Titles, main labels, menu buttons (pause menu, save/load, settings) |
| **48px** | Secondary text, info labels, smaller buttons, HUD elements (metrics, scoreboard) |

### Runtime UI Creation Pattern
Some UI components create their elements dynamically at runtime (not via Editor setup scripts):
- `SaveSlotSelectionUI.cs` - Creates save/load slot UI on first show
- `MetricsPanelUI.cs` - Creates engagement/sanity bars
- `LeaderboardUI.cs` - Creates scoreboard entries
- `ToastManager.cs` - Creates toast notifications

For these components, font sizes are set in code when elements are created. Changes take effect immediately without needing to run Editor setup tools.

### Editor Setup Scripts
Other UI is created via Editor menu tools that must be re-run to apply changes:
- `Tools > Setup Pause Menu` - PauseMenuSetup.cs
- `Tools > Setup Settings Panel in Pause Menu` - SettingsPanelSetup.cs
- `Tools > Setup Company Store` - CompanyStoreSetup.cs
- `Tools > Setup Dialogue Font Sizes` - DialogueFontSetup.cs (updates LinePresenter, WheelOptionView, BubbleContentView text)

These scripts have "recreate" logic that prompts to delete existing UI before rebuilding.

### HUD Buttons (Runtime-Created)
The corner HUD buttons (Menu, Feedback, Back, Run/Day tracker) are created at runtime by `PauseMenuManager.cs`. Their sizes are controlled by constants:
```csharp
private const float HudButtonHeight = 90f;
private const float HudMenuButtonWidth = 200f;
private const float HudBackButtonWidth = 240f;
private const float HudFeedbackButtonWidth = 300f;
private const float HudRunDayBoxWidth = 400f;
```
Font size is set to 60px in `CreateButtonText()` and `SetupRunDayTracker()`.

### Runtime Defaults Pattern
`MetricsPanelUI` uses `enforceRuntimeDefaults = true` to override serialized Inspector values at runtime. This ensures consistent font sizes (48px) even if old values are saved in the scene. The pattern:
```csharp
private void ApplyRuntimeDefaults()
{
    if (!enforceRuntimeDefaults) return;
    minFontSize = 48;
    fontSize = 48;
    // ... other defaults
}
```

### Dialogue Choice Font Override

Yarn Spinner's default `OptionItem` prefab (in Packages) has a hardcoded font. The solution is to use a **custom prefab** with the correct font settings.

**Custom Prefab Location**: `Assets/Prefabs/Option Item.prefab`
- Copied from `Packages/dev.yarnspinner.unity/Prefabs/Option Item.prefab`
- Font: monogram-extended SDF
- Font Size: 60px

**Setup**:
1. The custom prefab is assigned to `Options Presenter > Option View Prefab`
2. The "Last Line" text fields in Options Presenter are configured directly in the scene

**Why a custom prefab?** The default prefab is in Packages and shouldn't be modified directly. Copying to Assets allows full customization while preserving upgrade compatibility.

**Reference**: [Yarn Spinner - Theming Default Presenters](https://docs.yarnspinner.dev/yarn-spinner-for-unity/samples/theming-default-views)

---

## WebGL Notes

- Use **Gzip compression** (not Brotli) for iOS compatibility
- Power Preference: "Low Power" or "Default" (iOS crash prevention)
- PlayerPrefs = IndexedDB (browser-local, cleared with browser data)
- vercel.json handles .unityweb MIME types and encoding headers
- **FMOD banks**: Loaded async via custom `FMODWebGLBankLoader` (see FMOD WebGL Setup section)
- **Browser caching**: Handled automatically by the cache-busting system (see below)

---

## Version & Cache Management

The game uses an automatic cache-busting system to force browser cache invalidation on new deploys.

### How It Works
1. `version.json` contains the current version, build number, and build hash
2. `cache-buster.js` runs on page load, comparing server version to localStorage
3. On version mismatch: clears Unity IndexedDB + PlayerPrefs, then hard reloads
4. `vercel.json` uses aggressive no-cache headers for HTML and version.json

### Files
| File | Purpose |
|------|---------|
| `version.json` | Version manifest (never cached) |
| `assets/js/cache-buster.js` | Client-side version check & cache clear |
| `update-version.ps1` | Build script to update versions |

### Deploy Workflow
After completing a Unity WebGL build:

```powershell
# Auto-increment build number
.\update-version.ps1

# Or bump version
.\update-version.ps1 -Bump minor   # 4.0.0 -> 4.1.0
.\update-version.ps1 -Bump major   # 4.0.0 -> 5.0.0
.\update-version.ps1 -Bump patch   # 4.0.0 -> 4.0.1

# Then commit and push
git add .
git commit -m "Deploy v4.1.0"
# Push via GitHub Desktop
```

### What Gets Updated
The script updates:
- `webgl-build/version.json` - build number, hash, timestamp
- `index.html`, `play.html`, `mobile-play.html` - productVersion, titles, meta tags

## Key Variables

Core metrics: `$current_run`, `$current_day`, `$engagement`, `$sanity`, `$leaderboard_rank`
Relationships: `$trust_supervisor`, `$trust_alice`, `$trust_timmy`
Inventory: `$item_*` flags
Story state: `$aware_*` flags

## Migration from v3.5.3

This project is a clean restructure from `Mental_Break_AlphaV2.0`. Key changes:
- Eliminated double-nested directory structure
- Removed version numbers from paths
- Unity project now in `unity-project/` subdirectory
- Build output goes directly to `unity-project/webgl-build/`
- New GitHub repo: https://github.com/keatonslee1/MentalBreak

---

## Unity MCP Integration

Claude Code connects to the Unity Editor via the **Unity MCP** (Model Context Protocol) server, enabling direct interaction with the Unity Editor.

### Connection Details
| Setting | Value |
|---------|-------|
| Server Name | `UnityMCP` |
| Transport | HTTP |
| URL | `http://localhost:8080/mcp` |
| Scope | User config (available in all projects) |
| Server Version | 8.7.0 |
| Source | `github.com/CoplayDev/unity-mcp` |

### How It Works
1. **Unity Editor** runs the MCP plugin (installed via Package Manager or manually)
2. **MCP Server** starts automatically when Unity opens the project, listening on `localhost:8080`
3. **Claude Code** connects to the server via the configured HTTP endpoint

### Starting the Connection

**Automatic (recommended):**
- Simply open the Unity project in Unity Editor
- The MCP server starts automatically
- Claude Code connects when you start a session

**Manual verification:**
```powershell
# Check if MCP server is configured in Claude Code
claude mcp list

# Check connection status
claude mcp get UnityMCP
```

**If not configured, add it:**
```powershell
claude mcp add UnityMCP --transport http --url http://localhost:8080/mcp -s user
```

### Unity Instance Details
| Property | Value |
|----------|-------|
| Instance ID | `unity-project@ab62627946292c7a` |
| Unity Version | 6000.2.10f1 |
| Platform | WebGL (WindowsEditor) |
| Active Scene | `Assets/Scenes/MVPScene.unity` |

### Available Resources (Read-Only State)
| Resource | URI | Purpose |
|----------|-----|---------|
| Editor State | `unity://editor_state` | Play mode, compilation status, readiness |
| Project Info | `unity://project/info` | Paths, Unity version, platform |
| Scene Hierarchy | via `manage_scene` | GameObject tree |
| Custom Tools | `unity://custom-tools` | Project-specific tools |
| Console Logs | via `read_console` | Errors, warnings, logs |
| Tests | `mcpforunity://tests` | Available test methods |
| Tags/Layers | `unity://project/tags`, `unity://project/layers` | Project settings |

### Available Tools (Actions)
| Tool | Purpose |
|------|---------|
| `manage_editor` | Play/pause/stop, tags, layers |
| `manage_scene` | Load, save, get hierarchy, screenshot |
| `manage_gameobject` | Create, modify, find, add/remove components |
| `manage_asset` | Search, import, create, delete assets |
| `manage_script` | Create, read, delete C# scripts |
| `script_apply_edits` | Structured C# edits (methods, classes) |
| `apply_text_edits` | Raw text edits with line/column positions |
| `manage_material` | Create materials, set properties, assign to renderers |
| `manage_prefabs` | Open/close prefab stage, create from GameObject |
| `run_tests` / `run_tests_async` | Execute Unity tests |
| `read_console` | Get/clear console messages |
| `refresh_unity` | Trigger asset database refresh |
| `execute_menu_item` | Run Unity menu commands |

### Best Practices
1. **Check `editor_state` before mutations** - Ensure `ready_for_tools: true`
2. **Use `read_console` after script changes** - Check for compilation errors
3. **Use paging for large queries** - Scene hierarchy, asset searches
4. **Prefer `script_apply_edits`** over raw text edits for safer C# modifications

### Troubleshooting

**Connection failed:**
1. Ensure Unity Editor is open with the project loaded
2. Check if MCP server is running: look for `mcp_http_8080.pid` in `unity-project/Library/MCPForUnity/RunState/`
3. Restart Unity Editor to restart the MCP server

**Server not configured:**
```powershell
claude mcp add UnityMCP --transport http --url http://localhost:8080/mcp -s user
```

**Multiple Unity instances:**
- Use `set_active_instance` tool with `Name@hash` format
- Check available instances via `unity://instances` resource

### Files & Locations
| File | Purpose |
|------|---------|
| `unity-project/Library/MCPForUnity/RunState/mcp_http_8080.pid` | Server process ID |
| `unity-project/Library/MCPForUnity/TerminalScripts/mcp-terminal.cmd` | Manual server start script |
| `.claude/settings.local.json` | Local Claude permissions (project-specific) |

---

## UI Adjustment via MCP

Claude can directly inspect and modify Unity UI elements through MCP. This enables quick iteration on UI without manual Editor work.

### UI Element Classification

| Category | Elements | Location | Persistence Method |
|----------|----------|----------|-------------------|
| **Runtime-Created** | HUD buttons, MetricsPanel, Leaderboard, SaveSlotUI, Toast | Created by C# at runtime | Update code constants |
| **Editor-Created** | PauseMenuPanel, SettingsPanel, CompanyStore, Dialogue UI | Scene hierarchy | Save scene in Unity |

### MCP UI Workflow

```
1. DISCOVER  → Find UI element by name or component type
2. INSPECT   → Read current component properties
3. MODIFY    → Set new property values
4. VERIFY    → Take screenshot to confirm changes
5. PERSIST   → Scene: Save in Unity | Runtime: Update code constants
```

### MCP Commands

**Find UI element:**
```
manage_gameobject(action="find", search_method="by_name", search_term="ButtonName")
manage_gameobject(action="find", search_method="by_component", search_term="TMP_Text")
```

**Read properties:**
```
manage_gameobject(action="get_components", name="ElementName", include_properties=true)
```

**Modify properties:**
```
manage_gameobject(action="set_component_property", name="ElementName",
    component_properties={"ComponentName": {"property": value}})
```

**Take screenshot:**
```
manage_scene(action="screenshot")
```

### Property Format Reference

Properties must use nested dictionary format: `{ComponentName: {property: value}}`

```javascript
// Position
{"RectTransform": {"anchoredPosition": {"x": 100, "y": 50}}}

// Size
{"RectTransform": {"sizeDelta": {"x": 200, "y": 100}}}

// Text content & style
{"TextMeshProUGUI": {"text": "New Text", "fontSize": 48}}

// Color (RGBA 0-1 range)
{"Image": {"color": {"r": 1, "g": 0.5, "b": 0.5, "a": 1}}}

// Button state
{"Button": {"interactable": false}}
```

### Common UI Element Paths

| Element | Scene Path | Component |
|---------|------------|-----------|
| Dialogue text | `Dialogue System/Canvas/Line Presenter/Text` | TMP_Text |
| Character name | `Dialogue System/Canvas/Line Presenter/Character Name` | TMP_Text |
| Pause menu panel | `Dialogue System/Canvas/PauseMenuPanel` | Image, CanvasGroup |
| Settings panel | `PauseMenuPanel/SettingsPanel` | Image, CanvasGroup |
| Master volume slider | `SettingsPanel/ContentBox/MasterVolumeRow/MasterSlider` | Slider |
| Options presenter | `Dialogue System/Canvas/Options Presenter` | VerticalLayoutGroup |

### Runtime UI Constants

For runtime-created UI, modify these constants in code:

| Script | Constants | Purpose |
|--------|-----------|---------|
| `PauseMenuManager.cs` | `HudButtonHeight` (90), `HudMenuButtonWidth` (200), `HudBackButtonWidth` (240), `HudFeedbackButtonWidth` (300) | HUD button sizes |
| `MetricsPanelUI.cs` | `fontSize` (48), `minFontSize` (48), bar width/height | Metrics display |
| `LeaderboardUI.cs` | Column widths, `fontSize` (48) | Leaderboard layout |
| `SaveSlotSelectionUI.cs` | `RowHeight` (100), button colors | Save/load UI |

### Proactive UI Assistance

Claude will proactively help with UI when:
- User mentions UI looks wrong, off, or misaligned
- User shares a screenshot showing UI issues
- Console shows UI-related warnings or errors
- User asks about visual appearance

**Proactive workflow:**
1. Take screenshot to assess current state
2. Identify the UI element(s) involved
3. Propose specific adjustments with values
4. Ask for confirmation before modifying
5. Apply changes and verify with screenshot
6. Suggest persistence method based on UI type

### Notes

- MCP modifications are immediate in Editor but require scene save to persist
- Runtime UI changes need code constant updates to persist across sessions
- Screenshots save to `Assets/Screenshots/` (consider adding to .gitignore)
- Always verify changes with screenshot before persisting
