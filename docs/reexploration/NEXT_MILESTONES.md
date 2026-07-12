# Zombtoy — Next Milestones (dependency-ordered, 2026-07-12)

Ordering rationale: stabilize the branch that is half-committed (lowest risk, unblocks everything), land the one PR with real unmerged gameplay work, and only then take the big architectural fork (wire-or-cut the dormant layer). Documentation and tooling ride behind the code decisions they describe.

---

## M1 — Stabilize & land the Titan Zombunny branch (immediate)

**Scope**
1. Create a dedicated ground-crosshair GameObject for the boss (small quad/decal/projector): **not** the Floor object, **not** on the Floor layer, no collider that the mouse-turning raycast can hit. Assign it to `EnemyTargetShooting.groundTarget` in `Level1.unity` (replacing the current binding to Floor, fileID 933169021).
2. Remove `using UnityEditor;` from `Assets/Scripts/Rocket.cs:3`.
3. Harden `EnemyTargetShooting.cs`: null-guard `groundTarget`/`Player` (or resolve `Player` via the existing `GameObject.Find("Player")` it already does, and log-and-disable when `groundTarget` is missing) so prefab spawns without scene overrides can't NRE every frame.
4. Decide and encode the scene state: Titan active vs deactivated, `EnemyManagerInstance` enabled (it is the **only** spawner in Level1 — leaving it disabled ships a scene with no enemies). Owner decision; both toggles currently exist only as uncommitted diffs.
5. Commit the prefab migration + fixes; verify; merge PR #28 (fast-forward onto master).

**Prerequisites:** none — everything is local.
**Completion criteria:** entering/leaving boss range never freezes player rotation; the crosshair (not the floor) tracks the player in range; boss fires `EnemyRocket Variant` on cooldown; a player build compiles (no UnityEditor reference); `git status` clean; PR #28 merged.
**Risks:** the crosshair object, if given a collider on the Floor layer, would re-introduce a subtler version of the same bug (turning would target the crosshair surface) — keep it collider-free; scene merge is trivial now but stalls again if new uncommitted work accumulates.
**Validation:** in-editor play: walk in/out of the Range trigger (radius 3 × 4.5 scale) repeatedly while moving the mouse; confirm rotation via `PlayerMovement.Turning()` never stalls; check console for zero NREs; `dotnet`-independent — no backend needed. Optionally do a quick standalone build to prove the UnityEditor fix.

## M2 — Reconcile the inventory/weapon refactor (PR #25)

**Scope:** push local `39e7efe6` to the remote branch; rebase the branch onto post-M1 master; resolve `EnemyProjectile.cs`, `CamerFollow.cs`, `CamerPOV.cs` in favor of master; land `AmmoSystem.cs` + `IProjectile` interface work; review stash@{0} (apply if still meaningful) and stash@{1} (expect to drop, owner labeled it "likely dump"); clear stashes afterwards.
**Prerequisites:** M1 merged (otherwise double-conflict on EnemyProjectile).
**Completion criteria:** PR #25 merged or explicitly closed-with-extraction; all 4 weapons switch and consume ammo correctly; stash list empty; no scene GUID breakage (weapon prefabs unchanged or re-wired deliberately).
**Risks:** meta-file rename mirage between `EnemyTargetShooting.cs.meta` and `AmmoSystem.cs.meta` (GUIDs differ — keep both); serialized Inspector fields on Ammo/reloadCheck instances (×9 each in Level1) may need re-checking if field names changed.
**Validation:** play-test each weapon slot (keybinds 1–4), reload flows, rocket + cryo + tornado firing; grep scenes for dangling GUIDs afterwards.

## M3 — Wire-or-cut the dormant architecture layer (issue #16)

**Scope:** per-system decision on the compiled-but-unwired layer:
- Likely **wire in** (cheap, immediate value): `GameStarter` + `GameStateManager` (scene-place them; gives real state transitions to Pause/GameOver), `GameOverManager` (replaces ad-hoc game-over paths).
- Likely **decide deliberately** (bigger): `WeaponManager`/`WeaponSystem`/`RaycastWeapon`/`ProjectileWeapon` vs the now-decent data-driven `Inventory.cs` + AmmoSystem path — pick ONE weapon architecture and delete the loser; keeping both indefinitely is the current confusion's root.
- Likely **cut or finish**: `PlayerHealthRefactored`/`PlayerHealthProxy` (491 dormant lines shadowing the live 55-Find legacy `PlayerHealth`), `PlayerMovementRefactored` (attached-disabled on the Level1 Player), `PlayerInputManager`, stale `Assets/Prefabs/Player.prefab`, `vectorTest.cs` in Level1.
**Prerequisites:** M1+M2 (stable master, weapon architecture decided in M2 informs the WeaponManager verdict).
**Completion criteria:** zero scripts in `Assets/Scripts` that are neither scene/prefab-wired nor code-reachable (each either wired, deleted, or explicitly marked experimental in CURRENT_STATE.md); issue #16 closed or re-scoped.
**Risks:** highest-leverage but highest-risk milestone; do it as several small PRs (state management, then health, then weapons), never as one mega-refactor. `EnemyManager`'s reflection probe for `PlayerHealthProxy` must be updated if the proxy is cut.
**Validation:** full playthrough per PR (menu → Level1 → death → game over → score persists to leaderboard); regenerate DevTools dependency diagrams and confirm no orphan nodes.

## M4 — Repository & documentation hygiene

**Scope:** delete tracked `Assets/Scripts/Server/zombtoy-backend/` (Node backend + node_modules; declared removed in `d40b4eb2` but still present); archive/delete `feature/core-architecture-refactor` (25/26 patch-integrated); prepend status preambles to `REFACTOR_PLAN.md` ("historical; see docs/reexploration/CURRENT_STATE.md") and `DOTNET_BACKEND_INTEGRATION_GUIDE.md` ("target design, not current state"); fix README structure diagram + WeaponSystem wording; decide the `HighScores.cs` (dreamlo) vs `Leaderboard.cs` (.NET) duplication in `Menu 3.unity`; decide `Level2.unity` (restore to build or delete); review stash@{2} workflow.md lines.
**Prerequisites:** M1 (so docs describe the merged state); M3 preferred for final wording but not required.
**Completion criteria:** fresh-clone repo contains no obsolete backend, no misleading "COMPLETED" headers, branch list = master + active work only.
**Risks:** low; deletions are all git-recoverable. Confirm nothing in Unity references the Node folder before deleting (verified: nothing does — `Leaderboard.cs` targets the .NET endpoints; only `placeholder.prefab` sits in `Scripts/Server/`, keep it or move it).
**Validation:** `git grep zombtoy-backend` returns nothing outside history; Unity opens with zero missing-script/asset warnings.

## M5 — Backend & tooling refresh

**Scope:** verify DevTools parser against current code (specifically whether `EVENT_RAISE_RE`'s `GameEvents.X?.Invoke` pattern captures the trigger-method style used by callers; extend regex if it undercounts); regenerate `DevTools/Diagrams/out/` (stale since Oct 14 2025); backend: keep the minimal .NET API as-is unless multiplayer work actually starts (the 1311-line guide is a someday-plan, not a debt); C backend remains out of scope per owner instruction (only note: tracked `mongoose.o` binary could be gitignored).
**Prerequisites:** M3 (diagrams should capture the post-migration architecture, not the hybrid).
**Completion criteria:** `python3 DevTools/Diagrams/generate_all.py` runs clean; regenerated reports match CURRENT_STATE.md's map; out/ artifacts dated current.
**Risks:** none material.
**Validation:** cross-check regenerated event-flow report against the GameEvents subscriber list in CURRENT_STATE.md §3.1.

## M6 — Optional / future (unordered backlog, from open issues)

- #19 single-scene game-state consolidation (natural successor to M3's GameStateManager wiring)
- #22 cinematic camera work (master tip is already "partial #22")
- #21 shop/armory, #10 weapon damage types, #8 mana/spells (depend on the M2/M3 weapon architecture decision)
- #12/#4 NavMesh/custom AI, #20 particle states, #15 detailed animations
- #13 integration-test harness (would de-risk every milestone above; consider pulling forward)
- #17 auth/multiplayer backend (unblocks the DOTNET guide's aspirational content)
- #18 asset licensing audit before any commercial use
- #30 procedural levels, #26/#29 idea threads
