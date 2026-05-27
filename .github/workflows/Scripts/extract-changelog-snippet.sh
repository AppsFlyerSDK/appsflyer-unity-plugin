#!/usr/bin/env bash
# Extract changelog bullets for PLUGIN_VERSION into GITHUB_OUTPUT body (multiline EOF).
# Safe defaults per appsflyer-capacitor-plugin PR #201 (no inverted if/else on missing file).

set -eo pipefail

PLUGIN_VERSION="${PLUGIN_VERSION:?PLUGIN_VERSION is required}"
CHANGES=""

if [[ -f CHANGELOG.md ]]; then
  CHANGES=$(awk "/^## ${PLUGIN_VERSION}/,/^## [0-9]/" CHANGELOG.md | grep '^- ' | sed 's/^- /• /' | head -8 || true)
fi

if [[ -z "$CHANGES" ]]; then
  CHANGES="• No changelog bullets found for ${PLUGIN_VERSION}; see linked tickets below."
else
  echo "Found changelog bullets for ${PLUGIN_VERSION}."
fi

{
  echo "body<<EOF"
  echo "$CHANGES"
  echo "EOF"
} >> "${GITHUB_OUTPUT:?GITHUB_OUTPUT not set}"
