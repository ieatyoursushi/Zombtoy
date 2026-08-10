# Baseline Comparison — first GitHub commit vs today

**Baseline:** `071291f5` "Entire project commit - 1st." (the original high-school-era upload)
**Compared against:** `feature/Titan-Zombunny` @ `46636c52` (2026-07-12)
**Method:** git archaeology (`ls-tree`, `diff --stat`, `git grep` at both endpoints) + the wiring evidence
in [`CURRENT_STATE.md`](CURRENT_STATE.md) / [`CODE_MAP.md`](../CODE_MAP.md). Same metrics measured the
same way at both endpoints.

## Scale of the journey

| Metric | Baseline | Today |
|---|---|---|
| Commits | 1 | +40 |
| Scripts under `Assets/Scripts` | 45 | 75 |
| Script churn | — | 118 files, +9,803 / −2,596 lines |
| `GameObject.Find*` occurrences | **43** | **65** |
| Backends | dreamlo + Node/Express | + .NET 8/SQLite (live) + educational C |

Notable: the folder taxonomy (`Player/`, `Enemy/`, `Managers/`, `UI/`) already existed at baseline —
the refactor added `Core/`, `Weapons/`, `Utility/`, `Debug/`, `Server/` and ~30 scripts.

## Verdicts by domain

### Done well ✅

- **Managers rewritten from stubs into real systems** — `ScoreManager` 35→352 l, `EnemyManager` 41→622 l,
  `MusicManager` 35→450 l, all `Singleton<T>`-based, scene-placed, event-driven, and **actually wired**.
  This is the refactor's genuine success story: the baseline versions were near-empty placeholders.
- **`GameEvents` static hub** — the single highest-leverage addition. Legacy and new code both publish
  through it (`PlayerHealth`, `EnemyHealth`, UI binders), so the event architecture is real, not aspirational.
  Adding `SafeInvoke` + subscriber-count debug helpers was mature engineering.
- **`Inventory.cs` in-place rewrite** (issue #1) — 135/135 lines changed at identical size: replaced the
  hardcoded weapon fields with a data-driven `WeaponEntry` list *while keeping legacy fallback*. This is the
  **mindful pattern the rest of the migration should copy**: refactor what runs, in place, incrementally —
  instead of writing a parallel dormant version.
- **Backend replacement** — Node→.NET was executed completely (client `Leaderboard.cs` wired in Menu 3),
  documented accurately, and kept minimal instead of gold-plated.

### Done mindfully but left incomplete 🟡

- **The parallel "Refactored" layer** (`PlayerHealthRefactored`, `PlayerMovementRefactored`, `WeaponManager`/
  `WeaponSystem`, `GameStateManager`, `PlayerInputManager` — ~2,000 lines). The code quality is fine and the
  decision to not break gameplay was sound, but shipping it **unwired** created a two-architecture repo where
  filenames lie about what runs. The honest framing existed all along in issue #16; the docs (until this audit)
  claimed completion instead.
- **Boss/Titan work** — good incremental prefab-migration instinct (moving scene-added components into the
  prefab), stopped mid-flight with the groundTarget mis-binding (fixed 2026-07-12).

### Done badly ❌

- **The plan's own #1 goal went backwards:** "Remove all GameObject.Find() calls" was checked off ✅ in
  REFACTOR_PLAN.md, yet occurrences grew **43 → 65**. Most damning, `PlayerHealthRefactored.cs` — the
  refactor's centerpiece — itself contains `GameObject.Find` calls. New code kept using the anti-pattern
  the refactor existed to kill.
- **Repo hygiene:** the "removed" Node backend is still fully tracked including `node_modules`
  (~120 vendor packages — the source of GitHub's 4 dependabot alerts), and the C backend commits build
  artifacts (`mongoose.o`, `obj/`, compiled binary, a `.db` file).
- **Documentation drift:** premature completion claims (REFACTOR_PLAN header, README's "object pooling
  implemented" — zero pooling code exists) — corrected in the 2026-07-12 doc pass.

## Restart? **No.**

A restart would discard the parts that genuinely work (live event hub, real managers, working boss content,
functioning backend) to escape problems that are cheap to fix in place. The codebase's issue is not rot —
it's an **unfinished lane change**. The dependency-ordered path out already exists in
[`NEXT_MILESTONES.md`](NEXT_MILESTONES.md): finish or cull the dormant layer per-system (issue #16), copy the
`Inventory.cs` in-place pattern, and do the hygiene deletions. Estimated to be far cheaper than any rewrite.

**Overall grade: B.** Real architectural progress with two honest defects — declaring victory early, and
building the new lane without moving traffic onto it.
