#!/usr/bin/env bash

set -euo pipefail

VERSION=""
MAVEN_BASE="${MAVEN_BASE:-https://repo1.maven.org/maven2}"
WAIT=false
MAX_ATTEMPTS=40
INTERVAL_SEC=30

usage() {
  cat <<EOF
Usage: $(basename "$0") --version <version> [options]

Validates that com.appsflyer:unity-wrapper:<version> exists in Maven Central.

Options:
  --wait                 Poll until the artifact appears or max attempts are reached
  --max-attempts <n>     Max poll attempts when using --wait (default: 40)
  --interval-sec <n>     Seconds between poll attempts (default: 30)
EOF
}

artifact_exists() {
  local url="$MAVEN_BASE/com/appsflyer/unity-wrapper/$VERSION/unity-wrapper-$VERSION.aar"
  curl --fail --silent --show-error --location --head --retry 3 "$url" >/dev/null 2>&1
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="$2"
      shift 2
      ;;
    --wait)
      WAIT=true
      shift
      ;;
    --max-attempts)
      MAX_ATTEMPTS="$2"
      shift 2
      ;;
    --interval-sec)
      INTERVAL_SEC="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if [[ -z "$VERSION" ]]; then
  echo "Error: --version is required" >&2
  usage >&2
  exit 1
fi

ARTIFACT_URL="$MAVEN_BASE/com/appsflyer/unity-wrapper/$VERSION/unity-wrapper-$VERSION.aar"

echo "Validating Unity wrapper artifact: $ARTIFACT_URL"

if [[ "$WAIT" != true ]]; then
  if artifact_exists; then
    echo "Unity wrapper artifact exists: com.appsflyer:unity-wrapper:$VERSION"
    exit 0
  fi
else
  attempt=1
  while [[ "$attempt" -le "$MAX_ATTEMPTS" ]]; do
    if artifact_exists; then
      echo "Unity wrapper artifact exists: com.appsflyer:unity-wrapper:$VERSION"
      if [[ "$attempt" -gt 1 ]]; then
        echo "Artifact became available after $attempt attempt(s)."
      fi
      exit 0
    fi

    if [[ "$attempt" -eq "$MAX_ATTEMPTS" ]]; then
      break
    fi

    echo "Attempt $attempt/$MAX_ATTEMPTS: artifact not indexed yet; retrying in ${INTERVAL_SEC}s..."
    sleep "$INTERVAL_SEC"
    attempt=$((attempt + 1))
  done
fi

cat >&2 <<EOF
Error: com.appsflyer:unity-wrapper:$VERSION was not found.

Unity packages reference the wrapper from AppsFlyerDependencies.xml. Publish the
wrapper first, or rerun with a version that already exists in Maven Central.
EOF
exit 1
