"""Small deterministic C# source scanner; not a compiler or Unity wiring reader."""
import re
from pathlib import Path
from typing import List

CS_EXTENSIONS = {'.cs'}
# Match strings before comment markers, so URLs and escaped quotes cannot eat code.
NON_CODE_RE = re.compile(
    r'(?:\$@|@\$|@)"(?:[^"]|"")*"|\$?"(?:\\.|[^"\\])*"'
    r"|'(?:\\.|[^'\\])*'|//[^\n]*|/\*[\s\S]*?\*/"
)
CLASS_DECL_RE = re.compile(r'\b(class|interface|struct)\s+([A-Za-z_]\w*)')
EVENT_DECL_RE = re.compile(r'\bpublic\s+static\s+event\s+([^;]+?)\s+([A-Za-z_]\w*)\s*;')
EVENT_SUBSCRIBE_RE = re.compile(r'\bGameEvents\s*\.\s*(\w+)\s*\+=\s*([\w.]+)')
EVENT_UNSUBSCRIBE_RE = re.compile(r'\bGameEvents\s*\.\s*(\w+)\s*-=\s*([\w.]+)')
EVENT_RAISE_RE = re.compile(r'\bGameEvents\s*\.\s*(\w+)\s*\?\.\s*Invoke\s*\(')
EVENT_CALL_RE = re.compile(r'\bGameEvents\s*\.\s*(\w+)\s*\(')
METHOD_RE = re.compile(r'\bpublic\s+static\s+void\s+(\w+)\s*\([^)]*\)\s*(=>|\{)')
LOCAL_RAISE_RE = re.compile(r'\b(\w+)\s*(?:\?\.|\.)\s*Invoke\s*\(')
LIFECYCLE_RE = re.compile(r'\bvoid\s+(Awake|Start|OnEnable|OnDisable|OnDestroy)\s*\(\s*\)')
STATIC_ANALYSIS_NOTE = 'Static source analysis only; scene/prefab wiring, dynamic dispatch and runtime execution order are not inferred.'


def read_cs_files(root: Path) -> List[Path]:
    return sorted(root.rglob('*.cs'))


def strip_code(code: str) -> str:
    def replace(match):
        value = match.group()
        if value.startswith(('//', '/*')):
            return ''.join('\n' if c == '\n' else ' ' for c in value)
        return '""' + '\n' * value.count('\n')
    return NON_CODE_RE.sub(replace, code)


def load_and_strip(path: Path) -> str:
    # An unreadable source file must fail generation, not masquerade as empty code.
    return strip_code(path.read_text(encoding='utf-8-sig'))


def rel(path: Path, root: Path) -> str:
    try:
        return path.relative_to(root).as_posix()
    except ValueError:
        return str(path)


def method_bodies(code):
    """Yield supported public static void method names and bounded bodies."""
    for match in METHOD_RE.finditer(code):
        start = match.end()
        if match.group(2) == '=>':
            end = code.find(';', start)
            if end != -1:
                yield match.group(1), code[start:end]
        else:
            depth = 1
            for end in range(start, len(code)):
                depth += (code[end] == '{') - (code[end] == '}')
                if depth == 0:
                    yield match.group(1), code[start:end]
                    break


def scan_events(sources, root):
    """Two-pass GameEvents census shared by both reports. Sources are stripped."""
    events, wrappers, classes = {}, {}, {}
    for path, code in sorted(sources.items()):
        if path.name != 'GameEvents.cs':
            continue
        for match in EVENT_DECL_RE.finditer(code):
            events[match.group(2)] = {
                'type': ' '.join(match.group(1).split()), 'trigger_method': None,
                'publishers': [], 'subscribers': [], 'issues': []}
        for name, body in method_bodies(code):
            targets = {m.group(1) for m in LOCAL_RAISE_RE.finditer(body)} & events.keys()
            if targets:
                wrappers[name] = targets
                for event in targets:
                    events[event]['trigger_method'] = name

    for path, code in sorted(sources.items()):
        match = re.search(r'\bclass\s+(\w+)', code)
        name = match.group(1) if match else path.stem
        subscribed = sorted({m.groups() for m in EVENT_SUBSCRIBE_RE.finditer(code) if m.group(1) in events})
        unsubscribed = sorted({m.groups() for m in EVENT_UNSUBSCRIBE_RE.finditer(code) if m.group(1) in events})
        published = {m.group(1) for m in EVENT_RAISE_RE.finditer(code) if m.group(1) in events}
        for match in EVENT_CALL_RE.finditer(code):
            published.update(wrappers.get(match.group(1), set()))
        classes[name] = {
            'file': rel(path, root), 'lifecycle_methods': sorted(set(LIFECYCLE_RE.findall(code))),
            'subscribes_to': subscribed, 'unsubscribes_from': unsubscribed,
            'publishes': sorted(published), 'issues': []}
        for event in published:
            events[event]['publishers'].append(name)
        for event, handler in subscribed:
            events[event]['subscribers'].append((name, handler))
    for data in events.values():
        data['publishers'] = sorted(set(data['publishers']))
        data['subscribers'] = sorted(set(data['subscribers']))
    return dict(sorted(events.items())), dict(sorted(classes.items()))
