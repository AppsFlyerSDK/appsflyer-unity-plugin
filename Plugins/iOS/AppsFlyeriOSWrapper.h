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
#else
#import "PurchaseConnector.h"
#endif
#import <PurchaseConnector/PurchaseConnector-Swift.h>

// Add StoreKit 2 support
#if __has_include(<StoreKit/StoreKit.h>)
#import <StoreKit/StoreKit.h>
#endif

@interface AppsFlyeriOSWarpper : NSObject <AppsFlyerLibDelegate, AppsFlyerDeepLinkDelegate, AppsFlyerPurchaseRevenueDelegate, AppsFlyerPurchaseRevenueDataSource, AppsFlyerPurchaseRevenueDataSourceStoreKit2>

+ (BOOL) didCallStart;
+ (void) setDidCallStart:(BOOL)val;

// Add StoreKit 2 methods
- (void)setStoreKitVersion:(int)storeKitVersion;
- (void)logConsumableTransaction:(id)transaction;

@end


static AppsFlyeriOSWarpper *_AppsFlyerdelegate;
static const int kPushNotificationSize = 32;

static NSString* ConversionDataCallbackObject = @"AppsFlyerObject";

static const char* VALIDATE_CALLBACK = "didFinishValidateReceipt";
static const char* VALIDATE_ERROR_CALLBACK = "didFinishValidateReceiptWithError";
static const char* GCD_CALLBACK = "onConversionDataSuccess";
static const char* GCD_ERROR_CALLBACK = "onConversionDataFail";
static const char* OAOA_CALLBACK = "onAppOpenAttribution";
static const char* OAOA_ERROR_CALLBACK = "onAppOpenAttributionFailure";
static const char* GENERATE_LINK_CALLBACK = "onInviteLinkGenerated";
static const char* OPEN_STORE_LINK_CALLBACK = "onOpenStoreLinkGenerated";
static const char* START_REQUEST_CALLBACK = "requestResponseReceived";
static const char* IN_APP_RESPONSE_CALLBACK = "inAppResponseReceived";
static const char* ON_DEEPLINKING = "onDeepLinking";
static const char* VALIDATE_AND_LOG_V2_CALLBACK = "onValidateAndLogComplete";
static const char* VALIDATE_AND_LOG_V2_ERROR_CALLBACK = "onValidateAndLogFailure";


static NSString* validateObjectName = @"";
static NSString* openStoreObjectName = @"";
static NSString* generateInviteObjectName = @"";
static NSString* validateAndLogObjectName = @"";
static NSString* startRequestObjectName = @"";
static NSString* inAppRequestObjectName = @"";
static NSString* onDeeplinkingObjectName = @"";

static const char* PURCHASE_REVENUE_VALIDATION_CALLBACK = "didReceivePurchaseRevenueValidationInfo";
static const char* PURCHASE_REVENUE_ERROR_CALLBACK = "didReceivePurchaseRevenueError";

static NSString* onPurchaseValidationObjectName = @"";
