using System;
using System.Collections.Generic;

namespace AppsFlyerSDK
{
    
    /// <summary>
    /// Event args for AppsFlyer requests.
    /// Used for sessions and in-app events.
    /// Used to handle post request logic.
    /// 
    /// Examples:
    /// statusCode / errorDescription
    /// 
    /// 200 - null
    /// 
    /// 10 - "Event timeout. Check 'minTimeBetweenSessions' param"
    /// 11 - "Skipping event because 'isStopTracking' enabled"
    /// 40 - Network error: Error description comes from Android
    /// 41 - "No dev key"
    /// 50 - "Status code failure" + actual response code from the server
    /// 
    /// </summary>
    public class AppsFlyerRequestEventArgs : EventArgs
    {
        public AppsFlyerRequestEventArgs(int code, string description)
        {
            statusCode = code;
            errorDescription = description;
        }

        public int statusCode { get; }
        public string errorDescription { get; }
    }

    /// <summary>
    /// Event args for OnDeepLinkReceived.
    /// Used to handle deep linking results.
    /// </summary>
    public class DeepLinkEventsArgs : EventArgs
    {
        
        /// <summary>
        /// DeepLink dictionary to get additional parameters
        /// </summary>
        public Dictionary<string, object> deepLink;
        
        /// <summary>
        /// DeepLink status: FOUND, NOT_FOUND, ERROR
        /// </summary>
        public DeepLinkStatus status { get; }
        
        /// <summary>
        /// DeepLink error: TIMEOUT, NETWORK, HTTP_STATUS_CODE, UNEXPECTED
        /// </summary>
        public DeepLinkError error { get; }

        public string getMatchType()
        {
            return getDeepLinkParameter("match_type");
        }

        public string getDeepLinkValue()
        {
            return getDeepLinkParameter("deep_link_value");
        }

        public string getClickHttpReferrer()
        {
            return getDeepLinkParameter("click_http_referrer");
        }

        public string getMediaSource()
        {
            return getDeepLinkParameter("media_source");
        }

        public string getCampaign()
        {
            return getDeepLinkParameter("campaign");
        }

        public string getCampaignId()
        {
            return getDeepLinkParameter("campaign_id");
        }

        public string getAfSub1()
        {
            return getDeepLinkParameter("af_sub1");
        }

        public string getAfSub2()
        {
            return getDeepLinkParameter("af_sub2");
        }

        public string getAfSub3()
        { 
            return getDeepLinkParameter("af_sub3");
        }

        public string getAfSub4()
        {
            return getDeepLinkParameter("af_sub4");
        }

        public string getAfSub5()
        {
            return getDeepLinkParameter("af_sub5");
        }

        public bool isDeferred()
        {
            if (deepLink != null && deepLink.ContainsKey("is_deferred"))
            {
                try
                {
                    return (bool)deepLink["is_deferred"];
                }
                catch (Exception e)
                {
                    AppsFlyer.AFLog("DeepLinkEventsArgs.isDeferred", String.Format("{0} Exception caught.", e));
                }
            }

            return false;
        }

        public Dictionary<string, object> getDeepLinkDictionary()
        {
            return deepLink;
        }
        
        public DeepLinkEventsArgs(string str)
        {
            try
            {
                Dictionary<string, object> dictionary = AppsFlyer.CallbackStringToDictionary(str);

                string status = "";
                string error = "";
                Dictionary<string, object> deepLink;
                
                if (dictionary.ContainsKey("status") && dictionary["status"] != null)
                {
                    status = dictionary["status"].ToString();
                }
                
                if (dictionary.ContainsKey("error") && dictionary["error"] != null)
                {
                    error = dictionary["error"].ToString();
                }
                
                if (dictionary.ContainsKey("deepLink") && dictionary["deepLink"] != null)
                {
                    this.deepLink = AppsFlyer.CallbackStringToDictionary(dictionary["deepLink"].ToString());
                }
                
                switch (status)
                {
                    case "FOUND":
                        this.status = DeepLinkStatus.FOUND;
                        break;
                    case "NOT_FOUND":
                        this.status = DeepLinkStatus.NOT_FOUND;
                        break;
                    default:
                        this.status = DeepLinkStatus.ERROR;
                        break;
                }
                
                switch (error)
                {
                    case "TIMEOUT":
                        this.error = DeepLinkError.TIMEOUT;
                        break;
                    case "NETWORK":
                        this.error = DeepLinkError.NETWORK;
                        break;
                    case "HTTP_STATUS_CODE":
                        this.error = DeepLinkError.HTTP_STATUS_CODE;
                        break;
                    default:
                        this.error = DeepLinkError.UNEXPECTED;
                        break;
                }
                
            }
            catch (Exception e)
            {
                AppsFlyer.AFLog("DeepLinkEventsArgs.parseDeepLink", String.Format("{0} Exception caught.", e));
            }
        }
        
        private string getDeepLinkParameter(string name)
        {
            if (deepLink != null && deepLink.ContainsKey(name) && deepLink[name] != null)
            {
                return deepLink[name].ToString();
            }
            
            return null;
        }
        
    }

    public enum DeepLinkStatus {
        FOUND, NOT_FOUND, ERROR
    }

    public enum DeepLinkError {
        TIMEOUT, NETWORK, HTTP_STATUS_CODE, UNEXPECTED
    }
}
