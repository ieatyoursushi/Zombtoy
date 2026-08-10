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
- [ ] 🤖 Update CODE_MAP/checkpoint status rows *(do with M2 commit)*

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
- [ ] 🧑 **OWNER GATE 3 — play-test weapons** (checklist §C) ← **PAUSED HERE**
- [ ] 🤖 Admin-merge PR #25, delete branch + `backup/inventory-pre-rebase`
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
- [ ] 🤖 Update CODE_MAP rows (`Ammo.cs` gains the shoot API; `IProjectile` now earned/live)

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

## §C — Owner Gate 3 play-test checklist (weapons — do this next)

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

**Report:** pass, or which weapon + what went wrong. Merging PR #25 is blocked on this.

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

**2026-07-12 — M0 ✅, M1 ✅, M2 in progress; paused at OWNER GATE 2b (AmmoSystem disposition).**

Done this session:
- M0 complete; **M1 complete** — boss play-test passed, PR #28 admin-merged (`2fb46f2a`), Titan branch deleted.
- `master` is now the trunk and holds the boss fix + the full docs corpus.
- M2: PR #25 retargeted to `master`; branch rebuilt as `master` + 2 clean cherry-picks; compile verified
  (exit 0); Titan work confirmed un-regressed. Original branch preserved at `backup/inventory-pre-rebase`
  (local) and on `origin` at the pre-rewrite SHA until the force-push happens.

**Current branch state:** `feature/player-inventory-&-weapon-system-refactor` = `master` + 3 commits
(`4eb3bc5d` ammo API, `e95508ea` IProjectile, `ab3db9c9` drop unwired AmmoSystem). Force-pushed; PR #25
updated and ready. Compile verified clean (exit 0). Owner decisions taken: drop AmmoSystem, leave stashes.

**Next agent actions once the owner reports gate 3 (§C weapons play-test):**
1. `gh pr merge 25 --merge --admin`; `git checkout master && git pull`.
2. Delete `feature/player-inventory-&-weapon-system-refactor` (local+remote) and `backup/inventory-pre-rebase`.
3. Update `docs/CODE_MAP.md`: `Ammo.cs` gains the shoot API; `IProjectile` is live/earned (Rocket + IceBullet).
4. **Then M3 (The Cull)** — start with the pre-flight greps; note `IProjectile` now **stays** (it has live
   implementors), while `IFirearm`/`ISpell`/`IPlayerWeapon` remain on the delete list.
5. All 3 stashes still parked — revisit in M4 per owner decision.
