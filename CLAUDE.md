# CLAUDE.md — AEGIS

> Keep this file at the **repository root** (the folder with `Assets/` and `ProjectSettings/`). Claude Code reads it automatically every prompt. Full design and build docs are in **`ConstraintsC/`** — open them for detail.

## Project
**AEGIS** is a single-player **3D first-person shooter** in **Unity (C#)** with a **visible full body** (the camera is on the character's head, so looking down shows the player's own body). The player chooses one of two robot AIs: **AEGIS.2** (good — protects humans) or **X** (evil — a discarded, failed AEGIS unit out to kill all humans). Each wakes alone in a different part of a ruined underground lab and fights through it to the **same tragic ending**: at the lab entrance, armed military destroy the player, knowing only that "an incident" occurred. The two campaigns **share their core systems and the ending** but each has its **own scripted intro and beats**. *(Note: the game is titled "AEGIS"; the good-AI character is "AEGIS.2".)*

## Authoritative documents (read for detail)
- `ConstraintsC/AEGIS_GDD.md` — full design: story, systems, data models.
- `ConstraintsC/AEGIS_SYSTEMS_ROADMAP.md` — build order by **dependency tier** (Tier 0 = standalone scripts).

If anything here conflicts with those docs, **the docs win — flag the conflict** instead of guessing.

## Tech stack & environment
- Engine: Unity (recent LTS); language **C#**; pipeline **URP**; **3D**; **first-person** camera with a **visible full body**.
- Packages: **Input System**, **AI Navigation (NavMesh)**, **TextMeshPro**, **Timeline**. The **first-person camera is a custom C# script — NOT Cinemachine** (a plain Unity `Camera` on the head anchor, rotated by code).

## Hard constraints (do NOT change)
1. **Unity + C# + 3D + first-person**, with a **visible full body** — the player sees their own body when looking down (full-body FP rig: camera on the head, body mesh visible, head mesh hidden near the camera). The first-person camera is a **custom C# script, not Cinemachine**. No 2D, no third-person, no other engine.
2. The two AIs wake in **different rooms** — AEGIS.2 in a clean empty room, X in a failed-projects storage room. They do **not** start together (they meet later, only in X's campaign).
3. **Shared spine, distinct campaigns:** build the combat, crisis, and ending systems **once** and parameterize per side with a `CampaignConfig` `{ side, hostileFaction, allowedWeapons, climaxType, dialogueSet }`. Each campaign then has its own scripted intro/beats — do not duplicate whole systems.
4. **Hostiles per side:** AEGIS.2 fights **corrupted AEGIS robots** (insignia scratched, marked X); X fights **armed humans**. Robots identify friend/foe by the **shoulder insignia** (faction hostility; render the insignia).
5. **Mid-fight crisis** triggers **partway through, never at the start**, and plays during combat: AEGIS.2 = **reactor overload** (shut down to avert); X = **EMP bomb** (shut down so X isn't destroyed). One shared timed *reach-and-disable* system, two skins.
6. **Ending:** both campaigns end at the **lab entrance** with the **military destroying the player** — a short cutscene; **only dialogue/context differs**.
7. **Weapons** are **found during play** (no shops), data-driven via `WeaponData` ScriptableObjects (`allowed`: Good/Evil/Both). Some shared, some AEGIS.2-only, some X-only. **Arc Lance** = shared rapid-fire **machine gun**; the **charge/disintegration cannon** is **X-only**.
8. **Movement:** sprint, slide (brief i-frames), jump. **Firing is allowed while moving, sprinting, and sliding — firing must never lock movement.**

## Build approach
- Build by **dependency tier, starting at Tier 0** (standalone scripts with no dependencies — e.g. `GameManager`, `AudioManager`). Only build a script once everything it depends on exists. Follow `AEGIS_SYSTEMS_ROADMAP.md`.
- Stay **data-driven** (ScriptableObjects for weapons/enemies; `CampaignConfig` for the branch) and **event-driven** (decouple managers via event channels).
- Make **small, reviewable changes**; commit working states to git; ask before large refactors or deleting files.
- After editing scripts, recompile and **check the Unity console for errors** before continuing.

## Coding conventions
- One public type per file; filename matches the type. PascalCase types/methods/properties; camelCase locals; `_camelCase` private fields.
- Namespace under `Aegis.*` (e.g. `Aegis.Player`, `Aegis.Weapons`, `Aegis.Systems`).
- Keep MonoBehaviours thin: data in ScriptableObjects, logic in focused systems/managers.
- Follow the folder structure in the GDD (§15) and roadmap.

## [DESIGNER-PROVIDED] — do not invent; leave clean hooks
Final names (AEGIS.2 / X / title), the full weapon roster, both dialogue scripts (machine-voice), crisis timer amounts, the first-person body model/animations, art/audio/VO, and final `[TUNABLE]` values are supplied by the human. Use placeholders/defaults from the GDD until then.
