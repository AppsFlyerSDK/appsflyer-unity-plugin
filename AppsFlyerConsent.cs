using System;
using System.Collections.Generic;

namespace AppsFlyerSDK
{
    /// <summary>
    // Data class representing a user's consent for data processing in accordance with GDPR and DMA
    // (Digital Markets Act) compliance, specifically regarding advertising preferences.
  
    // This class should be used to notify and record the user's applicability
    // under GDPR, their general consent to data usage, and their consent to personalized
    // advertisements based on user data.
 
    // Note that the consent for data usage and ads personalization pair is only applicable when the user is
    // subject to GDPR guidelines. Therefore, the following factory methods should be used accordingly:
    // - Use [forGDPRUser] when the user is subject to GDPR.
    // - Use [forNonGDPRUser] when the user is not subject to GDPR.
    
    // @property isUserSubjectToGDPR Indicates whether GDPR regulations apply to the user (true if the user is
    //  a  subject of GDPR). It also serves as a flag for compliance with relevant aspects of DMA regulations.
    // @property hasConsentForDataUsage Indicates whether the user has consented to the use of their data for advertising
    //  purposes under both GDPR and DMA guidelines (true if the user has consented, nullable if not subject to GDPR).
    // @property hasConsentForAdsPersonalization Indicates whether the user has consented to the use of their data for
    //  personalized advertising within the boundaries of GDPR and DMA rules (true if the user has consented to
    //  personalized ads, nullable if not subject to GDPR).
    /// </summary>
    public class AppsFlyerConsent
    {
        public bool isUserSubjectToGDPR { get; private set; }
        public bool hasConsentForDataUsage { get; private set; }
        public bool hasConsentForAdsPersonalization { get; private set; }

        private AppsFlyerConsent(bool isGDPR, bool hasForDataUsage, bool hasForAdsPersonalization)
        {
            isUserSubjectToGDPR = isGDPR;
            hasConsentForDataUsage = hasForDataUsage;
            hasConsentForAdsPersonalization = hasForAdsPersonalization;
        }

        public static AppsFlyerConsent ForGDPRUser(bool hasConsentForDataUsage, bool hasConsentForAdsPersonalization)
        {
            return new AppsFlyerConsent(true, hasConsentForDataUsage, hasConsentForAdsPersonalization);
        }

        public static AppsFlyerConsent ForNonGDPRUser()
        {
            return new AppsFlyerConsent(false, false, false);
        }
    }

}