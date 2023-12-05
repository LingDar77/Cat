using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace SFC.Intergration.Netcode
{
    public class NewBehaviourScript : MonoBehaviour
    {
        [ContextMenu("Load Test Scene")]
        public void LoadTestScene()
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Netcode Test", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
}
