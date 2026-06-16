# DUAL CORE — Systems Roadmap (Build Order by Tier)
*Companion to the GDD. Engine: Unity (C#), 3D, third-person.*

---

## How to use this roadmap

This is the **build order** for the game's systems, grouped into tiers. **Build tier by tier, lowest first. Do not skip ahead** — each tier depends on the ones below it. Each tier ends with a **GATE**: don't move to the next tier until the gate passes.

- Start at **Tier 0** and finish it before touching Tier 1.
- Within a tier, systems can be built in any order unless a dependency is noted.
- **Tiers 0–6 = playable vertical slice. Tier 7 = content + polish.**
- See the GDD for the *design* of each system; this file is the *order and dependencies*.

**Legend:** ▸ = a system/module to build. `[TUNABLE]` values and `[DESIGNER]` content are defined in the GDD.

---

## TIER 0 — Foundation & Project Setup
**Purpose:** a Unity project that runs, with structure, packages, and core scaffolding in place.
**Depends on:** nothing.

▸ **Unity project** — create on a recent LTS; pick the render pipeline (URP recommended for performance).
▸ **Packages** — install & configure: Input System, Cinemachine, AI Navigation (NavMesh), TextMeshPro, Timeline.
▸ **Project structure** — folders per GDD §14 (`/Scripts/Core`, `/Player`, `/Enemies`, `/Weapons`, `/Systems`, `/Data`, `/Prefabs`, etc.).
▸ **Version control** — git repo + Unity `.gitignore`.
▸ **Boot scene + test room** — an empty greybox room with a floor and lighting to test everything in.
▸ **Core scaffolding** — `GameManager` (top-level state-machine stub: Boot → Menu → Campaign → Ending), `SceneLoader` (load/unload scenes).

**GATE:** project opens, runs, and shows the test room with a placeholder camera; no errors.

---

## TIER 1 — Core Player & Camera
**Purpose:** a fully controllable robot in the test room.
**Depends on:** Tier 0.

▸ **Input mapping** — Input System action map (move, look, sprint, slide, jump, fire, switch-weapon, interact).
▸ **Third-person camera** — Cinemachine orbit camera following the player.
▸ **Movement controller** — WASD ground movement + sprint + jump (CharacterController-based).
▸ **Slide system** — dash from sprint, with brief reduced hitbox / i-frames, duration & cooldown `[TUNABLE]`.
▸ **Player health** — health value, `TakeDamage()`, death event (shared damage interface lives in Tier 2).

**GATE:** the player can walk, sprint, jump, and slide fluidly with the third-person camera; firing (stub) does not interrupt movement.

---

## TIER 2 — Combat & Weapons Foundation
**Purpose:** pick up and fire weapons (incl. the Arc Lance) and deal damage.
**Depends on:** Tier 1.

▸ **Damage interface** — `IDamageable` implemented by player and enemies (single shared contract).
▸ **WeaponData (ScriptableObject)** — the data-driven weapon definition (GDD §7.3).
▸ **WeaponHandler** — equips active weapon, reads fire input, runs fire modes (Single / Auto / Charge / Beam).
▸ **Projectile + hitscan** — projectile prefab system and a hitscan path (chosen per weapon by `projectileSpeed`).
▸ **Weapon-effect framework** — maps `effect` enum to behavior; implement **ChainLightningExplosion** first for the **Arc Lance** (latch → charge → AoE chain).
▸ **Pickup & loadout** — world pickups (interact to grab), a small loadout, weapon switching.
▸ **Faction availability** — `allowed` (Good/Evil/Both) filtering so a side only spawns/uses its permitted weapons.

**GATE:** the player picks up at least 2 weapons including the Arc Lance, fires them, the Arc Lance charges and detonates, and damage kills a test target.

---

## TIER 3 — Enemy AI
**Purpose:** enemies that detect, chase, attack, die, and spawn in waves.
**Depends on:** Tier 2 (damage + weapons).

▸ **NavMesh** — bake navigation on the test room / level.
▸ **Enemy base + FSM** — `Idle → Alert → Engage → Dead` state machine on NavMesh.
▸ **Perception** — detection radius + line-of-sight to acquire the player `[TUNABLE]`.
▸ **Enemy combat + health** — ranged and/or melee attack, `IDamageable` health, death.
▸ **Two archetypes from one base** — **Evil AI** and **Armed Human** (same base, different data/prefab).
▸ **Spawner / wave system** — spawn enemies and support the "wake in waves" behavior (AEGIS path) and faction spawning.

**GATE:** an enemy detects, chases, attacks, and is killed; a wave of enemies can be spawned and cleared.

---

## TIER 4 — Campaign Branching & Faction Config
**Purpose:** the Good/Evil choice changes hostile faction and weapon set within the same level.
**Depends on:** Tiers 2 & 3.

▸ **CampaignConfig** — data object carrying `side`, `hostileFaction`, `allowedWeapons`, `climaxType`, `dialogueSet` (GDD §3).
▸ **Faction system** — defines who is hostile to whom; enemies read their faction from config.
▸ **Character Select scene** — both AIs shown **in the same room**; selecting one writes the `CampaignConfig`.
▸ **CampaignManager** — loads the single shared level and injects the config: spawns the correct hostile faction and filters weapon spawns.

**GATE:** choosing AEGIS spawns evil AIs + good-side weapons; choosing NULL spawns armed humans + evil-side weapons — in the same level.

---

## TIER 5 — Mission & Objective Systems (the mid-fight crisis)
**Purpose:** the timed reach-and-disable crisis (reactor / EMP) works for both sides.
**Depends on:** Tiers 3 & 4.

▸ **Interactable framework** — hold-to-interact (hold-F) with a channel time `[TUNABLE]`.
▸ **ObjectiveManager** — tracks objective state and raises banner/update events.
▸ **Mid-fight crisis system** (GDD §9) — trigger partway through combat (e.g. ~60% enemies cleared, `[TUNABLE]`), start a countdown, require the player to reach and disable the device while combat continues.
▸ **Crisis skins** — same system, two prefabs/labels: **reactor console** (AEGIS, stop overload) and **EMP device** (NULL, stop charge), read from `climaxType`.
▸ **Win / lose handling** — success → proceed to escape; failure (timer 0) → **bad end** (reactor detonates / NULL deactivated).

**GATE:** in both campaigns the crisis triggers mid-fight, the countdown runs, disabling the device in time succeeds, and letting it expire produces the correct bad-end.

---

## TIER 6 — Narrative, UI & Ending (completes the vertical slice)
**Purpose:** full HUD, story beats, and both endings.
**Depends on:** Tiers 4 & 5.

▸ **DialogueManager** — loads `[DESIGNER]` `aegis.json` / `null.json`, plays lines by story beat; hook for machine-voice VO.
▸ **UIManager / HUD** — health, current weapon + ammo, weapon-switch indicator, objective banner, **crisis countdown timer**, interact prompts, hit/damage feedback.
▸ **Menus** — Main Menu, Pause, Game-Over / Results.
▸ **Ending cutscene** — Timeline sequence on the surface: the military destroys the player; per-side dialogue (same scene, different script).

**GATE:** a full playthrough works end to end for both sides — choose → fight → mid-fight crisis → escape → ending cutscene → results — with HUD and dialogue.

---

## TIER 7 — Content, Polish & Audio (post-slice)
**Purpose:** expand from vertical slice to full build.
**Depends on:** Tiers 0–6 complete.

▸ **Full weapon roster** — `[DESIGNER]` weapons authored as `WeaponData` assets (incl. Good/Evil-exclusive sets).
▸ **Full level** — all six areas (Wake Room → Labs → Containment → Crisis Chamber → Ascent → Surface) built and connected.
▸ **Animation** — locomotion, sprint, slide, fire, hit, death.
▸ **VFX & SFX** — weapon effects, explosions, the reactor/EMP, impacts.
▸ **Machine-voice VO** — both AIs and any human/AI lines.
▸ **Balancing** — final pass on all `[TUNABLE]` values.
▸ **Settings & save** — options menu, audio/controls settings, save/checkpoint if desired.
▸ **Optimization & bug-fixing** — performance pass and QA.

**GATE:** all areas playable, both campaigns content-complete, performant, and bug-checked.

---

## Dependency Summary (quick view)

```
Tier 0  Foundation
   |
Tier 1  Player & Camera
   |
Tier 2  Weapons & Combat ----+
   |                         |
Tier 3  Enemy AI ------------+
   |                         |
Tier 4  Campaign Branching --+   (needs Weapons + Enemy AI)
   |
Tier 5  Objective / Crisis       (needs Enemy AI + Campaign)
   |
Tier 6  Narrative, UI, Ending    (needs Campaign + Crisis)  <-- vertical slice complete
   |
Tier 7  Content, Polish, Audio   (needs everything above)
```

**Vertical slice = Tiers 0–6.** Ship/test that before investing in Tier 7 content.
