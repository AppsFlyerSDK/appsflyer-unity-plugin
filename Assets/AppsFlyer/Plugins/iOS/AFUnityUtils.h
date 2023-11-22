//
//  AFUnityUtils.h
//
//  Created by Andrii H. and Dmitry O. on 16 Oct 2023
//

#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif

static NSString* stringFromChar(const char *str);
static NSDictionary* dictionaryFromJson(const char *jsonString);
static const char* stringFromdictionary(NSDictionary* dictionary);
static NSArray<NSString*> *NSArrayFromCArray(int length, const char **arr);
static char* getCString(const char* string);
static AppsFlyerLinkGenerator* generatorFromDictionary(NSDictionary* dictionary, AppsFlyerLinkGenerator*  generator);
static EmailCryptType emailCryptTypeFromInt(int emailCryptTypeInt);
static NSString* stringFromDeepLinkResultStatus(AFSDKDeepLinkResultStatus deepLinkResult);
static NSString* stringFromDeepLinkResultError(AppsFlyerDeepLinkResult *result);

