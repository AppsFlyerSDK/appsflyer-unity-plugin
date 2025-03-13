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
    
    /// ## Properties:
    /// - `isUserSubjectToGDPR` (optional) - Indicates whether GDPR regulations apply to the user. 
    ///   This may also serve as a general compliance flag for other regional regulations.
    /// - `hasConsentForDataUsage` (optional) - Indicates whether the user consents to the processing 
    ///   of their data for advertising purposes.
    /// - `hasConsentForAdsPersonalization` (optional) - Indicates whether the user consents to the 
    ///   use of their data for personalized advertising.
    /// - `hasConsentForAdStorage` (optional) - Indicates whether the user consents to ad-related storage access.
    ///
    /// **Usage Example:**
    /// ```csharp
    /// var consent = new AppsFlyerConsent(
    ///     isUserSubjectToGDPR: true, 
    ///     hasConsentForDataUsage: true, 
    ///     hasConsentForAdsPersonalization: false, 
    ///     hasConsentForAdStorage: true
    /// );
    /// **Deprecated APIs:**
    /// - `ForGDPRUser(...)` and `ForNonGDPRUser(...)` should no longer be used.
    /// - Use `new AppsFlyerConsent(...)` instead with relevant consent fields.
    ///
    /// </summary>
    public class AppsFlyerConsent
    {
        public bool? isUserSubjectToGDPR { get; private set; }
        public bool? hasConsentForDataUsage { get; private set; }
        public bool? hasConsentForAdsPersonalization { get; private set; }
        public bool? hasConsentForAdStorage { get; private set; }

        public AppsFlyerConsent( bool? isUserSubjectToGDPR = null, bool? hasConsentForDataUsage = null, bool? hasConsentForAdsPersonalization = null, bool? hasConsentForAdStorage = null)
        {
            this.isUserSubjectToGDPR = isUserSubjectToGDPR;
            this.hasConsentForDataUsage = hasConsentForDataUsage;
            this.hasConsentForAdsPersonalization = hasConsentForAdsPersonalization;
            this.hasConsentForAdStorage = hasConsentForAdStorage;
        }

        [Obsolete("Use the new constructor with optional booleans instead.")]
        private AppsFlyerConsent(bool isGDPR, bool hasForDataUsage, bool hasForAdsPersonalization)
        {
            isUserSubjectToGDPR = isGDPR;
            hasConsentForDataUsage = hasForDataUsage;
            hasConsentForAdsPersonalization = hasForAdsPersonalization;
        }

        [Obsolete("Use new AppsFlyerConsent(...) instead.")]
        public static AppsFlyerConsent ForGDPRUser(bool hasConsentForDataUsage, bool hasConsentForAdsPersonalization)
        {
            return new AppsFlyerConsent(true, hasConsentForDataUsage, hasConsentForAdsPersonalization);
        }

        [Obsolete("Use new AppsFlyerConsent(...) instead.")]
        public static AppsFlyerConsent ForNonGDPRUser()
        {
            return new AppsFlyerConsent(false, false, false);
        }
    }
}