using System.Linq;
using SFC.KinematicLocomotionSystem;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SFC.Intergration.Netcode
{
    /// <summary>
    /// An adaptor to make the built-in locomotion system compatiable
    /// with netcode. <see cref="ILocomotionSystem" />
    /// The Adaptor also managed the input action map.
    /// And the capsule info is also synced if the system is implemented
    /// by KCC <see cref="KinematicCharacterMotor"/> and
    /// <see cref="KinematicCharacterSystem"/>
    /// </summary>
    public class NetcodeControllerAdaptor : NetworkBehaviour
    {
        [System.Serializable]
        public class CapsuleSyncOptions
        {
            public bool SyncCapsuleRidus = false;
            public bool SyncCapsuleHeight = true;
            public bool SyncCapsuleYOffset = true;

            public int Count()
            {
                return (SyncCapsuleRidus ? 1 : 0) + (SyncCapsuleHeight ? 1 : 0) + (SyncCapsuleYOffset ? 1 : 0);
            }
        }
        [SerializeField] private InputActionAsset[] ActionMaps;
        [SerializeField] private CapsuleSyncOptions CapsuleSyncOption;
        public UnityEvent OnSpawnedAsClient;

        private KinematicCharacterMotor motor;
        private float[] capsuleInfos;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            var locomotionSystem = GetComponent<ILocomotionSystem>();
            if (locomotionSystem.transform.TryGetComponent(out motor) && IsOwner)
            {
                motor.OnCapsuleDimensionsUpdated += HandleCapsuleDimensions;
            }

            if (!IsOwner)
            {
                locomotionSystem.enabled = false;
                var actions = locomotionSystem.ActionProviders.ToList();
                foreach (var action in actions)
                {
                    action.enabled = false;
                }
                OnSpawnedAsClient.Invoke();
                return;
            }

            foreach (var map in ActionMaps)
            {
                foreach (var action in map)
                {
                    action.Enable();
                }
            }

            var syncLength = CapsuleSyncOption.Count();
            if (syncLength == 0) return;
            capsuleInfos = new float[syncLength];
        }

        private void HandleCapsuleDimensions(float capsuleRidus, float capsuleHeight, float capsuleYOffset)
        {
            if (capsuleInfos == null) return;
            var index = 0;
            if (CapsuleSyncOption.SyncCapsuleRidus)
            {
                capsuleInfos[index++] = capsuleRidus;
            }
            if (CapsuleSyncOption.SyncCapsuleHeight)
            {
                capsuleInfos[index++] = capsuleHeight;
            }
            if (CapsuleSyncOption.SyncCapsuleYOffset)
            {
                capsuleInfos[index++] = capsuleYOffset;
            }
            UpdateCapsuleDemensionsServerRpc(capsuleInfos);
        }

        private void OnDisable()
        {
            if (!IsOwner && IsSpawned) return;

            foreach (var map in ActionMaps)
            {
                foreach (var action in map)
                {
                    action.Disable();
                }
            }
        }

        private void OnEnable()
        {
            if (IsSpawned) return;
            foreach (var map in ActionMaps)
            {
                foreach (var action in map)
                {
                    action.Enable();
                }
            }
        }

        [ServerRpc]
        private void UpdateCapsuleDemensionsServerRpc(float[] capsuleInfos)
        {
            if (!IsOwner) return;
            UpdateCapsuleDemensionsClientRpc(capsuleInfos);

        }
        [ClientRpc]
        private void UpdateCapsuleDemensionsClientRpc(float[] capsuleInfos)
        {
            if (IsOwner) return;
            var capsuleRidus = motor.Capsule.radius;
            var capsuleHeight = motor.Capsule.height;
            var capsuleYOffset = motor.Capsule.center.y;

            var index = 0;
            if (CapsuleSyncOption.SyncCapsuleRidus)
            {
                capsuleRidus = capsuleInfos[index++];
            }
            if (CapsuleSyncOption.SyncCapsuleHeight)
            {
                capsuleHeight = capsuleInfos[index++];
            }
            if (CapsuleSyncOption.SyncCapsuleYOffset)
            {
                capsuleYOffset = capsuleInfos[index++];
            }

            motor.SetCapsuleDimensions(capsuleRidus, capsuleHeight, capsuleYOffset);
        }

    }
}
