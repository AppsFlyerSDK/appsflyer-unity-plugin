//
//  AppsFlyeriOSWarpper.mm
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import "AppsFlyeriOSWrapper.h"
#import <objc/runtime.h> 

#import <StoreKit/StoreKit.h>
#import "UnityFramework/UnityFramework-Swift.h"

#if __has_include(<PurchaseConnector/PurchaseConnector-Swift.h>)
#import <PurchaseConnector/PurchaseConnector-Swift.h>
#elif __has_include("PurchaseConnector-Swift.h")
#import "PurchaseConnector-Swift.h"
#endif

#if __has_include(<UnityFramework/UnityFramework-Swift.h>)
#import <UnityFramework/UnityFramework-Swift.h>
#elif __has_include("UnityFramework-Swift.h")
#import "UnityFramework-Swift.h"
#endif

static void unityCallBack(NSString* objectName, const char* method, const char* msg) {
    if(objectName){
        UnitySendMessage([objectName UTF8String], method, msg);
    }
}

extern "C" {
 
    const void _startSDK(bool shouldCallback, const char* objectName) {
        [[AppsFlyerLib shared] setPluginInfoWith: AFSDKPluginUnity
                                pluginVersion:@"6.17.1"
                                additionalParams:nil];
        startRequestObjectName = stringFromChar(objectName);
        AppsFlyeriOSWarpper.didCallStart = YES;
        [AppsFlyerAttribution shared].isBridgeReady = YES;
        [[NSNotificationCenter defaultCenter] postNotificationName:AF_BRIDGE_SET object: [AppsFlyerAttribution shared]];
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

    const void _setDeepLinkTimeout (long  deepLinkTimeout) {
        [AppsFlyerLib shared].deepLinkTimeout = deepLinkTimeout;
    }

    const void _anonymizeUser (bool anonymizeUser) {
        [AppsFlyerLib shared].anonymizeUser = anonymizeUser;
    }

    const void _enableTCFDataCollection (bool shouldCollectTcfData) {
       [[AppsFlyerLib shared] enableTCFDataCollection:shouldCollectTcfData];
    }

    const void _setConsentData(const char* isUserSubjectToGDPR, const char* hasConsentForDataUsage, const char* hasConsentForAdsPersonalization, const char* hasConsentForAdStorage) {
    
        NSNumber *gdpr = intFromNullableBool(isUserSubjectToGDPR);
        NSNumber *dataUsage = intFromNullableBool(hasConsentForDataUsage);
        NSNumber *adsPersonalization = intFromNullableBool(hasConsentForAdsPersonalization);
        NSNumber *adStorage = intFromNullableBool(hasConsentForAdStorage);

        AppsFlyerConsent *consentData = [[AppsFlyerConsent alloc] initWithIsUserSubjectToGDPR:gdpr
                                                                       hasConsentForDataUsage:dataUsage
                                                           hasConsentForAdsPersonalization:adsPersonalization
                                                                   hasConsentForAdStorage:adStorage];

        [[AppsFlyerLib shared] setConsentData:consentData];
    }

    const void _logAdRevenue(const char* monetizationNetwork, int mediationNetworkInt, const char* currencyIso4217Code, double eventRevenue, const char* additionalParameters) {
        AppsFlyerAdRevenueMediationNetworkType mediationNetwork = mediationNetworkTypeFromInt(mediationNetworkInt);
        NSNumber *number = [NSNumber numberWithDouble:eventRevenue];
        AFAdRevenueData *adRevenue = [[AFAdRevenueData alloc] initWithMonetizationNetwork:stringFromChar(monetizationNetwork) mediationNetwork:mediationNetwork currencyIso4217Code:stringFromChar(currencyIso4217Code) eventRevenue:number];
        [[AppsFlyerLib shared] logAdRevenue: adRevenue additionalParameters:dictionaryFromJson(additionalParameters)];
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
            [[AppsFlyerLib shared] setOneLinkCustomDomains:NSArrayFromCArray(length, oneLinkCustomDomains)];
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

    const void _setSharingFilterForPartners (int length, const char **partners) {
        if(length > 0 && partners) {
            [[AppsFlyerLib shared] setSharingFilterForPartners:NSArrayFromCArray(length, partners)];
        } else {
            [[AppsFlyerLib shared] setSharingFilterForPartners:nil];
        }
    }
    
    const void _validateAndSendInAppPurchase (const char* productIdentifier, const char* price, const char* currency, const char* transactionId, const char* additionalParameters, const char* objectName) {

        validateObjectName = stringFromChar(objectName);

        [[AppsFlyerLib shared]
         validateAndLogInAppPurchase:stringFromChar(productIdentifier)
         price:stringFromChar(price)
         currency:stringFromChar(currency)
         transactionId:stringFromChar(transactionId)
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

    const void _validateAndSendInAppPurchaseV2 (const char* product, const char* price, const char* currency, const char* transactionId, const char* extraEventValues, const char* objectName) {

        validateAndLogObjectName = stringFromChar(objectName);
        AFSDKPurchaseDetails *details = [[AFSDKPurchaseDetails alloc] initWithProductId:stringFromChar(product) price:stringFromChar(price) currency:stringFromChar(currency) transactionId:stringFromChar(transactionId)];

        [[AppsFlyerLib shared]
         validateAndLogInAppPurchase:details
         extraEventValues:dictionaryFromJson(extraEventValues)
         completionHandler:^(AFSDKValidateAndLogResult * _Nullable result) {
            if (result.status == AFSDKValidateAndLogStatusSuccess) {
                unityCallBack(validateAndLogObjectName, VALIDATE_AND_LOG_V2_CALLBACK, stringFromdictionary(result.result));
            } else if (result.status == AFSDKValidateAndLogStatusFailure) {
                 unityCallBack(validateAndLogObjectName, VALIDATE_AND_LOG_V2_CALLBACK, stringFromdictionary(result.errorData));
            } else {
                unityCallBack(validateAndLogObjectName, VALIDATE_AND_LOG_V2_ERROR_CALLBACK, stringFromdictionary(dictionaryFromNSError(result.error)));
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

    const void _setCurrentDeviceLanguage(const char* language) {
        [[AppsFlyerLib shared] setCurrentDeviceLanguage:stringFromChar(language)];
    }

    const void _setPartnerData(const char* partnerId, const char* partnerInfo) {
        [[AppsFlyerLib shared] setPartnerDataWithPartnerId: stringFromChar(partnerId) partnerInfo:dictionaryFromJson(partnerInfo)];
    }

    const void _disableIDFVCollection(bool isDisabled) {
        [AppsFlyerLib shared].disableIDFVCollection = isDisabled;
    }

    // Purchase connector
    const void _startObservingTransactions() {
        [[PurchaseConnector shared] startObservingTransactions];
    }

    const void _stopObservingTransactions() {
        [[PurchaseConnector shared] stopObservingTransactions];
    }

    const void _setIsSandbox(bool isSandBox) {
        [[PurchaseConnector shared] setIsSandbox:isSandBox];
    }

    const void _setPurchaseRevenueDelegate() {
        if (_AppsFlyerdelegate== nil) {
            _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
               }
        [[PurchaseConnector shared] setPurchaseRevenueDelegate:_AppsFlyerdelegate];
    }

    const void _setAutoLogPurchaseRevenue(int option) {
           [[PurchaseConnector shared] setAutoLogPurchaseRevenue:option];

    }

    const void _initPurchaseConnector(const char* objectName) {
        if (_AppsFlyerdelegate == nil) {
            _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
        }
        onPurchaseValidationObjectName = stringFromChar(objectName);
    }

    const void _setPurchaseRevenueDataSource(const char* objectName) {
        if (_AppsFlyerdelegate == nil) {
            _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
        }

        if (strstr(objectName, "StoreKit2") != NULL) {
            
            // Force protocol conformance
            Protocol *sk2Protocol = @protocol(AppsFlyerPurchaseRevenueDataSourceStoreKit2);
            class_addProtocol([_AppsFlyerdelegate class], sk2Protocol);
            
            if (![_AppsFlyerdelegate conformsToProtocol:@protocol(AppsFlyerPurchaseRevenueDataSourceStoreKit2)]) {
                NSLog(@"[AppsFlyer] Warning: SK2 protocol not conformed!");
            }
        }
        
        [PurchaseConnector shared].purchaseRevenueDataSource = _AppsFlyerdelegate;
    }

    const void _setStoreKitVersion(int storeKitVersion) {
        [[PurchaseConnector shared] setStoreKitVersion:(AFSDKStoreKitVersion)storeKitVersion];
    }

    const void _logConsumableTransaction(const char* transactionId) {
        if (@available(iOS 15.0, *)) {
            NSString *transactionIdStr = [NSString stringWithUTF8String:transactionId];
            [AFUnityStoreKit2Bridge fetchAFSDKTransactionSK2WithTransactionId:transactionIdStr completion:^(AFSDKTransactionSK2 *afTransaction) {
                if (afTransaction) {
                    [[PurchaseConnector shared] logConsumableTransaction:afTransaction];
                } else {
                    NSLog(@"No AFSDKTransactionSK2 found for id %@", transactionIdStr);
                }
            }];
        }
    }

    #ifdef __cplusplus
    extern "C" {
    #endif

    typedef const char *(*UnityPurchaseCallback)(const char *, const char *);

    UnityPurchaseCallback UnityPurchasesGetAdditionalParamsCallback = NULL;
    UnityPurchaseCallback UnityPurchasesGetAdditionalParamsCallbackSK2 = NULL;

    __attribute__((visibility("default")))
    void RegisterUnityPurchaseRevenueParamsCallback(UnityPurchaseCallback callback) {
        UnityPurchasesGetAdditionalParamsCallback = callback;
    }

    __attribute__((visibility("default")))
    void RegisterUnityPurchaseRevenueParamsCallbackSK2(UnityPurchaseCallback callback) {
        UnityPurchasesGetAdditionalParamsCallbackSK2 = callback;
    }


    #ifdef __cplusplus
    }
    #endif
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
        [dict setValue:@(result.deepLink.isDeferred) forKey:@"is_deferred"];
    }
    
    unityCallBack(onDeeplinkingObjectName, ON_DEEPLINKING, stringFromdictionary(dict));
}

// Purchase Connector
- (void)didReceivePurchaseRevenueValidationInfo:(NSDictionary *)validationInfo error:(NSError *)error {
    if (error != nil) {
        unityCallBack(onPurchaseValidationObjectName, PURCHASE_REVENUE_ERROR_CALLBACK, [[error localizedDescription] UTF8String]);
    } else {
        unityCallBack(onPurchaseValidationObjectName, PURCHASE_REVENUE_VALIDATION_CALLBACK, stringFromdictionary(validationInfo));
    }
}

- (NSDictionary *)purchaseRevenueAdditionalParametersForProducts:(NSSet<SKProduct *> *)products
                                                     transactions:(NSSet<SKPaymentTransaction *> *)transactions {

    NSMutableArray *productsArray = [NSMutableArray array];
    for (SKProduct *product in products) {
        [productsArray addObject:@{
            @"productIdentifier": product.productIdentifier ?: @"",
            @"localizedTitle": product.localizedTitle ?: @"",
            @"localizedDescription": product.localizedDescription ?: @"",
            @"price": [product.price stringValue] ?: @""
        }];
    }

    NSMutableArray *transactionsArray = [NSMutableArray array];
    for (SKPaymentTransaction *txn in transactions) {
        [transactionsArray addObject:@{
            @"transactionIdentifier": txn.transactionIdentifier ?: @"",
            @"transactionState": @(txn.transactionState),
            @"transactionDate": txn.transactionDate ? [@(txn.transactionDate.timeIntervalSince1970) stringValue] : @""
        }];
    }

    NSDictionary *input = @{
        @"products": productsArray,
        @"transactions": transactionsArray
    };

    NSError *error = nil;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:input options:0 error:&error];
    if (error || !jsonData) {
        NSLog(@"[AppsFlyer] Failed to serialize Unity purchase data: %@", error);
        return @{};
    }

    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    if (!jsonString || !UnityPurchasesGetAdditionalParamsCallback) {
        NSLog(@"[AppsFlyer] Unity callback not registered");
        return @{};
    }

    const char *resultCStr = UnityPurchasesGetAdditionalParamsCallback([jsonString UTF8String], "");
    if (!resultCStr) {
        NSLog(@"[AppsFlyer] Unity callback returned null");
        return @{};
    }

    NSString *resultJson = [NSString stringWithUTF8String:resultCStr];
    NSData *resultData = [resultJson dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *parsedResult = [NSJSONSerialization JSONObjectWithData:resultData options:0 error:&error];

    if (error || ![parsedResult isKindOfClass:[NSDictionary class]]) {
        NSLog(@"[AppsFlyer] Failed to parse Unity response: %@", error);
        return @{};
    }

    return parsedResult;
}

#pragma mark - AppsFlyerPurchaseRevenueDataSourceStoreKit2
- (NSDictionary *)purchaseRevenueAdditionalParametersStoreKit2ForProducts:(NSSet<AFSDKProductSK2 *> *)products transactions:(NSSet<AFSDKTransactionSK2 *> *)transactions {
    if (@available(iOS 15.0, *)) {
        NSArray *productInfoArray = [AFUnityStoreKit2Bridge extractSK2ProductInfo:[products allObjects]];
        NSArray *transactionInfoArray = [AFUnityStoreKit2Bridge extractSK2TransactionInfo:[transactions allObjects]];

        NSDictionary *input = @{
            @"products": productInfoArray,
            @"transactions": transactionInfoArray
        };

        if (UnityPurchasesGetAdditionalParamsCallbackSK2) {
            NSError *error = nil;
            NSData *jsonData = [NSJSONSerialization dataWithJSONObject:input options:0 error:&error];
            if (error || !jsonData) {
                NSLog(@"[AppsFlyer] Failed to serialize Unity purchase data: %@", error);
                return @{};
            }

            NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
            
            const char *resultCStr = UnityPurchasesGetAdditionalParamsCallbackSK2([jsonString UTF8String], "");
            if (!resultCStr) {
                NSLog(@"[AppsFlyer] Unity callback returned null");
                return @{};
            }

            NSString *resultJson = [NSString stringWithUTF8String:resultCStr];
            
            NSData *resultData = [resultJson dataUsingEncoding:NSUTF8StringEncoding];
            NSDictionary *parsedResult = [NSJSONSerialization JSONObjectWithData:resultData options:0 error:&error];

            if (error || ![parsedResult isKindOfClass:[NSDictionary class]]) {
                NSLog(@"[AppsFlyer] Failed to parse Unity response: %@", error);
                return @{};
            }

            return parsedResult;
        } else {
            NSLog(@"[AppsFlyer] SK2 - Unity callback is NOT registered");
        }
    } else {
        NSLog(@"[AppsFlyer] SK2 - iOS version not supported");
    }
    return @{};
}

@end


