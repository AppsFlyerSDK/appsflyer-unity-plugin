# Plan: Unity Core Standards Compliance Review

**Goal:** Assess the AppsFlyer Unity Plugin against Unity's 10 technical requirements, identify gaps, remediate them, and produce a compliance report for Camila / Unity.

**Context:** Unity sent a legal agreement and tech specs (Jun 18 2026) for listing the AppsFlyer SDK on Unity Core Standards. Kobi owns the technical response.

---

## Phase 0: Discovery (DONE)

Evidence gathered via codebase exploration. Summary below.

### Known Compliance Status

| # | Requirement | Status | Evidence |
|---|---|---|---|
| 1 | Unity 2022.3+ | ✅ OK | Plugin is built and tested with Unity 6 (6000.3.5f1) which exceeds the 2022.3+ requirement. `package.json` declares `"unity": "2019.4"` minimum — verify Unity's vetting tool doesn't flag this during submission (Phase 1). |
| 2 | UPM package ≤ 700MB | ✅ OK | `Assets/AppsFlyer/` = 2.4MB; `.unitypackage` = 472KB |
| 3 | User-declared namespaces | ✅ OK | Only `AppsFlyerSDK` + `AFMiniJSON` — no Unity namespaces |
| 4 | 64-bit Android | ✅ OK | arm64-v8a + x86_64 artifacts; abiFilters delegated to Unity build pipeline |
| 5 | No DRM/registration | ✅ OK | No license gates, trial limits, or registration walls |
| 6 | Opt-in analytics | ✅ OK | `stopSDK(true)` can be called before `startSDK()` to suppress all data collection; developer calls `stopSDK(false)` only after user grants consent. `setConsentData()` + `AppsFlyerConsent` (GDPR/DMA) provide granular opt-in/opt-out per data category. Full opt-in path is supported. |
| 7 | No embedded API keys | ✅ OK | Prefab devKey/appID empty; keys supplied at runtime by developer |
| 8 | Terms/costs disclosed | ⚠️ Investigate | Need more info on what Unity specifically requires here — unclear if this means linking to AppsFlyer's pricing/ToS page, adding a disclosure in the package README, or something else. No current pricing/ToS links in docs. |
| 9 | No executables | ✅ OK | `AppsFlyerBundle.bundle` is a `Mach-O 64-bit bundle` (not executable) — code-signed universal (x86_64 + arm64) macOS native plugin. Unity `.meta` registers it as `PluginImporter` for `Standalone: OSXUniversal`. Standard Unity native plugin format, not a standalone executable. |
| 10 | Documentation | ✅ OK | 19+ markdown docs: API ref, integration guides, DMA consent, migration, troubleshooting |

---

## Phase 1: Investigate the Three Uncertain Requirements

**Execute in a fresh session. Read all cited files before writing conclusions.**

### 1.1 — Opt-in Analytics Model (Req 6)

Unity requires analytics to be **opt-in** — no data must be collected before the user explicitly consents.

AppsFlyer's current integration pattern is:
```
initSDK(devKey, appID)  →  startSDK()  →  tracking begins immediately
```

**Tasks:**
1. Read `Assets/AppsFlyer/AppsFlyer.cs` lines 48–105 (initSDK + startSDK).
2. Read `Assets/AppsFlyer/AppsFlyerConsent.cs` (full file).
3. Read `docs/DMAConsent.md` (full file).
4. Read `docs/BasicIntegration.md` (full file) — is there a documented "defer startSDK until consent" pattern?
5. Answer: Can a developer legally integrate the plugin such that **zero data is sent** until the end-user explicitly opts in? Or does `initSDK` itself send data?
6. Document the gap clearly: what the current model does vs. what Unity requires.

**Verification:** If there is a compliant opt-in path, cite the exact API calls needed. If not, define what new API or documentation would satisfy the requirement.

---

### 1.2 — Executables in Package (Req 9)

Unity's rule: "No executables embedded in the package."

**Suspicious file:** `Assets/AppsFlyer/Mac/AppsFlyerBundle.bundle/Contents/MacOS/AppsFlyerBundle`

**Tasks:**
1. Run: `file /Users/kobi.kagan/Documents/Git/Plugins/appsflyer-unity-plugin/Assets/AppsFlyer/Mac/AppsFlyerBundle.bundle/Contents/MacOS/AppsFlyerBundle`
2. Run: `ls -la /Users/kobi.kagan/Documents/Git/Plugins/appsflyer-unity-plugin/Assets/AppsFlyer/Mac/`
3. Determine: Is this a macOS native plugin bundle (`.bundle`) used as a Unity native plugin — which Unity explicitly supports — or a standalone executable?
4. Check Unity docs (search for "native plugin bundle macOS" in Unity documentation) to confirm `.bundle` files are explicitly allowed as native plugins.
5. Check if `gradlew` / `gradlew.bat` (in `android-unity-wrapper/`) are included in the UPM package or only in the development wrapper (should NOT be in the published package).
6. Run: `cat /Users/kobi.kagan/Documents/Git/Plugins/appsflyer-unity-plugin/Assets/AppsFlyer/package.json` — check for any `exclude` patterns.

**Verification:** Confirm whether the macOS bundle is a native plugin (allowed) or an executable (not allowed). Confirm the Android wrapper scripts are excluded from the UPM package.

---

### 1.3 — UPM Package Scope (Req 2)

The full repo is 8GB (dominated by `test-app/` at 7.1GB and `Build/` at 741MB). These must NOT be included in the Unity Asset Store submission.

**Tasks:**
1. Run: `du -sh /Users/kobi.kagan/Documents/Git/Plugins/appsflyer-unity-plugin/Assets/AppsFlyer/`
2. Run: `find /Users/kobi.kagan/Documents/Git/Plugins/appsflyer-unity-plugin/Assets/AppsFlyer -type f | wc -l`
3. Check if there is a `.npmignore` or `.gitignore` that would exclude non-package files from UPM publishing.
4. Check if the `.unitypackage` artifact only contains `Assets/AppsFlyer/`: `unzip -l /Users/kobi.kagan/Documents/Git/Plugins/appsflyer-unity-plugin/deploy/*.unitypackage 2>/dev/null || true`

**Verification:** Confirm the UPM-publishable surface is ≤700MB and identify exactly what's included vs. excluded.

---

## Phase 2: Remediate the Gaps

**Execute after Phase 1 findings are confirmed. One task per gap.**

### 2.1 — Req 1 is Met (verify vetting tool behaviour only)

Plugin is built and tested on Unity 6 (6000.3.5f1), which is above the 2022.3 threshold. No code change needed. During Phase 1, confirm Unity's Asset Store Publisher tool does not reject packages whose `package.json` declares an older minimum than 2022.3 — if it does, bump `"unity"` field to `"2022.3"`.

---

### 2.2 — Add Terms/Pricing Disclosure (Req 8)

Unity requires: "Terms of API usage and costs must be clearly disclosed."

**Tasks:**
1. Add a **"Pricing & Terms"** section to `README.md` that includes:
   - AppsFlyer is a paid attribution platform; pricing tiers at `https://www.appsflyer.com/pricing/`
   - Link to AppsFlyer Terms of Service: `https://www.appsflyer.com/legal/terms-of-service/`
   - Link to Privacy Policy: `https://www.appsflyer.com/legal/privacy-policy/`
   - Note that a free trial is available and what the free tier includes.
2. Add the same links to `docs/Introduction.md`.

**Pattern:** Copy disclosure structure from the AppsFlyer Android or iOS SDK READMEs if they have this section.

**Verification:** `grep -n "pricing\|Terms of Service" README.md` returns results.

---

### 2.3 — Address Opt-in Analytics Gap (Req 6)

**Depends on Phase 1.1 findings.** Two possible outcomes:

**If a compliant opt-in path already exists:**
- Add a dedicated section to `docs/BasicIntegration.md`: "Opt-in Integration for Unity Core Standards"
- Document the exact sequence: show how to defer `startSDK()` until user consent is confirmed.
- Reference `AppsFlyerConsent`, `setConsentData()`, and `stopSDK(false)` to resume.

**If no compliant opt-in path exists:**
- Open a follow-up engineering task: implement an `initSDK()` overload that defers all data collection until `grantConsent()` is explicitly called.
- This is a **blocking gap** — escalate to Camila that this requires an SDK change before submission.

---

### 2.4 — Resolve Executable Question (Req 9)

**Depends on Phase 1.2 findings.** Two possible outcomes:

**If macOS `.bundle` is confirmed as a native plugin (expected):**
- No code change needed. Add a note in the compliance report explaining it is a Unity-native plugin bundle, not a standalone executable, per Unity's native plugin documentation.

**If the bundle is flagged by Unity's vetting tool:**
- Evaluate whether macOS support can be stripped from the UPM submission or restructured as a `.dylib`.

---

## Phase 3: Write Compliance Report for Unity/Camila

**Execute after Phases 1–2 are complete.**

Produce a document at `docs/unity-core-standards-compliance-report.md` with the following structure:

```
# AppsFlyer Unity Plugin — Unity Core Standards Compliance Report

## Summary
[Overall status: X of 10 requirements fully met, Y require clarification, Z require remediation]

## Requirement-by-Requirement Assessment

### 1. Unity 2022.3+ support
Status: ✅ / ❌ / ⚠️
Evidence: [what was found]
Action taken / needed: [what was or must be done]

[Repeat for all 10 requirements]

## Open Questions for Unity
[Numbered list of questions to send to Unity — especially around opt-in model and native plugin bundles]

## Proposed Timeline
[Estimates for any required SDK changes or doc updates]
```

This document becomes the basis for Camila's reply email to Unity and the email thread addition of a Product stakeholder.

---

## Quick Reference: Key Files

| File | Relevance |
|---|---|
| `Assets/AppsFlyer/package.json` | UPM manifest — unity version, name, license |
| `Assets/AppsFlyer/AppsFlyer.cs` | Public C# API — initSDK, startSDK, stopSDK |
| `Assets/AppsFlyer/AppsFlyerConsent.cs` | GDPR/DMA consent model |
| `Assets/AppsFlyer/AppsFlyerAndroid.cs` | Android bridge — stopTracking, setConsentData |
| `Assets/AppsFlyer/AppsFlyeriOS.cs` | iOS bridge — stopSDK, setConsentData |
| `Assets/AppsFlyer/Mac/AppsFlyerBundle.bundle/` | macOS native plugin — executable question |
| `docs/DMAConsent.md` | Existing consent documentation |
| `docs/BasicIntegration.md` | Integration guide — opt-in pattern needed here |
| `README.md` | Terms/pricing disclosure needed here |
| `CHANGELOG.md` | Update after version bump |
