// QADeepLinkBootstrap.mm
//
// Swizzles UnityAppController.application:didFinishLaunchingWithOptions: to
// read the -deepLinkURL launch argument injected by `xcrun simctl launch`.
//
// Timing: AF_BRIDGE_SET fires synchronously inside _startSDK BEFORE
// startWithCompletionHandler:, so calling handleOpenUrl: immediately would
// flush before the SDK is started and UDL would never resolve.
//
// Fix: capture the URL at launch, then dispatch a 5-second delayed call to
// application:openURL:options: — the same standard iOS URL-open pipeline that
// Flutter's AppDelegate uses. By 5s Unity has initialised, Start() has called
// initSDK() and startSDK(), so UDL resolves correctly.
//
// Using a category + swizzle rather than IMPL_APP_CONTROLLER_SUBCLASS avoids
// the conflict with the plugin's own AppsFlyerAppController subclass.

#import <objc/runtime.h>
#import "UnityAppController.h"

@implementation UnityAppController (QADeepLinkBootstrap)

+ (void)load {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        SEL original = @selector(application:didFinishLaunchingWithOptions:);
        SEL swizzled = @selector(qa_application:didFinishLaunchingWithOptions:);
        Method originalMethod = class_getInstanceMethod([self class], original);
        Method swizzledMethod = class_getInstanceMethod([self class], swizzled);
        if (originalMethod && swizzledMethod) {
            method_exchangeImplementations(originalMethod, swizzledMethod);
        }
    });
}

- (BOOL)qa_application:(UIApplication *)application
didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    BOOL result = [self qa_application:application didFinishLaunchingWithOptions:launchOptions];

    // Resolve URL from -deepLinkURL launch arg or NSUserDefaults fallback.
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

    if (deepLinkURL) {
        __weak typeof(self) weakSelf = self;
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(5.0 * NSEC_PER_SEC)),
                       dispatch_get_main_queue(), ^{
            __strong typeof(weakSelf) strongSelf = weakSelf;
            if (!strongSelf) return;
            // Route through the standard iOS URL-open delegate pipeline,
            // matching Flutter's AppDelegate approach.
            [strongSelf application:application openURL:deepLinkURL options:@{}];
        });
    }

    return result;
}

@end
