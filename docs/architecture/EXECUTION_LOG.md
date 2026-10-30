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
- [x] 🧑 Merge-policy decision: repo ruleset "Build" requires 1 approving review (unsatisfiable solo) → **owner chose per-PR `--admin` merges**, ruleset kept for its no-deletion / no-force-push protections

## M1 (tail) — Validate & merge PR #28 ✅ COMPLETE

- [x] Rotation-freeze fix implemented + batchmode-validated (`b993e60a`)
- [x] 🧑 **OWNER GATE 1 — play-test PASSED** (2026-07-12): rotation never freezes, crosshair tracks, boss fires, console clean
- [x] 🤖 Merged PR #28 into `master` (`2fb46f2a`, admin-merge)
- [x] 🤖 Deleted `feature/Titan-Zombunny` local + remote
- [x] 🤖 CODE_MAP/checkpoint status reconciled in M5 (2026-10-30)

**Rollback:** revert the merge commit; branch content survives in git.

## M2 — Land inventory/ammo work (PR #25)

*Prereq: M1 merged.*

- [x] 🤖 Push local-only `39e7efe6` to origin (data-loss protection — done early, out of order, deliberately)
- [x] 🤖 Retarget PR #25 base → `master`
- [x] 🤖 **Rebase abandoned in favour of cherry-pick** — see finding below. Branch reset to `master` + 2 cherry-picks (`4eb3bc5d`, `e95508ea`); safety branch `backup/inventory-pre-rebase` holds the original
- [x] 🤖 Batchmode compile after cherry-picks: **exit 0, zero CS errors**; Titan boss files + crosshair verified intact
- [x] 🧑 **OWNER GATE 2b — AmmoSystem disposition: DROP the unwired half** (owner decision 2026-07-12). Removed in `ab3db9c9` with rationale in the commit message; recoverable from history + `backup/inventory-pre-rebase`
- [x] 🤖 Re-verified compile after removal (exit 0, 0 errors; a transient `CS2001` was a stale gitignored `.csproj` reference, self-healed on regeneration)
- [x] 🤖 Force-pushed rebuilt branch (`--force-with-lease`); PR #25 title/body rewritten to the reduced scope
- [x] 🧑 **OWNER GATE 3 — play-test weapons PASSED** (see recorded result below)
- [x] 🤖 PR #25 merged and both branches deleted (`2087409a`)
- [x] 🧑 **OWNER GATE 2 — stashes: LEAVE ALONE for now** (owner decision); revisit all three during M4

### 🔍 M2 findings (2026-07-12) — these change M2's stated scope

**1. Rebase was the wrong tool.** `git rebase --onto master d40b4eb2` tried to replay the entire Aug-2025
core-refactor commit (`58d2d99b`), conflicting in MusicManager/TagManager/REFACTOR_PLAN — because most of the
branch's history is patch-*similar* but not patch-*identical* to master's cherry-picked copies.
**Cherry-picking the 2 real commits onto master produced zero conflicts.** The feared `EnemyProjectile`/camera
conflicts never materialized: those files were merely *stale tree state* on the old branch, not changes the
commits made. `dca3ac3c` "Initial plan" is an empty Copilot placeholder — dropped.

**2. PR #25 is two different things, and only one of them is wired:**

| Part | Wired? | Verdict |
|---|---|---|
| `Ammo.cs` gains `CanShoot()` / `TryShoot()` / `CanReload()`; `Pistol`, `RocketLauncher`, `PlayerShooting` refactored to call them instead of poking `ammoScript.ammo` directly | **YES** — modifies the live `Ammo.cs` (27 live instances) and live weapon scripts | **Keep.** This is the in-place house pattern (same as `Inventory.cs`) and is the real value of the PR |
| `IProjectile.cs` | **YES** — `Rocket` and `IceBullet` both implement it (2 live implementors) | **Keep** — earned per plan §4's "interface with a wired implementor stays" |
| `AmmoSystem.cs` (123 l) + `AMMO_REFACTOR_SUMMARY.md` | **NO** — zero scene/prefab refs, zero code refs; its own header says "works alongside existing Ammo.cs" | **Recommend drop** — it is a *new* dormant parallel class, precisely what standing rule #1 (wired-in-same-PR) and ADR-002 forbid. Merging it would repeat the 2025 mistake the Cull exists to undo |
- [ ] 🤖 Review `stash@{0}` (interface WIP — apply if still meaningful)
- [x] 🧑 **OWNER GATE 3 — weapons play-test PASSED** (2026-07-12)
- [x] 🤖 Admin-merged PR #25 (`2087409a`); deleted feature branch + `backup/inventory-pre-rebase`
- [x] 🤖 CODE_MAP rows updated in Cull, stale surrounding claims corrected in M5

**M2 COMPLETE.** Note: with #25 merged, `feature/core-architecture-refactor` is no longer any PR's base —
the M4 deletion is now unblocked.

**Final decision:** keep the existing `Ammo` component; the unwired `AmmoSystem` was dropped (ADR-006).
**Rollback:** revert merge commit.

## M3 — The Cull (one deletion PR, zero behavior change)

*Prereq: M1 + M2 merged (#25 touches `ProjectileWeapon`/`IFirearm`).*

Pre-flight (must all pass before any deletion):
- [x] 🤖 GUID census run → all delete-targets **0 scene/prefab refs**, except the two known scene-edit cases
- [x] 🤖 String-form reference grep (`Type.GetType`, `SendMessage`, `AddComponent("…")`) → all located and stripped
- [x] 🤖 GUID-checked both debuggers

### 🔍 Cull pre-flight findings — 3 corrections to plan §8-M3 scope

1. **`vectorTest` is in THREE scenes, not one.** Plan said "remove from Level1"; census found it in
   `Level1`, `Level2`, **and** `Level3` (identical fileIDs — the scenes were copied). It sits on a dedicated,
   already-inactive object named **"coordnate spawner test"** (Transform + vectorTest only), so the whole
   GameObject is removed rather than just the component.
2. **`ScoreManagerDebugger` IS WIRED** (`Menu 1.unity`) → **KEEP**, per the plan's own conditional
   ("keep if wired for debugging"). Only `ScoreDebugger` (0 refs) gets deleted.
3. **`IProjectile`'s `IWeapon`/`ISpell` mention is in a COMMENT only** — no code dependency, so deleting
   `ISpell`/`IPlayerWeapon`/`IFirearm` cannot break the interface we're keeping.

Also confirmed: `Assets/Prefabs/Player.prefab` still 0 scene refs → safe to delete.

Delete files (plan §8-M3): `Core/GameStateManager.cs`, `Core/GameStarter.cs`, `Core/ComponentCache.cs`,
`Managers/GameOverManager.cs`, `Player/PlayerHealthRefactored.cs`, `Player/PlayerHealthProxy.cs`,
`Player/PlayerInputManager.cs`, `Player/PlayerMovementRefactored.cs`, `Weapons/WeaponSystem.cs`,
`Weapons/WeaponManager.cs`, `Weapons/RaycastWeapon.cs`, `Weapons/ProjectileWeapon.cs`, unwired interfaces
(`IFirearm`, `ISpell`, `IPlayerWeapon`, + `IProjectile` if no live implementors), `vectorTest.cs`,
`Assets/Prefabs/Player.prefab` — **all with their `.meta` files**.

- [x] 🤖 Strip reflection probes from `HealthPotion.cs`, `AmmoItem.cs`, `EnemyManager.cs` (all 3 done — live code is now simpler)
- [x] 🤖 Delete `ScoreManager` dead GameStateManager log-block + GameStarter comment
- [x] 🤖 Remove `OnNetworkEvent`/`NetworkEvent` from `GameEvents.cs` (ADR-008) + rescope its doc-comment to ADR-004
- [x] 🤖 Write `Assets/Editor/CullSceneCleanup.cs` — does both scene edits via the **Unity API** (safer than hand-editing YAML across 3 scenes); deleted again at the end of the Cull
- [x] 🧑 Owner closed the Unity editor
- [x] 🤖 Ran `CullSceneCleanup.Run` → removed 3 `vectorTest` objects (Level1/2/3) + 1 `PlayerMovementRefactored` component (Level1 Player); 3 scenes re-saved by Unity itself
- [x] 🤖 Deleted the full file list (17 scripts + `Player.prefab`, all with `.meta`)
- [x] 🤖 Deleted `Assets/Editor/CullSceneCleanup.cs` + its folder
- [x] 🤖 Batchmode compile → **exit 0, 0 CS errors, no missing-script warnings** (a first pass showed 17 transient `CS2001` stale-`.csproj` entries; the gitignored project file regenerated clean, same self-healing pattern as M2)
- [x] 🤖 **Result: 73 → 56 scripts, 7,369 lines deleted, zero remaining references to any culled class**
- [x] 🤖 CODE_MAP updated — the 🔴 Dormant tier is gone; every listed script is wired
- [x] 🧑 **OWNER GATE 4 — missing-script sweep PASSED** (2026-07-12)
- [x] 🧑 **OWNER GATE 5 — full playthrough PASSED** — behavior identical after deleting 7,369 lines. **This validates the wiring-census methodology end-to-end** (plan §14's stated purpose for the Cull)
- [x] 🤖 Merged as PR #31; branch deleted
- [x] 🤖 Issue #3 closed; #16 received the Cull resolution comment during M4

**M3 COMPLETE.**

**Expected:** ≈ −2,300 lines, ~15 files. **Rollback:** single revert (one PR).

## M4 — Repo & product hygiene

- [x] 🤖 Deleted tracked Node backend (1,430 files, **−533,876 lines**) — PR #32. **Dependabot alerts: 5 → 0** ✅
- [x] 🤖 Untracked + gitignored C-backend build artifacts (kept on disk locally)
- [x] 🤖 Deleted `feature/core-architecture-refactor` (local + remote)
- [x] 🤖 Deleted `origin/highscore_backend`
- [x] 🤖 **Repo is now a single `master` branch, local and remote** — the "what work exists where" question is closed
- [x] 🧑 **GATE 6 — dreamlo: KEEP BOTH clients for now.** `HighScores.cs` (dreamlo) stays wired in `Menu 3` alongside `Leaderboard.cs`; it works without a local server, which is convenient mid-development. **Revisit trigger:** when the leaderboard UI gets real work, or before any public release — two clients writing to different stores is a known, accepted wart until then
- [x] 🧑 **GATE 7 — `Level2.unity`: DELETED.** Not in Build Settings, no code references, and its EnemyManager spawn table was serialized empty (would spawn nothing). Build indices unaffected — it was never in the list. Recoverable from git history
- [x] 🧑 **GATE 8 — Titan: SCENE-PLACED** (confirms current setup; no change needed). Bosses are scripted set-pieces, not spawn-table entries. Trigger-gated activation deferred to M6 boss polish
- [x] 🧑 **GATE 9 — multiplayer: NOT NEAR-TERM.** Confirms **ADR-008** as written: no networking provisions in client code; finish the single-player loop first. Revisit only via a dedicated ADR
- [ ] 🤖 Stashes: all 3 still parked (owner deferred). `stash@{0}`/`stash@{1}` are pre-Cull inventory experiments whose surrounding code has since changed substantially — expect them to be dropped rather than applied; `stash@{2}` is a small `workflow.md` addition

## M5 — Tooling refresh and Unity verification (2026-10-30)

- [x] Shared two-pass event scanner: 19 events; real method bodies; no file-order dependency
- [x] Two-pass interface classification; deterministic output; failures propagate
- [x] Remove invented lifecycle callbacks/order; label source-analysis limits
- [x] 19 scanner/regression tests passing
- [x] Regenerate six diagrams (PlantUML, SVG, PNG) and two reports; rendered core/event-health diagrams inspected
- [x] Seed ADR-001…008, correct Ammo/singleton facts, reconcile current status docs
- [x] Add editor-only validation/build/snapshot/prefs commands and shell runner
- [x] Untrack four old Python bytecode files and ignore future caches
- [x] Unity asset validation/compile: 33 scenes/prefabs, 0 errors; 57 legacy lighting warnings retained
- [ ] macOS standalone development build
- [ ] GUI smoke: movement/turning and Titan range/crosshair/projectiles
- [ ] GUI smoke: four weapons, reload/empty/pickups; spawns and score
- [ ] GUI smoke: pause/resume, death/results/leaderboard with temporary backend
- [ ] Restore original PlayerPrefs and stop test backend
- [ ] Inspect final diff; publish PR; merge only with required checks passing

See [UNITY_VALIDATION.md](UNITY_VALIDATION.md) for commands, evidence and limits.

## M6+ — Features resume (§9 rules apply)

Boss polish → balancing pass (fix the double-speed quirk inside it) → one of shop (#21) / camera (#22) / single-scene flow (#19).

---

## §A — Historical Gate 1 checklist (passed)

Open `Assets/Level1.unity`, press Play, and check:

1. **Rotation never freezes.** Walk toward and away from the Titan while moving the mouse — the player must keep facing the cursor at all times, especially crossing in/out of the boss's range ring (~13 m: SphereCollider r=3 × boss scale 4.5).
2. **Crosshair behaves.** A red square appears on the ground when you enter range, trails toward you (deliberate lag), disappears when you leave range. **The level floor must never move or vanish.**
3. **Boss attacks.** Rockets fire from `ShootPoint` every ~2 s toward where the marker sits; they explode and damage on hit.
4. **Console is clean.** No NullReferenceExceptions; specifically no `[EnemyTargetShooting]` error about groundTarget.
5. **Rest of the level still works** — regular enemies spawn (EnemyManager was re-enabled), score counts, player can die.

**Result:** owner passed this checklist; PR #28 was merged.

## §C — Historical Gate 3 checklist (passed)

PR #25 changed how **every weapon decides whether it can fire**. `Pistol`, `RocketLauncher`, and
`PlayerShooting` now call `Ammo.TryShoot()` / `CanShoot()` instead of checking and decrementing `ammo`
themselves. Behavior should be identical — this checklist is looking for off-by-one or reload-state bugs.

Open `Assets/Level1.unity`, press Play, and for **each of the 4 weapon slots** (number keys):

1. **Fires and consumes ammo** — the counter decrements by exactly 1 per shot (not 0, not 2).
2. **Stops at empty** — firing halts at 0; no shots "for free" at zero ammo.
3. **Reload works** (R): counter refills, and you **cannot fire mid-reload**.
4. **Rocket launcher specifically** — rockets still spawn, fly, explode, and damage (its call site changed most).
5. **Ammo pickups** still increase totals correctly.
6. **Console clean** — no NREs from `Ammo`/`Pistol`/`RocketLauncher`/`PlayerShooting`.

Also confirm the **boss still works** (regression check on the Titan merge): crosshair tracks, rockets fire,
rotation never freezes.

**Result:** owner passed this checklist; PR #25 was merged.

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

**2026-10-30 — M0–M4 complete; M5 implemented, Unity verification in progress.**

Branch: `codex/m5-tooling-and-validation`, based on `master` `a98be89a`.
The three deferred stashes remain untouched. Owner decisions remain: both leaderboard clients kept,
Level2 deleted, Titan scene-placed, multiplayer not near-term. M6 gameplay changes are out of this PR.

Current evidence: 19 Python tests pass; six diagrams rendered successfully; source census agrees across
both event reports. Unity asset validation passed: 33 scenes/prefabs, 0 errors, 57 unresolved legacy lightmap-parameter references retained as warnings. The macOS standalone build is running.
Unity needed execution outside the agent sandbox to access its existing licensing database.
Evidence directory: `/private/tmp/zombtoy-validation/`.

**Exact next step:** inspect `build.log`/`build.txt`; if build succeeds, launch the editor and begin the GUI smoke session through Tools > Zombtoy > Begin Smoke Session.

Unfinished checks above must stay unchecked on interruption. Record runtime defects with logs and
snapshots; do not change gameplay to make a check pass. Implementation checkpoint is the M5 tooling/validation commit on this branch; use `git log -1` for its SHA.
