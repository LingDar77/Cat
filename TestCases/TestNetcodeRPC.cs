using Unity.Netcode;
using UnityEngine;

namespace SFC.TestCases
{

    public class TestNetcodeRPC : NetworkBehaviour
    {
        [ContextMenu("Test RPC")]
        public void TestRPC()
        {
            if (!IsServer && IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                TestServerRpc(0, NetworkObjectId);
            }
        }

        [ClientRpc]
        void TestClientRpc(int value, ulong sourceNetworkObjectId)
        {
            Debug.LogWarning($"Client Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            if (IsOwner) //Only send an RPC to the server on the client that owns the NetworkObject that owns this NetworkBehaviour instance
            {
                TestServerRpc(value + 1, sourceNetworkObjectId);
            }
        }

        [ServerRpc]
        void TestServerRpc(int value, ulong sourceNetworkObjectId)
        {
            Debug.LogWarning($"Server Received the RPC #{value} on NetworkObject #{sourceNetworkObjectId}");
            TestClientRpc(value, sourceNetworkObjectId);
        }
    }
}
