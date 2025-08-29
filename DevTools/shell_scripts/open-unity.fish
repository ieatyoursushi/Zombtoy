#!/usr/bin/env fish

# Unity Direct Opener Script
# Opens Unity directly with the current project directory, bypassing Unity Hub
# Usage: ./open-unity.fish

# Unity executable path (adjust version if needed)
set UNITY_PATH "/Applications/Unity/Hub/Editor/2022.3.37f1/Unity.app/Contents/MacOS/Unity"

# Get current directory (project root)
set PROJECT_PATH (pwd)

echo "Opening Unity with project: $PROJECT_PATH"
echo "Unity path: $UNITY_PATH"

# Check if Unity executable exists
if not test -f $UNITY_PATH
    echo "Error: Unity executable not found at $UNITY_PATH"
    echo "Please check your Unity installation and update the path in this script."
    exit 1
end

# Check if we're in a Unity project (basic check for Assets folder)
if not test -d "Assets"
    echo "Warning: No 'Assets' folder found. Are you in a Unity project directory?"
    echo "Current directory: $PROJECT_PATH"
    read -P "Continue anyway? (y/N): " -n 1 response
    if test "$response" != "y" -a "$response" != "Y"
        echo "Aborted."
        exit 1
    end
end

# Open Unity with the project
echo "Launching Unity..."
echo "Unity is currently running. Press Ctrl+C to close this terminal (Unity will remain open)."
$UNITY_PATH -projectPath $PROJECT_PATH

echo "Unity closed."
