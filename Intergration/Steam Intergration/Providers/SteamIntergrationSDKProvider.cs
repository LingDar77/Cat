namespace Cat.Intergration.SteamSDKProviders
{
    using Cat.SDKProvider;

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX

    using System.Collections;
    using Cat.Utillities;
    using Steamworks;
    using UnityEngine;
    
    public partial class SteamIntergrationSDKProvider : DisableInEdtorScript, ISDKProvider
    {
        [SerializeField] private uint appid = 480;
        public bool IsInitialized { get; set; }
        public bool IsAvailable { get; } = true;
        private SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;

        protected virtual void OnEnable()
        {
            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
            }
            try
            {
                if (SteamAPI.RestartAppIfNecessary(new AppId_t(appid)))
                {
                    Debug.Log("[Steamworks.NET] RestartAppIfNecessary");
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

                Application.Quit();
                return;
            }

            IsInitialized = SteamAPI.Init();
            if (!IsInitialized)
            {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                return;
            }

            StartCoroutine(TickSteamCallbacks());
            Debug.Log("Steam SDK Provider Initialized.");
            if (steamAPIWarningMessageHook == null)
            {
                // Set up our callback to receive warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(steamAPIWarningMessageHook);
            }
        }
        protected virtual IEnumerator TickSteamCallbacks()
        {
            while (IsInitialized)
            {
                SteamAPI.RunCallbacks();
                yield return new WaitForSeconds(.1f);
            }
        }
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

    }
#else
    public partial class SteamIntergrationSDKProvider : UnsupportedSDKBase<SteamIntergrationSDKProvider>
    {
    }

#endif
}
