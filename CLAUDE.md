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
│   │   ├── Scripts/                       # C# game logic
│   │   ├── Graphics/                      # Backgrounds, portraits, UI
│   │   ├── Audio/                         # BGM and SFX
│   │   ├── Dialogue Wheel for Yarn Spinner/
│   │   ├── Speech Bubbles for Yarn Spinner/
│   │   └── StreamingAssets/               # FMOD banks (future)
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

### Core Systems

**GameManager** (`Assets/Scripts/GameManager.cs`)
- Central coordinator for game state and run transitions
- 4-run structure (R1_Start through R4_End)

**SaveLoadManager** (`Assets/Scripts/SaveLoadManager.cs`)
- Planned: 5 manual slots + 3 autosave FIFO
- WebGL: PlayerPrefs (IndexedDB-backed)
- Desktop: JSON files in persistentDataPath

**CommandHandlerRegistrar** (`Assets/Scripts/CommandHandlerRegistrar.cs`)
- Registers Yarn commands with DialogueRunner
- Required for WebGL builds (attribute discovery fails)

### Yarn Commands

| Command | Handler | Description |
|---------|---------|-------------|
| `<<bg key>>` | BackgroundCommandHandler | Background (video/static) |
| `<<bgm key>>` | AudioCommandHandler | Background music |
| `<<sfx key>>` | AudioCommandHandler | Sound effect |
| `<<checkpoint id>>` | CheckpointCommandHandler | Autosave point |
| `<<store>>` | StoreUI | Open store UI |

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

## WebGL Notes

- Use **Gzip compression** (not Brotli) for iOS compatibility
- Power Preference: "Low Power" or "Default" (iOS crash prevention)
- PlayerPrefs = IndexedDB (browser-local, cleared with browser data)
- vercel.json handles .unityweb MIME types and encoding headers

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
