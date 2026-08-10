# Zombtoy Plan Execution Log

**Purpose:** the running checklist for executing [`ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md`](ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md).
Designed to survive session breaks: every item is a checkbox, every owner-gate is explicit, and
"Where execution stopped" at the bottom is always current.

**Legend:** `[ ]` todo · `[x]` done · `[~]` in progress · 🧑 **owner-only gate** (Claude must pause here) · 🤖 agent-executable

**Started:** 2026-07-12 · **Branch at start:** `feature/Titan-Zombunny` @ `441cae5b` (6 ahead of `master`, 0 behind)

---

## Branch strategy (DECIDED — see §B for reasoning)

Target end-state: **`master` is the trunk; feature branches are short-lived and deleted after merge.**

Execution order (order matters — see the PR #25 trap):

1. `feature/Titan-Zombunny` → merge into `master` (clean fast-forward, PR #28)
2. **Retarget PR #25 base → `master`** *(must happen before any branch deletion)*
3. Land PR #25, then delete `feature/player-inventory-&-weapon-system-refactor`
4. **Only then** delete `feature/core-architecture-refactor` (it is currently PR #25's base)
5. Delete `origin/highscore_backend` (historical)

> ⚠️ **TRAP FOUND 2026-07-12 (corrects plan §4/M4):** `gh pr view 25` reports
> `baseRefName: feature/core-architecture-refactor`. Deleting that branch — which the plan lists as a
> safe M4 cleanup — **would auto-close PR #25 and orphan the AmmoSystem work.** Retarget first, delete last.

---

## M0 — Session-start housekeeping 🤖

- [x] Read plan + re-verify git state
- [x] Diagnose PR #28 `BLOCKED` → cause is `REVIEW_REQUIRED` only; `master` is **not** branch-protected, so the owner can merge directly
- [x] Discover PR #25 base-branch trap (above)
- [x] Create this execution log
- [x] Commit pending docs (principal-engineer plan, inspector audit, index rows, checkpoint/branch-audit updates)
- [x] Back up local-only commit `39e7efe6` to `origin` (it existed on one machine only)

## M1 (tail) — Validate & merge PR #28

- [x] Rotation-freeze fix implemented + batchmode-validated (`b993e60a`)
- [ ] 🧑 **OWNER GATE 1 — play-test Level1 boss fight.** Checklist in §A below. **Execution is paused here.**
- [ ] 🤖 Merge PR #28 into `master` (after owner reports pass)
- [ ] 🤖 Delete `feature/Titan-Zombunny` local+remote after merge
- [ ] 🤖 Update CODE_MAP/checkpoint status rows

**Rollback:** revert the merge commit; branch content survives in git.

## M2 — Land inventory/ammo work (PR #25)

*Prereq: M1 merged.*

- [x] 🤖 Push local-only `39e7efe6` to origin (data-loss protection — done early, out of order, deliberately)
- [ ] 🤖 Retarget PR #25 base → `master` (`gh pr edit 25 --base master`)
- [ ] 🤖 Rebase branch onto new `master`; resolve `EnemyProjectile.cs` **in master's favor** (Titan rewrite wins), then re-apply interface changes; camera scripts also master's favor
- [ ] 🤖 Review `stash@{0}` (interface WIP — apply if still meaningful)
- [ ] 🧑 **OWNER GATE 2 — review `stash@{1}`** ("ammo experimental changes (likely dump)" — owner's own label; confirm drop)
- [ ] 🧑 **OWNER GATE 3 — play-test all 4 weapon slots**, reloads, ammo pickups, weapon switching
- [ ] 🤖 Merge PR #25; delete branch; clear applied/dropped stashes
- [ ] 🤖 Update CODE_MAP rows (`AmmoSystem` live; `Ammo.cs` copies retired)

**Constraint:** `AmmoSystem` stays a per-weapon **component**, never promoted to a manager (ADR-006).
**Rollback:** revert merge commit.

## M3 — The Cull (one deletion PR, zero behavior change)

*Prereq: M1 + M2 merged (#25 touches `ProjectileWeapon`/`IFirearm`).*

Pre-flight (must all pass before any deletion):
- [ ] 🤖 Re-run GUID census for every file on the delete list → expect 0 scene/prefab refs each
- [ ] 🤖 Grep every deleted **class name as a string** (`Type.GetType`, `SendMessage`, `AddComponent("…")`) → expect only self-references
- [ ] 🤖 GUID-check `Debug/ScoreDebugger` + `ScoreManagerDebugger`; include in deletion only if unwired

Delete files (plan §8-M3): `Core/GameStateManager.cs`, `Core/GameStarter.cs`, `Core/ComponentCache.cs`,
`Managers/GameOverManager.cs`, `Player/PlayerHealthRefactored.cs`, `Player/PlayerHealthProxy.cs`,
`Player/PlayerInputManager.cs`, `Player/PlayerMovementRefactored.cs`, `Weapons/WeaponSystem.cs`,
`Weapons/WeaponManager.cs`, `Weapons/RaycastWeapon.cs`, `Weapons/ProjectileWeapon.cs`, unwired interfaces
(`IFirearm`, `ISpell`, `IPlayerWeapon`, + `IProjectile` if no live implementors), `vectorTest.cs`,
`Assets/Prefabs/Player.prefab` — **all with their `.meta` files**.

- [ ] 🤖 Delete the file list above
- [ ] 🤖 Strip reflection probes from `HealthPotion.cs`, `AmmoItem.cs`, `EnemyManager.cs:~141`
- [ ] 🤖 Delete `ScoreManager.cs:168-186` dead GameStateManager log-block + GameStarter comment
- [ ] 🤖 Remove `OnNetworkEvent`/`NetworkEvent` from `GameEvents.cs`
- [ ] 🤖 **Scene edit 1:** remove disabled `PlayerMovementRefactored` component from Level1 Player *(else missing-script warning — from inspector audit)*
- [ ] 🤖 **Scene edit 2:** remove `vectorTest` object/component from Level1
- [ ] 🤖 Batchmode compile → exit 0
- [ ] 🧑 **OWNER GATE 4 — open all 7 build scenes + enemy prefabs; confirm ZERO missing-script warnings** (only the editor can surface these reliably)
- [ ] 🧑 **OWNER GATE 5 — full playthrough**: menu → Level1 → all weapons → death → results → leaderboard. *If anything NREs: HALT and re-audit, do not patch.*
- [ ] 🤖 Standalone player build compiles
- [ ] 🤖 Update CODE_MAP (all-green), close/re-scope issues #3 and #16

**Expected:** ≈ −2,300 lines, ~15 files. **Rollback:** single revert (one PR).

## M4 — Repo & product hygiene

- [ ] 🤖 Delete tracked Node backend `Assets/Scripts/Server/zombtoy-backend/` (kills the 4 dependabot alerts)
- [ ] 🤖 Untrack + gitignore C-backend build artifacts (`mongoose.o`, `obj/`, binary, `zombtoy_c.db`)
- [ ] 🤖 Delete `feature/core-architecture-refactor` **(only after PR #25 is retargeted/landed)**
- [ ] 🤖 Delete `origin/highscore_backend`
- [ ] 🧑 **OWNER GATE 6 — confirm dreamlo deletion** (`HighScores.cs` + its `Menu 3` scene ref; .NET client becomes canonical). Plan recommends yes
- [ ] 🧑 **OWNER GATE 7 — confirm `Level2.unity` fate** (not in build; plan default = delete)
- [ ] 🤖 Fold or drop `stash@{2}` (workflow.md +23 lines)
- [ ] 🧑 **OWNER GATE 8 — confirm Titan spawn strategy** (scene-placed vs EnemyManager spawn table; plan recommends scene-placed for a scripted boss)
- [ ] 🧑 **OWNER GATE 9 — state multiplayer horizon** (informs ADR-008; plan assumes "not near-term")

## M5 — Tooling refresh

- [ ] 🤖 Fix `EVENT_RAISE_RE` in `DevTools/Diagrams/common.py` to match trigger-method style (`GameEvents.PlayerDeath()`)
- [ ] 🤖 Regenerate `DevTools/Diagrams/out/`; confirm against plan §0 pub/sub census
- [ ] 🤖 Seed `docs/architecture/adr/` with ADR-001…008 one-pagers (plan §11)

## M6+ — Features resume (§9 rules apply)

Boss polish → balancing pass (fix the double-speed quirk inside it) → one of shop (#21) / camera (#22) / single-scene flow (#19).

---

## §A — Owner Gate 1 play-test checklist (do this next)

Open `Assets/Level1.unity`, press Play, and check:

1. **Rotation never freezes.** Walk toward and away from the Titan while moving the mouse — the player must keep facing the cursor at all times, especially crossing in/out of the boss's range ring (~13 m: SphereCollider r=3 × boss scale 4.5).
2. **Crosshair behaves.** A red square appears on the ground when you enter range, trails toward you (deliberate lag), disappears when you leave range. **The level floor must never move or vanish.**
3. **Boss attacks.** Rockets fire from `ShootPoint` every ~2 s toward where the marker sits; they explode and damage on hit.
4. **Console is clean.** No NullReferenceExceptions; specifically no `[EnemyTargetShooting]` error about groundTarget.
5. **Rest of the level still works** — regular enemies spawn (EnemyManager was re-enabled), score counts, player can die.

**Report back:** pass, or the symptom + any console text. Merging PR #28 is blocked on this.

## §B — Branch strategy reasoning

| Branch | State (verified 2026-07-12) | Action |
|---|---|---|
| `master` @ `ce956b05` | Trunk; unprotected; tip is itself a "WIP" commit | Becomes the real trunk once #28 lands |
| `feature/Titan-Zombunny` @ `441cae5b` | 6 ahead / 0 behind → **clean fast-forward**; PR #28 open, MERGEABLE | Merge → delete |
| `feature/player-inventory-&-weapon-system-refactor` | PR #25 open; **base = core-arch branch (wrong)**; 1 local-only commit (now backed up) | Retarget to master → rebase → merge → delete |
| `feature/core-architecture-refactor` | PR #2 merged Aug 2025; 25/26 commits patch-identical in master → fully integrated | **Delete LAST** (PR #25's base until retargeted) |
| `origin/highscore_backend` | Pre-refactor historical | Delete |

**Why this shape:** the repo's branches accumulated because work was cherry-picked to master while branches
stayed standing — which is exactly what made "what work exists where" unanswerable after 8 months. Trunk +
short-lived branches removes that question permanently. After the rebase in M2, both live branches share
`master` as their base, which is the "same base" outcome the owner asked for.

---

## Where execution stopped

**2026-07-12 — paused at OWNER GATE 1 (§A play-test).**

- Done this session: M0 complete (docs committed, tracker created, PR-25 trap found, `39e7efe6` backed up to origin).
- Everything downstream (merge #28 → M2 → Cull) is blocked on the play-test result. Nothing else in the plan
  can proceed safely without it: the Cull's own validation strategy depends on knowing the live game is healthy.
- **Next agent action after owner reports pass:** `gh pr merge 28` (merge or FF), delete the branch, then
  start M2 at "retarget PR #25 base".
- No code/scene/prefab changes are pending in the working tree.
