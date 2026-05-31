#!/usr/bin/env bash
# Extract changelog bullets for PLUGIN_VERSION into GITHUB_OUTPUT body (multiline EOF).
# Unity CHANGELOG.md uses "## vX.Y.Z" headers and "*" bullet lines.
# Safe defaults per appsflyer-capacitor-plugin PR #201 (no inverted if/else on missing file).

set -eo pipefail

PLUGIN_VERSION="${PLUGIN_VERSION:?PLUGIN_VERSION is required}"
VERSION="${PLUGIN_VERSION#v}"
VERSION="${VERSION%-rc*}"

CHANGES=""
if [[ -f CHANGELOG.md ]]; then
  CHANGES=$(awk -v ver="v${VERSION}" '
    $0 == "## " ver { found=1; next }
    found && /^## / { exit }
    found && /^\* / { sub(/^\* /, "• "); print }
    found && /^- / { sub(/^- /, "• "); print }
  ' CHANGELOG.md | head -8 || true)
fi

if [[ -z "$CHANGES" ]]; then
  CHANGES="• No changelog bullets found for v${VERSION}; see linked tickets below."
else
  echo "Found changelog bullets for v${VERSION}."
fi

{
  echo "body<<EOF"
  echo "$CHANGES"
  echo "EOF"
} >> "${GITHUB_OUTPUT:?GITHUB_OUTPUT not set}"
