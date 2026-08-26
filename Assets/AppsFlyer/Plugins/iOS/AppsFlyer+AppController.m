//
//  AppsFlyer+AppController.m
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

// Classic (AppDelegate-style) UnityAppController swallows continueUserActivity without posting any
// NotificationCenter notification (see UnityAppController.mm) - unlike openURL and Scene-lifecycle
// deep links, which route through kUnityOnOpenURL and are observed directly in
// AppsFlyerAppController.mm. This is the one remaining case that still needs UnityAppController
// itself, so it's guarded out entirely where that header doesn't exist - e.g. Unity's Swift Xcode
// project type, which doesn't support UnityAppController subclassing or category swizzling at all.
#if __has_include("UnityAppController.h")

#import <objc/runtime.h>
#import "UnityAppController.h"
#import "AppsFlyerAttribution.h"

@implementation UnityAppController (AppsFlyerSwizzledAppController)

static IMP __original_continueUserActivity_Imp __unused;

+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        [self swizzleContinueUserActivity:[self class]];
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

@end

#endif
