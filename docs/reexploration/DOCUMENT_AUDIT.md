# Zombtoy — Markdown Document Audit (2026-07-12)

Every tracked `*.md` outside `node_modules`. Claims were checked against source code, scene/prefab GUID wiring, git history, and the running backend code. Confidence: **high** = claims individually verified; **medium** = structure verified, spot-checked claims; **low** = skimmed.

| Document | Purpose | Classification | Verified claims | Stale / unsupported claims | Recommended action | Confidence |
|---|---|---|---|---|---|---|
| `README.md` | Project overview, setup, architecture summary | **Mostly current** | Unity 2022.3.37f1 badge matches `ProjectVersion.txt`; .NET 8 backend exists; scenes/modes list broadly right; "actively being refactored… See REFACTOR_PLAN" honest | Project-structure diagram idealized (`Assets/Scenes/`, `Audio/` dirs don't exist — scenes sit at `Assets/` root); "WeaponSystem: Comprehensive weapon handling" implies active but WeaponSystem is dormant (0 refs); "Health & Stamina System: Complete" — stamina exists in legacy `PlayerHealth`, refactored version dormant | Keep as entry point. Correct structure diagram + soften WeaponSystem claim during M4 doc pass | high |
| `REFACTOR_PLAN.md` | Original refactor plan + completion report | **Partially stale → treat as historical record with a misleading header** | Batch lists match real files (GameEvents, Singleton, ComponentCache, GameStateManager, ScoreManager, EnemyManager, GameOverManager all exist); original problem analysis (Find() abuse, god objects) was accurate | Header "✅ REFACTOR COMPLETED — January 2025" (date also wrong; PR #2 merged Aug 2025); "✅ Remove all GameObject.Find() calls" — **55 remain**; "✅ Split PlayerHealth monolith" — split exists but scenes run legacy `PlayerHealth`; "✅ Modular weapon system framework" — dormant, unwired; "✅ Manager system completely overhauled" — only Score/Enemy/Item wired; "100% backward compatibility" true only because legacy is still what runs | Do **not** delete (useful history). Prepend a short status note pointing to `docs/reexploration/CURRENT_STATE.md`; retitle claims as "code written" vs "wired into game" (M4) | high |
| `DOTNET_BACKEND_INTEGRATION_GUIDE.md` (1311 l) | Backend build-out guide (models, services, SignalR, Docker, auth) | **Tutorial/reference — aspirational, not descriptive** | Stack choice (.NET Minimal API) matches reality | Describes services layer, SignalR hubs, JWT, Docker, `localhost:5000` client — none implemented; real backend is ~100-line `Program.cs` with 3 endpoints on port 3000 | Keep as design reference; add one-line preamble "target design, not current state" (M4) | medium |
| `workflow.md` | Git/Unity branch-switching & worktree practices | **Current source of truth** | .gitignore claims match repo (Library/Temp ignored); worktree guidance sound for this repo | stash@{2} contains +23 uncommitted lines for this file (unreviewed) | Keep. Review stash@{2} additions and fold in or drop | high |
| `Backend/ZombtoyBackend/README.md` | .NET backend usage | **Current source of truth** | Endpoints `GET /`, `POST /addScore`, `GET /getAllScores` all verified in `Program.cs`; SQLite file confirmed; "matches Leaderboard.cs expectations" consistent with `Assets/Scripts/Server/Leaderboard.cs` | none found | Keep as-is | high |
| `DevTools/Diagrams/README.md` | Diagram tool usage | **Mostly current** | Generator scripts exist and match names; regex approach documented in concepts.md | Doesn't note that `out/` snapshots go stale (currently Oct 14 2025) | Keep; add regeneration note (M5) | medium |
| `DevTools/Diagrams/concepts.md` | Explains regex static-analysis design | **Current (tool-scoped)** | Matches `common.py` implementation (verified regexes) | `EVENT_RAISE_RE` limitation (only `GameEvents.X?.Invoke` style) not documented | Keep; document the raise-detection caveat (M5) | high |
| `DevTools/Diagrams/GAMEEVENTS_DEBUG_GUIDE.md` | Using GameEvents debug helpers | **Current (tool-scoped)** | `GetSubscriberCount`/`SafeInvoke` helpers exist in `GameEvents.cs` | — | Keep | medium |
| `DevTools/Diagrams/out/core_architecture_report.md` | Generated report | **Generated, stale** (Oct 14 2025 — predates Titan/rocket work) | n/a | Snapshot of older code state | Regenerate after M1 (and after verifying parser, M5); never hand-edit | high |
| `DevTools/Diagrams/out/gameevents_debug_report.md` | Generated report | **Generated, stale** | n/a | Same | Same | high |
| `DevTools/picking_cherries.md` | Personal git cherry-pick tutorial | **Tutorial/reference only** | n/a (process notes) | — | Keep | low |
| `DevTools/stashing_gold.md` | Personal git stash tutorial | **Tutorial/reference only** | n/a | — | Keep | low |
| `Backend/ZombtoyBackend/`… (no other md) | — | — | — | — | — | — |
| `Backend/ZombtoyBackend-C/README.md` | C backend docs (educational) | **Mostly current** (md-only audit per owner; C sources not read) | Project structure matches disk (`src/{main,database,http_handlers}.c`, `include/models.h`, Makefile); endpoints mirror the .NET trio; honest about being a learning project | Calls mongoose a "single-header library" but repo also carries `mongoose.c` **and tracked build artifacts** (`mongoose.o`, `obj/`, compiled binary, `zombtoy_c.db`); port 8080 + `make test` unverified (code excluded from analysis) | Keep. Untrack build artifacts during M4 hygiene | medium |
| `Backend/ZombtoyBackend-C/C-Mem-Management-Refresher.md` | Generic C memory/pointers study notes | **Tutorial/reference only** | Educational content, no project-state claims to verify | — | Keep | low |
| `Assets/Scripts/Server/zombtoy-backend/node_modules/**/*.md` (~120 files) | Vendor docs | **Generated/vendor** | n/a | n/a | Removed automatically when the tracked Node backend is deleted (M4) | high |

## Actions taken (2026-07-12, follow-up session — supersedes the "recommended action" column where they overlap)

The owner authorized the doc-correction pass. What changed:

1. **Reorganized into `docs/`** (git mv, history preserved): `REFACTOR_PLAN.md` → `docs/history/`,
   `DOTNET_BACKEND_INTEGRATION_GUIDE.md` → `docs/backend/`, `workflow.md` → `docs/`. Root now has only `README.md`.
2. **`docs/history/REFACTOR_PLAN.md`**: prominent status-correction banner prepended (completion claims marked
   premature, live-vs-dormant list, pointer to CURRENT_STATE + issue #16). Body kept verbatim as history.
3. **`docs/backend/DOTNET_BACKEND_INTEGRATION_GUIDE.md`**: scope note prepended — target design, not documentation;
   real backend = 3 endpoints on port 3000.
4. **`README.md`**: structure diagram rewritten to match disk (no `Assets/Scenes/`/`Audio/`; docs/ tree added);
   WeaponSystem marked dormant; "Health & Stamina: Complete" softened; false "Object Pooling: Implemented" fixed
   (zero pooling code exists — verified by grep); Level2 not-in-build noted; links updated to new paths.
5. **`docs/workflow.md`**: broken `./open-unity.fish` paths corrected to `DevTools/shell_scripts/`.
6. **`DevTools/Diagrams/README.md`**: stale absolute venv paths (`~/Desktop/Zombtoy-Project/.venv` — a previous
   copy of the repo that still exists as a sibling dir) replaced with plain `python3`; out/ staleness warning added.
7. **New: [`docs/CODE_MAP.md`](../CODE_MAP.md)** — per-file map of all 75 scripts under `Assets/Scripts`
   (purpose + active/transitional/dormant/legacy status from GUID wiring evidence). This is the doc that
   contextualizes the C# codebase for future contributors/sessions.
8. **New: [`docs/README.md`](../README.md)** — documentation index ordered by trustworthiness.

Not done (deliberately): editing C-backend docs beyond audit (code excluded by owner); deleting any document;
regenerating DevTools `out/` reports (queued behind parser verification, M5).

## Cross-cutting conclusions

1. **The single most misleading artifact** is REFACTOR_PLAN.md's completion header. Everything else is either accurate, clearly aspirational, or obviously a tutorial. A one-paragraph status preamble fixes it without rewriting history.
2. There is **no current architecture document** that distinguishes "code that exists" from "code that runs." `docs/reexploration/CURRENT_STATE.md` (this audit's sibling) now fills that role and should be treated as the source of truth until the migration (issue #16) completes.
3. No document should be deleted this session. Consolidation candidates for later: fold the two `out/*.md` generated reports out of version control (or regenerate on demand), and let the Node-backend vendor docs disappear with the M4 hygiene deletion.
