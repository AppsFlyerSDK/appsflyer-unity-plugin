#!/usr/bin/env bash
# Fetch Jira issues for a fixVersion and write multiline tickets to GITHUB_OUTPUT.
# Requires: JIRA_FIX_VERSION, optional JIRA_EMAIL, JIRA_TOKEN, JIRA_DOMAIN (default appsflyer.atlassian.net)

set -euo pipefail

if [[ -z "${JIRA_FIX_VERSION:-}" ]]; then
  echo "::error::JIRA_FIX_VERSION is required"
  exit 1
fi

# Jira fix versions are always stripped semver (e.g. "Unity SDK v6.18.0"), never -rcN.
if [[ "$JIRA_FIX_VERSION" =~ ^(Unity SDK v[0-9]+\.[0-9]+\.[0-9]+)-rc[0-9]+$ ]]; then
  JIRA_FIX_VERSION="${BASH_REMATCH[1]}"
  echo "Normalized Jira fixVersion (stripped -rc suffix): $JIRA_FIX_VERSION"
fi

if [[ -z "${JIRA_EMAIL:-}" || -z "${JIRA_TOKEN:-}" ]]; then
  echo "Jira credentials not configured; skipping ticket fetch"
  echo "tickets=No assigned fix version found" >> "${GITHUB_OUTPUT:?GITHUB_OUTPUT not set}"
  exit 0
fi

DOMAIN="${JIRA_DOMAIN:-appsflyer.atlassian.net}"
echo "Looking for Jira tickets with fixVersion: $JIRA_FIX_VERSION"

JQL_QUERY="fixVersion=\"${JIRA_FIX_VERSION}\""
ENCODED_JQL=$(echo "$JQL_QUERY" | jq -sRr @uri)

RESPONSE=$(curl -s -w "\n%{http_code}" \
  -u "${JIRA_EMAIL}:${JIRA_TOKEN}" \
  -H "Accept: application/json" \
  -H "Content-Type: application/json" \
  "https://${DOMAIN}/rest/api/3/search/jql?jql=${ENCODED_JQL}&fields=key,summary&maxResults=20")

HTTP_CODE=$(echo "$RESPONSE" | tail -n1)
BODY=$(echo "$RESPONSE" | sed '$d')
echo "Jira HTTP $HTTP_CODE"

if [[ "$HTTP_CODE" != "200" ]]; then
  echo "Jira API request failed; first 500 bytes of body:"
  echo "$BODY" | head -c 500
  echo "tickets=No assigned fix version found" >> "$GITHUB_OUTPUT"
  exit 0
fi

TICKETS=$(echo "$BODY" | jq -r '.issues[]? | "• https://'"${DOMAIN}"'/browse/\(.key) - \(.fields.summary)"' 2>/dev/null | head -10)
if [[ -z "$TICKETS" ]]; then
  echo "tickets=No assigned fix version found" >> "$GITHUB_OUTPUT"
else
  {
    echo "tickets<<EOF"
    echo "$TICKETS"
    echo "EOF"
  } >> "$GITHUB_OUTPUT"
fi
