---
title: "Purchase and subscription validation"
slug: "purchase-subscription-validation-unity"
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
hidden: true
order: 13
---
Purchase validation ensures that only real, store-confirmed in-app purchases and subscriptions are measured in AppsFlyer. It improves revenue accuracy, helps prevent reporting errors, and supports better campaign decisions.

AppsFlyer offers two products to support purchase validation:

- **Receipt validation** – A free, lightweight solution for basic in-app purchase verification.
- **ROI360 Store revenue** – A premium, comprehensive solution for full revenue accuracy, including subscription lifecycle coverage and net revenue reporting.

For more information, see [Purchase and subscription validation](https://support.appsflyer.com/hc/en-us/articles/42120228484241--WIP-Purchase-and-subscription-validation-Overview).

## SDK Integration Methods

AppsFlyer supports two SDK integration methods for sending in-app purchase data to AppsFlyer for validation:

### 1. Manual Integration method – Validate and Log

Call Validate and Log (`validateAndSendInAppPurchase`) every time a transaction occurs in the app (such as an in-app purchase, subscription start, or trial start). The method sends the transaction to AppsFlyer, which validates it with the store and generates the relevant in-app event.

- Requires an explicit call from the app for every transaction
- Suitable for apps that need to capture events not included in the Purchase Connector’s default coverage. With the Validate and log method, developers can explicitly target and send these additional events.

To get started see: [`Validate and Log`](https://dev.appsflyer.com/hc/docs/validate-and-log-unity)
---

### 2. Automated Integration method – Purchase Connector

Purchase Connector automatically detects in-app purchases and subscriptions made on the device. Once initialized, it sends the required data to AppsFlyer without additional logging code.

- Supported only by ROI360 products and recommended for most apps
- Triggers validation automatically and returns the result to the client in real time
- The following capabilities cannot be supported through simple customization of the Validate and Log method and therefore require Purchase Connector:
    - Logging subscription revenue from users who subscribed before the integration was added.
    - Logging subscription price changes, ensuring revenue reflects updated pricing.

To get started see: [Unity purchase SDK connector](https://dev.appsflyer.com/hc/docs/purchase-connector-unity)


---
> ⚠️ Important
> 
> To avoid duplicate event logging and inconsistent validation results, it’s recommended to use only one integration method per application.
