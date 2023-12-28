namespace TUI.SDKProvider
{

    public interface IInGamePurchaseSDKProvider : ISDKProvider
    {
        [System.Serializable]
        public class Product
        {
            public string ProductId;
            public string Name;
            public string Description;
            public string FormattedPrice;
        }
        [System.Serializable]
        public class Purchase
        {
            public string ProductId;
            public System.DateTime GrantTime;
        }
        void PurchaseProduct(string productId, System.Action onSuccess = null, System.Action<string> onFailure = null);

        void ConsumeProduct(string productId, System.Action onSuccess = null, System.Action<string> onFailure = null);

        void GetProduct(string productId, System.Action<Product> onSuccess = null, System.Action<string> onFailure = null);

        void GetPurchasedProducts(System.Action<Purchase[]> onSuccess = null, System.Action<string> onFailure = null);
    }
}