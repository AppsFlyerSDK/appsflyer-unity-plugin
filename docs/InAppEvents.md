# In-App events

In-App Events provide insight on what is happening in your app. It is recommended to take the time and define the events you want to measure to allow you to measure ROI (Return on Investment) and LTV (Lifetime Value).

Recording in-app events is performed by calling sendEvent with event name and value parameters. See In-App Events documentation for more details.

Find more info about recording events [here](https://dev.appsflyer.com/hc/docs/in-app-events-sdk).

- [sendEvent](#sendEvent)


## <a id="sendEvent"> Send Event

`void sendEvent(string eventName, Dictionary<string, string> eventValues)`


| parameter      | type                         | description                                   |
| -----------    |----------------------------- |------------------------------------------     |
| `eventName`    | `string`                     | The name of the event                         |
| `eventValues`  | `Dictionary<string, string>` | The event values that are sent with the event |


*Example:*

```c#
Dictionary<string, string> eventValues = new Dictionary<string, string>();
eventValues.Add(AFInAppEvents.CURRENCY, "USD");
eventValues.Add(AFInAppEvents.REVENUE, "0.99");
eventValues.Add("af_quantity", "1");
AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
```

