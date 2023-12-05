using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SFC.Intergration.AA
{
    public class AddressableSceneManagerAdaptor : MonoBehaviour
    {
        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += () =>
            {
                NetworkManager.Singleton.SceneManager.SceneManagerHandler = new AddressablesSceneManagerHandler();
            };
        }
    }
}
