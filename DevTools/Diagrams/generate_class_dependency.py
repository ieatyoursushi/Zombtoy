#!/usr/bin/env python3
"""Generate PlantUML class inheritance / interface implementation diagram."""
from pathlib import Path
import re
from collections import defaultdict
from common import read_cs_files, load_and_strip, rel

ROOT = Path(__file__).resolve().parents[2] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'
OUT_DIR.mkdir(parents=True, exist_ok=True)

CLASS_DECL = re.compile(r'\bclass\s+(\w+)(?:<[^>{}]*>)?\s*(?::\s*([^\{]+))?\{')
INTERFACE_DECL = re.compile(r'\binterface\s+([A-Za-z_][A-Za-z0-9_]*)')


def parse():
    classes = {}
    interfaces = set()
    extends = defaultdict(set)  # child -> parents (base classes)
    implements = defaultdict(set)  # class -> interfaces

    sources = {f: load_and_strip(f) for f in read_cs_files(ROOT)}
    for code in sources.values():
        interfaces.update(INTERFACE_DECL.findall(code))
    for f, code in sorted(sources.items()):
        for m in CLASS_DECL.finditer(code):
            cls = m.group(1)
            parents = m.group(2)
            if parents:
                for p in [x.strip() for x in re.split(r',\s*(?![^<]*>)', parents.split('where')[0]) if x.strip()]:
                    if p in interfaces:
                        implements[cls].add(p)
                    else:
                        extends[cls].add(p)
            classes[cls] = f
    return classes, interfaces, extends, implements


def emit_puml(classes, interfaces, extends, implements):
    out = ['@startuml', 'title Class Inheritance & Interface Implementation (static source)' , 'skinparam classAttributeIconSize 0']
    for i in sorted(interfaces):
        out.append(f'interface {i}')
    for c in sorted(classes):
        out.append(f'class {c}')
    for child, bases in sorted(extends.items()):
        for b in sorted(bases):
            # Clean up generic syntax for PlantUML compatibility
            clean_base = b.split('<')[0] if '<' in b else b
            out.append(f'{clean_base} <|-- {child}')
    for cls, ifaces in sorted(implements.items()):
        for i in sorted(ifaces):
            out.append(f'{i} <|.. {cls}')
    out.append('@enduml')
    return '\n'.join(out)


def main():
    classes, interfaces, extends, implements = parse()
    print(f'DEBUG: parsed {len(classes)} classes, {len(interfaces)} interfaces, {sum(len(v) for v in extends.values())} extends, {sum(len(v) for v in implements.values())} implements')
    puml = emit_puml(classes, interfaces, extends, implements)
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    p = OUT_DIR / 'class_dependency.puml'
    p.write_text(puml, encoding='utf-8')
    print(f'Wrote {p}')

if __name__ == '__main__':
    main()
