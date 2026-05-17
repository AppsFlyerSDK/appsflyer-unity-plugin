---
name: e2e-smoke-test
description: Run or review a basic end-to-end smoke test for the AppsFlyer Unity plugin on Android emulator or iOS simulator, covering startup, initialization, and basic event flow.
---

# E2E Smoke Test

Use this skill for quick validation that the Unity plugin is basically working after a change.

## Goal

Check that the plugin still works end-to-end at a basic level after code, SDK, or version changes.

## Workflow

1. Identify the target platform: Android emulator | iOS simulator | both
2. Verify the scenario setup: Unity test app exported, APK/app available, emulator/simulator ready.
3. Run or inspect the basic smoke scenario: app launch → SDK init (`initSDK`) → SDK start (`startSDK`) → one basic AppsFlyer event → callback flow.
4. Collect evidence: logs, callback payloads, Unity console output.
5. Report whether the flow passed or failed.

## Output Format

Return:
- Scenario
- Platform
- Steps
- Expected behavior
- Actual behavior
- Evidence
- Status: Passed | Failed | Blocked
- Follow-up recommendation

## Rules

- Keep this focused on basic release confidence.
- Unity builds must be exported before running on emulator/simulator.
- Note which billing library variant was used (v7 or v8).
- If a key part cannot be tested locally, say so clearly.
