# AEGIS — Game Design Document & Build Spec
*Single-player 3D first-person shooter • Unity (C#) • two branching campaigns*

---

## 0. Brief for the Build Agent (read this first)

**What you are building:** a 3D **first-person shooter** in **Unity (C#)** with a **visible full body** (the camera sits on the character's head, so looking down shows the player's own torso and legs). The player chooses one of two robot AIs: **AEGIS.2** (good) or **X** (evil). Each wakes alone in a different part of an underground laboratory and fights through it to the same tragic ending — at the lab entrance, armed military destroy the player, knowing only that "an incident" occurred. The two campaigns **share most systems and the ending** but each has its **own scripted intro and beats**; build the shared systems once and drive the per-side differences from a `CampaignConfig`.

**Hard constraints (do not change):**
- Engine **Unity**, language **C#**, **3D**, **first-person** camera with a **visible full body** (looking down reveals the player's own body).
- The two AIs wake in **different rooms** (AEGIS.2: a clean empty room; X: a failed-projects storage room). They do **not** start together.
- Each side has a **mid-fight crisis** with an on-screen countdown: AEGIS.2 = **reactor overload** (shut it down); X = **EMP bomb** (shut it down so he isn't destroyed). The crisis triggers **partway through, not at the start**.
- Both campaigns end at the **lab entrance** with the **military destroying the player** — a short cutscene; only the dialogue/context differs.

**Legend:** `[TUNABLE]` = implement a default but expose it for tweaking. `[DESIGNER]` = content the human supplies later; build the system and use placeholders. Everything else is a committed decision.

---

## 1. Overview

**Title:** AEGIS *(working title; note the good-AI character is "AEGIS.2")*

**Logline:** Two robot AIs wake in a ruined underground lab — one built to protect, one built to destroy. You choose which to become. Both fight their way to the surface, and both are gunned down by a military that never learns the truth.

**Genre:** Single-player 3D **first-person shooter (FPS)**, two branching campaigns, one shared tragic ending.

---

## 2. Design Pillars
1. **One choice, two stories.** Good vs evil flips who is hostile, the weapon set, the mid-fight crisis, the scripted beats, and the dialogue — but both arrive at the same fate.
2. **Momentum combat.** Sprint, slide, and fire flow together; firing never locks movement.
3. **Embodied first person.** A visible full body grounds the player — they see their weapon and, looking down, their own body during sprints, slides, and falls.
4. **Shared spine, distinct flesh.** Both campaigns reuse the same combat systems, the crisis system, and the ending. Build those once; script each campaign's unique intro and beats on top, parameterized by `CampaignConfig`.

---

## 3. Characters & Lore
- **AEGIS.2** — the good AI and one playable character. Wears an **"AEGIS.2"** insignia on its shoulder. Built to protect humans.
- **X** — the evil AI and the other playable character. A **failed AEGIS unit** that was disposed of; wakes angry at the humans who discarded it. Sole purpose: kill all humans.
- **Corrupted AEGIS units (the rogue robots)** — the enemies in AEGIS.2's campaign. Each bears an **AEGIS insignia scratched out and marked with an X**. Hostile to humans and to AEGIS.2.
- **Insignia recognition (lore + visual detail):** robots identify friend/foe by the **shoulder insignia**. A rogue robot reads the player's "AEGIS.2" mark and turns hostile; X likewise recognizes an "AEGIS.2"-marked robot and attacks. Mechanically this is faction hostility; visually, render the insignia on robot shoulders. (In first person, the player can glance down and see their own shoulder insignia.)

---

## 4. What Is Shared vs What Differs
Build one set of systems and a single `Campaign` flow; inject differences via `CampaignConfig`.

| | SHARED (build once) | DIFFERS (inject per side) |
|---|---|---|
| Movement & combat systems | ✅ | — |
| Mid-fight crisis *system* (timed, reach-and-disable) | ✅ | reactor (AEGIS.2) vs EMP bomb (X) |
| Ending (military at the entrance) | ✅ | dialogue / context |
| Hostile faction | — | corrupted AEGIS robots (AEGIS.2) vs armed humans (X) |
| Player weapon set | — | shared + AEGIS.2-only vs shared + X-only |
| Scripted intro & beats | — | each campaign's own sequence (see §5–§6) |
| Dialogue | — | per side |

**`CampaignConfig` carries:** `side` (Good/Evil), `hostileFaction`, `allowedWeapons`, `climaxType` (Reactor/EMP), `dialogueSet`.

---

## 5. Story — AEGIS.2 (good) campaign
1. **Wake.** AEGIS.2 powers on in a **neat, clean, empty room** — no humans, no other robots, nothing.
2. **Explore + first weapon.** The player gets up, explores the room, and finds a **gun-like weapon** (the starting weapon).
3. **Into the halls.** Exiting the room, the hallway is **damaged** — objects fallen and broken.
4. **The scripted encounter.** Ahead, down a hallway to the left, a **human runs and falls**; a **corrupted AEGIS robot** (AEGIS insignia scratched, marked X) closes in and **shoots the human dead**.
5. **First fight.** The robot spots the player, **reads the "AEGIS.2" insignia**, and turns hostile. The player defends and **destroys it**.
6. **The bloody hallway.** Advancing back along the route the human and robot came from, the player reaches a **blood-streaked hallway** (the aftermath — escalation cue).
7. **Escalating combat.** The player pushes forward through **more corrupted AEGIS robots** for a stretch `[TUNABLE/DESIGNER duration]`.
8. **Crisis — reactor overload.** A **siren** warns the **nuclear reactor is nearing overload**; a **countdown timer** appears in the UI `[TUNABLE amount — designer to finalize]`. The player must **reach and shut down the reactor** before it expires (combat may continue). Success = averted; failure = bad end.
9. **The discovery.** After shutting it down, AEGIS.2 finds that **every human in the lab is dead**.
10. **The exit.** AEGIS.2 heads for the entrance.
11. **Ending (cutscene).** At the entrance, **armed military** are waiting. Knowing only that **something happened in the lab** — not that he was saving humans — they **open fire and destroy him**.

---

## 6. Story — X (evil) campaign
1. **Wake.** X is dumped in a **broken old storage room for failed projects** and powers on **angry at the humans** who discarded it.
2. **Slip out + first weapon.** X leaves the room **discreetly, unseen** (brief optional stealth), and **finds a weapon**.
3. **The rampage begins.** Armed, X **starts killing humans** — exterminating all of them becomes its sole purpose. **Humans fight back** with weapons.
4. **The failsafe.** Amid the slaughter, a **scientist reaches a kill-switch / button** that **destroys all the other dormant failed-project robots** — leaving X the **only active AI**. *(Locked interpretation — see §18.)* The **fleeing scientist** is caught and **shot** by X.
5. **Meeting AEGIS.2.** X turns and sees **another robot bearing the "AEGIS.2" insignia** (the good AI, here as an NPC) and **attacks it**.
6. **The overheard plan.** Moving on discreetly, X **overhears humans planning to set off an EMP** to destroy it.
7. **Crisis — EMP bomb.** X **kills those humans** and heads for the **EMP bomb**, kills the humans guarding it, and **shuts the bomb down** before it can fire `[TUNABLE timer]`. **Completing this = X survives;** failure = X is destroyed (bad end).
8. **Cleanup + exit.** Once sure **all humans are dead**, X leaves the lab.
9. **Ending (cutscene).** At the entrance, **armed military** — aware an incident occurred — **destroy X**. The same fate.

---

## 7. Convergent Ending & Tone
- **Both** campaigns end at the **lab entrance** with the **military destroying the player** in a **short cutscene**. The structure is identical; only the **dialogue/context** differs. For AEGIS.2 it is tragic irony; for X it is simply the end of a killer.
- **Tone / voice:** both AEGIS.2 and X speak in **machine-like, synthetic voices** (they are robots). Dialogue differs in content and intent, not delivery. [DESIGNER] both scripts.

---

## 8. Controls & Movement (3D, first-person)

| Action | Default input | Notes |
|---|---|---|
| Move | WASD | Ground movement |
| Look / aim | Mouse | First-person look; pitching down reveals the player's own body |
| Sprint | Left Shift (hold) | Faster move speed |
| Slide | Left Ctrl while sprinting | Low dash; brief i-frames; camera lowers; **fire allowed during slide** |
| Jump | Space | Standard jump |
| Fire | Left Mouse | Allowed while moving, sprinting, sliding — never locks movement |
| Switch weapon | Mouse wheel / 1–4 | Cycle loadout |
| Interact / pick up | F | Pick up weapons; shut down reactor/EMP (hold) |

**First-person body:** the camera is anchored to the **head of the actual character model** (full-body first person), so looking down shows the torso, legs, and the equipped weapon — not a floating arms-only viewmodel. Hide or scale the head/neck mesh near the camera to avoid clipping. Drive body motion (walk, sprint, slide, fire) with first-person body animations.

`[TUNABLE]` starting values: walk `5 m/s`, sprint `8 m/s`, slide speed `11 m/s`, slide duration `0.6 s`, slide cooldown `0.8 s`, jump height `1.5 m`, player health `100`, mouse sensitivity exposed in settings.

**Tech:** Input System (controls), CharacterController movement, and a **custom code-driven first-person camera** — a plain Unity `Camera` parented to the head anchor and rotated by a script (mouse look, pitch clamp). **Not Cinemachine.**

---

## 9. Combat & Weapons System
**Acquisition:** weapons are **found while playing** (no shops) — starting weapon in the intro room, plus pickups and enemy drops. Small loadout, cycle to switch.

**Faction availability:** each weapon has `allowed` = `Good` (AEGIS.2-only) / `Evil` (X-only) / `Both` (shared). A side only spawns/uses what it's permitted.

**Data model — `WeaponData` ScriptableObject:**
```
WeaponData : ScriptableObject
  id, displayName
  allowed        : Good | Evil | Both
  fireMode       : Single | Auto | Charge | Beam
  damage, splashRadius, chargeTime, fireRate, projectileSpeed, ammo
  effect         : ChainLightning | Knockback | Beam | Homing | Disintegrate | None
  projectilePrefab, muzzleVFX, hitVFX, fireSFX
  description
```

**Signature weapons** (others are [DESIGNER]):
- **Arc Lance** *(shared)* — `fireMode: Auto`, a **rapid-fire sci-fi machine gun** for sustained pressure.
- **Charge Cannon** *(X-only)* — `fireMode: Charge`, `effect: Disintegrate` — **charges, then fires a wide disintegration blast** (heavy collateral).
- **Guardian Pulse** *(AEGIS.2-only)* — `effect: Knockback`, a staggering shockwave.
- **Aegis Beam** *(AEGIS.2-only)* — `fireMode: Beam`, precise sustained beam.
- **Reaper Swarm** *(X-only)* — `effect: Homing` nanobots that seek living targets.

---

## 10. Enemies, Hazards & Behavior

| Entity | Campaign | Role / behavior |
|---|---|---|
| **Corrupted AEGIS robots** | AEGIS.2 — enemy | Patrol → detect → engage the player. The first is a scripted kill after it shoots the fleeing human; more appear as the player advances. |
| **Armed humans** | X — enemy | Lab staff/security that fight back; a scientist triggers the failsafe; others build and guard the EMP. |
| **AEGIS.2 (NPC)** | X — encounter | Appears in X's path; X recognizes the insignia and attacks. |
| **Reactor** | AEGIS.2 — hazard/objective | Begins overloading partway in; siren + timer; shut down to avert. |
| **EMP bomb** | X — hazard/objective | Humans build it to destroy X; reach and shut it down before it fires. |
| **Failsafe button** | X — scripted | A scientist triggers it, destroying the other dormant robots (X is now alone). |
| **Military** | both — ending only | At the lab entrance; destroy the player in the ending cutscene. Not a playable fight. |

**AI structure:** per-enemy FSM `Idle → Alert → Engage → Dead` on Unity NavMesh; faction-aware via the faction system. `[TUNABLE]` detection radius `15 m`, attack range `10 m` (ranged) / `2 m` (melee), enemy health `60`.

---

## 11. Mid-Fight Crisis System (shared, two skins)
Build once; configure per side. **Trigger partway through combat** (e.g. after ~60% of the area's enemies are cleared, or a scripted point) `[TUNABLE]`. A **countdown** starts `[TUNABLE]` and an **objective banner** appears. The player must **fight to the device and hold-interact to disable it** before the timer hits zero.
- AEGIS.2 → **reactor console** (stop the overload). Success → continue to the discovery + exit. Fail → reactor detonates (bad end).
- X → **EMP bomb** (stop the charge). Success → X survives, continue to cleanup + exit. Fail → X is destroyed (bad end).

`CrisisManager`/`ObjectiveManager` drives: trigger, timer, the interactable device, success/fail events. Only the prefab and banner/fail text differ (from `CampaignConfig`).

---

## 12. Game Flow & Scenes
```
Boot -> Main Menu -> Character Select (choose AEGIS.2 or X)
  -> Campaign  (shared systems; per-side scripted beats from CampaignConfig)
       AEGIS.2: clean room -> halls -> scripted encounter -> bloody hall -> combat -> reactor crisis -> discovery -> exit
       X:        storage room -> stealth/weapon -> rampage -> failsafe -> meet AEGIS.2 -> overhear EMP -> EMP crisis -> cleanup -> exit
  -> Ending cutscene (military at the entrance destroys the player; per-side dialogue)
  -> Results / Main Menu
```
`GameManager` runs the top-level state machine; `CampaignManager` loads the level and injects `CampaignConfig`.

---

## 13. Level / Environment (the laboratory)
One underground lab, presented as a connected sequence. Areas:
1. **AEGIS.2 intro — clean room** (empty; first weapon; tutorial).
2. **X intro — failed-projects storage** (broken; stealth out; first weapon; tutorial).
3. **Damaged hallways** — debris, the scripted human/robot encounter (AEGIS.2 side).
4. **Bloody hallway** — aftermath / escalation.
5. **Lab interior** — main combat (corrupted robots for AEGIS.2; armed humans for X), the failsafe room (X).
6. **Reactor room** (AEGIS.2 crisis) / **EMP bomb room** (X crisis).
7. **Entrance / threshold** — the ending cutscene with the military.

Shared geometry where sensible; spawns, the crisis device, scripted beats, and dialogue change per side.

---

## 14. UI / HUD
Health bar; current weapon + ammo and quick-switch; objective banner; **crisis countdown timer** (during §11); hit/damage feedback; interact prompt ("Hold F"); a reticle/crosshair for first-person aiming.

---

## 15. Tech Stack & Project Structure (Unity / C#)
```
/Assets
  /Scenes   (Boot, MainMenu, CharacterSelect, Lab, Ending)
  /Scripts
    /Core      (GameManager, CampaignManager, CampaignConfig, SceneLoader, AudioManager, EventChannels)
    /Player    (PlayerController, MovementController, FirstPersonCamera, WeaponHandler, HealthSystem, InteractionController)
    /Enemies   (EnemyController, EnemyStateMachine, EnemySpawner, FactionSystem)
    /Weapons   (WeaponData [SO], Weapon, Projectile)
    /Systems   (ObjectiveManager/CrisisManager, DialogueManager, UIManager, StealthSensor)
  /Data       (WeaponData/EnemyData/CampaignConfig assets, dialogue)
  /Prefabs /Art /Audio /VFX
```
**Unity systems:** Input System, NavMesh (AI Navigation), ScriptableObjects, Timeline (ending cutscene), TextMeshPro. The **first-person camera is a custom C# script** (a plain `Camera` on the head anchor, rotated by code) — **not Cinemachine**. The character uses a **full-body first-person rig** (camera on the head bone/anchor; body mesh visible; head mesh hidden near camera). Build order is in the **systems roadmap** (companion doc).

---

## 16. Scope & MVP (vertical slice first)
**View:** first-person with a **visible full body**. MVP: Character Select; first-person movement (mouse look + walk/sprint/slide/jump, fire while moving) with the body visible when looking down; 2–3 weapons incl. the Arc Lance machine gun with faction filtering; one enemy type with the FSM on NavMesh; one playable area with a scripted encounter and the mid-fight crisis (reach-and-disable, success + bad-end); the ending cutscene stub. Then expand to the full beats, full roster, full level, art/audio/VO.

---

## 17. Designer To-Do
- [ ] Final names (AEGIS.2 / X / title) — keep insignia art consistent.
- [ ] Full weapon roster as `WeaponData` assets.
- [ ] Both dialogue scripts (machine-voice).
- [ ] Crisis timer amounts (reactor / EMP) and combat durations.
- [ ] First-person body model + animations; mouse sensitivity defaults.
- [ ] Final `[TUNABLE]` values; art/audio/VO; enemy variants.

---

## 18. Locked Assumptions (override any of these)
- **First-person view with a visible full body** — the player sees their own body when looking down (full-body FP rig, not arms-only).
- **X's "button" beat:** a fleeing **scientist triggers a failsafe** that destroys the other dormant robots (leaving X alone), then is killed by X.
- **X's discreet exit** is treated as a **brief, optional stealth** intro.
- **Reactor overload cause** (AEGIS.2) is left as "amid the chaos"; attribute to the rogue robots if a cause is needed.
- **Weapon swap folded in:** Arc Lance is the shared rapid-fire machine gun; the charge/disintegration weapon is X-only.
- Default `[TUNABLE]` numbers throughout; placeholder weapons until the roster lands.
