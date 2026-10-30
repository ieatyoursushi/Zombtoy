#!/usr/bin/env python3
"""Generate the GameEvents source-level publisher/subscriber graph."""
from pathlib import Path
from collections import defaultdict
from common import read_cs_files, load_and_strip, scan_events, STATIC_ANALYSIS_NOTE

ROOT = Path(__file__).resolve().parents[2] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'


def collect():
    sources = {p: load_and_strip(p) for p in read_cs_files(ROOT)}
    events, _ = scan_events(sources, ROOT)
    producers, consumers = defaultdict(set), defaultdict(set)
    for event, data in events.items():
        producers[event].update(data['publishers'])
        consumers[event].update(cls for cls, _ in data['subscribers'])
    return set(events), producers, consumers


def emit_puml(decls, producers, consumers):
    out = ['@startuml', 'title Game Event Flow (source references)', 'skinparam linetype ortho']
    for event in sorted(decls):
        out.append(f'class {event} << (E,#FFCC00) Event >>')
    for event, classes in sorted(producers.items()):
        for cls in sorted(classes):
            out.append(f'{cls} --> {event} : raises')
    for event, classes in sorted(consumers.items()):
        for cls in sorted(classes):
            out.append(f'{event} --> {cls} : notifies')
    out.extend(['legend bottom', STATIC_ANALYSIS_NOTE, 'endlegend', '@enduml'])
    return '\n'.join(out)


def main():
    decls, producers, consumers = collect()
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    path = OUT_DIR / 'event_flow.puml'
    path.write_text(emit_puml(decls, producers, consumers), encoding='utf-8')
    print(f'{len(decls)} events; wrote {path}')


if __name__ == '__main__':
    main()
