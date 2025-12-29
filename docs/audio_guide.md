# Audio Guide for Mental Break

This document provides comprehensive documentation for the audio system in Mental Break, specifically for Claude Code and developers working on Yarn dialogue files.

---

## Quick Reference

### Recommended Commands (FMOD)
```yarn
<<music MainTheme>>           // Play main theme (auto-selects Side A/B)
<<music AliceTheme>>          // Play Alice's theme
<<music SupervisorTheme>>     // Play Arthur's theme
<<music_stop>>                // Stop with proper fade
<<music_stop true>>           // Stop immediately (no fade)
```

### Legacy Commands (AudioSource)
```yarn
<<bgm key>>                   // Play background music by key
<<sfx key>>                   // Play sound effect by key
```

### Advanced FMOD Commands
```yarn
<<fmod ASIDE_MainTheme>>      // Play specific FMOD event
<<fmod_loop LoopB>>           // Switch loop region
<<fmod_param EndFade 1>>      // Set FMOD parameter
```

---

## Two Audio Systems

Mental Break has **two** audio systems that can coexist:

### 1. FMOD (Modern - Recommended)
- Professional audio middleware with advanced features
- Supports Side A (Nela) and Side B (Franco) soundtracks
- Handles loop points, crossfades, and dynamic music
- Uses `.bank` files in `StreamingAssets/`
- Commands: `<<music>>`, `<<music_stop>>`, `<<fmod>>`, `<<fmod_loop>>`, `<<fmod_param>>`

### 2. Legacy AudioSource (Simple)
- Unity's built-in audio system
- Simple playback without advanced features
- Uses audio files in `Assets/Audio/`
- Commands: `<<bgm>>`, `<<sfx>>`

**When to use which:**
- Use **FMOD** (`<<music>>`) for character themes and narrative music
- Use **Legacy** (`<<bgm>>`, `<<sfx>>`) for simple sound effects or placeholder audio

---

## FMOD Commands in Detail

### `<<music theme [startLoop]>>`
Plays a music theme, automatically selecting the correct FMOD event based on the player's soundtrack preference (Side A or Side B).

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| theme | string | required | Theme name: `MainTheme`, `AliceTheme`, or `SupervisorTheme` |
| startLoop | int | 0 | Starting loop (0=A, 1=B, 2=C, 3=D). Only affects Side B. |

**Examples:**
```yarn
<<music MainTheme>>           // Start at LoopA
<<music SupervisorTheme 1>>   // Start at LoopB (Side B only)
<<music AliceTheme 2>>        // Start at LoopC (Side B only)
```

### `<<music_stop [immediate]>>`
Stops the currently playing music with proper ending behavior.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| immediate | bool | false | If true, stops immediately. If false, triggers proper ending (fade or closing section). |

**Examples:**
```yarn
<<music_stop>>                // Fade out / play closing section
<<music_stop true>>           // Stop immediately
```

### `<<fmod event [startLoop]>>`
Plays a specific FMOD event directly (bypasses theme mapping).

**Examples:**
```yarn
<<fmod ASIDE_MainTheme>>      // Force Side A
<<fmod BSIDE_ArthurTheme 2>>  // Force Side B, start at LoopC
```

### `<<fmod_stop [immediate]>>`
Same as `<<music_stop>>`. Alias for consistency.

### `<<fmod_loop loopName>>`
Changes the current loop region (Side B only).

| Parameter | Type | Description |
|-----------|------|-------------|
| loopName | string | `LoopA`, `LoopB`, `LoopC`, or `LoopD` |

**Example:**
```yarn
<<music SupervisorTheme>>     // Start playing
<<fmod_loop LoopB>>           // Switch to LoopB section
<<fmod_loop LoopC>>           // Switch to LoopC section
```

### `<<fmod_param parameter value>>`
Sets an FMOD parameter on the current event.

| Parameter | Type | Description |
|-----------|------|-------------|
| parameter | string | Parameter name (e.g., `EndFade`, `LoopChange`, `EndSection`) |
| value | float | Parameter value |

**Example:**
```yarn
<<fmod_param EndFade 1>>      // Trigger fade out manually
```

---

## Theme Mapping

The `<<music>>` command maps theme names to FMOD events based on the player's soundtrack preference:

| Theme Name | Side A Event (Nela) | Side B Event (Franco) |
|------------|---------------------|----------------------|
| `MainTheme` | `ASIDE_MainTheme` | `BSIDE_MainTheme` |
| `AliceTheme` | `ASIDE_AliceTheme` | `BSIDE_AliceTheme` |
| `SupervisorTheme` | `ASIDE_Supervisor` | `BSIDE_ArthurTheme` |

**Note:** The supervisor/Arthur theme has different names on each side because the character interpretation differs between soundtracks.

---

## Side A vs Side B

### Side A - Nela's Score
- Default soundtrack
- All events use `EndFade` parameter for stopping (0=play, 1=fade out)
- No loop regions - continuous playback
- Events: `ASIDE_MainTheme`, `ASIDE_AliceTheme`, `ASIDE_Supervisor`

### Side B - Franco's Score
- Alternative soundtrack (player toggle in settings)
- Supports loop regions via `LoopChange` parameter
- Different ending behavior per event:
  - `BSIDE_MainTheme` and `BSIDE_AliceTheme`: Use `EndSection` for closing section
  - `BSIDE_ArthurTheme`: Uses `EndFade` for fade out, has 4 loops (A-D)
- Events: `BSIDE_MainTheme`, `BSIDE_AliceTheme`, `BSIDE_ArthurTheme`

### Loop Values (Side B)
| Loop | LoopChange Value |
|------|------------------|
| LoopA | 0 |
| LoopB | 1 |
| LoopC | 2 |
| LoopD | 3 |

---

## WebGL Bank Loading

FMOD requires special handling for WebGL builds because bank files must be downloaded asynchronously.

### How It Works
1. `FMODWebGLBankLoader` auto-creates itself via `[RuntimeInitializeOnLoadMethod]` in WebGL builds
2. Downloads all `.bank` files from `StreamingAssets/` using UnityWebRequest
3. Loads banks into FMOD via `loadBankMemory()`
4. `FMODAudioManager.PlayMusic()` uses retry mechanism (10 attempts, 0.5s delay) to handle race conditions

### Bank Files
Located in `unity-project/Assets/StreamingAssets/`:
- `Master.bank` - Required base bank
- `Master.strings.bank` - Required string bank
- `Music_A_Side.bank` - Nela's soundtrack
- `Music_B_Side.bank` - Franco's soundtrack

### Retry Mechanism
If a music command fires before banks finish loading, the system automatically retries:
```
[FMOD] Event ASIDE_MainTheme not found, retrying (attempt 1/10)...
[FMOD] Event ASIDE_MainTheme not found, retrying (attempt 2/10)...
[FMOD] Successfully started event: ASIDE_MainTheme
```

---

## File Locations

### Scripts
| File | Location | Purpose |
|------|----------|---------|
| `FMODAudioManager.cs` | `Scripts/Audio/` | Main FMOD manager, theme mapping, Yarn commands |
| `FMODWebGLBankLoader.cs` | `Scripts/Audio/` | WebGL bank downloader |
| `AudioCommandHandler.cs` | `Scripts/Audio/` | Legacy bgm/sfx commands |
| `CommandHandlerRegistrar.cs` | `Scripts/Commands/` | Registers all Yarn commands |
| `FMODCacheFixer.cs` | `Scripts/Editor/` | Editor script for FMOD cache bugs |

### Assets
| Type | Location |
|------|----------|
| FMOD Banks | `Assets/StreamingAssets/*.bank` |
| Legacy Audio | `Assets/Audio/` |
| FMOD Plugin | `Assets/Plugins/FMOD/` |

### Configuration
| File | Location | Purpose |
|------|----------|---------|
| `vercel.json` | Repository root | CORS headers for .bank files |

---

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| 404 errors for .bank files | CDN cache stale after deployment | Clear browser cache completely (Ctrl+Shift+Delete) |
| "Event not found" errors | Banks still loading | Retry mechanism handles this automatically |
| Banks load but no sound | FMOD system not initialized | Check `[FMODWebGLBankLoader]` in browser console |
| `UI.bank.bank` naming bug | FMOD cache corruption | Run Unity, let `FMODCacheFixer` auto-fix, rebuild |
| Music doesn't change with side toggle | Need to restart music | Stop and replay after side change |

### Debug Logging
In WebGL builds, look for these console prefixes:
- `[FMODWebGLBankLoader]` - Bank download and loading status
- `[FMOD]` - FMOD system messages and retry attempts
- `[FMODAudioManager]` - Music playback commands

### Testing Locally
```powershell
cd C:\Users\epick\OneDrive\Documents\mentalbreak\unity-project\webgl-build
python -m http.server 8000
```

---

## Usage Examples in Yarn

### Basic Scene Music
```yarn
title: R1D1_Morning
---
<<music MainTheme>>
Good morning! Time to start another day at the office.
// ... scene continues with music playing
===
```

### Character Introduction
```yarn
title: R1D1_MeetArthur
---
<<music SupervisorTheme>>
Arthur: Welcome to the team. I'm Arthur, your supervisor.
Arthur: Let me show you around.
// ... Arthur's introduction
<<music_stop>>
===
```

### Music Transitions
```yarn
title: R1D1_AliceArrives
---
<<music_stop>>
// Brief pause
<<music AliceTheme>>
Alice enters the room with her usual bright smile.
Alice: Hey! You must be the new hire.
===
```

### Loop Changes (Side B Advanced)
```yarn
title: R1D1_TenseScene
---
<<music SupervisorTheme>>
// Scene starts calm
Arthur: We need to talk about your performance.
<<fmod_loop LoopB>>
// Music intensifies
Arthur: These numbers are unacceptable.
<<fmod_loop LoopC>>
// Music reaches peak tension
Arthur: I expect better from you.
<<music_stop>>
===
```

---

## For Future Claude

### When Adding Music to Yarn Files

1. **Use theme names, not event names:**
   ```yarn
   // GOOD - auto-selects side
   <<music MainTheme>>

   // AVOID unless you need to force a specific side
   <<fmod ASIDE_MainTheme>>
   ```

2. **Stop music before scene transitions:**
   ```yarn
   <<music_stop>>
   -> NextScene
   ```

3. **Match music to scene mood:**
   - `MainTheme` - General gameplay, office ambiance
   - `AliceTheme` - Alice scenes, friendship moments
   - `SupervisorTheme` - Arthur scenes, corporate tension

4. **The new R1D1 files need music added.** The December rewrite currently has zero audio commands. Reference what the old deleted R1D1 used:
   ```yarn
   // Old R1_D1_Start used:
   <<music SupervisorTheme>>

   // Old R1_D1_AliceIntro used:
   <<music AliceTheme>>

   // Old R1_D1_AliceAlone used:
   <<music MainTheme>>
   ```

### Key Code Locations

- **Theme mapping dictionary:** `FMODAudioManager.cs` line ~50-60
- **Yarn command registration:** `CommandHandlerRegistrar.cs` lines ~60-100
- **Retry mechanism:** `FMODAudioManager.cs` `PlayMusic()` method
- **Side preference storage:** PlayerPrefs key `SoundtrackSide` ("A" or "B")

### Modifying FMOD Events

If you need to add new themes:
1. Add FMOD events in FMOD Studio (not in Unity)
2. Export new bank files to `StreamingAssets/`
3. Add theme mapping in `FMODAudioManager.cs` `themeMapping` dictionary
4. Document the new theme in this guide

### Testing Audio Changes

1. Play in Unity Editor (FMOD works directly)
2. Build WebGL and test locally with `python -m http.server`
3. Check browser console for `[FMODAudioManager]` logs
4. Test both Side A and Side B in settings
