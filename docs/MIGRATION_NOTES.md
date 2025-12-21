# Migration Notes: Mental Break v3.5.3 → v4.0.0

This document tracks the complete migration from the legacy `Mental_Break_AlphaV2.0` structure to the clean v4.0.0 architecture.

## Migration Overview

| Aspect | v3.5.3 (Legacy) | v4.0.0 (Current) |
|--------|-----------------|------------------|
| Repository | Local only | GitHub: keatonslee1/MentalBreak |
| Directory | Double-nested with version numbers | Clean `unity-project/` structure |
| Scripts | Flat `Scripts/` folder | Organized by domain (Core/, UI/, etc.) |
| Save System | Basic 3-slot manual saves | 5 manual + 3 autosave + export/import |
| Audio | Unity AudioSource only | Dual system: Legacy + FMOD |
| Build Output | Nested build folders | Direct to `webgl-build/` |

---

## Phase 1: Initial Structure
**Commit:** `04a0c2b` | **Date:** Dec 20, 2025

### What Was Done
- Created clean repository structure eliminating double-nested directories
- Established `unity-project/` as the Unity project root
- Set up Vercel deployment configuration
- Migrated all Assets (Dialogue, Scripts, Graphics, Audio)
- Created initial CLAUDE.md documentation
- Configured .gitignore for Unity + WebGL + Vercel

### Directory Structure Established
```
mentalbreak/
├── unity-project/           # Unity project (was nested 2 levels deep)
│   ├── Assets/
│   │   ├── Dialogue/        # Yarn files (01-99 numbering)
│   │   ├── Scripts/         # C# code (flat at this point)
│   │   ├── Graphics/        # Backgrounds, portraits, UI
│   │   ├── Audio/           # BGM and SFX assets
│   │   └── ...
│   └── webgl-build/         # Build output for Vercel
├── api/                     # Vercel serverless functions
├── docs/                    # Documentation
├── vercel.json              # Deployment config
└── CLAUDE.md                # Project guidance
```

### Key Decisions
- **Removed version numbers from paths**: No more `Mental_Break_AlphaV2.0/Mental_Break_AlphaV2.0/` nesting
- **Single build output location**: `webgl-build/` directly in unity-project
- **Vercel integration**: Auto-deploy from main branch

---

## Phase 2: Script Reorganization
**Commit:** `d333c76` | **Date:** Dec 21, 2025

### What Was Done
Reorganized 40+ scripts from flat `Scripts/` folder into logical domain folders:

| Folder | Purpose | Scripts Moved |
|--------|---------|---------------|
| `Core/` | Game state & persistence | GameManager, SaveLoadManager, SaveLoadInputHandler, RunTransitionManager |
| `Dialogue/` | Yarn Spinner integration | DialogueAdvanceHandler, OptionsInputHandler, ChoiceDiagnostics, DialogueDebugNavigator, DialogueRuntimeWatcher, OptionsAutoSelector, StartDialogueOnPlay |
| `UI/` | User interface | MenuManager, PauseMenuManager, StoreUI, DayplannerUI, LeaderboardUI, MetricsDisplay, FeedbackForm, + 10 more |
| `Audio/` | Audio management | AudioCommandHandler |
| `Characters/` | Sprite/portrait management | CharacterSpriteManager, CharacterTalkAnimation |
| `Commands/` | Yarn command handlers | BackgroundCommandHandler, CheckpointCommandHandler, CommandHandlerRegistrar, MVPCommandHandlers |
| `Editor/` | Editor-only utilities | (unchanged - already organized) |

### Key Decisions
- **Domain-driven organization**: Scripts grouped by what they do, not technical type
- **Preserved Editor/ structure**: Already well-organized
- **No code changes**: Pure file moves, no refactoring

---

## Phase 3: Save System Overhaul
**Commit:** `7a8833a` | **Date:** Dec 21, 2025

### What Was Done

#### New Save Slot Architecture
| Slot Type | Indices | Behavior |
|-----------|---------|----------|
| Manual Saves | 1-5 | Player-controlled, explicit save/load |
| Autosaves | 0, -1, -2 | FIFO rotation at checkpoints |

**Autosave Rotation Logic:**
1. When `<<checkpoint>>` triggers autosave
2. Slot -2 (oldest) is discarded
3. Slot -1 → Slot -2
4. Slot 0 → Slot -1
5. New save → Slot 0 (newest)

#### New SaveExporter System
Created `Scripts/Core/SaveExporter.cs` for portable save sharing:

```csharp
// Export format: MBSAVE_1_{base64data}
string exportString = SaveExporter.ExportToString(saveData);

// Import from clipboard
SaveData imported = SaveExporter.ImportFromString(pastedString);
```

**Features:**
- Base64 encoding for safe copy/paste
- Version prefix for future migration support
- Validation and error handling
- Cross-device save transfer capability

#### SaveLoadManager Enhancements
- `SaveToSlot(int slotIndex)` / `LoadFromSlot(int slotIndex)`
- `TriggerAutosave(string checkpointId)` with FIFO rotation
- `ExportSave(int slotIndex)` / `ImportSave(string data, int targetSlot)`
- `GetAllSlotMetadata()` for UI population
- Platform-aware storage (PlayerPrefs for WebGL, JSON files for desktop)

#### SaveSlotSelectionUI Updates
- Support for 8 total slots (5 manual + 3 autosave)
- Visual distinction between manual and autosave slots
- Import button for pasting save strings
- Export functionality for copying saves

### Key Decisions
- **Negative indices for autosaves**: Clear semantic distinction from manual slots
- **FIFO not LIFO**: Players want recent autosaves, not oldest
- **Base64 export**: Universal, no special characters, works in any text field
- **Version prefix**: `MBSAVE_1_` allows future format changes

---

## Phase 4: FMOD Audio Integration
**Commits:** `42363c5`, `b2fd575` | **Date:** Dec 21, 2025

### What Was Done

#### Dual Audio System Architecture
The game now has two parallel audio systems:

| System | Command | Use Case | Technology |
|--------|---------|----------|------------|
| Legacy | `<<bgm key>>` | Mood-based tracks (darkwave, ambient, etc.) | Unity AudioSource |
| FMOD | `<<music theme>>` | Character themes with A/B side selection | FMOD Studio |

#### FMODAudioManager (`Scripts/Audio/FMODAudioManager.cs`)
**Core Features:**
- Singleton pattern with DontDestroyOnLoad
- PlayerPrefs-based side preference ("A" or "B")
- Smart theme mapping with fallbacks
- Proper ending handling (EndFade/EndSection parameters)

**Theme Mapping:**
| Theme Name | Side A (Nela) | Side B (Franco) |
|------------|---------------|-----------------|
| MainTheme | ASIDE_MainTheme | BSIDE_MainTheme |
| AliceTheme | ASIDE_AliceTheme | BSIDE_AliceTheme |
| SupervisorTheme | ASIDE_Supervisor | BSIDE_ArthurTheme |

**Yarn Commands Registered:**
```yarn
<<music MainTheme>>           // Auto-selects A or B side
<<music AliceTheme 1>>        // Start at LoopB
<<music_stop>>                // Fade out properly
<<music_stop true>>           // Immediate stop
<<fmod ASIDE_MainTheme>>      // Force specific event
<<fmod_loop LoopB>>           // Change loop section
<<fmod_param EndFade 1>>      // Manual parameter control
```

#### SoundtrackToggleUI (`Scripts/UI/SoundtrackToggleUI.cs`)
- Toggle button component for pause menu
- Labels: "Nela's Score" (Side A) / "Franco's Score" (Side B)
- Auto-updates when side changes
- Works with both TMP and legacy UI Text

#### FMOD Banks in StreamingAssets
| Bank | Size | Contents |
|------|------|----------|
| Master.bank | 1 KB | Core FMOD system |
| Master.strings.bank | 1 KB | Event name lookup |
| Music_A_Side.bank | 1.7 MB | Nela's soundtrack |
| Music_B_Side.bank | 4.5 MB | Franco's soundtrack |
| UI.bank | 0.5 KB | UI audio events |

#### Bug Fixes (b2fd575)
- Fixed event path: `event:/` not `event:/Music/`
- Fixed bank filename: `UI.bank` not `UI.bank.bank`
- Added FMOD Unity plugin to repository

### Key Decisions
- **Dual system approach**: Legacy bgm commands preserved for mood tracks without FMOD equivalents
- **Default to Side A**: Game starts with Nela's Score, player opts into Franco's
- **Theme abstraction**: Writers use theme names, not FMOD event paths
- **Arthur = Supervisor**: Same thematic role, different names per side

### FMOD Parameter Reference
| Event Type | Parameters | Behavior |
|------------|------------|----------|
| Side A (all) | EndFade: 0→1 | Triggers fade out |
| Side B (MainTheme, AliceTheme) | LoopChange: 0-1, EndSection: 0→1 | Loop selection, closing section |
| Side B (ArthurTheme) | LoopChange: 0-3, EndFade: 0→1 | Loop A-D, fade out |

---

## Phase 5: Settings Menu Implementation
**Commit:** `ab2399f` | **Date:** Dec 21, 2025

### What Was Done

#### SettingsManager (`Scripts/Core/SettingsManager.cs`)
Centralized settings persistence with PlayerPrefs:

**Volume Settings:**
- Master Volume (0.0-1.0, default 1.0) - Controls AudioListener.volume
- Music Volume (0.0-1.0, default 0.7) - Applied to AudioCommandHandler and FMODAudioManager
- SFX Volume (0.0-1.0, default 1.0) - Applied to AudioCommandHandler

**Features:**
- Event-based updates (OnMasterVolumeChanged, OnMusicVolumeChanged, OnSFXVolumeChanged)
- Auto-save on application pause/quit
- Reset to defaults functionality

#### SettingsPanel (`Scripts/UI/SettingsPanel.cs`)
In-game settings UI accessed from pause menu:

- Volume sliders with percentage display
- Soundtrack toggle (Nela's Score / Franco's Score)
- Reset to Defaults button
- Back button to return to pause menu

#### Audio System Integration
- **AudioCommandHandler**: Subscribes to volume changes, applies Music/SFX volume
- **FMODAudioManager**: Subscribes to Music volume changes, applies to FMOD events

#### PauseMenuManager Updates
- Settings button now enabled and functional
- Opens SettingsPanel, hides pause menu buttons
- ESC closes settings panel and returns to pause menu

### Editor Setup Tools
- `Tools > Setup Settings Panel in Pause Menu` - Creates complete settings UI
- `Tools > Setup SettingsManager in Scene` - Adds SettingsManager component

### PlayerPrefs Keys
| Key | Type | Default |
|-----|------|---------|
| `MasterVolume` | float | 1.0 |
| `MusicVolume` | float | 0.7 |
| `SFXVolume` | float | 1.0 |
| `SoundtrackSide` | string | "A" |

### Bug Fixes (f85e6d1)
1. **Missing Settings Button**: PauseMenuSetup.cs wasn't creating a Settings button.
   - Added SettingsButton to button creation array
   - Added `Tools > Add Settings Button to Pause Menu` menu item

2. **Settings Panel Sliders Not Rendering**: Manual slider creation produced broken UI.
   - Rewrote SettingsPanelSetup.cs to use `DefaultControls.CreateSlider()`
   - Changed from VerticalLayoutGroup to absolute positioning
   - Created centered ContentBox (450x380) with explicit element positions

---

## Remaining Work / Future Phases

### Phase 6: Save/Load UI Feedback (Planned)
- Toast notifications for save/load success/failure
- Visual feedback for export/import operations
- Better error messages for corrupted saves

### Phase 7: WebGL Build Verification (Planned)
- Full build with FMOD integration
- Test on target browsers (Chrome, Safari, Firefox)
- iOS Safari compatibility testing
- Performance profiling

### Future Considerations
- **FMOD track expansion**: As sound designer adds more tracks, extend ThemeMapping
- **Legacy audio deprecation**: Eventually migrate all `<<bgm>>` to `<<music>>`
- **Localization**: May need localized soundtrack side labels
- **Save migration**: EXPORT_VERSION allows future SaveData schema changes

---

## Technical Reference

### File Locations (Key Files)
```
Scripts/Core/
├── GameManager.cs           # Central game state coordinator
├── SaveLoadManager.cs       # Save/load with slot management
├── SaveExporter.cs          # Base64 export/import utility
├── SettingsManager.cs       # Volume and settings persistence
└── RunTransitionManager.cs  # 4-run structure management

Scripts/Audio/
├── AudioCommandHandler.cs   # Legacy Unity audio (<<bgm>>, <<sfx>>)
└── FMODAudioManager.cs      # FMOD integration (<<music>>)

Scripts/Commands/
└── CommandHandlerRegistrar.cs  # Yarn command registration (WebGL fix)

Scripts/UI/
├── PauseMenuManager.cs      # Pause menu with save/load/settings
├── SaveSlotSelectionUI.cs   # 8-slot save UI
├── SettingsPanel.cs         # Volume sliders and settings UI
└── SoundtrackToggleUI.cs    # A/B side toggle button

Scripts/Editor/
├── SettingsPanelSetup.cs    # Creates settings panel in scene
└── SoundtrackToggleSetup.cs # Creates soundtrack toggle
```

### Yarn Command Quick Reference
| Command | System | Description |
|---------|--------|-------------|
| `<<bg key>>` | Background | Set background image/video |
| `<<bgm key>>` | Legacy Audio | Play mood-based music |
| `<<sfx key>>` | Legacy Audio | Play sound effect |
| `<<music theme [loop]>>` | FMOD | Play theme with A/B side selection |
| `<<music_stop [immediate]>>` | FMOD | Stop music |
| `<<fmod event [loop]>>` | FMOD | Play specific FMOD event |
| `<<fmod_loop name>>` | FMOD | Change loop section |
| `<<fmod_param name value>>` | FMOD | Set FMOD parameter |
| `<<checkpoint id>>` | Save System | Trigger autosave |
| `<<store>>` | UI | Open store interface |

### WebGL Considerations
- **Compression**: Use Gzip (not Brotli) for iOS compatibility
- **Power Preference**: "Low Power" or "Default" to prevent iOS crashes
- **PlayerPrefs**: Backed by IndexedDB, cleared with browser data
- **FMOD WebGL**: Requires html5 platform libraries in Plugins/FMOD

---

## Commit History

| Hash | Phase | Description |
|------|-------|-------------|
| `04a0c2b` | 1 | Initial v4.0 structure - clean migration |
| `437b160` | 1 | Add WebGL build files |
| `84a992a` | 1 | Fix .gitignore for builds |
| `d333c76` | 2 | Reorganize Scripts into logical folders |
| `071eeb7` | 2 | Add folder meta files |
| `8075463` | 2 | Update CLAUDE.md with organization |
| `7a8833a` | 3 | Save System Overhaul |
| `b216c13` | 3 | Add LoadMenuPanelSetup editor script |
| `8f56021` | 3 | Add LoadMenuPanel diagnostic tools |
| `42363c5` | 4 | FMOD Audio Integration |
| `b2fd575` | 4 | Fix FMOD issues, add plugin |
| `af2bb07` | 4 | Add MIGRATION_NOTES.md documentation |
| `ab2399f` | 5 | Settings Menu Implementation |
| `75f1ea0` | 5 | Add Settings Button to PauseMenuSetup |
| `f85e6d1` | 5 | Fix Settings Panel slider UI |

---

*Last Updated: December 21, 2025*
*Version: 4.0.0*
