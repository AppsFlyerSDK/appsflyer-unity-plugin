---
name: plugin-release
description: Review the AppsFlyer Unity plugin for release readiness, including versioning, changelog, Android/iOS parity, billing library variants, .unitypackage artifacts, and integration safety.
---

# Plugin Release

Use this skill before releasing a new version of the AppsFlyer Unity plugin.

## Goal

Check whether the plugin is safe and ready for release.

## Workflow

1. Inspect the change set or current release candidate.
2. Identify: plugin version changes, Android SDK version changes, iOS SDK version changes, C# API changes, billing library variant changes.
3. Review release readiness: version consistency, changelog presence, compatibility risks, Android/iOS parity, missing validation.
4. Verify both billing library variants (v7 and v8) build correctly.
5. Check whether `.unitypackage` files in `deploy/` need to be regenerated.
6. Check whether new behavior requires release notes or migration guidance.
7. Summarize release blockers and non-blockers.

## What to Check

- `Assets/AppsFlyer/AppsFlyer.cs` — version constant
- `CHANGELOG.md`
- `android-unity-wrapper/build.gradle` — Android dependency versions, both billing variants
- iOS podspec — iOS dependency versions
- Public C# API changes
- Assembly definition integrity
- `deploy/` `.unitypackage` artifacts — freshness
- Backward compatibility risk

## Output Format

Return:
- Release summary
- Risk level: Low | Medium | High | Critical
- Blocking issues
- Non-blocking issues
- `.unitypackage` regeneration required: Yes | No
- Missing validations
- Recommended release decision

## Rules

- Do not approve a release if versioning is inconsistent.
- Both billing library variants must be buildable.
- `.unitypackage` files must be regenerated before tagging a release.
- Explicitly call out C#/API compatibility risk.
- Explicitly call out Android/iOS behavior drift.
