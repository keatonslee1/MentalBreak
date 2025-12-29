# Mental Break Writing Guide

## Tone
Grounded, dry, slightly sad sci-fi comedy. *Black Mirror* written by someone who's worked in an actual office. Wry, human, resigned. No speeches about the system being bad; characters just try to get through their day.

## Pacing
- **60-90 seconds** to first choice
- Lines are **1-2 sentences**
- Quick back-and-forth, no monologues
- No emdashes

## The Cutting Test
For every line: "Does the player need this RIGHT NOW?" If no, cut it.

**Cut:**
- What's about to happen (the scene will show it)
- What just happened (the player saw it)
- What the art will show (descriptions of appearance)
- References to paths the player hasn't seen
- Physical actions implied by dialogue ("walks over to say bye" before "Hey, I'm heading out")
- Alice summarizing outcomes ("Party confirmed. Charlotte intel acquired.")

## Alice's Commentary
**Speaks when:** Setting up a choice. Essential info. Brief reaction to major outcome.
**Silent when:** Summarizing. Explaining meaning. Meta-commentary. Filling silence.

## Character Voices

**Timmy:** College kid. "dude," "man," "kinda." Jokes to deflect. No theatrical metaphors.
- ✓ "I got to the 'Are you sure?' screen and just... froze."
- ✗ "I feel like I'm auditioning for a life I'll never have."

**Ari:** Plain, lightly mocking. Cares via teasing. Not a therapist.
- ✓ "We're going downstairs in five. If you're coming, put on shoes."
- ✗ "I'm saying it's already a choice. You're just pretending it isn't."

**Arthur:** Dry, tired, self-deprecating. Couches ethics in metrics language.
- ✓ "I wrote a memo. They turned it into a training example for 'how not to block innovation.'"

**Alice:** Sharp, dry, systems-thinker. Humor dodges vulnerability. Not a motivational poster.
- ✓ "Nervousness is just a word for 'updating faster than you'd like.'"
- ✗ "They erase the one inconvenient little spark that thought maybe there was another way."

**Clerk:** Theatrical, treats horror like SKU differences.
- ✓ "Welcome to the Company Store, where your moral compromises turn into fabulous prizes."

## Moral Choices
- Embedded in situations, not announced in speeches
- **Sidegrades:** Both options attractive AND concerning
- Don't spell out the moral; let players feel the tension
- The choice is often in what doesn't get said

## Worldbuilding
Implied through throwaway comments, UI labels, glitches. No infodumps. Let weird terms appear without explanation.

## Yarn Format
```
// PURPOSE: [One line]
title: R1D1_NodeName
tags: #run_1 #day_1 #morning #scene #char_Alice
---
<<bg bg_location>>

[Minimal exposition]

Character: Dialogue.

-> Choice text
    <<jump NextNode>>
===
```

## Checklist
- [ ] First choice within 60-90 seconds
- [ ] No line describes what art will show
- [ ] No line summarizes what just happened
- [ ] No references to unseen paths
- [ ] Alice speaks only when necessary
- [ ] All dialogue 1-2 sentences, no emdashes
- [ ] Each character sounds distinct
- [ ] Moral tension felt, not preached
- [ ] Consequences shown, not explained
