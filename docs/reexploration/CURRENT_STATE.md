# Zombtoy — Current State (Re-Exploration, 2026-07-12)

Produced by an autonomous read-only audit of the local repository at branch `feature/Titan-Zombunny`.
Evidence labels: **[confirmed]** = directly verified against code/serialized assets/git; **[strongly inferred]** = multiple converging signals; **[uncertain]** = needs a decision or runtime test.

Scope exclusion: `Backend/ZombtoyBackend-C/` (mongoose-based C backend) was deliberately **not analyzed** per project-owner instruction. It is listed in inventories only.

---

## 1. Repository and branch state

- Branch: `feature/Titan-Zombunny` @ `db956dab`, in sync with `origin/feature/Titan-Zombunny`, **2 commits ahead of `master`** (`2667397e` rocket refactor, `db956dab` scene adjustments). Merge-base with master is master's tip `ce956b05` → the branch fast-forwards cleanly onto master. [confirmed]
- Uncommitted working-tree changes (4 files — this is the exact stopping point of development, early Nov 2025):
  - `Assets/Level1.unity` — moves Titan boss components from scene-added overrides into the prefab; re-enables `EnemyManager`; deactivates the scene-placed Titan; sets `EnemyTargetShooting.Player`/`groundTarget` overrides. [confirmed]
  - `Assets/Titan Zombunny.prefab` — receives `EnemyTargetShooting` + new `Range` and `ShootPoint` children; boss `startingHealth` 3000 → 2000. [confirmed]
  - `Assets/Scripts/EnemyTargetShooting.cs` — adds only a bug-note comment ("problem: properly defining the floor / player rotation uncharacteristically modified"). [confirmed]
  - `Assets/Scripts/Rocket.cs` — adds `using UnityEditor;` (line 3), unused. **Breaks player builds** (UnityEditor assembly does not exist outside the editor). [confirmed]
- Stashes: `stash@{0}` and `stash@{1}` belong to the inventory/weapon branch (interface tweaks; Ammo/Inventory experiments self-labeled "likely dump"); `stash@{2}` is +23 lines to `workflow.md` from the core-arch branch. None applied. [confirmed]
- Tag: `v1.0-first`. Remote: `github.com/ieatyoursushi/Zombtoy.git`.
- Unity `2022.3.37f1` (ProjectSettings/ProjectVersion.txt — matches README badge). [confirmed]

## 2. Development timeline

| When | What | Evidence |
|---|---|---|
| ≤ Jan 2025 | Original game (Survival-Shooter-derived), dreamlo highscores, Node backend | issue #1 (Jan 2025), `Assets/Scripts/Server/zombtoy-backend/`, `HighScores.cs` |
| Aug 2025 | **Core architecture refactor** merged via PR #2; Copilot README via PR #23; issue burst #3–#22 | `git log`, PR list |
| Aug–Sep 2025 | Backend: Node → .NET Minimal API + SQLite (`d40b4eb2`); C backend boilerplate (`754b5450`); shell scripts | commit messages |
| Sep 2025 | **Inventory/weapon refactor** branch + PR #25; `Inventory.cs` refactored in place (`48027f5d` "mostly addressed issue #1"); `AmmoSystem.cs` on that branch; local WIP commit `39e7efe6` (Sep 22) | branch log |
| Oct 14 2025 | Camera WIP on master (`ce956b05`, "partial #22"); DevTools diagram outputs regenerated (all `out/` files dated Oct 14 2025) | git log, `ls -la DevTools/Diagrams/out/` |
| Nov 3–4 2025 | **Titan Zombunny boss work**: rocket logic refactor + `EnemyRocket Variant` (`2667397e`), scene adjustments (`db956dab`); PR #28 opened | git log, PR #28 |
| Nov 2025 | Development stops mid-task: prefab migration left uncommitted, rotation-freeze bug noted in a comment | working tree |
| 2026 | GitHub issues only (#26 Apr, #29/#30 May) — ideas, no code | issue dates |

**Where development stopped [confirmed]:** mid-way through moving the Titan's attack components into the prefab, with the ground-target crosshair mis-bound to the level floor (see §5) and the boss temporarily deactivated in Level1 for testing.

## 3. Domain architecture map

Scenes in build (ProjectSettings/EditorBuildSettings.asset): `Menu`, `Level1`, `Menu 1`, `Menu 2`, `Menu 3`, `Level3`, `Menu 4`. **`Level2.unity` exists but is not in the build.** [confirmed]

### 3.1 Core / event architecture
| System | Status | Evidence |
|---|---|---|
| `Core/GameEvents.cs` (static event hub, 20 events) | **Active** | No scene wiring needed (static). Used at runtime by wired scripts: `PlayerHealth`, `ScoreManager`, `EnemyManager`, `EnemyHealth`, `MusicManager`, `ScoreTextBinder`, `ZombieCountBinder`, `SpawnClown` [confirmed] |
| `Core/Singleton.cs` | **Active** (base of ScoreManager/EnemyManager/ItemManager) | Deliberately does **not** auto-create instances — comment "Don't auto-create - let the manager be placed explicitly in scene" (~line 41). Anything not scene-placed stays dormant. [confirmed] |
| `Core/ComponentCache.cs` | **Legacy-adjacent utility, referenced only by dormant scripts** (WeaponSystem, WeaponManager, PlayerMovementRefactored, PlayerHealthRefactored) | grep of code references [confirmed] |
| `Core/GameStateManager.cs` | **Dormant** — 0 scene/prefab refs; `Instance` returns null at runtime | GUID `6d91d52d…` absent from all `.unity`/`.prefab` [confirmed] |
| `Core/GameStarter.cs` | **Dormant** — designed to be scene-placed ("Place this on any GameObject in gameplay scenes"), never placed | GUID `34e1e0c4…` absent from scenes [confirmed] |

### 3.2 Player
| System | Status | Evidence |
|---|---|---|
| `Player/PlayerMovement.cs` (legacy; move + mouse-ray turning + anim) | **Active** — on Level1 Player (fileID 1117420106), enabled | scene GUID census [confirmed] |
| `Player/PlayerHealth.cs` (legacy; includes stamina; fires GameEvents) | **Active** — Level1/2/3 | GUID `544f2064…` in 3 level scenes [confirmed] |
| `Inventory.cs` (weapon switching) | **Active & already refactored in place** — data-driven `WeaponEntry` list with legacy-field fallback (outcome of issue #1 work) | on Level1 Player, enabled [confirmed] |
| `Player/PlayerShooting.cs` | **Active via prefabs** — lives in `Assets/Guns/{Machine Gun, MultiShot, Shotgun 1}.prefab` and `Assets/Prefabs/Player.prefab` | GUID `27c63be2…` in those prefabs [confirmed] |
| `Player/PlayerMovementRefactored.cs` | **Attached but disabled** on Level1 Player (`m_Enabled: 0`) | Level1.unity fileID 1559539636 [confirmed] |
| `Player/PlayerHealthRefactored.cs` (491 lines) | **Dormant** — 0 scene/prefab refs; only code refs are fallback probes | [confirmed] |
| `Player/PlayerHealthProxy.cs` (adapter subclassing PlayerHealth) | **Dormant** — never attached; probed via reflection by `EnemyManager` (line ~141 `Type.GetType("PlayerHealthProxy")`), `HealthPotion`, `AmmoItem` | [confirmed] |
| `Player/PlayerInputManager.cs` | **Dormant** — 0 refs anywhere | [confirmed] |
| `Assets/Prefabs/Player.prefab` | **Apparently unused** — referenced by zero scenes; contains legacy trio (PlayerShooting/PlayerHealth/PlayerMovement) | GUID `e3943703…` absent from scenes [confirmed] |
| `FirstPersonMovement.cs`, `CamerFollow.cs`, `CamerPOV.cs`, `CameraMovement.cs` | **Transitional/experimental** — camera work tied to open issue #22 ("partial" per `ce956b05`); CamerFollow + CamerPOV wired in Level1 | scene census [confirmed wiring; intent uncertain] |

### 3.3 Weapons / ammo
| System | Status | Evidence |
|---|---|---|
| Legacy per-weapon scripts (`Pistol.cs`, `RocketLauncher.cs`, `TornadoLaunch.cs`, `IceBullet.cs`, `Rocket.cs`, `Ammo.cs` ×9 instances, `reloadCheck.cs` ×9 instances) | **Active** — wired in Level1 scene and gun prefabs | scene census [confirmed] |
| `Weapons/WeaponManager.cs` (317 l), `WeaponSystem.cs` (261 l), `ProjectileWeapon.cs`, `RaycastWeapon.cs` | **Dormant** — 0 scene/prefab refs, 0 inbound code refs | GUID + symbol greps [confirmed] |
| `Weapons/Interfaces/` — `IBlast` | **Active** (implemented by `Rocket.cs`) | [confirmed] |
| `Weapons/Interfaces/` — `IFirearm`, `IPlayerWeapon`, `ISpell` | **Dormant** | no implementors wired [confirmed] |
| `IProjectile.cs` | **Exists only on the inventory branch** (local commit `39e7efe6`), not on this branch | tip-to-tip diff [confirmed] |
| `AmmoSystem.cs` (centralized ammo, 123 l) | **Exists only on inventory branch/PR #25** | [confirmed] |

### 3.4 Enemies / boss
| System | Status | Evidence |
|---|---|---|
| `Enemy/EnemyHealth.cs`, `EnemyMovement.cs`, `EnemyAttack.cs` | **Active** — on enemy prefabs; EnemyHealth raises GameEvents | prefab contents, code refs [confirmed] |
| `Managers/EnemyManager.cs` (622 l, weighted spawn table, Singleton) | **Active-but-toggled**: on `EnemyManagerInstance` in Level1 (disabled in committed scene, **re-enabled in uncommitted working tree**) and Level2. It is the **only spawner in Level1** — the committed scene spawned nothing. Titan is **not** in its spawn table. | Level1 fileID 1392213495; census [confirmed] |
| `EnemyTargetShooting.cs` (boss ground-target attack) | **Transitional / broken wiring** — see §5 | [confirmed] |
| `EnemyShooting.cs` | **Active** on `Clown.prefab` | GUID in prefab [confirmed] |
| `EnemyProjectile.cs` | **Active** on `EnemyProjectile.prefab` + `EnemyRocket Variant.prefab` (boss projectile). Straight-line flight; no homing; never touches player rotation. | file read [confirmed] |
| `Utility/TransientEnemyRegistration.cs` | **Active at runtime** (added by `SpawnClown.cs` code path) | code ref [confirmed] |
| `SpawnClown.cs`, `MiniClown.cs`, `EnemyRegen.cs`, `SelfDestruct.cs`, `Tornado.cs` | **Active** legacy support scripts (prefab-side) | [strongly inferred from prefab/code refs] |

### 3.5 Game state / score / UI
| System | Status | Evidence |
|---|---|---|
| `Managers/ScoreManager.cs` (Singleton, persistent) | **Active** — Level1/2/3 | [confirmed] |
| `Managers/ItemManager.cs` | **Active** — Level1 ×2 | [confirmed] |
| `Managers/GameOverManager.cs` | **Dormant** — 0 refs; depends on dormant GameStateManager | [confirmed] |
| UI: `MusicManager`, `Pause`, `zombieCount`, `ScoreTextBinder`, `ZombieCountBinder`, `Result*`, `Scene.cs`, `Index3Scene.cs`, `Sensitivity`, `MuteButton`, `KeybindText`/`Keybinds` | **Active** (mixture, wired across Level/Menu scenes) | census [confirmed for Level1 items; menus strongly inferred] |
| `vectorTest.cs` | **Experimental/debug leftover, live in Level1** | census [confirmed] |
| `Debug/ScoreDebugger.cs`, `ScoreManagerDebugger.cs` | Debug tooling | [confirmed by location] |

### 3.6 Backend & Unity↔backend
| System | Status | Evidence |
|---|---|---|
| `Backend/ZombtoyBackend` (.NET 8 Minimal API + SQLite/EF Core) | **Active (minimal)** — endpoints `GET /`, `POST /addScore`, `GET /getAllScores` in `Program.cs`; matches its README exactly | file read [confirmed] |
| `Assets/Scripts/Server/Leaderboard.cs` (HttpClient client for the .NET backend) | **Active** — wired in `Menu 3.unity` | GUID in scene [confirmed] |
| `HighScores.cs` (dreamlo.com third-party leaderboard) | **Legacy but still wired** — also in `Menu 3.unity`, alongside Leaderboard.cs | [confirmed] |
| `Assets/Scripts/Server/zombtoy-backend/` (Node/Express + `node_modules`) | **Obsolete but still tracked in git** despite `d40b4eb2` claiming "mark legacy Node backend for removal" | `git ls-files` [confirmed] |
| `Backend/ZombtoyBackend-C/` | **Excluded from analysis** (owner instruction). Note: compiled object `mongoose.o` is tracked. | `git ls-files` |

### 3.7 Tooling
| System | Status | Evidence |
|---|---|---|
| `DevTools/Diagrams/*.py` (regex-based C# static analysis → PlantUML/reports) | **Runnable, likely still parses current code** (generic regexes in `common.py`); caveat: `EVENT_RAISE_RE` only matches `GameEvents.X?.Invoke` style and may undercount the trigger-method call style actually used | file read [confirmed pattern; undercount = strongly inferred, verify by regenerating] |
| `DevTools/Diagrams/out/` | **Stale** — all artifacts dated Oct 14 2025, predating the entire Titan/rocket work | `ls -la` [confirmed] |
| `DevTools/shell_scripts/` (`project-stats.sh`, `open-unity.sh/.fish`, `lint-code.sh`) | Utility, low-risk | [confirmed exists] |

## 4. The transitional architecture in one paragraph

The August 2025 "core architecture refactor" (PR #2) landed as **code**, but only part of it was ever **wired**: the static `GameEvents` hub plus `ScoreManager`/`EnemyManager`/`ItemManager` are genuinely live, while the rest of the new layer (GameStateManager, GameStarter, GameOverManager, the whole WeaponManager/WeaponSystem framework, PlayerInputManager, PlayerHealthRefactored/Proxy, PlayerMovementRefactored) is compiled-but-dormant because `Singleton<T>` refuses to auto-create and no scene or prefab references those scripts. Gameplay still runs on the legacy player/weapon stack — which was itself improved in place (`Inventory.cs` is now data-driven). 55 `GameObject.Find` calls remain across `Assets/Scripts` [confirmed], contradicting the refactor plan's completion claims. Open issue #16 ("Full migration to the new core and manager-based scripts") is the accurate statement of status.

## 5. Known blockers (all in the uncommitted Titan work)

1. **Player-rotation freeze — root cause [confirmed]:**
   `PlayerMovement.Turning()` (`Assets/Scripts/Player/PlayerMovement.cs:34-46`) rotates the player only when a mouse ray from `Camera.main` hits layer mask `"Floor"` (layer 8 per `TagManager.asset`). The uncommitted Level1 change assigns the Titan's `EnemyTargetShooting.groundTarget` to scene fileID **933169021 = the "Floor" GameObject itself** (layer 8, MeshCollider). `EnemyTargetShooting.Start()` then calls `groundTarget.SetActive(false)` — deactivating the entire floor, so the turning raycast misses and rotation silently locks (matches the dev note "player rotation locked when outside boss shoot range"). When the player is in range, `Update()` lerps `groundTarget.transform.position` toward the player — i.e., it drags the whole floor. The script's header comments show the intent was a small crosshair visual on the ground; the binding is simply wrong. `EnemyProjectile.cs` has no homing and never touches the player transform — the earlier "boss homing vs player rotation" theory is disproven.
   **Fix direction:** dedicated crosshair GameObject (decal/quad, not on the Floor layer, no collider that intercepts the turning ray), assigned as `groundTarget`.
2. **Build breaker:** `using UnityEditor;` in `Rocket.cs:3` (unused). Compiles in-editor, fails in player builds. [confirmed]
3. **Prefab robustness:** in `Titan Zombunny.prefab`, `EnemyTargetShooting.Player` and `groundTarget` are `{fileID: 0}`; only the Level1 instance overrides them. Spawning the prefab any other way NREs in `Start()`/`Update()`. [confirmed]
4. **Scene-state ambiguity [uncertain — owner decision]:** working tree re-enables `EnemyManagerInstance` (restoring the only spawner) and deactivates the scene-placed Titan. Whether the final intent is (a) boss scene-placed + spawner on, (b) boss added to EnemyManager's spawn table, or (c) boss-only test scene, must be decided before committing the scene.
5. Minor: both `Rocket` and `EnemyProjectile` apply `speed` twice in `FixedUpdate` (`movement.Set(0,0,speed)` then `* speed * Time.deltaTime`) — effective speed is quadratic in the inspector value. Works today by tuning, but will surprise any balancing pass. [confirmed]

## 6. Major uncertainties

- Intended Titan spawn/activation strategy (see 5.4).
- Fate of `Level2.unity` (not in build; still wires EnemyManager/ScoreManager) and of `Level3` vs the single-scene plan (issue #19).
- Whether the dormant refactor layer should be wired in (issue #16) or partially culled — per-system decision, biggest architectural fork in the road.
- Which leaderboard client is canonical (`HighScores`/dreamlo vs `Leaderboard`/.NET) — both live in `Menu 3.unity`.
- Whether `PlayerMovementRefactored` (disabled on Player) is intended to replace `PlayerMovement` soon — if so the rotation-freeze fix should be checked against both implementations.
