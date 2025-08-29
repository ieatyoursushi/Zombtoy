#!/bin/bash
# lint-code.sh: Lint C# and TypeScript code for quality and consistency
# Usage: ./lint-code.sh

echo "Starting code linting..."

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
    cd ../../../..
else
    echo "✓ No TypeScript backend found to lint"
fi

echo ""
echo "Linting complete!"
