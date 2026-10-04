//
//  AppsFlyerAppController.mm
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 30/07/2019.
//

#import <Foundation/Foundation.h>
#import "AppsFlyerAttribution.h"
#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif

// Unity posts kUnityOnOpenURL/kUnityDidReceiveRemoteNotification via NSNotificationCenter for both
// classic AppDelegate methods and Scene-lifecycle equivalents (scene:openURLContexts:,
// scene:continueUserActivity: for Universal Links) - see UnityAppController.mm/UnityScene.mm.
// Observing them directly needs no UnityAppController subclassing/swizzling, so this works under
// Unity's Swift Xcode project type too, where subclassing UnityAppController isn't supported.
// Guarded on AppDelegateListener.h since that's where kUnityOnOpenURL/kUnityDidReceiveRemoteNotification
// are declared; if a Unity export doesn't ship it, this observer simply doesn't compile in - it does
// not fall back to guessing an equivalent API there.
#if __has_include("AppDelegateListener.h")
#import "AppDelegateListener.h"

// Classic (AppDelegate-style) UnityAppController swallows continueUserActivity without posting any
// notification (see UnityAppController.mm) - that one case is handled by the narrow, separately
// guarded swizzle in AppsFlyer+AppController.m instead.
@interface AppsFlyerDeepLinkObserver : NSObject
@end

@implementation AppsFlyerDeepLinkObserver

+ (void)load {
    static AppsFlyerDeepLinkObserver *observer;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        observer = [[AppsFlyerDeepLinkObserver alloc] init];
        [[NSNotificationCenter defaultCenter] addObserver:observer
                                                  selector:@selector(onOpenURL:)
                                                      name:kUnityOnOpenURL
                                                    object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:observer
                                                  selector:@selector(onDidReceiveRemoteNotification:)
                                                      name:kUnityDidReceiveRemoteNotification
                                                    object:nil];
    });
}

- (void)onOpenURL:(NSNotification *)notification {
    NSURL *url = notification.userInfo[@"url"];
    if (url == nil) return;

    NSString *sourceApplication = notification.userInfo[@"sourceApplication"] ?: @"";
    [[AppsFlyerAttribution shared] handleOpenUrl:url sourceApplication:sourceApplication annotation:notification.userInfo[@"annotation"]];
}

- (void)onDidReceiveRemoteNotification:(NSNotification *)notification {
    [[AppsFlyerLib shared] handlePushNotification:notification.userInfo];
}

@end

#endif
