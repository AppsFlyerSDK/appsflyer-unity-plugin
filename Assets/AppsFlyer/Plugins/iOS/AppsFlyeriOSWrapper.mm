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

#if __has_include(<AppsFlyerLib/AppsFlyerLib-Swift.h>)
#import <AppsFlyerLib/AppsFlyerLib-Swift.h>
#elif __has_include("AppsFlyerLib-Swift.h")
#import "AppsFlyerLib-Swift.h"
#endif

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

    // Purchase connector
#if __has_include(<PurchaseConnector/PurchaseConnector-Swift.h>) || __has_include("PurchaseConnector-Swift.h")
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
#else
    const void _startObservingTransactions() {}
    const void _stopObservingTransactions() {}
    const void _setIsSandbox(bool isSandBox) {}
    const void _setPurchaseRevenueDelegate() {}
    const void _setAutoLogPurchaseRevenue(int option) {}
    const void _initPurchaseConnector(const char* objectName) {}
    const void _setPurchaseRevenueDataSource(const char* objectName) {}
    const void _setStoreKitVersion(int storeKitVersion) {}
    const void _logConsumableTransaction(const char* transactionId) {}
#endif

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

// Purchase Connector
#if __has_include(<PurchaseConnector/PurchaseConnector-Swift.h>) || __has_include("PurchaseConnector-Swift.h")
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
#endif // PurchaseConnector

@end


