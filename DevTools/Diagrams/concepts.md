# Diagram Generator Concepts & Architecture

## 1. Static Analysis by Regex
- The tool reads C# source files as plain text (no compilation or execution).
- It strips comments and string literals to avoid false matches.
- Uses regular expressions (regex) to find:
  - Class and interface declarations
  - Inheritance and interface implementation
  - Event declarations, publishers, and subscribers
  - Method/class-level call relationships

## 2. Why Regex Works
- Programming languages have highly regular, predictable syntax.
- Regex can reliably match patterns like `class Foo : Bar`, `EventName += Handler`, or `Object.Method()`.
- This approach is language-agnostic: swap out regexes to support Java, Python, etc.

## 3. Output: PlantUML
- Relationships are output as PlantUML text files (`.puml`), which can be rendered to diagrams.
- Types of diagrams:
  - Event flow (publishers → events → subscribers)
  - Class dependency (inheritance, interfaces)
  - Call graph (class-to-class method calls)

## 4. Limitations
- Regex is fast and simple, but not a full parser:
  - May miss complex/obfuscated code
  - Only finds static (not runtime) relationships
  - Doesn't resolve dynamic dispatch, reflection, or runtime wiring
- For 100% accuracy, a language parser (e.g. Roslyn for C#) would be needed.

## 5. Extending the Tool
- To support new patterns, add or adjust regexes in `common.py`.
- To support new languages, add new regexes for that language's syntax.
- To add new diagram types, create a new generator script following the pattern of the existing ones.

---

# Quick Guide: Expanding & Documenting the Diagram Dev Tool

## Adding New Analysis Features
1. **Identify the pattern** you want to extract (e.g. property usage, async method calls).
2. **Write a regex** for that pattern in `common.py`.
3. **Add a new generator script** (e.g. `generate_property_usage.py`).
4. **Output PlantUML** or another diagram format as needed.
5. **Add a test** in `test_diagrams.py` to cover your new feature.

## Example: Adding Prefab-to-Script Mapping
- Write a regex to parse `.prefab` YAML for `m_Script` GUIDs.
- Map GUIDs to C# class names (using Unity's meta files).
- Output a diagram showing which prefabs use which scripts.

## Documenting the Tool
- Keep `README.md` up to date with usage, requirements, and new features.
- Add a section for each new generator script explaining:
  - What it analyzes
  - What relationships it visualizes
  - Example output
- Use `concepts.md` for deep dives into the architecture and design philosophy.

## Best Practices
- Keep regexes simple and well-commented.
- Add unit tests for every new pattern.
- Regenerate diagrams after major codebase changes.
- Use PlantUML stereotypes and colors for clarity in diagrams.

---

**This dev tool is designed to be fast, extensible, and language-agnostic—perfect for rapid architecture visualization and onboarding!**
