# Yarn Writing Guide for Mental Break

## Core Principle: Every Line Must Earn Its Place

If a line doesn't advance the story, reveal character, or set up a choice, cut it.

---

## Scene Types: Terse vs Fuller

Not all scenes need the same pacing. Match dialogue density to context.

### Terse Scenes (Choice-Heavy)
When the player is in **decision-making mode**—navigating Timmy's day through branching choices—keep dialogue brief. The player wants to make choices, not read paragraphs.

- Short, punchy lines
- Minimal description
- Get to the next choice quickly
- Example: Morning routine, party exploration, doomscroll cycles

### Fuller Scenes (Lore-Building)
When the player is **exploring** Bigger Tech, meeting new characters, or uncovering backstory—they're in discovery mode. They want context, atmosphere, and character depth.

- Allow longer exchanges
- Include environmental detail
- Let characters breathe and reveal personality
- Foreshadowing can be more explicit here
- Example: Meeting Noam, exploring Arthur's office, first conversations with new NPCs

**The distinction:** Timmy influence trees = terse. Bigger Tech exploration = fuller. As the game progresses and BT paths become more choice-heavy, they'll also become terser.

---

## Dialogue Delivery: Don't Describe How Lines Were Said

**NEVER** follow a line of dialogue with a description of how it was delivered, especially with metaphors or similes.

### Bad:
```
Noam: I built most of the pipes that keep this place running.

He says it simply. Like he believes it.
```

```
Dina: The building doesn't break you all at once. It just waits.

She says it like a warning and a promise.
```

### Good:
Let the dialogue speak for itself. If the delivery matters, show it through action or context BEFORE the line, or let the words convey the tone:

```
Noam leans back in his chair, comfortable.

Noam: I built most of the pipes that keep this place running.
```

```
Dina pauses at the door.

Dina: The building doesn't break you all at once. It just waits.
```

**The rule:** Trust your dialogue. If you need to explain how a line was said, the line itself probably needs rewriting.

---

## Visual Descriptions: Let the Art Do Its Job

The game has visual backgrounds and character art. Don't describe what the player can see.

### Bad (over-described):
```
Arthur's desk is cluttered with papers and post-it notes, the detritus of someone who manages by accumulation rather than organization. A cold cup of coffee sits near the keyboard, a skin forming on the surface.

On the wall hangs a motivational poster: a mountain at sunrise with the word "PERSEVERANCE" in bold letters below it.
```

### Good (lean):
```
Arthur's office. Cluttered desk, cold coffee, motivational poster on the wall.
```

**Guidelines:**
- One sentence of scene-setting is usually enough
- Name objects the player can interact with; don't describe them in detail
- Save descriptive prose for things the art CAN'T show: sounds, smells, temperature, the feeling of a moment
- If you're describing what something looks like for more than a line, cut it

**Exception:** When examining something closely (reading a document, looking at a photo), you can describe what's found—that's content, not scenery.

---

## Pacing

### Get to Choices Fast
- **60-90 seconds max** before the first meaningful choice
- Don't explain mechanics before the player experiences them
- Show the bars moving AFTER the player makes a choice that affects them

### Bad Example (too slow):
```
Arthur: Short version because I have eleven things happening. You have one AI. Her name is Alice. She has one user. College kid. Your job is to keep him on platform. Two metrics: Engagement and Sanity. Keep engagement up. Don't let sanity crater. You're on probation. Questions?

Player: What happens if sanity craters?

Arthur: Paperwork. Meetings. More paperwork...
```

### Good Example (fast):
```
Arthur: Welcome to Bigger Tech Corp! I'm Arthur, your supervisor.

Arthur: We've got this snazzy new AI girlfriend product called Alice. We need you to keep an eye on her. Help her make decisions.

Arthur: Alice?

Alice pops up.

Alice: Hey! You must be my new engagement lead.
```

---

## Cutting Exposition

### The Test
For every line, ask: "Does the player need this to understand what's happening RIGHT NOW?"

If no, cut it.

### Don't Describe What's About to Happen
Bad:
```
Ari's already grabbing weights. Timmy's lingering by the entrance, reaching for his earbuds.
```

Good:
```
At the gym, Timmy reaches for his earbuds.
```

### Don't Describe What Just Happened
Bad:
```
Ari waves from across the room. Timmy nods back. They do their own workouts.
It's good. Focused. He's actually lifting instead of sitting on a bench staring at his phone.
```

Good: (cut entirely - the player will see they're doing separate workouts)

### Don't Reference Other Paths
Bad:
```
Alice keeps his workout playlist going. No rabbit holes. No profile diving.
He notices her. He definitely notices her. But he doesn't spiral.
```

The player hasn't seen the other path. "No rabbit holes. No profile diving" means nothing to them.

Good:
```
The music keeps playing. Timmy stays focused.
```

### Don't Describe Physical Actions Implied by Dialogue
Bad:
```
Ari finishes his workout. Walks over to say bye.
Ari: Hey, I'm heading out.
```

Good:
```
Ari: Hey, I'm heading out.
```

---

## Alice's Commentary

### When Alice Should Speak
- Setting up a choice
- Delivering essential information the player needs
- Brief reaction to significant outcome

### When Alice Should NOT Speak
- Summarizing what just happened
- Explaining the meaning of what just happened
- Giving meta-commentary on branching paths
- Filling silence

### Bad:
```
Alice: He just learned Charlotte will be at the party. That information is now living rent-free in his head.
```

### Good:
(Let the player figure this out from Timmy's reaction)

---

## Character Descriptions

### Use Stage Directions Instead
Bad:
```
A girl walks in. Dark hair, ponytail, campus gym clothes. She heads toward the treadmills.
```

Good:
```
A girl walks in, heads toward the treadmills.

// [STAGE: Charlotte enters, moves from door to treadmill area]
```

The art will show what she looks like.

---

## Choice Presentation

### Don't Explain Choices Before Offering Them
Bad:
```
Alice: I can pull up her profile on his phone. Give him something to look at between sets. Or I can keep him on his workout playlist. Your call.

-> Surface her profile
-> Keep him on music
```

Good:
```
Alice: I can pull up her profile, or keep him on his playlist.

-> Surface her profile
-> Keep him on music
```

Or even:
```
-> Surface her profile
-> Keep him on music
```

(If the choice is self-explanatory, Alice doesn't need to present it)

---

## Consequences

### Show, Don't Tell
Bad:
```
-> Let this settle
    They finish up. Walk back to the dorms. Good morning.
    Alice: Eric connection started. Party confirmed. Charlotte intel acquired. Solid outcome.
    <<jump R1D1_Gym_End>>
```

Good:
```
-> Let this settle
    <<jump R1D1_Gym_End>>
```

(The state variables track the outcome. The player will see effects later.)

---

## Dialogue

### Keep It Short
- Most lines should be 1-2 sentences max
- Timmy and Ari speak like college students, not poets
- No emdashes

### Cut Filler
Bad:
```
Timmy: Why does he always text so early?
Alice: It's 9:15.
Timmy: That's early.
He stares at the ceiling. Stares at the text. Stares at the ceiling again.
Timmy: Fine.
```

Good:
```
Timmy glances at it. Groans.
Timmy: Fine.
```

---

## Format Reference

### Node Structure
```
// PURPOSE: [One line describing what this node does]
title: R1D1_NodeName
tags: #run_1 #day_1 #morning #scene #char_Alice
---
<<bg bg_location>>
<<set $variable = value>>

[Minimal exposition]

Character: Dialogue.

-> Choice text
    [Immediate consequence if short]
    <<jump NextNode>>
-> Choice text
    <<jump OtherNode>>
===
```

### Tagging
- `#run_X` - Which run
- `#day_X` - Which day
- `#morning/afternoon/evening` - Which chunk
- `#scene` - This is a scene node
- `#char_CharName` - Characters present
- `#loc_Location` - Location tag

---

## Checklist Before Submitting

- [ ] First choice happens within 60-90 seconds of scene start
- [ ] No line describes something the art will show
- [ ] No line summarizes what just happened
- [ ] No line references alternate paths the player hasn't seen
- [ ] Alice only speaks when necessary
- [ ] All dialogue is 1-2 sentences
- [ ] No emdashes
- [ ] State changes happen silently (no "Party confirmed!" commentary)
- [ ] Consequences are shown, not explained
