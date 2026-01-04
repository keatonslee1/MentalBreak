# Metrics SFX Animation - Salvaged Implementation

This folder contains all the code and documentation needed to implement the metrics animation system with SFX.

## What This Feature Does

When a metric (Engagement, Sanity, Suspicion) changes:
1. Plays a sound effect (up_swell.wav for increase, down_wobble.wav for decrease)
2. Highlights the metric panel with a background color pulse
3. Animates the bar smoothly over 1 second
4. If multiple metrics change at once, animates them sequentially (0.75s delay between each)

## Files in This Folder

### New Files to Create
| File | Destination |
|------|-------------|
| `NEW_FILE_MetricChange.cs` | `unity-project/Assets/Scripts/UI/MetricChange.cs` |
| `NEW_FILE_MetricsAnimator.cs` | `unity-project/Assets/Scripts/UI/MetricsAnimator.cs` |

### Diffs for Existing Files
| File | Target |
|------|--------|
| `DIFF_MetricsPanelUI.cs.diff` | `unity-project/Assets/Scripts/UI/MetricsPanelUI.cs` |
| `DIFF_AudioCommandHandler.cs.diff` | `unity-project/Assets/Scripts/Audio/AudioCommandHandler.cs` |

### Documentation
| File | Purpose |
|------|---------|
| `IMPLEMENTATION_GUIDE.md` | Detailed explanation of what to do |
| `CLAUDE_INSTRUCTIONS.md` | Step-by-step instructions for Claude to follow |
| `README.md` | This file |

## Quick Start

1. Copy `NEW_FILE_MetricChange.cs` to `unity-project/Assets/Scripts/UI/MetricChange.cs`
2. Copy `NEW_FILE_MetricsAnimator.cs` to `unity-project/Assets/Scripts/UI/MetricsAnimator.cs`
3. Apply changes from `DIFF_MetricsPanelUI.cs.diff` to `MetricsPanelUI.cs`
4. Apply changes from `DIFF_AudioCommandHandler.cs.diff` to `AudioCommandHandler.cs`
5. In Unity, add MetricsAnimator component and assign the 6 SFX clips

## SFX Files Location

The SFX files already exist at:
- `Assets/Audio/SFX/0_metrics/engagement/up_swell.wav`
- `Assets/Audio/SFX/0_metrics/engagement/down_wobble.wav`
- `Assets/Audio/SFX/0_metrics/sanity/up_swell.wav`
- `Assets/Audio/SFX/0_metrics/sanity/down_wobble.wav`
- `Assets/Audio/SFX/0_metrics/compliance/up_swell.wav` (for Suspicion)
- `Assets/Audio/SFX/0_metrics/compliance/down_wobble.wav` (for Suspicion)

## Configuration

After setup, you can adjust in the MetricsAnimator Inspector:
- `animationDuration`: Bar animation length (default: 1.0s)
- `sequenceDelay`: Delay between metrics (default: 0.75s)
- `highlightIntensity`: Pulse brightness (default: 0.3)
