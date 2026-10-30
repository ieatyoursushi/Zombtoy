#!/usr/bin/env bash
# Usage: validate-unity.sh [assets|build]; close this checkout's Editor first.
set -euo pipefail
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
EDITOR_PATH="${UNITY_EDITOR_PATH:-/Applications/Unity/Hub/Editor/2022.3.37f1/Unity.app/Contents/MacOS/Unity}"
OUTPUT_DIR="${ZOMBTOY_VALIDATION_DIR:-/private/tmp/zombtoy-validation}"
case "${1:-assets}" in
  assets) METHOD=ZombtoyValidation.ValidateProject ;;
  build) METHOD=ZombtoyValidation.BuildMacSmokePlayer ;;
  *) printf 'Usage: %s [assets|build]\n' "$0" >&2; exit 2 ;;
esac
mkdir -p "$OUTPUT_DIR"
export ZOMBTOY_VALIDATION_DIR="$OUTPUT_DIR"
"$EDITOR_PATH" -batchmode -projectPath "$PROJECT_DIR" -executeMethod "$METHOD" -logFile "$OUTPUT_DIR/${1:-assets}.log"
# Unity sometimes exits zero on errors during import; require our positive marker too.
if ! rg -q '\[Validation\] PASS:' "$OUTPUT_DIR/${1:-assets}.log"; then
  printf 'Validation did not finish: %s\n' "$OUTPUT_DIR/${1:-assets}.log" >&2
  exit 1
fi
