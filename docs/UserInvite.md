

##  <a id="UserInviteAttribution"> User invite attribution

AppsFlyer allows you to attribute and record installs originating from user invites within your app. Allowing your existing users to invite their friends and contacts as new users to your app can be a key growth factor for your app.

Example:
```c#

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData, IAppsFlyerUserInvite {

void Start()
  {
    //...

    AppsFlyer.initSDK("devkey", "appID");
    AppsFlyer.setAppInviteOneLinkID("XXXX"); //set up the one link ID for the user invite
    AppsFlyer.startSDK();
  }
   

    //...
    public void generateAppsFlyerLink()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("channel", "some_channel");
        parameters.Add("campaign", "some_campaign");
        parameters.Add("additional_param1", "some_param1");
        parameters.Add("additional_param2", "some_param2");
       
        // other params
        //parameters.Add("referrerName", "some_referrerName");
        //parameters.Add("referrerImageUrl", "some_referrerImageUrl");
        //parameters.Add("customerID", "some_customerID");
        //parameters.Add("baseDeepLink", "some_baseDeepLink");
        //parameters.Add("brandDomain", "some_brandDomain");
        

        AppsFlyer.generateUserInviteLink(parameters, this);
    }


    ... 

    public void onInviteLinkGenerated(string link)
    {
        AppsFlyer.AFLog("onInviteLinkGenerated", link);
    }

    public void onInviteLinkGeneratedFailure(string error)
    {
        AppsFlyer.AFLog("onInviteLinkGeneratedFailure", error);
    }

    public void onOpenStoreLinkGenerated(string link)
    {
        AppsFlyer.AFLog("onOpenStoreLinkGenerated", link);
    }
}
```
