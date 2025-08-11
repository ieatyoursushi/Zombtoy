import re
from pathlib import Path
from typing import Iterable, List

CS_EXTENSIONS = {'.cs'}

COMMENT_SINGLE = re.compile(r'//.*')
COMMENT_MULTI = re.compile(r'/\*.*?\*/', re.DOTALL)
STRING_LITERALS = re.compile(r'@?"(?:""|[^"\n])*"')

CLASS_DECL_RE = re.compile(r'\b(class|interface|struct)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)')
INHERIT_RE = re.compile(r'\bclass\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*([^\{]+)\{')
METHOD_CALL_RE = re.compile(r'\b([A-Z][A-Za-z0-9_]*)\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\(')

EVENT_DECL_RE = re.compile(r'\bpublic\s+static\s+event\s+[A-Za-z0-9_<>]+\s+([A-Za-z_][A-Za-z0-9_]*)')
EVENT_SUBSCRIBE_RE = re.compile(r'GameEvents\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\+=')
EVENT_RAISE_RE = re.compile(r'GameEvents\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\?\.\s*Invoke')

DEF_TYPE_RE = re.compile(r'\b(class|interface)\s+([A-Za-z_][A-Za-z0-9_]*)')
INHERIT_SPLIT_RE = re.compile(r'\s*,\s*')


def read_cs_files(root: Path) -> List[Path]:
    return [p for p in root.rglob('*') if p.suffix in CS_EXTENSIONS]


def strip_code(code: str) -> str:
    code = COMMENT_MULTI.sub('', code)
    code = COMMENT_SINGLE.sub('', code)
    code = STRING_LITERALS.sub('""', code)
    return code


def load_and_strip(path: Path) -> str:
    try:
        return strip_code(path.read_text(encoding='utf-8', errors='ignore'))
    except Exception:
        return ''


def rel(path: Path, root: Path) -> str:
    try:
        return str(path.relative_to(root))
    except Exception:
        return str(path)
