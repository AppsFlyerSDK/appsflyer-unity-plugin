namespace AppsFlyerSDK
{
    public interface IAppsFlyerUserInvite
    {
        /// <summary>
        /// The success callback for generating OneLink URLs. 
        /// </summary>
        /// <param name="link">A string of the newly created url.</param>
        void onInviteLinkGenerated(string link);

        /// <summary>
        /// The error callback for generating OneLink URLs
        /// </summary>
        /// <param name="error">A string describing the error.</param>
        void onInviteLinkGeneratedFailure(string error);

        /// <summary>
        /// (ios only) iOS allows you to utilize the StoreKit component to open
        /// the App Store while remaining in the context of your app.
        /// More details at <see>https://support.appsflyer.com/hc/en-us/articles/115004481946-Cross-Promotion-Tracking#tracking-cross-promotion-impressions</see>
        /// </summary>
        /// <param name="link">openStore callback Contains promoted `clickURL`</param>
        void onOpenStoreLinkGenerated(string link);

    }
}