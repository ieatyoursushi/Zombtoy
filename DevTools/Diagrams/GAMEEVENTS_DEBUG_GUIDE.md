# Inspecting GameEvents

Run `python3 -B DevTools/Diagrams/generate_all.py` from the repository root and read
`out/gameevents_debug_report.md`. See [README](README.md) for prerequisites and limits.

The report counts source publisher **classes** and subscriber **class/handler pairs**. A trigger such
as `GameEvents.PlayerDeath()` maps to the event actually invoked inside that method. This differs from
runtime subscriber counts: several instances can execute one class, and unwired code need not execute.

`GameEvents.cs` also exposes `GetSubscriberCount` and `SafeInvoke` helpers. Some triggers log subscriber
counts directly; `SafeInvoke` exists but is not automatically used by all triggers. Do not infer exception
isolation merely because the helper is available.

When investigating a reported missing subscription or publisher:

1. Read the trigger and calling code, then trace the components through scenes/prefabs.
2. Compare subscription and unsubscription **handler names**, and inspect their lifecycle paths.
3. Observe the scene in Play mode and read logs/runtime state before declaring a gameplay defect.

“No source publisher” and “no source subscriber” are observations, not instructions to delete events.
The current source contains unused events left from earlier work. This tooling milestone changes no
runtime event signatures. The lifecycle diagram shows declared methods only and promises no timing.
