# ADR-002: Evolve the live stack in place

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
The 2025 replacement health, input, game-state and weapon systems compiled but were largely unwired.

## Decision
Improve the components that scenes and prefabs actually use. New gameplay scripts must have a live caller or serialized attachment in the same PR.

## Consequences
Prefer composition and direct serialized references. Do not create parallel Refactored versions or replacement frameworks awaiting migration.

## Reconsider when
An individually justified exception requires its own ADR and a concrete migration/validation plan.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
