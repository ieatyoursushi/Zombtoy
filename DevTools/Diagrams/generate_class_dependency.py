#!/usr/bin/env python3
"""Generate PlantUML class inheritance / interface implementation diagram."""
from pathlib import Path
import re
from collections import defaultdict
from common import read_cs_files, load_and_strip, rel

ROOT = Path(__file__).resolve().parents[1] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'
OUT_DIR.mkdir(parents=True, exist_ok=True)

CLASS_DECL = re.compile(r'\bclass\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?::\s*([^\{]+))?\{')
INTERFACE_DECL = re.compile(r'\binterface\s+([A-Za-z_][A-Za-z0-9_]*)')


def parse():
    classes = {}
    interfaces = set()
    extends = defaultdict(set)  # child -> parents (base classes)
    implements = defaultdict(set)  # class -> interfaces

    for f in read_cs_files(ROOT):
        code = load_and_strip(f)
        if not code:
            continue
        for m in INTERFACE_DECL.finditer(code):
            interfaces.add(m.group(1))
        for m in CLASS_DECL.finditer(code):
            cls = m.group(1)
            parents = m.group(2)
            if parents:
                for p in [x.strip() for x in parents.split(',') if x.strip()]:
                    if p in interfaces:
                        implements[cls].add(p)
                    else:
                        extends[cls].add(p)
            classes[cls] = f
    return classes, interfaces, extends, implements


def emit_puml(classes, interfaces, extends, implements):
    out = ['@startuml', 'title Class Inheritance & Interface Implementation', 'skinparam classAttributeIconSize 0']
    for i in sorted(interfaces):
        out.append(f'interface {i}')
    for c in sorted(classes):
        out.append(f'class {c}')
    for child, bases in extends.items():
        for b in bases:
            # Clean up generic syntax for PlantUML compatibility
            clean_base = b.split('<')[0] if '<' in b else b
            out.append(f'{clean_base} <|-- {child}')
    for cls, ifaces in implements.items():
        for i in ifaces:
            out.append(f'{i} <|.. {cls}')
    out.append('@enduml')
    return '\n'.join(out)


def main():
    classes, interfaces, extends, implements = parse()
    puml = emit_puml(classes, interfaces, extends, implements)
    p = OUT_DIR / 'class_dependency.puml'
    p.write_text(puml, encoding='utf-8')
    print(f'Wrote {p}')

if __name__ == '__main__':
    main()
