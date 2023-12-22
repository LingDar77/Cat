#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLE_STEAMWORKS
#endif
using SFC.SDKProvider;
using SFC.Utillities;
#if !DISABLE_STEAMWORKS
using Steamworks;
#endif
using UnityEngine;

namespace SFC.Intergration.SteamSDKProviders
{
    public class SteamIntergrationSDKProvider : DisableInEdtorScript, ISDKProvider
    {
        private bool initialized = false;
#if !DISABLE_STEAMWORKS
        private SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;
#endif

        public bool IsAvailable()
        {
#if DISABLE_STEAMWORKS
            return false;
#else
            return true;
#endif
        }

        public bool IsInitialized()
        {
            return initialized;
        }
#if !DISABLE_STEAMWORKS
        private void OnEnable()
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
                if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
                {
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

            initialized = SteamAPI.Init();
            if (!initialized)
            {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
            }
            Debug.Log("Steam SDK Provider Initialized.");
            if (steamAPIWarningMessageHook == null)
            {
                // Set up our callback to receive warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(steamAPIWarningMessageHook);
            }
        }
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }
        private void OnDisable()
        {
            SteamAPI.Shutdown();
        }
#else
        private void Start()
        {
            Debug.LogError("Steam SDK Provider not available in current platform.");
        }
#endif
    }
}