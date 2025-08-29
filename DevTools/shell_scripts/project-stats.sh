#!/bin/bash
# project-stats.sh: Show lines of code, file counts
echo "Project Statistics:"
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
