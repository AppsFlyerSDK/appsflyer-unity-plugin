//
//  AppsFlyer+AppController.m
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import <objc/runtime.h>
#import "UnityAppController.h"
#import "AppsFlyeriOSWrapper.h"
#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif


@implementation UnityAppController (AppsFlyerSwizzledAppController)

static BOOL didEnteredBackGround __unused;
static IMP __original_applicationDidBecomeActive_Imp __unused;
static IMP __original_applicationDidEnterBackground_Imp __unused;
static IMP __original_didReceiveRemoteNotification_Imp __unused;
static IMP __original_continueUserActivity_Imp __unused;
static IMP __original_openUrl_Imp __unused;


+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{

        id swizzleFlag = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"AppsFlyerShouldSwizzle"];
        BOOL shouldSwizzle = swizzleFlag ? [swizzleFlag boolValue] : NO;
        
        if(shouldSwizzle){
            
            Method method1 = class_getInstanceMethod([self class], @selector(applicationDidBecomeActive:));
            __original_applicationDidBecomeActive_Imp = method_setImplementation(method1, (IMP)__swizzled_applicationDidBecomeActive);
                    
            Method method2 = class_getInstanceMethod([self class], @selector(applicationDidEnterBackground:));
            __original_applicationDidEnterBackground_Imp = method_setImplementation(method2, (IMP)__swizzled_applicationDidEnterBackground);
            
           
            Method method3 = class_getInstanceMethod([self class], @selector(didReceiveRemoteNotification:));
            __original_didReceiveRemoteNotification_Imp = method_setImplementation(method3, (IMP)__swizzled_didReceiveRemoteNotification);
            
           
            Method method4 = class_getInstanceMethod([self class], @selector(application:openURL:options:));
            __original_openUrl_Imp = method_setImplementation(method4, (IMP)__swizzled_openURL);
            
            if (_AppsFlyerdelegate == nil) {
                _AppsFlyerdelegate = [[AppsFlyeriOSWarpper alloc] init];
            }
           
            [self swizzleContinueUserActivity:[self class]];
        }

    });
}

+(void)swizzleContinueUserActivity:(Class)class {
    
    SEL originalSelector = @selector(application:continueUserActivity:restorationHandler:);
    
    Method defaultMethod = class_getInstanceMethod(class, originalSelector);
    Method swizzledMethod = class_getInstanceMethod(class, @selector(__swizzled_continueUserActivity));
    
    BOOL isMethodExists = !class_addMethod(class, originalSelector, method_getImplementation(swizzledMethod), method_getTypeEncoding(swizzledMethod));
    
    if (isMethodExists) {
        __original_continueUserActivity_Imp = method_setImplementation(defaultMethod, (IMP)__swizzled_continueUserActivity);
    } else {
        class_replaceMethod(class, originalSelector, (IMP)__swizzled_continueUserActivity, method_getTypeEncoding(swizzledMethod));
    }
}

BOOL __swizzled_continueUserActivity(id self, SEL _cmd, UIApplication* application, NSUserActivity* userActivity, void (^restorationHandler)(NSArray*)) {
    NSLog(@"swizzled continueUserActivity");
    [[AppsFlyerAttribution shared] continueUserActivity:userActivity restorationHandler:restorationHandler];
    
    if(__original_continueUserActivity_Imp){
        return ((BOOL(*)(id, SEL, UIApplication*, NSUserActivity*, void (^)(NSArray*)))__original_continueUserActivity_Imp)(self, _cmd, application, userActivity, NULL);
    }
    
    return YES;
}



void __swizzled_applicationDidBecomeActive(id self, SEL _cmd, UIApplication* launchOptions) {
    NSLog(@"swizzled applicationDidBecomeActive");
    [[AppsFlyerLib shared] setDelegate:_AppsFlyerdelegate];

    if(didEnteredBackGround && AppsFlyeriOSWarpper.didCallStart == YES){
        [[AppsFlyerLib shared] start];
    }
    
    if(__original_applicationDidBecomeActive_Imp){
        ((void(*)(id,SEL, UIApplication*))__original_applicationDidBecomeActive_Imp)(self, _cmd, launchOptions);
    }
}


void __swizzled_applicationDidEnterBackground(id self, SEL _cmd, UIApplication* application) {
    NSLog(@"swizzled applicationDidEnterBackground");
    didEnteredBackGround = YES;
    if(__original_applicationDidEnterBackground_Imp){
        ((void(*)(id,SEL, UIApplication*))__original_applicationDidEnterBackground_Imp)(self, _cmd, application);
    }
}


BOOL __swizzled_didReceiveRemoteNotification(id self, SEL _cmd, UIApplication* application, NSDictionary* userInfo,void (^UIBackgroundFetchResult)(void) ) {
    NSLog(@"swizzled didReceiveRemoteNotification");
    
    [[AppsFlyerLib shared] handlePushNotification:userInfo];
    
    if(__original_didReceiveRemoteNotification_Imp){
        return ((BOOL(*)(id, SEL, UIApplication*, NSDictionary*, (UIBackgroundFetchResult)))__original_didReceiveRemoteNotification_Imp)(self, _cmd, application, userInfo, nil);
    }
    return YES;
}



BOOL __swizzled_openURL(id self, SEL _cmd, UIApplication* application, NSURL* url, NSDictionary * options) {
    NSLog(@"swizzled openURL");
    [[AppsFlyerAttribution shared] handleOpenUrl:url options:options];
    if(__original_openUrl_Imp){
        return ((BOOL(*)(id, SEL, UIApplication*, NSURL*, NSDictionary*))__original_openUrl_Imp)(self, _cmd, application, url, options);
    }
    return NO;
}


@end



