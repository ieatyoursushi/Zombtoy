# Zombtoy Documentation Index

All project documentation, ordered by how much you should trust it. Docs that live next to code
(backend READMEs, DevTools docs) stay with their code; everything project-wide lives here.

## Start here (current source of truth)

| Doc | What it answers |
|---|---|
| [`CODE_MAP.md`](CODE_MAP.md) | Every C# script: what it does and whether it actually runs (active vs dormant vs legacy). |
| [`reexploration/CURRENT_STATE.md`](reexploration/CURRENT_STATE.md) | Deep architecture audit: timeline, wiring evidence, blockers, uncertainties (2026-07-12). |
| [`reexploration/NEXT_MILESTONES.md`](reexploration/NEXT_MILESTONES.md) | Dependency-ordered plan (M1 boss fix ✅ done → M2 inventory/weapons → M3 migration decision → …). |
| [`reexploration/FABLE_CHECKPOINT.md`](reexploration/FABLE_CHECKPOINT.md) | Session-to-session working log; read first when resuming AI-assisted work. |
| [`workflow.md`](workflow.md) | Git + Unity branch/worktree practices for this repo. |

## Audits

| Doc | What it answers |
|---|---|
| [`reexploration/DOCUMENT_AUDIT.md`](reexploration/DOCUMENT_AUDIT.md) | Which markdown claims were verified, stale, or misleading — and what was done about them. |
| [`reexploration/BRANCH_AND_WORK_AUDIT.md`](reexploration/BRANCH_AND_WORK_AUDIT.md) | Branches, PRs, stashes: what work exists where and the safest integration order. |

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
