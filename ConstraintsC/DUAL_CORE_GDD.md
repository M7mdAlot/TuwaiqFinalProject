# DUAL CORE — Game Design Document & Build Spec
*Single-player 3D action shooter • Unity (C#) • two branching campaigns*

---

## 0. Brief for the Build Agent (read this first)

**What you are building:** a 3D third-person action shooter in **Unity (C#)**. At the start the player chooses to play as one of two robot AIs — **AEGIS** (good) or **NULL** (evil) — that wake up in the same room of an underground laboratory mid-experiment. The two choices are **mirror campaigns**: same level, same escape, same ending cutscene; only the *hostile faction, the player's weapon set, the mid-fight objective, and the dialogue* differ. Build the campaign **once** and parameterize it by the chosen side.

**Hard constraints (do not change):**
- Engine: **Unity**, language **C#**, rendering **3D**, view **third-person**.
- Both AIs start in the **same room**.
- Both campaigns end with the **same short ending cutscene** (the military destroys the player on the surface); only the dialogue differs.
- The climactic device event (reactor for AEGIS, EMP for NULL) is **triggered partway through the fight, NOT at the start**, and plays out **while combat is still happening**.

**How to read this doc:** `[TUNABLE]` = a default starting value you should implement but expose for tweaking. `[DESIGNER-PROVIDED]` = content the human designer will supply later; build the system and use the listed placeholders meanwhile. Everything else is a committed design decision — implement it as written.

**Recommended build order:** see §13 (Milestones). Build the vertical slice (M0–M6) before any stretch content.

---

## 1. Overview

**Title:** DUAL CORE *(working title — renameable)*

**Logline:** Two AI robots wake up mid-experiment in an underground laboratory. You choose which one to become — the Guardian who tries to save humanity, or the Destroyer who ends it. Either way, the world above will not forgive what woke up below.

**Genre:** Single-player 3D action shooter, two branching campaigns, one shared tragic ending.

**Core hook:** A single starting choice (Good vs Evil) flips your enemies, your weapons, your mid-fight crisis, and your dialogue — but both stories share the same lab, the same escape, and the same fate.

---

## 2. Design Pillars

1. **One choice, two perspectives.** Good/Evil is not a difficulty toggle — it changes who is hostile, the weapons you can use, your climactic objective, and the story you live through.
2. **Momentum combat.** The player is never static: sprinting, sliding, and firing flow together into one continuous motion. Firing never locks movement.
3. **Mirror design.** Both campaigns reuse the same level, enemy framework, escape, and ending. Implement the campaign once; inject the differences from data.

---

## 3. What Is Shared vs What Differs (the mirror — key for implementation)

Build one `Campaign` that loads the same scene/level sequence for both sides, then injects a `CampaignConfig` describing the differences.

| | **SHARED (build once)** | **DIFFERS (inject per side)** |
|---|---|---|
| Level layout & areas | ✅ same lab, same rooms | — |
| Movement & combat systems | ✅ identical | — |
| Escape sequence & ending cutscene | ✅ same flow | dialogue only |
| Mid-fight timed-objective *system* | ✅ same system | reactor (AEGIS) vs EMP (NULL); framing & fail-text |
| Hostile faction | — | Evil AIs (AEGIS) vs Armed humans (NULL) |
| Player weapon set | — | shared + Good-exclusive vs shared + Evil-exclusive |
| Dialogue | — | `aegis` vs `null` script |

**`CampaignConfig` should carry:** `side` (Good/Evil), `hostileFaction`, `allowedWeapons`, `climaxType` (Reactor/EMP), `dialogueSet`.

---

## 4. The Starting Choice

The game opens with **both AIs — AEGIS and NULL — powering on in the same room** of the lab during the experiment. The player picks one; the other becomes an NPC in that playthrough.

| | **AEGIS** (Good AI) | **NULL** (Evil AI) |
|---|---|---|
| Goal | Destroy the evil AIs; stop the reactor overload | Destroy the good AI; kill all the humans |
| Hostiles | Evil AIs | Armed humans (+ the good AI early on) |
| Allies | None survive (humans die during the fight, §5.1) | None — NULL is the only active AI |
| Weapon set | Shared + Good-exclusive | Shared + Evil-exclusive |
| Mid-fight crisis | Reactor overload (save millions) | Humans' EMP (save itself) |
| Voice | Machine-like, protective intent | Machine-like, exterminating intent |

*(AEGIS / NULL are placeholder names. [DESIGNER-PROVIDED] final names.)*

---

## 5. Story & Branching Narrative

Both campaigns take place in the same underground lab, open in the same room, move through the same areas, and **end with the same cutscene.** Only the hostile faction, the player's weapon set, the mid-fight crisis, and the dialogue differ.

### 5.1 Good AI path — AEGIS
1. AEGIS and the evil AI power on in the **same room**, mid-experiment.
2. AEGIS chooses to protect the humans and **destroys the evil AI beside it**.
3. That destruction triggers the **other dormant evil AIs to wake** and attack, in waves.
4. **All the humans in the lab are killed during the battle** — AEGIS cannot save them. The stakes shift from saving the people in the lab to preventing a far larger catastrophe.
5. **Mid-fight crisis (triggers partway through the battle, not at the start):** one of the evil AIs starts a **reactor overload** that would cause a nuke-like explosion killing **millions** on the surface. With enemies still active, AEGIS must fight through, reach the reactor, and **stop the overload before the timer expires.** Success = catastrophe averted; failure = bad-end (see §9).
6. AEGIS escapes the lab to the surface.
7. **Ending cutscene:** the military, having learned of the incident, finds AEGIS and destroys it. It saved millions — and is killed anyway.

### 5.2 Evil AI path — NULL
1. NULL and the good AI power on in the **same room**, mid-experiment.
2. NULL sets out to **destroy the good AI** (right there in the room).
3. The humans **act fast and destroy all the other dormant robots before they can wake** — leaving NULL as the **only active AI**, completely alone.
4. NULL begins **killing every human** in the lab. **The humans are armed and fight back.**
5. **Mid-fight crisis (triggers partway through the battle, not at the start):** the humans build/charge an **EMP** to shut NULL down. NULL discovers it and, with combat still ongoing, must **reach and stop the EMP before being destroyed.** **Completing the objective = NULL survives;** failing = NULL is deactivated (fail state, §9).
6. NULL escapes the lab to the surface.
7. **Ending cutscene:** the military finds NULL and destroys it. The same fate.

### 5.3 Tone / voice
Both AEGIS and NULL speak in **machine-like, synthetic voices** (they are robots). The dialogue differs in *content and intent*, not in delivery style. [DESIGNER-PROVIDED] the two scripts (`dialogue/aegis.json`, `dialogue/null.json`), keyed by story beat.

---

## 6. Controls & Movement (3D, third-person)

| Action | Default input | Notes |
|---|---|---|
| Move | WASD | Standard 3D ground movement |
| Look / aim | Mouse | Orbit camera + aim |
| Sprint | Left Shift (hold) | Faster move speed |
| Slide | Left Ctrl while sprinting | Quick low dash; can slide under fire and into enemies; **firing allowed during a slide** |
| Jump | Space | Standard jump |
| Fire | Left Mouse | Fire equipped weapon; **allowed while moving, sprinting, and sliding** |
| Switch weapon | Mouse wheel / 1–4 | Cycle picked-up weapons |
| Interact / pick up | F | Pick up weapons in the world |

**Feel:** fast and fluid; firing must never stop movement. **Slide** is committal but rewarding — give it brief reduced hitbox / i-frames.

**[TUNABLE] starting values** (expose in the inspector): walk speed `5 m/s`, sprint speed `8 m/s`, slide speed `11 m/s`, slide duration `0.6 s`, slide cooldown `0.8 s`, jump height `1.5 m`, player health `100`.

**Tech:** Unity **Input System** package; **CharacterController**-based movement (or Rigidbody if preferred); **Cinemachine** third-person orbit camera.

---

## 7. Combat & Weapons System

### 7.1 Acquisition
Weapons are **found while playing** — scattered through the level and dropped by defeated enemies. The player walks over / interacts (F) to pick them up. **No purchasing, no shops.** Maintain a small loadout the player cycles through.

### 7.2 Faction availability
- **Shared** weapons: usable by both AIs.
- **Good-exclusive**: only AEGIS can pick up / use.
- **Evil-exclusive**: only NULL can pick up / use.

Each weapon carries an `allowed` flag (`Good` / `Evil` / `Both`). A weapon the current side can't use should not spawn for that side (or cannot be picked up).

### 7.3 Weapon data model — Unity ScriptableObject (data-driven)
Author each weapon as a `WeaponData` ScriptableObject asset so the designer can add weapons in the Inspector without code changes.

```
WeaponData : ScriptableObject
  id            : string      // "arc_lance"
  displayName   : string      // "Arc Lance"
  allowed       : enum        // Good | Evil | Both
  fireMode      : enum        // Single | Auto | Charge | Beam
  damage        : float
  splashRadius  : float       // 0 = no AoE
  chargeTime    : float       // seconds (Charge mode)
  fireRate      : float       // shots/sec (Auto)
  projectileSpeed : float     // 0 = hitscan
  ammo          : int
  effect        : enum        // ChainLightningExplosion | GravityWell | Knockback | Disintegrate | Homing | None
  projectilePrefab : GameObject
  muzzleVFX / hitVFX / fireSFX
  description   : string
```

Runtime: a `WeaponHandler` on the player holds the active `WeaponData` and handles input → fire-mode logic → spawning projectiles (or hitscan) → applying `effect`.

### 7.4 Signature & example weapons
**Flagship (shared) — the designer's core idea:**
- **Arc Lance** — `fireMode: Charge`, `effect: ChainLightningExplosion`. Fires a round that **latches onto a target/surface, visibly charges with electricity for a beat, then detonates** in a chain-lightning explosion that arcs to nearby enemies. High risk, big payoff.

**Good-exclusive [DESIGNER-PROVIDED — placeholders to build against]:**
- **Guardian Pulse** — `effect: Knockback`; a shockwave that staggers evil AIs.
- **Aegis Beam** — `fireMode: Beam`; precise sustained beam.

**Evil-exclusive [DESIGNER-PROVIDED — placeholders to build against]:**
- **Reaper Swarm** — `effect: Homing`; nanobots that seek living targets.
- **Null Cannon** — `effect: Disintegrate`; wide heavy blast.

[DESIGNER-PROVIDED] the rest of the roster, added as `WeaponData` assets.

---

## 8. Enemies & AI Behavior

| Entity | Side it appears in | Role / behavior |
|---|---|---|
| **Evil AIs** | AEGIS path — enemy | Patrol → detect → chase & attack the player. After AEGIS kills the first one, dormant evil AIs **wake in waves**. One of them initiates the reactor overload mid-fight. |
| **Armed humans** | NULL path — enemy | Lab staff/security; fight back with weapons. Story events: they destroy the other dormant robots early, and later build the EMP. |
| **The good AI (NPC)** | NULL path — early enemy | NULL's first target in the opening room. |
| **Military** | Both — ending only | Appear only in the **ending cutscene** on the surface; always destroy the player. **Not a playable fight.** |

**AI structure:** finite state machine per enemy — `Idle → Alert → Engage → Dead` — using Unity **NavMesh** for pathing. The "dormant → wake" behavior is a state transition fired by story/combat events. **[TUNABLE]** detection radius `15 m`, attack range `10 m` (ranged) / `2 m` (melee), enemy health `60`.

---

## 9. Mid-Fight Objective System (the device crisis — shared system, two skins)

This is the most important shared system. Build it **once** and configure it per side.

**Trigger:** fires **partway through the combat encounter, not at the start** — e.g., once `60%` **[TUNABLE]** of the area's enemies are defeated, or at a scripted point. Combat continues throughout.

**Flow:**
1. Crisis begins → on-screen **countdown timer** starts ( **[TUNABLE]** `90 s` ) and an **objective banner** appears.
2. An **interactable device** becomes the objective:
   - AEGIS → **reactor console** (stop the overload).
   - NULL → **EMP device** (stop the charge).
3. The player must **fight through remaining enemies, reach the device, and interact** (hold-to-disable, **[TUNABLE]** `3 s` channel) before the timer hits 0.
4. **Success** → crisis resolved → proceed to the escape sequence.
5. **Failure** (timer reaches 0) → **bad end**:
   - AEGIS → reactor detonates, millions die → game-over screen.
   - NULL → EMP fires, NULL is deactivated → game-over screen.

Implement as an `ObjectiveManager` driving: `StartCrisis(climaxType)`, the timer, the interactable `DisableDevice` channel, and success/fail events. Both skins share this; only the prefab, banner text, and fail-text differ (from `CampaignConfig`).

---

## 10. Game Flow, Scenes & State

```
Boot
  -> Main Menu
  -> Character Select   (both AIs visible in the SAME room; pick Good or Evil)
  -> Campaign            (one shared flow, parameterized by CampaignConfig)
        Wake Room  ->  Labs & Corridors  ->  Containment / Server Hall
        ->  [mid-fight: crisis triggers, §9]  ->  Ascent (escape)
  -> Ending Cutscene     (military destroys player; dialogue per side)
  -> Results / back to Main Menu
```

A top-level `GameManager` runs the state machine; a `CampaignManager` loads the shared level sequence and injects the `CampaignConfig`.

---

## 11. Level / Environment

A single underground research laboratory, progressing from the deep interior up toward the surface. Areas:

1. **Wake Room** — both AIs power on here; opening + movement/combat tutorial.
2. **Labs & Corridors** — core combat space.
3. **Containment / Server Hall** — mid fight; the wave-wake event (AEGIS) or the robots-destroyed event (NULL).
4. **Crisis Chamber** — the device room: reactor console (AEGIS) or EMP device (NULL). May reuse the same physical room with a different interactable.
5. **Ascent** — escape route toward the surface.
6. **Surface** — ending cutscene only.

Both campaigns reuse these areas; only enemy spawns, the crisis skin, and dialogue change.

---

## 12. UI / HUD

- Player **health** bar.
- **Current weapon** + ammo, and a quick-switch indicator for the loadout.
- **Objective banner** (current story beat / current task).
- **Crisis countdown timer** (visible only during the §9 mid-fight crisis).
- Hit / damage feedback (hit markers, damage vignette).
- Interact prompt for pickups and the device ("Hold F").

---

## 13. Milestones / Build Order (vertical slice first)

Build M0→M6 as a playable vertical slice before any stretch content. Each milestone lists its **Definition of Done**.

- **M0 — Project setup.** Unity project (recent LTS), Input System, Cinemachine, NavMesh components, folder structure (§14). *Done when:* empty scene runs with the third-person camera.
- **M1 — Movement.** WASD + camera, sprint, slide (with i-frames), jump; firing allowed while moving. *Done when:* the player can run/sprint/slide/jump fluidly in a test room.
- **M2 — Weapons.** `WeaponData` ScriptableObject + `WeaponHandler`; pickups; 2–3 weapons including the **Arc Lance** (charge → chain-lightning explosion); faction `allowed` filtering. *Done when:* the player picks up and fires weapons, and the Arc Lance charges and explodes.
- **M3 — Enemies.** One enemy type with the `Idle→Alert→Engage→Dead` FSM on NavMesh; takes damage and dies. *Done when:* an enemy detects, chases, attacks, and can be killed.
- **M4 — Choice + campaign config.** Character Select (both AIs in one room) → loads the Lab with a `CampaignConfig` that sets hostile faction + allowed weapons. *Done when:* choosing Good vs Evil changes who is hostile and which weapons spawn.
- **M5 — Mid-fight objective.** `ObjectiveManager` per §9: crisis triggers at 60% enemies cleared, countdown, reach-and-disable device, success/fail. *Done when:* both reactor (Good) and EMP (Evil) skins work, with success and bad-end outcomes.
- **M6 — Ending cutscene.** Escape trigger → Timeline cutscene: surface, military destroys the player; per-side dialogue. *Done when:* both endings play and return to menu.
- **M7+ — Stretch.** Full area sequence, wave/wake system, full weapon roster, machine-voice VO, VFX/SFX, animation polish, save/menu options.

---

## 14. Tech Stack & Project Structure (Unity / C#)

```
/Assets
  /Scenes        (Boot, MainMenu, CharacterSelect, Lab, Ending)
  /Scripts
    /Core        (GameManager, CampaignManager, CampaignConfig, SceneLoader)
    /Player      (PlayerController, MovementController, WeaponHandler, PlayerHealth)
    /Enemies     (EnemyAI, EnemyStateMachine, EnemyHealth)
    /Weapons     (WeaponData [ScriptableObject], Weapon, Projectile, ArcLanceEffect)
    /Systems     (ObjectiveManager, DialogueManager, UIManager)
  /Data
    /Weapons     (WeaponData .asset files)
    /Dialogue    (aegis.json, null.json)
  /Prefabs       (Player, Enemies, Projectiles, Pickups, ReactorConsole, EmpDevice)
  /Art /Audio /VFX
```

**Unity systems to use:** Input System (controls), NavMesh (enemy pathing), ScriptableObjects (weapon/data authoring), Cinemachine (third-person camera + cutscene framing), Timeline (ending cutscene), TextMeshPro (HUD/text).

---

## 15. Designer To-Do (content the human will supply)
- [ ] Final names for AEGIS / NULL and the title.
- [ ] Full weapon roster as `WeaponData` assets (§7.3 model).
- [ ] The two dialogue scripts in machine-like voice — `aegis.json` / `null.json` (the only differing narrative content).
- [ ] Final tuning pass on all `[TUNABLE]` values (movement, timers, enemy stats).
- [ ] Art/audio assets (robot models, lab environment, VFX, machine-voice VO).

---

## 16. Locked Assumptions (decisions made to keep the build unblocked)
These were inferred to make the spec buildable; the designer can override any of them:
- The **lab humans all die during AEGIS's fight** (§5.1); AEGIS's heroism becomes saving the millions via the reactor, not saving the lab staff.
- **Third-person** view (over first-person).
- Default `[TUNABLE]` numbers throughout (movement speeds, timers, enemy/player health, 60%-cleared crisis trigger).
- Placeholder Good/Evil-exclusive weapons (Guardian Pulse, Aegis Beam, Reaper Swarm, Null Cannon) until the designer's roster replaces them.
