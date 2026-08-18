//
//  AppsFlyeriOSWarpper.h
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import "AFUnityUtils.mm"
#import "UnityAppController.h"
#import "AppsFlyerAttribution.h"
#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif
#if __has_include(<PurchaseConnector/PurchaseConnector.h>)
#import <PurchaseConnector/PurchaseConnector.h>
#import <PurchaseConnector/PurchaseConnector-Swift.h>
#elif __has_include("PurchaseConnector.h")
#import "PurchaseConnector.h"
#endif

// Add StoreKit 2 support
#if __has_include(<StoreKit/StoreKit.h>)
#import <StoreKit/StoreKit.h>
#endif

#if __has_include(<PurchaseConnector/PurchaseConnector.h>) || __has_include("PurchaseConnector.h")
@interface AppsFlyeriOSWarpper : NSObject <AppsFlyerLibDelegate, AppsFlyerPurchaseRevenueDelegate, AppsFlyerPurchaseRevenueDataSource, AppsFlyerPurchaseRevenueDataSourceStoreKit2>
#else
@interface AppsFlyeriOSWarpper : NSObject <AppsFlyerLibDelegate>
#endif

+ (BOOL) didCallStart;
+ (void) setDidCallStart:(BOOL)val;

#if __has_include(<PurchaseConnector/PurchaseConnector.h>) || __has_include("PurchaseConnector.h")
- (void)setStoreKitVersion:(int)storeKitVersion;
- (void)logConsumableTransaction:(id)transaction;
#endif

@end


static AppsFlyeriOSWarpper *_AppsFlyerdelegate;

static NSString* ConversionDataCallbackObject = @"AppsFlyerObject";

static const char* GCD_CALLBACK = "onConversionDataSuccess";
static const char* GCD_ERROR_CALLBACK = "onConversionDataFail";
static const char* OAOA_CALLBACK = "onAppOpenAttribution";
static const char* OAOA_ERROR_CALLBACK = "onAppOpenAttributionFailure";

static const char* PURCHASE_REVENUE_VALIDATION_CALLBACK = "didReceivePurchaseRevenueValidationInfo";
static const char* PURCHASE_REVENUE_ERROR_CALLBACK = "didReceivePurchaseRevenueError";

static NSString* onPurchaseValidationObjectName = @"";
