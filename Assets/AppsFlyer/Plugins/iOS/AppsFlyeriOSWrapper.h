//
//  AppsFlyeriOSWarpper.h
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import "AFUnityUtils.mm"
#import "UnityAppController.h"
#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif

@interface AppsFlyeriOSWarpper : NSObject <AppsFlyerLibDelegate, AppsFlyerDeepLinkDelegate>
+ (BOOL) didCallStart;
+ (void) setDidCallStart:(BOOL)val;
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


static NSString* validateObjectName = @"";
static NSString* openStoreObjectName = @"";
static NSString* generateInviteObjectName = @"";
static NSString* startRequestObjectName = @"";
static NSString* inAppRequestObjectName = @"";
static NSString* onDeeplinkingObjectName = @"";

