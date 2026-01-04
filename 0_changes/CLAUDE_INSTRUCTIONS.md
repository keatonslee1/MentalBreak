# Instructions for Claude: Implementing Metrics SFX Animation

## Your Task

Implement a metrics animation system that plays SFX and animates the UI when engagement, sanity, or suspicion metrics change.

## Step-by-Step Instructions

### Step 1: Create MetricChange.cs

Create a new file at `unity-project/Assets/Scripts/UI/MetricChange.cs`.

Copy the EXACT contents from `NEW_FILE_MetricChange.cs` in this folder.

### Step 2: Modify MetricsPanelUI.cs

Open `unity-project/Assets/Scripts/UI/MetricsPanelUI.cs`.

Apply the 6 changes described in `DIFF_MetricsPanelUI.cs.diff`:

1. **After line 112** (after `private Image suspicionFillImage;`), add:
   - 3 new private fields for background Images
   - The `useAnimatedUpdates` flag
   - The entire `#region Public Accessors` block

2. **In CreateMetricPanel signature**, add `out Image backgroundImage` parameter

3. **In CreateMetricPanel body**, add `backgroundImage = null;` after other nulls, and `backgroundImage = bgImage;` after bgImage is created

4. **Update the 3 CreateMetricPanel calls** to include the new out parameter

5. **In UpdateMetrics()**, wrap the fill/text updates in `if (!useAnimatedUpdates) { ... }`

6. **In UpdateSuspicionPanel()**, restructure to only update fill/text when `!useAnimatedUpdates`

### Step 3: Modify AudioCommandHandler.cs

Open `unity-project/Assets/Scripts/Audio/AudioCommandHandler.cs`.

After the `PlaySFX` method (after line 258), add the new `PlaySFXClip` method.

See `DIFF_AudioCommandHandler.cs.diff` for exact code.

### Step 4: Create MetricsAnimator.cs

Create a new file at `unity-project/Assets/Scripts/UI/MetricsAnimator.cs`.

Copy the EXACT contents from `NEW_FILE_MetricsAnimator.cs` in this folder.

### Step 5: Unity Editor Setup

After the code compiles:

1. Find the Dialogue System GameObject (or wherever MetricsPanelUI lives)
2. Add a MetricsAnimator component
3. Assign the 6 AudioClip references:
   - Engagement Up: `Assets/Audio/SFX/0_metrics/engagement/up_swell.wav`
   - Engagement Down: `Assets/Audio/SFX/0_metrics/engagement/down_wobble.wav`
   - Sanity Up: `Assets/Audio/SFX/0_metrics/sanity/up_swell.wav`
   - Sanity Down: `Assets/Audio/SFX/0_metrics/sanity/down_wobble.wav`
   - Suspicion Up: `Assets/Audio/SFX/0_metrics/compliance/up_swell.wav`
   - Suspicion Down: `Assets/Audio/SFX/0_metrics/compliance/down_wobble.wav`

## Key Points

- The delay between successive metric animations is 0.75 seconds
- Each bar animation takes 1 second
- The background pulses (brightens then fades) during the animation
- Order is always: Engagement → Sanity → Suspicion
- Suspicion is skipped if $suspicion_hud_active is false

## Testing

1. Play the game
2. Make a dialogue choice that changes a metric
3. You should hear the SFX, see the panel highlight, and see the bar animate smoothly
4. Try choices that change multiple metrics to see the sequential animation

## Troubleshooting

- If SFX don't play: Check AudioClip assignments in Inspector
- If animation doesn't work: Check that MetricsAnimator found MetricsPanelUI (check console)
- If bars update instantly: Ensure MetricsAnimator.Start() sets `metricsPanel.UseAnimatedUpdates = true`
