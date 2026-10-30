# Diagram scanner concepts

`common.py` strips comments and string/character literals before scanning. Strings are recognized
before comment markers so a URL cannot erase the rest of a line. Whitespace separates removed tokens.

Event discovery has two passes. First collect all GameEvents declarations, including multi-argument
`Action` types, and map public static trigger methods to the events their bodies invoke. Balanced braces
bound block bodies; expression bodies end at their semicolon. Then collect source call sites and
subscriptions independently of file enumeration order. The flow and debug reports share that result.
Class inheritance similarly collects interfaces before classifying parents.

These are deliberately bounded heuristics for the current code patterns. They do not implement full C#
syntax, resolve Unity GUIDs, inspect Inspector values, or establish runtime ordering. In particular,
string interpolation expressions are stripped with their containing strings, aliases/reflection can be
missed, and first-class attribution is approximate for files containing several types. A future unsupported
pattern should receive a focused fixture and scanner change; do not quietly invent runtime conclusions.

`test_diagrams.py` includes both adversarial fixtures and agreement checks against the real project.
Generation/rendering failures propagate to the shell. Sorted traversal and emissions make snapshots
repeatable. Use [README](README.md) for commands and output descriptions.
