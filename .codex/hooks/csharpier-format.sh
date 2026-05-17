#!/usr/bin/env bash
FILE=$(jq -r '.tool_input.file_path' 2>/dev/null || echo "")
if [[ "$FILE" == *.cs ]]; then
  cd "$(dirname "${BASH_SOURCE[0]}")/../.."
  dotnet csharpier format "$FILE" 2>/dev/null || true
fi
