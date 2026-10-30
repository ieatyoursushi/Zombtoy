# ADR-004: Use GameEvents for state-change notifications

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
The flat static event hub connects scoring, enemy counts, music and UI without a bus framework.

## Decision
Keep GameEvents for state-change fan-out. UI commands call methods directly. No per-frame messaging or speculative network events.

## Consequences
Match subscriptions with the same event/handler unsubscription. Static reports identify source references; they do not prove runtime activity, ordering, or leaks.

## Reconsider when
Event count triples with real consumers, or traceability demonstrably fails in two separate features.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
