# ADR-005: Scene-place three existing singleton managers

**Status:** Accepted · **Recorded:** 2026-10-30

## Context
Source inspection on 2026-10-30 found three Singleton<T> subclasses, correcting the earlier four-singleton claim.

## Decision
ScoreManager, EnemyManager and MusicManager are the existing singleton owners. All three currently override Persistent => true. ItemManager and SFXManager are regular MonoBehaviours.

## Consequences
Keep no-auto-create behavior in Singleton.Instance. AllowAutoCreate is currently an unused setting, despite some subclass overrides. Persistence still depends on the base Awake implementation and object hierarchy. Do not silently change EnemyManager lifetime while correcting these docs.

## Reconsider when
A feature needs another cross-scene owner, or a measured lifetime bug requires changing existing persistence.

Source: [principal-engineer plan](../ZOMBTOY_PRINCIPAL_ENGINEER_PLAN.md) and the decisions/results in [execution log](../EXECUTION_LOG.md).
