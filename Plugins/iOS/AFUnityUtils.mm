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

static const char* stringFromdictionary(NSDictionary* dictionary) {
    if(dictionary){
        NSError * err;
        NSData * jsonData = [NSJSONSerialization  dataWithJSONObject:dictionary options:0 error:&err];
        NSString * myString = [[NSString alloc] initWithData:jsonData   encoding:NSUTF8StringEncoding];
        return [myString UTF8String];
    }

    return nil;
}

