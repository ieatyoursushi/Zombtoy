#!/usr/bin/env python3
"""
Generate GameEvents debugging report and visualization.
Shows event usage patterns, potential issues, and subscription health.
"""
import re
from pathlib import Path
from collections import defaultdict, Counter
from common import read_cs_files, load_and_strip, rel

ROOT = Path(__file__).resolve().parents[2] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'
OUT_DIR.mkdir(parents=True, exist_ok=True)

# Enhanced regex patterns for GameEvents analysis
EVENT_DECL_RE = re.compile(r'public\s+static\s+event\s+([^;]+?)\s+([A-Za-z_][A-Za-z0-9_]*)\s*;')
EVENT_TRIGGER_RE = re.compile(r'public\s+static\s+void\s+([A-Za-z_][A-Za-z0-9_]*)\s*\([^)]*\)')
EVENT_SUBSCRIBE_RE = re.compile(r'GameEvents\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\+=\s*([A-Za-z_][A-Za-z0-9_]*)')
EVENT_UNSUBSCRIBE_RE = re.compile(r'GameEvents\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\-=\s*([A-Za-z_][A-Za-z0-9_]*)')
EVENT_INVOKE_RE = re.compile(r'GameEvents\s*\.\s*([A-Za-z_][A-Za-z0-9_]*)\s*\([^)]*\)')
LIFECYCLE_METHOD_RE = re.compile(r'void\s+(Awake|Start|OnEnable|OnDisable|OnDestroy)\s*\(\s*\)')

class EventAnalysis:
    def __init__(self):
        self.events = {}  # event_name -> {type, trigger_method, subscribers, publishers, issues}
        self.classes = {}  # class_name -> {file, lifecycle_methods, event_interactions}
        self.issues = []   # potential problems
        
    def analyze_file(self, file_path):
        code = load_and_strip(file_path)
        if not code:
            return
            
        # Extract class name
        class_match = re.search(r'class\s+([A-Za-z_][A-Za-z0-9_]*)', code)
        class_name = class_match.group(1) if class_match else file_path.stem
        
        # Initialize class data
        if class_name not in self.classes:
            self.classes[class_name] = {
                'file': rel(file_path, ROOT),
                'lifecycle_methods': [],
                'subscribes_to': [],
                'unsubscribes_from': [],
                'publishes': [],
                'issues': []
            }
        
        class_data = self.classes[class_name]
        
        # Find lifecycle methods
        for match in LIFECYCLE_METHOD_RE.finditer(code):
            class_data['lifecycle_methods'].append(match.group(1))
        
        # Find event declarations (only in GameEvents.cs)
        if 'GameEvents' in file_path.name:
            for match in EVENT_DECL_RE.finditer(code):
                event_type = match.group(1).strip()
                event_name = match.group(2)
                self.events[event_name] = {
                    'type': event_type,
                    'trigger_method': None,
                    'subscribers': [],
                    'publishers': [],
                    'issues': []
                }
            
            # Find trigger methods
            for match in EVENT_TRIGGER_RE.finditer(code):
                trigger_name = match.group(1)
                # Match trigger to event (assume OnEventName -> EventName pattern)
                event_name = f"On{trigger_name}" if not trigger_name.startswith('On') else trigger_name
                if event_name in self.events:
                    self.events[event_name]['trigger_method'] = trigger_name
        
        # Find subscriptions
        for match in EVENT_SUBSCRIBE_RE.finditer(code):
            event_name = match.group(1)
            handler_name = match.group(2)
            class_data['subscribes_to'].append((event_name, handler_name))
            
            if event_name in self.events:
                self.events[event_name]['subscribers'].append((class_name, handler_name))
        
        # Find unsubscriptions
        for match in EVENT_UNSUBSCRIBE_RE.finditer(code):
            event_name = match.group(1)
            handler_name = match.group(2)
            class_data['unsubscribes_from'].append((event_name, handler_name))
        
        # Find event invocations (publishers)
        for match in EVENT_INVOKE_RE.finditer(code):
            trigger_name = match.group(1)
            class_data['publishes'].append(trigger_name)
            
            # Find corresponding event
            event_name = f"On{trigger_name}" if not trigger_name.startswith('On') else trigger_name
            if event_name in self.events:
                self.events[event_name]['publishers'].append(class_name)
    
    def detect_issues(self):
        """Detect potential issues in event usage patterns."""
        
        # Check for events with no subscribers
        for event_name, event_data in self.events.items():
            if not event_data['subscribers']:
                self.issues.append(f"⚠️  Event '{event_name}' has no subscribers (dead event)")
            
            # Check for events with no publishers
            if not event_data['publishers']:
                self.issues.append(f"⚠️  Event '{event_name}' is never fired (unused event)")
        
        # Check for subscription/unsubscription mismatches
        for class_name, class_data in self.classes.items():
            subscribed = {event for event, _ in class_data['subscribes_to']}
            unsubscribed = {event for event, _ in class_data['unsubscribes_from']}
            
            missing_unsub = subscribed - unsubscribed
            if missing_unsub:
                self.issues.append(f"🔥 Class '{class_name}' subscribes to {missing_unsub} but never unsubscribes (memory leak risk)")
            
            extra_unsub = unsubscribed - subscribed
            if extra_unsub:
                self.issues.append(f"❓ Class '{class_name}' unsubscribes from {extra_unsub} but never subscribes")
        
        # Check for lifecycle method patterns
        for class_name, class_data in self.classes.items():
            if class_data['subscribes_to']:
                lifecycle_methods = set(class_data['lifecycle_methods'])
                
                # Should have OnEnable/OnDisable or Start/OnDestroy pattern
                if 'OnEnable' in lifecycle_methods and 'OnDisable' not in lifecycle_methods:
                    class_data['issues'].append("Missing OnDisable() for OnEnable() subscription pattern")
                
                if 'Start' in lifecycle_methods and 'OnDestroy' not in lifecycle_methods:
                    class_data['issues'].append("Missing OnDestroy() for Start() subscription pattern")
                
                if not any(method in lifecycle_methods for method in ['OnEnable', 'Start', 'Awake']):
                    class_data['issues'].append("Subscriptions found but no clear lifecycle method for setup")

def generate_debug_report(analysis):
    """Generate detailed debugging report."""
    lines = [
        "# GameEvents Debug Report",
        f"Generated: {Path(__file__).name}",
        "",
        "## Summary",
        f"- Total Events: {len(analysis.events)}",
        f"- Classes with Event Interactions: {len([c for c in analysis.classes.values() if c['subscribes_to'] or c['publishes']])}",
        f"- Issues Found: {len(analysis.issues)}",
        ""
    ]
    
    # Issues section
    if analysis.issues:
        lines.extend([
            "## 🚨 Issues Detected",
            ""
        ])
        for issue in analysis.issues:
            lines.append(f"- {issue}")
        lines.append("")
    
    # Events overview
    lines.extend([
        "## 📡 Events Overview",
        ""
    ])
    
    for event_name, event_data in sorted(analysis.events.items()):
        lines.extend([
            f"### {event_name}",
            f"- **Type**: `{event_data['type']}`",
            f"- **Trigger Method**: `{event_data['trigger_method'] or 'None'}`",
            f"- **Publishers**: {len(event_data['publishers'])} ({', '.join(event_data['publishers'])})",
            f"- **Subscribers**: {len(event_data['subscribers'])} ({', '.join(f'{cls}.{method}' for cls, method in event_data['subscribers'])})",
            ""
        ])
        
        if event_data['issues']:
            lines.append("  **Issues**:")
            for issue in event_data['issues']:
                lines.append(f"  - {issue}")
            lines.append("")
    
    # Class interactions
    lines.extend([
        "## 🏗️ Class Event Interactions",
        ""
    ])
    
    for class_name, class_data in sorted(analysis.classes.items()):
        if not (class_data['subscribes_to'] or class_data['publishes']):
            continue
            
        lines.extend([
            f"### {class_name}",
            f"- **File**: `{class_data['file']}`",
            f"- **Lifecycle Methods**: {', '.join(class_data['lifecycle_methods']) or 'None'}",
        ])
        
        if class_data['subscribes_to']:
            lines.append(f"- **Subscribes To**: {', '.join(f'{event}({handler})' for event, handler in class_data['subscribes_to'])}")
        
        if class_data['publishes']:
            lines.append(f"- **Publishes**: {', '.join(class_data['publishes'])}")
        
        if class_data['issues']:
            lines.append("- **Issues**:")
            for issue in class_data['issues']:
                lines.append(f"  - ⚠️ {issue}")
        
        lines.append("")
    
    return '\n'.join(lines)

def generate_health_visualization(analysis):
    """Generate PlantUML health visualization."""
    lines = [
        "@startuml",
        "title GameEvents Health Check",
        "skinparam backgroundColor #FAFAFA",
        ""
    ]
    
    # Color code events by health
    for event_name, event_data in analysis.events.items():
        color = "#90EE90"  # Light green - healthy
        
        if not event_data['subscribers']:
            color = "#FFB6C1"  # Light red - no subscribers
        elif not event_data['publishers']:
            color = "#FFE4B5"  # Light orange - never fired
        elif len(event_data['subscribers']) == 1 and len(event_data['publishers']) > 3:
            color = "#ADD8E6"  # Light blue - potential bottleneck
        
        lines.append(f'rectangle "{event_name}\\n{len(event_data["publishers"])}→{len(event_data["subscribers"])}" as {event_name} {color}')
    
    lines.extend([
        "",
        "note top",
        "Legend:",
        "Green = Healthy (has publishers & subscribers)",
        "Red = Dead (no subscribers)", 
        "Orange = Unused (never fired)",
        "Blue = Potential bottleneck (1 subscriber, many publishers)",
        "end note",
        "",
        "@enduml"
    ])
    
    return '\n'.join(lines)

def main():
    analysis = EventAnalysis()
    
    # Analyze all C# files
    for file_path in read_cs_files(ROOT):
        analysis.analyze_file(file_path)
    
    # Detect issues
    analysis.detect_issues()
    
    # Generate outputs
    debug_report = generate_debug_report(analysis)
    health_viz = generate_health_visualization(analysis)
    
    # Write files
    (OUT_DIR / 'gameevents_debug_report.md').write_text(debug_report, encoding='utf-8')
    (OUT_DIR / 'gameevents_health.puml').write_text(health_viz, encoding='utf-8')
    
    print(f"Generated GameEvents debug report: {OUT_DIR / 'gameevents_debug_report.md'}")
    print(f"Generated health visualization: {OUT_DIR / 'gameevents_health.puml'}")
    print(f"Found {len(analysis.issues)} potential issues")

if __name__ == '__main__':
    main()
