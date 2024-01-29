namespace TUI.LocomotionSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using TUI.Utillities;
    using UnityEngine;

    public enum MovementSweepState
    {
        Initial,
        AfterFirstHit,
        FoundBlockingCrease,
        FoundBlockingCorner,
    }

    [System.Serializable]
    public class GroundingStatus
    {
        public bool AnyGroundBelow;
        public bool IsStandingOnGround;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public Collider GroundCollider;
        public Vector3 GroundPoint;
        public bool SnappingPrevented;

        public void CopyFrom(GroundingStatus report)
        {
            AnyGroundBelow = report.AnyGroundBelow;
            IsStandingOnGround = report.IsStandingOnGround;
            SnappingPrevented = report.SnappingPrevented;
            GroundNormal = report.GroundNormal;
            InnerGroundNormal = report.InnerGroundNormal;
            OuterGroundNormal = report.OuterGroundNormal;

            GroundCollider = null;
            GroundPoint = Vector3.zero;
        }
        public void Init()
        {
            AnyGroundBelow = false;
            IsStandingOnGround = false;
            SnappingPrevented = false;
            GroundNormal = Vector3.up;
            InnerGroundNormal = Vector3.up;
            OuterGroundNormal = Vector3.up;

            GroundCollider = null;
            GroundPoint = Vector3.zero;
        }
    }

    public struct HitStability
    {
        public bool IsStable;

        public bool FoundInnerNormal;
        public Vector3 InnerNormal;
        public bool FoundOuterNormal;
        public Vector3 OuterNormal;

        public bool ValidStepDetected;
        public Collider SteppedCollider;

        public bool LedgeDetected;
        public bool IsOnEmptySideOfLedge;
        public float DistanceFromLedge;
        public bool IsMovingTowardsEmptySideOfLedge;
        public Vector3 LedgeGroundNormal;
        public Vector3 LedgeRightDirection;
        public Vector3 LedgeFacingDirection;
    }

    public struct RigidbodyProjectionHit
    {
        public Rigidbody Rigidbody;
        public Vector3 HitPoint;
        public Vector3 EffectiveHitNormal;
        public Vector3 HitVelocity;
        public bool StableOnHit;
    }

    [RequireComponent(typeof(CapsuleCollider))]
    public class BuiltinLocomotionSystem : LocomotionSystemBase
    {
        public static class ControllerConstants
        {
            public const int MaxHitsBudget = 8;
            public const int MaxCollisionBudget = 8;
            public const int MaxRigidbodyOverlapsCount = 8;
            public const int MaxGroundingSweepIterations = 2;
            public const float CollisionOffset = 0.004f;
            public const float GroundProbeReboundDistance = 0.02f;
            public const float MinimumGroundProbingDistance = 0.005f;
            public const float GroundProbingBackstepDistance = 0.1f;
            public const float SweepProbingBackstepDistance = 0.002f;
            public const float SecondaryProbesVertical = 0.02f;
            public const float SecondaryProbesHorizontal = 0.001f;
            public const float MinVelocityMagnitude = 0.01f;
            public const float SteppingForwardDistance = 0.03f;
            public const float CorrelationForVerticalObstruction = 0.01f;
        }

        [ReadOnlyInEditor]
        public CapsuleCollider Capsule;

        #region  Simulation Params
        [Header("Simulation Params")]
        public LayerMask StableGroundLayers = -1;
        public float MaxStepHeight = 0.6f;
        [Range(0f, 89f)]
        public float MaxStableSlopeAngle = 60f;
        [Range(1f, 180f)]
        public float MaxStableDenivelationAngle = 180f;

        public float MaxStableDistanceFromLedge = 0.5f;
        public float MinRequiredStepDepth = 0.1f;
        public bool AffectRigidbody = true;
        public float SimulatedCharacterMass = 1f;
        public int MaxMovementIterations = 2;
        public int MaxDecollisionIterations = 1;
        #endregion

        #region  Simulation Status
        [Header("Simulation Status")]
        [ReadOnlyInEditor]
        public Vector3 TargetVelocity;
        [ReadOnlyInEditor]
        public Vector3 TargetPosition;
        [ReadOnlyInEditor]
        public Quaternion TargetRotation;
        [ReadOnlyInEditor]
        public GroundingStatus GroundingStatus = new();
        private readonly GroundingStatus LastGroundingStatus = new();
        private bool forceUngrounded = false;

        #endregion

        #region Private Fields
        private int rigidbodyProjectionHitCount = 0;
        private int overlapsCount;
        private readonly Vector3[] overlaps = new Vector3[ControllerConstants.MaxRigidbodyOverlapsCount];
        private readonly RaycastHit[] internalCharacterHits = new RaycastHit[ControllerConstants.MaxHitsBudget];
        private readonly Collider[] internalProbedColliders = new Collider[ControllerConstants.MaxCollisionBudget];
        private readonly List<Rigidbody> rigidbodiesPushedThisMove = new(ControllerConstants.MaxCollisionBudget);
        private readonly RigidbodyProjectionHit[] internalRigidbodyProjectionHits = new RigidbodyProjectionHit[ControllerConstants.MaxRigidbodyOverlapsCount];
        private Vector3 CharacterUp => TargetRotation * Vector3.up;
        private Vector3 CharacterTransformToCapsuleBottom => Capsule.center + (-Vector3.up * (Capsule.height * 0.5f));
        private Vector3 CharacterTransformToCapsuleBottomHemi => Capsule.center + (-Vector3.up * (Capsule.height * 0.5f)) + (Vector3.up * Capsule.radius);
        private Vector3 CharacterTransformToCapsuleTopHemi => Capsule.center + (Vector3.up * (Capsule.height * 0.5f)) + (-Vector3.up * Capsule.radius);
        #endregion


        #region  Editor Interface
        protected virtual void Reset()
        {
            OnValidate();
        }

        protected virtual void OnValidate()
        {
            this.EnsureComponent(ref Capsule);

            MaxStepHeight = Mathf.Clamp(MaxStepHeight, 0f, Mathf.Infinity);
            MinRequiredStepDepth = Mathf.Clamp(MinRequiredStepDepth, 0f, Capsule.radius);
            MaxStableDistanceFromLedge = Mathf.Clamp(MaxStableDistanceFromLedge, 0f, Capsule.radius);
        }

        protected virtual void AWake()
        {
            OnValidate();

            TargetPosition = transform.position;
            TargetRotation = transform.rotation;
        }

        protected virtual void Update()
        {
            DoSimulation(Time.deltaTime);
        }


        #endregion

        #region  Locomotion System Inteface
        protected override Quaternion GetCurrentRotation()
        {
            return TargetRotation;
        }
        protected override Vector3 GetCurrentVelocity()
        {
            return TargetVelocity;
        }
        public override bool IsOnGround()
        {
            return GroundingStatus.IsStandingOnGround && !forceUngrounded;
        }
        public override bool IsStable()
        {
            return TargetVelocity.y == 0;
        }

        public override void MarkUngrounded()
        {
            if (!forceUngrounded)
            {
                forceUngrounded = true;
                CoroutineHelper.WaitForSeconds(() => forceUngrounded = false, .1f);
            }
        }

        public override void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public override void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public override void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
        }

        #endregion

        #region  Helper Function

        public void DoSimulation(float deltaTime)
        {
            PrepareSimulatioin(deltaTime);
            Simulation(deltaTime);
            transform.SetPositionAndRotation(TargetPosition, TargetRotation);
        }

        protected virtual void PrepareSimulatioin(float deltaTime)
        {
            // NaN propagation safety stop
            if (float.IsNaN(TargetVelocity.x) || float.IsNaN(TargetVelocity.y) || float.IsNaN(TargetVelocity.z))
            {
                TargetVelocity = Vector3.zero;
            }

            rigidbodiesPushedThisMove.Clear();

            BeforeUpdate(deltaTime);

            TargetPosition = transform.position;
            TargetRotation = transform.rotation;
            rigidbodyProjectionHitCount = 0;
            overlapsCount = 0;

            LastGroundingStatus.CopyFrom(GroundingStatus);
            GroundingStatus.Init();

            #region Resolve initial overlaps
            int iterationsMade = 0;
            while (iterationsMade < MaxDecollisionIterations)
            {
                int nbOverlaps = CharacterCollisionsOverlap(TargetPosition, TargetRotation, internalProbedColliders);

                if (nbOverlaps == 0)
                {
                    break;
                }

                for (int i = 0; i < nbOverlaps; i++)
                {
                    if (GetInteractiveRigidbody(internalProbedColliders[i]) != null)
                    {
                        continue;
                    }

                    // Process overlap
                    Transform overlappedTransform = internalProbedColliders[i].GetComponent<Transform>();
                    if (Physics.ComputePenetration(Capsule, TargetPosition, TargetRotation, internalProbedColliders[i], overlappedTransform.position, overlappedTransform.rotation, out Vector3 resolutionDirection, out float resolutionDistance))
                    {
                        resolutionDirection = GetObstructionNormal(resolutionDirection, IsStableOnNormal(resolutionDirection));

                        // Solve overlap
                        Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + ControllerConstants.CollisionOffset);
                        TargetPosition += resolutionMovement;

                        // Remember overlaps
                        if (overlapsCount < overlaps.Length)
                        {
                            overlaps[overlapsCount] = resolutionDirection;
                            overlapsCount++;
                        }

                        break;
                    }
                }

                iterationsMade++;
            }
            #endregion

            #region Ground Probing and Snapping

            if (forceUngrounded)
            {
                TargetPosition += CharacterUp * (ControllerConstants.MinimumGroundProbingDistance * 1.5f);
                return;
            }

            // Choose the appropriate ground probing distance
            float selectedGroundProbingDistance = ControllerConstants.MinimumGroundProbingDistance;
            if (!LastGroundingStatus.SnappingPrevented && LastGroundingStatus.IsStandingOnGround || LastGroundingStatus.AnyGroundBelow)
            {
                selectedGroundProbingDistance = Mathf.Max(Capsule.radius, MaxStepHeight);
            }
            ProbeGround(ref TargetPosition, TargetRotation, selectedGroundProbingDistance, ref GroundingStatus);

            if (!LastGroundingStatus.IsStandingOnGround && GroundingStatus.IsStandingOnGround)
            {
                // Handle stable landing
                TargetVelocity = Vector3.ProjectOnPlane(TargetVelocity, CharacterUp);
                TargetVelocity = GetDirectionTangentToSurface(TargetVelocity, GroundingStatus.GroundNormal) * TargetVelocity.magnitude;
            }

            #endregion

        }

        protected virtual void Simulation(float deltaTime)
        {
            UpdateRotation(ref TargetRotation, deltaTime);

            if (AffectRigidbody)
            {
                int iterationsMade = 0;
                while (iterationsMade < MaxDecollisionIterations)
                {
                    int nbOverlaps = CharacterCollisionsOverlap(TargetPosition, TargetRotation, internalProbedColliders);
                    if (nbOverlaps == 0)
                    {
                        break;
                    }

                    for (int i = 0; i < nbOverlaps; i++)
                    {
                        // Process overlap
                        Transform overlappedTransform = internalProbedColliders[i].GetComponent<Transform>();
                        if (!Physics.ComputePenetration(Capsule, TargetPosition, TargetRotation, internalProbedColliders[i], overlappedTransform.position, overlappedTransform.rotation, out Vector3 resolutionDirection, out float resolutionDistance))
                        {
                            continue;
                        }
                        // Resolve along obstruction direction

                        resolutionDirection = GetObstructionNormal(resolutionDirection, IsStableOnNormal(resolutionDirection));

                        // Solve overlap
                        Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + ControllerConstants.CollisionOffset);
                        TargetPosition += resolutionMovement;

                        // Remember overlaps
                        if (overlapsCount < overlaps.Length)
                        {
                            overlaps[overlapsCount] = resolutionDirection;
                            overlapsCount++;
                        }

                        break;
                    }

                    iterationsMade++;
                }

            }

            UpdateVelocity(ref TargetVelocity, deltaTime);

            //this.CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
            if (TargetVelocity.magnitude < ControllerConstants.MinVelocityMagnitude)
            {
                TargetVelocity = Vector3.zero;
            }

            #region Calculate Character movement from base velocity   
            // Perform the move from base velocity
            if (TargetVelocity.sqrMagnitude > 0f)
            {
                InternalCharacterMove(ref TargetVelocity, deltaTime);
            }

            // Process rigidbody hits/overlaps to affect velocity
            if (AffectRigidbody)
            {
                ProcessVelocityForRigidbodyHits(ref TargetVelocity, deltaTime);
            }
            #endregion

            PostUpdate(deltaTime);
        }

        protected virtual bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(CharacterUp, normal) < MaxStableSlopeAngle;
        }

        protected virtual bool IsStableWithSpecialCases(ref HitStability stabilityReport, Vector3 velocity)
        {

            if (stabilityReport.LedgeDetected)
            {
                if (stabilityReport.IsMovingTowardsEmptySideOfLedge)
                {
                    // Max snap vel
                    Vector3 velocityOnLedgeNormal = Vector3.Project(velocity, stabilityReport.LedgeFacingDirection);
                    if (velocityOnLedgeNormal.sqrMagnitude > 0)
                    {
                        return false;
                    }
                }

                // Distance from ledge
                if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
                {
                    return false;
                }
            }

            // "Launching" off of slopes of a certain denivelation angle
            if (LastGroundingStatus.AnyGroundBelow && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
            {
                float denivelationAngle = Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal);
                if (denivelationAngle > MaxStableDenivelationAngle)
                {
                    return false;
                }

                denivelationAngle = Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal);
                if (denivelationAngle > MaxStableDenivelationAngle)
                {
                    return false;
                }
            }

            return true;
        }

        public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref GroundingStatus groundingReport)
        {
            if (probingDistance < ControllerConstants.MinimumGroundProbingDistance)
            {
                probingDistance = ControllerConstants.MinimumGroundProbingDistance;
            }

            int groundSweepsMade = 0;
            Vector3 groundSweepPosition = probingPosition;
            Vector3 groundSweepDirection = atRotation * -Vector3.up;
            float groundProbeDistanceRemaining = probingDistance;
            while (groundProbeDistanceRemaining > 0 && (groundSweepsMade <= ControllerConstants.MaxGroundingSweepIterations))
            {
                // Sweep for ground detection
                if (!CharacterGroundSweep(groundSweepPosition, atRotation, groundSweepDirection, groundProbeDistanceRemaining, out RaycastHit groundSweepHit))
                {
                    break;
                }

                Vector3 targetPosition = groundSweepPosition + (groundSweepDirection * groundSweepHit.distance);
                HitStability groundHitStabilityReport = new();
                EvaluateHitStability(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, targetPosition, TargetRotation, TargetVelocity, ref groundHitStabilityReport);

                groundingReport.AnyGroundBelow = true;
                groundingReport.GroundNormal = groundSweepHit.normal;
                groundingReport.InnerGroundNormal = groundHitStabilityReport.InnerNormal;
                groundingReport.OuterGroundNormal = groundHitStabilityReport.OuterNormal;
                groundingReport.GroundCollider = groundSweepHit.collider;
                groundingReport.GroundPoint = groundSweepHit.point;
                groundingReport.SnappingPrevented = false;

                // Found stable ground
                if (groundHitStabilityReport.IsStable)
                {
                    // Find all scenarios where ground snapping should be canceled
                    groundingReport.SnappingPrevented = !IsStableWithSpecialCases(ref groundHitStabilityReport, TargetVelocity);

                    groundingReport.IsStandingOnGround = true;

                    // Ground snapping
                    if (!groundingReport.SnappingPrevented)
                    {
                        probingPosition = groundSweepPosition + (groundSweepDirection * (groundSweepHit.distance - ControllerConstants.CollisionOffset));
                    }

                    break;
                }

                // Calculate movement from this iteration and advance position
                Vector3 sweepMovement = (groundSweepDirection * groundSweepHit.distance) + (atRotation * Vector3.up * Mathf.Max(ControllerConstants.CollisionOffset, groundSweepHit.distance));
                groundSweepPosition += sweepMovement;

                // Set remaining distance
                groundProbeDistanceRemaining = Mathf.Min(ControllerConstants.GroundProbeReboundDistance, Mathf.Max(groundProbeDistanceRemaining - sweepMovement.magnitude, 0f));

                // Reorient direction
                groundSweepDirection = Vector3.ProjectOnPlane(groundSweepDirection, groundSweepHit.normal).normalized;

                groundSweepsMade++;
            }
        }

        protected virtual Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, CharacterUp);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        protected virtual bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime)
        {
            if (deltaTime <= 0f) return false;

            bool wasCompleted = true;
            Vector3 remainingMovementDirection = transientVelocity.normalized;
            float remainingMovementMagnitude = transientVelocity.magnitude * deltaTime;
            int sweepsMade = 0;
            bool hitSomethingThisSweepIteration = true;
            Vector3 tmpMovedPosition = TargetPosition;
            bool previousHitIsStable = false;
            Vector3 previousVelocity = Vector3.zero;
            Vector3 previousObstructionNormal = Vector3.zero;
            MovementSweepState sweepState = MovementSweepState.Initial;

            // Project movement against current overlaps before doing the sweeps
            for (int i = 0; i < overlapsCount; i++)
            {
                Vector3 overlapNormal = overlaps[i];
                if (Vector3.Dot(remainingMovementDirection, overlapNormal) < 0f)
                {
                    bool stableOnHit = IsStableOnNormal(overlapNormal);
                    Vector3 velocityBeforeProjection = transientVelocity;
                    Vector3 obstructionNormal = GetObstructionNormal(overlapNormal, stableOnHit);

                    InternalHandleVelocityProjection(stableOnHit, obstructionNormal, ref sweepState, previousHitIsStable, previousVelocity, previousObstructionNormal, ref transientVelocity, ref remainingMovementMagnitude, ref remainingMovementDirection);

                    previousHitIsStable = stableOnHit;
                    previousVelocity = velocityBeforeProjection;
                    previousObstructionNormal = obstructionNormal;
                }
            }

            // Sweep the desired movement to detect collisions
            while (remainingMovementMagnitude > 0f && (sweepsMade <= MaxMovementIterations) && hitSomethingThisSweepIteration)
            {
                bool foundClosestHit = false;
                Vector3 closestSweepHitPoint = default;
                Vector3 closestSweepHitNormal = default;
                float closestSweepHitDistance = 0f;
                Collider closestSweepHitCollider = null;


                int numOverlaps = CharacterCollisionsOverlap(tmpMovedPosition, TargetRotation, internalProbedColliders, 0f, false);
                if (numOverlaps > 0)
                {
                    closestSweepHitDistance = 0f;

                    float mostObstructingOverlapNormalDotProduct = 2f;

                    for (int i = 0; i < numOverlaps; i++)
                    {
                        Collider tmpCollider = internalProbedColliders[i];

                        if (Physics.ComputePenetration(Capsule, tmpMovedPosition, TargetRotation, tmpCollider, tmpCollider.transform.position, tmpCollider.transform.rotation, out Vector3 resolutionDirection, out float resolutionDistance))
                        {
                            float dotProduct = Vector3.Dot(remainingMovementDirection, resolutionDirection);
                            if (dotProduct < 0f && dotProduct < mostObstructingOverlapNormalDotProduct)
                            {
                                mostObstructingOverlapNormalDotProduct = dotProduct;

                                closestSweepHitNormal = resolutionDirection;
                                closestSweepHitCollider = tmpCollider;
                                closestSweepHitPoint = tmpMovedPosition + (TargetRotation * Capsule.center) + (resolutionDirection * resolutionDistance);

                                foundClosestHit = true;
                            }
                        }
                    }
                }


                if (!foundClosestHit && CharacterCollisionsSweep(tmpMovedPosition, TargetRotation, remainingMovementDirection, remainingMovementMagnitude + ControllerConstants.CollisionOffset, out RaycastHit closestSweepHit, internalCharacterHits) > 0)
                {
                    closestSweepHitNormal = closestSweepHit.normal;
                    closestSweepHitDistance = closestSweepHit.distance;
                    closestSweepHitCollider = closestSweepHit.collider;
                    closestSweepHitPoint = closestSweepHit.point;

                    foundClosestHit = true;
                }

                if (foundClosestHit)
                {
                    // Calculate movement from this iteration
                    Vector3 sweepMovement = remainingMovementDirection * Mathf.Max(0f, closestSweepHitDistance - ControllerConstants.CollisionOffset);
                    tmpMovedPosition += sweepMovement;
                    remainingMovementMagnitude -= sweepMovement.magnitude;

                    // Evaluate if hit is stable
                    HitStability moveHitStabilityReport = new();
                    EvaluateHitStability(closestSweepHitCollider, closestSweepHitNormal, closestSweepHitPoint, tmpMovedPosition, TargetRotation, transientVelocity, ref moveHitStabilityReport);

                    // Handle stepping up steps points higher than bottom capsule radius
                    bool foundValidStepHit = false;
                    if (moveHitStabilityReport.ValidStepDetected)
                    {
                        float obstructionCorrelation = Mathf.Abs(Vector3.Dot(closestSweepHitNormal, CharacterUp));
                        if (obstructionCorrelation <= ControllerConstants.CorrelationForVerticalObstruction)
                        {
                            Vector3 stepForwardDirection = Vector3.ProjectOnPlane(-closestSweepHitNormal, CharacterUp).normalized;
                            Vector3 stepCastStartPoint = tmpMovedPosition + (stepForwardDirection * ControllerConstants.SteppingForwardDistance) +
                                (CharacterUp * MaxStepHeight);

                            // Cast downward from the top of the stepping height
                            int nbStepHits = CharacterCollisionsSweep(stepCastStartPoint, TargetRotation, -CharacterUp, MaxStepHeight, out _, internalCharacterHits, 0f);

                            // Check for hit corresponding to stepped collider
                            for (int i = 0; i < nbStepHits; i++)
                            {
                                if (internalCharacterHits[i].collider == moveHitStabilityReport.SteppedCollider)
                                {
                                    Vector3 endStepPosition = stepCastStartPoint + (-CharacterUp * (internalCharacterHits[i].distance - ControllerConstants.CollisionOffset));
                                    tmpMovedPosition = endStepPosition;
                                    foundValidStepHit = true;

                                    // Project velocity on ground normal at step
                                    transientVelocity = Vector3.ProjectOnPlane(transientVelocity, CharacterUp);
                                    remainingMovementDirection = transientVelocity.normalized;

                                    break;
                                }
                            }
                        }
                    }

                    // Handle movement solving
                    if (!foundValidStepHit)
                    {
                        Vector3 obstructionNormal = GetObstructionNormal(closestSweepHitNormal, moveHitStabilityReport.IsStable);


                        // Handle remembering rigidbody hits
                        if (AffectRigidbody && closestSweepHitCollider.attachedRigidbody)
                        {
                            StoreRigidbodyHit(closestSweepHitCollider.attachedRigidbody, transientVelocity, closestSweepHitPoint, obstructionNormal, moveHitStabilityReport);
                        }

                        bool stableOnHit = moveHitStabilityReport.IsStable;
                        Vector3 velocityBeforeProj = transientVelocity;

                        // Project velocity for next iteration
                        InternalHandleVelocityProjection(stableOnHit, obstructionNormal, ref sweepState, previousHitIsStable, previousVelocity, previousObstructionNormal, ref transientVelocity, ref remainingMovementMagnitude, ref remainingMovementDirection);

                        previousHitIsStable = stableOnHit;
                        previousVelocity = velocityBeforeProj;
                        previousObstructionNormal = obstructionNormal;
                    }
                }
                // If we hit nothing...
                else
                {
                    hitSomethingThisSweepIteration = false;
                }

                // Safety for exceeding max sweeps allowed
                sweepsMade++;
                if (sweepsMade > MaxMovementIterations)
                {
                    remainingMovementMagnitude = 0f;

                    transientVelocity = Vector3.zero;

                    wasCompleted = false;
                }
            }

            // Move position for the remainder of the movement
            tmpMovedPosition += remainingMovementDirection * remainingMovementMagnitude;
            TargetPosition = tmpMovedPosition;

            return wasCompleted;
        }

        protected virtual Vector3 GetObstructionNormal(Vector3 hitNormal, bool stableOnHit)
        {
            // Find hit/obstruction/offset normal
            Vector3 obstructionNormal = hitNormal;
            if (GroundingStatus.IsStandingOnGround && !stableOnHit)
            {
                Vector3 obstructionLeftAlongGround = Vector3.Cross(GroundingStatus.GroundNormal, obstructionNormal).normalized;
                obstructionNormal = Vector3.Cross(obstructionLeftAlongGround, CharacterUp).normalized;
            }

            // Catch cases where cross product between parallel normals returned 0
            if (obstructionNormal.sqrMagnitude == 0f)
            {
                obstructionNormal = hitNormal;
            }

            return obstructionNormal;
        }

        protected virtual void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal, HitStability hitStabilityReport)
        {
            if (rigidbodyProjectionHitCount < internalRigidbodyProjectionHits.Length)
            {
                if (hitRigidbody.TryGetComponent<ILocomotionSystem>(out _))
                {
                    return;
                }

                RigidbodyProjectionHit rph = new()
                {
                    Rigidbody = hitRigidbody,
                    HitPoint = hitPoint,
                    EffectiveHitNormal = obstructionNormal,
                    HitVelocity = hitVelocity,
                    StableOnHit = hitStabilityReport.IsStable
                };

                internalRigidbodyProjectionHits[rigidbodyProjectionHitCount] = rph;
                rigidbodyProjectionHitCount++;
            }
        }

        protected virtual void InternalHandleVelocityProjection(bool stableOnHit, Vector3 obstructionNormal, ref MovementSweepState sweepState, bool previousHitIsStable, Vector3 previousVelocity, Vector3 previousObstructionNormal, ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
        {
            if (transientVelocity.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 velocityBeforeProjection = transientVelocity;

            if (stableOnHit)
            {
                HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
            }
            else
            {
                // Handle projection
                if (sweepState == MovementSweepState.Initial)
                {
                    HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
                    sweepState = MovementSweepState.AfterFirstHit;
                }
                // Blocking crease handling
                else if (sweepState == MovementSweepState.AfterFirstHit)
                {
                    EvaluateCrease(transientVelocity, previousVelocity, obstructionNormal, previousObstructionNormal, stableOnHit, previousHitIsStable, GroundingStatus.IsStandingOnGround, out bool foundCrease, out Vector3 creaseDirection);

                    if (foundCrease)
                    {
                        if (GroundingStatus.IsStandingOnGround)
                        {
                            transientVelocity = Vector3.zero;
                            sweepState = MovementSweepState.FoundBlockingCorner;
                        }
                        else
                        {
                            transientVelocity = Vector3.Project(transientVelocity, creaseDirection);
                            sweepState = MovementSweepState.FoundBlockingCrease;
                        }
                    }
                    else
                    {
                        HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
                    }
                }
                // Blocking corner handling
                else if (sweepState == MovementSweepState.FoundBlockingCrease)
                {
                    transientVelocity = Vector3.zero;
                    sweepState = MovementSweepState.FoundBlockingCorner;
                }
            }

            float newVelocityFactor = transientVelocity.magnitude / velocityBeforeProjection.magnitude;
            remainingMovementMagnitude *= newVelocityFactor;
            remainingMovementDirection = transientVelocity.normalized;
        }

        protected virtual void EvaluateCrease(Vector3 currentCharacterVelocity, Vector3 previousCharacterVelocity, Vector3 currentHitNormal, Vector3 previousHitNormal, bool currentHitIsStable, bool previousHitIsStable, bool characterIsStable, out bool isValidCrease, out Vector3 creaseDirection)
        {
            isValidCrease = false;
            creaseDirection = default;

            if (!characterIsStable || !currentHitIsStable || !previousHitIsStable)
            {
                Vector3 tmpBlockingCreaseDirection = Vector3.Cross(currentHitNormal, previousHitNormal).normalized;
                float dotPlanes = Vector3.Dot(currentHitNormal, previousHitNormal);
                bool isVelocityConstrainedByCrease = false;

                // Avoid calculations if the two planes are the same
                if (dotPlanes < 0.999f)
                {
                    // TODO: can this whole part be made simpler? (with 2d projections, etc)
                    Vector3 normalAOnCreasePlane = Vector3.ProjectOnPlane(currentHitNormal, tmpBlockingCreaseDirection).normalized;
                    Vector3 normalBOnCreasePlane = Vector3.ProjectOnPlane(previousHitNormal, tmpBlockingCreaseDirection).normalized;
                    float dotPlanesOnCreasePlane = Vector3.Dot(normalAOnCreasePlane, normalBOnCreasePlane);

                    Vector3 enteringVelocityDirectionOnCreasePlane = Vector3.ProjectOnPlane(previousCharacterVelocity, tmpBlockingCreaseDirection).normalized;

                    if (dotPlanesOnCreasePlane <= (Vector3.Dot(-enteringVelocityDirectionOnCreasePlane, normalAOnCreasePlane) + 0.001f) &&
                        dotPlanesOnCreasePlane <= (Vector3.Dot(-enteringVelocityDirectionOnCreasePlane, normalBOnCreasePlane) + 0.001f))
                    {
                        isVelocityConstrainedByCrease = true;
                    }
                }

                if (isVelocityConstrainedByCrease)
                {
                    // Flip crease direction to make it representative of the real direction our velocity would be projected to
                    if (Vector3.Dot(tmpBlockingCreaseDirection, currentCharacterVelocity) < 0f)
                    {
                        tmpBlockingCreaseDirection = -tmpBlockingCreaseDirection;
                    }

                    isValidCrease = true;
                    creaseDirection = tmpBlockingCreaseDirection;
                }
            }
        }

        protected virtual void HandleVelocityProjection(ref Vector3 velocity, Vector3 obstructionNormal, bool stableOnHit)
        {
            if (stableOnHit)
            {
                // Handle stable landing
                velocity = Vector3.ProjectOnPlane(velocity, CharacterUp);
                velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                return;
            }
            velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
        }

        protected virtual void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
        {
            for (int i = 0; i < rigidbodyProjectionHitCount; i++)
            {
                RigidbodyProjectionHit bodyHit = internalRigidbodyProjectionHits[i];

                if (!bodyHit.Rigidbody || rigidbodiesPushedThisMove.Contains(bodyHit.Rigidbody))
                {
                    continue;
                }
                // Remember we hit this rigidbody
                rigidbodiesPushedThisMove.Add(bodyHit.Rigidbody);

                float characterMass = SimulatedCharacterMass;
                Vector3 characterVelocity = bodyHit.HitVelocity;

                bool hitBodyIsDynamic = !bodyHit.Rigidbody.isKinematic;
                float hitBodyMassAtPoint = bodyHit.Rigidbody.mass; // todo
                Vector3 hitBodyVelocity = bodyHit.Rigidbody.velocity;

                // Calculate the ratio of the total mass that the character mass represents
                float characterToBodyMassRatio;
                if (characterMass + hitBodyMassAtPoint > 0f)
                {
                    characterToBodyMassRatio = characterMass / (characterMass + hitBodyMassAtPoint);
                }
                else
                {
                    characterToBodyMassRatio = 0.5f;
                }

                ComputeCollisionResolutionForHitBody(bodyHit.EffectiveHitNormal, characterVelocity, hitBodyVelocity, characterToBodyMassRatio, out Vector3 velocityChangeOnCharacter, out Vector3 velocityChangeOnBody);

                processedVelocity += velocityChangeOnCharacter;

                if (hitBodyIsDynamic)
                {
                    bodyHit.Rigidbody.AddForceAtPosition(velocityChangeOnBody, bodyHit.HitPoint, ForceMode.VelocityChange);
                }
            }

        }

        protected virtual void ComputeCollisionResolutionForHitBody(Vector3 hitNormal, Vector3 characterVelocity, Vector3 bodyVelocity, float characterToBodyMassRatio, out Vector3 velocityChangeOnCharacter, out Vector3 velocityChangeOnBody)
        {
            velocityChangeOnCharacter = default;
            velocityChangeOnBody = default;

            float bodyToCharacterMassRatio = 1f - characterToBodyMassRatio;
            float characterVelocityMagnitudeOnHitNormal = Vector3.Dot(characterVelocity, hitNormal);
            float bodyVelocityMagnitudeOnHitNormal = Vector3.Dot(bodyVelocity, hitNormal);

            // if character velocity was going against the obstruction, restore the portion of the velocity that got projected during the movement phase
            if (characterVelocityMagnitudeOnHitNormal < 0f)
            {
                Vector3 restoredCharacterVelocity = hitNormal * characterVelocityMagnitudeOnHitNormal;
                velocityChangeOnCharacter += restoredCharacterVelocity;
            }

            // solve impulse velocities on both bodies, but only if the body velocity would be giving resistance to the character in any way
            if (bodyVelocityMagnitudeOnHitNormal > characterVelocityMagnitudeOnHitNormal)
            {
                Vector3 relativeImpactVelocity = hitNormal * (bodyVelocityMagnitudeOnHitNormal - characterVelocityMagnitudeOnHitNormal);
                velocityChangeOnCharacter += relativeImpactVelocity * bodyToCharacterMassRatio;
                velocityChangeOnBody += -relativeImpactVelocity * characterToBodyMassRatio;
            }
        }

        protected virtual bool CheckIfColliderValidForCollisions(Collider coll)
        {
            // Ignore self
            if (coll == Capsule)
            {
                return false;
            }

            if (!InternalIsColliderValidForCollisions(coll))
            {
                return false;
            }

            return true;
        }

        protected virtual bool InternalIsColliderValidForCollisions(Collider coll)
        {

            // Custom checks
            bool colliderValid = IsColliderValid(coll);
            if (!colliderValid)
            {
                return false;
            }

            return true;
        }

        protected virtual void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, Vector3 withCharacterVelocity, ref HitStability stabilityReport)
        {

            Vector3 atCharacterUp = atCharacterRotation * Vector3.up;
            Vector3 innerHitDirection = Vector3.ProjectOnPlane(hitNormal, atCharacterUp).normalized;

            stabilityReport.IsStable = IsStableOnNormal(hitNormal);

            stabilityReport.FoundInnerNormal = false;
            stabilityReport.FoundOuterNormal = false;
            stabilityReport.InnerNormal = hitNormal;
            stabilityReport.OuterNormal = hitNormal;

            // Ledge handling

            float ledgeCheckHeight = MaxStepHeight;


            bool isStableLedgeInner = false;
            bool isStableLedgeOuter = false;
            var inner = hitPoint + (atCharacterUp * ControllerConstants.SecondaryProbesVertical) + (innerHitDirection * ControllerConstants.SecondaryProbesHorizontal);
            var outer = hitPoint + (atCharacterUp * ControllerConstants.SecondaryProbesVertical) + (-innerHitDirection * ControllerConstants.SecondaryProbesHorizontal);

            if (CharacterCollisionsRaycast(inner, -atCharacterUp, ledgeCheckHeight + ControllerConstants.SecondaryProbesVertical, out RaycastHit innerLedgeHit, internalCharacterHits) > 0)
            {
                Vector3 innerLedgeNormal = innerLedgeHit.normal;
                stabilityReport.InnerNormal = innerLedgeNormal;
                stabilityReport.FoundInnerNormal = true;
                isStableLedgeInner = IsStableOnNormal(innerLedgeNormal);
            }

            if (CharacterCollisionsRaycast(outer, -atCharacterUp, ledgeCheckHeight + ControllerConstants.SecondaryProbesVertical, out RaycastHit outerLedgeHit, internalCharacterHits) > 0)
            {
                Vector3 outerLedgeNormal = outerLedgeHit.normal;
                stabilityReport.OuterNormal = outerLedgeNormal;
                stabilityReport.FoundOuterNormal = true;
                isStableLedgeOuter = IsStableOnNormal(outerLedgeNormal);
            }

            stabilityReport.LedgeDetected = (isStableLedgeInner != isStableLedgeOuter);
            if (stabilityReport.LedgeDetected)
            {
                stabilityReport.IsOnEmptySideOfLedge = isStableLedgeOuter && !isStableLedgeInner;
                stabilityReport.LedgeGroundNormal = isStableLedgeOuter ? stabilityReport.OuterNormal : stabilityReport.InnerNormal;
                stabilityReport.LedgeRightDirection = Vector3.Cross(hitNormal, stabilityReport.LedgeGroundNormal).normalized;
                stabilityReport.LedgeFacingDirection = Vector3.ProjectOnPlane(Vector3.Cross(stabilityReport.LedgeGroundNormal, stabilityReport.LedgeRightDirection), CharacterUp).normalized;
                stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane(hitPoint - (atCharacterPosition + (atCharacterRotation * CharacterTransformToCapsuleBottom)), atCharacterUp).magnitude;
                stabilityReport.IsMovingTowardsEmptySideOfLedge = Vector3.Dot(withCharacterVelocity.normalized, stabilityReport.LedgeFacingDirection) > 0f;
            }

            if (stabilityReport.IsStable)
            {
                stabilityReport.IsStable = IsStableWithSpecialCases(ref stabilityReport, withCharacterVelocity);
            }


            // Step handling
            if (!stabilityReport.IsStable)
            {
                // Stepping not supported on dynamic rigidbodies
                Rigidbody hitRigidbody = hitCollider.attachedRigidbody;
                if (!(hitRigidbody && !hitRigidbody.isKinematic))
                {
                    DetectSteps(atCharacterPosition, atCharacterRotation, hitPoint, innerHitDirection, ref stabilityReport);

                    if (stabilityReport.ValidStepDetected)
                    {
                        stabilityReport.IsStable = true;
                    }
                }
            }
        }

        protected virtual void DetectSteps(Vector3 characterPosition, Quaternion characterRotation, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStability stabilityReport)
        {
            Vector3 characterUp = characterRotation * Vector3.up;
            Vector3 verticalCharToHit = Vector3.Project(hitPoint - characterPosition, characterUp);
            Vector3 horizontalCharToHitDirection = Vector3.ProjectOnPlane(hitPoint - characterPosition, characterUp).normalized;
            Vector3 stepCheckStartPos = hitPoint - verticalCharToHit + (characterUp * MaxStepHeight) + (horizontalCharToHitDirection * ControllerConstants.CollisionOffset * 3f);

            // Do outer step check with capsule cast on hit point
            int nbStepHits = CharacterCollisionsSweep(stepCheckStartPos, characterRotation, -characterUp, MaxStepHeight + ControllerConstants.CollisionOffset, out _, internalCharacterHits, 0f);

            // Check for overlaps and obstructions at the hit position
            if (CheckStepValidity(nbStepHits, characterPosition, characterRotation, innerHitDirection, stepCheckStartPos, out Collider tmpCollider))
            {
                stabilityReport.ValidStepDetected = true;
                stabilityReport.SteppedCollider = tmpCollider;
            }


        }

        protected virtual bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Quaternion characterRotation, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider)
        {
            hitCollider = null;
            Vector3 characterUp = characterRotation * Vector3.up;

            // Find the farthest valid hit for stepping
            bool foundValidStepPosition = false;

            while (nbStepHits > 0 && !foundValidStepPosition)
            {
                // Get farthest hit among the remaining hits
                RaycastHit farthestHit = new();
                float farthestDistance = 0f;
                int farthestIndex = 0;
                for (int i = 0; i < nbStepHits; i++)
                {
                    float hitDistance = internalCharacterHits[i].distance;
                    if (hitDistance > farthestDistance)
                    {
                        farthestDistance = hitDistance;
                        farthestHit = internalCharacterHits[i];
                        farthestIndex = i;
                    }
                }

                Vector3 characterPositionAtHit = stepCheckStartPos + (-characterUp * (farthestHit.distance - ControllerConstants.CollisionOffset));

                int atStepOverlaps = CharacterCollisionsOverlap(characterPositionAtHit, characterRotation, internalProbedColliders);
                if (atStepOverlaps <= 0)
                {
                    // Check for outer hit slope normal stability at the step position
                    if (CharacterCollisionsRaycast(farthestHit.point + (characterUp * ControllerConstants.SecondaryProbesVertical) + (-innerHitDirection * ControllerConstants.SecondaryProbesHorizontal), -characterUp, MaxStepHeight + ControllerConstants.SecondaryProbesVertical, out RaycastHit outerSlopeHit, internalCharacterHits) > 0)
                    {
                        if (IsStableOnNormal(outerSlopeHit.normal))
                        {
                            // Cast upward to detect any obstructions to moving there
                            if (CharacterCollisionsSweep(characterPosition, characterRotation, characterUp, MaxStepHeight - farthestHit.distance, out _, internalCharacterHits) <= 0)
                            {
                                // Do inner step check...
                                bool innerStepValid = false;

                                // At the capsule center at the step height
                                if (CharacterCollisionsRaycast(characterPosition + Vector3.Project(characterPositionAtHit - characterPosition, characterUp), -characterUp, MaxStepHeight, out RaycastHit innerStepHit, internalCharacterHits) > 0)
                                {
                                    if (IsStableOnNormal(innerStepHit.normal))
                                    {
                                        innerStepValid = true;
                                    }
                                }

                                if (!innerStepValid)
                                {
                                    // At inner step of the step point
                                    if (CharacterCollisionsRaycast(farthestHit.point + (innerHitDirection * ControllerConstants.SecondaryProbesHorizontal), -characterUp, MaxStepHeight, out innerStepHit, internalCharacterHits) > 0)
                                    {
                                        if (IsStableOnNormal(innerStepHit.normal))
                                        {
                                            innerStepValid = true;
                                        }
                                    }
                                }

                                // Final validation of step
                                if (innerStepValid)
                                {
                                    hitCollider = farthestHit.collider;
                                    return true;
                                }
                            }
                        }
                    }
                }

                // Discard hit if not valid step
                if (!foundValidStepPosition)
                {
                    nbStepHits--;
                    if (farthestIndex < nbStepHits)
                    {
                        internalCharacterHits[farthestIndex] = internalCharacterHits[nbStepHits];
                    }
                }
            }

            return false;
        }

        protected virtual Rigidbody GetInteractiveRigidbody(Collider onCollider)
        {
            Rigidbody colliderAttachedRigidbody = onCollider.attachedRigidbody;
            if (colliderAttachedRigidbody)
            {
                if (!colliderAttachedRigidbody.isKinematic)
                {
                    return colliderAttachedRigidbody;
                }
            }
            return null;
        }

        protected virtual int CharacterCollisionsOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {

            Vector3 bottom = position + (rotation * CharacterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * CharacterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }

            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(bottom, top, Capsule.radius + inflate, overlappedColliders, StableGroundLayers, QueryTriggerInteraction.Ignore);

            // Filter out invalid colliders
            int nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                if (!CheckIfColliderValidForCollisions(overlappedColliders[i]))
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        overlappedColliders[i] = overlappedColliders[nbHits];
                    }
                }
            }

            return nbHits;
        }

        protected virtual int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f)
        {

            Vector3 bottom = position + (rotation * CharacterTransformToCapsuleBottomHemi) - (direction * ControllerConstants.SweepProbingBackstepDistance);
            Vector3 top = position + (rotation * CharacterTransformToCapsuleTopHemi) - (direction * ControllerConstants.SweepProbingBackstepDistance);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }

            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(bottom, top, Capsule.radius + inflate, direction, hits, distance + ControllerConstants.SweepProbingBackstepDistance, StableGroundLayers, QueryTriggerInteraction.Ignore);

            // Hits filter
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            // Capsule cast
            int nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                hits[i].distance -= ControllerConstants.SweepProbingBackstepDistance;

                RaycastHit hit = hits[i];
                float hitDistance = hit.distance;

                // Filter out the invalid hits
                if (hitDistance <= 0f || !CheckIfColliderValidForCollisions(hit.collider))
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    // Remember closest valid hit
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestDistance = hitDistance;
                    }
                }
            }

            return nbHits;
        }

        private bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = new RaycastHit();

            // Capsule cast
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                position + (rotation * CharacterTransformToCapsuleBottomHemi) - (direction * ControllerConstants.GroundProbingBackstepDistance),
                position + (rotation * CharacterTransformToCapsuleTopHemi) - (direction * ControllerConstants.GroundProbingBackstepDistance),
                Capsule.radius,
                direction,
                internalCharacterHits,
                distance + ControllerConstants.GroundProbingBackstepDistance,
                StableGroundLayers,
                QueryTriggerInteraction.Ignore);

            // Hits filter
            bool foundValidHit = false;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < nbUnfilteredHits; i++)
            {
                RaycastHit hit = internalCharacterHits[i];
                float hitDistance = hit.distance;

                // Find the closest valid hit
                if (hitDistance > 0f && CheckIfColliderValidForCollisions(hit.collider))
                {
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestHit.distance -= ControllerConstants.GroundProbingBackstepDistance;
                        closestDistance = hitDistance;

                        foundValidHit = true;
                    }
                }
            }

            return foundValidHit;
        }

        protected virtual int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits)
        {
            int nbUnfilteredHits = Physics.RaycastNonAlloc(position, direction, hits, distance, StableGroundLayers, QueryTriggerInteraction.Ignore);

            // Hits filter
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            // Raycast
            int nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                RaycastHit hit = hits[i];
                float hitDistance = hit.distance;

                // Filter out the invalid hits
                if (hitDistance <= 0f || !CheckIfColliderValidForCollisions(hit.collider))
                {
                    nbHits--;
                    if (i < nbHits)
                    {
                        hits[i] = hits[nbHits];
                    }
                }
                else
                {
                    // Remember closest valid hit
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestDistance = hitDistance;
                    }
                }
            }

            return nbHits;
        }

        #endregion
    }
}
