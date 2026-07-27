//
//  AppsFlyerRPCWrapper.mm
//  Bridges C# DllImport _afExecuteJson → AppsFlyerRPCBridge (Swift).
//  Uses a blocking semaphore so the P/Invoke boundary stays synchronous.
//  Gracefully stubs when AppsFlyerRPC framework is not linked.
//

#include <string.h>

#if __has_include(<AppsFlyerRPC/AppsFlyerRPC-Swift.h>)
#import <AppsFlyerRPC/AppsFlyerRPC-Swift.h>
#elif __has_include("AppsFlyerRPC-Swift.h")
#import "AppsFlyerRPC-Swift.h"
#endif

#include <dispatch/dispatch.h>

extern "C" {

// Wires the RPC → Unity event channel. Must be called during SDK init, before
// registerSessionReadyListener, so the sessionReady callback can reach Unity.
void _setRPCEventHandler(const char* objectName) {
#if __has_include(<AppsFlyerRPC/AppsFlyerRPC-Swift.h>) || __has_include("AppsFlyerRPC-Swift.h")
    NSString *callbackObject = [[NSString alloc] initWithUTF8String:objectName ?: ""];
    [AppsFlyerRPCBridge.shared setEventHandler:^(NSString *jsonEvent) {
        if (callbackObject.length > 0)
            UnitySendMessage([callbackObject UTF8String], "onRPCEvent", [jsonEvent UTF8String] ?: "{}");
    }];
#endif
}

// Fire-and-forget: used for setter methods that need no return value.
// AppsFlyerRPCBridge is @MainActor-isolated; dispatch to main queue so the
// Swift actor isolation is respected and async callbacks (e.g. sessionReady) fire.
void _afFireJson(const char* jsonRequest) {
#if __has_include(<AppsFlyerRPC/AppsFlyerRPC-Swift.h>) || __has_include("AppsFlyerRPC-Swift.h")
    NSString *requestStr = [[NSString alloc] initWithUTF8String:jsonRequest ?: "{}"];
    dispatch_async(dispatch_get_main_queue(), ^{
        [AppsFlyerRPCBridge.shared executeJson:requestStr completion:^(NSString *__unused r) {}];
    });
#endif
}

// Synchronous: used for getter methods (e.g. getAppsFlyerUID) that return data.
// Blocks a background thread — NEVER call from the main Unity thread.
const char* _afExecuteJson(const char* jsonRequest) {
#if __has_include(<AppsFlyerRPC/AppsFlyerRPC-Swift.h>) || __has_include("AppsFlyerRPC-Swift.h")
    __block NSString *response = nil;
    dispatch_semaphore_t sem = dispatch_semaphore_create(0);
    NSString *requestStr = [[NSString alloc] initWithUTF8String:jsonRequest ?: "{}"];

    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        [AppsFlyerRPCBridge.shared executeJson:requestStr completion:^(NSString *jsonResponse) {
            response = jsonResponse;
            dispatch_semaphore_signal(sem);
        }];
    });

    dispatch_time_t timeout = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(5 * NSEC_PER_SEC));
    dispatch_semaphore_wait(sem, timeout);

    if (!response) {
        return strdup("{\"id\":\"rpc-error\",\"error\":{\"code\":-1,\"message\":\"No response from AppsFlyerRPC\"}}");
    }
    return strdup([response UTF8String] ?: "{}");
#else
    return strdup("{\"id\":\"rpc-stub\",\"error\":{\"code\":-2,\"message\":\"AppsFlyerRPC framework not linked\"}}");
#endif
}

} // extern "C"
