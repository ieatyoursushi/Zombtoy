# ADR-006: Concrete weapons and the existing Ammo component

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
PR #25 contained useful Ammo encapsulation and an unused parallel AmmoSystem class.

## Decision
Keep Inventory, concrete weapon scripts, and the live Ammo.CanShoot/TryShoot/CanReload API. AmmoSystem was intentionally dropped before merge. Keep IBlast and IProjectile while their live uses justify them.

## Consequences
Config-only ScriptableObjects become useful at a real balancing pass or repeated tuning across three assets. Do not rebuild a weapon-manager/base-class hierarchy.

## Reconsider when
More than roughly eight weapons or a demonstrated designer-tuning workflow changes the tradeoff.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
