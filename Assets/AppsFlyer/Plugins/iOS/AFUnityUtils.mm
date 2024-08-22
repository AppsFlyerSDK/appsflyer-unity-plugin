//
//  AFUnityUtils.mm
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#import "AFUnityUtils.h"

static NSString* stringFromChar(const char *str) {
    return str ? [NSString stringWithUTF8String:str] : nil;
}

static NSDictionary* dictionaryFromJson(const char *jsonString) {
    if(jsonString){
        NSData *jsonData = [[NSData alloc] initWithBytes:jsonString length:strlen(jsonString)];
        NSDictionary *dictionary = [NSJSONSerialization JSONObjectWithData:jsonData options:kNilOptions error:nil];
        return dictionary;
    }
    
    return nil;
}

static const char* stringFromdictionary(NSDictionary* dictionary) {
    if(dictionary){
        NSError * err;
        NSData * jsonData = [NSJSONSerialization  dataWithJSONObject:dictionary options:0 error:&err];
        NSString * myString = [[NSString alloc] initWithData:jsonData   encoding:NSUTF8StringEncoding];
        return [myString UTF8String];
    }

    return nil;
}

static NSDictionary* dictionaryFromNSError(NSError* error) {
    if(error){
            NSInteger code = [error code];
            NSString *localizedDescription = [error localizedDescription];
            
            NSDictionary *errorDictionary = @{
                @"code" : @(code) ?: @(-1),
                @"localizedDescription" : localizedDescription,
            };
        return  errorDictionary;
    }

    return nil;
}


static NSArray<NSString*> *NSArrayFromCArray(int length, const char **arr) {
    NSMutableArray<NSString *> *res = [[NSMutableArray alloc] init];
    for(int i = 0; i < length; i++) {
        if (arr[i]) {
            [res addObject:[NSString stringWithUTF8String:arr[i]]];
        }
    }
    
    return res;
}

static char* getCString(const char* string){
    if (string == NULL){
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

static AppsFlyerLinkGenerator* generatorFromDictionary(NSDictionary* dictionary, AppsFlyerLinkGenerator*  generator) {
    
    NSArray* generatorKeys = @[@"channel", @"customerID", @"campaign", @"referrerName", @"referrerImageUrl", @"deeplinkPath", @"baseDeeplink", @"brandDomain"];
    
    NSMutableDictionary* mutableDictionary = [dictionary mutableCopy];
    
    [generator setChannel:[dictionary objectForKey: @"channel"]];
    [generator setReferrerCustomerId:[dictionary objectForKey: @"customerID"]];
    [generator setCampaign:[dictionary objectForKey: @"campaign"]];
    [generator setReferrerName:[dictionary objectForKey: @"referrerName"]];
    [generator setReferrerImageURL:[dictionary objectForKey: @"referrerImageUrl"]];
    [generator setDeeplinkPath:[dictionary objectForKey: @"deeplinkPath"]];
    [generator setBaseDeeplink:[dictionary objectForKey: @"baseDeeplink"]];
    [generator setBrandDomain:[dictionary objectForKey: @"brandDomain"]];


    [mutableDictionary removeObjectsForKeys:generatorKeys];
    
    [generator addParameters:mutableDictionary];
    
    return generator;
}

static EmailCryptType emailCryptTypeFromInt(int emailCryptTypeInt){
    
    EmailCryptType emailCryptType;
    switch (emailCryptTypeInt){
        case 1:
            emailCryptType = EmailCryptTypeSHA256;
            break;
        default:
            emailCryptType = EmailCryptTypeNone;
            break;
    }

    return emailCryptType;
}

static AppsFlyerAdRevenueMediationNetworkType mediationNetworkTypeFromInt(int mediationNetworkInt){
    
    AppsFlyerAdRevenueMediationNetworkType mediationNetworkType;
    switch (mediationNetworkInt){
        case 1:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob;
            break;
        case 2:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeIronSource;
            break;
        case 3:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeApplovinMax;
            break;
        case 4:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeFyber;
            break;
        case 5:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeAppodeal;
            break;
        case 6:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeAdmost;
            break;
        case 7:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeTopon;
            break;
        case 8:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeTradplus;
            break;
        case 9:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeYandex;
            break;
        case 10:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeChartBoost;
            break;
        case 11:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeUnity;
            break;
        case 12:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeToponPte;
            break;
        case 13:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeCustom;
            break;
        case 14:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeDirectMonetization;
            break;
        default:
            mediationNetworkType = AppsFlyerAdRevenueMediationNetworkTypeCustom;
            break;
    }

    return mediationNetworkType;
}

static NSString* stringFromDeepLinkResultStatus(AFSDKDeepLinkResultStatus deepLinkResult){
    NSString* result;
    switch (deepLinkResult){
        case AFSDKDeepLinkResultStatusFound:
            result = @"FOUND";
            break;
        case AFSDKDeepLinkResultStatusFailure:
            result = @"ERROR";
            break;
        case AFSDKDeepLinkResultStatusNotFound:
            result = @"NOT_FOUND";
            break;
        default:
            result = @"ERROR";
            break;
    }
    
    return result;
}

static NSString* stringFromDeepLinkResultError(AppsFlyerDeepLinkResult *result){
    NSString* res;
    
    if (result && result.error){
        if ([[result.error userInfo][NSUnderlyingErrorKey] code] == -1001) {
            res = @"TIMEOUT";
       } else if ([[result.error userInfo][NSUnderlyingErrorKey] code] == -1009) {
           res = @"NETWORK";
       }
    }
    
    res = @"UNKNOWN";
    
    return res;
}
