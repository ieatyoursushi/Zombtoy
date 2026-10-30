# Source diagrams

These Python tools scan `Assets/Scripts` and emit six PlantUML diagrams plus two Markdown reports.
They describe **source references**, not Unity scene/prefab wiring or runtime execution order.
For the running architecture, start with [CODE_MAP](../../docs/CODE_MAP.md) and the
[architecture decisions](../../docs/architecture/adr/).

## Run

From the repository root, using Python 3.9+:

```sh
python3 -B DevTools/Diagrams/run_tests.py
python3 -B DevTools/Diagrams/generate_all.py
```

Full rendering requires `plantuml`, Java and Graphviz on PATH. Use `--no-render` explicitly when
only PlantUML/Markdown sources are wanted. No virtual environment or Python packages are required.
`run_tests.py --generate` runs tests followed by generation/rendering. Any generator or renderer
failure fails the command; existing rendered files are not accepted as evidence of success.

## Outputs under out/

| Stem | Meaning |
|---|---|
| `event_flow` | Declared GameEvents and source publisher/subscriber class links |
| `gameevents_health` | Source-reference counts by event, not measured runtime health |
| `class_dependency` | Declared inheritance and interface implementation |
| `call_graph` | Approximate calls through capitalized type names; instance calls/dynamic dispatch are incomplete |
| `core_architecture` | Core and manager source types; name/folder classification includes helpers such as ScoreManagerDebugger |
| `core_lifecycle_flow` | Methods declared in each file; **not** a sequence/timing trace |

The two `*_report.md` files explain core types and event usage. The 2026-10-30 refresh finds
19 events and three singleton subclasses. Unused event declarations are reported as observations,
not automatically deleted: checking gameplay requires the Unity-side tools.

## Generator limits

Both event reports use `common.scan_events`: declarations first, then method-body mapping and usage.
It handles the project's ordinary C# strings/comments, generic event types and block/expression trigger
methods. Utility calls are excluded unless their method body actually invokes a declared event.
Traversal and output are sorted for reproducibility.

This remains a small scanner, not Roslyn. Multi-type files are attributed to their first class for
event/call analysis; lambdas, interpolated expressions, aliases, reflection, external assemblies and
transitive wrapper chains can need manual inspection. Missing/unreadable files fail generation.
Reports cannot establish lifecycle symmetry at runtime merely from method names. Subscription warnings
compare event **and handler** source pairs, not just event names.

`out/` is a committed snapshot. Regenerate it when relevant C# changes; validate the generator before
trusting new conclusions. No current-time stamp is embedded so identical source yields identical output.
