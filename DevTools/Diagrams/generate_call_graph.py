#!/usr/bin/env python3
"""Generate a high-level class-to-class call graph (approximate) as PlantUML."""
from pathlib import Path
import re
from collections import defaultdict
from common import read_cs_files, load_and_strip

ROOT = Path(__file__).resolve().parents[1] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'
OUT_DIR.mkdir(parents=True, exist_ok=True)

CLASS_NAME_RE = re.compile(r'\bclass\s+([A-Za-z_][A-Za-z0-9_]*)')
CALL_RE = re.compile(r'\b([A-Z][A-Za-z0-9_]*)\s*\.\s*[A-Za-z_][A-Za-z0-9_]*\s*\(')


def collect():
    class_files = {}
    for f in read_cs_files(ROOT):
        code = load_and_strip(f)
        m = CLASS_NAME_RE.search(code)
        if m:
            class_files[m.group(1)] = code
    edges = defaultdict(int)
    classes = set(class_files.keys())
    for cls, code in class_files.items():
        for m in CALL_RE.finditer(code):
            target = m.group(1)
            if target != cls and target in classes:
                edges[(cls, target)] += 1
    return classes, edges


def emit_puml(classes, edges):
    out = ['@startuml', 'title High-Level Class Call Graph', 'skinparam linetype ortho']
    for c in sorted(classes):
        out.append(f'class {c}')
    for (src, dst), weight in edges.items():
        label = '' if weight < 2 else f' : {weight}'
        out.append(f'{src} --> {dst}{label}')
    out.append('@enduml')
    return '\n'.join(out)


def main():
    classes, edges = collect()
    puml = emit_puml(classes, edges)
    p = OUT_DIR / 'call_graph.puml'
    p.write_text(puml, encoding='utf-8')
    print(f'Wrote {p}')

if __name__ == '__main__':
    main()
