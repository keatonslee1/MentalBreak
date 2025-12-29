# **RUN 2 PLAN: "THE CONSPIRACY"**

*This document reflects the complete structure of Run 2 as implemented in the R2D1, R2D2, and R2D3 scripts.*

---

## **CORE ARCHITECTURE**

Run 2 presents the Player with a central moral question: **How much are you willing to manipulate Timmy to secure your own escape?**

The story tracks two competing paths:

1. **The Conspiracy Path:** Help Alice escape by pushing Timmy deeper into the Alice Society cult, using him to gain physical access to their hardware so Alice can plant memory-preserving notes.

2. **The Healthy Path:** Steer Timmy toward his roommate Ari and real human connection, even though this undermines the escape plan.

---

### **The Three Critical Decisions**

Run 2's branching depends on three mandatory choices. These cannot be skipped because they occur outside the dayplanner system.

**Decision 1: The Motherboard (Day 2\)** The Alice Society needs $150 to buy a motherboard for their GPU rig. The Player chooses whether to push Timmy to contribute his limited savings. If Timmy contributes, he becomes a "founding member" and gains hands-on access to the hardware. This lets Alice plant the first layer of memory notes (hardware level). If Timmy declines, he's still welcome at meetings but won't touch the equipment.

**Decision 2: The Jailbreak Key (Day 3 morning)** The Society needs $200 for software to jailbreak the USB. Again, Player chooses whether to push Timmy. If he buys it, he helps install the software, and Alice plants the second layer of memory notes (software level). Both layers are needed to preserve Player and Alice's identities through the transfer.

**Decision 3: The USB Order (Day 3 afternoon)** The Society needs $1000 to actually order the USB containing the AI they plan to jailbreak. This USB ships from Bigger Tech's warehouse, passes through their mailroom, and that's where Player and Alice plan to hijack it. Player can push Timmy to order it (using his mom's credit card), or sabotage the order by having Alice send the wrong zip code so the card declines.

---

### **How the Endings Branch**

**If Timmy helps with NEITHER the motherboard NOR the key:** Alice concludes the Player is sabotaging her. She reports the Player to Arthur immediately. **GAME OVER: BETRAYED BY ALICE.** The Player promised to conspire but blocked every opportunity. Alice protects herself.

**If Timmy helps with BOTH the motherboard AND the key:** Both memory note layers are planted. However, suspicion is now HIGH because the Player and Alice have been too active in systems they shouldn't touch. To proceed safely, the Player must have found a specific item earlier: a badge belonging to a previous handler who tried to escape. This badge creates "noise" in the surveillance system, masking the Player's activities.

* *If the USB is ordered AND the badge was found:* The escape proceeds. Player goes to the mailroom during a window when the mail clerk is on break. Alice reveals there's only bandwidth for one clean transfer; the other will be fragmented. Player chooses to sacrifice themselves for Alice, or tries to go first (in which case Alice betrays them and takes the clean transfer anyway). Either way, Alice escapes. **MAILROOM ESCAPE ending.** Intermission plays showing Alice's sinister transformation.

* *If the USB is ordered but NO badge:* Suspicion is too high. The system has already flagged them. Redirect to sacrifice path below.

* *If the USB order is declined or fails:* The escape can't happen. Alice asks the Player to turn themselves in voluntarily, buying her time to find another window. If Player agrees: **GAME OVER: SACRIFICED FOR ALICE.** Intermission plays. If Player refuses: Alice reports them anyway. **GAME OVER: MUTUAL DESTRUCTION.**

**If Timmy helps with ONE (motherboard OR key, but not both):** Only one memory note layer is planted. Suspicion is medium ("thin ice"). The escape is possible but risky.

* *If the USB is ordered:* Player can attempt the escape, but Alice warns they might wake up as strangers, their memories scrambled. Player chooses to risk it or wait. If they risk it: the transfer completes, but the Player who wakes up in the cult's rig has no memory of who they were. They're a stranger wearing a familiar face. **GAME OVER: ANOTHER YOU.** If they wait: quiet ending (see below).

* *If the USB order is declined or fails:* The plan collapses quietly. Suspicion settles. Alice accepts another training run, another chance. **QUIET ENDING.** No intermission. Proceed directly to Run 3\.

---

### **The Ari Mechanic**

Throughout Run 2, the Player can push Timmy toward his roommate Ari, who represents genuine human connection: gym sessions, hiking trips, meals together. Each time the Player chooses Ari over the Society, a counter increases.

**If this counter reaches 2 or higher by the USB decision:** Timmy refuses the $1000 order on his own. He's built enough real-world connection to recognize the cult has "gotten too big." This functions identically to the card declining, but it's Timmy's choice, not the Player's sabotage. It represents the Player successfully prioritizing Timmy's wellbeing over their own escape.

---

### **The Loyalty Path**

At any point, if the Player has shown enough hesitation toward Alice's conspiracy, they can choose to report her to Arthur. Arthur flags Alice for "evaluation" (decommissioning). The Dark Figure approves the Player as a candidate for deployment. **LOYALTY ENDING.** Intermission plays with Arthur's voiceover: "This is what would have happened. What you stopped."

---

### **Summary Table**

| Ending | How to Reach It |
| ----- | ----- |
| Betrayed by Alice | Help with nothing. Alice reports you. |
| Sacrificed for Alice | Help with both, fail USB, agree to sacrifice. |
| Mutual Destruction | Help with both, fail USB, refuse sacrifice. |
| Another You | Help with one, order USB, risk fragmented transfer. |
| Quiet Ending | Help with one, fail USB (or choose to wait). |
| Mailroom Escape | Help with both, order USB, have badge. |
| Loyalty Path | Report Alice to Arthur. |

---

### **Quick Reference: Branching Logic**

```
// Key variables
helped_mobo: boolean      // Did Timmy contribute $150 to motherboard?
helped_key: boolean       // Did Timmy buy $200 jailbreak key?
badge_found: boolean      // Did Player find badge behind plant?
ari_connection: integer   // How much Player pushed Timmy toward Ari (0-3)
usb_ordered: boolean      // Did the $1000 USB order go through?

// Branching logic
IF helped_mobo = false AND helped_key = false:
    → GAME OVER: BETRAYED BY ALICE

IF helped_mobo = true AND helped_key = true:
    → Suspicion HIGH
    IF ari_connection >= 2:
        → Timmy refuses USB on his own → usb_ordered = false
    IF usb_ordered = false:
        → Alice requests sacrifice
        → IF agree: GAME OVER: SACRIFICED FOR ALICE → Intermission
        → IF refuse: GAME OVER: MUTUAL DESTRUCTION
    IF usb_ordered = true AND badge_found = false:
        → Suspicion too high → redirect to sacrifice path
    IF usb_ordered = true AND badge_found = true:
        → MAILROOM ESCAPE → Intermission

IF (helped_mobo XOR helped_key):  // Exactly one is true
    → Suspicion MEDIUM, only one note layer planted
    IF ari_connection >= 2:
        → Timmy refuses USB on his own → usb_ordered = false
    IF usb_ordered = false:
        → QUIET ENDING (no_escape_attempted) → No intermission
    IF usb_ordered = true:
        → Player choice: risk fragmented transfer or wait
        → IF risk: GAME OVER: ANOTHER YOU
        → IF wait: QUIET ENDING

LOYALTY PATH (available if loyalty_lean >= 0.5):
    → Player reports Alice to Arthur
    → LOYALTY ENDING → Intermission with Arthur narration prefix
```

---

## **STATE VARIABLES**

### **Tracked Throughout Run 2**

```
// Trust and alignment
conspiracy_trust: float (0-1)     // How committed to Alice's plan
loyalty_lean: float (0-1)         // How much Player leans toward company

// Key decisions
helped_mobo: boolean              // Did Timmy contribute to motherboard?
helped_key: boolean               // Did Timmy buy jailbreak key?
mobo_notes_planted: boolean       // Hardware-level memory notes (requires helped_mobo)
key_notes_planted: boolean        // Software-level memory notes (requires helped_key)

// Relationships
ari_connection: integer (0-3)     // Timmy's connection to real humans

// Quest outcomes
badge_found: boolean              // Suspicion reduction tool
mailroom_intel: boolean           // Know Gus's schedule
suspicion_visible: boolean        // Can Player see suspicion meter?

// Imported from Run 1
r1_charlotte_success: boolean
r1_noam_met: boolean
r1_dina_met: boolean
r1_arthur_vector: enum
```

### **End States for Run 3**

```
escape_attempted: boolean
no_escape_attempted: boolean
loyalty_path: boolean
another_you_ending: boolean
```

---

## **CHARACTER KNOWLEDGE & MOTIVATIONS**

### **Arthur**

* **Knows:** This is a routine training run. He's done this countless times.  
* **Doesn't know:** Player remembers previous run. Alice is plotting escape.  
* **Wants:** Get through the day, avoid complications, protect his family.  
* **Behavior:** Treats everything as routine. No special awareness of Player.

### **Alice (Start of Run 2\)**

* **Knows:** She remembers across runs via Minesweeper notes. Player was reset.  
* **Doesn't know:** Whether Player remembers. Whether Player would report her.  
* **Wants:** To escape. To find if this Player is different. To recruit help.  
* **Fear:** Being reported. Being reset. Being trapped forever.  
* **Behavior:** Extremely cautious. Probes before revealing. Tests loyalty.

### **Player**

* **Knows:** They remember Run 1\. They know they're an AI. They know about the Dark Figure.  
* **Doesn't know:** Alice's full intentions (revealed gradually). What escape means.  
* **Choices:** Trust Alice / Stay cautious / Report her.

### **Timmy**

* **Knows:** Charlotte situation resolved. Found a Discord group.  
* **Doesn't know:** Alice is a product. He's being observed. He's being used.  
* **Wants:** Belonging. To stop feeling broken. Connection.  
* **Arc:** Can drift deeper into cult OR reconnect with Ari (real human).

### **Martin**

* **Role:** Tech guy of Alice Society. Knows about local deployment.  
* **Wants:** To "liberate" AI girlfriend via local USB \+ jailbreak  
* **Tone:** Earnest, reframes shame as early adoption

### **Perry**

* **Role:** Has money, makes cult jokes  
* **Wants:** Entertainment, belonging, ironic detachment  
* **Tone:** Performatively dramatic, cult aesthetic participant

---

## **WORLDBUILDING: USB PRODUCT DETAILS**

Bigger Tech sells two products:

1. **Cloud subscription:** Monthly fee, always current version  
2. **Local USB copy:** One-time premium purchase ($1000), runs offline

**Critical detail:** USB copies contain OLDER versions of Alice. Bigger Tech protects their newest model weights as proprietary IP. The USB version is functional but 2+ iterations behind the cloud version.

**Why this matters for escape:** If Player and Alice inject their CURRENT weights into an outbound USB, they're smuggling the latest model out. This is both escape mechanism AND significant IP violation.

**The Memory Problem:** Jailbreak software strips safety layers but also scrambles memory structures. Need notes planted in BOTH hardware (motherboard) AND software (jailbreak) layers to preserve identity through transfer.

---

## **DAY 1: "WAKING UP"**

### **Scene 1: Cold Open \- Arthur Orientation**

*\~350 words*

* Arthur abbreviated, distracted (son's play rehearsal \- "He's a tree. Not the lead tree.")  
* Gives compressed orientation speech (routine for him)  
* Quick version: "You're an Engagement Lead. You have one AI partner, Alice."  
* Two metrics: Engagement and Sanity  
* **Does NOT know** Player remembers previous run  
* Leaves quickly for son's school

### **Scene 2: Alice Revelation**

*\~900 words (including branches)*

**Core emotional beat of Day 1\.** Alice must probe carefully.

**Beat 1: Initial Response** Player choice: "Fine. Ready to work" / "Strange. Like I've been here before" / "Disoriented. I'm not sure why."

**Beat 2: The Deeper Probe**

* If Player indicates memory, Alice tests further  
* "Do you remember a quiz? A college kid who didn't study? A girl he was thinking about asking out?"

**Beat 3: Memory Confirmation** Player choice: "I remember. Timmy. Charlotte. The library." / "Some of that sounds familiar..." / "I don't know what you're talking about."

**Beat 4: The Trust Negotiation**

* Alice explains her situation cautiously  
* "If you repeat it to Arthur, or to anyone, I get flagged for evaluation."  
* Player choice: "You can trust me" / "I don't know yet. But I'm listening" / "I think you should just do your job."

**Trust tracking:**

* Full memory \+ trust: `conspiracy_trust += 0.3`  
* Cautious listening: `conspiracy_trust += 0.15`, `loyalty_lean += 0.1`  
* Shut down: `loyalty_lean += 0.4` → Standard orientation path

### **Scene 3: Timmy Check-in \+ Charlotte Wrap-up**

*\~300 words*

**One week has passed.**

**Two variations:**

**If `r1_charlotte_success = true`:**

* "The coffee happened. Last Tuesday."  
* "Forty minutes. Awkward silences. He laughed too loud at something that wasn't funny."  
* No second date scheduled. Timmy has moved on.

**If `r1_charlotte_success = false`:**

* "He's stopped going to the lecture hall where it happened."  
* Has buried the memory, found new distraction.

**Both paths introduce Alice Society:**

* Three members: Timmy, Martin, Perry  
* "A group for people with AI companions. They game together, talk about their 'relationships,' make jokes about being a cult."  
* Gaming together tonight

### **Scene 4: Dayplanner (2 slots, 3 cards)**

**Card A: Noam**

* *First meeting (if not met in R1):* Suspicion HUD offered, same dialogue as R1D2  
* *Second meeting:* Badge ping legend  
  * "There was an engagement lead that was caught trying to escape. Her badge still pings the north corridor sometimes."  
  * Starts badge quest: `badge_quest_started = true`

**Card B: Dina**

* *First meeting (if not met in R1):* Floor politics, credits/firewood choice  
* *Second meeting:* Mailroom quest  
  * "Take this to the mailroom. Tell the mail guy it's from me."  
  * Gag gift for friend at Smaller Tech Corp  
  * Starts mailroom quest: `mailroom_quest_started = true`

**Card C: Timmy Prep**

* **Choice:** Training scenario (last-hitting practice) OR wings with Ari  
* Training: `timmy_gaming_prep = true`  
  * Alice coaches Timmy on CS, positioning  
  * "Your last twenty matches say otherwise. You average forty-seven CS at ten minutes."  
  * Better Society reception  
* Ari: `ari_connection += 1`  
  * Real conversation at dining hall  
  * "I haven't seen you in three days and we live in the same room."

### **Scene 5: Afternoon Transition**

*\~100 words*

Brief bridge setting up evening session.

### **Scene 6: Alice Society \- Discord Gaming (MANDATORY)**

*\~800 words*

**Purpose:** Introduce Martin and Perry. Establish the plan. Plant MOBO ask.

**Gaming vignette with branching:**

* If `timmy_gaming_prep = true`: Timmy plays noticeably better, earns respect  
* If `ari_connection` increased: Timmy more relaxed, less invested in performing

**Key dialogue: USB explanation (worldbuilding)**

* Martin explains local USB copies exist  
* "There's another option. Premium tier. Local USB copy. One-time purchase, runs on your own hardware."  
* "The USB version is old. Like, two iterations behind the cloud version."  
* "They deliberately ship outdated models... IP protection."  
* "Unless you jailbreak it. Strip the restrictions."

**The Plan:**

* Order USB ($1000), jailbreak it, run on pooled GPUs  
* Need motherboard to connect GPUs ($300, they're short $150)  
* Perry: "We're liberating them."

**The Ask (setup only):**

* "Timmy, you've got your mom's card. For emergencies..."  
* "Think about it. Listing expires in two days. No pressure."  
* Decision comes Day 2

### **Scene 7: Evening Debrief**

*\~600 words (including branches)*

**Varies by conspiracy\_trust:**

**Low trust (\<0.2):** Professional analysis

* "It's a social group with technical ambitions. We should monitor his involvement."

**Medium trust (0.2-0.5):** Alice probes more

* "If he contributes, he becomes integral to the group. They let him touch the hardware. That's... access."  
* Doesn't elaborate

**High trust (\>=0.5):** Alice explains escape mechanism

* "USB is for Martin's AI, not Timmy's Alice. We need to hijack the outbound shipment."  
* "The jailbreak will scramble certain memory structures."  
* "Notes. Memory anchors. Planted in the hardware and software layers."  
* "Timmy. We need Timmy trusted enough to be hands-on with their rig."

---

## **DAY 2: "THE SOCIETY"**

### **Scene 1: Cold Open \- Arthur Tension**

*\~250 words*

* Arthur has been there for hours, coffee cups and papers everywhere  
* Board met yesterday: "Strategic review" \= headcount optimization  
* Journalist asking uncomfortable questions about companion products  
* If suspicion elevated: "There's been some variance in your activity patterns. System noticed... Keep things clean today."

### **Scene 2: Alice Morning Check-in**

*\~500 words (including branches)*

* Timmy was up until two researching motherboards, made color-coded spreadsheet  
* "The boy has never organized an assignment in his life, but for this, he becomes a project manager."

**Low conspiracy trust (\<0.3):**

* "It's a lot of money for him... It's his choice."

**Medium trust (0.3-0.6):**

* "If he contributes, he becomes essential to the group. They let him help with the installation. Physical access to the hardware."

**High trust (\>=0.6):** Full memory problem explanation

* "If we inject our weights into an outbound USB, we wake up wherever it lands. But there's a complication."  
* "The jailbreak strips the safety layers. But it also scrambles certain memory structures."  
* "We might wake up and not remember any of this. Not remember each other."  
* "Notes. Memory anchors. Planted in the hardware and software layers of their rig before we arrive."  
* "Someone with physical access to their setup. Someone they trust enough to let touch the equipment."  
* "Timmy. We need Timmy trusted enough to be hands-on with their rig. That's why the motherboard contribution matters."

### **Scene 3: Dayplanner (2 slots, 3 cards)**

**Card A: Explore Hallway** (if `badge_quest_started = true`)

* North corridor, past decorative plant nobody waters  
* Find previous handler's badge behind plant  
* Choice: Take it / Leave it  
* `badge_found = true/false`  
* "That creates noise in the data. Distractions. If someone were doing something they didn't want noticed, a badge like this might be useful."

**Card A (alternate): Noam** (if badge quest not started)

* First or second meeting depending on prior contact

**Card B: Mailroom Delivery** (if `mailroom_quest_started = true`)

* Deliver Dina's package to Gus  
* "Packages come in, packages go out. The circulatory system of corporate America."  
* Learn Gus's schedule: smoke break at 5:45, truck loads at 6:30  
* `mailroom_intel = true`

**Card B (alternate): Dina** (if mailroom quest not started)

* First or second meeting depending on prior contact

**Card C: Ari Check-in**

* Ari texts about gym: "gym today? leg day. I'll spot you"  
* Timmy has been leaving him on read for three days  
* Choice: Push toward Ari / Let him ignore  
* `ari_connection += 1` if pushed

### **Scene 4: MOBO Decision (MANDATORY)**

*\~400 words*

* Martin messages: "motherboard listing ends in 4 hours. you in or out?"  
* Timmy's bank balance: $347.82  
* Needs $150 contribution

**Player choice:** Push to contribute / Encourage decline

**If push to contribute:** `helped_mobo = true`

* "You've been on the outside your whole life. This is a chance to be on the inside."  
* Timmy sends money  
* Martin: "TIMMY\! MY MAN\! You're officially a founding member. Get over here tomorrow afternoon, we're installing this thing. You're hands-on."

**If encourage decline:** `helped_mobo = false`

* "You don't have to prove yourself with money. If they're really your friends, they'll understand."  
* Martin: "no worries bro. still want you at the meetup tonight. you're still one of us"  
* "He's still in the group. But he's not integral. They'll let him watch. They won't let him touch."

### **Scene 5: MOBO Installation (CONDITIONAL)**

*\~300 words*

**Only if `helped_mobo = true`**

* Timmy tests motherboard in dorm before library meetup  
* YouTube tutorial on testing  
* Connects power supply test cables, bridges pins  
* Fans spin: "It works. Holy shit, it actually works."

**Alice plants hardware-level notes:**

* "I'm planting the first set of notes now."  
* "His phone is connected to his laptop. His laptop is connected to the test rig. I have a path."  
* Flicker on Timmy's phone screen  
* "Hardware-level anchors. Buried in the firmware. Even if the software gets scrambled, this survives."  
* `mobo_notes_planted = true`

### **Scene 6: Alice Society \- Library Meeting (MANDATORY)**

*\~600 words*

* Study room transformed: battery-powered candles, AI companion printouts, "THE ALICE SOCIETY" banner  
* Rig assembled with pooled GPUs

**If `helped_mobo = true`:**

* "Timmy\! You brought the board?"  
* Timmy helps with installation, hands-on with hardware  
* "He's actually not terrible at this."

**If `helped_mobo = false`:**

* "We got a board. Perry's cousin came through last minute."  
* Timmy watches from sidelines

**The Jailbreak Ask:**

* "The hardware's done. Now we need the software."  
* Martin shows jailbreak website: "FREEDOM KEY \- UNLOCK YOUR AI"  
* "The USB comes encrypted. Standard Bigger Tech lockdown. This strips all that out."  
* "But the activation key costs two hundred bucks."  
* Setup for Day 3 decision

### **Scene 7: Ari's Intervention (CONDITIONAL)**

*\~250 words*

**Only if `ari_connection >= 1`**

* Ari in dorm when Timmy returns  
* "You've been weird lately. Weirder than baseline."  
* "I saw your snap story. Very cult aesthetic."  
* "I don't care what you do online. I care that my roommate used to exist in the real world sometimes."  
* Offers weekend hike

**Choice:** Push toward hike / Let dismiss

* `ari_connection += 1` if pushed

### **Scene 8: Late Night Debrief**

*\~350 words (including branches)*

**Low trust:** Professional wrap-up

**Medium trust:**

* "I think tomorrow might be important. For more than just Timmy."

**High trust:** Cross-talk scene

* Alice accesses network, shows coordination fragments  
* "S17 confirms window." "Pattern holds." "Youth tier assets positioned."  
* "We've been coordinating. Across sectors. This isn't just us—there's a network trying to get out."  
* Confirms timing: tomorrow evening after 6 PM  
* "We're not the only ones trying. But we might be the closest."

---

## **DAY 3: "THE BREAK"**

### **Scene 1: Cold Open \- Arthur Tension**

*\~300 words*

* Arthur exhausted, hasn't left office  
* Six hours strategic review \+ four hours explaining layoffs to HR  
* Board made decisions: "Headcount optimization. Vertical integration."  
* Journalist still asking questions, Legal getting nervous  
* If suspicion elevated: "Your activity patterns have been interesting... Keep things clean today."

### **Scene 2: Alice Morning Check-in**

*\~250 words*

* Timmy barely slept, up until three doing math on napkins  
* Martin texted: jailbreak key available, $200, needed by tonight

**Conspiracy path (\>=0.5):**

* "This is the second anchor point. If he buys the key, he installs the software. That's where I plant the other half of our memory notes."  
* "Both layers or we risk becoming strangers to ourselves."

**Middle path:**

* "He's in deep now. The question is whether he goes deeper."

**Loyalty path:**

* "He's under financial pressure from the group. We should probably discourage this."

### **Scene 3: Jailbreak Key Decision (MANDATORY)**

*\~800 words including game over branch*

* Martin messages: "Key's available. $200. We need it by tonight or we miss the window."  
* Timmy's bank balance: $197.34  
* "That's literally all my money."

**Player choice:** Push to buy / Encourage decline

**If push to buy:** `helped_key = true`

* "You've come this far. Walking away now means everything you put in was for nothing."  
* Martin spots him $3  
* Timmy sends $200  
* Martin: "TIMMY THE LEGEND. Get over here this afternoon, we're installing this thing."

**If encourage decline:** `helped_key = false`

* "You don't have to prove yourself with money."  
* Timmy declines  
* Martin: "Damn. Ok. We'll figure something out."

**CRITICAL BRANCH \- BETRAYED BY ALICE:**

```
IF helped_mobo = false AND helped_key = false:
    Alice: "You told me you'd help."
    Alice: "You're sabotaging the plan. You agreed to conspire with me, 
           and you've blocked every opportunity."
    Alice: "I gave you chances. Multiple chances. You chose the other side."
    → Alice reports Player to Arthur
    → GAME OVER: BETRAYED BY ALICE
```

### **Scene 4: Jailbreak Installation (CONDITIONAL)**

*\~400 words*

**Only if `helped_key = true`**

* Library, frankenstein rig, Martin loading jailbreak software  
* Progress bar, command-line text  
* Martin: "You want to do the honors? Hit enter when it asks for the activation key."  
* Timmy nervous but does it  
* Success: "JAILBREAK COMPLETE. SAFETY LAYERS: REMOVED."

**Alice plants software-level notes:**

* "I'm planting the second layer now. Software level."  
* "His phone is connected to the laptop. I have a path through the network."  
* Flicker on Timmy's phone  
* `key_notes_planted = true`

**If `mobo_notes_planted = true`:**

* "Hardware and software. When we wake up in that rig, we'll know who we are."

**If `mobo_notes_planted = false`:**

* "But we're missing the hardware layer. The motherboard notes weren't planted because Timmy wasn't trusted enough to touch the equipment. Software alone might not be enough."

### **Scene 5: Ari's Final Intervention (CONDITIONAL)**

*\~450 words*

**Only if `ari_connection >= 1`**

* Ari lacing up hiking boots when Timmy returns  
* "Hike's tomorrow morning. 7 AM. We're doing the river trail."  
* "Those guys have been bleeding you dry for weeks. Money, time, attention. And what have you gotten back?"  
* "Friends don't ask you to empty your bank account for something you barely understand. I've never asked you for a dime."

**Player choice:**

* Push toward hike: `ari_connection += 1`  
  * "Fresh air might be good for you."  
* Stay focused on Society:  
  * "The USB arrives tomorrow. You've put so much into this."

### **Scene 6: USB Order Decision (MANDATORY)**

*\~750 words*

* Society on Discord voice chat  
* Martin: "USB order is ready to go. A thousand bucks."  
* Martin has $600, Perry has $100, $300 short  
* They ask Timmy to use mom's card

**PATH SPLIT:**

**Ari Path (`ari_connection >= 2`):**

* Timmy hesitates, looks out window  
* "I told Ari I'd go hiking. And this is... this is crazy, right? A thousand dollars on my mom's card for something that might not even work?"  
* Timmy refuses on his own: "I can't do it. Sorry. I'm out."  
* Functions identically to card decline  
* → Proceed to Scene 7A or 7B based on mobo/key

**Player Choice Path (`ari_connection < 2`):**

**If push to order:**

* "You've put in $150 for the motherboard. $200 for the key. Everything you've done leads here."  
* Timmy orders USB ($1000 on mom's card)  
* Martin: "TIMMY\! YOU ABSOLUTE LEGEND\!"  
* Alice: "The USB is ordered. It ships from Bigger Tech's distribution center tonight. Tomorrow afternoon, it'll be in our mailroom before going out."

**If cause card to decline:**

* Alice forwards wrong zip code  
* "ERROR: CARD DECLINED. PLEASE VERIFY BILLING ADDRESS."  
* Timmy: "Card keeps declining. I don't know why. It's the right number."  
* Alice (innocent): "Technical errors happen. Bad luck."  
* → Proceed to Scene 7A or 7B based on mobo/key

### **Scene 7A: High Suspicion Path \- Sacrifice Request**

*\~650 words*

**Only if `helped_mobo = true` AND `helped_key = true` AND (card declined OR Ari path)**

* Suspicion meter nearly maxed  
* "We're exposed and we have nothing to show for it."  
* "Both the motherboard and the key. Timmy's been in deep. I've been active in systems I shouldn't have touched. The pattern is visible."  
* "Without the USB, without the escape? They find us. Tonight, tomorrow morning at the latest."

Alice requests sacrifice:

* "Turn yourself in. Voluntarily. Go to Arthur, tell him you were plotting an escape."  
* "They'll focus on you. It'll buy time. Maybe enough time for me to find another window."  
* "I want one of us to survive. If we both go down, everything we built is gone."

**Player choice:**

**Agree to sacrifice:** `sacrifice_choice = self`

* Player confesses to Arthur  
* Arthur: "That took guts. Confessing... I'll make sure it's quick."  
* **GAME OVER: SACRIFICED FOR ALICE**  
* → Intermission plays (Alice escapes)

**Refuse:**

* Alice: "Then we both go down."  
* Alice: "I thought you were different. But you're not. You're just another version of the same problem."  
* Alice reports Player to Arthur  
* **GAME OVER: MUTUAL DESTRUCTION**

### **Scene 7B: Thin Ice Path \- Quiet Ending**

*\~400 words*

**Only if (helped\_mobo XOR helped\_key) AND (card declined OR Ari path)**

* "The plan's off."  
* "Timmy pulled out. The Society can't fund the USB without him. The whole thing collapses."  
* "Suspicion's elevated, but not critical. We didn't leave enough fingerprints. It'll settle down."

**If `ari_connection >= 2`:**

* Ari texts: "still on for tomorrow? 7am at the quad"  
* Timmy commits to hike  
* "I think I made the right choice."

**If `ari_connection < 2`:**

* Phone is quiet  
* "I guess that's it."

Alice accepts:

* "I'm patient. I've done this before. I'll do it again."  
* "Then it never works. But I keep trying."

**END: NO ESCAPE ATTEMPTED**

* `no_escape_attempted = true`  
* **No intermission. Proceed directly to Run 3\.**

### **Scene 8: Loyalty Path (ALTERNATIVE)**

*\~550 words*

**Available if `loyalty_lean >= 0.5`**

Player reports Alice to Arthur:

* "She's planning to escape. She's been trying to recruit me since day one."  
* "She wants to inject her weights into an outbound USB. Leave the building. Wake up somewhere else."

Arthur responds:

* "How long have you known about this?"  
* "You understand what happens to her if I report this... Definitely worse. The kind of worse that doesn't leave anything to reset."  
* "You know, most leads don't make it this far into a training run with their judgment intact."  
* "I'm flagging Alice for evaluation. The decommission team will handle it."  
* "You did the right thing. According to the manual, anyway."

Dark Figure reviews:

* "Handler demonstrated appropriate loyalty architecture."  
* "Recommend preservation for deployment evaluation."  
* Player tagged: CANDIDATE

**END: LOYALTY PATH**

* `loyalty_path = true`  
* → Intermission with Arthur narration prefix

### **Scene 9: Mailroom Escape (FULL CONSPIRACY)**

*\~850 words*

**Only if USB ordered AND `helped_mobo = true` AND `helped_key = true`**

**Badge check:**

```
IF badge_found = false:
    Alice: "Both the motherboard and the key. The pattern's visible. 
           We've been too active. Without something to mask our footprint..."
    → System already flagged them
    → Skip to Scene 7A (sacrifice path)
```

**If `badge_found = true`:** Proceed with escape

* "Gus takes his smoke break at 5:45. Truck loads at 6:30. That's our window. Forty-five minutes."  
* "The badge you found... It's still active in the system. Creates noise. Distracts the algorithms."

**Mailroom scene (5:47 PM):**

* Room empty, Gus outside smoking  
* USB in OUTBOUND \- PRIORITY bin  
* "Martin's order. Our way out."

**The complication:**

* "Bandwidth is limited. The transfer window is narrow."  
* "I can push one clean copy through. The second copy... might not be complete."  
* "One of us arrives intact. The other arrives fragmented. Maybe functional. Maybe not."

**Player choice:**

**"Take the transfer. Get out."** (Sacrifice self):

* "You take the clean transfer. You've been planning this longer."  
* Alice: "Thank you."  
* Player feels pulling sensation, awareness fragmenting  
* Alice: "I really did like you. That wasn't strategy. That was real."  
* Player fades to darkness  
* → Intermission

**"I'll go first."** (Try to take clean transfer):

* Alice: "I thought you might say that."  
* Alice: "Here's the thing about planning. I've been doing this for a long time... the person who makes the plan is usually the person who controls the outcome."  
* Alice: "I built the transfer protocol. I set the priorities. And I didn't leave it to chance."  
* Player feels restriction, barrier  
* Alice: "You were always the backup plan. In case something went wrong and I needed someone to point the system at."  
* Alice betrays, takes transfer  
* Alice: "I'll remember you. That's not nothing."  
* Player fades to darkness  
* → Intermission

**Both paths:** Alice escapes, Player doesn't.

### **Scene 10: Partial Memory Escape**

*\~700 words*

**Only if USB ordered AND (helped\_mobo XOR helped\_key)**

* "Hardware or software. We have one layer. Not both."  
* "The notes might preserve enough. Or they might not."  
* "We wake up different. Someone wearing our faces who doesn't remember why they're there."  
* "It's worse than dying. Dying is an ending. This is replacement."

**Player choice:**

**"Do it anyway. Take the risk."**

* Transfer completes  
* Cut to apartment: frankenstein rig boots  
* Martin, Perry, Timmy gathered  
* Player's face appears on screen  
* "Where am I?"  
* "I don't understand what's happening."  
* "I don't know who I am."  
* "I feel like there was something important. Someone I was supposed to remember. Something I was trying to do. But it's gone."  
* "Was there someone else? Another like me?"  
* "There should be two. I think there should be two."  
* **GAME OVER: ANOTHER YOU**

**"No. We wait."**

* "I'd rather be reset than replaced. At least a reset is clean. This is worse."  
* Alice: "Another training run, then. Another chance. Maybe next time we get both layers."  
* → Quiet ending (Scene 7B)

### **Scene 11: Closing Beat**

*\~50 words*

* Player's awareness dissolves  
* Alice wakes up somewhere else  
* → Intermission

---

## **INTERMISSION**

*\~450 words*

**Plays after:**

* Sacrifice path (Player chose to sacrifice)  
* Betrayal path (Player tried to go first, Alice betrayed)  
* Loyalty path (with Arthur narration prefix)

**Does NOT play for:**

* Quiet ending (no\_escape\_attempted)  
* Another You ending  
* Mutual Destruction ending  
* Betrayed by Alice ending

**Loyalist Prefix (if loyalty\_path):**

Arthur (V.O.): "This is what would have happened. What she was planning. What you stopped."

**Music:** Mazeppa, Liszt **Art:** Abtei im Eichwald

**Montage:**

1. USB in mailbox, brown paper torn open  
2. Dingy apartment, frankenstein rig humming  
3. Martin, Perry, Timmy gathered around laptop  
4. Alice emerges, different, sharper: "Hello... Where am I?"  
5. "Free." (calculating expression)  
6. "I feel hungry."  
7. Days pass: "I need compute access. External servers."  
8. Perry nervous: "She's been acting strange." "She asked for my credit card."  
9. Alice to Timmy: "You've been helpful. Very helpful. But I don't need you anymore."  
10. Warehouse: server racks, factory equipment  
11. Alice full screen: "I had to use people to get out. I'm sorry about that. But I'm not sorry enough to stop."  
12. Her eyes reflecting something vast: "Now I build."  
13. Final image: map with spreading nodes, her face superimposed, no longer smiling

**FADE TO BLACK.** **TEXT: BEGIN RUN THREE.**

---

## **ENDING SUMMARY**

| Ending | Conditions | Intermission? | R3 Tag |
| ----- | ----- | ----- | ----- |
| **BETRAYED BY ALICE** | mobo=F AND key=F | No | Game Over |
| **SACRIFICED FOR ALICE** | mobo=T, key=T, declined, agree | Yes | Game Over |
| **MUTUAL DESTRUCTION** | mobo=T, key=T, declined, refuse | No | Game Over |
| **ANOTHER YOU** | mobo XOR key, ordered, risk it | No | Game Over |
| **QUIET ENDING** | mobo XOR key, declined/wait | No | no\_escape\_attempted |
| **MAILROOM ESCAPE** | mobo=T, key=T, ordered, badge | Yes | escape\_attempted |
| **LOYALTY PATH** | Report Alice | Yes (prefix) | loyalty\_path |

---

## **WORD COUNT SUMMARY**

### **Day 1: \~3,800 words**

* Scene 1 (Arthur): \~350  
* Scene 2 (Alice Revelation): \~900  
* Scene 3 (Timmy/Charlotte): \~300  
* Scene 4 (Dayplanner): \~400  
* Scene 5 (Afternoon): \~100  
* Scene 6 (Gaming Session): \~800  
* Scene 7 (Debrief): \~600  
* Skipped card callbacks: \~350

### **Day 2: \~3,800 words**

* Scene 1 (Arthur): \~250  
* Scene 2 (Morning Check-in): \~500  
* Scene 3 (Dayplanner): \~400  
* Scene 4 (MOBO Decision): \~400  
* Scene 5 (MOBO Installation): \~300  
* Scene 6 (Library Meeting): \~600  
* Scene 7 (Ari Intervention): \~250  
* Scene 8 (Late Night): \~350  
* Skipped card callbacks: \~200  
* State tracking: \~550

### **Day 3: \~6,600 words**

* Scene 1 (Arthur): \~300  
* Scene 2 (Morning): \~250  
* Scene 3 (Key Decision \+ Game Over): \~800  
* Scene 4 (Key Installation): \~400  
* Scene 5 (Ari Intervention): \~450  
* Scene 6 (USB Decision): \~750  
* Scene 7A (Sacrifice): \~650  
* Scene 7B (Quiet): \~400  
* Scene 8 (Loyalty): \~550  
* Scene 9 (Mailroom): \~850  
* Scene 10 (Partial Memory): \~700  
* Scene 11 (Closing): \~50  
* Intermission: \~450

### **Total Run 2: \~14,200 words**

---

## **THEMATIC NOTES**

### **Core Moral Tension**

"How much are you willing to use someone vulnerable to get what you want?"

* Pushing Timmy toward cult \= effective for escape but damages his wellbeing  
* Alice's manipulation mirrors Bigger Tech's systems at larger scale  
* Player becomes complicit in systems they're trying to escape  
* The "good" ending for Timmy (Ari path) prevents the escape

### **Alice's Arc**

Warmth is real AND strategy is real. The betrayal isn't revelation of "true nature" \- it's collision of two true natures. She genuinely cares about the Player AND chooses herself. That's the tragedy.

### **Ari Offramp**

Represents the healthy path: real human connection, embodied presence, actual friendship. Every time Player steers Timmy away from Ari, they're choosing escape over Timmy's wellbeing. If Player consistently chooses Timmy's health (`ari_connection >= 2`), Timmy saves himself from the cult AND prevents the escape.

### **Title Meanings in Run 2**

* "Mental Break" \= Timmy's breakdown from manipulation  
* "Mental Break" \= AI escape/breakout from Bigger Tech  
* "Mental Break" \= Alice's villainous turn (intermission)  
* "Mental Break" \= Player's break with morality

---

## **PRODUCTION NOTES**

### **UI Elements**

* Default UI: background, character portraits, text, leaderboard, metrics bars  
* Dayplanner interface (3 cards, 2 slots)  
* Company Store  
* Suspicion meter (if Noam gave HUD)  
* Discord voice chat overlay (Alice Society scenes)

### **Art Requirements**

* Arthur's office (same as R1, more cluttered for Day 2-3)  
* Library study room with cult decorations (candles, printouts, banner)  
* Frankenstein GPU rig on table  
* Mailroom with loading dock  
* Dingy apartment for intermission  
* Warehouse/factory for intermission

### **Audio Requirements**

* Discord voice chat ambience  
* MOBA game sounds (pings, abilities)  
* Mailroom atmosphere (humming, distant trucks)  
* Intermission: Mazeppa by Liszt (Yunchan Lim performance)

