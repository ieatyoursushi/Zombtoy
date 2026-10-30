# ADR-008: Defer multiplayer provisions

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
Network-ready declarations and an unused network event complicated a game without an implemented networking requirement.

## Decision
Build no client networking abstractions, auth, accounts or anticheat until a dedicated multiplayer decision is approved.

## Consequences
The unused OnNetworkEvent was removed. Finish the single-player loop; do not claim that local events or singletons establish multiplayer readiness.

## Reconsider when
A finished single-player loop, demand for co-op, and owner willingness to fund a substantial implementation lead to a netcode ADR.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
