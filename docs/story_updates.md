# Story Updates Log

## World-Building / Lore

### Bigger Tech Monitoring Policy
- **Standard monitoring:** Most of Bigger Tech has passive surveillance (screensavers, ambient systems, network tracking)
- **Supervisor offices EXEMPT:** Arthur's office (and other supervisor offices) have no monitoring due to sensitive conversations (performance reviews, disciplinary matters, confidential calls)
- **Gameplay implication:** Player can safely snoop in Arthur's office; Alice can reassure hesitant players about this

---

## Character Updates

### Arthur Russell (formerly unnamed/Chen)
- **Full Name:** Arthur Russell
- **Ethnicity:** White
- **Role:** Floor Supervisor, Engagement Lead Division
- Arthur is NOT named Chen. Update any references.

### Noam (Revised Characterization)
- **Original:** "Perpetually overworked tech person with frazzled competence and dark humor"
- **Updated:** Warm, friendly presence. The warm light in a cool server room.
- **Personality:** Middle-aged, probably a father, even-tempered. NOT jaded or cynical.
- **Beliefs:** Thinks the company is doing necessary work. Sympathizes with new hires who don't understand the ropes yet.
- **Role:** Lead IT guy. Built a lot of the data plumbing that keeps Bigger Tech running.
- **Why he helps:** He disagrees with higher-ups about transparency. Thinks engagement leads should be able to see their Compliance status to do their jobs better.

## Terminology Updates

### Compliance Meter (formerly "Suspicion Meter")
- The metric that tracks how much attention the player is attracting from Compliance/security
- Renamed from "suspicion" to "compliance" for consistency with the Compliance Officer character
- Noam gives the player access to this meter because he believes in transparency
- **DIRECTION:** Compliance goes DOWN when you do something non-compliant (100 = fully compliant, 0 = red flag)

## Character Backstory

### David (Former Engagement Lead)
- David tried to escape Bigger Tech. He failed.
- His legend echoes in fragments throughout the building
- His desk remains untouched (Minesweeper high scores still visible)
- Dina won't say his name, just hints about "someone who asked questions"
- Noam speaks warmly but carefully about him
- His "badge" pings behind the potted plant at the end of the hall (Noam mentions this in second meeting)
- **The badge is actually a signal jammer** hidden under a root (plant grew over it). Player can find this after Noam's hint.
- Alice picked up the Minesweeper habit from working with David; both plant notes to themselves in high score boards

## Environment Updates

### Arthur's Office
- **Motivational Poster:** "PERSEVERANCE" poster is TAPED ON TOP OF a whiteboard
- **Behind the poster:** Old notes Arthur made with ideas for improving Bigger Tech Corp (the Ethical Engagement Framework notes)
- **Mystery Photo:** Picture of Arthur shaking hands with a man in a suit. Room too dark, photo too old/low quality to make out the man's features. (This is secretly the Dark Figure.)

## Variable Naming
- `$suspicion_hud` should be renamed to `$compliance_hud`
- Related variables should use "compliance" not "suspicion"
