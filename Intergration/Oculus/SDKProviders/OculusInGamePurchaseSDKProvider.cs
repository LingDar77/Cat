namespace Cat.Intergration.Oculus.SDKProviders
{
    using System.Collections.Generic;
    using Oculus.Platform;
    using Oculus.Platform.Models;
    using Cat.SDKProvider;
    using Cat.Utillities;
    using UnityEngine;
    using UnityEngine.XR;

    public partial class OculusInGamePurchaseSDKProvider : DisableInEdtorScript, IInGamePurchaseSDKProvider
    {
        public bool IsInitialized { get => Core.IsInitialized(); }
        public bool IsAvailable
        {
            get
            {
                List<XRInputSubsystem> subsystems = new();
                SubsystemManager.GetInstances(subsystems);
                if (subsystems.Count == 0) return false;

                return subsystems[0].subsystemDescriptor.id.Contains("oculus");
            }
        }

        private void Start()
        {
            if (!Core.IsInitialized())
            {
                Core.AsyncInitialize();
            }
        }
        public void PurchaseProduct(string productId, System.Action onSuccess = null, System.Action<string> onFailure = null)
        {
            IAP.LaunchCheckoutFlow(productId).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke();
            });

        }
        public void ConsumeProduct(string productId, System.Action onSuccess = null, System.Action<string> onFailure = null)
        {
            IAP.ConsumePurchase(productId).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke();
            });
        }
        public void GetProduct(string productId, System.Action<IInGamePurchaseSDKProvider.Product> onSuccess = null, System.Action<string> onFailure = null)
        {
            IAP.GetProductsBySKU(new string[] { productId }).OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                onSuccess?.Invoke(CreateProduct(e.GetProductList()[0]));
            });
        }
        public void GetPurchasedProducts(System.Action<IInGamePurchaseSDKProvider.Purchase[]> onSuccess = null, System.Action<string> onFailure = null)
        {
            IAP.GetViewerPurchases().OnComplete(e =>
            {
                if (e.IsError)
                {
                    onFailure?.Invoke(e.GetError().Message);
                    return;
                }
                var result = new List<IInGamePurchaseSDKProvider.Purchase>();
                var purchases = e.GetPurchaseList();
                foreach (var item in purchases)
                {
                    result.Add(CreatePurchase(item));
                }
                onSuccess?.Invoke(result.ToArray());
            });
        }



        protected IInGamePurchaseSDKProvider.Product CreateProduct(Product product)
        {
            return new()
            {
                ProductId = product.Sku,
                Name = product.Name,
                FormattedPrice = product.FormattedPrice,
                Description = product.Description
            };
        }
        protected IInGamePurchaseSDKProvider.Purchase CreatePurchase(Purchase purchase)
        {
            return new()
            {
                ProductId = purchase.Sku,
                GrantTime = purchase.GrantTime
            };
        }


    }
}