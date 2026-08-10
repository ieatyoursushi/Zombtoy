# FABLE_CHECKPOINT — Re-Exploration Session Log (2026-07-12)

Session: Claude (Fable 5), autonomous read-only re-exploration of `feature/Titan-Zombunny`. All investigation completed in one window; the four sibling documents in this directory are finished and mutually consistent. **This checkpoint exists so a future session can continue without re-deriving anything.**

## Repository state at session end
- Branch `feature/Titan-Zombunny` @ `db956dab` (= origin), 2 ahead of master (`ce956b05` = merge-base).
- Uncommitted (untouched by this session): `Assets/Level1.unity`, `Assets/Scripts/EnemyTargetShooting.cs`, `Assets/Scripts/Rocket.cs`, `Assets/Titan Zombunny.prefab`.
- This session added ONLY `docs/reexploration/*` (5 new untracked files). No code, scene, prefab, config, branch, or stash was modified.

## Investigation completed (with the commands that produced the evidence)
1. **Git state**: `git status -b`, `log`, `branch -a -v`, `stash list`, `tag`, `remote -v`, merge-bases, `rev-list --left-right --count`, `git cherry master feature/core-architecture-refactor` (→ 25/26 integrated), tip-to-tip `git diff --stat` vs inventory branch, `git stash show --stat` ×3, `gh pr list --state all`, `gh issue list --state all`.
2. **Working-tree forensics**: full `git diff` of all 4 modified files; identified the prefab component migration, `m_IsActive: 0` on Titan, EnemyManager re-enable, health 3000→2000, and the `using UnityEditor` addition.
3. **Root-cause analysis of the rotation freeze**: read `EnemyTargetShooting.cs`, `PlayerMovement.cs`, `range.cs`, `Rocket.cs`, `EnemyProjectile.cs`; traced scene fileID `933169021` → GameObject "Floor" (layer 8 = "Floor" per `TagManager.asset`). Verdict in CURRENT_STATE.md §5.1 — **confirmed**, not homing-related.
4. **Wiring census**: resolved script GUIDs via `.meta` files; grepped every `.unity`/`.prefab` for each GUID; enumerated every `m_Script` GUID in `Level1.unity` and mapped to files; inspected Player object (fileID 1117420106) component-by-component including `m_Enabled` flags; inspected `EnemyManagerInstance` (fileID 1392213495) serialized spawn table (Titan NOT in it).
5. **Code-reference graph**: grepped symbol usage for every Core/Weapons/Player/Managers class; confirmed `Singleton<T>` does not auto-create; counted 55 remaining `GameObject.Find` calls.
6. **Backend**: read `Backend/ZombtoyBackend/Program.cs` (3 endpoints) + README (accurate); found dreamlo `HighScores.cs` AND `.NET Leaderboard.cs` both wired in `Menu 3.unity`; confirmed legacy Node backend (incl. node_modules) still tracked. C backend intentionally not analyzed (owner instruction).
7. **DevTools**: read `common.py` regexes, `generate_all.py`; `out/` all dated Oct 14 2025 (stale). Flagged `EVENT_RAISE_RE` possible undercount. Did NOT run the generators (kept session read-only).
8. **Docs**: audited all 15 non-vendor tracked `*.md` (see DOCUMENT_AUDIT.md).

## Key resolved questions
- Rotation freeze = groundTarget bound to the Floor object; `SetActive(false)` kills the floor raycast `PlayerMovement.Turning()` depends on. Freeze occurs when the player is OUTSIDE boss range (floor inactive), matching the dev comment, not "in range" as remembered.
- "Refactor completed" ≠ wired: only GameEvents/ScoreManager/EnemyManager/ItemManager live; the rest of the new layer is dormant (details CURRENT_STATE.md §3).
- `feature/core-architecture-refactor` fully integrated (archivable); real unmerged work = PR #25's AmmoSystem + local `39e7efe6` + stash@{0} (+ stash@{1} probably droppable).
- Committed Level1 had NO active enemy spawner; the uncommitted change re-enables the only one (EnemyManager).

## Open questions (owner decisions, not further archaeology)
1. Titan spawn strategy: scene-placed & active, added to EnemyManager's table, or test-only? (blocks M1 step 4)
2. EnemyManager permanently enabled in Level1? (presumably yes — otherwise no enemies)
3. Weapon architecture endgame: data-driven `Inventory.cs`+`AmmoSystem` vs dormant `WeaponManager` framework? (M2/M3 fork)
4. Canonical leaderboard client (dreamlo vs .NET) in Menu 3; fate of `Level2.unity` (not in build).

## UPDATE (same session, later): M1 core fix APPLIED
The owner authorized continuing, and the M1 rotation-freeze fix was implemented and validated:
- New `GroundTarget` child inside `Titan Zombunny.prefab` (flat quad, layer Default, **no collider**, fileIDs 7777777777777777001–004) using new asset `Assets/Materials/BossGroundTargetMaterial.mat` (Unlit/Color red, GUID e8d5ce8c5bfb42828643961bd345d157); prefab's `EnemyTargetShooting.groundTarget` now points at it.
- `Level1.unity`: removed the `groundTarget` → Floor (933169021) override; Titan re-activated (`m_IsActive` override 0→1). Player override kept.
- `EnemyTargetShooting.cs`: null-guards for `Player`/`groundTarget`; runtime check refusing a collidable Floor-layer groundTarget (encodes the lesson); `InvokeRepeating` moved after guards.
- `Rocket.cs`: `using UnityEditor;` removed → file back to committed state.
- Validation: Unity 2022.3.37f1 batchmode open (exit 0, zero CS errors, all changed assets imported, no broken refs). **In-editor play-test still pending (owner).**

## UPDATE 2 (2026-07-12, later session): commits pushed + documentation pass DONE

**Git:** Fix + audit docs committed (`b993e60a` fix, `e2fb5e1c` docs) and pushed to
`origin/feature/Titan-Zombunny`. No conflicts (branch is 2+ ahead of master, master not diverged).
Owner's in-editor play-test of the boss fight still pending but owner confirmed the crosshair renders.

**Documentation audit/rewrite (Part II) — complete.** All tracked markdowns read and cross-verified,
including the previously excluded `Backend/ZombtoyBackend-C/*.md` (markdown-only; C sources still excluded).
Actions recorded in DOCUMENT_AUDIT.md §"Actions taken". Summary: docs reorganized under `docs/`
(REFACTOR_PLAN → history/ with correction banner; DOTNET guide → backend/ with scope note; workflow.md moved
+ path fixes), README structure diagram + false claims fixed (object pooling, WeaponSystem, stamina "complete"),
DevTools README stale venv paths fixed, and two new docs: **`docs/CODE_MAP.md`** (per-file status of all 75
scripts — the C# contextualization doc) and **`docs/README.md`** (docs index).

## UPDATE 3 (2026-07-12, principal-engineer planning session): architecture decision document created

**New doc: [`docs/architecture/ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md`](../architecture/ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md)** — read it before any
architecture-touching work. Produced by a read-only first-principles review that independently re-verified the audit corpus
(all 7 dormant-script GUID checks re-traced: 0 refs each; pub/sub census re-derived; no WeaponData assets exist).

Key rulings (details + evidence in the plan):
- **No restart; no new architecture.** Target = the live architecture, completed and cleaned.
- **M1/M2 unchanged.** **M3 is redefined as "The Cull"**: delete the dormant layer (weapons framework, `Player*Refactored`+Proxy,
  PlayerInputManager, ComponentCache, **and the GameStateManager/GameStarter/GameOverManager trio**) instead of wiring it.
  New evidence: `ScoreManager.cs:168-186` shows the GameStateManager integration was already attempted and reverted after it
  broke scoring. Rebuild a small `GameFlow` only when issue #19 actually starts.
- Weapon fork decided: live `Inventory` + PR #25 `AmmoSystem` path wins; dormant framework goes in the Cull (after #25 lands).
- Governance adopted (plan §9): wired-in-same-PR, rule-of-three, feature-first budget, CODE_MAP updated per PR.
- Owner confirmations wanted (plan §13): Titan spawn strategy; dreamlo client deletion; Level2 fate; multiplayer horizon.

Session changed docs only: the plan doc, one index row in `docs/README.md`, this note. No code/scene/prefab/config changes; nothing committed.

## UPDATE 4 (2026-07-12, same-day follow-up): inspector-dependency audit added

**New doc: [`INSPECTOR_DEPENDENCY_AUDIT.md`](INSPECTOR_DEPENDENCY_AUDIT.md)** — quantifies how much behavior
lives in editor-serialized data vs code (owner-requested extension of the audit corpus; deep-dive behind
plan §7 coupling items #2/#3). Headlines: ~412 inspector knobs; 307 scene MonoBehaviours; ~500 scene-level
prefab overrides; zero ScriptableObjects in the live game; 50 `GameObject.Find` + 12 layer-name + 9 hardcoded
`LoadScene(int)` string/index couplings; **concrete drift found: Level2's EnemyManager spawn table is
serialized empty (Level1 has 9 entries) — Level2 spawns nothing.** Cull note: the disabled
`PlayerMovementRefactored` on Level1's Player must be removed from the scene, not just deleted as a file.
Session changed docs only (audit doc + index row + this note); nothing committed — owner to commit together
with the UPDATE 3 plan doc.

## UPDATE 5 (2026-07-12): plan execution STARTED — tracker is now the live status doc

**Day-to-day execution status has moved to [`docs/architecture/EXECUTION_LOG.md`](../architecture/EXECUTION_LOG.md)**
(checkbox milestones + owner gates + branch strategy). Read that first when resuming; this checkpoint stays
the archaeology/session narrative.

M0 done: docs committed; `39e7efe6` pushed to origin (no longer local-only); branch strategy decided
(trunk = master, short-lived features).

**Two corrections to the plan discovered while starting execution:**
1. **PR #25's base is `feature/core-architecture-refactor`, not master.** Plan §4/M4 says that branch is safe
   to delete — it is NOT until #25 is retargeted, because deleting a base branch auto-closes its PR.
   Corrected order is in the execution log's branch-strategy section.
2. PR #28's `BLOCKED` state is only `REVIEW_REQUIRED`; `master` has no branch protection, so the owner can
   merge it directly once the play-test passes.

**Execution paused at OWNER GATE 1** (Level1 boss play-test; checklist in the execution log §A).

## Exact next steps for the next session (in order)
0. **Read `docs/architecture/EXECUTION_LOG.md` first** — it holds the live checkboxes and the resume point.
1. Read `docs/README.md` + this file — do NOT re-explore the repo.
2. ~~Baseline comparison review~~ — **DONE same session**: `docs/reexploration/BASELINE_COMPARISON.md`
   (verdict: grade B, no restart; Find() calls grew 43→65 vs baseline; managers/GameEvents/Inventory = wins;
   dormant parallel layer + hygiene = defects).
3. Owner play-tests Level1 boss fight (rotation freeze gone, crosshair tracks, rockets fire), then merge PR #28.
4. Then M2 (PR #25 reconciliation) — push local `39e7efe6` first; it exists only on this machine.
5. Later: M4 hygiene (delete tracked Node backend `node_modules` — source of GitHub's 4 dependabot alerts —
   and C-backend build artifacts `mongoose.o`/`obj/`/binary/`zombtoy_c.db`), M5 DevTools parser check + regen.

## Files/symbols a continuation will touch first
- `Assets/Scripts/EnemyTargetShooting.cs` (Start/Update, groundTarget)
- `Assets/Scripts/Rocket.cs:3`
- `Assets/Level1.unity` — Titan PrefabInstance overrides (search `d066529bb94d95d4fbed707e8af180e7`), `groundTarget` override currently → fileID 933169021 (Floor); EnemyManager MonoBehaviour under GameObject fileID 1392213495
- `Assets/Titan Zombunny.prefab` — EnemyTargetShooting component fileID 9168717761059352862; Range child (SphereCollider r=3, trigger); ShootPoint fileID 621786824255763080
- `Assets/Scripts/Player/PlayerMovement.cs:34` `Turning()` — do not change it for the fix; it is behaving as designed
- Key GUIDs: EnemyTargetShooting `f14ce9881c3ae4c2e833ce4368fb0f38`, range.cs `21b9d38f78ab6814c8c262d16ce5540c`, EnemyManager `76a99222b1e4a384e923ac7e86efc709`, EnemyRocket Variant `49c93289c70934d538239dbced3c2fb3`, Titan prefab `d066529bb94d95d4fbed707e8af180e7`
