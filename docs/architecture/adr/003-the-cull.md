# ADR-003: Archive dormant code in Git history

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
Wiring and inbound-reference checks distinguished dormant implementations from the live game.

## Decision
The dormant layer was removed in PR #31. Scene references were removed through Unity APIs before script deletion. Owner regression checks passed.

## Consequences
Git history is the recovery mechanism. ScoreManagerDebugger survived because it is wired. IBlast and IProjectile survived because live components implement them. Deletion counts included serialized asset data, not only C#.

## Reconsider when
A shipped feature needs a specific piece again; reintroduce only that piece with wiring and tests.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
