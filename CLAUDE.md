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
| Dialogue/ | Dialogue flow | DialogueAdvanceHandler, OptionsInputHandler |
| UI/ | User interface | MenuManager, PauseMenuManager, SettingsPanel, StoreUI, ToastManager |
| Audio/ | Audio commands | AudioCommandHandler, FMODAudioManager, FMODWebGLBankLoader |
| Characters/ | Portraits | CharacterSpriteManager, CharacterTalkAnimation |
| Commands/ | Yarn handlers | BackgroundCommandHandler, CheckpointCommandHandler |
| Editor/ | Dev tools | DialogueSystemUIAutoWire, SettingsPanelSetup, various scripts |

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

## WebGL Notes

- Use **Gzip compression** (not Brotli) for iOS compatibility
- Power Preference: "Low Power" or "Default" (iOS crash prevention)
- PlayerPrefs = IndexedDB (browser-local, cleared with browser data)
- vercel.json handles .unityweb MIME types and encoding headers
- **FMOD banks**: Loaded async via custom `FMODWebGLBankLoader` (see FMOD WebGL Setup section)
- **Browser caching**: After deploying changes, users may need to fully clear browser cache (Ctrl+Shift+Delete) - incognito mode alone may not bypass CDN cache

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
