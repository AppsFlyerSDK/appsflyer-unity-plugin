//
//  AppsFlyer+UnityScene.m
//  Unity-iPhone
//
//  Scene-lifecycle counterpart to AppsFlyer+AppController.m's continueUserActivity swizzle,
//  which stops being invoked by iOS once a scene delegate (UnityScene) is configured - see
//  UIApplicationSceneManifest in the exported Info.plist.
//
//  Unity's own UnityScene.mm funnels BOTH custom-URL-scheme opens (scene:openURLContexts:,
//  and the URLContexts branch of scene:willConnectToSession:options:) AND Universal Links
//  (scene:continueUserActivity:, and the userActivities branch of
//  scene:willConnectToSession:options:) through one private helper,
//  applyURL:sourceApplication:annotation:, which always posts the generic kUnityOnOpenURL
//  notification - already observed in AppsFlyerAppController.mm and forwarded to AppsFlyerLib's
//  handleOpenURL:sourceApplication:withAnnotation:. That's the correct AppsFlyerLib entry point
//  for custom-URL-scheme deep links, but the wrong one for Universal Links, which AppsFlyerLib's
//  own header documents should go through continueUserActivity:restorationHandler: instead (its
//  OneLink-specific handling).
//
//  This file hooks the two scene-delegate methods where the NSUserActivity is still intact,
//  forwards it through the correct AppsFlyerLib entry point directly, and marks the one
//  applyURL: call that follows so it skips re-posting kUnityOnOpenURL for that same event -
//  without touching Unity's handling of genuine custom-URL-scheme opens, which keeps working
//  exactly as before, and without needing AppsFlyerAppController.mm to change at all.
#if __has_include("UnityScene.h")

#import <objc/runtime.h>
#import "UnityScene.h"
#import "AppsFlyerAttribution.h"

// Declared directly, matching the exact signature Unity's own Classes/Unity/UnityInternalInterface.h
// exposes - avoided importing that header itself to not take on a dependency on its path staying
// stable across Unity editor versions for the sake of a single, long-stable C symbol.
extern void UnitySetAbsoluteURL(const char *url);

// applyURL:sourceApplication:annotation: isn't declared in UnityScene.h - it's UnityScene.mm's own
// private helper - so it's declared here only to give the compiler visibility into the selector.
@interface UnityScene (AppsFlyerPrivateSelectors)
- (void)applyURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
@end

@implementation UnityScene (AppsFlyerSwizzledScene)

static IMP __original_scene_continueUserActivity_Imp __unused;
static IMP __original_scene_willConnectToSession_Imp __unused;
static IMP __original_applyURL_Imp __unused;

// Set right before we let Unity's own implementation run for a Universal-Link-derived event, and
// consumed (cleared) the next time applyURL: fires - whether that's synchronously in the same
// call (warm continueUserActivity:) or later, once the scene activates, for a cold launch
// (willConnectToSession: defers its own applyURL: call to sceneWillEnterForeground:). Safe as a
// plain flag: every scene-delegate callback here runs on the main thread, and Unity never defers
// this dispatch across another custom-URL-scheme open in between.
static BOOL __af_suppressNextApplyURLDispatch = NO;

+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        [self swizzleSceneContinueUserActivity:[self class]];
        [self swizzleSceneWillConnectToSession:[self class]];
        [self swizzleApplyURL:[self class]];
    });
}

+(void)swizzleSceneContinueUserActivity:(Class)class {

    SEL originalSelector = @selector(scene:continueUserActivity:);

    Method defaultMethod = class_getInstanceMethod(class, originalSelector);
    Method swizzledMethod = class_getInstanceMethod(class, @selector(__swizzled_scene_continueUserActivity));

    BOOL isMethodExists = !class_addMethod(class, originalSelector, method_getImplementation(swizzledMethod), method_getTypeEncoding(swizzledMethod));

    if (isMethodExists) {
        __original_scene_continueUserActivity_Imp = method_setImplementation(defaultMethod, (IMP)__swizzled_scene_continueUserActivity);
    } else {
        class_replaceMethod(class, originalSelector, (IMP)__swizzled_scene_continueUserActivity, method_getTypeEncoding(swizzledMethod));
    }
}

// Warm Universal Link re-open.
void __swizzled_scene_continueUserActivity(id self, SEL _cmd, UIScene *scene, NSUserActivity *userActivity) {
    if (userActivity != nil) {
        NSLog(@"[AppsFlyer+UnityScene] scene:continueUserActivity: forwarding to AppsFlyerLib continueUserActivity:restorationHandler:");
        __af_suppressNextApplyURLDispatch = YES;
        [[AppsFlyerAttribution shared] continueUserActivity:userActivity restorationHandler:nil];
    }

    if (__original_scene_continueUserActivity_Imp) {
        ((void (*)(id, SEL, UIScene *, NSUserActivity *))__original_scene_continueUserActivity_Imp)(self, _cmd, scene, userActivity);
    }
}

+(void)swizzleSceneWillConnectToSession:(Class)class {

    SEL originalSelector = @selector(scene:willConnectToSession:options:);

    Method defaultMethod = class_getInstanceMethod(class, originalSelector);
    Method swizzledMethod = class_getInstanceMethod(class, @selector(__swizzled_scene_willConnectToSession));

    BOOL isMethodExists = !class_addMethod(class, originalSelector, method_getImplementation(swizzledMethod), method_getTypeEncoding(swizzledMethod));

    if (isMethodExists) {
        __original_scene_willConnectToSession_Imp = method_setImplementation(defaultMethod, (IMP)__swizzled_scene_willConnectToSession);
    } else {
        class_replaceMethod(class, originalSelector, (IMP)__swizzled_scene_willConnectToSession, method_getTypeEncoding(swizzledMethod));
    }
}

// Mirrors UnityScene.mm's own firstBrowsingActivityFromActivities: filter, since that helper
// isn't exposed for us to call directly.
static NSUserActivity *__af_firstBrowsingActivity(NSSet<NSUserActivity *> *activities) {
    for (NSUserActivity *activity in activities) {
        if ([activity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb] && activity.webpageURL != nil && activity.webpageURL.absoluteString != nil) {
            return activity;
        }
    }
    return nil;
}

// Cold launch (custom-URL-scheme or Universal Link) - Unity's own implementation must still run
// here unconditionally, it performs required engine init and defers the eventual applyURL: call
// to sceneWillEnterForeground:.
void __swizzled_scene_willConnectToSession(id self, SEL _cmd, UIScene *scene, UISceneSession *session, UISceneConnectionOptions *connectionOptions) {
    NSUserActivity *userActivity = __af_firstBrowsingActivity(connectionOptions.userActivities);
    if (userActivity != nil) {
        NSLog(@"[AppsFlyer+UnityScene] scene:willConnectToSession:options: forwarding cold-launch Universal Link to AppsFlyerLib continueUserActivity:restorationHandler:");
        __af_suppressNextApplyURLDispatch = YES;
        [[AppsFlyerAttribution shared] continueUserActivity:userActivity restorationHandler:nil];
    }

    if (__original_scene_willConnectToSession_Imp) {
        ((void (*)(id, SEL, UIScene *, UISceneSession *, UISceneConnectionOptions *))__original_scene_willConnectToSession_Imp)(self, _cmd, scene, session, connectionOptions);
    }
}

+(void)swizzleApplyURL:(Class)class {

    SEL originalSelector = @selector(applyURL:sourceApplication:annotation:);

    Method defaultMethod = class_getInstanceMethod(class, originalSelector);
    Method swizzledMethod = class_getInstanceMethod(class, @selector(__swizzled_applyURL));

    BOOL isMethodExists = !class_addMethod(class, originalSelector, method_getImplementation(swizzledMethod), method_getTypeEncoding(swizzledMethod));

    if (isMethodExists) {
        __original_applyURL_Imp = method_setImplementation(defaultMethod, (IMP)__swizzled_applyURL);
    } else {
        class_replaceMethod(class, originalSelector, (IMP)__swizzled_applyURL, method_getTypeEncoding(swizzledMethod));
    }
}

// The single choke point both custom-URL-scheme opens and Universal Links funnel into. Only
// Universal-Link-derived calls (flagged above) are diverted here; everything else - genuine
// deep links - runs through Unity's original implementation exactly as before.
void __swizzled_applyURL(id self, SEL _cmd, NSURL *url, NSString *sourceApplication, id annotation) {
    if (__af_suppressNextApplyURLDispatch) {
        __af_suppressNextApplyURLDispatch = NO;
        NSLog(@"[AppsFlyer+UnityScene] applyURL:sourceApplication:annotation: suppressed duplicate kUnityOnOpenURL dispatch for Universal Link already forwarded via continueUserActivity:");
        if (url != nil && url.absoluteString != nil) {
            UnitySetAbsoluteURL(url.absoluteString.UTF8String);
        }
        return;
    }

    if (__original_applyURL_Imp) {
        ((void (*)(id, SEL, NSURL *, NSString *, id))__original_applyURL_Imp)(self, _cmd, url, sourceApplication, annotation);
    }
}

@end

#endif
