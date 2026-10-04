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
static const char* stringFromdictionary(NSDictionary* dictionary);

