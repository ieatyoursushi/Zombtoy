#!/usr/bin/env python3
"""Generate current source reports, failing on any generator/render error."""
from subprocess import run, CalledProcessError
from pathlib import Path
import argparse
import shutil
import sys

ROOT = Path(__file__).resolve().parent
OUT = ROOT / 'out'
SCRIPTS = ['generate_event_flow.py', 'generate_class_dependency.py',
           'generate_call_graph.py', 'generate_gameevents_debug.py',
           'generate_core_architecture.py']
DIAGRAMS = ['event_flow', 'class_dependency', 'call_graph', 'gameevents_health',
            'core_architecture', 'core_lifecycle_flow']


def run_generators():
    for script in SCRIPTS:
        run([sys.executable, '-B', str(ROOT / script)], check=True)


def render():
    if not shutil.which('plantuml'):
        raise RuntimeError('PlantUML missing; install it or explicitly use --no-render')
    for name in DIAGRAMS:
        source = OUT / (name + '.puml')
        for kind in ('svg', 'png'):
            destination = OUT / (name + '.' + kind)
            # Never leave a stale rendered image behind after a rendering failure.
            destination.unlink(missing_ok=True)
            run(['plantuml', '-failfast2', '-t' + kind, str(source)], check=True)
            if not destination.is_file() or destination.stat().st_size == 0:
                raise RuntimeError(f'Render did not create {destination}')


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--no-render', action='store_true')
    args = parser.parse_args()
    OUT.mkdir(exist_ok=True)
    try:
        run_generators()
        if not args.no_render:
            render()
    except (CalledProcessError, OSError, RuntimeError) as error:
        print(f'Generation failed: {error}', file=sys.stderr)
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())
