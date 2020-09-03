//
//  AFUnityUtils.mm
//  Unity-iPhone
//
//  Created by Jonathan Wesfield on 24/07/2019.
//

#if __has_include(<AppsFlyerLib/AppsFlyerLib.h>)
#import <AppsFlyerLib/AppsFlyerLib.h>
#else
#import "AppsFlyerLib.h"
#endif


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

