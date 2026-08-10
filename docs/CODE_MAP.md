# Zombtoy Code Map — every C# script, what it does, and whether it actually runs

Last verified: **2026-07-12**, branch `feature/Titan-Zombunny`. Wiring status comes from GUID tracing
across every `.unity` scene and `.prefab` (not from filenames or comments). For the deep audit behind
these labels see [`docs/reexploration/CURRENT_STATE.md`](reexploration/CURRENT_STATE.md).

**Status legend**
- 🟢 **Active** — wired into a shipped scene/prefab, or a static/runtime path that provably executes
- 🟡 **Transitional** — partially wired, mid-migration, or toggled per-scene
- ⚪ **Legacy support** — old-style code that is still what actually runs the game
- 🧪 **Debug/experimental** — scratch or diagnostic code

> **The Cull (M3) completed 2026-07-12.** The dormant parallel layer from the Aug-2025 refactor —
> 17 scripts, ~2,300 lines of C# plus a stale prefab — has been **deleted**, along with the reflection
> shims that live code carried for it. There is no 🔴 Dormant tier in this map any more: **every script
> listed here is wired and runs.** Deleted code lives in git history (see ADR-003); rebuild triggers are
> in the plan's §10. Script count: 73 → 56.

The big picture: gameplay runs on the legacy player/weapon stack, which publishes into the `GameEvents`
hub; four scene-placed singleton managers own global state. That *is* the architecture now — one live
path per concern, no parallel versions.

---

## Assets/Scripts/Core — new architecture layer

| File | Status | What it is |
|---|---|---|
| `GameEvents.cs` (155 l) | 🟢 | Static event hub (~20 events: health, score, enemy lifecycle, game state). The one piece of the refactor that is fully live — legacy and new code both publish/subscribe through it. Includes `SafeInvoke` + subscriber-count debug helpers. |
| `Singleton.cs` (107 l) | 🟢 | Generic `Singleton<T>` base for managers. **Deliberately does not auto-create instances** — a manager that isn't placed in a scene stays dormant. This single design choice explains most of the "written but never ran" refactor code. |

*Culled 2026-07-12: `ComponentCache.cs`, `GameStateManager.cs`, `GameStarter.cs`. A minimal scene-placed
`GameFlow` gets written when issue #19 (single-scene game flow) actually starts — see plan §10.*

## Assets/Scripts/Player

| File | Status | What it is |
|---|---|---|
| `PlayerMovement.cs` (60 l) | 🟢⚪ | **The movement/rotation owner in Level1.** Survival-Shooter-style: WASD via rigidbody + mouse-ray turning against the Floor layer (`Turning()` — the raycast at the heart of the 2025 rotation-freeze bug, fixed 2026-07-12). |
| `PlayerHealth.cs` (234 l) | 🟢⚪ | **The live health system** (Level1/2/3): health + stamina + sprint + death, publishes `GameEvents.PlayerHealthChanged/PlayerDeath`. Still the god-object the refactor plan complained about, but it's what runs. |
| `PlayerShooting.cs` (130 l) | 🟢⚪ | Raycast gun logic. Lives on the gun prefabs (`Assets/Guns/Machine Gun`, `MultiShot`, `Shotgun 1`) — not scene-level, so a scene GUID grep misses it. Now gates firing through `Ammo.TryShoot()`. |

*Culled 2026-07-12: `PlayerMovementRefactored.cs` (also removed as a disabled component from Level1),
`PlayerHealthRefactored.cs`, `PlayerHealthProxy.cs`, `PlayerInputManager.cs`. `PlayerHealth` is now the
sole health system — evolve it **in place** (extract `PlayerStamina`, move UI writes to a binder) when a
feature next touches it; never as a parallel rewrite (ADR-002).*

## Assets/Scripts (root) — legacy gameplay + camera + items

| File | Status | What it is |
|---|---|---|
| `Inventory.cs` (135 l) | 🟢⚪ | Weapon switching on the Player. **Refactored in place** (Sep 2025, issue #1): data-driven `WeaponEntry` list with legacy-field fallback. The better version (`AmmoSystem.cs`, `IProjectile`) lives only on PR #25 / the inventory branch. |
| `Ammo.cs` (~115 l) | 🟢 | Per-weapon ammo counter + UI (9 instances per level scene). **Owns the shoot rules** via `CanShoot()` / `TryShoot(int)` / `CanReload()` (PR #25) — weapon scripts call these instead of manipulating `ammo` directly. |
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

## Assets/Scripts/Managers

| File | Status | What it is |
|---|---|---|
| `ScoreManager.cs` (352 l) | 🟢 | `Singleton<ScoreManager>`, persistent, event-driven score + high-score persistence. Scene-placed in Level1/2/3 → genuinely live. |
| `EnemyManager.cs` (622 l) | 🟢🟡 | `Singleton<EnemyManager>`, weighted-table enemy spawner and **the only spawner in Level1**. Was toggled off in the committed scene during boss testing; re-enabled 2026-07-12. The Titan is *not* in its spawn table (scene-placed instead). |
| `ItemManager.cs` (61 l) | 🟢 | Item spawn management (Level1 ×2). |

*Culled 2026-07-12: `GameOverManager.cs` — the game-over flow players actually see runs through legacy paths.*

## Assets/Scripts/Enemy

| File | Status | What it is |
|---|---|---|
| `EnemyHealth.cs` (279 l) | 🟢 | On every enemy prefab. Health/death/sinking + score, attribute system (`blast_immunity`), publishes `GameEvents.EnemyKilled/Damaged/Destroyed`. The main legacy↔new bridge on the enemy side. |
| `EnemyMovement.cs` (54 l) | 🟢⚪ | NavMesh chase-the-player (with `GameObject.Find`). |
| `EnemyAttack.cs` (91 l) | 🟢⚪ | Contact damage on the player. |

## Assets/Scripts/Weapons — dormant refactor framework

| File | Status | What it is |
|---|---|---|
| `Interfaces/IBlast.cs` | 🟢 | Earned interface — implemented by `Rocket` and `SelfDestruct`, consumed by `EnemyHealth.TakeDamage`. |
| `Interfaces/IProjectile.cs` | 🟢 | Earned interface (landed with PR #25) — implemented by `Rocket` and `IceBullet`. |

*Culled 2026-07-12: `WeaponSystem.cs`, `WeaponManager.cs`, `RaycastWeapon.cs`, `ProjectileWeapon.cs`,
`IFirearm.cs`, `IPlayerWeapon.cs`, `ISpell.cs`. Weapons stay **concrete components**; a `WeaponConfig`
ScriptableObject arrives only at the rule-of-three / first-balancing-pass trigger (ADR-006).*

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
| `Debug/ScoreManagerDebugger.cs` (90 l) | 🧪🟢 | Score-persistence diagnostic — **wired in `Menu 1.unity`**, so it survived the Cull. (`ScoreDebugger.cs` was unwired and was deleted.) |
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
  scene-place any `Singleton<T>` manager you add, and evolve the existing player/weapon components in place.
- **Wired-in-same-PR (standing rule #1).** A script not referenced by a scene, prefab, or live code path
  in the PR that adds it does not merge. No parallel versions, no `*Refactored` twins. This rule is what
  the Cull existed to enforce — don't rebuild what it removed.
- **Keep this map honest:** update the relevant row in the same PR that adds, deletes, or re-statuses a
  script. A doc that contradicts runtime is a bug (plan §9 rule #5).
- **Trust wiring over names**: lowercase legacy names (`range`, `flashlight`) are live and load-bearing.
