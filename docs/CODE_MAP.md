# Zombtoy Code Map — every C# script, what it does, and whether it actually runs

Last verified: **2026-07-12**, branch `feature/Titan-Zombunny`. Wiring status comes from GUID tracing
across every `.unity` scene and `.prefab` (not from filenames or comments). For the deep audit behind
these labels see [`docs/reexploration/CURRENT_STATE.md`](reexploration/CURRENT_STATE.md).

**Status legend**
- 🟢 **Active** — wired into a shipped scene/prefab, or a static/runtime path that provably executes
- 🟡 **Transitional** — partially wired, mid-migration, or toggled per-scene
- 🔴 **Dormant** — compiles but zero scene/prefab/code references reach it at runtime
- ⚪ **Legacy support** — old-style code that is still what actually runs the game
- 🧪 **Debug/experimental** — scratch or diagnostic code

The big picture: the Aug-2025 "core architecture refactor" produced a new event/manager layer, but only
part of it was wired in. **Gameplay still runs on the legacy player/weapon stack**, which publishes into
the new `GameEvents` hub. Full migration is GitHub issue #16.

---

## Assets/Scripts/Core — new architecture layer

| File | Status | What it is |
|---|---|---|
| `GameEvents.cs` (155 l) | 🟢 | Static event hub (~20 events: health, score, enemy lifecycle, game state). The one piece of the refactor that is fully live — legacy and new code both publish/subscribe through it. Includes `SafeInvoke` + subscriber-count debug helpers. |
| `Singleton.cs` (107 l) | 🟢 | Generic `Singleton<T>` base for managers. **Deliberately does not auto-create instances** — a manager that isn't placed in a scene stays dormant. This single design choice explains most of the "written but never ran" refactor code. |
| `ComponentCache.cs` (140 l) | 🔴 | Per-GameObject component caching utility. Only referenced by the dormant refactored scripts (WeaponSystem/WeaponManager/Player*Refactored). |
| `GameStateManager.cs` (194 l) | 🔴 | Centralized game-state machine (`Playing/Paused/GameOver`). Never placed in any scene → `Instance` is null at runtime. Referenced by GameStarter/ScoreManager/GameOverManager/WeaponManager, all of which either null-check it or are dormant themselves. |
| `GameStarter.cs` (46 l) | 🔴 | Scene bootstrap: resets ScoreManager + sets state to Playing. Header says "Place this on any GameObject in gameplay scenes" — it never was. |

## Assets/Scripts/Player

| File | Status | What it is |
|---|---|---|
| `PlayerMovement.cs` (60 l) | 🟢⚪ | **The movement/rotation owner in Level1.** Survival-Shooter-style: WASD via rigidbody + mouse-ray turning against the Floor layer (`Turning()` — the raycast at the heart of the 2025 rotation-freeze bug, fixed 2026-07-12). |
| `PlayerHealth.cs` (234 l) | 🟢⚪ | **The live health system** (Level1/2/3): health + stamina + sprint + death, publishes `GameEvents.PlayerHealthChanged/PlayerDeath`. Still the god-object the refactor plan complained about, but it's what runs. |
| `PlayerShooting.cs` (130 l) | 🟢⚪ | Raycast gun logic. Lives on the gun prefabs (`Assets/Guns/Machine Gun`, `MultiShot`, `Shotgun 1`) and `Assets/Prefabs/Player.prefab` — not scene-level, so a scene GUID grep misses it. |
| `PlayerMovementRefactored.cs` (346 l) | 🔴 | Modular movement replacement (walk/sprint/stamina via events). **Attached to the Level1 Player but disabled** (`m_Enabled: 0`). If you re-enable it, re-test rotation: it must not fight `PlayerMovement`. |
| `PlayerHealthRefactored.cs` (491 l) | 🔴 | Component-split health system (the refactor plan's centerpiece). Zero scene/prefab references; only reached by fallback probes in HealthPotion/AmmoItem/EnemyManager. |
| `PlayerHealthProxy.cs` (45 l) | 🔴 | Adapter exposing the legacy `PlayerHealth` API backed by `PlayerHealthRefactored`. Never attached anywhere; probed via `Type.GetType` reflection. |
| `PlayerInputManager.cs` (225 l) | 🔴 | Centralized input abstraction ("multiplayer-ready"). Zero references of any kind. |

## Assets/Scripts (root) — legacy gameplay + camera + items

| File | Status | What it is |
|---|---|---|
| `Inventory.cs` (135 l) | 🟢⚪ | Weapon switching on the Player. **Refactored in place** (Sep 2025, issue #1): data-driven `WeaponEntry` list with legacy-field fallback. The better version (`AmmoSystem.cs`, `IProjectile`) lives only on PR #25 / the inventory branch. |
| `Ammo.cs` (88 l) | 🟢⚪ | Per-weapon ammo counter + UI (9 instances across Level1). PR #25 centralizes this. |
| `AmmoItem.cs` (104 l) | 🟢 | Ammo pickup; notifies `ItemManager`, probes both health systems. |
| `HealthPotion.cs` (90 l) | 🟢 | Heal pickup; same dual-health-system probing pattern. |
| `Pistol.cs` (29 l), `RocketLauncher.cs` (41 l), `TornadoLaunch.cs` (53 l), `IceBullet.cs` (131 l), `flashlight.cs` (36 l), `reloadCheck.cs` (17 l) | 🟢⚪ | The legacy per-weapon scripts the `Weapons/` framework was meant to replace. Wired in Level1 + gun prefabs; still the real weapon system. |
| `Rocket.cs` (226 l) | 🟢⚪ | Player rocket projectile (implements `IBlast`): spherecast collision, inner/outer blast radii, `blast_immunity` handling. Watch-out: speed is applied twice in `FixedUpdate` (quadratic in the inspector value) — same quirk in `EnemyProjectile`. |
| `Tornado.cs` (207 l) | 🟢⚪ | Tornado spell projectile (pull + damage). |
| `EnemyProjectile.cs` (134 l) | 🟢 | Boss/clown projectile (`EnemyProjectile.prefab`, `EnemyRocket Variant.prefab`). Straight-line flight, no homing — it never touches player rotation. |
| `EnemyShooting.cs` (114 l) | 🟢 | Ranged attack used by `Clown.prefab` (audio + projectile spawn). |
| `EnemyTargetShooting.cs` (~92 l) | 🟢 | **Titan boss ground-target attack** (on `Titan Zombunny.prefab`): crosshair lerps to the player while in `range`, then fires `EnemyRocket Variant` at the marked point. Hardened 2026-07-12 (null-guards; refuses a collidable Floor-layer `groundTarget`). |
| `range.cs` (29 l) | 🟢 | Trigger-sphere "player in range" flag (used by boss/clown attack scripts). Lowercase class name is legacy style. |
| `EnemyRegen.cs` (28 l), `SelfDestruct.cs` (90 l), `MiniClown.cs` (16 l), `SpawnClown.cs` (61 l) | 🟢 | Enemy support behaviors (regen, exploding zomduck via `IBlast`, clown minions — SpawnClown registers spawns with `GameEvents`/`TransientEnemyRegistration`). |
| `CamerFollow.cs` (24 l), `CamerPOV.cs` (47 l) | 🟡 | Camera follow/POV scripts wired in Level1; part of the unfinished camera work (issue #22, "partial" per `ce956b05`). Note the typo'd names. |
| `CameraMovement.cs` (75 l), `FirstPersonMovement.cs` (52 l) | 🟡 | First-person/alternate-view experiments tied to the same issue; wiring varies by scene. |
| `Pause.cs` (74 l) | 🟢⚪ | Pause menu (contains a classic `GameObject.Find`). |
| `Keybinds.cs` (65 l), `KeybindText.cs` (26 l), `Sensitivity.cs` (19 l) | 🟢 | Settings/keybinding UI helpers. |
| `SFXManager.cs` (28 l) | 🟢 | Simple SFX playback helper. |
| `HighScores.cs` (67 l) | 🟡⚪ | **Old** leaderboard client (dreamlo.com third-party service). Still wired in `Menu 3.unity` *alongside* the new `Leaderboard.cs` — one of them should eventually be retired. |
| `zombieCount.cs` (18 l) | 🟢⚪ | Legacy zombie-count UI text (new equivalent: `UI/ZombieCountBinder`). |
| `vectorTest.cs` (31 l) | 🧪 | Scratch experiment, but live in Level1 — harmless; delete when convenient. |

## Assets/Scripts/Managers

| File | Status | What it is |
|---|---|---|
| `ScoreManager.cs` (352 l) | 🟢 | `Singleton<ScoreManager>`, persistent, event-driven score + high-score persistence. Scene-placed in Level1/2/3 → genuinely live. |
| `EnemyManager.cs` (622 l) | 🟢🟡 | `Singleton<EnemyManager>`, weighted-table enemy spawner and **the only spawner in Level1**. Was toggled off in the committed scene during boss testing; re-enabled 2026-07-12. The Titan is *not* in its spawn table (scene-placed instead). |
| `ItemManager.cs` (61 l) | 🟢 | Item spawn management (Level1 ×2). |
| `GameOverManager.cs` (96 l) | 🔴 | Event-driven game-over UI handler. Depends on the dormant `GameStateManager`; zero scene references. The game-over flow players actually see runs through legacy paths. |

## Assets/Scripts/Enemy

| File | Status | What it is |
|---|---|---|
| `EnemyHealth.cs` (279 l) | 🟢 | On every enemy prefab. Health/death/sinking + score, attribute system (`blast_immunity`), publishes `GameEvents.EnemyKilled/Damaged/Destroyed`. The main legacy↔new bridge on the enemy side. |
| `EnemyMovement.cs` (54 l) | 🟢⚪ | NavMesh chase-the-player (with `GameObject.Find`). |
| `EnemyAttack.cs` (91 l) | 🟢⚪ | Contact damage on the player. |

## Assets/Scripts/Weapons — dormant refactor framework

| File | Status | What it is |
|---|---|---|
| `WeaponSystem.cs` (261 l) | 🔴 | `IWeapon` + `WeaponData` ScriptableObject + `BaseWeapon` base class. Zero references. |
| `WeaponManager.cs` (317 l) | 🔴 | Centralized weapon switching/inventory with network-state serialization. Zero references. |
| `RaycastWeapon.cs` (110 l), `ProjectileWeapon.cs` (108 l) | 🔴 | Hitscan/projectile `BaseWeapon` implementations. Zero references. |
| `Interfaces/IBlast.cs` | 🟢 | The one live interface — implemented by `Rocket` and `SelfDestruct`, consumed by `EnemyHealth.TakeDamage`. |
| `Interfaces/IFirearm.cs`, `IPlayerWeapon.cs`, `ISpell.cs` | 🔴 | Contracts with no wired implementors. (`IProjectile` exists only on the PR #25 branch.) |

## Assets/Scripts/UI

| File | Status | What it is |
|---|---|---|
| `MusicManager.cs` (450 l) | 🟢 | `Singleton<MusicManager>`, persistent, inspector-driven alternating background/lobby music (Sep 2025 rework). |
| `ScoreTextBinder.cs` (38 l), `ZombieCountBinder.cs` (31 l) | 🟢 | Event-driven UI binders (the "new way" — subscribe to `GameEvents`, update text). |
| `Result.cs` (78 l), `ResultStandalone.cs` (148 l), `ResultsButton.cs` (68 l) | 🟢 | Results/score screens; `ResultStandalone` exists specifically to work when the ScoreManager singleton isn't present. |
| `Scene.cs` (30 l), `Index3Scene.cs` (27 l) | 🟢⚪ | Scene-navigation button helpers. |
| `MuteButton.cs` (33 l) | 🟢 | Audio mute toggle. |

## Assets/Scripts/{Utility, Debug, Server}

| File | Status | What it is |
|---|---|---|
| `Utility/TransientEnemyRegistration.cs` (19 l) | 🟢 | Registers spawned objects without `EnemyHealth` into the enemy-count events (added by `SpawnClown` at runtime). |
| `Debug/ScoreDebugger.cs` (125 l), `Debug/ScoreManagerDebugger.cs` (90 l) | 🧪 | Score-persistence diagnostics from the refactor era. |
| `Server/Leaderboard.cs` (225 l) | 🟢 | **Current** backend client (HttpClient → .NET backend on `localhost:3000`; wired in `Menu 3.unity`). Also defines `RequestPacket`. |
| `Server/zombtoy-backend/` (Node/Express + committed `node_modules`) | 🔴⚪ | **Obsolete** original backend, superseded by `Backend/ZombtoyBackend` — but still tracked in git (~120 vendor files; source of the GitHub dependabot alerts). Deletion is planned milestone M4. |

## Outside Assets/ (for orientation)

| Path | Status | What it is |
|---|---|---|
| `Backend/ZombtoyBackend/` (.NET 8) | 🟢 | The real backend: ~100-line Minimal API + SQLite/EF Core, 3 endpoints. Its README is accurate. |
| `Backend/ZombtoyBackend-C/` | 🧪 | Educational C re-implementation (mongoose + SQLite). Self-contained; not analyzed in depth by owner instruction. Note: build artifacts (`mongoose.o`, `obj/`, binary, `zombtoy_c.db`) are tracked in git. |
| `DevTools/Diagrams/` (Python) | 🟢🧪 | Regex-based C# static analysis → PlantUML diagrams + event-health reports. Independent of the game; `out/` snapshots are from Oct 2025 (stale until regenerated). |
| `DevTools/shell_scripts/` | 🟢 | `open-unity.{sh,fish}`, `project-stats.sh`, `lint-code.sh`. |

---

## Reading this map when you add features

- **New gameplay code today** should follow the *live* patterns: publish/subscribe via `GameEvents`,
  scene-place any `Singleton<T>` manager you add, and expect the legacy player/weapon stack.
- **Don't extend the dormant framework** (`Weapons/`, `Player*Refactored`, `GameStateManager`) without
  first deciding issue #16 (wire it in properly) — otherwise you deepen the two-architecture split.
- **Trust wiring over names**: `*Refactored` ≠ running, and lowercase legacy names (`range`, `flashlight`) ≠ dead.
