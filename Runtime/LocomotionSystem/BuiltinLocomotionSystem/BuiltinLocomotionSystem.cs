namespace TUI.LocomotioinSystem
{
    using System.Collections.Generic;
    using TUI.KinematicLocomotionSystem;
    using TUI.Utillities;
    using UnityEngine;

    public static class LocomotionControllerConstant
    {
        public const int MaxHitsBudget = 16;
        public const int MaxCollisionBudget = 16;
        public const int MaxGroundingSweepIterations = 2;
        public const int MaxRigidbodyOverlapsCount = 16;
        public const float CollisionOffset = 0.01f;
        public const float GroundProbeReboundDistance = 0.02f;
        public const float MinimumGroundProbingDistance = 0.005f;
        public const float GroundProbingBackstepDistance = 0.1f;
        public const float SweepProbingBackstepDistance = 0.002f;
        public const float SecondaryProbesVertical = 0.02f;
        public const float SecondaryProbesHorizontal = 0.001f;
        public const float MinVelocityMagnitude = 0.01f;
    }

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

    [System.Serializable]
    public class LocomotioinControllerParams
    {
        public Vector3 TargetPosition;
        public Quaternion TargetRotation;
        public Vector3 CharacterUp => TargetRotation * Vector3.up;
        public Vector3 CharacterTransformToCapsuleBottom;
        public Vector3 CharacterTransformToCapsuleBottomHemi;
        public Vector3 CharacterTransformToCapsuleTopHemi;
        public int RigidbodyProjectionHitCount = 0;
        public int OverlapsCount;
        public bool LastMovementIterationFoundAnyGround;
        public Vector3 BaseVelocity;
        public readonly Vector3[] OverlapsNormals = new Vector3[LocomotionControllerConstant.MaxRigidbodyOverlapsCount];
        public readonly RaycastHit[] InternalCharacterHits = new RaycastHit[LocomotionControllerConstant.MaxHitsBudget];
        public readonly Collider[] InternalProbedColliders = new Collider[LocomotionControllerConstant.MaxCollisionBudget];
        public readonly List<Rigidbody> RigidbodiesPushedThisMove = new();
        public readonly RigidbodyProjectionHit[] InternalRigidbodyProjectionHits = new RigidbodyProjectionHit[LocomotionControllerConstant.MaxRigidbodyOverlapsCount];
    }

    [RequireComponent(typeof(CapsuleCollider))]
    public class BuiltinLocomotionSystem : LocomotionSystemBase
    {
        [ReadOnlyInEditor]
        public CapsuleCollider Capsule;
        [ReadOnlyInEditor]
        public GroundingStatus GroundingStatus = new();
        [ReadOnlyInEditor]
        public LocomotioinControllerParams parmams = new();
        public LocomotionControllerConfig config = new();


        protected GroundingStatus LastGroundingStatus = new();

        protected virtual void Reset()
        {
            OnValidate();
        }
        
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (Capsule == null) Capsule = GetComponent<CapsuleCollider>();
#endif
            config.MaxStepHeight = Mathf.Clamp(config.MaxStepHeight, 0f, Mathf.Infinity);
            config.MaxStableDistanceFromLedge = Mathf.Clamp(config.MaxStableDistanceFromLedge, 0f, Capsule.radius);
            parmams.CharacterTransformToCapsuleBottom = Capsule.center + (-Vector3.up * (Capsule.height * 0.5f));
            parmams.CharacterTransformToCapsuleBottomHemi = Capsule.center + (-Vector3.up * (Capsule.height * 0.5f)) + (Vector3.up * Capsule.radius);
            parmams.CharacterTransformToCapsuleTopHemi = Capsule.center + (Vector3.up * (Capsule.height * 0.5f)) + (-Vector3.up * Capsule.radius);
        }

        protected virtual void Awake()
        {
            parmams.TargetPosition = transform.position;
            parmams.TargetRotation = transform.rotation;
        }

        protected virtual void Update()
        {
            var time = Time.deltaTime;
            UpdatePhase1(time);
            UpdatePhase2(time);

            transform.SetPositionAndRotation(parmams.TargetPosition, parmams.TargetRotation);
        }

        private void BeforeUpdate(float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.BeforeProcess(time);
            }
        }
        
        private void PostUpdate(float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.AfterProcess(time);
            }
        }

        private void UpdateVelocity(ref Vector3 velocity, float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.ProcessVelocity(ref velocity, time);
            }
        }
        
        private void UpdateRotation(ref Quaternion rotation, float time)
        {
            foreach (var provider in ActionProviders)
            {
                provider.ProcessRotation(ref rotation, time);
            }
        }

        public void UpdatePhase1(float deltaTime)
        {

            parmams.RigidbodiesPushedThisMove.Clear();

            BeforeUpdate(deltaTime);

            parmams.TargetPosition = transform.position;
            parmams.TargetRotation = transform.rotation;
            parmams.RigidbodyProjectionHitCount = 0;
            parmams.OverlapsCount = 0;

            LastGroundingStatus = GroundingStatus;
            GroundingStatus.Init();

            #region Ground Probing and Snapping

            float selectedGroundProbingDistance = LocomotionControllerConstant.MinimumGroundProbingDistance;
            if (!LastGroundingStatus.SnappingPrevented && (LastGroundingStatus.IsStableOnGround || parmams.LastMovementIterationFoundAnyGround))
            {
                selectedGroundProbingDistance = Mathf.Max(Capsule.radius, config.MaxStepHeight);


            }

            ProbeGround(ref parmams.TargetPosition, parmams.TargetRotation, selectedGroundProbingDistance, ref GroundingStatus);

            if (!LastGroundingStatus.IsStableOnGround && GroundingStatus.IsStableOnGround)
            {
                // Handle stable landing
                parmams.BaseVelocity = Vector3.ProjectOnPlane(parmams.BaseVelocity, parmams.CharacterUp);
                parmams.BaseVelocity = GetDirectionTangentToSurface(parmams.BaseVelocity, GroundingStatus.GroundNormal) * parmams.BaseVelocity.magnitude;
            }

            parmams.LastMovementIterationFoundAnyGround = false;

            #endregion


        }

        public void UpdatePhase2(float deltaTime)
        {
            UpdateRotation(ref parmams.TargetRotation, deltaTime);

            if (config.InteractiveRigidbodyHandling)
            {

                #region Resolve overlaps that could've been caused by rotation or physics movers simulation pushing the character
                int iterationsMade = 0;
                bool overlapSolved = false;
                while (iterationsMade < config.MaxDecollisionIterations && !overlapSolved)
                {
                    int nbOverlaps = CharacterCollisionsOverlap(parmams.TargetPosition, parmams.TargetRotation, parmams.InternalProbedColliders);
                    if (nbOverlaps > 0)
                    {
                        for (int i = 0; i < nbOverlaps; i++)
                        {
                            // Process overlap
                            Transform overlappedTransform = parmams.InternalProbedColliders[i].GetComponent<Transform>();
                            if (Physics.ComputePenetration(
                                    Capsule,
                                    parmams.TargetPosition,
                                    parmams.TargetRotation,
                                    parmams.InternalProbedColliders[i],
                                    overlappedTransform.position,
                                    overlappedTransform.rotation,
                                    out Vector3 resolutionDirection,
                                    out float resolutionDistance))
                            {

                                resolutionDirection = GetObstructionNormal(resolutionDirection, IsStableOnNormal(resolutionDirection));

                                // Solve overlap
                                Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + LocomotionControllerConstant.CollisionOffset);
                                parmams.TargetPosition += resolutionMovement;

                                // If interactiveRigidbody, register as rigidbody hit for velocity
                                if (config.InteractiveRigidbodyHandling)
                                {
                                    Rigidbody probedRigidbody = GetInteractiveRigidbody(parmams.InternalProbedColliders[i]);
                                    if (probedRigidbody != null)
                                    {
                                        HitStabilityReport tmpReport = new()
                                        {
                                            IsStable = IsStableOnNormal(resolutionDirection)
                                        };
                                        if (tmpReport.IsStable)
                                        {
                                            parmams.LastMovementIterationFoundAnyGround = tmpReport.IsStable;
                                        }
                                        Vector3 estimatedCollisionPoint = parmams.TargetPosition;

                                        StoreRigidbodyHit(
                                            probedRigidbody,
                                            parmams.BaseVelocity,
                                            estimatedCollisionPoint,
                                            resolutionDirection);
                                    }
                                }

                                // Remember overlaps
                                if (parmams.OverlapsCount < parmams.OverlapsNormals.Length)
                                {
                                    parmams.OverlapsNormals[parmams.OverlapsCount] = resolutionDirection;
                                    parmams.OverlapsCount++;
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        overlapSolved = true;
                    }

                    iterationsMade++;
                }
                #endregion
            }


            UpdateVelocity(ref parmams.BaseVelocity, deltaTime);

            //this.CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
            if (parmams.BaseVelocity.magnitude < LocomotionControllerConstant.MinVelocityMagnitude)
            {
                parmams.BaseVelocity = Vector3.zero;
            }

            #region Calculate Character movement from base velocity   
            // Perform the move from base velocity
            if (parmams.BaseVelocity.sqrMagnitude > 0f)
            {
                InternalCharacterMove(ref parmams.BaseVelocity, deltaTime);
            }

            // Process rigidbody hits/overlaps to affect velocity
            if (config.InteractiveRigidbodyHandling)
            {
                ProcessVelocityForRigidbodyHits(ref parmams.BaseVelocity);
            }
            #endregion

            PostUpdate(deltaTime);

        }

        public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref GroundingStatus groundingReport)
        {
            if (probingDistance < LocomotionControllerConstant.MinimumGroundProbingDistance)
            {
                probingDistance = LocomotionControllerConstant.MinimumGroundProbingDistance;
            }

            int groundSweepsMade = 0;
            bool groundSweepingIsOver = false;
            Vector3 groundSweepPosition = probingPosition;
            Vector3 groundSweepDirection = atRotation * -Vector3.up;
            float groundProbeDistanceRemaining = probingDistance;
            while (groundProbeDistanceRemaining > 0 && (groundSweepsMade <= LocomotionControllerConstant.MaxGroundingSweepIterations) && !groundSweepingIsOver)
            {
                // Sweep for ground detection
                if (CharacterGroundSweep(
                        groundSweepPosition, // position
                        atRotation, // rotation
                        groundSweepDirection, // direction
                        groundProbeDistanceRemaining, // distance
                        out RaycastHit groundSweepHit)) // hit
                {
                    Vector3 targetPosition = groundSweepPosition + (groundSweepDirection * groundSweepHit.distance);
                    HitStabilityReport groundHitStabilityReport = new();
                    EvaluateHitStability(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, targetPosition, parmams.TargetRotation, ref groundHitStabilityReport);

                    groundingReport.FoundAnyGround = true;
                    groundingReport.GroundNormal = groundSweepHit.normal;
                    groundingReport.InnerGroundNormal = groundHitStabilityReport.InnerNormal;
                    groundingReport.SnappingPrevented = false;

                    // Found stable ground
                    if (groundHitStabilityReport.IsStable)
                    {
                        // Find all scenarios where ground snapping should be canceled
                        groundingReport.SnappingPrevented = !IsStableWithSpecialCases(ref groundHitStabilityReport);

                        groundingReport.IsStableOnGround = true;

                        // Ground snapping
                        if (!groundingReport.SnappingPrevented)
                        {
                            probingPosition = groundSweepPosition + (groundSweepDirection * (groundSweepHit.distance - LocomotionControllerConstant.CollisionOffset));
                        }

                        groundSweepingIsOver = true;
                    }
                    else
                    {
                        Vector3 sweepMovement = (groundSweepDirection * groundSweepHit.distance) + (atRotation * Vector3.up * Mathf.Max(LocomotionControllerConstant.CollisionOffset, groundSweepHit.distance));
                        groundSweepPosition += sweepMovement;

                        groundProbeDistanceRemaining = Mathf.Min(LocomotionControllerConstant.GroundProbeReboundDistance, Mathf.Max(groundProbeDistanceRemaining - sweepMovement.magnitude, 0f));

                        groundSweepDirection = Vector3.ProjectOnPlane(groundSweepDirection, groundSweepHit.normal).normalized;
                    }
                }
                else
                {
                    groundSweepingIsOver = true;
                }

                groundSweepsMade++;
            }
        }

        private bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = new RaycastHit();
            position.y -= config.CollisionDetectOffset;
            // Capsule cast
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                position + (rotation * parmams.CharacterTransformToCapsuleBottomHemi) - (direction * LocomotionControllerConstant.GroundProbingBackstepDistance),
                position + (rotation * parmams.CharacterTransformToCapsuleTopHemi) - (direction * LocomotionControllerConstant.GroundProbingBackstepDistance),
                Capsule.radius,
                direction,
                parmams.InternalCharacterHits,
                distance + LocomotionControllerConstant.GroundProbingBackstepDistance,
                 config.StableGroundLayers,
                QueryTriggerInteraction.Ignore);

            // Hits filter
            bool foundValidHit = false;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < nbUnfilteredHits; i++)
            {
                RaycastHit hit = parmams.InternalCharacterHits[i];
                float hitDistance = hit.distance;

                // Find the closest valid hit
                if (hitDistance > 0f && CheckIfColliderValidForCollisions(hit.collider))
                {
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestHit.distance -= LocomotionControllerConstant.GroundProbingBackstepDistance;
                        closestDistance = hitDistance;

                        foundValidHit = true;
                    }
                }
            }

            return foundValidHit;
        }

        private bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(parmams.CharacterUp, normal) <= config.MaxStableSlopeAngle;
        }

        private bool IsStableWithSpecialCases(ref HitStabilityReport stabilityReport)
        {

            if (stabilityReport.LedgeDetected)
            {

                if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > config.MaxStableDistanceFromLedge)
                {
                    return false;
                }
            }

            if (LastGroundingStatus.FoundAnyGround && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
            {
                float denivelationAngle = Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal);
                if (denivelationAngle > config.MaxStableDenivelationAngle)
                {
                    return false;
                }
                denivelationAngle = Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal);
                if (denivelationAngle > config.MaxStableDenivelationAngle)
                {
                    return false;
                }
            }

            return true;
        }

        private Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, parmams.CharacterUp);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        private Vector3 GetObstructionNormal(Vector3 hitNormal, bool stableOnHit)
        {
            Vector3 obstructionNormal = hitNormal;
            if (GroundingStatus.IsStableOnGround && !stableOnHit)
            {
                Vector3 obstructionLeftAlongGround = Vector3.Cross(GroundingStatus.GroundNormal, obstructionNormal).normalized;
                obstructionNormal = Vector3.Cross(obstructionLeftAlongGround, parmams.CharacterUp).normalized;
            }

            if (obstructionNormal.sqrMagnitude == 0f)
            {
                obstructionNormal = hitNormal;
            }

            return obstructionNormal;
        }

        private void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal)
        {
            if (parmams.RigidbodyProjectionHitCount < parmams.InternalRigidbodyProjectionHits.Length)
            {
                if (!hitRigidbody.GetComponent<KinematicCharacterMotor>())
                {
                    RigidbodyProjectionHit rph = new()
                    {
                        Rigidbody = hitRigidbody,
                        HitPoint = hitPoint,
                        EffectiveHitNormal = obstructionNormal,
                        HitVelocity = hitVelocity,
                    };

                    parmams.InternalRigidbodyProjectionHits[parmams.RigidbodyProjectionHitCount] = rph;
                    parmams.RigidbodyProjectionHitCount++;
                }
            }
        }

        private void EvaluateCrease(Vector3 currentCharacterVelocity, Vector3 previousCharacterVelocity, Vector3 currentHitNormal, Vector3 previousHitNormal, bool currentHitIsStable, bool previousHitIsStable, bool characterIsStable, out bool isValidCrease, out Vector3 creaseDirection)
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

        private void HandleVelocityProjection(ref Vector3 velocity, Vector3 obstructionNormal, bool stableOnHit)
        {
            if (GroundingStatus.IsStableOnGround)
            {
                // On stable slopes, simply reorient the movement without any loss
                if (stableOnHit)
                {
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                // On blocking hits, project the movement on the obstruction while following the grounding plane
                else
                {
                    Vector3 obstructionRightAlongGround = Vector3.Cross(obstructionNormal, GroundingStatus.GroundNormal).normalized;
                    Vector3 obstructionUpAlongGround = Vector3.Cross(obstructionRightAlongGround, obstructionNormal).normalized;
                    velocity = GetDirectionTangentToSurface(velocity, obstructionUpAlongGround) * velocity.magnitude;
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
            else
            {
                if (stableOnHit)
                {
                    // Handle stable landing
                    velocity = Vector3.ProjectOnPlane(velocity, parmams.CharacterUp);
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                // Handle generic obstruction
                else
                {
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
        }

        private void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity)
        {
            for (int i = 0; i < parmams.RigidbodyProjectionHitCount; i++)
            {
                RigidbodyProjectionHit bodyHit = parmams.InternalRigidbodyProjectionHits[i];

                if (bodyHit.Rigidbody && !parmams.RigidbodiesPushedThisMove.Contains(bodyHit.Rigidbody))
                {
                    // Remember we hit this rigidbody
                    parmams.RigidbodiesPushedThisMove.Add(bodyHit.Rigidbody);

                    // Calculate the ratio of the total mass that the character mass represents
                    float characterToBodyMassRatio = config.SimulatedCharacterMass / (config.SimulatedCharacterMass + bodyHit.Rigidbody.mass);

                    ComputeCollisionResolutionForHitBody(
                        bodyHit.EffectiveHitNormal,
                        bodyHit.HitVelocity,
                        bodyHit.Rigidbody.velocity,
                        characterToBodyMassRatio,
                        out Vector3 velocityChangeOnCharacter,
                        out Vector3 velocityChangeOnBody);

                    processedVelocity += velocityChangeOnCharacter;

                    bodyHit.Rigidbody.AddForceAtPosition(velocityChangeOnBody, bodyHit.HitPoint, ForceMode.VelocityChange);
                }
            }

        }

        private void ComputeCollisionResolutionForHitBody(Vector3 hitNormal, Vector3 characterVelocity, Vector3 bodyVelocity, float characterToBodyMassRatio, out Vector3 velocityChangeOnCharacter, out Vector3 velocityChangeOnBody)
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

        public void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport stabilityReport)
        {

            Vector3 atCharacterUp = atCharacterRotation * Vector3.up;
            Vector3 innerHitDirection = Vector3.ProjectOnPlane(hitNormal, atCharacterUp).normalized;

            stabilityReport.IsStable = IsStableOnNormal(hitNormal);

            stabilityReport.InnerNormal = hitNormal;
            stabilityReport.OuterNormal = hitNormal;
            float ledgeCheckHeight = config.MaxStepHeight;

            bool isStableLedgeInner = false;
            bool isStableLedgeOuter = false;

            if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * LocomotionControllerConstant.SecondaryProbesVertical) + (innerHitDirection * LocomotionControllerConstant.SecondaryProbesHorizontal),
                    -atCharacterUp,
                    ledgeCheckHeight + LocomotionControllerConstant.SecondaryProbesVertical,
                    out RaycastHit innerLedgeHit,
                    parmams.InternalCharacterHits) > 0)
            {
                Vector3 innerLedgeNormal = innerLedgeHit.normal;
                stabilityReport.InnerNormal = innerLedgeNormal;
                isStableLedgeInner = IsStableOnNormal(innerLedgeNormal);
            }

            if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * LocomotionControllerConstant.SecondaryProbesVertical) + (-innerHitDirection * LocomotionControllerConstant.SecondaryProbesHorizontal),
                    -atCharacterUp,
                    ledgeCheckHeight + LocomotionControllerConstant.SecondaryProbesVertical,
                    out RaycastHit outerLedgeHit,
                    parmams.InternalCharacterHits) > 0)
            {
                Vector3 outerLedgeNormal = outerLedgeHit.normal;
                stabilityReport.OuterNormal = outerLedgeNormal;
                isStableLedgeOuter = IsStableOnNormal(outerLedgeNormal);
            }

            stabilityReport.LedgeDetected = isStableLedgeInner != isStableLedgeOuter;
            if (stabilityReport.LedgeDetected)
            {
                stabilityReport.IsOnEmptySideOfLedge = isStableLedgeOuter && !isStableLedgeInner;
                stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane(hitPoint - (atCharacterPosition + (atCharacterRotation * parmams.CharacterTransformToCapsuleBottom)), atCharacterUp).magnitude;
            }

            if (stabilityReport.IsStable)
            {
                stabilityReport.IsStable = IsStableWithSpecialCases(ref stabilityReport);
            }


            // Step handling
            if (!stabilityReport.IsStable)
            {
                // Stepping not supported on dynamic rigidbodies
                Rigidbody hitRigidbody = hitCollider.attachedRigidbody;
                if (!(hitRigidbody && !hitRigidbody.isKinematic))
                {

                    if (stabilityReport.ValidStepDetected)
                    {
                        stabilityReport.IsStable = true;
                    }
                }
            }
        }

        public int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits)
        {
            int queryLayers = config.StableGroundLayers;
            int nbUnfilteredHits = Physics.RaycastNonAlloc(
                position,
                direction,
                hits,
                distance,
                queryLayers,
                QueryTriggerInteraction.Ignore);

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
                if (hitDistance <= 0f ||
                    !CheckIfColliderValidForCollisions(hit.collider))
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

        private bool CheckIfColliderValidForCollisions(Collider coll)
        {
            if (coll == Capsule)
            {
                return false;
            }

            return IsColliderValid(coll);
        }

        public int CharacterCollisionsOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f)
        {
            int queryLayers = config.StableGroundLayers;

            Vector3 bottom = position + (rotation * parmams.CharacterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * parmams.CharacterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }

            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(
                        bottom,
                        top,
                        Capsule.radius + inflate,
                        overlappedColliders,
                        queryLayers,
                        QueryTriggerInteraction.Ignore);

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

        private Rigidbody GetInteractiveRigidbody(Collider onCollider)
        {
            Rigidbody colliderAttachedRigidbody = onCollider.attachedRigidbody;
            if (colliderAttachedRigidbody)
            {
                if (colliderAttachedRigidbody.gameObject.GetComponent<PhysicsMover>())
                {
                    return colliderAttachedRigidbody;
                }

                if (!colliderAttachedRigidbody.isKinematic)
                {
                    return colliderAttachedRigidbody;
                }
            }
            return null;
        }

        private bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime)
        {
            if (deltaTime <= 0f)
                return false;

            bool wasCompleted = true;
            Vector3 remainingMovementDirection = transientVelocity.normalized;
            float remainingMovementMagnitude = transientVelocity.magnitude * deltaTime;
            int sweepsMade = 0;
            bool hitSomethingThisSweepIteration = true;
            Vector3 tmpMovedPosition = parmams.TargetPosition;
            bool previousHitIsStable = false;
            Vector3 previousVelocity = Vector3.zero;
            Vector3 previousObstructionNormal = Vector3.zero;
            MovementSweepState sweepState = MovementSweepState.Initial;

            // Project movement against current overlaps before doing the sweeps
            for (int i = 0; i < parmams.OverlapsCount; i++)
            {
                Vector3 overlapNormal = parmams.OverlapsNormals[i];
                if (Vector3.Dot(remainingMovementDirection, overlapNormal) < 0f)
                {
                    bool stableOnHit = IsStableOnNormal(overlapNormal);
                    Vector3 velocityBeforeProjection = transientVelocity;
                    Vector3 obstructionNormal = GetObstructionNormal(overlapNormal, stableOnHit);

                    InternalHandleVelocityProjection(
                        stableOnHit,
                        obstructionNormal,
                        ref sweepState,
                        previousHitIsStable,
                        previousVelocity,
                        previousObstructionNormal,
                        ref transientVelocity,
                        ref remainingMovementMagnitude,
                        ref remainingMovementDirection);

                    previousHitIsStable = stableOnHit;
                    previousVelocity = velocityBeforeProjection;
                    previousObstructionNormal = obstructionNormal;
                }
            }

            // Sweep the desired movement to detect collisions
            while (remainingMovementMagnitude > 0f &&
                (sweepsMade <= config.MaxMovementIterations) &&
                hitSomethingThisSweepIteration)
            {
                bool foundClosestHit = false;
                Vector3 closestSweepHitPoint = default;
                Vector3 closestSweepHitNormal = default;
                float closestSweepHitDistance = 0f;
                Collider closestSweepHitCollider = null;


                if (!foundClosestHit && CharacterCollisionsSweep(
                        tmpMovedPosition, // position
                         parmams.TargetRotation, // rotation
                        remainingMovementDirection, // direction
                        remainingMovementMagnitude + LocomotionControllerConstant.CollisionOffset, // distance
                        out RaycastHit closestSweepHit, // closest hit
                         parmams.InternalCharacterHits) // all hits
                    > 0)
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
                    Vector3 sweepMovement = remainingMovementDirection * Mathf.Max(0f, closestSweepHitDistance - LocomotionControllerConstant.CollisionOffset);
                    tmpMovedPosition += sweepMovement;
                    remainingMovementMagnitude -= sweepMovement.magnitude;

                    // Evaluate if hit is stable
                    HitStabilityReport moveHitStabilityReport = new();
                    EvaluateHitStability(closestSweepHitCollider, closestSweepHitNormal, closestSweepHitPoint, tmpMovedPosition, parmams.TargetRotation, ref moveHitStabilityReport);

                    Vector3 obstructionNormal = GetObstructionNormal(closestSweepHitNormal, moveHitStabilityReport.IsStable);

                    // Handle remembering rigidbody hits
                    if (config.InteractiveRigidbodyHandling && closestSweepHitCollider.attachedRigidbody)
                    {
                        StoreRigidbodyHit(
                            closestSweepHitCollider.attachedRigidbody,
                            transientVelocity,
                            closestSweepHitPoint,
                            obstructionNormal);
                    }

                    bool stableOnHit = moveHitStabilityReport.IsStable;
                    Vector3 velocityBeforeProj = transientVelocity;

                    // Project velocity for next iteration
                    InternalHandleVelocityProjection(
                        stableOnHit,
                        obstructionNormal,
                        ref sweepState,
                        previousHitIsStable,
                        previousVelocity,
                        previousObstructionNormal,
                        ref transientVelocity,
                        ref remainingMovementMagnitude,
                        ref remainingMovementDirection);

                    previousHitIsStable = stableOnHit;
                    previousVelocity = velocityBeforeProj;
                    previousObstructionNormal = obstructionNormal;
                }
                // If we hit nothing...
                else
                {
                    hitSomethingThisSweepIteration = false;
                }

                // Safety for exceeding max sweeps allowed
                sweepsMade++;

            }

            // Move position for the remainder of the movement
            tmpMovedPosition += remainingMovementDirection * remainingMovementMagnitude;
            parmams.TargetPosition = tmpMovedPosition;

            return wasCompleted;
        }

        private void InternalHandleVelocityProjection(bool stableOnHit, Vector3 obstructionNormal, ref MovementSweepState sweepState, bool previousHitIsStable, Vector3 previousVelocity, Vector3 previousObstructionNormal, ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
        {
            if (transientVelocity.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 velocityBeforeProjection = transientVelocity;

            if (stableOnHit)
            {
                parmams.LastMovementIterationFoundAnyGround = true;
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
                    EvaluateCrease(
                        transientVelocity,
                        previousVelocity,
                        obstructionNormal,
                        previousObstructionNormal,
                        stableOnHit,
                        previousHitIsStable,
                        GroundingStatus.IsStableOnGround,
                        out bool foundCrease,
                        out Vector3 creaseDirection);

                    if (foundCrease)
                    {
                        if (GroundingStatus.IsStableOnGround)
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

        public int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f)
        {
            int queryLayers = config.StableGroundLayers;


            Vector3 bottom = position + (rotation * parmams.CharacterTransformToCapsuleBottomHemi) - (direction * LocomotionControllerConstant.SweepProbingBackstepDistance);
            Vector3 top = position + (rotation * parmams.CharacterTransformToCapsuleTopHemi) - (direction * LocomotionControllerConstant.SweepProbingBackstepDistance);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }
            bottom.y -= config.CollisionDetectOffset;
            top.y -= config.CollisionDetectOffset;
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                    bottom,
                    top,
                    Capsule.radius + inflate,
                    direction,
                    hits,
                    distance + LocomotionControllerConstant.SweepProbingBackstepDistance,
                    queryLayers,
                    QueryTriggerInteraction.Ignore);

            // Hits filter
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            // Capsule cast
            int nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                hits[i].distance -= LocomotionControllerConstant.SweepProbingBackstepDistance;

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
    }
}
