# AEGIS — Systems Roadmap (by Dependency Tier)
*Companion to the GDD. Unity (C#), 3D, first-person (visible full body).*

---

## How to use this roadmap

Scripts are grouped by **dependency tier**. **Tier 0 has no dependencies on other game scripts** (standalone managers, data, interfaces, utilities) — build and test these first, in isolation. Every higher tier depends **only on tiers below it**, so build **bottom-up** and never write a script before the things it needs exist.

- Each entry lists the script, a one-line purpose, and **deps:** (which tier its dependencies live in).
- Keep managers **decoupled via event channels** so they stay buildable in isolation.
- A working **vertical slice** is reachable around Tier 4; Tier 5 is content + polish.

**Legend:** `[SO]` = ScriptableObject (data). `[singleton]` = single persistent instance. Items in *italics* are optional/stretch.

---

## TIER 0 — Standalone scripts (no dependencies on other game scripts)
Build first; each works alone.

**Managers / services**
- `GameManager` `[singleton]` — top-level app state machine (Boot → MainMenu → CharacterSelect → Campaign → Ending); broadcasts state via events. **deps:** none.
- `AudioManager` `[singleton]` — play SFX/music; volume control. **deps:** none.
- `SceneLoader` `[singleton]` — load/unload/additive scene management. **deps:** none.
- `SaveSettingsManager` `[singleton]` — options, key bindings, volumes, mouse sensitivity; persistence. **deps:** none.

**Decoupling & utilities**
- `EventChannel` / `GameEvents` `[SO]` — typed event channels so systems talk without hard refs. **deps:** none.
- `InputReader` `[SO]` — Input System wrapper exposing actions (move, look, sprint, slide, jump, fire, switch, interact). **deps:** none.
- `ObjectPool<T>` — generic pooling for projectiles/VFX. **deps:** none.
- `Singleton<T>` — base utility for the managers above. **deps:** none.
- `Timer` — reusable countdown/stopwatch helper. **deps:** none.

**Contracts (interfaces)**
- `IDamageable` — anything that can take damage. **deps:** none.
- `IInteractable` — anything the player can interact with (hold-F). **deps:** none.

**Data definitions `[SO]` (pure data, no logic)**
- `WeaponData` — id, allowed (Good/Evil/Both), fireMode, damage, ammo, effect, prefabs, etc. **deps:** none.
- `EnemyData` — health, speed, detection/attack ranges, weapon/faction. **deps:** none.
- `CampaignConfig` — side, hostileFaction, allowedWeapons, climaxType, dialogueSet. **deps:** none.
- `DialogueData` — ordered lines keyed by story beat. **deps:** none.

**Build note:** Tier 0 should compile and run with a test scene that pokes each manager (play a sound, fire an event, load a scene) before moving on.

---

## TIER 1 — Core mechanics (depend only on Tier 0)
- `FactionSystem` — defines who is hostile to whom; resolves the insignia/faction logic. **deps:** data/enums (T0).
- `HealthSystem` — implements `IDamageable`; raises damaged/death events; pings `AudioManager`. **deps:** T0.
- `MovementController` — walk/sprint/slide/jump via `InputReader` + CharacterController. **deps:** T0.
- `FirstPersonCamera` — **custom C# script (not Cinemachine)**: a head-anchored **first-person** camera + mouse look (pitch/yaw, pitch clamp); keeps the **full body visible** and hides/scales the head mesh near the camera to avoid clipping. **deps:** T0.
- `Projectile` — travels, applies damage via `IDamageable`, returns to `ObjectPool`. **deps:** T0.
- `InteractionController` — raycasts for `IInteractable`, drives hold-to-interact via `InputReader`. **deps:** T0.
- `UIManager` / HUD — subscribes to event channels; shows health, ammo, objective banner, **crisis timer**, crosshair, prompts. **deps:** T0 (events).
- `DialogueManager` — plays `DialogueData` lines on beat; raises/consumes events. **deps:** T0.
- *`StealthSensor`* — line-of-sight/detection for X's intro. **deps:** T0.

---

## TIER 2 — Actors & interactive objects (depend on Tier 1 + below)
- `Weapon` + `WeaponHandler` — equips `WeaponData`, runs fire modes (Single/Auto/**Charge**/Beam), spawns `Projectile`; reads `InputReader`. Covers the **Arc Lance** (Auto) and the **Charge Cannon** (Charge). **deps:** T0 data + T1 `Projectile`.
- `WeaponPickup` — world pickup; respects `allowed` faction; uses `InteractionController`. **deps:** T1.
- `PlayerController` — composes `MovementController` + `FirstPersonCamera` + `HealthSystem` + `InteractionController` + `WeaponHandler`; manages the **first-person body rig**. **deps:** T1–T2.
- `EnemyController` (+ `EnemyStateMachine`) — FSM `Idle → Alert → Engage → Dead` on NavMesh; uses `HealthSystem`, fires a `Weapon`/`Projectile`, faction-aware via `FactionSystem`; reads `EnemyData`. Two archetypes from one base: **corrupted AEGIS robot** and **armed human**. **deps:** T1–T2.
- `InteractableDevice` — the **reactor console** / **EMP bomb**; implements `IInteractable`; hold-to-disable; raises success/fail events. **deps:** T0–T1.
- `HitFeedback` — spawns hit/death VFX from `ObjectPool` on health events. **deps:** T0–T1.

---

## TIER 3 — Encounter systems (depend on Tier 2 + below)
- `EnemySpawner` / `WaveSystem` — spawns `EnemyController`s (incl. escalating waves of corrupted robots). **deps:** T2.
- `CrisisManager` (a.k.a. `ObjectiveManager`) — triggers the mid-fight crisis partway through (e.g. ~60% cleared `[TUNABLE]`), runs the `Timer`, points the player at the `InteractableDevice`, fires success/bad-end. Skinned reactor vs EMP from `CampaignConfig`. **deps:** T2 device + T1 UI + T0 events/timer.
- `ScriptedSequence` / `TriggerZone` — fires story beats (the hallway encounter, the human/robot scripted kill, the failsafe, overhearing the EMP, sirens). **deps:** T2 spawner/actors + T0 events.

---

## TIER 4 — Campaign orchestration (depend on Tier 3 + below) — vertical slice completes here
- `CampaignManager` — reads `CampaignConfig`; sets `FactionSystem`, tells `EnemySpawner` which faction to spawn, filters `WeaponPickup`s, and sequences the level's scripted beats + the crisis. **deps:** T3.
- `CharacterSelectController` — writes `CampaignConfig` (AEGIS.2 vs X) and tells `GameManager` to start the campaign. **deps:** T0 + T4 config.
- `EndingSequencer` — Timeline cutscene at the entrance (military destroys the player); plays per-side dialogue. **deps:** T1 dialogue + T0.

**Slice gate:** choose a side → fight the correct faction with the correct weapons (first-person, body visible) → mid-fight crisis (reach-and-disable, success + bad-end) → ending cutscene → results.

---

## TIER 5 — Content, polish & audio (depend on everything)
- **Level assembly** — build/connect all areas (AEGIS.2 clean room, X storage room, damaged halls, bloody hall, lab interior, reactor room, EMP room, entrance); bake NavMesh; lighting.
- **Content** — full weapon roster (`WeaponData` assets), enemy variants (`EnemyData`), the insignia art on robot shoulders, the **first-person body model**.
- **Presentation** — first-person body animations (locomotion, slide, fire, death), VFX/SFX wiring, **machine-voice VO**, both dialogue scripts.
- **Tuning & QA** — finalize all `[TUNABLE]` values and crisis timers, mouse sensitivity, performance pass, bug-fixing.

---

## Dependency summary
```
Tier 0  Standalone: managers, events, input, pools, interfaces, data SOs
   |        (no game-script deps — build & test in isolation first)
Tier 1  Core mechanics: faction, health, movement, first-person camera, projectile, interaction, UI, dialogue
   |
Tier 2  Actors & objects: weapons, pickups, PlayerController, EnemyController, reactor/EMP device
   |
Tier 3  Encounter systems: spawner/waves, crisis manager, scripted triggers
   |
Tier 4  Campaign: CampaignManager, character select, ending sequencer   <-- vertical slice
   |
Tier 5  Content, polish, audio
```
Build strictly bottom-up: a script may only reference scripts in **lower** tiers.
