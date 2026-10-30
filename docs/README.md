# Zombtoy Documentation Index

Current navigation and historical audit records. Start with EXECUTION_LOG for status; older audits retain their original evidence. Docs that live next to code
(backend READMEs, DevTools docs) stay with their code; everything project-wide lives here.

## Start here (current source of truth)

| Doc | What it answers |
|---|---|
| [`architecture/ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md`](architecture/ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) | The architecture decision document (2026-07-12): product identity, keep/delete verdicts, pattern rulings, governance rules. **Supersedes NEXT_MILESTONES M3** (now "The Cull"). |
| [`architecture/EXECUTION_LOG.md`](architecture/EXECUTION_LOG.md) | **Live execution checklist** for that plan — milestone checkboxes, owner gates, branch strategy, and where execution currently stands. Read this to resume work. |
| [`CODE_MAP.md`](CODE_MAP.md) | Every C# script: what it does and whether it actually runs (active vs dormant vs legacy). |
| [`reexploration/FABLE_CHECKPOINT.md`](reexploration/FABLE_CHECKPOINT.md) | Historical session narrative; use EXECUTION_LOG for the current resume step. |
| [`workflow.md`](workflow.md) | Git + Unity branch/worktree practices for this repo. |

Current decisions: [ADR-001…008](architecture/adr/). Agent/editor checks: [Unity validation](architecture/UNITY_VALIDATION.md).
M1–M4 are merged. M5 tooling, asset/build checks and the owner-assisted gameplay check are complete;
publication status is in EXECUTION_LOG. New gameplay features begin separately in M6.

## Historical audits

[Original state snapshot](reexploration/CURRENT_STATE.md) and [original milestone proposal](reexploration/NEXT_MILESTONES.md) are preserved as historical records; executed decisions supersede them.

| Doc | What it answers |
|---|---|
| [`reexploration/DOCUMENT_AUDIT.md`](reexploration/DOCUMENT_AUDIT.md) | Which markdown claims were verified, stale, or misleading — and what was done about them. |
| [`reexploration/BRANCH_AND_WORK_AUDIT.md`](reexploration/BRANCH_AND_WORK_AUDIT.md) | Branches, PRs, stashes: what work exists where and the safest integration order. |
| [`reexploration/BASELINE_COMPARISON.md`](reexploration/BASELINE_COMPARISON.md) | Review of the whole journey vs the first commit: what was refactored well, badly, and why no restart is needed. |
| [`reexploration/INSPECTOR_DEPENDENCY_AUDIT.md`](reexploration/INSPECTOR_DEPENDENCY_AUDIT.md) | How much behavior lives in editor-serialized data (scenes/prefabs/inspector/settings) vs code: ~412 knobs, per-scene config drift, string-coupling counts, silent-vs-loud failure map. |

## Reference / design (aspirational, not descriptive)

| Doc | What it is |
|---|---|
| [`backend/DOTNET_BACKEND_INTEGRATION_GUIDE.md`](backend/DOTNET_BACKEND_INTEGRATION_GUIDE.md) | Target design for a full backend (SignalR, auth, Docker). **Not implemented** — see its scope note. |

## Historical record (kept unmodified, headers corrected)

| Doc | What it is |
|---|---|
| [`history/REFACTOR_PLAN.md`](history/REFACTOR_PLAN.md) | The 2025 core-refactor plan + its premature completion claims. Accurate problem analysis; see the status-correction banner. |

## Docs that live with their code

- [`Backend/ZombtoyBackend/README.md`](../Backend/ZombtoyBackend/README.md) — the real backend's usage (accurate).
- [`Backend/ZombtoyBackend-C/README.md`](../Backend/ZombtoyBackend-C/README.md) + `C-Mem-Management-Refresher.md` — educational C backend.
- [`DevTools/Diagrams/README.md`](../DevTools/Diagrams/README.md), `concepts.md`, `GAMEEVENTS_DEBUG_GUIDE.md` — diagram/debug tooling.
- `DevTools/picking_cherries.md`, `DevTools/stashing_gold.md` — personal git technique notes.
