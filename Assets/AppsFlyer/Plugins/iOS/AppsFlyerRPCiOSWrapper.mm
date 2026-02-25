#define AF_RPC_AVAILABLE 0

#if __has_include(<AppsFlyerRPC/AppsFlyerRPC-Swift.h>)
  #import <AppsFlyerRPC/AppsFlyerRPC-Swift.h>
  #undef AF_RPC_AVAILABLE
  #define AF_RPC_AVAILABLE 1
#elif __has_include(<AppsFlyerRPC/AppsFlyerRPC.h>)
  #import <AppsFlyerRPC/AppsFlyerRPC.h>
  #undef AF_RPC_AVAILABLE
  #define AF_RPC_AVAILABLE 1
#endif

#if AF_RPC_AVAILABLE

static NSString *_rpcCallbackObjectName = nil;

static NSString* serializeData(id data) {
    if (!data || data == [NSNull null])
        return @"";
    if ([data isKindOfClass:[NSDictionary class]] || [data isKindOfClass:[NSArray class]]) {
        NSData *jsonData = [NSJSONSerialization dataWithJSONObject:data options:0 error:nil];
        return jsonData ? [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding] : @"";
    }
    return [data description];
}

static void dispatchRPCEvent(NSString *jsonEvent) {
    if (!_rpcCallbackObjectName)
        return;

    NSData *rawData = [jsonEvent dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *eventDict = [NSJSONSerialization JSONObjectWithData:rawData options:0 error:nil];
    if (!eventDict)
        return;

    NSString *eventType = eventDict[@"event"];
    id data = eventDict[@"data"];

    const char *callback = NULL;
    NSString *payload = @"";

    if ([eventType isEqualToString:@"onConversionDataSuccess"]) {
        callback = "onConversionDataSuccess";
        payload = serializeData(data);
    } else if ([eventType isEqualToString:@"onConversionDataFail"]) {
        callback = "onConversionDataFail";
        payload = serializeData(data);
    } else if ([eventType isEqualToString:@"onAppOpenAttribution"]) {
        callback = "onAppOpenAttribution";
        payload = serializeData(data);
    } else if ([eventType isEqualToString:@"onAppOpenAttributionFailure"]) {
        callback = "onAppOpenAttributionFailure";
        payload = serializeData(data);
    } else if ([eventType isEqualToString:@"onDeepLinkReceived"]) {
        callback = "onDeepLinking";
        payload = serializeData(data);
    }

    if (callback) {
        NSLog(@"[AppsFlyer RPC] Event -> %s: %@", callback, payload);
        UnitySendMessage([_rpcCallbackObjectName UTF8String], callback, [payload UTF8String]);
    }
}

#endif

extern "C" {

void _afRPCExecuteJson(const char* jsonRequest)
{
#if AF_RPC_AVAILABLE
    if (jsonRequest == NULL)
        return;

    NSString *request = [NSString stringWithUTF8String:jsonRequest];
    [[AppsFlyerRPCBridge shared] executeJson:request completion:^(NSString *response) {
        NSLog(@"[AppsFlyer RPC] Response: %@", response);
    }];
#else
    NSLog(@"[AppsFlyer RPC] AppsFlyerRPC framework not available. Add the AppsFlyerRPC pod.");
#endif
}

void _afRPCSetEventHandler(const char* objectName)
{
#if AF_RPC_AVAILABLE
    if (objectName != NULL) {
        _rpcCallbackObjectName = [NSString stringWithUTF8String:objectName];
    }
    [[AppsFlyerRPCBridge shared] setEventHandler:^(NSString *jsonEvent) {
        NSLog(@"[AppsFlyer RPC] Event received: %@", jsonEvent);
        dispatchRPCEvent(jsonEvent);
    }];
#endif
}

}
