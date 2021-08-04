<img src="https://massets.appsflyer.com/wp-content/uploads/2018/06/20092440/static-ziv_1TP.png"  width="400" >

# appsflyer-unity-plugin

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![GitHub tag](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)


🛠 In order for us to provide optimal support, we would kindly ask you to submit any issues to support@appsflyer.com

> *When submitting an issue please specify your AppsFlyer sign-up (account) email , your app ID , production steps, logs, code snippets and any additional relevant information.*




### <a id="plugin-build-for"> This plugin is built for

- Android AppsFlyer SDK **v6.3.2** 
- iOS AppsFlyer SDK **v6.3.2**

## <a id="breaking-changes"> 	❗❗ Breaking changes when updating to 6.3.0 ❗❗

- 6.3.0 supports Universal Windows Platform. As part of this update, the AppsFlyerObjectScript changes to include the app_id for your UWP app. If you made changes to this file, please merge them with the new AppsFlyerObjectScript.
Please also note that you can leave the uwp app id field empty. 

- From version `6.3.0`, we use `xcframework` for iOS platform, then you need to use cocoapods version >= 1.10

## <a id="migration"> ⏩ Migration 
  
Migrating from the old plugin? (version V4) <br/>
View the migration docs [here](/docs/MigrationGuide.md).

⚠️ There are **breaking** changes when migrating to `Unity v5`. This includes new API, different class/package names, and the removal of `com.appsflyer.GetDeepLinkingActivity`.


## Table of content
- [Adding the SDK to your project](#add-sdk-to-project)
- [Initializing the SDK](#init-sdk)
- [In-app Events](#in-app-events)
- [Get Conversion Data](#gcd)
- [Deep Linking](#deep-linking)
- [Advanced APIs](#advanced-apis)
- [Testing the integration](#Testing)
- [API](#api) 

<hr/>



## <a id="init-sdk"> 🚀 Initializing the SDK




 ## <a id="guides"> 📖 Guides


- [Installation](/docs/Installation.md)
- [Init SDK](/docs/Guides.md#init-sdk)
- [Deep Linking](/docs/Guides.md#deeplinking)
- [Uninstall](/docs/Guides.md#track-app-uninstalls)
- [User Invite](/docs/Guides.md#-user-invite-attribution)


## <a id="api"> 📑 API
  
See the full [API](/docs/API.md) available for this plugin.





