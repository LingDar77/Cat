using UnityEngine;
namespace SFC.SDKProvider
{

    public class UnsupportedSDKBase<ImplementType> : MonoBehaviour, ISDKProvider
    {
        public bool IsInitialized { get; } = false;
        public bool IsAvailable { get; } = false;
        private void OnEnable()
        {
            Debug.LogWarning($"{typeof(ImplementType).Name} is not supported on this platform.");
            enabled = false;
        }
    }
}