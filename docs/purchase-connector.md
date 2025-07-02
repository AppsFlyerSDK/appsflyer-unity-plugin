---
title: "Purchase connector"
slug: "purchase-connector-unity"
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
excerpt: "Used to validate and report in-app purchase and subscription revenue events"
hidden: false
order: 13
---

## Overview

The AppsFlyer ROI360 purchase connector is used to validate and report in-app purchase and subscription revenue events. It's part of the ROI360 in-app purchase and subscription revenue measurement solution.

- Using the purchase connector requires an ROI360 subscription.
- If you use this in-app purchase and subscription revenue measurement solution, you shouldn't send [in-app purchase events](https://dev.appsflyer.com/hc/docs/inappevents) with revenue or execute [`validateAndLogInAppPurchase`](https://dev.appsflyer.com/hc/docs/validate-and-log-purchase-ios), as doing so results in duplicate revenue being reported.
- Before implementing the purchase connector, the ROI360 in-app purchase and subscription revenue measurement needs to be integrated with Google Play and the App Store. [See instructions (steps 1 and 2)](https://support.appsflyer.com/hc/en-us/articles/7459048170769)

## Prerequisites

### iOS Requirements
- StoreKit SDK v1 or v2 (StoreKit 2 requires iOS 15+)
- iOS version 9 and higher
- Unity AppsFlyer plugin **6.17.1** and higher

### Android Requirements
- Google Play Billing library version **5.x.x**, **6.x.x**, and **7.x.x**
- Unity AppsFlyer plugin **6.17.1** and higher

### General Requirements
- Unity version **2020.3** and higher
- ROI360 subscription

## Installation

### Method 1: Integrated Approach (v6.17.1+) - **Recommended**

**Starting with version 6.17.1, the Purchase Connector is integrated directly into the main AppsFlyer Unity plugin.** You no longer need to download or import a separate package.

1. Download the latest [AppsFlyer Unity plugin](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin) (v6.17.1 or higher)
2. [Import](https://docs.unity3d.com/Manual/AssetPackages.html) the `appsflyer-unity-plugin-x.x.x.unitypackage` into your Unity project
   - Go to **Assets** → **Import Package** → **Custom Package**
   - Select the `appsflyer-unity-plugin-x.x.x.unitypackage`

The Purchase Connector functionality is now included automatically - no additional imports required!

### Method 2: Separate Repository Approach (Pre-v6.17.1)

**This approach is only relevant for versions prior to 6.17.1.** Starting with version 6.17.1, the Purchase Connector is integrated into the main AppsFlyer Unity plugin and this separate repository approach is no longer needed.

If you are using a version older than 6.17.1:

1. Download the [AppsFlyer Unity plugin](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin)
2. Download the [Purchase Connector repository](https://github.com/AppsFlyerSDK/appsflyer-unity-purchase-connector)
3. Import both packages into your Unity project:
   - First import `appsflyer-unity-plugin-x.x.x.unitypackage`
   - Then import `appsflyer-unity-purchase-connector-x.x.x.unitypackage`

**Note:** When using the separate repository approach, make sure the Purchase Connector version is compatible with your AppsFlyer Unity plugin version.

**Recommendation:** Consider upgrading to version 6.17.1+ to use the integrated approach for simplified setup and access to the latest features.

## ProGuard Rules

_Android Only_ - If you are using ProGuard, add the following keep rules to your `proguard-rules.pro` file:

```groovy
-keep class com.appsflyer.** { *; }
-keep class kotlin.jvm.internal.Intrinsics{ *; }
-keep class kotlin.collections.**{ *; }
-keep class kotlin.Result$Companion { *; }
```

## Strict Mode Support

The Purchase Connector supports Strict Mode, which completely removes IDFA collection functionality and AdSupport framework dependencies. Use Strict Mode when developing apps for kids or when IDFA collection is not desired.

Make sure to use the strict mode AppsFlyer Unity plugin along with the Purchase Connector's strict mode functionality.

## Choosing Your Implementation Method

### When to Use Integrated Approach (v6.17.1+)
✅ **Recommended for:**
- New projects starting with v6.17.1+
- Existing projects that can upgrade to v6.17.1+
- Simplified setup and maintenance
- Access to latest features like StoreKit 2 support

## Implementation Guide

The implementation differs slightly depending on which installation method you chose:

### Required Interfaces

Your MonoBehaviour class must implement the following interfaces:

**For Integrated Approach (v6.17.1+):**
```csharp
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour, 
    IAppsFlyerConversionData,                    // For conversion data callbacks
    IAppsFlyerPurchaseValidation,               // For purchase validation callbacks  
    IAppsFlyerPurchaseRevenueDataSource,        // For StoreKit 1 additional parameters 
    IAppsFlyerPurchaseRevenueDataSourceStoreKit2 // For StoreKit 2 additional parameters 
{
    // Implementation goes here
}
```

**For Separate Repository Approach:**
```csharp
using AppsFlyerSDK;
using AppsFlyerConnector; // Additional namespace for separate repository

public class AppsFlyerObjectScript : MonoBehaviour, 
    IAppsFlyerConversionData,                    // For conversion data callbacks
    IAppsFlyerPurchaseValidation,               // For purchase validation callbacks  
    IAppsFlyerPurchaseRevenueDataSource,        // For additional parameters (iOS)
{
    // Implementation goes here
}
```

### Basic Setup

#### Integrated Approach (v6.17.1+)

```csharp
using UnityEngine;
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour, 
    IAppsFlyerConversionData,
    IAppsFlyerPurchaseValidation,
    IAppsFlyerPurchaseRevenueDataSource,
    IAppsFlyerPurchaseRevenueDataSourceStoreKit2
{
    [Header("AppsFlyer Settings")]
    public string devKey = "YOUR_DEV_KEY";
    public string appID = "YOUR_APP_ID";
    public bool isDebug = true;
    public bool getConversionData = true;
    
    void Start()
    {
        // 1. Initialize AppsFlyer SDK
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
        AppsFlyer.setIsDebug(isDebug);
        
        // 2. Initialize Purchase Connector
        AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
        
        // 3. Configure Purchase Connector
        ConfigurePurchaseConnector();
        
        // 4. Build and start observing
        AppsFlyerPurchaseConnector.build();
        AppsFlyerPurchaseConnector.startObservingTransactions();
        
        // 5. Start AppsFlyer SDK
        AppsFlyer.startSDK();
    }
    
    private void ConfigurePurchaseConnector()
    {
        // Set sandbox mode for testing
        AppsFlyerPurchaseConnector.setIsSandbox(true);
        
        // Configure StoreKit version (iOS only)
        AppsFlyerPurchaseConnector.setStoreKitVersion(StoreKitVersion.SK2);
        
        // Enable automatic logging for subscriptions and in-app purchases
        AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
        );
        
        // Enable purchase validation callbacks
        AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
        
        // Set data sources for additional parameters (iOS)
        AppsFlyerPurchaseConnector.setPurchaseRevenueDataSource(this);
        AppsFlyerPurchaseConnector.setPurchaseRevenueDataSourceStoreKit2(this);
    }
}
```

#### Legacy (2-repos) Approach

```csharp
using UnityEngine;
using AppsFlyerSDK;
using AppsFlyerConnector;

public class AppsFlyerObjectScript : MonoBehaviour, 
    IAppsFlyerConversionData,
    IAppsFlyerPurchaseValidation,
    IAppsFlyerPurchaseRevenueDataSource
{
    [Header("AppsFlyer Settings")]
    public string devKey = "YOUR_DEV_KEY";
    public string appID = "YOUR_APP_ID";
    public bool isDebug = true;
    public bool getConversionData = true;
    
    void Start()
    {
        // 1. Initialize AppsFlyer SDK
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
        AppsFlyer.setIsDebug(isDebug);
        
        // 2. Initialize Purchase Connector (using AppsFlyerConnector namespace)
        AppsFlyerPurchaseConnector.init(this, AppsFlyerConnector.Store.GOOGLE);
        
        // 3. Configure Purchase Connector
        ConfigurePurchaseConnector();
        
        // 4. Build and start observing
        AppsFlyerPurchaseConnector.build();
        AppsFlyerPurchaseConnector.startObservingTransactions();
        
        // 5. Start AppsFlyer SDK
        AppsFlyer.startSDK();
    }
    
    private void ConfigurePurchaseConnector()
    {
        // Set sandbox mode for testing
        AppsFlyerPurchaseConnector.setIsSandbox(true);
        
        // Enable automatic logging for subscriptions and in-app purchases
        AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
        );
        
        // Enable purchase validation callbacks
        AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
        
    }
}
```

## Core APIs

### Initialization

```csharp
// Initialize Purchase Connector with store type
AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
```

### Configuration Options

#### StoreKit Version (iOS Only)
```csharp
// Set StoreKit version - SK1 or SK2
AppsFlyerPurchaseConnector.setStoreKitVersion(StoreKitVersion.SK2);
```

#### Sandbox Mode
```csharp
// Enable sandbox mode for testing
AppsFlyerPurchaseConnector.setIsSandbox(true);
```

#### Auto-Logging Options
```csharp
// Enable automatic logging for specific purchase types
AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
);
```

**Available Options:**
- `AppsFlyerAutoLogPurchaseRevenueOptionsDisabled`: Disable automatic logging
- `AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions`: Log auto-renewable subscriptions
- `AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases`: Log in-app purchases

### Transaction Observation

```csharp
// Start observing transactions
AppsFlyerPurchaseConnector.startObservingTransactions();

// Stop observing transactions
AppsFlyerPurchaseConnector.stopObservingTransactions();
```

### Build and Complete Setup

```csharp
// Build the Purchase Connector with all configurations
AppsFlyerPurchaseConnector.build();
```

## Interface Implementations
### IAppsFlyerPurchaseValidation

```csharp
public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
{
    AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
    Debug.Log("Purchase validation success: " + validationInfo);
    
    // Handle different purchase types on Android
#if UNITY_ANDROID
    if (dictionary.ContainsKey("productPurchase") && dictionary["productPurchase"] != null)
    {
        // Handle in-app purchase validation result
        Debug.Log("In-app purchase validated");
    }
    else if (dictionary.ContainsKey("subscriptionPurchase") && dictionary["subscriptionPurchase"] != null)
    {
        // Handle subscription validation result  
        Debug.Log("Subscription validated");
    }
#endif
}

public void didReceivePurchaseRevenueError(string error)
{
    AppsFlyer.AFLog("didReceivePurchaseRevenueError", error);
    Debug.LogError("Purchase validation error: " + error);
}
```

### IAppsFlyerPurchaseRevenueDataSource (StoreKit 1)

```csharp
public Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(
    HashSet<object> products, 
    HashSet<object> transactions)
{
    // Add custom parameters to purchase events
    return new Dictionary<string, object>
    {
        ["custom_param_1"] = "value1",
        ["custom_param_2"] = "value2",
        ["user_level"] = 5,
        ["purchase_source"] = "main_store"
    };
}
```

### IAppsFlyerPurchaseRevenueDataSourceStoreKit2 (StoreKit 2)

```csharp
public Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(
    HashSet<object> products, 
    HashSet<object> transactions)
{
    // Add custom parameters specifically for StoreKit 2 purchases
    return new Dictionary<string, object>
    {
        ["sk2_custom_param"] = "sk2_value",
        ["storekit_version"] = "2.0",
        ["transaction_count"] = transactions.Count
    };
}
```

## Advanced Features

### StoreKit 2 Consumable Transactions (iOS 15+ to iOS 18+)

On iOS 15 and above, consumable in-app purchases are handled via StoreKit 2.  
- **On iOS 18 and later:**  
  Apple introduced a new Info.plist flag: `SKIncludeConsumableInAppPurchaseHistory`.  
  - If you set `SKIncludeConsumableInAppPurchaseHistory` to `YES` in your Info.plist, automatic collection will happen.
  - If the flag is not present or is set to `NO`, you must manually log consumable transactions as shown below.

- **On iOS 15–118:**  
  Consumable purchases must always be logged manually.

```csharp
    AppsFlyerPurchaseConnector.logConsumableTransaction(transactionId); //(iOS SK2 only)

```
  
### Custom Purchase Validation Callbacks

Enable validation callbacks to receive detailed purchase information:

```csharp
AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
```

## Testing in Sandbox

### iOS Testing
1. Set sandbox mode: `AppsFlyerPurchaseConnector.setIsSandbox(true)`
2. Use TestFlight sandbox accounts for testing
3. Test on real devices with Xcode

### Android Testing  
1. Set sandbox mode: `AppsFlyerPurchaseConnector.setIsSandbox(true)`
2. Follow [Google Play Billing testing guidelines](https://developer.android.com/google/play/billing/test)
3. Use test accounts and test products

> **⚠️ IMPORTANT**: Remove `setIsSandbox(true)` or set it to `false` before releasing to production. Production purchases sent in sandbox mode will not be validated properly!

## Complete Implementation Examples

### Integrated Approach (v6.17.1+)

```csharp
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour, 
    IAppsFlyerConversionData,
    IAppsFlyerPurchaseValidation,
    IAppsFlyerPurchaseRevenueDataSource,
    IAppsFlyerPurchaseRevenueDataSourceStoreKit2
{
    [Header("AppsFlyer Configuration")]
    public string devKey = "YOUR_DEV_KEY";
    public string appID = "YOUR_APP_ID";
    public string UWPAppID = "YOUR_UWP_APP_ID";
    public string macOSAppID = "YOUR_MACOS_APP_ID";
    public bool isDebug = true;
    public bool getConversionData = true;

    void Start()
    {
        // 1. Initialize AppsFlyer SDK
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif

        AppsFlyer.setIsDebug(isDebug);

        // 2. Initialize and configure Purchase Connector
        AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
        AppsFlyerPurchaseConnector.setStoreKitVersion(StoreKitVersion.SK2);
        AppsFlyerPurchaseConnector.setIsSandbox(true); // Remove for production
        
        AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
        );
        
        AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);
        AppsFlyerPurchaseConnector.setPurchaseRevenueDataSource(this);
        AppsFlyerPurchaseConnector.setPurchaseRevenueDataSourceStoreKit2(this);

        // 3. Build and start
        AppsFlyerPurchaseConnector.build();
        AppsFlyerPurchaseConnector.startObservingTransactions();

        // 4. Start AppsFlyer SDK
        AppsFlyer.startSDK();
        
        Debug.Log("AppsFlyer SDK + Purchase Connector initialized successfully");
    }

    // --- Purchase Revenue Data Sources ---
    public Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(
        HashSet<object> products, 
        HashSet<object> transactions)
    {
        return new Dictionary<string, object>
        {
            ["storekit_version"] = "1.0",
            ["additional_param"] = "sk1_value",
            ["product_count"] = products.Count,
            ["transaction_count"] = transactions.Count
        };
    }

    public Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(
        HashSet<object> products, 
        HashSet<object> transactions)
    {
        return new Dictionary<string, object>
        {
            ["storekit_version"] = "2.0", 
            ["additional_param"] = "sk2_value",
            ["product_count"] = products.Count,
            ["transaction_count"] = transactions.Count
        };
    }

    // --- Purchase Validation Callbacks ---
    public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
    {
        AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
        Debug.Log("Purchase validation success: " + validationInfo);
        
        // Parse and handle validation info
        var dict = AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;
        
#if UNITY_ANDROID
        if (dict.ContainsKey("productPurchase"))
        {
            Debug.Log("Android in-app purchase validated");
        }
        else if (dict.ContainsKey("subscriptionPurchase"))
        {
            Debug.Log("Android subscription validated");
        }
#endif
    }

    public void didReceivePurchaseRevenueError(string error)
    {
        AppsFlyer.AFLog("didReceivePurchaseRevenueError", error);
        Debug.LogError("Purchase validation error: " + error);
    }

    // --- Conversion Data Callbacks ---
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        var dict = AppsFlyer.CallbackStringToDictionary(conversionData);
        // Handle deferred deep linking
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        var dict = AppsFlyer.CallbackStringToDictionary(attributionData);
        // Handle direct deep linking
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }
}
```

### Legacy Approach

```csharp
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using AppsFlyerConnector;

public class AppsFlyerObjectScript : MonoBehaviour, 
    IAppsFlyerConversionData,
    IAppsFlyerPurchaseValidation,
    IAppsFlyerPurchaseRevenueDataSource,
    IAppsFlyerPurchaseRevenueDataSourceStoreKit2
{
    [Header("AppsFlyer Configuration")]
    public string devKey = "YOUR_DEV_KEY";
    public string appID = "YOUR_APP_ID";
    public string UWPAppID = "YOUR_UWP_APP_ID";
    public string macOSAppID = "YOUR_MACOS_APP_ID";
    public bool isDebug = true;
    public bool getConversionData = true;

    void Start()
    {
        // 1. Initialize AppsFlyer SDK
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif

        AppsFlyer.setIsDebug(isDebug);

        // 2. Initialize and configure Purchase Connector (using separate repository approach)
        AppsFlyerPurchaseConnector.init(this, AppsFlyerConnector.Store.GOOGLE);
        AppsFlyerPurchaseConnector.setIsSandbox(true); // Remove for production
        
        AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
            AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases
        );
        
        AppsFlyerPurchaseConnector.setPurchaseRevenueValidationListeners(true);

        // 3. Build and start
        AppsFlyerPurchaseConnector.build();
        AppsFlyerPurchaseConnector.startObservingTransactions();

        // 4. Start AppsFlyer SDK
        AppsFlyer.startSDK();
        
        Debug.Log("AppsFlyer SDK + Purchase Connector (separate repository) initialized successfully");
    }

    // --- Purchase Revenue Data Sources ---
    public Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(
        HashSet<object> products, 
        HashSet<object> transactions)
    {
        return new Dictionary<string, object>
        {
            ["implementation_type"] = "separate_repository",
            ["additional_param"] = "value",
            ["product_count"] = products.Count,
            ["transaction_count"] = transactions.Count
        };
    }

    public Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(
        HashSet<object> products, 
        HashSet<object> transactions)
    {
        // Note: StoreKit 2 support depends on Purchase Connector version
        return new Dictionary<string, object>
        {
            ["implementation_type"] = "separate_repository_sk2",
            ["additional_param"] = "sk2_value",
            ["product_count"] = products.Count,
            ["transaction_count"] = transactions.Count
        };
    }

    // --- Purchase Validation Callbacks ---
    public void didReceivePurchaseRevenueValidationInfo(string validationInfo)
    {
        AppsFlyer.AFLog("didReceivePurchaseRevenueValidationInfo", validationInfo);
        Debug.Log("Purchase validation success: " + validationInfo);
        
        // Parse and handle validation info
        var dict = AFMiniJSON.Json.Deserialize(validationInfo) as Dictionary<string, object>;
        
#if UNITY_ANDROID
        if (dict.ContainsKey("productPurchase"))
        {
            Debug.Log("Android in-app purchase validated");
        }
        else if (dict.ContainsKey("subscriptionPurchase"))
        {
            Debug.Log("Android subscription validated");
        }
#endif
    }

    public void didReceivePurchaseRevenueError(string error)
    {
        AppsFlyer.AFLog("didReceivePurchaseRevenueError", error);
        Debug.LogError("Purchase validation error: " + error);
    }

    // --- Conversion Data Callbacks ---
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        var dict = AppsFlyer.CallbackStringToDictionary(conversionData);
        // Handle deferred deep linking
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        var dict = AppsFlyer.CallbackStringToDictionary(attributionData);
        // Handle direct deep linking
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }
}
```

## Migration Guide

### Migrating from Legacy to Integrated (Recommended)

If you're upgrading from the separate repository approach to the integrated approach:

1. **Backup your project** before making changes
2. **Remove old Purchase Connector package**: Delete the old Purchase Connector files from your project
3. **Update AppsFlyer Unity plugin**: Install AppsFlyer Unity plugin v6.17.1 or higher
4. **Update code changes**:
   ```csharp
   // OLD (Separate Repository)
   using AppsFlyerConnector; //Remove
   
   // NEW (Integrated)
   using AppsFlyerSDK; // Only this namespace needed
   AppsFlyerPurchaseConnector.init(this, Store.GOOGLE);
   ```
5. **Test thoroughly**: Verify all Purchase Connector functionality works as expected
6. **Update documentation/comments**: Remove references to separate repository setup
