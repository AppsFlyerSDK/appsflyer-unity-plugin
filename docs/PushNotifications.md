---
title: Push Notifications
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 8
hidden: false
---

## Unity push notifications using OneLink & Firebase

<span class="annotation-recommended">Recommended</span>  
This is the recommended method for implementing push notification measurement in the Unity Appsflyer SDK.

**To integrate AppsFlyer with Android push notifications:**

1. In your `AppsFlyerObjectScript.cs`, call `addPushNotificationDeepLinkPath` **before** calling `start`:

```csharp
AppsFlyer.addPushNotificationDeepLinkPath("af_push_link");
```

In the example above, the SDK is configured to look for the `af_push_link` key in the first level of the push notification payload.  
When calling `addPushNotificationDeepLinkPath` the SDK verifies that:

- The required key exists in the payload.
- The key contains a valid OneLink URL.

> 📘 Note
> 
> `addPushNotificationDeepLinkPath` accepts an array of strings too, to allow you to extract the relevant key from nested JSON structures. For more information, see [`addPushNotificationDeepLinkPath`](https://dev.appsflyer.com/hc/docs/api#addpushnotificationdeeplinkpath).

2. Create a new Firebase Unity app and follow the [Firebase guide](https://firebase.google.com/docs/unity/setup) (Import the SDK package and the GoogleService files to Unity)
3. Create FirebaseManager empty object and add `FirebaseManager.cs` to it: 

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using AppsFlyerSDK;

public class FirebaseManager : MonoBehaviour
{
    private Vector2 scrollViewVector = Vector2.zero;
    private string logText = "";
    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;
    private string topic = "TestTopic";

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string errorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    errorCode = String.Format("Error.{0}: ",
                      ((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(errorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }


    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Setup message event handlers.
    void InitializeFirebase()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(task =>
        {
            LogTaskCompletion(task, "SubscribeAsync");
        });
        DebugLog("Firebase Messaging Initialized");

        // This will display the prompt to request permission to receive
        // notifications if the prompt has not already been displayed before. (If
        // the user already responded to the prompt, thier decision is cached by
        // the OS and can be changed in the OS settings).
        Firebase.Messaging.FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
          task =>
          {
              LogTaskCompletion(task, "RequestPermissionAsync");
          }
        );
        isFirebaseInitialized = true;
    }

    public virtual void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        DebugLog("Received a new message");
        var notification = e.Message.Notification;
        if (notification != null)
        {
            DebugLog("title: " + notification.Title);
            DebugLog("body: " + notification.Body);
            var android = notification.Android;
            if (android != null)
            {
                DebugLog("android channel_id: " + android.ChannelId);
            }
        }
        if (e.Message.From.Length > 0)
            DebugLog("from: " + e.Message.From);
        if (e.Message.Link != null)
        {
            DebugLog("link: " + e.Message.Link.ToString());
        }
        if (e.Message.Data.Count > 0)
        {
            DebugLog("data:");
            foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                     e.Message.Data)
            {
                DebugLog("  " + iter.Key + ": " + iter.Value);
            }
        }
#if UNITY_IOS && !UNITY_EDITOR
        DebugLog("DidReceivedDeepLink: true");
        appsFlyerObj.DidReceivedDeepLink = true;
        var dataDict = new Dictionary<string, string>(e.Message.Data);
        AppsFlyeriOS.handlePushNotification(dataDict);
#endif
    }

    public virtual void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        DebugLog("Received Registration Token: " + token.Token);
    }

    public void ToggleTokenOnInit()
    {
        bool newValue = !Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled;
        Firebase.Messaging.FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
        DebugLog("Set TokenRegistrationOnInitEnabled to " + newValue);
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // End our messaging session when the program exits.
    public void OnDestroy()
    {
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        print(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize)
        {
            int index = logText.IndexOf("\n");
            logText = logText.Substring(index + 1);
        }

        scrollViewVector.y = int.MaxValue;
    }
}
```

> 📘 Note
> 
> In the `OnMessageReceived` function, for iOS specifically, we are calling the `AppsFlyeriOS.handlePushNotification(Dictionary<string, string> pushPayload)` method ([read more](https://dev.appsflyer.com/hc/docs/api#addpushnotificationdeeplinkpath)) in order to trigger the AppsFlyerSDK method that is getting overridden due to Firebase's swizzling.

## iOS

1. Add an [APN Authentication key](https://firebase.google.com/docs/cloud-messaging/ios/client#upload_your_apns_authentication_key) to your Firebase Unity iOS app  
   ![Firebase - project setting - cloud messeging](https://files.readme.io/a3e7231-Screenshot_2023-05-30_at_18.35.30.png)
2. After building your project, open the XCode project and add the following capabilities:
   - Push Notifications
   - Background Modes -> Remote Notifications
   - Associated Domains (for UDL) - [read more](https://developer.apple.com/documentation/xcode/supporting-associated-domains)
3. If you are getting errors when building the app due to bitcode, disable bitcode

## Android

1. In the AndroidManifest: Add the following service:
   ```xml
   <service android:name="com.google.firebase.messaging.MessageForwardingService"
               android:permission="android.permission.BIND_JOB_SERVICE" android:exported="true" />
   ```
2. Create a Keystore and a key for your app and generate the SHA1 fingerprint ([and the SHA256 fingerprint for Android App Links](https://dev.appsflyer.com/hc/docs/dl_android_init_setup#procedures-for-android-app-links)) 
   - SHA1 for Firebase  
     ![Firebase - app settings - Android app - adding SHA1 fingerprint](https://files.readme.io/0bfbfe6-Screenshot_2023-05-30_at_18.39.06.png)