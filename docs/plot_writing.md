# Plot Writing Best Practices for Mental Break

## Core Structure

### Game Architecture
- **4 runs** (acts), each revealing new twists
- **3 days per run**
- **3 chunks per day:** Morning, Afternoon, Evening
- **Per chunk:** Player chooses to focus on Timmy OR explore Bigger Tech
  - If Timmy: Player controls Alice's influence on him
  - If Bigger Tech: Alice handles Timmy on autopilot (engagement-optimized)
  - Exception: R1D1 Morning is forced Timmy focus (tutorial)

### Branching Pattern
- Target: **2Ã¢â€ â€™4Ã¢â€ â€™8 expansion** where possible
- Use **diamond convergence** to manage scope (branches that reconverge)
- Each layer should lead to **genuinely different situations**, not minor variations

---

## Choice Design

### Number of Options
- **Default: 2 options** (binary choice)
- **Maximum: 3 options** (rare, avoid when possible)
- **Never: 4+ options** at a single node

### Depth Over Width

**THE CRITICAL PRINCIPLE:** When you have 3+ ideas for a branch point, do NOT present them as a pick-1-of-3 choice. Instead:
1. Pick 2 for the immediate binary choice
2. Use the 3rd idea (and others) to add DEPTH to subsequent layers
3. Build deep trees, not flat trees

**BAD (flat):**
```
[CHOICE: Dorm / Library / Cafeteria]
â”œâ”€â”€ DORM â†’ stuff happens
â”œâ”€â”€ LIBRARY â†’ stuff happens
â””â”€â”€ CAFETERIA â†’ stuff happens
```

**GOOD (deep):**
```
[CHOICE: Stay on campus / Head back to dorm]
â”œâ”€â”€ STAY_ON_CAMPUS
â”‚   â””â”€â”€ Goes to cafeteria
â”‚       â””â”€â”€ [CHOICE: Eat alone / Join Ari's table]
â”‚           â”œâ”€â”€ EAT_ALONE â†’ [CHOICE: Leave after / Linger]
â”‚           â””â”€â”€ JOIN_TABLE â†’ Group heads to library
â”‚               â””â”€â”€ [CHOICE: Go with them / Split off]
â”‚
â””â”€â”€ HEAD_TO_DORM
    â””â”€â”€ Runs into Martin on the path
        â””â”€â”€ [CHOICE: Stop and chat / Keep walking]
            â”œâ”€â”€ CHAT â†’ Martin invites him to library study group
            â”‚   â””â”€â”€ [CHOICE: Accept / Decline]
            â””â”€â”€ KEEP_WALKING â†’ Back to dorm alone
```

The deep version has 6+ choice points. The flat version has 1. Same number of locations, vastly more player agency.

**How to apply this:**
- When brainstorming produces 3 ideas, ask: "Which 2 are the immediate fork? Where does the 3rd become a subsequent choice?"
- Every branch should contain at least one more choice within it
- Characters can appear as "encounters along the way" to create natural branching
- Locations can be reached via multiple paths

### Anti-Patterns: Fake Depth

**Do NOT pad trees with these tricks:**

1. **Double-asking the same decision:** Surface notification â†’ push to respond â†’ "are you sure?" is the same choice asked three times. Pick ONE decision point.

2. **Branches that converge to the same thing:** If paths A, B, and C all lead to "goes home and games," that's not real branching. Each path needs genuinely different content.

3. **Pointless routing nodes:** "Stay here a sec" that leads nowhere interesting is just bloat. Either cut it or put real content in it.

4. **Encounters that re-ask previous questions:** If Timmy declined Ari's invite, having Eric appear to ask the same thing is lazy. Eric should offer something DIFFERENT or the encounter shouldn't exist.

5. **Choices without plausible influence mechanism:** If Alice can't realistically cause something to happen, it's not a valid choice. Every choice needs a clear HOW.

**The test:** For each branch, ask "What INTERESTING THING happens here that doesn't happen in the other branch?" If the answer is just "routing" or "the same thing with extra steps," delete it and think harder.

### What Makes a Good Choice
- Leads to **different locations, people, or situations**
- Has **clear trade-offs** (not good vs bad, but value vs value)
- Player understands **what they're choosing** (mechanism is clear)
- Consequences are **visible and meaningful**

### Scale of Alice's Interventions

Alice coaching Timmy on what to say should only occur for **medium or large stakes moments**. 

**BAD (too minor):**
- "Ask 'what game?' / Stay noncommittal" - Timmy wouldn't obsess over this, wouldn't ask Alice, and Alice couldn't realistically coach him in a split second of casual conversation
- "Say hi / Stay quiet" when passing an acquaintance
- Any micro-dialogue choice in a flowing conversation

**GOOD (appropriate stakes):**
- "Ask her out / Let the moment pass" - high stakes, Timmy is genuinely frozen
- "Commit to the party / Hedge" - meaningful decision with consequences
- "Be honest with mom / Deflect" - shapes a relationship
- "Join them / Head home alone" - determines where the scene goes

**The test:** Would Timmy actually pause, feel uncertain, and benefit from a nudge? If it's something any person would just... do naturally in conversation, it's too small for a choice.

### What Makes a Bad Choice
- Multiple options that lead to **minor variations of the same outcome**
- "Choose your arrival time" style branches
- Options where one is obviously correct
- Choices the player can't meaningfully influence through Alice

### Choice Types by Impact

Not all choices need to change the plot. Use these intentionally:

**Plot Choice:** Changes where the story goes
- Example: "Surface Ari's text / Bury it" → Gym path vs Dorm path
- These are the backbone of branching

**Flavor Choice:** Changes dialogue color but not plot direction
- Example: "Knock / Wait for him to finish" at Arthur's door
- Both lead to same scene, but feel different
- Use sparingly; don't pad trees with these

**Reactionary Choice:** Player expresses attitude, story acknowledges it, then continues
- Example: Player can be skeptical or trusting toward Alice's explanation
- Alice responds differently, but plot proceeds same direction
- Good for characterizing the Player's personality

**Convergent Choice:** Branches that immediately reconverge
- Example: Three ways to explore the party, all lead back to main floor
- Use for texture and exploration without scope explosion
- Each branch should still have unique content worth seeing

---

## Cross-Character Awareness

Characters should notice what the player has done elsewhere. This adds texture and makes the world feel connected.

**Examples:**
- If player met Dina in afternoon, Noam might say: "Dina mentioned someone new was poking around."
- If player found David's desk, Alice might comment: "You found his desk."
- If player explored Arthur's office, Alice: "I see you've been snooping."

**Guidelines:**
- Keep references brief and natural
- Don't make them feel like achievements unlocking
- Characters share information plausibly (coworkers talk)
- Alice sees most things—she can reference almost anything

---

## Bigger Tech Environment Notes

### Arthur's Office - No Monitoring
Arthur's office has **no Bigger Tech surveillance monitoring**. As a supervisor, he has sensitive conversations in his office (performance reviews, disciplinary matters, confidential calls). Corporate policy exempts supervisor offices from the standard monitoring mesh.

**Implications for gameplay:**
- Player can snoop in Arthur's office without being detected
- Alice can reassure a hesitant player: "Supervisor offices aren't monitored. Sensitive conversations."
- This is why Arthur's office is a safe place to discover secrets (whiteboard, photo, files)

### The Engagement Floor
- Mostly empty—"used to be thirty leads, now it's ghosts"
- Monitoring is present but passive (screensavers, ambient systems)
- David's desk remains untouched—nobody wants to claim it

---

## Alice's Influence (Plausibility Rules)

### Alice CAN:
- **Control notifications:** Surface or bury texts, calls, reminders
- **Shape content feeds:** What videos, posts, profiles appear
- **Control IoT devices:** Noise cancelling, alarms, lights, screen brightness
- **Whisper suggestions:** "Say yes," "Ask about her," "You should go"
- **Surface information:** Pull up someone's profile, show a reminder
- **Timing:** When notifications arrive, how urgent they feel

### Alice CANNOT:
- Force Timmy to do physical actions
- Make him say specific words (only suggest)
- Control other people
- Read minds (only observe behavior, biometrics, content consumption)
- Be in places Timmy isn't (she's in his devices)

### Influence Examples
| Mechanism | Example |
|-----------|---------|
| Notification control | Surface Ari's gym text vs bury it |
| Content serving | Show Charlotte's Instagram vs keep him on music |
| IoT control | Noise cancelling ON vs OFF |
| Whispered coaching | "Just say yes" when party invite comes |
| Environmental | Alarms, lights, screen dimming |

---

## Conversation Design

### Depth Limits
- **Maximum 2-3 choice layers** within a single conversation
- If a conversation needs more depth, it should **lead to a new scene/location**
- No multi-stage dialogue trees where player makes 4+ choices in one exchange

### Character Voice (Early Game)
- **NPCs are not therapists.** Especially early in story.
- Ari says "You should come" not "What's going on with you lately? You've been distant."
- Friends are casual. Concern comes later, after relationship is established.
- Preachy dialogue = lazy writing

### Information Through Conversation
- Intel should come **naturally through dialogue**
- Example: Eric mentions "Charlotte's coming Saturday" as casual info, not exposition
- Avoid characters explaining things they wouldn't naturally say

---

## Location Management

### Finite Art Budget
Each location must justify its existence by being **reused across the game**.

### Full Backgrounds (20 planned)

**Bigger Tech:**
1. Supervisor's Office
2. IT/Server Room
3. Mailroom
4. Courtroom
5. Dark Room
6. Player's Desk
7. Engagement Floor (TBD if separate from Player's Desk)
8. Breakroom
9. Bigger Tech Hallway (includes janitor's closet, potted plant investigation points)
10. Conference Room / Board Room
11. Bigger Tech Company Store
12. Museum / Training Run Archive

**Timmy's World:**
13. College Library (Day, normal)
14. College Library (Night, Alice Society meeting variant)
15. Lecture Hall
16. Timmy's Room
17. College Cafeteria

**Other:**
18. Botnet (Alice's expansion visualization)
19. TBD
20. TBD

### Extra Graphics (Simple)
1. **Loading Screen** - Bigger Tech laptop with loading bar.
2. **Blue Screen of Death** - Custom entertaining error text.
3. **Map of World** - Alice spreads visualization.

### Location Usage Rules
- Each location should appear **multiple times across the game**
- Different things happen in same location based on time/state
- Don't create a location for a single scene
- Some locations have variants (e.g., Library day vs Library night/Alice Society)

---

## State and Variables

### Relationship Tracking
- Relationships start at **0** on Day 1
- Don't write branches that assume high values early (e.g., "if Ari +3" on D1 morning)
- Small increments: +1 for positive interaction, +2 for meaningful moment, +3 for breakthrough
- Negative possible: -1 for damage (lying, rejecting help)

### Key Flags vs Numeric Values
- **Flags** for binary states: Party_committed, Charlotte_aware, Made_class
- **Numbers** for relationships: Ari, Mom, Charlotte, Eric, Martin
- **Compound flags** for specific outcomes: Ari_knows_about_Charlotte, Lie_between_them

### State Gating
- Later options can be **gated by earlier choices**
- Example: Party only available in evening if invited earlier
- But don't over-gate early game; keep options open

---

## Pacing

### Opening
- **60-90 seconds maximum** before first meaningful choice
- Setup: Here's your job, here's Timmy, here's your first decision
- Save exposition for later; earn player's attention first

### Chunk Length
- Each chunk (morning/afternoon/evening) = **one major arc**
- Player should feel they **shaped something meaningful** in each chunk
- End chunks with clear state change and preview of what's next

### Day Length
- 3 chunks Ãƒâ€” meaningful choices = **player feels the day mattered**
- End of day = Arthur debrief, state summary, anticipation for tomorrow

---

## Moral Dimensions

### The Core Question
Every choice should connect to: **"How much do you optimize Timmy vs let him be human?"**

### Types of Moral Tension
1. **Engagement vs Sanity:** System rewards attention; person needs rest
2. **Connection vs Obsession:** Helping him notice Charlotte vs feeding fixation
3. **Intervention vs Autonomy:** Coaching him vs letting him fail/succeed on his own
4. **Truth vs Performance:** Real conversation vs surface-level check-ins
5. **Present vs Future:** Short-term metrics vs long-term wellbeing

### Showing Trade-offs
- Make both options **defensible**
- Player should sometimes feel torn
- Avoid obvious "good choice / bad choice" framing
- Consequences should be **visible but not preachy**

---

## Common Mistakes to Avoid

### Lazy Branching
Ã¢ÂÅ’ Multiple paths that all lead to "he goes to class, just at different times"
Ã¢Å“â€¦ Different choices lead to gym vs dorm vs class (different places, people, events)

### Over-Nested Dialogue
Ã¢ÂÅ’ 5 consecutive choices within one Ari conversation
Ã¢Å“â€¦ One choice in conversation, then scene changes based on result

### Implausible Influence
Ã¢ÂÅ’ Alice somehow makes Timmy show his phone to Ari
Ã¢Å“â€¦ Alice surfaces content; Ari happens to glance over; natural moment

### Preachy NPCs
Ã¢ÂÅ’ Ari: "You've been isolating yourself. That's not healthy. Talk to me."
Ã¢Å“â€¦ Ari: "You coming Saturday? Jake's thing."

### Front-loaded Exposition
Ã¢ÂÅ’ 10 minutes of Arthur explaining the company before gameplay
Ã¢Å“â€¦ 60 seconds of setup, then first choice, learn by doing

### Forgetting Plausibility
Ã¢ÂÅ’ Alice controls what Timmy physically does
Ã¢Å“â€¦ Alice shapes environment; Timmy responds to environment

---

## Checklist for New Scenes

Before finalizing a scene/branch:

- [ ] Are choices binary or max 3 options?
- [ ] Do different choices lead to genuinely different situations?
- [ ] Is Alice's influence mechanism plausible?
- [ ] Is conversation depth Ã¢â€°Â¤3 layers?
- [ ] Are NPCs acting naturally (not preachy)?
- [ ] Does this reuse an existing location or justify a new one?
- [ ] Are relationship assumptions realistic for this point in the story?
- [ ] Is there a meaningful trade-off (not obvious good/bad)?
- [ ] Does this connect to the core moral tension?
- [ ] Is pacing appropriate (not too much exposition)?

---

## File Naming Convention

```
[run]_[day]_[chunk].yarn

Examples:
r1_d1_morning.yarn
r1_d1_afternoon.yarn
r1_d2_evening.yarn
r2_d1_morning.yarn
```

## Variable Naming Convention

```
Relationships: ari, mom, charlotte, eric, martin, dina, noam, arthur
Flags: party_committed, charlotte_aware, made_class, missed_class
Compound: ari_knows_charlotte, lie_between_them, charlotte_obsession
Counters: engagement_score, sanity_score, society_seeds
```
