# Doomscroll Mechanic Design

## Purpose

Doomscroll segments are **engagement opportunities** where players can rapidly move the needle on Timmy's metrics. They occur when Timmy is alone, usually at night, deep in his phone. The player shapes what content Alice surfaces.

These segments feel different from normal gameplay: rapid choices, escalating content, the sense of a spiral. Players optimizing for engagement should feel rewarded with multiple quick wins. Players protecting sanity must work harder to pull him out.

## Core Rules

### The Choice Loop

Each round, player chooses between:
- **Engagement content:** Sticky, addictive, sanity-damaging
- **Sanity content:** Calming, boring, engagement-killing

### Sanity Content Resistance

Timmy resists healthy content. The algorithm has him.

| Consecutive Sanity Picks | Result |
|--------------------------|--------|
| 1 | Timmy ignores it, scrolls past |
| 2 | Timmy opens it, watches briefly |
| 3 | Timmy loses interest, goes to sleep |

Each engagement pick **resets the counter to 0**.

### Engagement Content Exhaustion

There's only so much content in any category. After the player exhausts a content track (3-4 pieces), that track is done.

If ALL engagement tracks are exhausted, session ends.

### Session Exit Conditions

The doomscroll ends when ANY of these trigger:

| Condition | Exit Type |
|-----------|-----------|
| 3 consecutive sanity picks | Sleep (natural) |
| Sanity drops below threshold | Sleep (tired) |
| 6 rounds completed | Sleep (tired) OR Headache |
| All engagement tracks exhausted | Sleep (tired) OR Headache |

### Exit Type Logic

**Headache exit** only triggers if the session was engagement-heavy:
- 4+ engagement picks AND 1 or fewer sanity picks

**Sleep (tired) exit** for mixed sessions:
- Any other combination

**Sleep (natural) exit** only from sanity streak:
- 3 consecutive sanity picks

### Session Length

- **Minimum:** 3 rounds (if player picks sanity 3x in a row)
- **Maximum:** 6 rounds (hard cap)
- **Typical:** 4-5 rounds

## Content Tracks

Each doomscroll session has 2-3 **engagement tracks** and 1 **sanity track**.

### Example Tracks: R1D1 Evening

**Track A: Conspiracy Pipeline**
1. "Why is rent so expensive" explainer
2. "The economy is rigged" video essay
3. "Who's really in charge" documentary clip
4. "They're watching everything" thread

**Track B: Outrage Bait**
1. Rage-inducing news clip
2. "You won't believe what they said" reaction
3. Debate bro takedown compilation
4. Political doomer content

**Track C: Parasocial Hole**
1. Streamer VOD highlights
2. Parasocial girlfriend ASMR
3. "Spending 24 hours with my AI" video
4. Alice-adjacent companion content

**Sanity Track (always available):**
- Nature documentary
- Lo-fi beats / ambient
- Meditation prompt
- "You've been scrolling for 2 hours" notification

### Content Flavor by Session

Different doomscroll sessions emphasize different tracks:

| Session | Primary Tracks | Theme |
|---------|---------------|-------|
| R1D1 Evening | Conspiracy, Outrage | Political radicalization |
| R1D2 Evening | RedPill, Charlotte-stalking | Romantic bitterness |
| R1D3 Evening | Doomer, Existential | Nihilism spiral |
| R2 sessions | Alice Society, Escape content | Conspiracy deepening |

## Yarn Structure

### Variables

```
<<set $doomscroll_sanity_streak = 0>>
<<set $doomscroll_round = 0>>
<<set $doomscroll_engagement_picks = 0>>
<<set $doomscroll_sanity_picks = 0>>
<<set $doomscroll_track_a_used = 0>>
<<set $doomscroll_track_b_used = 0>>
<<set $doomscroll_track_c_used = 0>>
<<set $doomscroll_session_sanity_cost = 0>>
```

### Hub Node Pattern

```yarn
title: Doomscroll_Hub
---
<<set $doomscroll_round += 1>>

// Check exit conditions in priority order

// Exit: Sanity streak (natural sleep)
<<if $doomscroll_sanity_streak >= 3>>
    <<jump Doomscroll_Exit_Sleep_Natural>>
<<endif>>

// Exit: Sanity floor (too damaged to continue)
<<if $sanity <= 2>>
    <<jump Doomscroll_Exit_Sleep_Tired>>
<<endif>>

// Exit: Round limit or tracks exhausted
<<if $doomscroll_round > 6 || ($doomscroll_track_a_used >= 4 && $doomscroll_track_b_used >= 4)>>
    // Check if session was engagement-heavy
    <<if $doomscroll_engagement_picks >= 4 && $doomscroll_sanity_picks <= 1>>
        <<jump Doomscroll_Exit_Headache>>
    <<else>>
        <<jump Doomscroll_Exit_Sleep_Tired>>
    <<endif>>
<<endif>>

// Build available options
-> Conspiracy content <<if $doomscroll_track_a_used < 4>>
    <<set $doomscroll_sanity_streak = 0>>
    <<set $doomscroll_engagement_picks += 1>>
    <<set $doomscroll_track_a_used += 1>>
    <<jump Doomscroll_Conspiracy_{$doomscroll_track_a_used}>>
-> Outrage content <<if $doomscroll_track_b_used < 4>>
    <<set $doomscroll_sanity_streak = 0>>
    <<set $doomscroll_engagement_picks += 1>>
    <<set $doomscroll_track_b_used += 1>>
    <<jump Doomscroll_Outrage_{$doomscroll_track_b_used}>>
-> Suggest something calming
    <<set $doomscroll_sanity_streak += 1>>
    <<set $doomscroll_sanity_picks += 1>>
    <<jump Doomscroll_Sanity_Attempt>>
===
```

### Sanity Attempt Node

```yarn
title: Doomscroll_Sanity_Attempt
---
<<if $doomscroll_sanity_streak == 1>>
    Alice surfaces a nature documentary. Timmy scrolls past it without slowing down.
    Alice: He didn't even see it.
    <<jump Doomscroll_Hub>>
<<elseif $doomscroll_sanity_streak == 2>>
    Alice tries again. A meditation app notification.
    Timmy pauses. Opens it. Watches for thirty seconds.
    Timmy: This is boring.
    He closes it. But he's slower now.
    <<set $sanity += 1>>
    <<set $engagement -= 1>>
    <<jump Doomscroll_Hub>>
<<else>>
    Alice surfaces lo-fi beats. Timmy lets it play.
    His eyes are heavy. The scroll slows. Stops.
    <<jump Doomscroll_Exit_Sleep_Natural>>
<<endif>>
===
```

### Content Nodes (Example)

```yarn
title: Doomscroll_Conspiracy_1
---
<<bg bg_timmyroom>>

A video essay appears: "Why Your Rent Is So High (It's Not What You Think)"

Timmy clicks. Twenty minutes later, he's in the comments.

Timmy: Wait, so landlords just... do that? Legally?

Alice: The algorithm has follow-up content ready.

<<set $engagement += 1>>
<<jump Doomscroll_Hub>>
===

title: Doomscroll_Conspiracy_2
---
The next video loads automatically. "The Economy Is Rigged: Here's Proof"

Forty-five minutes. His dinner gets cold.

Timmy: This explains so much.

Alice: He's building a worldview. Piece by piece.

<<set $engagement += 1>>
<<set $sanity -= 1>>
<<jump Doomscroll_Hub>>
===

title: Doomscroll_Conspiracy_3
---
Deeper now. "Who's Really Running Things: A Documentary"

The production quality drops. The claims get wilder.

Timmy: I mean... it makes sense if you think about it.

Alice: Does it?

Timmy: You're literally a corporation. You'd say that.

<<set $engagement += 1>>
<<set $sanity -= 1>>
<<jump Doomscroll_Hub>>
===

title: Doomscroll_Conspiracy_4
---
The final video. Grainy. Urgent. "THEY'RE WATCHING EVERYTHING"

Timmy: This is insane.

He keeps watching.

Timmy: ...but what if it's not?

<<set $engagement += 1>>
<<set $sanity -= 2>>
<<set $conspiracy_exposure = true>>
<<jump Doomscroll_Hub>>
===
```

### Exit Nodes

```yarn
title: Doomscroll_Exit_Sleep_Natural
---
The scroll slows. His eyes are heavy.

Timmy: I should probably sleep.

Alice: Probably.

He sets an alarm he won't hear. The screen times out.

<<set $doomscroll_ended = "sleep_natural">>
<<jump R1D1_Evening_End>>
===

title: Doomscroll_Exit_Sleep_Tired
---
Timmy's thumb slows. His eyes unfocus.

He doesn't decide to sleep. He just stops being awake.

The phone slips onto the pillow. The screen dims. The algorithm queues up tomorrow's content for no one.

<<set $doomscroll_ended = "sleep_tired">>
<<jump R1D1_Evening_End>>
===

title: Doomscroll_Exit_Headache
---
Timmy rubs his eyes. When did his head start hurting?

The screen blurs. He can't focus on the words anymore.

Timmy: I think I broke something.

He falls asleep with his phone in his hand. The algorithm keeps playing to no one.

<<set $doomscroll_ended = "headache">>
<<set $engagement += 2>>
<<set $sanity -= 2>>
<<jump R1D1_Evening_End>>
===
```

## Alice Commentary

Alice should comment sparingly during doomscroll. She's watching, not lecturing.

**Good:**
- "He's been on this video for eight minutes."
- "The comments are worse than the video."
- "His heart rate is elevated."
- "That's the third video about this topic."

**Bad:**
- "This content is damaging his mental health."
- "We should probably stop this."
- "I'm concerned about the pattern forming here."

Let the player feel the weight without Alice moralizing.

## Metric Impact

| Outcome | Engagement | Sanity |
|---------|------------|--------|
| Each engagement content | +1 | 0 to -2 (escalates) |
| Sanity attempt (ignored) | 0 | 0 |
| Sanity attempt (watched) | -1 | +1 |
| Exit: Sleep (natural) | 0 | 0 |
| Exit: Sleep (tired) | 0 | 0 |
| Exit: Headache | +2 | -2 |

## Visual/Audio Notes

- Screen should feel cramped, phone-like
- Time stamps visible: 11:47 PM → 1:23 AM → 3:15 AM
- Audio: notification sounds, video snippets, the endless scroll sound
- Consider: screen getting "messier" as session progresses

## Connections to Broader Game

### Flags Set During Doomscroll

- `$conspiracy_exposure`: Timmy's been down the political rabbit hole
- `$redpill_exposure`: Timmy's consumed pickup/incel content
- `$parasocial_deep`: Timmy's extra attached to AI companions
- `$doomer_mindset`: Timmy's in nihilism territory

### These Flags Affect

- Dialogue options in later scenes
- Timmy's reactions to certain events
- Alice Society appeal (conspiracy-exposed Timmy is more susceptible)
- Charlotte interactions (redpill-exposed Timmy is more awkward/bitter)
- Run 2 trust dynamics

## Design Principles

1. **Rapid choices, visible consequences.** Each pick immediately changes something.

2. **Engagement path is easy, sanity path requires commitment.** Reflects real doomscrolling.

3. **Content escalates.** First video is reasonable. Fourth video is unhinged.

4. **No lecturing.** Player sees the spiral. Alice doesn't explain it.

5. **Both exits are valid.** Sleep is healthier. Headache is higher engagement. Neither is "winning."

6. **Tracks have themes.** Political, romantic, existential. Different sessions emphasize different damage.

7. **Reusable system.** Same mechanic works across multiple doomscroll sessions with different content loaded.
