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
 
    const void _startSDK(bool shouldCallback, const char* objectName) {
        startRequestObjectName = stringFromChar(objectName);
        AppsFlyeriOSWarpper.didCallStart = YES;
        [[AppsFlyerLib shared] startWithCompletionHandler:^(NSDictionary<NSString *,id> *dictionary, NSError *error) {
            if(shouldCallback){
                if (error) {
                    NSDictionary *callbackDictionary = @{@"statusCode":[NSNumber numberWithLong:[error code]]};
                    unityCallBack(startRequestObjectName, START_REQUEST_CALLBACK, stringFromdictionary(callbackDictionary));
                    return;
                }
                if (dictionary) {
                    unityCallBack(startRequestObjectName, START_REQUEST_CALLBACK, stringFromdictionary(dictionary));
                    return;
                }
            }
        }];
    }
    
    const void _setCustomerUserID (const char* customerUserID) {
        [[AppsFlyerLib shared] setCustomerUserID:stringFromChar(customerUserID)];
    }

    const void _setAdditionalData (const char* customData) {
        [[AppsFlyerLib shared] setAdditionalData:dictionaryFromJson(customData)];
    }

    const void _setAppsFlyerDevKey (const char* appsFlyerDevKey) {
        [AppsFlyerLib shared].appsFlyerDevKey = stringFromChar(appsFlyerDevKey);
    }

    const void _setAppleAppID (const char* appleAppID) {
        [AppsFlyerLib shared].appleAppID = stringFromChar(appleAppID);
    }

    const void _setCurrencyCode (const char* currencyCode) {
        [[AppsFlyerLib shared] setCurrencyCode:stringFromChar(currencyCode)];
    }

    const void _setDisableCollectAppleAdSupport (bool disableAdvertisingIdentifier) {
        [AppsFlyerLib shared].disableAdvertisingIdentifier = disableAdvertisingIdentifier;
    }

    const void _setIsDebug (bool isDebug) {
        [AppsFlyerLib shared].isDebug = isDebug;
    }

    const void _setShouldCollectDeviceName (bool shouldCollectDeviceName) {
        [AppsFlyerLib shared].shouldCollectDeviceName = shouldCollectDeviceName;
    }

    const void _setAppInviteOneLinkID (const char*  appInviteOneLinkID) {
        [[AppsFlyerLib shared] setAppInviteOneLink:stringFromChar(appInviteOneLinkID)];
    }

    const void _anonymizeUser (bool anonymizeUser) {
        [AppsFlyerLib shared].anonymizeUser = anonymizeUser;
    }

    const void _setDisableCollectIAd (bool disableCollectASA) {
        [AppsFlyerLib shared].disableCollectASA = disableCollectASA;
    }
    
    const void _setUseReceiptValidationSandbox (bool useReceiptValidationSandbox) {
        [AppsFlyerLib shared].useReceiptValidationSandbox = useReceiptValidationSandbox;
    }
    
    const void _setUseUninstallSandbox (bool useUninstallSandbox) {
        [AppsFlyerLib shared].useUninstallSandbox = useUninstallSandbox;
    }

    const void _setResolveDeepLinkURLs (int length, const char **resolveDeepLinkURLs) {
        if(length > 0 && resolveDeepLinkURLs) {
            [[AppsFlyerLib shared] setResolveDeepLinkURLs:NSArrayFromCArray(length, resolveDeepLinkURLs)];
        }
    }

    const void _setOneLinkCustomDomains (int length, const char **oneLinkCustomDomains) {
        if(length > 0 && oneLinkCustomDomains) {
            [[AppsFlyerLib shared] setResolveDeepLinkURLs:NSArrayFromCArray(length, oneLinkCustomDomains)];
        }
    }

    const void _afSendEvent (const char* eventName, const char* eventValues, bool shouldCallback, const char* objectName) {
        inAppRequestObjectName = stringFromChar(objectName);
        [[AppsFlyerLib shared] logEventWithEventName:stringFromChar(eventName) eventValues:dictionaryFromJson(eventValues) completionHandler:^(NSDictionary<NSString *,id> *dictionary, NSError *error) {
                if(shouldCallback){
                    if (error) {
                        NSDictionary *callbackDictionary = @{@"statusCode":[NSNumber numberWithLong:[error code]]};
                        unityCallBack(inAppRequestObjectName, IN_APP_RESPONSE_CALLBACK, stringFromdictionary(callbackDictionary));
                        return;
                    }
                    if (dictionary) {
                        unityCallBack(inAppRequestObjectName, IN_APP_RESPONSE_CALLBACK, stringFromdictionary(dictionary));
                        return;
                    }
                }
        }];
    }

    const void _recordLocation (double longitude, double latitude) {
        [[AppsFlyerLib shared] logLocation:longitude latitude:latitude];
    }

    const char* _getAppsFlyerId () {
        return getCString([[[AppsFlyerLib shared] getAppsFlyerUID] UTF8String]);
    }

    const void _registerUninstall (unsigned char* deviceToken) {
        if(deviceToken){
            NSData* tokenData = [NSData dataWithBytes:(const void *)deviceToken length:sizeof(unsigned char)*kPushNotificationSize];
            [[AppsFlyerLib shared] registerUninstall:tokenData];
        }
    }

    const void _handlePushNotification (const char* pushPayload) {
        [[AppsFlyerLib shared] handlePushNotification:dictionaryFromJson(pushPayload)];
    }

    const char* _getSDKVersion () {
        return getCString([[[AppsFlyerLib shared] getSDKVersion] UTF8String]);
    }

    const void _setHost (const char* host, const char* hostPrefix) {
        [[AppsFlyerLib shared] setHost:stringFromChar(host) withHostPrefix:stringFromChar(hostPrefix)];
    }

    const void _setMinTimeBetweenSessions (int minTimeBetweenSessions) {
        [AppsFlyerLib shared].minTimeBetweenSessions = minTimeBetweenSessions;
    }

    const void _stopSDK (bool isStopped) {
        [AppsFlyerLib shared].isStopped = isStopped;
    }

    const BOOL _isSDKStopped () {
        return [AppsFlyerLib shared].isStopped;
    }

    const void _handleOpenUrl(const char *url, const char *sourceApplication, const char *annotation) {
        [[AppsFlyerLib shared] handleOpenURL:[NSURL URLWithString:stringFromChar(url)] sourceApplication:stringFromChar(sourceApplication) withAnnotation:stringFromChar(annotation)];    }

    const void _recordCrossPromoteImpression (const char* appID, const char* campaign, const char* parameters) {
        [AppsFlyerCrossPromotionHelper logCrossPromoteImpression:stringFromChar(appID) campaign:stringFromChar(campaign) parameters:dictionaryFromJson(parameters)];    }
    
    const void _attributeAndOpenStore (const char* appID, const char* campaign, const char* parameters, const char* objectName) {

        openStoreObjectName = stringFromChar(objectName);

        [AppsFlyerCrossPromotionHelper
         logAndOpenStore:stringFromChar(appID)
         campaign:stringFromChar(campaign)
         parameters:dictionaryFromJson(parameters)
         openStore:^(NSURLSession * _Nonnull urlSession, NSURL * _Nonnull clickURL) {
            unityCallBack(openStoreObjectName, OPEN_STORE_LINK_CALLBACK, [clickURL.absoluteString UTF8String]);
        }];
    }
    
    const void _generateUserInviteLink (const char* parameters, const char* objectName) {

        generateInviteObjectName = stringFromChar(objectName);

        [AppsFlyerShareInviteHelper generateInviteUrlWithLinkGenerator:^AppsFlyerLinkGenerator * _Nonnull(AppsFlyerLinkGenerator * _Nonnull generator) {
            return generatorFromDictionary(dictionaryFromJson(parameters), generator);
        } completionHandler:^(NSURL * _Nullable url) {
            unityCallBack(generateInviteObjectName, GENERATE_LINK_CALLBACK, [url.absoluteString UTF8String]);
        }];
    }
    
    const void _recordInvite (const char* channel, const char* parameters) {
        [AppsFlyerShareInviteHelper logInvite:stringFromChar(channel) parameters:dictionaryFromJson(parameters)];
    }
    
    const void _setUserEmails (int emailCryptTypeInt , int length, const char **userEmails) {
        if(length > 0 && userEmails) {
            [[AppsFlyerLib shared] setUserEmails:NSArrayFromCArray(length, userEmails) withCryptType:emailCryptTypeFromInt(emailCryptTypeInt)];
        }
    }

    const void _setPhoneNumber (const char* phoneNumber) {
        [[AppsFlyerLib shared] setPhoneNumber:stringFromChar(phoneNumber)];
    }

    const void _setSharingFilterForAllPartners () {
        [[AppsFlyerLib shared] setSharingFilterForAllPartners];
    }

    const void _setSharingFilter (int length, const char **partners) {
        if(length > 0 && partners) {
            [[AppsFlyerLib shared] setSharingFilter:NSArrayFromCArray(length, partners)];
        }
    }
    
    const void _validateAndSendInAppPurchase (const char* productIdentifier, const char* price, const char* currency, const char* tranactionId, const char* additionalParameters, const char* objectName) {

        validateObjectName = stringFromChar(objectName);

        [[AppsFlyerLib shared]
         validateAndLogInAppPurchase:stringFromChar(productIdentifier)
         price:stringFromChar(price)
         currency:stringFromChar(currency)
         transactionId:stringFromChar(tranactionId)
         additionalParameters:dictionaryFromJson(additionalParameters)
         success:^(NSDictionary *result){
                 unityCallBack(validateObjectName, VALIDATE_CALLBACK, stringFromdictionary(result));
         } failure:^(NSError *error, id response) {
            if(response && [response isKindOfClass:[NSDictionary class]]) {
                 NSDictionary* value = (NSDictionary*)response;
                 unityCallBack(validateObjectName, VALIDATE_ERROR_CALLBACK, stringFromdictionary(value));
             } else {
                 unityCallBack(validateObjectName, VALIDATE_ERROR_CALLBACK, error ? [[error localizedDescription] UTF8String] : "error");
             }
         }];
    }
    
    const void _getConversionData(const char* objectName) {
        if (_AppsFlyerdelegate == nil) {
            _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
        }
        ConversionDataCallbackObject = stringFromChar(objectName);
        [[AppsFlyerLib shared] setDelegate:_AppsFlyerdelegate];
    }

    const void _waitForATTUserAuthorizationWithTimeoutInterval (int timeoutInterval) {
        [[AppsFlyerLib shared] waitForATTUserAuthorizationWithTimeoutInterval:timeoutInterval];
    }

    const void _disableSKAdNetwork (bool isDisabled) {
        [AppsFlyerLib shared].disableSKAdNetwork = isDisabled;
    }

    const void _addPushNotificationDeepLinkPath (int length, const char **paths) {
        if(length > 0 && paths) {
            [[AppsFlyerLib shared] addPushNotificationDeepLinkPath:NSArrayFromCArray(length, paths)];
        }
    }

    const void _subscribeForDeepLink (const char* objectName) {

        onDeeplinkingObjectName = stringFromChar(objectName);
        
        if (_AppsFlyerdelegate == nil) {
            _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
        }
        [[AppsFlyerLib shared] setDeepLinkDelegate:_AppsFlyerdelegate];
    }
}

@implementation AppsFlyeriOSWarpper

static BOOL didCallStart;
+ (BOOL) didCallStart
{ @synchronized(self) { return didCallStart; } }
+ (void) setDidCallStart:(BOOL)val
{ @synchronized(self) { didCallStart = val; } }

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

- (void)didResolveDeepLink:(AppsFlyerDeepLinkResult *)result{
    
    NSMutableDictionary *dict = [[NSMutableDictionary alloc] init];
    
    [dict setValue:stringFromDeepLinkResultError(result) forKey:@"error"];
    [dict setValue:stringFromDeepLinkResultStatus(result.status) forKey:@"status"];
    
    if(result && result.deepLink){
        [dict setValue:result.deepLink.description forKey:@"deepLink"];
    }
    
    unityCallBack(onDeeplinkingObjectName, ON_DEEPLINKING, stringFromdictionary(dict));
}

@end

