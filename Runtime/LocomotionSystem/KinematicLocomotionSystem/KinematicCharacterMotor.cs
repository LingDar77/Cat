using System;
using System.Collections.Generic;
using UnityEngine;

namespace TUI.KinematicLocomotionSystem
{


    public enum MovementSweepState
    {
        Initial,
        AfterFirstHit,
        FoundBlockingCrease,
        FoundBlockingCorner,
    }

    /// <summary>
    /// Describes an overlap between the character capsule and another collider
    /// </summary>
    public struct OverlapResult
    {
        public Vector3 Normal;
        public Collider Collider;

        public OverlapResult(Vector3 normal, Collider collider)
        {
            Normal = normal;
            Collider = collider;
        }
    }

    /// <summary>
    /// Contains all the information for the motor's grounding status
    /// </summary>
    public struct CharacterGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public Collider GroundCollider;
        public Vector3 GroundPoint;

    }

    /// <summary>
    /// Contains the simulation-relevant information for the motor's grounding status
    /// </summary>
    public struct CharacterTransientGroundingReport
    {
        public bool FoundAnyGround;
        public bool IsStableOnGround;
        public bool SnappingPrevented;
        public Vector3 GroundNormal;
        public Vector3 InnerGroundNormal;
        public Vector3 OuterGroundNormal;

        public void CopyFrom(CharacterGroundingReport groundingReport)
        {
            FoundAnyGround = groundingReport.FoundAnyGround;
            IsStableOnGround = groundingReport.IsStableOnGround;
            SnappingPrevented = groundingReport.SnappingPrevented;
            GroundNormal = groundingReport.GroundNormal;
            InnerGroundNormal = groundingReport.InnerGroundNormal;
            OuterGroundNormal = groundingReport.OuterGroundNormal;
        }
    }

    /// <summary>
    /// Contains all the information from a hit stability evaluation
    /// </summary>
    public struct HitStabilityReport
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

    /// <summary>
    /// Contains the information of hit rigidbodies during the movement phase, so they can be processed afterwards
    /// </summary>
    public struct RigidbodyProjectionHit
    {
        public Rigidbody Rigidbody;
        public Vector3 HitPoint;
        public Vector3 EffectiveHitNormal;
        public Vector3 HitVelocity;
        public bool StableOnHit;
    }

    /// <summary>
    /// Component that manages character collisions and movement solving
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicCharacterMotor : MonoBehaviour
    {

        public CapsuleCollider Capsule;


        /// <summary>
        /// Maximum slope angle on which the character can be stable
        /// </summary>    
        [Range(0f, 89f)]
        [Tooltip("Maximum slope angle on which the character can be stable")]
        public float MaxStableSlopeAngle = 60f;
        /// <summary>
        /// Which layers can the character be considered stable on
        /// </summary>    
        [Tooltip("Which layers can the character be considered stable on")]
        public LayerMask StableGroundLayers = -1;

        public float CollisionDetectOffset;

        /// <summary>
        /// Maximum height of a step which the character can climb
        /// </summary>    
        [Tooltip("Maximum height of a step which the character can climb")]
        public float MaxStepHeight = 0.5f;

        /// <summary>
        /// Minimum length of a step that the character can step on (used in Extra stepping method. Use this to let the character step on steps that are smaller that its radius
        /// </summary>    
        [Tooltip("Minimum length of a step that the character can step on (used in Extra stepping method). Use this to let the character step on steps that are smaller that its radius")]
        public float MinRequiredStepDepth = 0.1f;

        /// <summary>
        /// The distance from the capsule central axis at which the character can stand on a ledge and still be stable
        /// </summary>    
        [Tooltip("The distance from the capsule central axis at which the character can stand on a ledge and still be stable")]
        public float MaxStableDistanceFromLedge = 0.5f;

        /// <summary>
        /// The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground
        /// </summary>    
        [Tooltip("The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground")]
        [Range(1f, 180f)]
        public float MaxStableDenivelationAngle = 180f;


        [Header("Rigidbody interaction settings")]
        /// <summary>
        /// Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies
        /// </summary>
        [Tooltip("Handles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies")]
        public bool InteractiveRigidbodyHandling = true;

        [Tooltip("Mass used for pushing bodies")]
        public float SimulatedCharacterMass = 1f;

        [Header("Other settings")]
        /// <summary>
        /// How many times can we sweep for movement per update
        /// </summary>
        [Tooltip("How many times can we sweep for movement per update")]
        public int MaxMovementIterations = 5;
        /// <summary>
        /// How many times can we check for decollision per update
        /// </summary>
        [Tooltip("How many times can we check for decollision per update")]
        public int MaxDecollisionIterations = 1;

        /// <summary>
        /// Contains the current grounding information
        /// </summary>
        [System.NonSerialized]
        public CharacterGroundingReport GroundingStatus = new CharacterGroundingReport();
        /// <summary>
        /// Contains the previous grounding information
        /// </summary>
        [System.NonSerialized]
        public CharacterTransientGroundingReport LastGroundingStatus = new();
        /// <summary>
        /// Specifies the LayerMask that the character's movement algorithm can detect collisions with. By default, this uses the rigidbody's layer's collision matrix
        /// </summary>
        [System.NonSerialized]
        public LayerMask CollidableLayers = -1;
        public Vector3 _transientPosition;
        private Vector3 _characterUp;
        private Vector3 _characterTransformToCapsuleBottom;
        private Vector3 _characterTransformToCapsuleBottomHemi;
        private Vector3 _characterTransformToCapsuleTopHemi;
        private int _overlapsCount;
        private OverlapResult[] _overlaps = new OverlapResult[MaxRigidbodyOverlapsCount];

        /// <summary>
        /// The motor's assigned controller
        /// </summary>
        [NonSerialized]
        public ICharacterController CharacterController;
        /// <summary>
        /// Did the motor's last swept collision detection find a ground?
        /// </summary>
        [NonSerialized]
        public bool LastMovementIterationFoundAnyGround;
        /// <summary>
        /// The character's velocity resulting from direct movement
        /// </summary>
        [NonSerialized]
        public Vector3 BaseVelocity;

        // Private
        private RaycastHit[] _internalCharacterHits = new RaycastHit[MaxHitsBudget];
        private Collider[] _internalProbedColliders = new Collider[MaxCollisionBudget];
        private List<Rigidbody> _rigidbodiesPushedThisMove = new List<Rigidbody>(16);
        private RigidbodyProjectionHit[] _internalRigidbodyProjectionHits = new RigidbodyProjectionHit[MaxRigidbodyOverlapsCount];
        private bool _solveMovementCollisions = true;
        private int _rigidbodyProjectionHitCount = 0;
        private Vector3 _cachedWorldUp = Vector3.up;
        private Vector3 _cachedZeroVector = Vector3.zero;

        public Quaternion _transientRotation;

        /// <summary>
        /// The character's goal rotation in its movement calculations (always up-to-date during the character update phase)
        /// </summary>
        public Quaternion TransientRotation
        {
            get
            {
                return _transientRotation;
            }
            private set
            {
                _transientRotation = value;
                _characterUp = _transientRotation * _cachedWorldUp;
            }
        }


        // Warning: Don't touch these constants unless you know exactly what you're doing!
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
        public const float SteppingForwardDistance = 0.03f;
        public const float MinDistanceForLedge = 0.05f;
        public const float CorrelationForVerticalObstruction = 0.01f;

        public Action<float, float, float> OnCapsuleDimensionsUpdated;

        private void OnEnable()
        {
            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.RegisterCharacterMotor(this);
        }

        private void OnDisable()
        {
            KinematicCharacterSystem.UnregisterCharacterMotor(this);
        }

        private void Reset()
        {
            ValidateData();
        }

        private void OnValidate()
        {
            ValidateData();
        }

        /// <summary>
        /// Handle validating all required values
        /// </summary>
        public void ValidateData()
        {
            Capsule = GetComponent<CapsuleCollider>();
            Capsule.radius = Mathf.Clamp(Capsule.radius, 0f, Capsule.height * 0.5f);
            Capsule.direction = 1;
            SetCapsuleDimensions(Capsule.radius, Capsule.height, Capsule.center.y);

            MaxStepHeight = Mathf.Clamp(MaxStepHeight, 0f, Mathf.Infinity);
            MinRequiredStepDepth = Mathf.Clamp(MinRequiredStepDepth, 0f, Capsule.radius);
            MaxStableDistanceFromLedge = Mathf.Clamp(MaxStableDistanceFromLedge, 0f, Capsule.radius);

        }


        /// <summary>
        /// Sets the character's rotation directly
        /// </summary>
        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
            TransientRotation = rotation;
        }

        /// <summary>
        /// Resizes capsule. ALso caches importand capsule size data
        /// </summary>
        public void SetCapsuleDimensions(float radius, float height, float yOffset)
        {
            height = Mathf.Max(height, (radius * 2f) + 0.01f); // Safety to prevent invalid capsule geometries

            Capsule.radius = radius;
            Capsule.height = height;
            Capsule.center = new Vector3(Capsule.center.x, yOffset, Capsule.center.z);

            Capsule.radius = Capsule.radius;
            Capsule.height = Mathf.Clamp(Capsule.height, Capsule.radius * 2f, Capsule.height);
            Capsule.center = new Vector3(0f, Capsule.center.y, 0f);

            _characterTransformToCapsuleBottom = Capsule.center + (-_cachedWorldUp * (Capsule.height * 0.5f));
            _characterTransformToCapsuleBottomHemi = Capsule.center + (-_cachedWorldUp * (Capsule.height * 0.5f)) + (_cachedWorldUp * Capsule.radius);
            _characterTransformToCapsuleTopHemi = Capsule.center + (_cachedWorldUp * (Capsule.height * 0.5f)) + (-_cachedWorldUp * Capsule.radius);

            OnCapsuleDimensionsUpdated?.Invoke(Capsule.radius, Capsule.height, Capsule.center.y);
        }

        private void Awake()
        {
            ValidateData();

            _transientPosition = transform.position;
            TransientRotation = transform.rotation;

            // Build CollidableLayers mask
            CollidableLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(gameObject.layer, i))
                {
                    CollidableLayers |= 1 << i;
                }
            }

            SetCapsuleDimensions(Capsule.radius, Capsule.height, Capsule.center.y);
        }

        /// <summary>
        /// Update phase 1 is meant to be called after physics movers have calculated their velocities, but
        /// before they have simulated their goal positions/rotations. It is responsible for:
        /// - Initializing all values for update
        /// - Handling MovePosition calls
        /// - Solving initial collision overlaps
        /// - Ground probing
        /// - Handle detecting potential interactable rigidbodies
        /// </summary>
        public void UpdatePhase1(float deltaTime)
        {

            _rigidbodiesPushedThisMove.Clear();

            // Before update
            CharacterController.BeforeCharacterUpdate(deltaTime);

            _transientPosition = transform.position;
            TransientRotation = transform.rotation;
            _rigidbodyProjectionHitCount = 0;
            _overlapsCount = 0;

            LastGroundingStatus.CopyFrom(GroundingStatus);
            GroundingStatus = new()
            {
                GroundNormal = _characterUp
            };

            #region Ground Probing and Snapping

            // Choose the appropriate ground probing distance
            float selectedGroundProbingDistance = MinimumGroundProbingDistance;
            if (!LastGroundingStatus.SnappingPrevented && (LastGroundingStatus.IsStableOnGround || LastMovementIterationFoundAnyGround))
            {
                selectedGroundProbingDistance = Mathf.Max(Capsule.radius, MaxStepHeight);


            }

            ProbeGround(ref _transientPosition, _transientRotation, selectedGroundProbingDistance, ref GroundingStatus);

            if (!LastGroundingStatus.IsStableOnGround && GroundingStatus.IsStableOnGround)
            {
                // Handle stable landing
                BaseVelocity = Vector3.ProjectOnPlane(BaseVelocity, _characterUp);
                BaseVelocity = GetDirectionTangentToSurface(BaseVelocity, GroundingStatus.GroundNormal) * BaseVelocity.magnitude;
            }

            LastMovementIterationFoundAnyGround = false;

            #endregion

            CharacterController.PostGroundingUpdate(deltaTime);


        }

        /// <summary>
        /// Update phase 2 is meant to be called after physics movers have simulated their goal positions/rotations. 
        /// At the end of this, the TransientPosition/Rotation values will be up-to-date with where the motor should be at the end of its move. 
        /// It is responsible for:
        /// - Solving Rotation
        /// - Handle MoveRotation calls
        /// - Solving potential attached rigidbody overlaps
        /// - Solving Velocity
        /// - Applying planar constraint
        /// </summary>
        public void UpdatePhase2(float deltaTime)
        {
            // Handle rotation
            CharacterController.UpdateRotation(ref _transientRotation, deltaTime);
            TransientRotation = _transientRotation;


            if (_solveMovementCollisions && InteractiveRigidbodyHandling)
            {
                if (InteractiveRigidbodyHandling)
                {

                    #region Resolve overlaps that could've been caused by rotation or physics movers simulation pushing the character
                    int iterationsMade = 0;
                    bool overlapSolved = false;
                    while (iterationsMade < MaxDecollisionIterations && !overlapSolved)
                    {
                        int nbOverlaps = CharacterCollisionsOverlap(_transientPosition, _transientRotation, _internalProbedColliders);
                        if (nbOverlaps > 0)
                        {
                            for (int i = 0; i < nbOverlaps; i++)
                            {
                                // Process overlap
                                Transform overlappedTransform = _internalProbedColliders[i].GetComponent<Transform>();
                                Vector3 resolutionDirection;
                                float resolutionDistance;
                                if (Physics.ComputePenetration(
                                        Capsule,
                                        _transientPosition,
                                        _transientRotation,
                                        _internalProbedColliders[i],
                                        overlappedTransform.position,
                                        overlappedTransform.rotation,
                                        out resolutionDirection,
                                        out resolutionDistance))
                                {
                                    // Resolve along obstruction direction
                                    HitStabilityReport mockReport = new HitStabilityReport();
                                    mockReport.IsStable = IsStableOnNormal(resolutionDirection);
                                    resolutionDirection = GetObstructionNormal(resolutionDirection, mockReport.IsStable);

                                    // Solve overlap
                                    Vector3 resolutionMovement = resolutionDirection * (resolutionDistance + CollisionOffset);
                                    _transientPosition += resolutionMovement;

                                    // If interactiveRigidbody, register as rigidbody hit for velocity
                                    if (InteractiveRigidbodyHandling)
                                    {
                                        Rigidbody probedRigidbody = GetInteractiveRigidbody(_internalProbedColliders[i]);
                                        if (probedRigidbody != null)
                                        {
                                            HitStabilityReport tmpReport = new()
                                            {
                                                IsStable = IsStableOnNormal(resolutionDirection)
                                            };
                                            if (tmpReport.IsStable)
                                            {
                                                LastMovementIterationFoundAnyGround = tmpReport.IsStable;
                                            }
                                            Vector3 estimatedCollisionPoint = _transientPosition;

                                            StoreRigidbodyHit(
                                                probedRigidbody,
                                                BaseVelocity,
                                                estimatedCollisionPoint,
                                                resolutionDirection,
                                                tmpReport);
                                        }
                                    }

                                    // Remember overlaps
                                    if (_overlapsCount < _overlaps.Length)
                                    {
                                        _overlaps[_overlapsCount] = new OverlapResult(resolutionDirection, _internalProbedColliders[i]);
                                        _overlapsCount++;
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
            }

            // Handle velocity
            CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);

            //this.CharacterController.UpdateVelocity(ref BaseVelocity, deltaTime);
            if (BaseVelocity.magnitude < MinVelocityMagnitude)
            {
                BaseVelocity = Vector3.zero;
            }

            #region Calculate Character movement from base velocity   
            // Perform the move from base velocity
            if (BaseVelocity.sqrMagnitude > 0f)
            {
                if (_solveMovementCollisions)
                {
                    InternalCharacterMove(ref BaseVelocity, deltaTime);
                }
                else
                {
                    _transientPosition += BaseVelocity * deltaTime;
                }
            }

            // Process rigidbody hits/overlaps to affect velocity
            if (InteractiveRigidbodyHandling)
            {
                ProcessVelocityForRigidbodyHits(ref BaseVelocity, deltaTime);
            }
            #endregion


            CharacterController.AfterCharacterUpdate(deltaTime);
        }

        /// <summary>
        /// Determines if motor can be considered stable on given slope normal
        /// </summary>
        private bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(_characterUp, normal) <= MaxStableSlopeAngle;
        }

        /// <summary>
        /// Determines if motor can be considered stable on given slope normal
        /// </summary>
        private bool IsStableWithSpecialCases(ref HitStabilityReport stabilityReport, Vector3 velocity)
        {

            if (stabilityReport.LedgeDetected)
            {

                // Distance from ledge
                if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
                {
                    return false;
                }
            }

            // "Launching" off of slopes of a certain denivelation angle
            if (LastGroundingStatus.FoundAnyGround && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
            {
                float denivelationAngle = Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal);
                if (denivelationAngle > MaxStableDenivelationAngle)
                {
                    return false;
                }
                else
                {
                    denivelationAngle = Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal);
                    if (denivelationAngle > MaxStableDenivelationAngle)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Probes for valid ground and midifies the input transientPosition if ground snapping occurs
        /// </summary>
        public void ProbeGround(ref Vector3 probingPosition, Quaternion atRotation, float probingDistance, ref CharacterGroundingReport groundingReport)
        {
            if (probingDistance < MinimumGroundProbingDistance)
            {
                probingDistance = MinimumGroundProbingDistance;
            }

            int groundSweepsMade = 0;
            RaycastHit groundSweepHit = new();
            bool groundSweepingIsOver = false;
            Vector3 groundSweepPosition = probingPosition;
            Vector3 groundSweepDirection = atRotation * -_cachedWorldUp;
            float groundProbeDistanceRemaining = probingDistance;
            while (groundProbeDistanceRemaining > 0 && (groundSweepsMade <= MaxGroundingSweepIterations) && !groundSweepingIsOver)
            {
                // Sweep for ground detection
                if (CharacterGroundSweep(
                        groundSweepPosition, // position
                        atRotation, // rotation
                        groundSweepDirection, // direction
                        groundProbeDistanceRemaining, // distance
                        out groundSweepHit)) // hit
                {
                    Vector3 targetPosition = groundSweepPosition + (groundSweepDirection * groundSweepHit.distance);
                    HitStabilityReport groundHitStabilityReport = new();
                    EvaluateHitStability(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, targetPosition, _transientRotation, BaseVelocity, ref groundHitStabilityReport);

                    groundingReport.FoundAnyGround = true;
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
                        groundingReport.SnappingPrevented = !IsStableWithSpecialCases(ref groundHitStabilityReport, BaseVelocity);

                        groundingReport.IsStableOnGround = true;

                        // Ground snapping
                        if (!groundingReport.SnappingPrevented)
                        {
                            probingPosition = groundSweepPosition + (groundSweepDirection * (groundSweepHit.distance - CollisionOffset));
                        }

                        CharacterController.OnGroundHit(groundSweepHit.collider, groundSweepHit.normal, groundSweepHit.point, ref groundHitStabilityReport);
                        groundSweepingIsOver = true;
                    }
                    else
                    {
                        // Calculate movement from this iteration and advance position
                        Vector3 sweepMovement = (groundSweepDirection * groundSweepHit.distance) + (atRotation * _cachedWorldUp * Mathf.Max(CollisionOffset, groundSweepHit.distance));
                        groundSweepPosition += sweepMovement;

                        // Set remaining distance
                        groundProbeDistanceRemaining = Mathf.Min(GroundProbeReboundDistance, Mathf.Max(groundProbeDistanceRemaining - sweepMovement.magnitude, 0f));

                        // Reorient direction
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


        /// <summary>
        /// Returns the direction adjusted to be tangent to a specified surface normal relatively to the character's up direction.
        /// Useful for reorienting a direction on a slope without any lateral deviation in trajectory
        /// </summary>
        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, _characterUp);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        /// <summary>
        /// Moves the character's position by given movement while taking into account all physics simulation, step-handling and 
        /// velocity projection rules that affect the character motor
        /// </summary>
        /// <returns> Returns false if movement could not be solved until the end </returns>
        private bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime)
        {
            if (deltaTime <= 0f)
                return false;

            bool wasCompleted = true;
            Vector3 remainingMovementDirection = transientVelocity.normalized;
            float remainingMovementMagnitude = transientVelocity.magnitude * deltaTime;
            Vector3 originalVelocityDirection = remainingMovementDirection;
            int sweepsMade = 0;
            bool hitSomethingThisSweepIteration = true;
            Vector3 tmpMovedPosition = _transientPosition;
            bool previousHitIsStable = false;
            Vector3 previousVelocity = _cachedZeroVector;
            Vector3 previousObstructionNormal = _cachedZeroVector;
            MovementSweepState sweepState = MovementSweepState.Initial;

            // Project movement against current overlaps before doing the sweeps
            for (int i = 0; i < _overlapsCount; i++)
            {
                Vector3 overlapNormal = _overlaps[i].Normal;
                if (Vector3.Dot(remainingMovementDirection, overlapNormal) < 0f)
                {
                    bool stableOnHit = IsStableOnNormal(overlapNormal);
                    Vector3 velocityBeforeProjection = transientVelocity;
                    Vector3 obstructionNormal = GetObstructionNormal(overlapNormal, stableOnHit);

                    InternalHandleVelocityProjection(
                        stableOnHit,
                        overlapNormal,
                        obstructionNormal,
                        originalVelocityDirection,
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
                (sweepsMade <= MaxMovementIterations) &&
                hitSomethingThisSweepIteration)
            {
                bool foundClosestHit = false;
                Vector3 closestSweepHitPoint = default;
                Vector3 closestSweepHitNormal = default;
                float closestSweepHitDistance = 0f;
                Collider closestSweepHitCollider = null;


                if (!foundClosestHit && CharacterCollisionsSweep(
                        tmpMovedPosition, // position
                        _transientRotation, // rotation
                        remainingMovementDirection, // direction
                        remainingMovementMagnitude + CollisionOffset, // distance
                        out RaycastHit closestSweepHit, // closest hit
                        _internalCharacterHits) // all hits
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
                    Vector3 sweepMovement = remainingMovementDirection * Mathf.Max(0f, closestSweepHitDistance - CollisionOffset);
                    tmpMovedPosition += sweepMovement;
                    remainingMovementMagnitude -= sweepMovement.magnitude;

                    // Evaluate if hit is stable
                    HitStabilityReport moveHitStabilityReport = new HitStabilityReport();
                    EvaluateHitStability(closestSweepHitCollider, closestSweepHitNormal, closestSweepHitPoint, tmpMovedPosition, _transientRotation, transientVelocity, ref moveHitStabilityReport);

                    // Handle stepping up steps points higher than bottom capsule radius
                    bool foundValidStepHit = false;
                    if (moveHitStabilityReport.ValidStepDetected)
                    {
                        float obstructionCorrelation = Mathf.Abs(Vector3.Dot(closestSweepHitNormal, _characterUp));
                        if (obstructionCorrelation <= CorrelationForVerticalObstruction)
                        {
                            Vector3 stepForwardDirection = Vector3.ProjectOnPlane(-closestSweepHitNormal, _characterUp).normalized;
                            Vector3 stepCastStartPoint = tmpMovedPosition + (stepForwardDirection * SteppingForwardDistance) +
                                (_characterUp * MaxStepHeight);

                            // Cast downward from the top of the stepping height
                            int nbStepHits = CharacterCollisionsSweep(
                                                stepCastStartPoint, // position
                                                _transientRotation, // rotation
                                                -_characterUp, // direction
                                                MaxStepHeight, // distance
                                                out RaycastHit closestStepHit, // closest hit
                                                _internalCharacterHits,
                                                0f,
                                                true); // all hits 

                            // Check for hit corresponding to stepped collider
                            for (int i = 0; i < nbStepHits; i++)
                            {
                                if (_internalCharacterHits[i].collider == moveHitStabilityReport.SteppedCollider)
                                {
                                    Vector3 endStepPosition = stepCastStartPoint + (-_characterUp * (_internalCharacterHits[i].distance - CollisionOffset));
                                    tmpMovedPosition = endStepPosition;
                                    foundValidStepHit = true;

                                    // Project velocity on ground normal at step
                                    transientVelocity = Vector3.ProjectOnPlane(transientVelocity, _characterUp);
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

                        // Movement hit callback
                        CharacterController.OnMovementHit(closestSweepHitCollider, closestSweepHitNormal, closestSweepHitPoint, ref moveHitStabilityReport);

                        // Handle remembering rigidbody hits
                        if (InteractiveRigidbodyHandling && closestSweepHitCollider.attachedRigidbody)
                        {
                            StoreRigidbodyHit(
                                closestSweepHitCollider.attachedRigidbody,
                                transientVelocity,
                                closestSweepHitPoint,
                                obstructionNormal,
                                moveHitStabilityReport);
                        }

                        bool stableOnHit = moveHitStabilityReport.IsStable;
                        Vector3 velocityBeforeProj = transientVelocity;

                        // Project velocity for next iteration
                        InternalHandleVelocityProjection(
                            stableOnHit,
                            closestSweepHitNormal,
                            obstructionNormal,
                            originalVelocityDirection,
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
            tmpMovedPosition += (remainingMovementDirection * remainingMovementMagnitude);
            _transientPosition = tmpMovedPosition;

            return wasCompleted;
        }

        /// <summary>
        /// Gets the effective normal for movement obstruction depending on current grounding status
        /// </summary>
        private Vector3 GetObstructionNormal(Vector3 hitNormal, bool stableOnHit)
        {
            // Find hit/obstruction/offset normal
            Vector3 obstructionNormal = hitNormal;
            if (GroundingStatus.IsStableOnGround && !stableOnHit)
            {
                Vector3 obstructionLeftAlongGround = Vector3.Cross(GroundingStatus.GroundNormal, obstructionNormal).normalized;
                obstructionNormal = Vector3.Cross(obstructionLeftAlongGround, _characterUp).normalized;
            }

            // Catch cases where cross product between parallel normals returned 0
            if (obstructionNormal.sqrMagnitude == 0f)
            {
                obstructionNormal = hitNormal;
            }

            return obstructionNormal;
        }

        /// <summary>
        /// Remembers a rigidbody hit for processing later
        /// </summary>
        private void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal, HitStabilityReport hitStabilityReport)
        {
            if (_rigidbodyProjectionHitCount < _internalRigidbodyProjectionHits.Length)
            {
                if (!hitRigidbody.GetComponent<KinematicCharacterMotor>())
                {
                    RigidbodyProjectionHit rph = new()
                    {
                        Rigidbody = hitRigidbody,
                        HitPoint = hitPoint,
                        EffectiveHitNormal = obstructionNormal,
                        HitVelocity = hitVelocity,
                        StableOnHit = hitStabilityReport.IsStable
                    };

                    _internalRigidbodyProjectionHits[_rigidbodyProjectionHitCount] = rph;
                    _rigidbodyProjectionHitCount++;
                }
            }
        }

        /// <summary>
        /// Processes movement projection upon detecting a hit
        /// </summary>
        private void InternalHandleVelocityProjection(bool stableOnHit, Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalDirection,
            ref MovementSweepState sweepState, bool previousHitIsStable, Vector3 previousVelocity, Vector3 previousObstructionNormal,
            ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
        {
            if (transientVelocity.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 velocityBeforeProjection = transientVelocity;

            if (stableOnHit)
            {
                LastMovementIterationFoundAnyGround = true;
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

        private void EvaluateCrease(
            Vector3 currentCharacterVelocity,
            Vector3 previousCharacterVelocity,
            Vector3 currentHitNormal,
            Vector3 previousHitNormal,
            bool currentHitIsStable,
            bool previousHitIsStable,
            bool characterIsStable,
            out bool isValidCrease,
            out Vector3 creaseDirection)
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

        /// <summary>
        /// Allows you to override the way velocity is projected on an obstruction
        /// </summary>
        public virtual void HandleVelocityProjection(ref Vector3 velocity, Vector3 obstructionNormal, bool stableOnHit)
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
                    velocity = Vector3.ProjectOnPlane(velocity, _characterUp);
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                // Handle generic obstruction
                else
                {
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
        }

        /// <summary>
        /// Takes into account rigidbody hits for adding to the velocity
        /// </summary>
        private void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
        {
            for (int i = 0; i < _rigidbodyProjectionHitCount; i++)
            {
                RigidbodyProjectionHit bodyHit = _internalRigidbodyProjectionHits[i];

                if (bodyHit.Rigidbody && CharacterController.IsRigidbodyValidForCollisions(bodyHit.Rigidbody) && !_rigidbodiesPushedThisMove.Contains(bodyHit.Rigidbody))
                {
                    // Remember we hit this rigidbody
                    _rigidbodiesPushedThisMove.Add(bodyHit.Rigidbody);

                    float characterMass = SimulatedCharacterMass;
                    Vector3 characterVelocity = bodyHit.HitVelocity;

                    KinematicCharacterMotor hitCharacterMotor = bodyHit.Rigidbody.GetComponent<KinematicCharacterMotor>();
                    bool hitBodyIsCharacter = hitCharacterMotor != null;
                    bool hitBodyIsDynamic = !bodyHit.Rigidbody.isKinematic;
                    float hitBodyMassAtPoint = bodyHit.Rigidbody.mass; // todo
                    Vector3 hitBodyVelocity = bodyHit.Rigidbody.velocity;
                    if (hitBodyIsCharacter)
                    {
                        hitBodyMassAtPoint = hitCharacterMotor.SimulatedCharacterMass; // todo
                        hitBodyVelocity = hitCharacterMotor.BaseVelocity;
                    }
                    else if (!hitBodyIsDynamic)
                    {
                        PhysicsMover physicsMover = bodyHit.Rigidbody.GetComponent<PhysicsMover>();
                        if (physicsMover)
                        {
                            hitBodyVelocity = physicsMover.Velocity;
                        }
                    }

                    // Calculate the ratio of the total mass that the character mass represents
                    float characterToBodyMassRatio;
                    {
                        if (characterMass + hitBodyMassAtPoint > 0f)
                        {
                            characterToBodyMassRatio = characterMass / (characterMass + hitBodyMassAtPoint);
                        }
                        else
                        {
                            characterToBodyMassRatio = 0.5f;
                        }

                        // Hitting a non-dynamic body
                        if (!hitBodyIsDynamic)
                        {
                            characterToBodyMassRatio = 0f;
                        }

                    }

                    ComputeCollisionResolutionForHitBody(
                        bodyHit.EffectiveHitNormal,
                        characterVelocity,
                        hitBodyVelocity,
                        characterToBodyMassRatio,
                        out Vector3 velocityChangeOnCharacter,
                        out Vector3 velocityChangeOnBody);

                    processedVelocity += velocityChangeOnCharacter;

                    if (hitBodyIsCharacter)
                    {
                        hitCharacterMotor.BaseVelocity += velocityChangeOnCharacter;
                    }
                    else if (hitBodyIsDynamic)
                    {
                        bodyHit.Rigidbody.AddForceAtPosition(velocityChangeOnBody, bodyHit.HitPoint, ForceMode.VelocityChange);
                    }


                }
            }

        }

        public void ComputeCollisionResolutionForHitBody(
            Vector3 hitNormal,
            Vector3 characterVelocity,
            Vector3 bodyVelocity,
            float characterToBodyMassRatio,
            out Vector3 velocityChangeOnCharacter,
            out Vector3 velocityChangeOnBody)
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

        /// <summary>
        /// Determines if the input collider is valid for collision processing
        /// </summary>
        /// <returns> Returns true if the collider is valid </returns>
        private bool CheckIfColliderValidForCollisions(Collider coll)
        {
            // Ignore self
            if (coll == Capsule)
            {
                return false;
            }

            return CharacterController.IsColliderValidForCollisions(coll);
        }


        /// <summary>
        /// Determines if the motor is considered stable on a given hit
        /// </summary>
        public void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, Vector3 withCharacterVelocity, ref HitStabilityReport stabilityReport)
        {

            Vector3 atCharacterUp = atCharacterRotation * _cachedWorldUp;
            Vector3 innerHitDirection = Vector3.ProjectOnPlane(hitNormal, atCharacterUp).normalized;

            stabilityReport.IsStable = this.IsStableOnNormal(hitNormal);

            stabilityReport.FoundInnerNormal = false;
            stabilityReport.FoundOuterNormal = false;
            stabilityReport.InnerNormal = hitNormal;
            stabilityReport.OuterNormal = hitNormal;


            float ledgeCheckHeight = MinDistanceForLedge;
            ledgeCheckHeight = MaxStepHeight;

            bool isStableLedgeInner = false;
            bool isStableLedgeOuter = false;

            if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * SecondaryProbesVertical) + (innerHitDirection * SecondaryProbesHorizontal),
                    -atCharacterUp,
                    ledgeCheckHeight + SecondaryProbesVertical,
                    out RaycastHit innerLedgeHit,
                    _internalCharacterHits) > 0)
            {
                Vector3 innerLedgeNormal = innerLedgeHit.normal;
                stabilityReport.InnerNormal = innerLedgeNormal;
                stabilityReport.FoundInnerNormal = true;
                isStableLedgeInner = IsStableOnNormal(innerLedgeNormal);
            }

            if (CharacterCollisionsRaycast(
                    hitPoint + (atCharacterUp * SecondaryProbesVertical) + (-innerHitDirection * SecondaryProbesHorizontal),
                    -atCharacterUp,
                    ledgeCheckHeight + SecondaryProbesVertical,
                    out RaycastHit outerLedgeHit,
                    _internalCharacterHits) > 0)
            {
                Vector3 outerLedgeNormal = outerLedgeHit.normal;
                stabilityReport.OuterNormal = outerLedgeNormal;
                stabilityReport.FoundOuterNormal = true;
                isStableLedgeOuter = IsStableOnNormal(outerLedgeNormal);
            }

            stabilityReport.LedgeDetected = isStableLedgeInner != isStableLedgeOuter;
            if (stabilityReport.LedgeDetected)
            {
                stabilityReport.IsOnEmptySideOfLedge = isStableLedgeOuter && !isStableLedgeInner;
                stabilityReport.LedgeGroundNormal = isStableLedgeOuter ? stabilityReport.OuterNormal : stabilityReport.InnerNormal;
                stabilityReport.LedgeRightDirection = Vector3.Cross(hitNormal, stabilityReport.LedgeGroundNormal).normalized;
                stabilityReport.LedgeFacingDirection = Vector3.ProjectOnPlane(Vector3.Cross(stabilityReport.LedgeGroundNormal, stabilityReport.LedgeRightDirection), _characterUp).normalized;
                stabilityReport.DistanceFromLedge = Vector3.ProjectOnPlane(hitPoint - (atCharacterPosition + (atCharacterRotation * _characterTransformToCapsuleBottom)), atCharacterUp).magnitude;
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

            CharacterController.ProcessHitStabilityReport(hitCollider, hitNormal, hitPoint, atCharacterPosition, atCharacterRotation, ref stabilityReport);
        }

        private void DetectSteps(Vector3 characterPosition, Quaternion characterRotation, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStabilityReport stabilityReport)
        {
            Vector3 characterUp = characterRotation * _cachedWorldUp;
            Vector3 verticalCharToHit = Vector3.Project(hitPoint - characterPosition, characterUp);
            Vector3 horizontalCharToHitDirection = Vector3.ProjectOnPlane((hitPoint - characterPosition), characterUp).normalized;
            Vector3 stepCheckStartPos = hitPoint - verticalCharToHit + (characterUp * MaxStepHeight) + (3f * CollisionOffset * horizontalCharToHitDirection);

            // Do outer step check with capsule cast on hit point
            int nbStepHits = CharacterCollisionsSweep(
                            stepCheckStartPos,
                            characterRotation,
                            -characterUp,
                            MaxStepHeight + CollisionOffset,
                            out RaycastHit outerStepHit,
                            _internalCharacterHits,
                            0f,
                            true);

            // Check for overlaps and obstructions at the hit position
            if (CheckStepValidity(nbStepHits, characterPosition, characterRotation, innerHitDirection, stepCheckStartPos, out Collider tmpCollider))
            {
                stabilityReport.ValidStepDetected = true;
                stabilityReport.SteppedCollider = tmpCollider;
            }


        }

        private bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Quaternion characterRotation, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider)
        {
            hitCollider = null;
            Vector3 characterUp = characterRotation * Vector3.up;


            while (nbStepHits > 0)
            {
                // Get farthest hit among the remaining hits
                RaycastHit farthestHit = new();
                float farthestDistance = 0f;
                for (int i = 0; i < nbStepHits; i++)
                {
                    float hitDistance = _internalCharacterHits[i].distance;
                    if (hitDistance > farthestDistance)
                    {
                        farthestDistance = hitDistance;
                        farthestHit = _internalCharacterHits[i];
                    }
                }

                Vector3 characterPositionAtHit = stepCheckStartPos + (-characterUp * (farthestHit.distance - CollisionOffset));

                int atStepOverlaps = CharacterCollisionsOverlap(characterPositionAtHit, characterRotation, _internalProbedColliders);
                if (atStepOverlaps <= 0)
                {
                    // Check for outer hit slope normal stability at the step position
                    if (CharacterCollisionsRaycast(
                            farthestHit.point + (characterUp * SecondaryProbesVertical) + (-innerHitDirection * SecondaryProbesHorizontal),
                            -characterUp,
                            MaxStepHeight + SecondaryProbesVertical,
                            out RaycastHit outerSlopeHit,
                            _internalCharacterHits,
                            true) > 0)
                    {
                        if (IsStableOnNormal(outerSlopeHit.normal))
                        {
                            // Cast upward to detect any obstructions to moving there
                            if (CharacterCollisionsSweep(
                                                characterPosition, // position
                                                characterRotation, // rotation
                                                characterUp, // direction
                                                MaxStepHeight - farthestHit.distance, // distance
                                                out RaycastHit tmpUpObstructionHit, // closest hit
                                                _internalCharacterHits) // all hits
                                    <= 0)
                            {

                            }
                        }
                    }
                }

                --nbStepHits;
            }

            return false;
        }

        /// <summary>
        /// Determines if a collider has an attached interactive rigidbody
        /// </summary>
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
        /// <summary>
        /// Detect if the character capsule is overlapping with anything collidable
        /// </summary>
        /// <returns> Returns number of overlaps </returns>
        public int CharacterCollisionsOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            int queryLayers = CollidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                queryLayers = CollidableLayers & StableGroundLayers;
            }

            Vector3 bottom = position + (rotation * _characterTransformToCapsuleBottomHemi);
            Vector3 top = position + (rotation * _characterTransformToCapsuleTopHemi);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }

            int nbHits = 0;
            int nbUnfilteredHits = Physics.OverlapCapsuleNonAlloc(
                        bottom,
                        top,
                        Capsule.radius + inflate,
                        overlappedColliders,
                        queryLayers,
                        QueryTriggerInteraction.Ignore);

            // Filter out invalid colliders
            nbHits = nbUnfilteredHits;
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

        /// <summary>
        /// Sweeps the capsule's volume to detect collision hits
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        public int CharacterCollisionsSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, float inflate = 0f, bool acceptOnlyStableGroundLayer = false)
        {
            int queryLayers = CollidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                queryLayers = CollidableLayers & StableGroundLayers;
            }

            Vector3 bottom = position + (rotation * _characterTransformToCapsuleBottomHemi) - (direction * SweepProbingBackstepDistance);
            Vector3 top = position + (rotation * _characterTransformToCapsuleTopHemi) - (direction * SweepProbingBackstepDistance);
            if (inflate != 0f)
            {
                bottom += rotation * Vector3.down * inflate;
                top += rotation * Vector3.up * inflate;
            }
            bottom.y -= CollisionDetectOffset;
            top.y -= CollisionDetectOffset;
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                    bottom,
                    top,
                    Capsule.radius + inflate,
                    direction,
                    hits,
                    distance + SweepProbingBackstepDistance,
                    queryLayers,
                    QueryTriggerInteraction.Ignore);

            // Hits filter
            closestHit = new RaycastHit();
            float closestDistance = Mathf.Infinity;
            // Capsule cast
            int nbHits = nbUnfilteredHits;
            for (int i = nbUnfilteredHits - 1; i >= 0; i--)
            {
                hits[i].distance -= SweepProbingBackstepDistance;

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

        /// <summary>
        /// Casts the character volume in the character's downward direction to detect ground
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        private bool CharacterGroundSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = new RaycastHit();
            position.y -= CollisionDetectOffset;
            // Capsule cast
            int nbUnfilteredHits = Physics.CapsuleCastNonAlloc(
                position + (rotation * _characterTransformToCapsuleBottomHemi) - (direction * GroundProbingBackstepDistance),
                position + (rotation * _characterTransformToCapsuleTopHemi) - (direction * GroundProbingBackstepDistance),
                Capsule.radius,
                direction,
                _internalCharacterHits,
                distance + GroundProbingBackstepDistance,
                CollidableLayers & StableGroundLayers,
                QueryTriggerInteraction.Ignore);

            // Hits filter
            bool foundValidHit = false;
            float closestDistance = Mathf.Infinity;
            for (int i = 0; i < nbUnfilteredHits; i++)
            {
                RaycastHit hit = _internalCharacterHits[i];
                float hitDistance = hit.distance;

                // Find the closest valid hit
                if (hitDistance > 0f && CheckIfColliderValidForCollisions(hit.collider))
                {
                    if (hitDistance < closestDistance)
                    {
                        closestHit = hit;
                        closestHit.distance -= GroundProbingBackstepDistance;
                        closestDistance = hitDistance;

                        foundValidHit = true;
                    }
                }
            }

            return foundValidHit;
        }

        /// <summary>
        /// Raycasts to detect collision hits
        /// </summary>
        /// <returns> Returns the number of hits </returns>
        public int CharacterCollisionsRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closestHit, RaycastHit[] hits, bool acceptOnlyStableGroundLayer = false)
        {
            int queryLayers = CollidableLayers;
            if (acceptOnlyStableGroundLayer)
            {
                queryLayers = CollidableLayers & StableGroundLayers;
            }

            // Raycast
            int nbHits = 0;
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
            nbHits = nbUnfilteredHits;
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
    }
}