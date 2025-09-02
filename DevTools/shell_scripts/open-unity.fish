#!/usr/bin/env fish

# Unity Direct Opener Script
# Opens Unity directly with the current project directory, bypassing Unity Hub
# Usage: ./open-unity.fish

# Function to find Unity project root (directory containing 'Assets')
function find_project_root
    set current_dir $argv[1]
    while test "$current_dir" != "/"
        if test -d "$current_dir/Assets"
            echo "$current_dir"
            return 0
        end
        set current_dir (dirname "$current_dir")
    end
    return 1
end

# Auto-detect project root
set PROJECT_PATH (find_project_root (pwd))

if test -z "$PROJECT_PATH"
    echo "Error: Could not find Unity project root (no 'Assets' folder found)."
    exit 1
end

# Unity executable path (adjust version if needed)
set UNITY_PATH "/Applications/Unity/Hub/Editor/2022.3.37f1/Unity.app/Contents/MacOS/Unity"

echo "Opening Unity with project: $PROJECT_PATH"
echo "Unity path: $UNITY_PATH"

# Check if Unity executable exists
if not test -f $UNITY_PATH
    echo "Error: Unity executable not found at $UNITY_PATH"
    echo "Please check your Unity installation and update the path in this script."
    exit 1
end

# Open Unity with the project
echo "Launching Unity..."
echo "Unity is currently running. Press Ctrl+C to close this terminal (Unity will remain open)."
$UNITY_PATH -projectPath $PROJECT_PATH

echo "Unity closed."
