# Versions

## v6.12.10
* Update iOS SDK version - 6.12.0
* Update Android SDK version - 6.12.1

## v6.10.30
* Update iOS SDK version - 6.10.1
* Update Android SDK version - 6.10.3

## v6.10.10
* Update iOS SDK version - 6.10.0
* Update Android SDK version - 6.10.1
* Fix NullReferenceException when not using Request Listeners with UWP
* Align `setHost`, `disableSKAdNetwork` and `setCurrencyCode` to work if called prior to `initSdk`

## v6.9.4
* Update iOS SDK version - 6.9.1
* Update Android SDK version - 6.9.4
* Fix appsflyer logo on prefab
* Fix attributeAndOpenStore issue on Android

## v6.8.5
* Fix setHost API for iOS

## v6.8.4
* Fix package

## v6.8.3
* Fix android issue
* Fix in-app callback issue

## v6.8.2
* Fix dependency for strict mode

## v6.8.1
* Update iOS SDK version - 6.8.1
* Update Android SDK version - 6.8.0
* Added new Android API `setDisableNetworkData`

## v6.6.0
* Update iOS SDK version - 6.6.0
* Update Android SDK version - 6.6.0
* add new API setPartnerData
* Beta support for MacOS

## v6.5.4
* Update iOS SDK version - 6.5.4
* Update Android SDK version - 6.5.4

## v6.5.3
* Additional solution for Swizzling using macroprocessor

## v6.5.2
* Update iOS SDK version - 6.5.2
* Update Android SDK version - 6.5.2

## v6.4.41
* Fix issue with deep linking
* `getAppsFlyerUID` support for uwp

## v6.4.4
* Update iOS SDK version - 6.4.4
* Update Android SDK version - 6.4.3
* Fixed issue with deep linking

## v6.4.1
* Update iOS SDK version - 6.4.1
* Update Android SDK version - 6.4.1
* Fixed a bug causing a crash in Unity apps

## v6.4.0
* Update iOS SDK version - 6.4.0
* Update Android SDK version - 6.4.0
* new API `setSharingFilterForPartners`
* Deprecated API `setSharingFilterForAllPartners` and `setSharingFilter`
* Android Target API updated to 30

## v6.3.5
* Update iOS SDK version - 6.3.5
* Fix issue with Facebook login
* Fix issue with uwp

## v6.3.2
* Update iOS SDK version - 6.3.2
* Update Android SDK version - 6.3.2
* new Android API `setDisableAdvertisingIdentifiers`

## v6.3.1
* Update iOS SDK version - 6.3.1


## v6.3.0
* Support for UWP
* Update iOS SDK version - 6.3.0
* Update Android SDK version - 6.3.0

## v6.2.63
* fix swizzling DDL

## v6.2.62

* fix setOneLinkCustomDomains API
* fix swizzling

## v6.2.61

* Fix android dependency

## v6.2.6

* Update iOS SDK version - 6.2.6
* Update Android SDK version - 6.2.3

## v6.2.5

* Update iOS SDK version - 6.2.5
* Deprecated setShouldCollectDeviceName
* AttributionObject to handle DeepLink

## v6.2.41

* Fix Skad issue

## v6.2.4

* RD-59026 - iOS SDK Version - 6.2.4

## v6.2.3

* RD-54266 - iOS SDK Version - 6.2.3

## v6.2.2

* RD-54266 - iOS SDK Version - 6.2.2
* RD-54266 - Android SDK Version - 6.2.0

## v6.2.0

* RD-55161 - Fixed don't call start before startSDK() (iOS) 
* RD-55566 - Fix onAppOpenAttribution called from kill for swizziling class (iOS)
* RD-45032 - Send reponse code in Purchase Validation Error (iOS) 
* RD-54266 - iOS SDK Version - 6.2.0
* RD-54266 - Android SDK Version - 6.1.4

## v6.1.4

* RD-55566 - Fix onAppOpenAttribution called from kill

## v6.1.3

* RD-50954 - Added Unified Deep Linking API
* RD-54264 - Added addPushNotificationDeepLinkPath api for iOS & Android
* RD-54266 - iOS SDK Version - 6.1.3
* RD-54266 - Android SDK Version - 6.1.3

## v6.1.0

* iOS SDK Version - 6.1.1
* Android SDK Version - 6.1.0
* Added onRequestResponse and onInAppResponse events.

## v6.0.7

* RD-49435 - ios swizzle options fix
* iOS SDK Version - 6.0.7


## v6.0.6

* RD-48888 - continueUserActivity remove super call
* RD-48915 - AppsFlyer+AppController update to lastest version

## v6.0.5

* iOS SDK Version - 6.0.5


## 6.0.3

* RD-44538 - empty game object fix
* Added disableSKAdNetwork api
* Added waitForATTUserAuthorizationWithTimeoutInterval api
* Update android [installreferrer](https://mvnrepository.com/artifact/com.android.installreferrer/installreferrer) to 2.1 
* Android SDK Version - 5.4.3
* iOS SDK Version - 6.0.3


## 5.4.2

* RD-43178 - added setSharingFilterForAllPartners() api
* RD-43178 - added setSharingFilter(params string[] partners) api
* RD-42761 - fix validateAndSendInAppPurchase callback on iOS

## 5.4.1

* RD-40404 - add additional params for recordCrossPromoteImpression api (ios & android)
* RD-42760 - add setPhoneNumber api (ios & android)
* RD-42761 - fix validateAndSendInAppPurchase callback on iOS

* Android SDK Version - 5.4.1
* iOS SDK Version     - 5.4.1

## 5.3.1

* RD-39294 - add super call from continueUserActivity
* RD-39255 - make IAppsFlyerConversionData public	
* RD-39254 - add setCollectOaid api (android)
* RD-39216 - add handleOpenUrl api (iOS)


## 5.3.0

* Android SDK Version - 5.3.0
* iOS SDK Version.    - 5.3.0
* Prefab fix for unity 2018.2.21f

## 5.2.1

* RD-37433 - Fix for 'duplicate symbol '__sendEvent' issue

## 5.2.0


* Android SDK Version - 5.2.0
* iOS SDK Version.    - 5.2.0

Changes and fixes: 
 - New plugin with breaking changes. Please see migration doc with details.


