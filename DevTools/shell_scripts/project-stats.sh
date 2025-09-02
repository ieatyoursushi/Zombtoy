#!/bin/bash
# project-stats.sh: Show lines of code, file counts

# Function to find Unity project root (directory containing 'Assets')
find_project_root() {
    local current_dir="$1"
    while [[ "$current_dir" != "/" ]]; do
        if [[ -d "$current_dir/Assets" ]]; then
            echo "$current_dir"
            return 0
        fi
        current_dir="$(dirname "$current_dir")"
    done
    return 1
}

# Auto-detect project root
PROJECT_ROOT=$(find_project_root "$(pwd)")

if [[ -z "$PROJECT_ROOT" ]]; then
    echo "Error: Could not find Unity project root (no 'Assets' folder found)."
    exit 1
fi

echo "Project Statistics:"
echo "Project root: $PROJECT_ROOT"

# Change to project root for consistent path handling
cd "$PROJECT_ROOT"

ASSET_FILES_COUNT=$(find Assets/Scripts -type f -name "*.cs" | wc -l)
echo "C# files: $ASSET_FILES_COUNT"
BACKEND_FILES_COUNT=$(find Backend -type f -name "*.cs" -not -path "*/obj/*" -not -path "*/bin/*" -not -path "*/.vs/*" | wc -l)
echo "Backend files: $BACKEND_FILES_COUNT"
ASSET_LINES=$(find Assets/Scripts -type f -name "*.cs" -exec wc -l {} \; | awk '{sum += $1} END {print sum}')
echo "Total lines in C# assets: $ASSET_LINES"
BACKEND_LINES=$(find Backend -type f -name "*.cs" -not -path "*/obj/*" -not -path "*/bin/*" -not -path "*/.vs/*" -exec wc -l {} \; | awk '{sum += $1} END {print sum}')
echo "Total lines in backend: $BACKEND_LINES"

echo "Total lines in entire project: $(($ASSET_LINES + $BACKEND_LINES))"

echo ""
echo "Backend C# files found:"
find Backend -type f -name "*.cs" -not -path "*/obj/*" -not -path "*/bin/*" -not -path "*/.vs/*"
