#!/usr/bin/env python3
"""Generate PlantUML event flow diagram for GameEvents publishers and subscribers."""
from pathlib import Path
from collections import defaultdict
import re
from common import read_cs_files, load_and_strip, rel, EVENT_DECL_RE, EVENT_SUBSCRIBE_RE, EVENT_RAISE_RE

ROOT = Path(__file__).resolve().parents[2] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'
OUT_DIR.mkdir(parents=True, exist_ok=True)

EVENTS_SOURCE = 'GameEvents'


def collect():
    decls = set()
    producers = defaultdict(set)  # event -> set(class)
    consumers = defaultdict(set)  # event -> set(class)
    # map wrapper method name -> declared event name (e.g. EnemySpawned -> OnEnemySpawned)
    wrapper_map = {}

    # try to locate GameEvents.cs and build wrapper->event mapping
    for f in read_cs_files(ROOT):
        if f.name == 'GameEvents.cs':
            code = load_and_strip(f)
            if not code:
                break
            # expression-bodied methods: public static void EnemySpawned(...) => OnEnemySpawned?.Invoke(...);
            for m in re.finditer(r'public\s+static\s+[^(\s]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\([^)]*\)\s*=>\s*([A-Za-z_][A-Za-z0-9_]*)\s*\?\.\s*Invoke', code):
                wrapper, ev = m.group(1), m.group(2)
                wrapper_map[wrapper] = ev
            # block-bodied methods: public static void Foo(...) { ... OnBar?.Invoke(...); }
            for m in re.finditer(r'public\s+static\s+[^(\s]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\([^)]*\)\s*\{', code):
                wrapper = m.group(1)
                # naive body capture: find the body following the method header up to the next closing brace at same level
                # simple approach: search for first occurrence of "?\.Invoke" after the method name and take the token before it
                post = code[m.end():m.end()+200]
                em = re.search(r'([A-Za-z_][A-Za-z0-9_]*)\s*\?\.\s*Invoke', post)
                if em:
                    wrapper_map[wrapper] = em.group(1)

    for f in read_cs_files(ROOT):
        code = load_and_strip(f)
        if not code:
            continue
        # class name heuristic
        class_match = re.search(r'class\s+([A-Za-z_][A-Za-z0-9_]*)', code)
        class_name = class_match.group(1) if class_match else rel(f, ROOT)

        for m in EVENT_DECL_RE.finditer(code):
            decls.add(m.group(1))
        # detect direct raises via ?.Invoke (rare) or wrapper method calls GameEvents.SomeTrigger(...)
        for m in EVENT_RAISE_RE.finditer(code):
            producers[m.group(1)].add(class_name)
        # match calls like GameEvents.EnemySpawned(...)
        for m in re.finditer(r'GameEvents\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\(', code):
            name = m.group(1)
            # if this is a wrapper mapped to a declared event, map to the event name
            if name in wrapper_map:
                producers[wrapper_map[name]].add(class_name)
            else:
                # otherwise record as-is (will show as a node if no matching event decl)
                producers[name].add(class_name)
        for m in EVENT_SUBSCRIBE_RE.finditer(code):
            consumers[m.group(1)].add(class_name)
    return decls, producers, consumers


def emit_puml(decls, producers, consumers):
    out = ['@startuml', 'title Game Event Flow', 'skinparam linetype ortho']
    # define event as diamond nodes (use stereotypes)
    for e in sorted(decls):
        out.append(f'class {e} << (E,#FFCC00) Event >>')
    # link producers -> event
    for e, classes in producers.items():
        for c in classes:
            out.append(f'{c} --> {e} : raises')
    # link event -> consumers
    for e, classes in consumers.items():
        for c in classes:
            out.append(f'{e} --> {c} : notifies')
    out.append('@enduml')
    return '\n'.join(out)


def main():
    decls, producers, consumers = collect()
    print(f'DEBUG: found {len(decls)} event decls, {sum(len(v) for v in producers.values())} producer links, {sum(len(v) for v in consumers.values())} consumer links')
    puml = emit_puml(decls, producers, consumers)
    p = OUT_DIR / 'event_flow.puml'
    p.write_text(puml, encoding='utf-8')
    print(f'Wrote {p}')

if __name__ == '__main__':
    main()
