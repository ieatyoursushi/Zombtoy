#!/bin/bash
# lint-code.sh: Lint C# and TypeScript code for quality and consistency
# Usage: ./lint-code.sh

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

echo "Starting code linting..."
echo "Project root: $PROJECT_ROOT"

# Change to project root for consistent path handling
cd "$PROJECT_ROOT"

# Lint C# files (Unity Assets and Backend)
echo ""
echo "Linting C# code..."
if command -v dotnet &> /dev/null; then
    echo "Using dotnet format for C# files..."
    
    # Find and check .csproj files
    CSPROJ_FILES=$(find . -name "*.csproj" -type f)
    
    if [ -n "$CSPROJ_FILES" ]; then
        for proj in $CSPROJ_FILES; do
            echo "Checking $proj..."
            dotnet format "$proj" --verify-no-changes --verbosity quiet
            if [ $? -eq 0 ]; then
                echo "✓ $proj: No formatting issues found"
            else
                echo "⚠ $proj: Formatting issues detected (run 'dotnet format $proj' to fix)"
            fi
        done
    else
        echo "⚠ No .csproj files found. dotnet format requires project files."
    fi
else
    echo "⚠ dotnet not found. Install .NET SDK to lint C# files."
fi

# Lint TypeScript files (Legacy Node.js backend)
echo ""
echo "Linting TypeScript code..."
if [ -d "Assets/Scripts/Server/zombtoy-backend" ]; then
    cd "Assets/Scripts/Server/zombtoy-backend"
    if [ -f "package.json" ] && command -v npx &> /dev/null; then
        echo "Using ESLint for TypeScript files..."
        if [ -f "eslint.config.js" ] || [ -f ".eslintrc.js" ] || [ -f ".eslintrc.json" ] || [ -f ".eslintrc.yml" ]; then
            if npx eslint . --ext .ts,.js --quiet 2>/dev/null; then
                echo "✓ TypeScript: No linting issues found"
            else
                echo "⚠ TypeScript: Linting issues detected (run 'npx eslint .' to see details)"
            fi
        else
            echo "⚠ No ESLint config found. Create eslint.config.js or .eslintrc.* to enable TypeScript linting"
        fi
    else
        echo "⚠ ESLint not available. Install dependencies with 'npm install' in Assets/Scripts/Server/zombtoy-backend"
    fi
    cd "$PROJECT_ROOT"
else
    echo "✓ No TypeScript backend found to lint"
fi

echo ""
echo "Linting complete!"
