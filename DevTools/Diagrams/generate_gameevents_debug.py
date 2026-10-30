#!/usr/bin/env python3
"""
Generate GameEvents debugging report and visualization.
Shows event usage patterns, potential issues, and subscription health.
"""
from pathlib import Path
from common import read_cs_files, load_and_strip, scan_events, STATIC_ANALYSIS_NOTE

ROOT = Path(__file__).resolve().parents[2] / 'Assets' / 'Scripts'
OUT_DIR = Path(__file__).resolve().parent / 'out'


class EventAnalysis:
    def __init__(self):
        self.sources = {}
        self.events, self.classes, self.issues = {}, {}, []

    def analyze_file(self, file_path):
        self.sources[file_path] = load_and_strip(file_path)

    def detect_issues(self):
        self.events, self.classes = scan_events(self.sources, ROOT)
        self.issues = []
        for event, data in self.events.items():
            if not data['subscribers']:
                self.issues.append(f"Event '{event}' has no source subscriptions")
            if not data['publishers']:
                self.issues.append(f"Event '{event}' has no source publishers")
        for name, data in self.classes.items():
            missing = set(data['subscribes_to']) - set(data['unsubscribes_from'])
            extra = set(data['unsubscribes_from']) - set(data['subscribes_to'])
            for event, handler in sorted(missing):
                self.issues.append(f"Review {name}: subscription {event}({handler}) has no matching unsubscription")
            for event, handler in sorted(extra):
                self.issues.append(f"Review {name}: unsubscription {event}({handler}) has no matching subscription")


def generate_debug_report(analysis):
    """Generate detailed debugging report."""
    lines = [
        "# GameEvents Debug Report",
        f"Generated: {Path(__file__).name}", STATIC_ANALYSIS_NOTE,
        "Findings are source-reference observations, not proof of leaks or gameplay activity.",
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
    for event_name, event_data in sorted(analysis.events.items()):
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
        "Green = Source publishers and subscribers found",
        "Red = No source subscribers",
        "Orange = No source publishers",
        "Blue = One subscriber, multiple source publishers (not a performance measurement)",
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
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    (OUT_DIR / 'gameevents_debug_report.md').write_text(debug_report, encoding='utf-8')
    (OUT_DIR / 'gameevents_health.puml').write_text(health_viz, encoding='utf-8')

    print(f"Generated GameEvents debug report: {OUT_DIR / 'gameevents_debug_report.md'}")
    print(f"Generated health visualization: {OUT_DIR / 'gameevents_health.puml'}")
    print(f"Found {len(analysis.issues)} potential issues")

if __name__ == '__main__':
    main()
