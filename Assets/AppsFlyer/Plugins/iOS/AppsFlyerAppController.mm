//
//  AppsFlyerAppController.mm
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 30/07/2019.
//

#import <Foundation/Foundation.h>
#import "UnityAppController.h"
#import "AppDelegateListener.h"
#if __has_include(<AppsFlyerLib/AppsFlyerTracker.h>)
#import <AppsFlyerLib/AppsFlyerTracker.h>
#else
#import "AppsFlyerTracker.h"
#endif

/**
 Note if you would like to use method swizzeling see AppsFlyer+AppController.m
 If you are using swizzeling then comment out the method that is being swizzeled in AppsFlyerAppController.mm
 Only use swizzeling if there are conflicts with other plugins that needs to be resolved.
*/

@interface AppsFlyerAppController : UnityAppController <AppDelegateListener>
{
    BOOL didEnteredBackGround;
}
@end

@implementation AppsFlyerAppController

- (instancetype)init
{
    self = [super init];
    if (self) {
        UnityRegisterAppDelegateListener(self);
    }
    return self;
}

- (void)didFinishLaunching:(NSNotification*)notification {
    NSLog(@"got didFinishLaunching = %@",notification.userInfo);
    if (notification.userInfo[@"url"]) {
        [self onOpenURL:notification];
    }
}

-(void)didBecomeActive:(NSNotification*)notification {
    NSLog(@"got didBecomeActive(out) = %@", notification.userInfo);
    if (didEnteredBackGround == YES) {
        [[AppsFlyerTracker sharedTracker] trackAppLaunch];
        didEnteredBackGround = NO;
    }
}

- (void)didEnterBackground:(NSNotification*)notification {
    NSLog(@"got didEnterBackground = %@", notification.userInfo);
    didEnteredBackGround = YES;
}

- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray *))restorationHandler {
    [[AppsFlyerTracker sharedTracker] continueUserActivity:userActivity restorationHandler:restorationHandler];
    return YES;
}

-(BOOL) application:(UIApplication *)application openUrl:(NSURL *)url options:(NSDictionary *)options {
    NSLog(@"got openUrl: %@",url);
    [[AppsFlyerTracker sharedTracker] handleOpenUrl:url options:options];
    return YES;
}

- (void)onOpenURL:(NSNotification*)notification {
    NSLog(@"got onOpenURL = %@", notification.userInfo);
    NSURL *url = notification.userInfo[@"url"];
    NSString *sourceApplication = notification.userInfo[@"sourceApplication"];
    
    if (sourceApplication == nil) {
        sourceApplication = @"";
    }
    
    if (url != nil) {
        [[AppsFlyerTracker sharedTracker] handleOpenURL:url sourceApplication:sourceApplication withAnnotation:nil];
    }
    
}

- (void)didReceiveRemoteNotification:(NSNotification*)notification {
    NSLog(@"got didReceiveRemoteNotification = %@", notification.userInfo);
    [[AppsFlyerTracker sharedTracker] handlePushNotification:notification.userInfo];
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(AppsFlyerAppController)


/**
Note if you would not like to use IMPL_APP_CONTROLLER_SUBCLASS you can replace it with the code below.
 <code>
 +(void)load
 {
 [AppsFlyerAppController plugin];
 }
 
 // Singleton accessor.
 + (AppsFlyerAppController *)plugin
 {
 static AppsFlyerAppController *sharedInstance = nil;
 static dispatch_once_t onceToken;
 
 dispatch_once(&onceToken, ^{
 
 sharedInstance = [[AppsFlyerAppController alloc] init];
 });
 
 return sharedInstance;
 }
</code>
 **/
