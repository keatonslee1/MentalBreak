# Apply Pixel Art Aesthetic to Dialogue Box, Video Stage, and Buttons

## Current Issue
The dialogue box, video stage container, and choice buttons currently have rounded corners, soft shadows, and smooth styling that doesn't match a pixel art aesthetic.

## Solution
Apply pixel art styling to these elements:
1. `.mb-hero-stage` (video container)
2. `.mb-dialogue` (dialogue box)
3. `.mb-choice` (buttons)

**Pixel Art Aesthetic Features:**
- Remove border-radius (sharp, square corners)
- Use inset/outset borders for retro 3D effect
- Simplify shadows (remove or use solid borders instead)
- Add pixelated image rendering
- Use sharp, crisp styling

**Changes to make in [`landing-v35.css`](unity-project/webgl-build/assets/css/landing-v35.css):**

1. **`.mb-hero-stage` (lines 288-297):**
   - Remove `border-radius: 18px;`
   - Simplify or remove `box-shadow`
   - Consider adding inset border effect

2. **`.mb-hero-stage__bg` video (lines 299-309):**
   - Add `image-rendering: pixelated;` or `image-rendering: crisp-edges;` for pixelated video rendering
   - Remove soft transform/scale effects or make them sharper

3. **`.mb-dialogue` (lines 337-356):**
   - Remove `border-radius: 16px;`
   - Simplify or remove `box-shadow`
   - Consider inset border styling

4. **`.mb-choice` buttons (lines 375-389):**
   - Remove `border-radius: 14px;`
   - Simplify shadows
   - Use inset/outset borders for pressed/unpressed states
   - Make hover states more "clicky" and pixelated

