# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Mental Break is a Unity 6.2 WebGL narrative game deployed via Vercel. It uses Yarn Spinner for dialogue with custom dialogue wheel and speech bubble systems.

**Current Version**: 4.0.0 (Clean restructure from v3.5.3)

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
```powershell
cd C:\Users\epick\OneDrive\Documents\mentalbreak
git add . && git commit -m "Update" && git push
# Vercel auto-deploys from main branch
```

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

## FMOD Audio System

**Banks** (in `StreamingAssets/`):
- `Master.bank` + `Master.strings.bank` (required)
- `Music_A_Side.bank` - Nela's soundtrack (Side A)
- `Music_B_Side.bank` - Franco's soundtrack (Side B)

### Side A Events (Nela)
All use `EndFade` parameter (0=play, 1=fade out):
- `ASIDE_MainTheme`
- `ASIDE_AliceTheme`
- `ASIDE_Supervisor`

### Side B Events (Franco)
Use `LoopChange` + ending parameters:

| Event | LoopChange | Ending |
|-------|------------|--------|
| `BSIDE_MainTheme` | LoopA/B only | EndSection (closing section) |
| `BSIDE_AliceTheme` | LoopA/B only | EndSection (closing section) |
| `BSIDE_ArthurTheme` | LoopA/B/C/D | EndFade (fade out) |

### Soundtrack Side Selection
- **Default**: Side A (Nela's Score)
- **Player toggle**: Settings in pause menu ("Nela's Score" / "Franco's Score")
- **Storage**: PlayerPrefs key `SoundtrackSide` ("A" or "B")

### Theme Mapping
| Theme Name | Side A Event | Side B Event |
|------------|--------------|--------------|
| MainTheme | ASIDE_MainTheme | BSIDE_MainTheme |
| AliceTheme | ASIDE_AliceTheme | BSIDE_AliceTheme |
| SupervisorTheme | ASIDE_Supervisor | BSIDE_ArthurTheme |

### Usage in Yarn (Recommended)
```yarn
<<music MainTheme>>                // Auto-selects A or B based on player setting
<<music AliceTheme>>               // Same - uses current side preference
<<music SupervisorTheme 1>>        // Start at LoopB (Side B only)
<<fmod_loop LoopB>>                // Switch to LoopB
<<music_stop>>                     // Stop with proper ending
<<music_stop true>>                // Stop immediately
```

### Direct FMOD Access (Advanced)
```yarn
<<fmod ASIDE_MainTheme>>           // Force specific side
<<fmod BSIDE_ArthurTheme 2>>       // Start at LoopC (0=A,1=B,2=C,3=D)
<<fmod_param EndFade 1>>           // Manual parameter control
```

### FMOD WebGL Setup

FMOD requires special handling for WebGL builds due to async bank loading.

**Key Components** (in `Scripts/Audio/`):

| File | Purpose |
|------|---------|
| `FMODWebGLBankLoader.cs` | Custom WebGL bank loader that downloads banks via UnityWebRequest |
| `FMODAudioManager.cs` | Main audio manager with retry mechanism for race conditions |
| `FMODCacheFixer.cs` | Editor script that fixes FMOD cache naming bugs |

**How it works:**
1. `FMODWebGLBankLoader` auto-creates itself via `[RuntimeInitializeOnLoadMethod]` in WebGL builds
2. Downloads all banks from StreamingAssets using UnityWebRequest
3. Loads banks into FMOD via `loadBankMemory()`
4. `FMODAudioManager.PlayMusic()` uses retry mechanism (10 attempts, 0.5s delay) to handle race conditions where music commands fire before banks finish loading

**vercel.json Configuration:**
The deployment config includes CORS headers for .bank files:
```json
{
  "src": "/StreamingAssets/(.*\\.bank)",
  "dest": "/StreamingAssets/$1",
  "headers": {
    "Content-Type": "application/octet-stream",
    "Access-Control-Allow-Origin": "*",
    "Access-Control-Allow-Methods": "GET, OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type"
  }
}
```

### FMOD WebGL Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| 404 errors for .bank files | CDN cache stale after deployment | Clear browser cache completely (Ctrl+Shift+Delete), not just incognito |
| "Event not found" errors | Banks still loading when music command fires | Retry mechanism handles this automatically; check console for retry logs |
| Banks load but no sound | FMOD system not initialized | Check `[FMODWebGLBankLoader]` logs in browser console |
| `UI.bank.bank` naming bug | FMOD cache corruption | Run Unity, let `FMODCacheFixer` auto-fix, rebuild |

**Debug Logging:**
In WebGL builds, look for these console prefixes:
- `[FMODWebGLBankLoader]` - Bank download and loading status
- `[FMOD]` - FMOD system messages and retry attempts
- `[FMODAudioManager]` - Music playback commands

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

See `docs/MIGRATION_NOTES.md` for detailed migration history.
