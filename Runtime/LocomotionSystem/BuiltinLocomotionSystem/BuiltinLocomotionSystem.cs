namespace TUI.LocomotioinSystem
{
    using TUI.Utillities;
    using UnityEngine;

    [System.Serializable]
    public class LocomotionControllerConfig
    {
        [Header("General")]
        public LayerMask StableGroundLayers = -1;
        public float CollisionDetectOffset;
        [Header("Step")]
        public float MaxStepHeight = 0.5f;
        public float MaxStableDistanceFromLedge = 0.5f;
        public float MaxStableSlopeAngle = 60f;
        public float MaxStableDenivelationAngle = 180f;
        public float SimulatedCharacterMass = 1f;
        [Header("Simulation")]
        public bool InteractiveRigidbodyHandling = true;
        public int MaxMovementIterations = 4;
        public int MaxDecollisionIterations = 1;

    }
    [System.Serializable]
    public class GroundingStatus
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;

        public void Init()
        {
            FoundAnyGround = false;
            IsStableOnGround = false;
            SnappingPrevented = false;
            GroundNormal = Vector3.zero;
            InnerGroundNormal = Vector3.zero;
        }
    }


    [RequireComponent(typeof(CapsuleCollider))]
    public class BuiltinLocomotionSystem : LocomotionSystemBase
    {
        [ReadOnlyInEditor]
        public GroundingStatus GroundingStatus = new();
        public LocomotionControllerConfig config = new();


        private GroundingStatus LastGroundingStatus = new();

    }
}
