namespace AppsFlyerSDK
{
    public interface IAppsFlyerConversionData
    {
        /// <summary>
        ///  `conversionData` contains information about install. Organic/non-organic, etc.
        /// <see>https://support.appsflyer.com/hc/en-us/articles/360000726098-Conversion-Data-Scenarios#Introduction</see>
        /// </summary>
        /// <param name="conversionData">JSON string of the returned conversion data.</param>
        void onConversionDataSuccess(string conversionData);

        /// <summary>
        /// Any errors that occurred during the conversion request.
        /// </summary>
        /// <param name="error">A string describing the error.</param>
        void onConversionDataFail(string error);

        /// <summary>
        /// `attributionData` contains information about OneLink, deeplink.
        /// <see>https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#Intro</see>
        /// </summary>
        /// <param name="attributionData">JSON string of the returned deeplink data.</param>
        void onAppOpenAttribution(string attributionData);

        /// <summary>
        /// Any errors that occurred during the attribution request.
        /// </summary>
        /// <param name="error">A string describing the error.</param>
        void onAppOpenAttributionFailure(string error);
    }
}