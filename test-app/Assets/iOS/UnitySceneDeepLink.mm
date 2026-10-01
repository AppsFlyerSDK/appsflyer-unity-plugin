// UnitySceneDeepLink.mm
//
// Scene-lifecycle counterpart to UnityAppControllerDeepLink.mm - this test-app's Unity export is
// scene-based (UIApplicationSceneManifest / UnitySceneDelegateClassName: UnityScene), so
// UnityAppController's application:didFinishLaunchingWithOptions:/application:openURL:options:
// are no longer invoked by iOS for scene-managed launches (see AppsFlyer+UnityScene.m in the
// plugin itself for the same finding applied to the production bridge). The equivalent QA
// injection point here is UnityScene.
//
// Same -deepLinkURL launch-argument / NSUserDefaults fallback and same 5-second startup delay as
// UnityAppControllerDeepLink.mm, for the same reason: AF_BRIDGE_SET fires synchronously inside
// _startSDK before startWithCompletionHandler:, so injecting immediately would flush before the
// SDK is started and UDL would never resolve.
//
// Universal Links (http/https) are injected by calling scene:continueUserActivity: directly with
// a constructed NSUserActivity, exercising AppsFlyer+UnityScene.m's fix end-to-end. Custom-URL-
// scheme opens are injected by posting kUnityOnOpenURL directly - what UnityScene.mm's own
// scene:openURLContexts: would have posted - since UISceneConnectionOptions/UIOpenURLContext have
// no public initializer to fabricate a real scene:openURLContexts: call with.

#import <objc/runtime.h>
#if __has_include("UnityScene.h")
#import "UnityScene.h"

#if __has_include("AppDelegateListener.h")
#import "AppDelegateListener.h"
#else
#import "PluginBase/AppDelegateListener.h"
#endif

@implementation UnityScene (QASceneDeepLinkBootstrap)

+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        SEL original = @selector(scene:willConnectToSession:options:);
        SEL swizzled = @selector(qa_scene:willConnectToSession:options:);
        Method originalMethod = class_getInstanceMethod([self class], original);
        Method swizzledMethod = class_getInstanceMethod([self class], swizzled);
        if (originalMethod && swizzledMethod) {
            method_exchangeImplementations(originalMethod, swizzledMethod);
        }
    });
}

- (void)qa_scene:(UIScene *)scene willConnectToSession:(UISceneSession *)session options:(UISceneConnectionOptions *)connectionOptions {
    [self qa_scene:scene willConnectToSession:session options:connectionOptions];

    NSURL *deepLinkURL = nil;
    NSArray<NSString *> *args = [NSProcessInfo processInfo].arguments;
    NSUInteger idx = [args indexOfObject:@"-deepLinkURL"];
    if (idx != NSNotFound && idx + 1 < args.count) {
        deepLinkURL = [NSURL URLWithString:args[idx + 1]];
    }
    if (!deepLinkURL) {
        NSString *stored = [[NSUserDefaults standardUserDefaults] stringForKey:@"deepLinkURL"];
        if (stored) deepLinkURL = [NSURL URLWithString:stored];
    }
    if (!deepLinkURL) return;

    __weak typeof(self) weakSelf = self;
    __weak UIScene *weakScene = scene;
    dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(5.0 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
        UnityScene *strongSelf = weakSelf;
        UIScene *strongScene = weakScene;
        if (!strongSelf || !strongScene) return;

        BOOL isUniversalLink = [deepLinkURL.scheme isEqualToString:@"http"] || [deepLinkURL.scheme isEqualToString:@"https"];
        if (isUniversalLink) {
            NSUserActivity *activity = [[NSUserActivity alloc] initWithActivityType:NSUserActivityTypeBrowsingWeb];
            activity.webpageURL = deepLinkURL;
            [strongSelf scene:strongScene continueUserActivity:activity];
        } else {
            NSMutableDictionary<NSString *, id> *notifData = [NSMutableDictionary dictionaryWithCapacity:1];
            notifData[@"url"] = deepLinkURL;
            [[NSNotificationCenter defaultCenter] postNotificationName:kUnityOnOpenURL object:nil userInfo:notifData];
        }
    });
}

@end

#endif
