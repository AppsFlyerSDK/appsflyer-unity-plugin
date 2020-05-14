//
//  AppsFlyeriOSWarpper.mm
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import "AppsFlyeriOSWrapper.h"


static void unityCallBack(NSString* objectName, const char* method, const char* msg) {
    if(objectName){
        UnitySendMessage([objectName UTF8String], method, msg);
    }
}

extern "C" {
    
    const void _startSDK() {
        [[AppsFlyerTracker sharedTracker] trackAppLaunch];
    }
    
    const void _setCustomerUserID (const char* customerUserID) {
        [[AppsFlyerTracker sharedTracker] setCustomerUserID:stringFromChar(customerUserID)];
    }

    const void _setAdditionalData (const char* customData) {
        [[AppsFlyerTracker sharedTracker] setAdditionalData:dictionaryFromJson(customData)];
    }

    const void _setAppsFlyerDevKey (const char* appsFlyerDevKey) {
        [AppsFlyerTracker sharedTracker].appsFlyerDevKey = stringFromChar(appsFlyerDevKey);
    }

    const void _setAppleAppID (const char* appleAppID) {
        [AppsFlyerTracker sharedTracker].appleAppID = stringFromChar(appleAppID);
    }

    const void _setCurrencyCode (const char* currencyCode) {
        [[AppsFlyerTracker sharedTracker] setCurrencyCode:stringFromChar(currencyCode)];
    }

    const void _setDisableCollectAppleAdSupport (bool disableAppleAdSupportTracking) {
        [AppsFlyerTracker sharedTracker].disableAppleAdSupportTracking = disableAppleAdSupportTracking;
    }

    const void _setIsDebug (bool isDebug) {
        [AppsFlyerTracker sharedTracker].isDebug = isDebug;
    }

    const void _setShouldCollectDeviceName (bool shouldCollectDeviceName) {
        [AppsFlyerTracker sharedTracker].shouldCollectDeviceName = shouldCollectDeviceName;
    }

    const void _setAppInviteOneLinkID (const char*  appInviteOneLinkID) {
        [[AppsFlyerTracker sharedTracker] setAppInviteOneLink:stringFromChar(appInviteOneLinkID)];
    }

    const void _anonymizeUser (bool deviceTrackingDisabled) {
        [AppsFlyerTracker sharedTracker].deviceTrackingDisabled = deviceTrackingDisabled;
    }

    const void _setDisableCollectIAd (bool disableIAdTracking) {
        [AppsFlyerTracker sharedTracker].disableIAdTracking = disableIAdTracking;
    }
    
    const void _setUseReceiptValidationSandbox (bool useReceiptValidationSandbox) {
        [AppsFlyerTracker sharedTracker].useReceiptValidationSandbox = useReceiptValidationSandbox;
    }
    
    const void _setUseUninstallSandbox (bool useUninstallSandbox) {
        [AppsFlyerTracker sharedTracker].useUninstallSandbox = useUninstallSandbox;
    }

    const void _setResolveDeepLinkURLs (int length, const char **resolveDeepLinkURLs) {
        if(length > 0 && resolveDeepLinkURLs) {
            [[AppsFlyerTracker sharedTracker] setResolveDeepLinkURLs:NSArrayFromCArray(length, resolveDeepLinkURLs)];
        }
    }

    const void _setOneLinkCustomDomains (int length, const char **oneLinkCustomDomains) {
        if(length > 0 && oneLinkCustomDomains) {
            [[AppsFlyerTracker sharedTracker] setResolveDeepLinkURLs:NSArrayFromCArray(length, oneLinkCustomDomains)];
        }
    }

    const void _afSendEvent (const char* eventName, const char* eventValues) {
        [[AppsFlyerTracker sharedTracker] trackEvent:stringFromChar(eventName) withValues:dictionaryFromJson(eventValues)];
    }

    const void _recordLocation (double longitude, double latitude) {
        [[AppsFlyerTracker sharedTracker] trackLocation:longitude latitude:latitude];
    }

    const char* _getAppsFlyerId () {
        return getCString([[[AppsFlyerTracker sharedTracker] getAppsFlyerUID] UTF8String]);
    }

    const void _registerUninstall (unsigned char* deviceToken) {
        if(deviceToken){
            NSData* tokenData = [NSData dataWithBytes:(const void *)deviceToken length:sizeof(unsigned char)*kPushNotificationSize];
            [[AppsFlyerTracker sharedTracker] registerUninstall:tokenData];
        }
    }

    const void _handlePushNotification (const char* pushPayload) {
        [[AppsFlyerTracker sharedTracker] handlePushNotification:dictionaryFromJson(pushPayload)];
    }

    const char* _getSDKVersion () {
        return getCString([[[AppsFlyerTracker sharedTracker] getSDKVersion] UTF8String]);
    }

    const void _setHost (const char* host, const char* hostPrefix) {
        [[AppsFlyerTracker sharedTracker] setHost:stringFromChar(host) withHostPrefix:stringFromChar(hostPrefix)];
    }

    const void _setMinTimeBetweenSessions (int minTimeBetweenSessions) {
        [AppsFlyerTracker sharedTracker].minTimeBetweenSessions = minTimeBetweenSessions;
    }

    const void _stopSDK (bool isStopTracking) {
        [AppsFlyerTracker sharedTracker].isStopTracking = isStopTracking;
    }

    const BOOL _isSDKStopped () {
        return [AppsFlyerTracker sharedTracker].isStopTracking;
    }

    const void _handleOpenUrl(const char *url, const char *sourceApplication, const char *annotation) {
        [[AppsFlyerTracker sharedTracker] handleOpenURL:[NSURL URLWithString:stringFromChar(url)] sourceApplication:stringFromChar(sourceApplication) withAnnotation:stringFromChar(annotation)];
    }

    const void _recordCrossPromoteImpression (const char* appID, const char* campaign) {
        [AppsFlyerCrossPromotionHelper trackCrossPromoteImpression:stringFromChar(appID) campaign:stringFromChar(campaign)];
    }
    
    const void _attributeAndOpenStore (const char* appID, const char* campaign, const char* parameters, const char* objectName) {
        [AppsFlyerCrossPromotionHelper
         trackAndOpenStore:stringFromChar(appID)
         campaign:stringFromChar(campaign)
         paramters:dictionaryFromJson(parameters)
         openStore:^(NSURLSession * _Nonnull urlSession, NSURL * _Nonnull clickURL) {
             unityCallBack(stringFromChar(objectName), OPEN_STORE_LINK_CALLBACK, [clickURL.absoluteString UTF8String]);
             
         }];
    }
    
    const void _generateUserInviteLink (const char* parameters, const char* objectName) {
        [AppsFlyerShareInviteHelper generateInviteUrlWithLinkGenerator:^AppsFlyerLinkGenerator * _Nonnull(AppsFlyerLinkGenerator * _Nonnull generator) {
            return generatorFromDictionary(dictionaryFromJson(parameters), generator);
        } completionHandler:^(NSURL * _Nullable url) {
            unityCallBack(stringFromChar(objectName), GENERATE_LINK_CALLBACK, [url.absoluteString UTF8String]);
        }];
    }
    
    const void _recordInvite (const char* channel, const char* parameters) {
        [AppsFlyerShareInviteHelper trackInvite:stringFromChar(channel) parameters:dictionaryFromJson(parameters)];
    }
    
    const void _setUserEmails (int emailCryptTypeInt , int length, const char **userEmails) {
        if(length > 0 && userEmails) {
            [[AppsFlyerTracker sharedTracker] setUserEmails:NSArrayFromCArray(length, userEmails) withCryptType:emailCryptTypeFromInt(emailCryptTypeInt)];
        }
    }
    
    const void _validateAndSendInAppPurchase (const char* productIdentifier, const char* price, const char* currency, const char* tranactionId, const char* additionalParameters, const char* objectName) {
        
        [[AppsFlyerTracker sharedTracker]
         validateAndTrackInAppPurchase:stringFromChar(productIdentifier)
         price:stringFromChar(price)
         currency:stringFromChar(currency)
         transactionId:stringFromChar(tranactionId)
         additionalParameters:dictionaryFromJson(additionalParameters)
         success:^(NSDictionary *result){
                 unityCallBack(stringFromChar(objectName), VALIDATE_CALLBACK, stringFromdictionary(result));
         } failure:^(NSError *error, id response) {
                 unityCallBack(stringFromChar(objectName), VALIDATE_ERROR_CALLBACK, error ? [[error localizedDescription] UTF8String] : "error");
         }];
    }
    
    const void _getConversionData(const char* objectName) {
        if (_AppsFlyerdelegate == nil) {
            _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
        }
        ConversionDataCallbackObject = stringFromChar(objectName);
        [[AppsFlyerTracker sharedTracker] setDelegate:_AppsFlyerdelegate];
    }
}

@implementation AppsFlyeriOSWarpper

- (void)onConversionDataSuccess:(NSDictionary *)installData {
    unityCallBack(ConversionDataCallbackObject, GCD_CALLBACK, stringFromdictionary(installData));
}

- (void)onConversionDataFail:(NSError *)error {
    unityCallBack(ConversionDataCallbackObject, GCD_ERROR_CALLBACK, [[error localizedDescription] UTF8String]);
}

- (void)onAppOpenAttribution:(NSDictionary *)attributionData {
    unityCallBack(ConversionDataCallbackObject, OAOA_CALLBACK, stringFromdictionary(attributionData));
}

- (void)onAppOpenAttributionFailure:(NSError *)error {
    unityCallBack(ConversionDataCallbackObject, OAOA_ERROR_CALLBACK, [[error localizedDescription] UTF8String]);
}

@end

