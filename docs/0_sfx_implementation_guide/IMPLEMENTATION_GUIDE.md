# Metrics SFX Animation Implementation Guide

This guide explains exactly how to implement the metrics animation system with SFX.

## Overview

When a metric (Engagement, Sanity, or Suspicion) changes:
1. Play the appropriate SFX (up_swell.wav or down_wobble.wav)
2. Highlight the metric panel with a background color pulse
3. Animate the bar fill over 1 second
4. If multiple metrics change, animate them sequentially with 0.75s delay

Order: Engagement → Sanity → Suspicion (skip any that didn't change, skip Suspicion if hidden)

## Files to Create

### 1. `unity-project/Assets/Scripts/UI/MetricChange.cs`
Copy the contents from `NEW_FILE_MetricChange.cs` in this folder.

### 2. `unity-project/Assets/Scripts/UI/MetricsAnimator.cs`
Copy the contents from `NEW_FILE_MetricsAnimator.cs` in this folder.

## Files to Modify

### 1. `unity-project/Assets/Scripts/UI/MetricsPanelUI.cs`
Apply the changes described in `DIFF_MetricsPanelUI.cs.diff`.

**Summary of changes:**
- Add 3 private fields for background Images (engagementBackground, sanityBackground, suspicionBackground)
- Add a `useAnimatedUpdates` flag
- Add a `#region Public Accessors` with properties exposing all UI elements
- Modify `CreateMetricPanel` to output the background Image
- Update the 3 calls to `CreateMetricPanel` to capture background references
- Wrap the fill/text updates in `UpdateMetrics()` with `if (!useAnimatedUpdates)`
- Wrap the fill/text updates in `UpdateSuspicionPanel()` with `if (!useAnimatedUpdates)`

### 2. `unity-project/Assets/Scripts/Audio/AudioCommandHandler.cs`
Apply the changes described in `DIFF_AudioCommandHandler.cs.diff`.

**Summary of changes:**
- Add `PlaySFXClip(AudioClip clip)` method for direct clip playback

## Unity Setup After Code Changes

1. **Add MetricsAnimator component:**
   - Find the GameObject that has MetricsPanelUI (or create a sibling)
   - Add the MetricsAnimator component

2. **Assign SFX clips in Inspector:**
   - Engagement Up: `Assets/Audio/SFX/0_metrics/engagement/up_swell.wav`
   - Engagement Down: `Assets/Audio/SFX/0_metrics/engagement/down_wobble.wav`
   - Sanity Up: `Assets/Audio/SFX/0_metrics/sanity/up_swell.wav`
   - Sanity Down: `Assets/Audio/SFX/0_metrics/sanity/down_wobble.wav`
   - Suspicion Up: `Assets/Audio/SFX/0_metrics/compliance/up_swell.wav`
   - Suspicion Down: `Assets/Audio/SFX/0_metrics/compliance/down_wobble.wav`

3. **References (auto-found if not set):**
   - `metricsPanel`: Will auto-find MetricsPanelUI
   - `audioHandler`: Will auto-find AudioCommandHandler

## Configuration Options

In MetricsAnimator Inspector:
- `animationDuration`: How long the bar animation takes (default: 1.0s)
- `sequenceDelay`: Delay between metrics when multiple change (default: 0.75s)
- `highlightIntensity`: How bright the pulse effect is (default: 0.3)
- `pollInterval`: How often to check for changes (default: 0.1s)

## How It Works

1. **MetricsAnimator** polls Yarn variables ($engagement, $sanity, $alert_level) every 0.1s
2. Compares to previous values to detect changes
3. When changes detected, queues them in priority order
4. Processes queue with coroutines:
   - Plays SFX immediately
   - Starts background pulse coroutine
   - Animates fill amount with SmoothStep interpolation
   - Updates value text during animation
5. **MetricsPanelUI** skips its instant updates when `useAnimatedUpdates = true`

## Edge Cases Handled

- **Initial load**: Previous values set to current without animating
- **Rapid changes**: Queued and played sequentially
- **Scene transitions**: Queue cleared, coroutines stopped in OnDisable()
- **Suspicion hidden**: Skipped when $suspicion_hud_active is false
- **Suspicion color**: MetricsPanelUI still handles dynamic color (green→orange→red)
