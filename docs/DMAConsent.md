---
title: Sending Consent Data for DMA Compliance
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 3
hidden: false
---

# Send consent for DMA compliance (Unity)

For a general introduction to DMA consent data, see the [DMA consent overview](https://dev.appsflyer.com/hc/docs/send-consent-for-dma-compliance) in the AppsFlyer docs. The SDK offers two alternative methods for gathering consent data:

- **Through a Consent Management Platform (CMP):** If your app uses a CMP that complies with the IAB **Transparency and Consent Framework (TCF) v2.2**, the SDK can automatically retrieve consent details.

**OR**

- **Through a dedicated SDK API:** Provide Google’s required consent data directly to the SDK using the **Unity** consent APIs.

> **Note**  
> Use **only one** method per specific event. If both are supplied for the same event, AppsFlyer prioritizes the manual consent data sent via the dedicated API.

---

## Use CMP to collect consent data

A CMP compatible with **TCF v2.2** persists the consent strings on-device (NSUserDefaults on iOS / SharedPreferences on Android). To enable the Unity SDK to access this data and include it with every event, follow these steps:

1. **Initialize** the SDK (in your Unity scene startup code).  
2. **Before** starting the SDK, call `AppsFlyer.enableTCFDataCollection(true)` to instruct the SDK to collect TCF data from the device.  
3. Use your CMP to decide if the consent dialog is needed in the current session.  
4. If needed, **show the consent dialog** to capture the user’s decision; otherwise skip to step 6.  
5. After the CMP confirms the user made a decision and the TCF data is stored, proceed.  
6. Call `AppsFlyer.startSDK()`.

### Unity (C#) example

```csharp
using AppsFlyerSDK;
using UnityEngine;

public class DMAConsentCmpFlow : MonoBehaviour
{
    [SerializeField] string devKey = "AF_DEV_KEY";
    [SerializeField] string appId  = "com.example.app"; // empty on Android

    void Start()
    {
        AppsFlyer.initSDK(devKey, appId);
        AppsFlyer.enableTCFDataCollection(true); // collect TCF strings from device

        // Pseudocode for your CMP flow:
        if (CmpManager.HasConsentReady())
        {
            AppsFlyer.startSDK();
        }
        else
        {
            CmpManager.ShowDialog(onUserDecided: () =>
            {
                // CMP indicates consent data now stored in device prefs
                AppsFlyer.startSDK();
            });
        }
    }
}
```

---

## Manually collect consent data

If your app **does not** use a TCF v2.2-compatible CMP, provide the consent data directly to the SDK via `AppsFlyer.setConsentData(AppsFlyerConsent)`.

1. **Initialize** the SDK in your startup flow.  
2. Determine whether **GDPR applies** to the user.  
3. Check whether consent data is **already stored** for this session.  
   - If not stored, **show your own consent dialog** and collect the user’s choices.  
   - If stored, continue.  
4. Create an `AppsFlyerConsent` instance with the following parameters, then call `AppsFlyer.setConsentData(consent)`:
   - `isUserSubjectToGDPR` – whether GDPR applies to this user.  
   - `hasConsentForDataUsage` – consent to use data for advertising/measurement.  
   - `hasConsentForAdsPersonalization` – consent for personalized ads.  
   - `hasConsentForAdStorage` – consent to store/access information on the device.  
5. If GDPR **does not apply**, set `isUserSubjectToGDPR = false` and **the rest to `null`**.  
6. Call `AppsFlyer.startSDK()`.

### Unity (C#) examples

**GDPR applies (example values):**
```csharp
// Build from your dialog results
var consent = new AppsFlyerConsent(
    isUserSubjectToGDPR:            true,
    hasConsentForDataUsage:         true,
    hasConsentForAdsPersonalization:true,
    hasConsentForAdStorage:         false
);

AppsFlyer.setConsentData(consent);
AppsFlyer.startSDK();
```

**GDPR does NOT apply:**
```csharp
var nonGdpr = new AppsFlyerConsent(
    isUserSubjectToGDPR:            false,
    hasConsentForDataUsage:         null,
    hasConsentForAdsPersonalization:null,
    hasConsentForAdStorage:         null
);

AppsFlyer.setConsentData(nonGdpr);
AppsFlyer.startSDK();
```

> **Note**  
> The SDK registers only the parameters you explicitly pass via the `AppsFlyerConsent` object. (Unset parameters remain unknown.)

---

## Verify consent data is sent

To confirm the SDK sends DMA consent data with each event:

1. **Enable debug mode** in the Unity Plugin (development only).  
2. Inspect the device logs for the outgoing request and look for a `consent_data` section.  
   - **CMP flow** includes a `tcf` block (e.g., `gdpr_applies`, `tcstring`, `cmp_sdk_id`, `policy_version`).  
   - **Manual flow** includes a `manual` block (e.g., `gdpr_applies`, `ad_user_data_enabled`, `ad_personalization_enabled`).  

Example patterns from Android/iOS logs (your Unity build will emit analogous payloads per platform):

- CMP flow: `..."consent_data":{"tcf":{"policy_version":4,"cmp_sdk_id":...,"gdpr_applies":1,"tcstring":"..."}}...`  
- Manual flow: `..."consent_data":{"manual":{"gdpr_applies":true,"ad_user_data_enabled":true,"ad_personalization_enabled":true}}...`

---

## Quick API reference (Unity)

- **Collect TCF strings automatically:**  
  `AppsFlyer.enableTCFDataCollection(true|false)` — call **before** `startSDK()`.

- **Send manual consent:**  
  `AppsFlyer.setConsentData(AppsFlyerConsent consent)` with:  
  `AppsFlyerConsent(bool? isUserSubjectToGDPR, bool? hasConsentForDataUsage, bool? hasConsentForAdsPersonalization, bool? hasConsentForAdStorage)` (use `null` where unknown/not applicable).

---

### Notes & best practices

- Set consent **before** `startSDK()` so the first session includes the correct signals.  
- If the user changes their choice later, rebuild `AppsFlyerConsent` with the new values and call `setConsentData` again.
