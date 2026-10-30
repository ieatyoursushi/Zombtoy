#!/usr/bin/env python3
"""Run scanner regression tests; optionally regenerate/render all outputs."""
import argparse
from pathlib import Path
import subprocess
import sys


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--generate', action='store_true')
    args = parser.parse_args()
    root = Path(__file__).resolve().parent
    result = subprocess.run([sys.executable, '-B', str(root / 'test_diagrams.py')])
    if result.returncode:
        return result.returncode
    if args.generate:
        return subprocess.run([sys.executable, '-B', str(root / 'generate_all.py')]).returncode
    return 0


if __name__ == '__main__':
    sys.exit(main())
