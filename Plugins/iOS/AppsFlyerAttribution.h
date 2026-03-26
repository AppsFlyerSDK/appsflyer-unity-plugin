//
//  AppsFlyerAttribution.h
//  UnityFramework
//
//  Created by Margot Guetta on 11/04/2021.
//

#ifndef AppsFlyerAttribution_h
#define AppsFlyerAttribution_h
#endif /* AppsFlyerAttribution_h */
#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif


@interface AppsFlyerAttribution : NSObject
@property NSUserActivity*_Nullable  userActivity;
@property (nonatomic, copy) void (^ _Nullable restorationHandler)(NSArray *_Nullable );
@property NSURL * _Nullable url;
@property NSDictionary * _Nullable options;
@property NSString* _Nullable sourceApplication;
@property id _Nullable annotation;
@property BOOL isBridgeReady;

+ (AppsFlyerAttribution *_Nullable)shared;
- (void) continueUserActivity: (NSUserActivity*_Nullable) userActivity restorationHandler: (void (^_Nullable)(NSArray * _Nullable))restorationHandler;
- (void) handleOpenUrl:(NSURL*_Nullable)url options:(NSDictionary*_Nullable) options;
- (void) handleOpenUrl: (NSURL *_Nonnull)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation;

@end

static NSString * _Nullable const AF_BRIDGE_SET = @"bridge is set";
