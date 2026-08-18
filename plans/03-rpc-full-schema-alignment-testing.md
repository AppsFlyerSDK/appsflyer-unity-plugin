# Plan 03 — Full RPC Schema Alignment: Unit Test Plan

**Branch:** `dev/DELIVERY-124647/rpc-implementation`
**Context:** Supersedes the "zero breaking changes" constraint in `docs/RPC-Implementation-Plan.md` / `plans/01-rpc-phase1-csharp-layer.md` (confirmed superseded — see Notion doc). Companion to the Unity RPC Public API alignment matrix (Notion): every Unity public method aligns 100% with `Assets/AppsFlyer/appsflyer-plugins-rpc-schema.json` — naming and types, both platforms, no legacy bridge fallback.

**Rollout mechanism:** rename current `Assets/AppsFlyer/AppsFlyer.cs` → `AppsFlyerV1.cs`, comment out its entire contents (inert reference/rollback copy, deleted once the new implementation is confirmed via testing). New implementation lands directly as `AppsFlyer.cs` / class `AppsFlyer`.

This document tracks what happens to `Assets/AppsFlyer/Tests/Tests_Suite.cs` as a result. Categorized against the current suite (102 `[Test]` cases across 5 fixtures) as read from source.

---

## Open question surfaced while planning this: orphaned legacy files

Once `AppsFlyerV1.cs` is commented out **and** the new `AppsFlyer.cs` never references the legacy bridge either, the following become entirely unreferenced by the live plugin:

- `Assets/AppsFlyer/AppsFlyerAndroid.cs`
- `Assets/AppsFlyer/AppsFlyeriOS.cs`
- `Assets/AppsFlyer/IAppsFlyerNativeBridge.cs`
- `Assets/AppsFlyer/IAppsFlyerAndroidBridge.cs`
- `Assets/AppsFlyer/IAppsFlyerIOSBridge.cs`

**Decision needed:** delete these alongside `AppsFlyerV1.cs` once testing confirms the new implementation, or keep them indefinitely as a lower-level rollback net? Affects whether `AppsFlyerAndroidTests`/`AppsFlyeriOSTests` (below) can even compile if kept partially.

---

## Category A — Delete (mock the legacy bridge; new implementation never calls it)

**Rationale:** `AppsFlyerSDK`'s new `AppsFlyer.cs` is 100% RPC-driven — it does not call into `AppsFlyerAndroid`/`AppsFlyeriOS`/`IAppsFlyerNativeBridge` at all. Every test below asserts a call landing on that legacy bridge, so they test a code path that no longer exists.

### `AppsFlyerSDKTests` (mocks `IAppsFlyerNativeBridge`)
- `SendEvent_WithParams_ShouldCallBridge`
- `SendEvent_NullParams_ShouldCallBridge`
- `SetCustomerUserId_ShouldCallBridge`
- `SetAdditionalData_ShouldCallBridge`
- `SetResolveDeepLinkURLs_ShouldCallBridge`
- `SetCurrencyCode_ShouldCallBridge`
- `SetMinTimeBetweenSessions_ShouldCallBridge`
- `SetHost_ShouldCallBridge`
- `SetSharingFilterForAllPartners_ShouldCallBridge` (`[Obsolete]`)
- `SetSharingFilter_ShouldCallBridge` (`[Obsolete]`)
- `SetConsentData_ShouldCallBridge_WhenInstanceIsNotNull`
- `SetConsentData_ShouldNotThrow_WhenInstanceIsNull`
- `RecordLocation_ShouldCallBridge`
- `GetAppsFlyerId_ShouldCallBridge`
- `GetConversionData_ShouldCallBridge`
- `GenerateUserInviteLink_ShouldCallBridge`
- `AttributeAndOpenStore_WithParams_ShouldCallBridge`
- `AttributeAndOpenStore_NullParams_ShouldCallBridge`
- `RecordCrossPromoteImpression_WithParams_ShouldCallBridge`
- `RecordCrossPromoteImpression_WithoutParams_ShouldCallBridge`
- `AddPushNotificationDeepLinkPath_ShouldCallBridge`
- `IsSDKStopped_ShouldCallBridge`

### `AppsFlyerAndroidTests` (mocks `IAppsFlyerAndroidBridge`) — entire fixture, all 18 tests
`UpdateServerUninstallToken_ShouldCallBridge`, `SetImeiData_ShouldCallBridge`, `SetAndroidIdData_ShouldCallBridge`, `WaitForCustomerUserId_ShouldCallBridge`, `SetCustomerIdAndStartSDK_ShouldCallBridge`, `GetOutOfStore_ShouldCallBridge`, `SetOutOfStore_ShouldCallBridge`, `SetCollectAndroidID_ShouldCallBridge`, `SetCollectIMEI_ShouldCallBridge`, `SetIsUpdate_ShouldCallBridge`, `SetPreinstallAttribution_ShouldCallBridge`, `IsPreInstalledApp_ShouldCallBridge`, `GetAttributionId_ShouldCallBridge`, `HandlePushNotifications_ShouldCallBridge`, `ValidateAndSendInAppPurchase_ShouldCallBridge`, `SetCollectOaid_ShouldCallBridge`, `SetDisableAdvertisingIdentifiers_ShouldCallBridge`, `SetDisableNetworkData_ShouldCallBridge`

### `AppsFlyeriOSTests` (mocks `IAppsFlyerIOSBridge`) — entire fixture, all 16 tests
`DisableCollectAppleAdSupport_True/False_ShouldCallBridge`, `ShouldCollectDeviceName_True/False_ShouldCallBridge` (`[Obsolete]`), `DisableCollectIAd_True/False_ShouldCallBridge`, `UseReceiptValidationSandbox_True/False_ShouldCallBridge`, `UseUninstallSandbox_True/False_ShouldCallBridge`, `ValidateAndSendInAppPurchase_ShouldCallBridge`, `RegisterUninstall_ShouldCallBridge`, `HandleOpenUrl_ShouldCallBridge`, `WaitForATTUserAuthorizationWithTimeoutInterval_ShouldCallBridge`, `SetCurrentDeviceLanguage_ShouldCallBridge`, `DisableSKAdNetwork_True/False_ShouldCallBridge`

---

## Category B — Rewrite (currently assert the exact pre-fix behavior being corrected)

These tests exist today and pass — but they pass *because* they assert the bug. Each must flip to assert the corrected schema-aligned behavior.

| Test | Current assertion (wrong) | Must become |
|---|---|---|
| `SetUserEmails_iOS_SendsPluralMethodWithArrayAndCryptType` | Asserts `setUserEmails` (plural) fires with `cryptType`+`emails`; asserts `setUserEmail` does **not** fire | Assert `setUserEmail` (singular) fires with `email` = first address; assert `setUserEmails` does **not** fire |
| `SetPhoneNumber_iOS_SendsSetPhoneNumberNotSetUserPhone` | Asserts `setPhoneNumber` fires; asserts `setUserPhone` does not | `setPhoneNumber` is deprecated in favor of cross-platform `setUserPhone(countryCode, phoneNumber)` — replace with a `SetUserPhone_iOS_Fires` test |
| `SetPhoneNumber_Android_DoesNotFire` | Asserts `setPhoneNumber` is a no-op on Android today | Update given `setPhoneNumber` deprecation; Android leg of `setUserPhone` already fires correctly (EXACT) — no new gap here, just remove/retire this test's premise |
| `ValidateAndSendInAppPurchase_iOS_SendsValidateAndLogInAppPurchase` | Only checks method name fires, `Arg.Any<Dictionary<string,object>>()` — passes even with today's wrong flat/int-cast payload | Assert nested `product`/`transaction` objects and **string** `purchaseType` (`"subscription"`/`"oneTimePurchase"`), not the current `(int)` cast |
| `SetDisableAdvertisingIdentifiers_iOS_SendsPluralForm` | Only checks method name fires, never checks the `disable` key value | Add explicit key assertion (`d["disable"]`); add a **new** Android-side RPC-contract test asserting `isDisable` (no such test exists today — Android coverage here is currently only the legacy-bridge test being deleted in Category A) |
| `WaitForATTUserAuthorizationWithTimeoutInterval_ShouldCallBridge` | No assertion at all (pure smoke test) | Once the dead `waitForATT` RPC fire is removed (deprecated method, confirmed out of scope — see Notion doc), assert explicitly that **no** RPC call fires for this method |

Lower priority (don't assert real `AppsFlyer.cs` behavior, just `BuildRequest` mechanics with example data — fine to leave, but the example payloads no longer reflect the real schema shape so they read as misleading documentation):
- `BuildRequest_SetUserEmails_iOS_HasCryptTypeAndEmailsKeys`
- `BuildRequest_ValidateAndLogInAppPurchase_HasRequiredFields` (uses flat keys; real schema nests under `product`/`transaction`)

---

## Category C — Net-new (zero coverage today)

Matches the fixes already agreed in the Notion alignment matrix / migration plan.

- `logAndOpenStore` → assert `promotedAppId` key (not `appId`)
- `generateInviteLink` / `generateUserInviteLink` → assert top-level spread keys (`channel`, `campaign`, etc.), not nested under a single `parameters` key
- `updateServerUninstallToken` / `registerUninstall` iOS leg → assert `deviceToken` key (not `token`)
- `logAdRevenue` → assert `mediationNetwork` fires as the schema's string value, not the current `(int)` cast
- `validateAndLogInAppPurchase` Android → brand-new RPC integration (today: legacy-bridge-only, zero RPC coverage)
- `handlePushNotification` iOS → assert the new `pushPayload` param is present once the payload-carrying overload is added
- `handleOpenUrl` → new `{url, options}` shape — **resolved**: schema declares `options` as free-form (`additionalProperties: true`), matching iOS's native untyped options dictionary; no fixed shape to confirm
- `clearUserPii`, `setUserFirstName`, `setUserLastName`, `setUserFbLoginId`, `setUserPhone` → new iOS-firing tests once the `#if UNITY_ANDROID` guards are removed (platform-gap fix)
- `getHostName`, `getHostPrefix`, `isStopped` → brand-new public methods; need existence + RPC-firing tests
- `getAppsFlyerId`, `getSdkVersion`, `getAttributionId`, `getOutOfStore`, `isPreInstalledApp`, `isSDKStopped`, `subscribeForDeepLink` → migrated onto `AppsFlyerRPCClient.Execute()`/`ExecuteFire` as primary path; need new RPC-contract tests replacing the deleted legacy-bridge ones in Category A
- `registerConversionListener` → assert zero params (`maxProperties: 0`, both platforms) — **resolved**: confirmed no undeclared callbackObjectName side channel; routing is via the generic onRPCEvent handler wired at init

---

## Category D — Keep as-is

- **`AppsFlyerRPCClientTests`** (all tests) — pure `BuildRequest`/`ParseResponse`/`onRPCEvent` mechanics, untouched by any naming/parameter fix.
- **Already-correct rows in `AppsFlyerRPCContractTests`:**
  - `StartSDK_FiresStartViaRPC`
  - `StopSDK_iOS_SendsStopNotSetStopped`
  - `StopSDK_Resume_SendsShouldStopFalse`
  - `AnonymizeUser_iOS_SendsAnonymizeUserNotSetAnonymizeUser`
  - `SetAppInviteOneLinkID_iOS_SendsSetAppInviteOneLink`
  - `SetOneLinkCustomDomain_iOS_SendsSingularNotPlural`
  - `SetHost_iOS_UsesHostPrefixNameParam`
  - `SetShouldCollectDeviceName_iOS_Fires`
  - `InitSDK_Android_SendsInitWithDevKeyOnly`
  - `InitSDK_iOS_SendsInitializeWithDevKeyAndAppId`
  - `SubscribeForDeepLink_Android_SendsSubscribeForDeepLink` (name assertion still valid — note this is legacy-bridge-only per Category C above, will need a parity update once migrated onto RPC as primary, but the current assertion isn't *wrong*, just incomplete)
  - `SubscribeForDeepLink_iOS_SendsRegisterDeeplinkListener` (same note)

These carry over directly to whatever new test file backs the new `AppsFlyer.cs`.

---

## Open questions — resolved

All three were answerable directly from `Assets/AppsFlyer/appsflyer-plugins-rpc-schema.json` (the canonical
source of truth) cross-checked against the current `AppsFlyer.cs` implementation, which already conforms:

- `registerConversionListener` extra params — **resolved**: schema declares zero params; no undeclared
  callbackObjectName side channel exists (routing is via the generic onRPCEvent handler wired at init).
- `handleOpenUrl` `options` shape — **resolved**: schema declares it free-form (`additionalProperties: true`),
  matching iOS's native untyped options dictionary; there is no fixed shape to confirm.
- `purchaseType` exact enum string casing — **resolved**: schema gives exact per-platform strings
  (Android `subscription`/`one_time_purchase`, iOS `subscription`/`oneTimePurchase`); `AppsFlyer.cs`'s
  `PurchaseTypeToAndroidString`/`PurchaseTypeToIOSString` already produce these exactly. Recommend one
  on-device validation pass before final sign-off, but this is no longer a blocking unknown.

**Decision on orphaned legacy files (line 22, above): resolved — deleted.** `AppsFlyerV1.cs`,
`AppsFlyerAndroid.cs`, `AppsFlyeriOS.cs`, `IAppsFlyerAndroidBridge.cs`, `IAppsFlyerIOSBridge.cs`, and
`IAppsFlyerNativeBridge.cs` have been removed now that the open questions above are settled and
`Tests_Suite.cs` no longer has any Category A fixtures depending on them.
