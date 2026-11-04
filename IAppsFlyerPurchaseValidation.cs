namespace AppsFlyerSDK
{
    public interface IAppsFlyerPurchaseValidation
    {
        void didReceivePurchaseRevenueValidationInfo(string validationInfo);
        void didReceivePurchaseRevenueError(string error);
    }
}