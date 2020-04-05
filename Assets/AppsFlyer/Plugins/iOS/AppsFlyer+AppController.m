//
//  AppsFlyer+AppController.m
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import <objc/runtime.h>
#import "UnityAppController.h"
#if __has_include(<AppsFlyerLib/AppsFlyerTracker.h>)
#import <AppsFlyerLib/AppsFlyerTracker.h>
#else
#import "AppsFlyerTracker.h"
#endif

// Based on : https://blog.newrelic.com/engineering/right-way-to-swizzle/ (MUST READ!!)
// https://medium.com/rocknnull/ios-to-swizzle-or-not-to-swizzle-f8b0ed4a1ce6

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
        
        /** <remove comment if you are swizzling applicationDidBecomeActive>
         Method method1 = class_getInstanceMethod([self class], @selector(applicationDidBecomeActive:));
         __original_applicationDidBecomeActive_Imp = method_setImplementation(method1, (IMP)__swizzled_applicationDidBecomeActive);
         */
        
        /** <remove comment if you are swizzling applicationDidEnterBackground>
         Method method2 = class_getInstanceMethod([self class], @selector(applicationDidEnterBackground:));
         __original_applicationDidEnterBackground_Imp = method_setImplementation(method2, (IMP)__swizzled_applicationDidEnterBackground);
         */
        
        /** <remove comment if you are swizzling didReceiveRemoteNotification>
         Method method3 = class_getInstanceMethod([self class], @selector(didReceiveRemoteNotification:));
         __original_didReceiveRemoteNotification_Imp = method_setImplementation(method3, (IMP)__swizzled_didReceiveRemoteNotification);
         */

        /** <remove comment if you are swizzling openURL>
         Method method4 = class_getInstanceMethod([self class], @selector(application:openURL:options:));
         __original_openUrl_Imp = method_setImplementation(method4, (IMP)__swizzled_openURL);
         */

        /** <remove comment if you are swizzling continueUserActivity>
        [self swizzleContinueUserActivity:[self class]];
        */
        
    });
}

/** <remove comment if you are swizzling continueUserActivity>
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
 [[AppsFlyerTracker sharedTracker] continueUserActivity:userActivity restorationHandler:restorationHandler];
 
 if(__original_continueUserActivity_Imp){
 return ((BOOL(*)(id, SEL, UIApplication*, NSUserActivity*))__original_continueUserActivity_Imp)(self, _cmd, application, userActivity);
 }
 
 return YES;
 }
 
*/

/** <remove comment if you are swizzling applicationDidBecomeActive>
void __swizzled_applicationDidBecomeActive(id self, SEL _cmd, UIApplication* launchOptions) {
    NSLog(@"swizzled applicationDidBecomeActive");
    
    if(didEnteredBackGround){
        [[AppsFlyerTracker sharedTracker] trackAppLaunch];
    }
    
    if(__original_applicationDidBecomeActive_Imp){
        ((void(*)(id,SEL, UIApplication*))__original_applicationDidBecomeActive_Imp)(self, _cmd, launchOptions);
    }
}
 */

/** <remove comment if you are swizzling applicationDidEnterBackground>
void __swizzled_applicationDidEnterBackground(id self, SEL _cmd, UIApplication* application) {
    NSLog(@"swizzled applicationDidEnterBackground");
    didEnteredBackGround = YES;
    if(__original_applicationDidEnterBackground_Imp){
        ((void(*)(id,SEL, UIApplication*))__original_applicationDidEnterBackground_Imp)(self, _cmd, application);
    }
}
 */

/** <remove comment if you are swizzling didReceiveRemoteNotification>
BOOL __swizzled_didReceiveRemoteNotification(id self, SEL _cmd, UIApplication* application, NSDictionary* userInfo,void (^UIBackgroundFetchResult)(void) ) {
    NSLog(@"swizzled didReceiveRemoteNotification");

   [[AppsFlyerTracker sharedTracker] handlePushNotification:userInfo];

    if(__original_didReceiveRemoteNotification_Imp){
        return ((BOOL(*)(id, SEL, UIApplication*, NSDictionary*, (UIBackgroundFetchResult)))__original_didReceiveRemoteNotification_Imp)(self, _cmd, application, userInfo, nil);
    }
    return YES;
}
 */

/** <remove comment if you are swizzling openURL>
BOOL __swizzled_openURL(id self, SEL _cmd, UIApplication* application, NSURL* url, NSDictionary * options) {
    NSLog(@"swizzled openURL");
    [[AppsFlyerTracker sharedTracker] handleOpenUrl:url options:options];
    if(__original_openUrl_Imp){
        return ((BOOL(*)(id, SEL, UIApplication*, NSURL*, NSDictionary*))__original_openUrl_Imp)(self, _cmd, application, url, options);
    }
    return YES;
}
*/

@end




