#nullable enable

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class InAppPurchaseValidationResult : EventArgs
{
    public bool success;
    public ProductPurchase? productPurchase;
    public ValidationFailureData? failureData;
    public string? token;
}

[System.Serializable]
public class ProductPurchase
{
    public string? kind;
    public string? purchaseTimeMillis;
    public int purchaseState;
    public int consumptionState;
    public string? developerPayload;
    public string? orderId;
    public int purchaseType;
    public int acknowledgementState;
    public string? purchaseToken;
    public string? productId;
    public int quantity;
    public string? obfuscatedExternalAccountId;
    public string? obfuscatedExternalProfil;
    public string? regionCode;
}

[System.Serializable]
public class ValidationFailureData
{
    public int status;
    public string? description;
}

[System.Serializable]
public class SubscriptionValidationResult
{
    public bool success;
    public SubscriptionPurchase? subscriptionPurchase;
    public ValidationFailureData? failureData;
    public string? token;
}

[System.Serializable]
public class SubscriptionPurchase
{
    public string? acknowledgementState;
    public CanceledStateContext? canceledStateContext;
    public ExternalAccountIdentifiers? externalAccountIdentifiers;
    public string? kind;
    public string? latestOrderId;
    public List<SubscriptionPurchaseLineItem>? lineItems;
    public string? linkedPurchaseToken;
    public PausedStateContext? pausedStateContext;
    public string? regionCode;
    public string? startTime;
    public SubscribeWithGoogleInfo? subscribeWithGoogleInfo;
    public string? subscriptionState;
    public TestPurchase? testPurchase;
}

[System.Serializable]
public class CanceledStateContext
{
    public DeveloperInitiatedCancellation? developerInitiatedCancellation;
    public ReplacementCancellation? replacementCancellation;
    public SystemInitiatedCancellation? systemInitiatedCancellation;
    public UserInitiatedCancellation? userInitiatedCancellation;

}

[System.Serializable]
public class ExternalAccountIdentifiers
{
    public string? externalAccountId;
    public string? obfuscatedExternalAccountId;
    public string? obfuscatedExternalProfileId;
}

[System.Serializable]
public class SubscriptionPurchaseLineItem
{
    public AutoRenewingPlan? autoRenewingPlan;
    public DeferredItemReplacement? deferredItemReplacement;
    public string? expiryTime;
    public OfferDetails? offerDetails;
    public PrepaidPlan? prepaidPlan;
    public string? productId;
}

[System.Serializable]
public class PausedStateContext
{
    public string? autoResumeTime;
}

[System.Serializable]
public class SubscribeWithGoogleInfo
{
    public string? emailAddress;
    public string? familyName;
    public string? givenName;
    public string? profileId;
    public string? profileName;
}

[System.Serializable]
public class TestPurchase{}

[System.Serializable]
public class DeveloperInitiatedCancellation{}

[System.Serializable]
public class ReplacementCancellation{}

[System.Serializable]
public class SystemInitiatedCancellation{}

[System.Serializable]
public class UserInitiatedCancellation
{
    public CancelSurveyResult? cancelSurveyResult;
    public string? cancelTime;
}

[System.Serializable]
public class AutoRenewingPlan
{
    public string? autoRenewEnabled;
    public SubscriptionItemPriceChangeDetails? priceChangeDetails;
}

[System.Serializable]
public class DeferredItemReplacement
{
    public string? productId;
}

[System.Serializable]
public class OfferDetails
{
    public List<string>? offerTags;
    public string? basePlanId;
    public string? offerId;
}

[System.Serializable]
public class PrepaidPlan
{
    public string? allowExtendAfterTime;
}

[System.Serializable]
public class CancelSurveyResult
{
    public string? reason;
    public string? reasonUserInput;
}

[System.Serializable]
public class SubscriptionItemPriceChangeDetails
{
    public string? expectedNewPriceChargeTime;
    public Money? newPrice;
    public string? priceChangeMode;
    public string? priceChangeState;
}

[System.Serializable]
 public class Money
 {
    public string? currencyCode;
    public long nanos;
    public long units;
 }
   