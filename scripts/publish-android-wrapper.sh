#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ANDROID_WRAPPER_DIR="$REPO_ROOT/android-unity-wrapper"
GRADLE_PROPS="$ANDROID_WRAPPER_DIR/gradle.properties"
VALIDATE_SCRIPT="$SCRIPT_DIR/validate-unity-wrapper.sh"

VERSION=""
SKIP_IF_EXISTS=false

usage() {
  cat <<EOF
Usage: $(basename "$0") --version <version> [options]

Build and publish com.appsflyer:unity-wrapper to Maven Central via Sonatype.

Options:
  --skip-if-exists   Skip publish when the artifact already exists in Maven Central

Gradle credentials (pass via -P or ORG_GRADLE_PROJECT_* env vars):
  ossrhToken, ossrhTokenPassword
  signing.keyId, signing.secretKey, signing.password
EOF
}

append_gradle_credential_args() {
  GRADLE_ARGS=()

  local ossrh_user="${ossrhToken:-${SONATYPE_USERNAME:-${ORG_GRADLE_PROJECT_ossrhToken:-}}}"
  local ossrh_pass="${ossrhTokenPassword:-${SONATYPE_PASSWORD:-${ORG_GRADLE_PROJECT_ossrhTokenPassword:-}}}"
  local sign_key_id="${MAVEN_SIGNING_KEY_ID:-${ORG_GRADLE_PROJECT_signing_keyId:-}}"
  local sign_secret="${MAVEN_SIGNING_KEY:-${ORG_GRADLE_PROJECT_signing_secretKey:-}}"
  local sign_password="${MAVEN_SIGNING_PASSWORD:-${ORG_GRADLE_PROJECT_signing_password:-}}"

  [[ -n "$ossrh_user" ]] && GRADLE_ARGS+=(-PossrhToken="$ossrh_user")
  [[ -n "$ossrh_pass" ]] && GRADLE_ARGS+=(-PossrhTokenPassword="$ossrh_pass")
  [[ -n "$sign_key_id" ]] && GRADLE_ARGS+=(-Psigning.keyId="$sign_key_id")
  [[ -n "$sign_secret" ]] && GRADLE_ARGS+=(-Psigning.secretKey="$sign_secret")
  [[ -n "$sign_password" ]] && GRADLE_ARGS+=(-Psigning.password="$sign_password")
}

read_version_code() {
  grep '^VERSION_CODE=' "$GRADLE_PROPS" | cut -d= -f2
}

set_gradle_property() {
  local key="$1"
  local value="$2"
  if grep -q "^${key}=" "$GRADLE_PROPS"; then
    sed -i.bak "s|^${key}=.*|${key}=${value}|" "$GRADLE_PROPS"
    rm -f "${GRADLE_PROPS}.bak"
  else
    echo "${key}=${value}" >>"$GRADLE_PROPS"
  fi
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="$2"
      shift 2
      ;;
    --skip-if-exists)
      SKIP_IF_EXISTS=true
      shift
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

if [[ ! -f "$GRADLE_PROPS" ]]; then
  echo "Error: gradle.properties not found at $GRADLE_PROPS" >&2
  exit 1
fi

if [[ "$SKIP_IF_EXISTS" == true ]]; then
  echo "Checking whether com.appsflyer:unity-wrapper:$VERSION already exists..."
  if "$VALIDATE_SCRIPT" --version "$VERSION"; then
    echo "Skipping publish: unity-wrapper:$VERSION is already on Maven Central."
    exit 0
  fi
  echo "Artifact not found; proceeding with publish."
fi

current_code="$(read_version_code)"
current_name="$(grep '^VERSION_NAME=' "$GRADLE_PROPS" | cut -d= -f2)"

echo "Updating $GRADLE_PROPS:"
echo "  VERSION_NAME=$VERSION (was $current_name)"

if [[ "$current_name" != "$VERSION" ]]; then
  new_code=$((current_code + 1))
  echo "  VERSION_CODE=$new_code (was $current_code)"
  set_gradle_property VERSION_NAME "$VERSION"
  set_gradle_property VERSION_CODE "$new_code"
else
  new_code="$current_code"
  echo "  VERSION_CODE=$current_code (unchanged; bump-version.sh already set VERSION_NAME)"
fi

append_gradle_credential_args

cd "$ANDROID_WRAPPER_DIR"

echo "Assembling unity-wrapper release AAR..."
./gradlew :unitywrapper:assembleRelease "${GRADLE_ARGS[@]}"

echo "Publishing to Sonatype and releasing staging repository..."
set +e
publish_output="$(
  ./gradlew publishToSonatype closeAndReleaseSonatypeStagingRepository -x test \
    "${GRADLE_ARGS[@]}" 2>&1
)"
publish_status=$?
set -e

echo "$publish_output"

staging_repo_id="$(echo "$publish_output" | grep -Eo 'stagingRepositoryId[=: ][^[:space:]]+' | tail -1 | sed 's/.*[=: ]//')"
if [[ -n "$staging_repo_id" ]]; then
  echo "Sonatype staging repository id: $staging_repo_id"
fi

if [[ "$publish_status" -ne 0 ]]; then
  echo "Error: Gradle publish failed (exit $publish_status)." >&2
  exit "$publish_status"
fi

echo "Published com.appsflyer:unity-wrapper:$VERSION (VERSION_CODE=$new_code)."
