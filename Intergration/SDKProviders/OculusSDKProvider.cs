#if META_INTERACTION_OVR
using Oculus.Platform;
using SFC.XRSDKProvider;
using UnityEngine;

namespace SFC.Intergration.SDKProviders
{
    public class OculusSDKProvider : MonoBehaviour, IXRSDKProvider
    {
        [SerializeField] private bool AutoCreateSession = true;
        private OVRManager manager;

        public bool AdaptiveResolution { get => manager.enableDynamicResolution; set => manager.enableDynamicResolution = value; }
        public int FoveationLevel { get => (int)OVRPlugin.foveatedRenderingLevel; set => OVRPlugin.foveatedRenderingLevel = (OVRPlugin.FoveatedRenderingLevel)value; }
        public bool LateLatching { get => manager.LateLatching; set => manager.LateLatching = value; }
        public int SharpeningLevel
        {
            get => manager.sharpenType == OVRPlugin.LayerSharpenType.None ? 0 : (manager.sharpenType == OVRPlugin.LayerSharpenType.Normal ? 1 : 2);
            set => manager.sharpenType = value == 0 ? OVRPlugin.LayerSharpenType.None : value == 1 ? OVRPlugin.LayerSharpenType.Normal : OVRPlugin.LayerSharpenType.Quality;
        }


        public event System.Action OnRecenterSuccessed;

        private void Start()
        {
            var id = XRPlatformSDKHelper.GetSubsystemID();
            Debug.Log($"Screen Log: Tesing Oculus SDK status, current id: {id}");

            if (id == null || !id.Contains("oculus")) return;
            XRPlatformSDKHelper.SDKProvider = this;
            Debug.Log("Screen Log: using oculus sdk.");

            manager = OVRManager.instance;
            if (manager == null)
            {
                manager = gameObject.AddComponent<OVRManager>();
            }
            manager.LateLatching = true;
            manager.enableDynamicResolution = true;
            manager.AllowRecenter = false;
            OVRPlugin.useDynamicFoveatedRendering = true;
            OVRPlugin.foveatedRenderingLevel = OVRPlugin.FoveatedRenderingLevel.HighTop;
            OVRPlugin.suggestedCpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.PowerSavings;
            OVRPlugin.suggestedGpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.PowerSavings;
            Core.Initialize();

            GroupPresence.SetLobbySession("Test Session").OnComplete(e =>
            {
                Debug.Log($"Screen Log: Set Lobby Session.");

                GroupPresence.SetDestination("Test1").OnComplete(e =>
                {
                    Debug.Log($"Screen Log: Set Destination.");
                    if (AutoCreateSession)
                    {
                        GroupPresence.SetIsJoinable(true).OnComplete(e =>
                        {
                            Debug.Log($"Screen Log: Set Is Joinable.");
                            LaunchInvitePanel();
                        });
                    }


                });

            });

            var skus = new string[] { "TestItem1" };
            Debug.Log("Screen Log: Testing IAP");
            IAP.GetProductsBySKU(skus).OnComplete(e =>
            {
                if (e.IsError)
                {

                    Debug.Log($"Screen Log: {e.GetError().Message}");
                    return;
                }

                foreach (var item in e.GetProductList())
                {
                    Debug.Log($"Screen Log: Product: sku:{item.Sku} name:{item.Name} price:{item.FormattedPrice}");
                }
                IAP.LaunchCheckoutFlow("TestItem1").OnComplete(e =>
                {
                    var p = e.GetPurchase();
                    Debug.Log($"Screen Log: Purchase {p.Sku} {e.IsError}");
                    IAP.GetViewerPurchases().OnComplete(e =>
                    {
                        var purchases = e.GetPurchaseList();
                        foreach (var item in purchases)
                        {
                            Debug.Log($"Screen Log: Purchased {item.Sku} {item.GrantTime}");
                        }
                    });
                });

            });



            Users.GetLoggedInUser().OnComplete(e =>
            {
                Debug.Log($"Screen Log: Get LoggedInUser. {e.GetUser().DisplayName},{e.GetUser().ID}, {e.GetUser().OculusID}");
                Users.Get(e.GetUser().ID).OnComplete(message =>
                {
                    Debug.Log($"Screen Log: Get User. {message.GetUser().DisplayName},{message.GetUser().ID}, {message.GetUser().OculusID}");
                });
            });

            GroupPresence.SetJoinIntentReceivedNotificationCallback(e =>
            {
                Debug.Log($"Screen Log: Recived {e.Data.LobbySessionId}");
            });
            Debug.Log("Screen Log: oculus sdk initialized.");

        }

        private void LaunchInvitePanel()
        {
            Debug.Log("Screen Log: Launching Invite Panel...");
            var options = new InviteOptions();
            GroupPresence.LaunchInvitePanel(options).OnComplete(message =>
             {
                 Debug.Log("Screen Log: Invite panel closed");
                 if (message.IsError)
                 {
                     Debug.Log($"Screen Log: {message.GetError().Message}");
                 }
             });
        }
    }
}
#endif