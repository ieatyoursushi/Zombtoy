# ADR-007: Keep the minimal score API

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
The .NET 8 backend implements GET /, POST /addScore and GET /getAllScores using EF Core and SQLite. The long integration guide describes an unimplemented future design.

## Decision
Keep that contract until a feature demands change. A named-score UI may introduce additive JSON v2 endpoints. Auth, SignalR and Docker remain deferred.

## Consequences
The .NET Leaderboard client and dreamlo HighScores client both remain in Menu 3 by owner decision. Revisit that exception during leaderboard work or before public release. Use a temporary database for automation.

## Reconsider when
A leaderboard feature needs names or a dedicated accounts/multiplayer ADR supersedes this decision.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
