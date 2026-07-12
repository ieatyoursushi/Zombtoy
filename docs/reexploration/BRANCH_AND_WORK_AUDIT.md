# Zombtoy — Branch & Work Audit (2026-07-12)

## 1. Branch / PR map

| Branch | Tip | PR | Status | Verdict |
|---|---|---|---|---|
| `feature/Titan-Zombunny` (HEAD) | `db956dab` (= origin) | **#28 OPEN** ("everything feature-wise for the Titan-Zombunny boss entity", Nov 4 2025) | 2 ahead of master; merge-base = master tip `ce956b05` → **fast-forwardable**; plus 4 uncommitted files | **The continuation point.** Finish M1, commit, merge #28 |
| `master` | `ce956b05` ("WIP: small camera-view changes (partial #22)") | — | default branch; note its tip is itself labeled WIP | Base for everything |
| `feature/core-architecture-refactor` | `fd0906bb` (local = origin) | **#2 MERGED** (Aug 10 2025) | `git cherry master`: **25 of 26 commits patch-identical in master**; sole exception `dca3ac3c` "Initial plan" (Copilot placeholder, no needed content) | **Fully integrated. Archive/delete branch** (safe). The 28/28 ahead-behind count is duplicate SHAs from cherry-pick/re-commit, not divergent work [confirmed] |
| `feature/player-inventory-&-weapon-system-refactor` | local `39e7efe6` = origin `f80386b0` **+1 local-only commit** | **#25 OPEN** (Sep 6 2025) | merge-base `d40b4eb2`; most of its history is duplicated on master via cherry-picks; **real unmerged work exists** (below) | **Keep. Reconcile in M2.** Local commit `39e7efe6` exists nowhere on the remote — do not delete the local branch before pushing or extracting it |
| `origin/highscore_backend` | `2d328b87` ("Revert 'Entire project commit - 1st.'") | — | pre-refactor era | Historical; ignore (delete remotely whenever) |
| `origin/copilot/fix-ada1382c…` | `9f30a343` | **#23 MERGED** | README/images work, merged | Historical; ignore |

Tag: `v1.0-first`.

## 2. Work that exists ONLY on the inventory/weapon branch (unmerged into this line)

Tip-to-tip diff vs HEAD, `Assets/Scripts` only [confirmed]:

- **`AmmoSystem.cs` (new, 123 lines)** — centralized ammo operations (commit `f80386b0` "centralized ammo script operations easier to manage / add more weapons from the ammo perspective"). The core deliverable of PR #25 not yet on master.
- `Ammo.cs` (+27), `Pistol.cs`, `RocketLauncher.cs`, `Player/PlayerShooting.cs` — small adaptations toward AmmoSystem.
- **Local-only commit `39e7efe6` "WIP: weapon modularity and org" (Sep 22 2025)** — adds `Weapons/Interfaces/IProjectile.cs`, reworks `IceBullet.cs`/`Rocket.cs` toward interfaces, touches `IFirearm`, `ProjectileWeapon`.
- **`stash@{0}`** ("temp stashed latest changes for this branch") — IceBullet/Rocket/IBlast/IProjectile/ProjectileWeapon touch-ups (8+/10−). Continuation of the same WIP.
- **`stash@{1}`** ("ammo experimental changes (likely dump)") — `Ammo.cs` +76, `Inventory.cs` +115. Self-labeled as a probable dump; review once, expect to drop.

Also parked: **`stash@{2}`** (on core-arch branch) — `workflow.md` +23 lines only.

## 3. Divergences that will bite a naive merge of PR #25

- `EnemyProjectile.cs` was **heavily rewritten on the Titan branch** (boss rocket variant, explosion handling); the inventory branch still has the old version → guaranteed conflict, resolve in favor of the Titan version, then re-apply any interface changes.
- `CamerFollow.cs`/`CamerPOV.cs` differ (master's Oct 14 camera WIP not on inventory branch).
- `EnemyTargetShooting.cs` exists only on the Titan line (git may propose a spurious rename against `AmmoSystem.cs.meta` due to meta-file similarity — the GUIDs differ; keep both files, both GUIDs).
- Much of the inventory branch's history is patch-duplicated on master (same messages, different SHAs: "Addition: C-Backend", "shell script maintanence", MusicManager fixes, Inventory refactor, etc.) — a merge would be noisy but content-identical for those commits.

## 4. Likely cherry-pick history [strongly inferred]

Commit messages appearing with different SHAs on `master`, `feature/core-architecture-refactor`, and `feature/player-inventory-&-weapon-system-refactor` (e.g. `754b5450`/`ad08b984`/`fd0906bb`-era "Addition: C-Backend") indicate the owner's workflow was: work on a feature branch, cherry-pick or re-commit onto master, leave the branch standing. `DevTools/picking_cherries.md` documents exactly this practice. Consequence: ahead/behind counts overstate divergence; `git cherry` (patch-id) is the reliable measure, and it shows only the inventory-branch work above as genuinely unmerged.

## 5. Abandoned vs still-useful

| Work | Verdict |
|---|---|
| Core-arch branch | Integrated → archive |
| Inventory branch: AmmoSystem + interface WIP + stash@{0} | **Still useful** — it is the natural successor to the in-place `Inventory.cs` refactor and the only existing path toward retiring the 4 duplicated per-weapon scripts |
| stash@{1} ammo experiments | Probably abandoned (owner's own label); review then drop |
| stash@{2} workflow.md edits | Trivial; review then fold or drop |
| `highscore_backend`, `copilot/*` remotes | Historical |
| Tracked Node backend (`Assets/Scripts/Server/zombtoy-backend/` incl. `node_modules`) | Obsolete; `d40b4eb2` declared removal but never deleted the tree → delete in M4 (nothing in Unity references it; `Leaderboard.cs` targets the .NET endpoints) |
| `Assets/Prefabs/Player.prefab` | Stale snapshot (0 scene refs) — decide keep-as-template vs delete in M3 |

## 6. Safest integration strategy (dependency order)

1. **Finish and land the Titan branch first** (M1): fix the groundTarget binding + build breaker, commit the prefab migration, merge PR #28 into master (fast-forward + 1 fix commit). Rationale: it is the branch with uncommitted state, the smallest delta, and it contains the EnemyProjectile rewrite that PR #25 must be reconciled against.
2. **Then reconcile PR #25 on top of the new master** (M2): rebase or selectively cherry-pick `f80386b0` + `39e7efe6` (+ stash@{0} if still relevant), resolving `EnemyProjectile.cs`/camera conflicts in favor of master. Test weapon switching + ammo before merging. Push local `39e7efe6` to the remote branch first so nothing lives only on this machine.
3. Apply/drop stashes explicitly, then clear them; archive `feature/core-architecture-refactor`; delete the tracked Node backend.
4. Only after both PRs are closed, take on the dormant-layer wire-or-cut decision (issue #16) — doing it earlier multiplies conflict surface.

Following the owner's own `workflow.md`: close Unity before branch operations, prefer worktrees for testing PR #25 against master.
