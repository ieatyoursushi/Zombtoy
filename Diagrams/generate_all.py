#!/usr/bin/env python3
"""Run all diagram generators and optionally render PNG/SVG if plantuml is available."""
from subprocess import run, PIPE
from pathlib import Path
import shutil
import sys

ROOT = Path(__file__).resolve().parent
OUT = ROOT / 'out'
OUT.mkdir(exist_ok=True)

SCRIPTS = [
    'generate_event_flow.py',
    'generate_class_dependency.py',
    'generate_call_graph.py',
]

def run_generators():
    for s in SCRIPTS:
        print(f'-- Running {s}')
        r = run([sys.executable, str(ROOT / s)], capture_output=True, text=True)
        if r.returncode != 0:
            print(r.stderr)
        else:
            print(r.stdout.strip())

def plantuml_available():
    return shutil.which('plantuml') is not None

def render():
    if not plantuml_available():
        print('PlantUML not found in PATH; skipping render (install with: brew install plantuml graphviz)')
        return
    puml_files = list(OUT.glob('*.puml'))
    if not puml_files:
        print('No .puml files to render')
        return
    print(f'Rendering {len(puml_files)} diagrams...')
    for f in puml_files:
        run(['plantuml', '-tsvg', str(f)], check=False)
        run(['plantuml', '-tpng', str(f)], check=False)

if __name__ == '__main__':
    run_generators()
    render()
    print('Done.')
