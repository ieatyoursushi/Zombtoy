#!/usr/bin/env python3
"""
Generate Core architecture visualization showing the relationship between
Core components, Managers, and their integration patterns.
"""
import re
from pathlib import Path
from collections import defaultdict
from common import read_cs_files, load_and_strip, rel

ROOT = Path(__file__).resolve().parents[2] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'
OUT_DIR.mkdir(parents=True, exist_ok=True)

# Core architecture patterns
SINGLETON_INHERIT_RE = re.compile(r'class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*Singleton<([^>]+)>')
MANAGER_RE = re.compile(r'class\s+([A-Za-z_][A-Za-z0-9_]*Manager)')
CORE_COMPONENT_RE = re.compile(r'class\s+(GameEvents|GameStarter|GameStateManager|ComponentCache|Singleton)')
MONOBEHAVIOUR_RE = re.compile(r'class\s+([A-Za-z_][A-Za-z0-9_]*)\s*:\s*MonoBehaviour')
AWAKE_START_RE = re.compile(r'void\s+(Awake|Start|OnEnable|OnDisable|OnDestroy)\s*\(')
DEPENDENCY_INJECT_RE = re.compile(r'\[SerializeField\]\s*(?:private\s+)?([A-Za-z_][A-Za-z0-9_]*)\s+([a-zA-Z_][a-zA-Z0-9_]*)')
FIND_OBJECT_RE = re.compile(r'FindObjectOfType<([^>]+)>')

class CoreAnalysis:
    def __init__(self):
        self.core_components = {}      # component_name -> details
        self.managers = {}             # manager_name -> details  
        self.singletons = {}          # singleton_name -> details
        self.dependencies = defaultdict(set)  # class_name -> set of dependencies
        self.lifecycle_usage = defaultdict(set)  # class_name -> set of lifecycle methods
        
    def analyze_file(self, file_path):
        code = load_and_strip(file_path)
        if not code:
            return
        
        # Extract class name first
        class_match = re.search(r'class\s+([A-Za-z_][A-Za-z0-9_]*)', code)
        if not class_match:
            return
        class_name = class_match.group(1)
            
        # Determine file category  
        path_str = str(file_path.resolve())
        is_core = '/Core/' in path_str or 'GameEvents' in class_name
        is_manager = '/Managers/' in path_str or class_name.endswith('Manager')
        
        # Basic class info
        class_info = {
            'file': rel(file_path, ROOT),
            'category': 'Core' if is_core else ('Manager' if is_manager else 'Other'),
            'is_singleton': False,
            'is_monobehaviour': bool(MONOBEHAVIOUR_RE.search(code)),
            'dependencies': [],
            'lifecycle_methods': [],
            'patterns': []
        }
        
        # Check for singleton pattern
        singleton_match = SINGLETON_INHERIT_RE.search(code)
        if singleton_match:
            class_info['is_singleton'] = True
            class_info['singleton_type'] = singleton_match.group(2)
            self.singletons[class_name] = class_info
        
        # Find lifecycle methods
        for match in AWAKE_START_RE.finditer(code):
            method = match.group(1)
            class_info['lifecycle_methods'].append(method)
            self.lifecycle_usage[class_name].add(method)
        
        # Find serialized dependencies
        for match in DEPENDENCY_INJECT_RE.finditer(code):
            dep_type = match.group(1)
            dep_name = match.group(2)
            class_info['dependencies'].append((dep_type, dep_name))
            self.dependencies[class_name].add(dep_type)
        
        # Find runtime dependencies (FindObjectOfType)
        for match in FIND_OBJECT_RE.finditer(code):
            dep_type = match.group(1)
            class_info['dependencies'].append((dep_type, 'runtime_lookup'))
            self.dependencies[class_name].add(dep_type)
        
        # Detect patterns
        if 'GameEvents.' in code:
            class_info['patterns'].append('Uses GameEvents')
        if 'DontDestroyOnLoad' in code:
            class_info['patterns'].append('Persistent')
        if '.Instance' in code:
            class_info['patterns'].append('Uses Singletons')
        
        # Categorize
        if is_core or CORE_COMPONENT_RE.search(code):
            self.core_components[class_name] = class_info
        elif is_manager or MANAGER_RE.search(code):
            self.managers[class_name] = class_info
            
def generate_core_architecture_diagram(analysis):
    """Generate PlantUML diagram showing Core architecture."""
    lines = [
        "@startuml",
        "title Core Architecture Overview",
        "skinparam packageStyle rectangle",
        "skinparam backgroundColor #FAFAFA",
        "left to right direction",
        ""
    ]
    
    # Core package
    lines.extend([
        "package \"Core\" as CorePkg {",
    ])
    
    for comp_name, comp_info in analysis.core_components.items():
        color = "#E1F5FE" if comp_info['is_singleton'] else "#F3E5F5"
        stereotype = "<<Singleton>>" if comp_info['is_singleton'] else ""
        lines.append(f'  class {comp_name} {stereotype} {color}')
        
        # Add key methods/properties
        if comp_name == 'GameEvents':
            lines.append(f'  {comp_name} : +static events')
            lines.append(f'  {comp_name} : +static triggers')
        elif comp_info['is_singleton']:
            lines.append(f'  {comp_name} : +Instance')
    
    lines.extend([
        "}",
        ""
    ])
    
    # Managers package
    if analysis.managers:
        lines.extend([
            "package \"Managers\" as MgrPkg {",
        ])
        
        for mgr_name, mgr_info in analysis.managers.items():
            color = "#E8F5E8" if mgr_info['is_singleton'] else "#FFF3E0"
            stereotype = "<<Singleton>>" if mgr_info['is_singleton'] else ""
            lines.append(f'  class {mgr_name} {stereotype} {color}')
            
            if mgr_info['is_singleton']:
                lines.append(f'  {mgr_name} : +Instance')
        
        lines.extend([
            "}",
            ""
        ])
    
    # Show key relationships
    for comp_name, comp_info in {**analysis.core_components, **analysis.managers}.items():
        if 'Uses GameEvents' in comp_info['patterns']:
            lines.append(f"{comp_name} --> GameEvents : uses")
        
        # Show dependencies
        for dep_type, _ in comp_info['dependencies']:
            if dep_type in analysis.core_components or dep_type in analysis.managers:
                lines.append(f"{comp_name} --> {dep_type} : depends on")
    
    # Add legend
    lines.extend([
        "",
        "note top",
        "Legend:",
        "Blue = Core Singleton",
        "Purple = Core Component", 
        "Green = Manager Singleton",
        "Orange = Manager Component",
        "end note",
        ""
    ])
    
    lines.extend([
        "@enduml"
    ])
    
    return '\n'.join(lines)

def generate_lifecycle_flow_diagram(analysis):
    """Generate sequence diagram showing typical lifecycle flow."""
    lines = [
        "@startuml",
        "title Core Component Lifecycle Flow",
        "skinparam backgroundColor #FAFAFA",
        ""
    ]
    
    # Participants
    participants = []
    for comp_name in analysis.core_components.keys():
        if comp_name != 'GameEvents':  # GameEvents is static
            participants.append(comp_name)
    
    for mgr_name in list(analysis.managers.keys())[:3]:  # Limit to first 3 managers
        participants.append(mgr_name)
    
    for participant in participants:
        lines.append(f"participant {participant}")
    
    lines.extend([
        "",
        "== Initialization Phase ==",
        ""
    ])
    
    # Show typical lifecycle flow
    for participant in participants:
        info = analysis.core_components.get(participant) or analysis.managers.get(participant)
        if info and 'Awake' in info['lifecycle_methods']:
            lines.append(f"activate {participant}")
            lines.append(f"{participant} -> {participant} : Awake()")
            
            if info['is_singleton']:
                lines.append(f"{participant} -> {participant} : Initialize Singleton")
            
            if 'Uses GameEvents' in info['patterns']:
                lines.append(f"{participant} -> GameEvents : Subscribe to events")
    
    lines.extend([
        "",
        "== Runtime Phase ==",
        ""
    ])
    
    # Show event flow example
    lines.extend([
        "EnemyManager -> GameEvents : EnemySpawned(enemy)",
        "GameEvents -> EnemyManager : OnEnemySpawned callback",
        "GameEvents -> ScoreManager : OnEnemySpawned callback (if subscribed)",
        ""
    ])
    
    lines.extend([
        "== Cleanup Phase ==",
        ""
    ])
    
    for participant in participants:
        info = analysis.core_components.get(participant) or analysis.managers.get(participant)
        if info and 'OnDestroy' in info['lifecycle_methods']:
            if 'Uses GameEvents' in info['patterns']:
                lines.append(f"{participant} -> GameEvents : Unsubscribe from events")
            lines.append(f"deactivate {participant}")
    
    lines.extend([
        "@enduml"
    ])
    
    return '\n'.join(lines)

def generate_core_report(analysis):
    """Generate detailed Core architecture report."""
    lines = [
        "# Core Architecture Report",
        f"Generated: {Path(__file__).name}",
        "",
        "## Overview",
        f"- Core Components: {len(analysis.core_components)}",
        f"- Managers: {len(analysis.managers)}",  
        f"- Singletons: {len(analysis.singletons)}",
        "",
        "## Core Components",
        ""
    ]
    
    for comp_name, comp_info in sorted(analysis.core_components.items()):
        lines.extend([
            f"### {comp_name}",
            f"- **File**: `{comp_info['file']}`",
            f"- **Type**: {'Singleton' if comp_info['is_singleton'] else 'Static' if comp_name == 'GameEvents' else 'Component'}",
            f"- **MonoBehaviour**: {'Yes' if comp_info['is_monobehaviour'] else 'No'}",
            f"- **Lifecycle Methods**: {', '.join(comp_info['lifecycle_methods']) or 'None'}",
        ])
        
        if comp_info['dependencies']:
            lines.append(f"- **Dependencies**: {', '.join(dep[0] for dep in comp_info['dependencies'])}")
        
        if comp_info['patterns']:
            lines.append(f"- **Patterns**: {', '.join(comp_info['patterns'])}")
        
        lines.append("")
    
    lines.extend([
        "## Managers",
        ""
    ])
    
    for mgr_name, mgr_info in sorted(analysis.managers.items()):
        lines.extend([
            f"### {mgr_name}",
            f"- **File**: `{mgr_info['file']}`",
            f"- **Type**: {'Singleton' if mgr_info['is_singleton'] else 'Component'}",
            f"- **Lifecycle Methods**: {', '.join(mgr_info['lifecycle_methods']) or 'None'}",
        ])
        
        if mgr_info['dependencies']:
            lines.append(f"- **Dependencies**: {', '.join(dep[0] for dep in mgr_info['dependencies'])}")
        
        if mgr_info['patterns']:
            lines.append(f"- **Patterns**: {', '.join(mgr_info['patterns'])}")
        
        lines.append("")
    
    lines.extend([
        "## Architectural Patterns",
        "",
        "### Singleton Usage",
    ])
    
    for singleton_name, singleton_info in sorted(analysis.singletons.items()):
        lines.append(f"- **{singleton_name}**: `{singleton_info.get('singleton_type', 'self')}`")
    
    lines.extend([
        "",
        "### Event Bus Integration",
        ""
    ])
    
    event_users = []
    for comp_name, comp_info in {**analysis.core_components, **analysis.managers}.items():
        if 'Uses GameEvents' in comp_info['patterns']:
            event_users.append(comp_name)
    
    for user in event_users:
        lines.append(f"- **{user}**: Integrates with GameEvents")
    
    return '\n'.join(lines)

def main():
    analysis = CoreAnalysis()
    
    # Analyze all C# files
    for file_path in read_cs_files(ROOT):
        analysis.analyze_file(file_path)
    
    # Generate outputs
    arch_diagram = generate_core_architecture_diagram(analysis)
    lifecycle_diagram = generate_lifecycle_flow_diagram(analysis)
    report = generate_core_report(analysis)
    
    # Write files
    (OUT_DIR / 'core_architecture.puml').write_text(arch_diagram, encoding='utf-8')
    (OUT_DIR / 'core_lifecycle_flow.puml').write_text(lifecycle_diagram, encoding='utf-8')
    (OUT_DIR / 'core_architecture_report.md').write_text(report, encoding='utf-8')
    
    print(f"Generated core architecture diagram: {OUT_DIR / 'core_architecture.puml'}")
    print(f"Generated lifecycle flow diagram: {OUT_DIR / 'core_lifecycle_flow.puml'}")
    print(f"Generated architecture report: {OUT_DIR / 'core_architecture_report.md'}")

if __name__ == '__main__':
    main()
