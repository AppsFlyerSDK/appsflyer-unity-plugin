// Requests App Tracking Transparency authorization on demand, called from C# after
// AppsFlyer.init() has already run.
//
// Deliberately NOT hooked to app lifecycle (applicationDidBecomeActive:/didFinishLaunching...):
// AppsFlyer+AppController.m already swizzles applicationDidBecomeActive: for its own SDK
// start/session-ready flow. Triggering the ATT system prompt from that same hook fires it before
// Unity's C# Start() coroutine (async .env read, then AppsFlyer.init()) has run — the prompt
// forces a resign/become-active cycle, and on that second becomeActive call AppsFlyer's own
// swizzle exercises session logic before devKey/appID are set natively, crashing with
// "devKey and appleAppID must be set before calling registerSessionReadyListener:".

#import <AppTrackingTransparency/AppTrackingTransparency.h>

extern "C" void UnitySendMessage(const char* obj, const char* method, const char* msg);

extern "C" {

void _afqaRequestTrackingAuthorization() {
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        if (@available(iOS 14, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                NSLog(@"[AF_QA][ATT] requestTrackingAuthorization status=%ld", (long)status);
                // Notify C# so start() can wait for the user's actual decision instead of
                // firing before the prompt is even answered.
                NSString *statusStr = [NSString stringWithFormat:@"%ld", (long)status];
                UnitySendMessage("QATestObject", "OnATTAuthorizationDetermined", [statusStr UTF8String]);
            }];
        } else {
            UnitySendMessage("QATestObject", "OnATTAuthorizationDetermined", "-1");
        }
    });
}

}
