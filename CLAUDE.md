# CLAUDE.md — DUAL CORE

> Keep this file at the **repository root** (the folder that contains `Assets/` and `ProjectSettings/`). Claude Code reads it automatically on every prompt. The full design and build docs live in **`ConstraintsC/`** — open them when you need detail.

## Project
**DUAL CORE** is a single-player **3D third-person action shooter** built in **Unity (C#)**. The player chooses to play as one of two robot AIs that wake up in the **same lab room** mid-experiment: **AEGIS** (good — protects humans) or **NULL** (evil — kills humans). The two choices are **mirror campaigns** that share one level, one escape, and one ending; only the hostile faction, weapon set, mid-fight crisis, and dialogue differ.

## Authoritative documents (read these for detail)
- `ConstraintsC/DUAL_CORE_GDD.md` — full game design: systems, story, data models.
- `ConstraintsC/DUAL_CORE_SYSTEMS_ROADMAP.md` — the build order, by tier, with a GATE per tier.

Read the relevant section before implementing a system. **If anything here conflicts with those docs, the docs win — flag the conflict instead of guessing.**

## Tech stack & environment
- Engine: Unity (recent LTS); language **C#**; render pipeline **URP**; **3D**; **third-person** camera.
- Packages: **Input System**, **Cinemachine**, **AI Navigation (NavMesh)**, **TextMeshPro**, **Timeline**.

## Hard constraints (do NOT change)
1. **Unity + C# + 3D + third-person.** No 2D, no other engine.
2. Both AIs — **AEGIS** (good) / **NULL** (evil) — power on in the **same room** at the start.
3. **Mirror design:** build the campaign **once** and parameterize it with a `CampaignConfig` `{ side, hostileFaction, allowedWeapons, climaxType, dialogueSet }`. Do **not** fork into two separate campaigns.
4. **Mid-fight crisis** triggers **partway through the fight — never at the start** — and plays out **while combat continues**: a reactor overload (AEGIS, save millions) or a human EMP (NULL, save itself). One shared timed *reach-and-disable* system, two skins.
5. **Ending:** both campaigns end in the **same short cutscene** — the military destroys the player on the surface. **Everyone in the lab dies**, so the military cannot tell AEGIS was good and destroys it anyway. **Only the dialogue differs** between the two endings.
6. **Weapons** are **found during play** (no shops). Some shared, some AEGIS-only, some NULL-only — data-driven via `WeaponData` ScriptableObjects (`allowed`: Good/Evil/Both). Flagship weapon: **Arc Lance** (charge → chain-lightning explosion).
7. **Movement:** sprint, slide (brief i-frames), jump. **Firing is allowed while moving, sprinting, and sliding — firing must never lock movement.**

## Build approach
- Build **tier by tier starting at Tier 0**; do not skip ahead. Pass each tier's **GATE** before moving up.
- Stay **data-driven** (ScriptableObjects for weapons; a `CampaignConfig` for the branch).
- Make **small, reviewable changes**. Commit working states to git. Ask before large refactors or deleting files.
- After editing scripts, recompile and **check the Unity console for errors** before continuing.

## Coding conventions
- One public type per file; filename matches the type. PascalCase for types/methods/properties; camelCase for locals; `_camelCase` for private fields.
- Namespace everything under `DualCore.*` (e.g. `DualCore.Player`, `DualCore.Weapons`, `DualCore.Systems`).
- Keep MonoBehaviours thin: data in ScriptableObjects, logic in focused systems/managers.
- Follow the folder structure in the GDD (§14) and roadmap.

## [DESIGNER-PROVIDED] — do not invent; leave clean hooks
Final names (AEGIS/NULL/title), the full weapon roster, the two dialogue scripts (machine-voice), art/audio, and final `[TUNABLE]` values are supplied by the human. Use the placeholders/defaults from the GDD until then.
