# Inspector / Editor-Serialized-State Dependency Audit

**Date:** 2026-07-12 · **Branch:** `feature/Titan-Zombunny` · Companion to
[`CURRENT_STATE.md`](CURRENT_STATE.md) and the plan's coupling ranking
([`ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md §7`](../architecture/ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md)) — this is
the quantified deep-dive behind coupling items #2 (string coupling) and #3 (scene-serialization drift).

**Question answered:** how much of Zombtoy's behavior lives in Unity-editor-serialized data (scenes,
prefabs, inspector values, project settings) rather than in code — and what that means for refactoring
with autonomous tooling.

> **Post-Cull note (2026-07-12, later same day):** the counts below were measured *before* M3/M4.
> Since then `Level2.unity` was **deleted** (its serialized-empty spawn table — cited here as the flagship
> example of scene-config drift — was a factor in that decision), 4 dormant components/objects were removed
> from the remaining level scenes, and `Assets/Prefabs/Player.prefab` is gone. The **conclusions are
> unchanged and the drift finding was acted on**; only the absolute numbers are now slightly high.

**Method:** grep/GUID census over `Assets/Scripts` (excluding `node_modules`), all 8 scenes, and the
gameplay prefabs. Counts are reproducible (commands in the appendix). Approximate counts are labeled.

---

## 1. Headline verdict

**Dependency level: HIGH — the game cannot be understood, tuned, or safely refactored from C# alone.**

- ~**412 inspector-exposed knobs** in code (297 public serialized fields + 115 `[SerializeField]`, approx).
- **307 MonoBehaviour attachments** across the 8 scenes, hand-wired per scene (no additive/bootstrap scene).
- **~500 prefab-instance property overrides** across the level scenes (Level1: 208, Level3: 178, Level2: 90) —
  each one a place where a scene silently diverges from its prefab.
- **Zero ScriptableObject data assets in the live game.** The only ScriptableObject type (`WeaponData` in
  dormant `WeaponSystem.cs`) is never used. All tuning lives in scene/prefab instances — the least
  reusable, least diffable location Unity offers.
- Balance/config data (spawn tables, weapon loadouts, damage numbers) is **duplicated per scene by hand**,
  and has already measurably drifted (see §3).

This is normal for a project of this origin (Survival-Shooter tutorial lineage), and it is *workable* —
but it is the project's single biggest source of silent breakage, and most of it is invisible to any tool
that only reads C#. This is precisely why pre-2026 LLM tooling kept misjudging the repo.

## 2. Where behavior actually lives — dependency classes

### A. Inspector-tuned values (behavior config in serialized data)

~412 serialized fields across 75 scripts. Top carriers:

| Script | Public fields | Notes |
|---|---|---|
| `WeaponSystem.cs` | 32 | dormant — goes with the Cull |
| `Inventory.cs` | 25 | live: whole weapon loadout is inspector data |
| `EnemyHealth.cs` | 20 | health, score value, attributes (`blast_immunity`), sink speed — per prefab |
| `Rocket.cs` | 16 | speed, 3 damage tiers, radii, destroy timers — all scene/prefab data |
| `PlayerHealth.cs` | 16 | health/stamina numbers + direct UI object references |
| `EnemyManager.cs` | 12 | spawn table (see §C) |

Consequence: a balance question like "how much damage does a rocket do?" has **no answer in code** —
`directHitDMG` is whatever `Rocket.prefab`/`Rocket 1.prefab` say. Code review of a `.cs` diff can never
catch a balance regression; only prefab/scene diffs can. (Example from this branch: Titan health
3000→2000 happened purely inside `Titan Zombunny.prefab`.)

### B. Scene-graph wiring (who exists at runtime)

| Scene | MonoBehaviours | PrefabInstances | Overrides | Serialized UI events |
|---|---|---|---|---|
| Level1 | 86 | 9 | **208** | 2 |
| Level3 | 95 | 6 | 178 | 2 |
| Level2 (not in build) | 59 | 5 | 90 | 0 |
| Menu 3 (leaderboard) | 25 | 1 | 23 | 1 |
| Menu, Menu 1/2/4 | 2–29 | 0 | 0 | 1–5 |

- Every manager (`ScoreManager`, `EnemyManager`, `ItemManager`, `MusicManager`) exists **only because a
  scene object carries it** — `Singleton<T>` deliberately never auto-creates (ADR'd as correct, but it
  makes scene placement load-bearing).
- Gameplay prefabs carry 4–15 MonoBehaviours each (Titan & Clown: 15). Enemy behavior is an
  inspector-composed stack, not a code-declared one.
- UI `Button.onClick` persistent calls are few (≈16 total) — most UI flows through code, which is good.

### C. Per-scene duplicated config — **the drift engine (highest-risk finding)**

The same data is hand-maintained in every level scene, and the copies have already diverged:

| Data | Level1 | Level2 | Level3 | Verdict |
|---|---|---|---|---|
| `EnemyManager` spawn table | **9 weighted entries** | **0 entries** | n/a (no EnemyManager) | Level2's spawner is serialized-empty → spawns nothing. Concrete drift, already shipped |
| `Inventory` weapon wiring | 4 refs | 6 refs | 6 refs | loadouts differ per scene — unclear if intentional |
| `Ammo` component instances | 9 | 9 | 9 | 27 hand-wired ammo configs project-wide (PR #25's `AmmoSystem` centralizes exactly this) |

This is the mechanism behind "Level2 drifted out of the build": nothing enforces that three scenes stay
consistent, so they don't. The single-scene consolidation (issue #19) and the prefab-self-sufficiency
rule (plan §7.3) are the structural fixes.

### D. String-name coupling from code into serialized objects

Code reaching into the scene by name/string — breaks **silently** on rename, with no compiler protection:

| Pattern | Count | Risk profile |
|---|---|---|
| `GameObject.Find("…")` | 50 | rename/renest an object → null at runtime; several in `Awake`/spawn paths |
| `LayerMask.GetMask("…")` | 12 | depends on TagManager layer names (`Floor`=8, `Shootable`=9). The Titan rotation-freeze bug lived here |
| `SetTrigger/SetBool/SetFloat("…")` | 17 | animator parameter names live in `.controller` assets |
| `InvokeRepeating`/`Invoke("method")` | 11 | method rename compiles fine, silently never fires |
| `SendMessage` | 9 | same class of failure |
| `.name ==` comparisons | 7 | includes `range.cs` gating boss attacks on the literal name `"Player"` |
| `CompareTag`/`.tag ==` | 5 | tag list in TagManager |
| `Type.GetType("…")` | 4 | reflection probes for the dormant layer — removed by the Cull |

Meanwhile `GetComponent<…>` appears 182× — runtime lookup rather than serialized references, i.e. wiring
resolved at play time, invisible in both the scene file *and* the inspector.

### E. Project-settings coupling (outside both code and scenes)

- **Layer numbers/names** (`TagManager.asset`): `Floor`/`Shootable` are load-bearing for player turning,
  all shooting raycasts, and explosion queries.
- **Build scene list** (`EditorBuildSettings.asset`): `LoadScene(0…6)` is called with **9 hardcoded build
  indices** in code (only 2 call sites use names/variables). Reordering the build list silently reroutes
  every menu button. Level2's exclusion from the build interacts with this: indices shift depending on
  what's in the list.
- Physics collision matrix, input axes (`"Horizontal"`, `"CameraVertical"`…), and quality settings are
  all behavior-relevant serialized state in `ProjectSettings/`.

### F. What is *not* editor-dependent (the safe islands)

- `GameEvents` hub — pure static C#; zero serialized surface. (Its 20 events are the most
  refactor-friendly seam in the project.)
- Persistence — 38 `PlayerPrefs` calls; code-driven, no scene involvement.
- Backend (`Backend/ZombtoyBackend`), `Leaderboard.cs` HTTP logic, DevTools, shell scripts.
- Most of the dormant layer (ironically): zero scene refs — which is what makes the Cull safe.
  **One exception:** the disabled `PlayerMovementRefactored` component on the Level1 Player must be
  removed *from the scene* as part of the Cull, not just deleted as a file (else: missing-script warning).

## 3. Risk analysis — what breaks loudly vs silently

| Change you might make | What happens | Detected by |
|---|---|---|
| Rename a GameObject ("Player", "Fill", "RocketLauncher", "block"…) | 50 `Find` sites + 7 name-compares silently return null / false | runtime NRE or nothing at all |
| Rename an animator parameter or invoked method | compiles clean, feature silently dies | play-testing only |
| Reorder Build Settings scenes | menu buttons route to wrong scenes | play-testing only |
| Delete/rename a layer | turning, shooting, explosions mis-target | play-testing only |
| Edit a prefab that scenes override | scene keeps the stale override (208 in Level1) | prefab-vs-scene diff only |
| Delete a script file still on a scene object | loud "missing script" warning | Unity console (the *good* case) |
| Change a serialized field's name in code | value silently resets to default unless `[FormerlySerializedAs]` is used | balance drift, invisible |

The pattern: **Unity fails loud on missing scripts, and silent on everything else.** The project's
current defenses are exactly two: the headless batchmode import/compile check (catches YAML corruption +
compile errors, *not* wiring), and play-testing.

## 4. Implications for the roadmap (aligned with plan §8)

1. **The Cull (M3) is inspector-safe by construction** — dormant files have no scene refs. Checklist
   addition: remove the disabled `PlayerMovementRefactored` from Level1's Player, and re-run the
   zero-GUID-reference grep per file *at cull time* rather than trusting this audit's snapshot.
2. **M2 (PR #25 / AmmoSystem)** is where inspector work concentrates: centralizing ammo means touching
   9 hand-wired `Ammo` instances ×3 scenes. Expect the real merge cost in scene/prefab YAML, not C#.
   Do the wiring in the Unity editor; use text diffs to *verify*, not to author.
3. **Single-scene consolidation (#19)** is the structural fix for §C drift — it converts "3 hand-synced
   copies" into 1. Until then: treat Level1 as canonical; don't hand-edit Level2/Level3 configs.
4. **New-code rules** (extends plan §9): prefer `[SerializeField]` private over public fields; prefab-first
   wiring (scene overrides only for genuinely scene-specific values); `LoadScene` by name; no new
   `GameObject.Find` (serialize the reference or resolve via the event hub); `[FormerlySerializedAs]` on
   any serialized-field rename.
5. **For AI/autonomous tooling sessions:** any claim about behavior must check serialized data, not just
   code — the tools that only read C# were wrong about this repo for a year. The GUID-census technique
   (grep scenes/prefabs for a script's `.meta` GUID) is the cheap ground truth; batchmode import is the
   cheap validity check. Both are documented in `FABLE_CHECKPOINT.md`.

## Appendix — reproduce the numbers

```bash
# knobs in code
grep -rE '\[SerializeField\]' Assets/Scripts --include='*.cs' | grep -v node_modules | wc -l
# per-scene wiring load
for s in Assets/*.unity; do echo "$s: $(grep -c 'm_Script: {fileID: 11500000' "$s") MB, \
  $(grep -c 'propertyPath:' "$s") overrides"; done
# string coupling (repeat per pattern)
grep -rE 'GameObject\.Find\(' Assets/Scripts --include='*.cs' | grep -v node_modules | wc -l
# is a script wired anywhere? (the ground-truth check)
g=$(grep -m1 guid: path/to/Script.cs.meta | awk '{print $2}'); grep -rl "guid: $g" Assets --include='*.unity' --include='*.prefab'
# spawn-table drift example
grep -c spawnWeight Assets/Level1.unity Assets/Level2.unity
```
