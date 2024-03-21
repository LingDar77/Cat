namespace Cat.LocomotionSystem
{
    using System.Collections.Generic;
    using UnityEngine;
    using Cat.Utilities;
    using dnlib.DotNet.Emit;

    public enum CharacterActorState
    {
        NotGrounded,
        StableGrounded,
        UnstableGrounded
    }

    /// <summary>
    /// This class represents a character actor. It contains all the character information, collision flags, collision events, and so on. It also responsible for the execution order 
    /// of everything related to the character, such as movement, rotation, teleportation, rigidbodies interactions, body size, etc. Since the character can be 2D or 3D, this abstract class must be implemented in the 
    /// two formats, one for 2D and one for 3D.
    /// </summary>
    [RequireComponent(typeof(LocomotionShape))]
    [DefaultExecutionOrder(10)]
    public class BuiltinLocomotionSystem1 : LocomotionSystemBase1, ILocomotionSystem
    {

        public Vector3 CurrentVelocity => Velocity;
        public Quaternion CurrentRotation => Rotation;
        protected readonly List<IActionProvider> actions = new();

        private void Update()
        {
            var vel = CurrentVelocity;
            var rot = Rotation;
            var dt = Time.deltaTime;

            foreach (var action in actions)
            {
                action.BeforeProcess(dt);
            }

            foreach (var action in actions)
            {
                action.ProcessVelocity(ref vel, dt);
            }

            foreach (var action in actions)
            {
                action.ProcessRotation(ref rot, dt);
            }

            foreach (var action in actions)
            {
                action.AfterProcess(dt);
            }

            Rotation = rot;
            LocalVelocity = vel;
        }

        public void RegisterActionProvider(IActionProvider action)
        {
            actions.Add(action);
        }

        public void UnregisterActionProvider(IActionProvider action)
        {
            actions.Remove(action);
        }
        bool ILocomotionSystem.IsStable()
        {
            return IsStable;
        }
        public bool IsOnGround()
        {
            return CurrentState == CharacterActorState.StableGrounded;
        }

        public void MarkUngrounded()
        {
        }

        public bool IsColliderValid(Collider collider)
        {
            return true;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;

        }

        public void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;

        }

        public float GetMaxiumStableAngle()
        {
            return slopeLimit;
        }

        public Vector3 GetGroundNormal()
        {
            return GroundContactNormal;
        }

        public Collider GetGrandCollider()
        {
            return GroundRigidbody3D.GetComponent<Collider>();
        }

        [Header("Stability")]

        [Tooltip("If the character is stable, the ground slope angle must be less than or equal to this value in order to remain \"stable\". " +
        "The angle is calculated using the \"ground stable normal\".")]
        [Range(1f, 89f)]
        public float slopeLimit = 55f;

        [Tooltip("Objects NOT represented by this layer mask will be considered by the character as \"unstable objects\". " +
        "If you don't want to define unstable layers, select \"Everything\" (default value).")]
        public LayerMask stableLayerMask = -1;

        [Tooltip("Whether or not to allow other characters to be considered as stable surfaces.")]
        public bool allowCharactersAsStableSurfaces = true;

        [Tooltip("A high planar velocity value (e.g. tight platformer) can be projected onto an unstable surface, causing the character to " +
            "climb over obstacles it is not supposed to. This option will prevent that by removing all the extra planar velocity.")]
        public bool preventUnstableClimbing = true;

        [Tooltip("This will prevent the character from stepping over an unstable surface (a \"bad\" step). This requires a bit more processing, so if your character does not need this level of precision " +
        "you can disable it.")]
        public bool preventBadSteps = true;

        [Header("Step handling")]

        [Tooltip("The offset distance applied to the bottom of the character. A higher offset means more walkable surfaces")]
        [Min(0f)]
        public float stepUpDistance = 0.5f;

        [Tooltip("The distance the character is capable of detecting ground. Use this value to clamp (or not) the character to the ground.")]
        [Min(0f)]
        public float stepDownDistance = 0.5f;


        [Header("Grounding")]

        [Tooltip("Prevents the character from enter grounded state (IsGrounded will be false)")]
        public bool alwaysNotGrounded = false;

        [Tooltip("If enabled the character will do an initial ground check (at \"Start\"). If the test fails the starting state will be \"Not grounded\".")]
        public bool forceGroundedAtStart = true;

        [Tooltip("This option will enable a trigger (located at the capsule bottom center) that can be used to generate OnTriggerXXX messages. " +
        "Normally the character won't generate these messages (OnCollisionXXX/OnTriggerXXX) since the collider is not making direct contact with the ground")]
        public bool useGroundTrigger = true;

        [Tooltip("With this enabled the character bottom sphere (capsule) will be simulated as a cylinder. This works only when the character is standing on an edge.")]
        public bool edgeCompensation = false;

        [Tooltip("Situation: The character makes contact with the ground and detect a stable edge." +
        "\n\n True: the character will enter stable state regardless of the collision contact angle.\n" +
        "\n\n False: the character will use the contact angle instead (contact normal) in order to determine stability (<= slopeLimit).")]
        public bool useStableEdgeWhenLanding = true;

        [Tooltip("Should the character detect a new (and valid) ground if its vertical velocity is positive?")]
        public bool detectGroundWhileAscending = false;


        [Header("Dynamic ground")]

        [Tooltip("Should the character be affected by the movement of the ground?")]
        public bool supportDynamicGround = true;

        public LayerMask dynamicGroundLayerMask = -1;

        [Tooltip("The forward direction of the character will be affected by the rotation of the ground (only yaw motion allowed).")]
        public bool rotateForwardDirection = true;

        [Tooltip("This is the maximum ground velocity delta (from the previous frame to the current one) tolerated by the character." +
        "\n\nIf the ground accelerates too much, then the character will stop moving with it." + "\n\nImportant: This does not apply to one way platforms.")]
        public float maxGroundVelocityChange = 30f;


        [UnityEngine.Serialization.FormerlySerializedAs("maxForceNotGroundedGroundVelocity")]
        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents the minimum planar velocity required.")]
        public float inheritedGroundPlanarVelocityThreshold = 2f;

        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents how much of the planar component is utilized.")]
        public float inheritedGroundPlanarVelocityMultiplier = 1f;

        [UnityEngine.Serialization.FormerlySerializedAs("maxForceNotGroundedGroundVelocity")]
        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents the minimum vertical velocity required.")]
        public float inheritedGroundVerticalVelocityThreshold = 2f;

        [Tooltip("When the character becomes \"not grounded\" (after a ForceNotGrounded call) part of the ground velocity can be transferred to its own velocity. " +
        "This value represents how much of the planar component is utilized.")]
        public float inheritedGroundVerticalVelocityMultiplier = 1f;

        [Header("Velocity")]
        [Tooltip("Whether or not to project the initial velocity (stable) onto walls.")]
        [SerializeField]
        public bool slideOnWalls = true;

        [SerializeField]
        bool resetVelocityOnTeleport = true;

        [Tooltip("Should the actor re-assign the rigidbody velocity after the simulation is done?\n\n" +
        "PreSimulationVelocity: the character uses the velocity prior to the simulation (modified by this component).\nPostSimulationVelocity: the character uses the velocity received from the simulation (no re-assignment).\nInputVelocity: the character \"gets back\" its initial velocity (before being modified by this component).")]
        public CharacterVelocityMode stablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;

        [Tooltip("Should the actor re-assign the rigidbody velocity after the simulation is done?\n\n" +
        "PreSimulationVelocity: the character uses the velocity prior to the simulation (modified by this component).\nPostSimulationVelocity: the character uses the velocity received from the simulation (no re-assignment).\nInputVelocity: the character \"gets back\" its initial velocity (before being modified by this component).")]
        public CharacterVelocityMode unstablePostSimulationVelocity = CharacterVelocityMode.UsePostSimulationVelocity;


        [Header("Rotation")]

        [Tooltip("Should this component define the character \"Up\" direction?")]
        public bool constraintRotation = true;

        public Transform upDirectionReference = null;

        [Tooltip("The desired up direction.")]
        public Vector3 constraintUpDirection = Vector3.up;



        [Header("Physics")]

        public bool canPushDynamicRigidbodies = true;

        public LayerMask pushableRigidbodyLayerMask = -1;

        public bool applyWeightToGround = true;

        public LayerMask applyWeightLayerMask = -1;

        public float weightGravity = CharacterConstants.DefaultGravity;


        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        public enum SizeReferenceType
        {
            Top,
            Center,
            Bottom
        }

        Vector3 groundToCharacter;
        bool forceNotGroundedFlag = false;
        int forceNotGroundedFrames = 0;
        bool inheritVelocityFlag = false;
        Quaternion preSimulationGroundRotation;
        Vector3 preSimulationPosition;
        RigidbodyComponent groundRigidbodyComponent = null;
        CharacterCollisions characterCollisions = new CharacterCollisions();
        CircleCollider2D groundTriggerCollider2D = null;
        SphereCollider groundTriggerCollider3D = null;
        float unstableGroundContactTime = 0f;

        ColliderComponent.PenetrationDelegate removePenetrationAction;


        public float StepOffset => stepUpDistance - BodySize.x / 2f;

        public void OnValidate()
        {
            if (CharacterBody == null)
                CharacterBody = GetComponent<LocomotionShape>();

            stepUpDistance = Mathf.Clamp(
                stepUpDistance,
                CharacterConstants.ColliderMinBottomOffset + CharacterBody.BodySize.x / 2f,
                CharacterBody.BodySize.y - CharacterBody.BodySize.x / 2f
            );

            maxGroundVelocityChange = Mathf.Min(maxGroundVelocityChange, 0);
            inheritedGroundPlanarVelocityThreshold = Mathf.Min(inheritedGroundPlanarVelocityThreshold, 0);
            inheritedGroundPlanarVelocityMultiplier = Mathf.Min(inheritedGroundPlanarVelocityMultiplier, 0);
            inheritedGroundVerticalVelocityThreshold = Mathf.Min(inheritedGroundVerticalVelocityThreshold, 0);
            inheritedGroundVerticalVelocityMultiplier = Mathf.Min(inheritedGroundVerticalVelocityMultiplier, 0);

        }


        /// <summary>
        /// Gets the RigidbodyComponent component associated with the character.
        /// </summary>
        public override RigidbodyComponent RigidbodyComponent => CharacterBody.RigidbodyComponent;

        /// <summary>
        /// Gets the ColliderComponent component associated with the character.
        /// </summary>
        public ColliderComponent ColliderComponent => CharacterBody.ColliderComponent;

        /// <summary>
        /// Gets the physics component from the character.
        /// </summary>
        public PhysicsComponent PhysicsComponent { get; private set; }

        /// <summary>
        /// Gets the CharacterBody component associated with this character actor.
        /// </summary>
        public LocomotionShape CharacterBody { get; private set; }

        /// <summary>
        /// Returns the current character actor state. This enum variable contains the information about the grounded and stable state, all in one.
        /// </summary>
        public CharacterActorState CurrentState
        {
            get
            {
                if (IsGrounded)
                    return IsStable ? CharacterActorState.StableGrounded : CharacterActorState.UnstableGrounded;
                else
                    return CharacterActorState.NotGrounded;
            }
        }

        /// <summary>
        /// Returns the character actor state from the previous frame.
        /// </summary>
        public CharacterActorState PreviousState
        {
            get
            {
                if (WasGrounded)
                    return WasStable ? CharacterActorState.StableGrounded : CharacterActorState.UnstableGrounded;
                else
                    return CharacterActorState.NotGrounded;
            }
        }

        /// <summary>
        /// Returns true if the character is standing on an edge.
        /// </summary>
        public bool IsOnEdge => characterCollisionInfo.isOnEdge;

        /// <summary>
        /// Returns the angle between the both sides of the edge.
        /// </summary>
        public float EdgeAngle => characterCollisionInfo.edgeAngle;

        /// <summary>
        /// Gets the grounded state, true if the ground object is not null, false otherwise.
        /// </summary>
        public bool IsGrounded => characterCollisionInfo.groundObject != null;

        /// <summary>
        /// Gets the angle between the up vector and the stable normal.
        /// </summary>
        public float GroundSlopeAngle => characterCollisionInfo.groundSlopeAngle;

        /// <summary>
        /// Gets the contact point obtained directly from the ground test (sphere cast).
        /// </summary>
        public Vector3 GroundContactPoint => characterCollisionInfo.groundContactPoint;

        /// <summary>
        /// Gets the normal vector obtained directly from the ground test (sphere cast).
        /// </summary>
        public Vector3 GroundContactNormal => characterCollisionInfo.groundContactNormal;

        /// <summary>
        /// Gets the normal vector used to determine stability. This may or may not be the normal obtained from the ground test.
        /// </summary>
        public Vector3 GroundStableNormal => IsStable ? characterCollisionInfo.groundStableNormal : Up;


        /// <summary>
        /// Gets the GameObject component of the current ground.
        /// </summary>
        public GameObject GroundObject => characterCollisionInfo.groundObject;

        /// <summary>
        /// Gets the Transform component of the current ground.
        /// </summary>
        public Transform GroundTransform => GroundObject != null ? GroundObject.transform : null;

        /// <summary>
        /// Gets the Collider2D component of the current ground.
        /// </summary>
        public Collider2D GroundCollider2D => characterCollisionInfo.groundCollider2D;
        /// <summary>
        /// Gets the Collider3D component of the current ground.
        /// </summary>
        public Collider GroundCollider3D => characterCollisionInfo.groundCollider3D;

        /// <summary>
        /// Gets the Rigidbody component of the current ground.
        /// </summary>
        public Rigidbody GroundRigidbody3D => characterCollisionInfo.groundRigidbody3D;

        // Wall ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────	

        /// <summary>
        /// Gets the wall collision flag, true if the character hit a wall, false otherwise.
        /// </summary>
        public bool WallCollision => characterCollisionInfo.wallCollision;


        /// <summary>
        /// Gets the angle between the contact normal (wall collision) and the Up direction.
        /// </summary>	
        public float WallAngle => characterCollisionInfo.wallAngle;


        /// <summary>
        /// Gets the current contact (wall collision).
        /// </summary>
        public Contact WallContact => characterCollisionInfo.wallContact;


        // Head ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────	

        /// <summary>
        /// Gets the head collision flag, true if the character hits something with its head, false otherwise.
        /// </summary>
        public bool HeadCollision => characterCollisionInfo.headCollision;


        /// <summary>
        /// Gets the angle between the contact normal (head collision) and the Up direction.
        /// </summary>
        public float HeadAngle => characterCollisionInfo.headAngle;


        /// <summary>
        /// Gets the current contact (head collision).
        /// </summary>
        public Contact HeadContact => characterCollisionInfo.headContact;


        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the current stability state of the character. Stability is equal to "grounded + slope angle <= slope limit".
        /// </summary>
        public bool IsStable
        {
            get
            {
                if (!IsGrounded)
                    return false;

                if (!EvaluateGroundStability(characterCollisionInfo.groundObject.transform))
                    return false;

                if (WasStable)
                {
                    // If the character is stable, then use the groundSlopeAngle (updated in ProbeGround).
                    return characterCollisionInfo.groundSlopeAngle <= slopeLimit;
                }
                else
                {
                    if (useStableEdgeWhenLanding)
                    {
                        return characterCollisionInfo.groundSlopeAngle <= slopeLimit;
                    }
                    else
                    {
                        // If the character was not stable, then define stability by using the contact normal instead of the "stable" normal.
                        float contactSlopeAngle = Vector3.Angle(Up, characterCollisionInfo.groundContactNormal);
                        return contactSlopeAngle <= slopeLimit;
                    }

                }
            }
        }

        /// <summary>
        /// Returns true if the character is grounded onto an unstable ground, false otherwise.
        /// </summary>
        public bool IsOnUnstableGround => IsGrounded && characterCollisionInfo.groundSlopeAngle > slopeLimit;

        /// <summary>
        /// Gets the previous grounded state.
        /// </summary>
        public bool WasGrounded { get; private set; }

        /// <summary>
        /// Gets the previous stability state.
        /// </summary>
        public bool WasStable { get; private set; }

        /// <summary>
        /// A property that indicates if the character has become grounded during the previous physics update.
        /// </summary>
        public bool HasBecomeGrounded { get; private set; }

        /// <summary>
        /// A property that indicates if the character has become stable during the previous physics update.
        /// </summary>
        public bool HasBecomeStable { get; private set; }

        /// <summary>
        /// A property that indicates if the character has abandoned the grounded state during the previous physics update.
        /// </summary>
        public bool HasBecomeNotGrounded { get; private set; }

        /// <summary>
        /// A property that indicates if the character has become unstable during the previous physics update.
        /// </summary>
        public bool HasBecomeUnstable { get; private set; }

        /// <summary>
        /// Gets the RigidbodyComponent component from the ground.
        /// </summary>
        public RigidbodyComponent GroundRigidbodyComponent
        {
            get
            {
                if (!IsStable)
                    groundRigidbodyComponent = null;

                return groundRigidbodyComponent;
            }
        }


        /// <summary>
        /// Gets the ground rigidbody position.
        /// </summary>
        public Vector3 GroundPosition => GroundRigidbody3D.position;

        /// <summary>
        /// Gets the ground rigidbody rotation.
        /// </summary>
        public Quaternion GroundRotation => GroundRigidbody3D.rotation;

        /// <summary>
        /// Returns true if the current ground is a Rigidbody (2D or 3D), false otherwise.
        /// </summary>
        public bool IsGroundARigidbody => characterCollisionInfo.groundRigidbody3D != null;

        /// <summary>
        /// Returns true if the current ground is a kinematic Rigidbody (2D or 3D), false otherwise.
        /// </summary>
        public bool IsGroundAKinematicRigidbody => characterCollisionInfo.groundRigidbody3D.isKinematic;

        /// <summary>
        /// Returns the point velocity (Rigidbody API) of the ground at a given position.
        /// </summary>
        public Vector3 GetGroundPointVelocity(Vector3 point)
        {
            if (!IsGroundARigidbody)
                return Vector3.zero;

            return characterCollisionInfo.groundRigidbody3D.GetPointVelocity(point);
        }

        protected CharacterCollisionInfo characterCollisionInfo = new CharacterCollisionInfo();

        /// <summary>
        /// Gets a structure with all the information regarding character collisions. Most of the character properties (e.g. IsGrounded, IsStable, GroundObject, and so on)
        /// can be obtained from this structure.
        /// </summary>
        public CharacterCollisionInfo CharacterCollisionInfo => characterCollisionInfo;


#if UNITY_TERRAIN_MODULE
        Dictionary<Transform, Terrain> terrains = new Dictionary<Transform, Terrain>();
#endif
        Dictionary<Transform, RigidbodyComponent> groundRigidbodyComponents = new Dictionary<Transform, RigidbodyComponent>();



        public float GroundedTime { get; private set; }
        public float NotGroundedTime { get; private set; }

        public float StableElapsedTime { get; private set; }
        public float UnstableElapsedTime { get; private set; }


        /// <summary>
        /// Gets the current body size (width and height).
        /// </summary>
        public Vector2 BodySize { get; private set; }

        /// <summary>
        /// Gets the current body size (width and height).
        /// </summary>
        public Vector2 DefaultBodySize => CharacterBody.BodySize;



        /// <summary>
        /// Gets/Sets the rigidbody velocity projected onto a plane formed by its up direction.
        /// </summary>
        public Vector3 StableVelocity
        {
            get
            {
                return Velocity.ProjectOnTangent(GroundStableNormal, Up);
            }
            set
            {
                Velocity = value.ProjectOnTangent(GroundStableNormal, Up);
            }
        }


        public Vector3 LastGroundedVelocity { get; private set; }


        #region public Body properties

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 Center
        {
            get
            {
                return GetCenter(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 Top
        {
            get
            {
                return GetTop(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 Bottom
        {
            get
            {
                return GetBottom(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 TopCenter
        {
            get
            {
                return GetTopCenter(Position);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 BottomCenter
        {
            get
            {
                return GetBottomCenter(Position, 0f);
            }
        }

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 OffsettedBottomCenter
        {
            get
            {
                return GetBottomCenter(Position, StepOffset);
            }
        }

        #endregion

        #region Body functions

        /// <summary>
        /// Gets the center of the collision shape.
        /// </summary>
        public Vector3 GetCenter(Vector3 position)
        {
            return position + Up * BodySize.y / 2f;
        }

        /// <summary>
        /// Gets the top most point of the collision shape.
        /// </summary>
        public Vector3 GetTop(Vector3 position)
        {
            return position + Up * (BodySize.y - CharacterConstants.SkinWidth);
        }

        /// <summary>
        /// Gets the bottom most point of the collision shape.
        /// </summary>
        public Vector3 GetBottom(Vector3 position)
        {
            return position + Up * CharacterConstants.SkinWidth;
        }

        /// <summary>
        /// Gets the center of the top sphere of the collision shape.
        /// </summary>
        public Vector3 GetTopCenter(Vector3 position)
        {
            return position + Up * (BodySize.y - BodySize.x / 2f);
        }

        /// <summary>
        /// Gets the center of the top sphere of the collision shape (considering an arbitrary body size).
        /// </summary>
        public Vector3 GetTopCenter(Vector3 position, Vector2 bodySize)
        {
            return position + Up * (bodySize.y - bodySize.x / 2f);
        }

        /// <summary>
        /// Gets the center of the bottom sphere of the collision shape.
        /// </summary>
        public Vector3 GetBottomCenter(Vector3 position, float bottomOffset = 0f)
        {
            return position + Up * (BodySize.x / 2f + bottomOffset);
        }


        /// <summary>
        /// Gets the center of the bottom sphere of the collision shape (considering an arbitrary body size).
        /// </summary>
        public Vector3 GetBottomCenter(Vector3 position, Vector2 bodySize)
        {
            return position + Up * (bodySize.x / 2f);
        }

        /// <summary>
        /// Gets the a vector that goes from the bottom center to the top center (topCenter - bottomCenter).
        /// </summary>
        public Vector3 GetBottomCenterToTopCenter()
        {
            return Up * (BodySize.y - BodySize.x);
        }

        /// <summary>
        /// Gets the a vector that goes from the bottom center to the top center (topCenter - bottomCenter).
        /// </summary>
        public Vector3 GetBottomCenterToTopCenter(Vector2 bodySize)
        {
            return Up * (bodySize.y - bodySize.x);
        }


        #endregion


        public CharacterCollisions CharacterCollisions => characterCollisions;


        protected void Awake()
        {

            CharacterBody = GetComponent<LocomotionShape>();
            BodySize = CharacterBody.BodySize;

            PhysicsComponent = gameObject.AddComponent<PhysicsComponent3D>();


            RigidbodyComponent.IsKinematic = false;
            RigidbodyComponent.UseGravity = false;
            RigidbodyComponent.Mass = CharacterBody.Mass;
            RigidbodyComponent.LinearDrag = 0f;
            RigidbodyComponent.AngularDrag = 0f;
            RigidbodyComponent.Constraints = RigidbodyConstraints.FreezeRotation;


            characterCollisions.Initialize(this, PhysicsComponent);

            // Ground trigger
            groundTriggerCollider3D = gameObject.AddComponent<SphereCollider>();
            groundTriggerCollider3D.hideFlags = HideFlags.NotEditable;
            groundTriggerCollider3D.isTrigger = true;
            groundTriggerCollider3D.radius = BodySize.x / 2f;
            groundTriggerCollider3D.center = Vector3.up * (BodySize.x / 2f - CharacterConstants.GroundTriggerOffset);

            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), groundTriggerCollider3D, true);
            removePenetrationAction = RemovePenetrationAction;
        }

        protected override void Start()
        {
            base.Start();

            HitInfoFilter filter = new(
                PhysicsComponent.CollisionLayerMask,
                false,
                true
            );

            // Initial OWP check
            characterCollisions.CheckOverlap(
                Position,
                0f,
                in filter
            );

            // Initial "Force Grounded"
            if (forceGroundedAtStart && !alwaysNotGrounded)
                ForceGrounded();

            SetColliderSize();
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            OnTeleport += OnTeleportMethod;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnTeleport -= OnTeleportMethod;
        }

        void OnTeleportMethod(Vector3 position, Quaternion rotation)
        {
            if (resetVelocityOnTeleport)
                Velocity = Vector3.zero;
        }

        /// <summary>
        /// Applies a force at the ground contact point, in the direction of the weight (mass times gravity).
        /// </summary>
        protected virtual void ApplyWeight(Vector3 contactPoint)
        {
            if (!applyWeightToGround)
                return;

            if (!GroundObject.layer.BelongsToLayerMask(applyWeightLayerMask))
                return;

            if (GroundCollider3D?.attachedRigidbody == null)
                return;

            GroundCollider3D.attachedRigidbody.AddForceAtPosition(-Up * (CharacterBody.Mass * weightGravity), contactPoint);
        }


        /// <summary>
        /// Gets a list with all the current contacts.
        /// </summary>
        public List<Contact> Contacts
        {
            get
            {
                if (PhysicsComponent == null)
                    return null;

                return PhysicsComponent.Contacts;
            }
        }



        // Triggers ──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets the most recent trigger.
        /// </summary>
        public Trigger CurrentTrigger
        {
            get
            {
                if (PhysicsComponent.Triggers.Count == 0)
                    return new Trigger();   // "Null trigger"

                return PhysicsComponent.Triggers[PhysicsComponent.Triggers.Count - 1];
            }
        }

        /// <summary>
        /// Gets a list with all the triggers.
        /// </summary>
        public List<Trigger> Triggers
        {
            get
            {
                return PhysicsComponent.Triggers;
            }
        }

        public enum CharacterVelocityMode
        {
            UseInputVelocity,
            UsePreSimulationVelocity,
            UsePostSimulationVelocity
        }


        /// <summary>
        /// Gets the character velocity vector (Velocity) assigned prior to the FixedUpdate call. This is also known as the "input" velocity, 
        /// since it is the value the user has specified.
        /// </summary>
        public Vector3 InputVelocity { get; private set; }

        /// <summary>
        /// Gets a velocity vector which is the input velocity modified, based on the character actor internal rules (step up, slope limit, etc). 
        /// This velocity corresponds to the one used by the physics simulation.
        /// </summary>
        public Vector3 PreSimulationVelocity { get; private set; }

        /// <summary>
        /// Gets the character velocity as the result of the Physics simulation.
        /// </summary>
        public Vector3 PostSimulationVelocity { get; private set; }

        /// <summary>
        /// Gets the difference between the post-simulation velocity (after the physics simulation) and the pre-simulation velocity (just before the physics simulation). 
        /// This value is useful to detect any external response due to the physics simulation, such as hits coming from other rigidbodies.
        /// </summary>
        public Vector3 ExternalVelocity { get; private set; }


        void HandleRotation()
        {
            if (!constraintRotation)
                return;

            if (upDirectionReference != null)
            {
                Vector3 targetPosition = Position;
                Vector3 referenceToTarget = upDirectionReference.position - targetPosition;
                referenceToTarget.Normalize();

                constraintUpDirection = referenceToTarget;
            }

            Up = constraintUpDirection;
        }



        /// <summary>
        /// Returns a lits of all the contacts involved with wall collision events.
        /// </summary>
        public List<Contact> WallContacts { get; private set; } = new List<Contact>(10);

        /// <summary>
        /// Returns a lits of all the contacts involved with head collision events.
        /// </summary>
        public List<Contact> HeadContacts { get; private set; } = new List<Contact>(10);

        /// <summary>
        /// Returns a lits of all the contacts involved with head collision events.
        /// </summary>
        public List<Contact> GroundContacts { get; private set; } = new List<Contact>(10);



        void GetContactsInformation()
        {
            bool wasCollidingWithWall = characterCollisionInfo.wallCollision;
            bool wasCollidingWithHead = characterCollisionInfo.headCollision;

            GroundContacts.Clear();
            WallContacts.Clear();
            HeadContacts.Clear();

            for (int i = 0; i < Contacts.Count; i++)
            {
                Contact contact = Contacts[i];

                float verticalAngle = Vector3.Angle(Up, contact.normal);

                if (verticalAngle.NearlyEqualsTo(90f, CharacterConstants.WallContactAngleTolerance))
                {
                    WallContacts.Add(contact);
                }

                if (verticalAngle >= CharacterConstants.HeadContactMinAngle)
                {
                    HeadContacts.Add(contact);
                }

                if (verticalAngle <= 89f)
                {
                    GroundContacts.Add(contact);
                }
            }


            if (WallContacts.Count == 0)
            {
                characterCollisionInfo.ResetWallInfo();
            }
            else
            {
                Contact wallContact = WallContacts[0];
                characterCollisionInfo.SetWallInfo(in wallContact, this);

                if (!wasCollidingWithWall)
                    OnWallHit?.Invoke(wallContact);
            }


            if (HeadContacts.Count == 0)
            {
                characterCollisionInfo.ResetHeadInfo();
            }
            else
            {
                Contact headContact = HeadContacts[0];
                characterCollisionInfo.SetHeadInfo(in headContact, this);

                if (!wasCollidingWithHead)
                    OnHeadHit?.Invoke(headContact);
            }

        }

        protected override void PreSimulationUpdate(float dt)
        {
            UpdateStabilityFlags();

            PhysicsComponent.ClearContacts();

            InputVelocity = Velocity;

            if (alwaysNotGrounded && WasGrounded)
                ForceNotGrounded();

            if (!IsKinematic)
                ProcessVelocity(dt);

            SetColliderSize();

            PreSimulationVelocity = Velocity;
            preSimulationPosition = Position;

            // Enable/Disable the ground trigger.
            groundTriggerCollider3D.enabled = useGroundTrigger;
        }

        protected override void PostSimulationUpdate(float dt)
        {
            HandleRotation();
            GetContactsInformation();

            PostSimulationVelocity = Velocity;
            ExternalVelocity = PostSimulationVelocity - PreSimulationVelocity;

            PreGroundProbingPosition = PostGroundProbingPosition = Position;
            if (!IsKinematic)
            {
                if (!IsStable)
                {
                    Vector3 position = Position;
                    UnstableProbeGround(ref position, dt);
                    SetDynamicGroundData(position);
                    Position = position;
                }

                if (IsStable)
                {
                    ProcessDynamicGroundMovement(dt);

                    PreGroundProbingPosition = Position;
                    ProbeGround(dt);
                    PostGroundProbingPosition = Position;
                }
            }

            GroundProbingDisplacement = PostGroundProbingPosition - PreGroundProbingPosition;

            PostSimulationVelocityUpdate();

            UpdateTimers(dt);

            if (HasBecomeGrounded)
                OnGroundedStateEnter?.Invoke(LocalVelocity);

            if (HasBecomeNotGrounded)
                OnGroundedStateExit?.Invoke();

            if (HasBecomeStable)
                OnStableStateEnter?.Invoke(LocalVelocity);

            if (HasBecomeUnstable)
                OnStableStateExit?.Invoke();

            if (forceNotGroundedFrames != 0)
                forceNotGroundedFrames--;

            forceNotGroundedFlag = false;
            inheritVelocityFlag = false;
        }

        void UpdateTimers(float dt)
        {
            if (IsStable)
            {
                StableElapsedTime += dt;
                UnstableElapsedTime = 0f;
            }
            else
            {
                StableElapsedTime = 0f;
                UnstableElapsedTime += dt;
            }

            if (IsGrounded)
            {
                NotGroundedTime = 0f;
                GroundedTime += dt;
            }
            else
            {
                NotGroundedTime += dt;
                GroundedTime = 0f;
            }
        }


        #region Dynamic Ground

        bool IsAllowedToFollowRigidbodyReference()
        {
            if (!supportDynamicGround)
                return false;

            if (!IsStable)
                return false;

            if (!GroundObject.layer.BelongsToLayerMask(dynamicGroundLayerMask))
                return false;

            if (characterCollisionInfo.groundRigidbody3D == null)
                return false;

            return true;

        }

        void SetDynamicGroundData(Vector3 position)
        {
            if (!IsAllowedToFollowRigidbodyReference())
                return;

            preSimulationGroundRotation = GroundRotation;
            groundToCharacter = position - GroundPosition;
        }

        void ApplyGroundMovement(ref Vector3 position, ref Quaternion rotation, float dt)
        {
            Quaternion deltaRotation = GroundRotation * Quaternion.Inverse(preSimulationGroundRotation);
            position = GroundPosition + (deltaRotation * groundToCharacter);

            if (rotateForwardDirection)
            {
                Vector3 forward = deltaRotation * Forward;
                forward = Vector3.ProjectOnPlane(forward, Up);
                forward.Normalize();

                rotation = Quaternion.LookRotation(forward, Up);
            }
        }

        void UpdateGroundVelocity()
        {
            PreviousGroundVelocity = GroundVelocity;
            GroundVelocity = GetGroundPointVelocity(GroundContactPoint);
        }

        void ProcessDynamicGroundMovement(float dt)
        {
            if (!IsAllowedToFollowRigidbodyReference())
                return;

            IgnoreGroundResponse();

            Vector3 targetPosition = Position;
            Quaternion targetRotation = Rotation;
            ApplyGroundMovement(ref targetPosition, ref targetRotation, dt);

            // When landing on dynamic ground, simulate infinite friction by removing the platform velocity from the character velocity.
            if (!WasGrounded)
            {
                Vector3 planarVelocityOnPlatform = Vector3.Project(PlanarVelocity, GetGroundPointVelocity(GroundContactPoint));
                PlanarVelocity -= planarVelocityOnPlatform;
            }

            if (GroundDeltaVelocity.magnitude > maxGroundVelocityChange)
            {
                float upToDynamicGroundVelocityAngle = Vector3.Angle(Vector3.Normalize(GroundVelocity), Up);
                if (upToDynamicGroundVelocityAngle < 45f)
                    ForceNotGrounded();

                Vector3 characterVelocity = PreviousGroundVelocity;

                Velocity = characterVelocity;
                Position += characterVelocity * dt;
                Rotation = targetRotation;
            }
            else
            {

                Position = targetPosition;
                Vector3 position = Position;
                bool overlapDetected = RemovePenetration(ref position);
                Position = position;

                if (!overlapDetected)
                    Rotation = targetRotation;
            }
        }

        void ProcessInheritedVelocity()
        {
            if (!forceNotGroundedFlag)
                return;

            if (!inheritVelocityFlag)
                return;

            Vector3 localGroundVelocity = Quaternion.Inverse(transform.rotation) * GroundVelocity;
            Vector3 planarGroundVelocity = Vector3.ProjectOnPlane(GroundVelocity, Up);
            Vector3 verticalGroundVelocity = Vector3.Project(GroundVelocity, Up);
            Vector3 inheritedGroundVelocity = Vector3.zero;

            if (planarGroundVelocity.magnitude >= inheritedGroundPlanarVelocityThreshold)
                inheritedGroundVelocity += planarGroundVelocity * inheritedGroundPlanarVelocityMultiplier;

            if (verticalGroundVelocity.magnitude >= inheritedGroundVerticalVelocityThreshold)
            {
                // This prevents an edge case where the character is unable to jump (descending platform)
                if (LocalVelocity.y > -localGroundVelocity.y)
                    inheritedGroundVelocity += verticalGroundVelocity * inheritedGroundVerticalVelocityMultiplier;
            }

            Velocity += inheritedGroundVelocity;
            GroundVelocity = Vector3.zero;
            PreviousGroundVelocity = Vector3.zero;
        }



        #endregion

        protected bool RemovePenetration(ref Vector3 position)
        {
            int overlapIteration = 0;
            bool iterationOverlapDetected = false;
            bool overlapDetected = false;
            var rotation = Rotation;

            do
            {
                iterationOverlapDetected = ColliderComponent.ComputePenetration(ref position, ref rotation, removePenetrationAction);
                overlapDetected |= iterationOverlapDetected;
                overlapIteration++;
            } while (overlapIteration < 2 && iterationOverlapDetected);

            return overlapDetected;
        }

        void RemovePenetrationAction(ref Vector3 bodyPosition, ref Quaternion bodyRotation, Transform otherColliderTransform, Vector3 penetrationDirection, float penetrationDistance)
        {
            Vector3 separation = penetrationDirection * penetrationDistance;

            if (IsStable)
                separation = Vector3.ProjectOnPlane(separation, GroundStableNormal).normalized * penetrationDistance;

            separation.AddMagnitude(CharacterConstants.SkinWidth);

            bodyPosition += separation;
        }

        void PostSimulationVelocityUpdate()
        {
            if (IsStable)
            {
                switch (stablePostSimulationVelocity)
                {
                    case CharacterVelocityMode.UseInputVelocity:

                        Velocity = InputVelocity;

                        break;
                    case CharacterVelocityMode.UsePreSimulationVelocity:

                        Velocity = PreSimulationVelocity;

                        // Take the rigidbody velocity and convert it into planar velocity
                        if (WasStable)
                            PlanarVelocity = Vector3.Normalize(PlanarVelocity) * Velocity.magnitude;

                        break;
                    case CharacterVelocityMode.UsePostSimulationVelocity:

                        // Take the rigidbody velocity and convert it into planar velocity
                        if (WasStable)
                            PlanarVelocity = Vector3.Normalize(PlanarVelocity) * Velocity.magnitude;

                        break;
                }

                UpdateGroundVelocity();
            }
            else
            {
                switch (unstablePostSimulationVelocity)
                {
                    case CharacterVelocityMode.UseInputVelocity:

                        Velocity = InputVelocity;

                        break;
                    case CharacterVelocityMode.UsePreSimulationVelocity:

                        Velocity = PreSimulationVelocity;

                        break;
                    case CharacterVelocityMode.UsePostSimulationVelocity:

                        break;
                }
            }

            if (IsGrounded)
                LastGroundedVelocity = Velocity;
        }


        bool IgnoreGroundResponse()
        {
            for (int i = 0; i < Contacts.Count; i++)
            {
                Contact contact = Contacts[i];
                if (!contact.isRigidbody)
                    continue;

                if (!contact.isKinematicRigidbody)
                    continue;

                // Check if the contact belongs to the ground rigidbody
                if (contact.collider3D.attachedRigidbody == GroundRigidbody3D)
                {
                    Velocity = PreSimulationVelocity;
                    return true;
                }
            }

            return false;
        }


        #region Events



        /// <summary>
        /// This event is called when the character hits its head (not grounded).
        /// 
        /// The related collision information struct is passed as an argument.
        /// </summary>
        public event System.Action<Contact> OnHeadHit;

        /// <summary>
        /// This event is called everytime the character is blocked by an unallowed geometry, this could be
        /// a wall or a steep slope (depending on the "slopeLimit" value).
        /// 
        /// The related collision information struct is passed as an argument.
        /// </summary>
        public event System.Action<Contact> OnWallHit;



        /// <summary>
        /// This event is called when the character enters the grounded state.
        /// 
        /// The local linear velocity is passed as an argument.
        /// </summary>
        public event System.Action<Vector3> OnGroundedStateEnter;

        /// <summary>
        /// This event is called when the character exits the grounded state.
        /// </summary>
        public event System.Action OnGroundedStateExit;

        /// <summary>
        /// This event is called when the character make contact with a new ground (object).
        /// </summary>
        public event System.Action OnNewGroundEnter;

        /// <summary>
        /// This event is called when the character becomes stable.
        /// 
        /// The local linear velocity is passed as an argument.
        /// </summary>
        public event System.Action<Vector3> OnStableStateEnter;

        /// <summary>
        /// This event is called when the character becomes unstable.
        /// 
        /// The local linear velocity is passed as an argument.
        /// </summary>
        public event System.Action OnStableStateExit;

        #endregion


        /// <summary>
        /// Gets the velocity of the ground (rigidbody).
        /// </summary>
        public Vector3 GroundVelocity { get; private set; }

        /// <summary>
        /// Gets the previous velocity of the ground (rigidbody).
        /// </summary>
        public Vector3 PreviousGroundVelocity { get; private set; }

        /// <summary>
        /// The ground change in velocity (current velocity - previous velocity).
        /// </summary>
        public Vector3 GroundDeltaVelocity => GroundVelocity - PreviousGroundVelocity;

        /// <summary>
        /// The ground acceleration (GroundDeltaVelocity / dt).
        /// </summary>
        public Vector3 GroundAcceleration => (GroundVelocity - PreviousGroundVelocity) / Time.fixedDeltaTime;


        /// <summary>
        /// Returns true if the ground vertical displacement (moving ground) is positive.
        /// </summary>
        public bool IsGroundAscending => (Quaternion.Inverse(transform.rotation) * Vector3.Project(GroundVelocity * Time.deltaTime, Up)).y > 0;

        void ProcessVelocity(float dt)
        {
            Vector3 position = Position;

            if (IsStable)
                ProcessStableMovement(dt, ref position);
            else
                ProcessUnstableMovement(dt, ref position);

            Velocity = (position - Position) / dt;
        }


        void ProcessStableMovement(float dt, ref Vector3 position)
        {
            ApplyWeight(GroundContactPoint);

            VerticalVelocity = Vector3.zero;

            Vector3 displacement = (Velocity * dt).ProjectOnTangent(GroundStableNormal, Up);

            StableCollideAndSlide(ref position, displacement, false);

            SetDynamicGroundData(position);

            if (!IsStable)
            {
                groundRigidbodyComponent = null;
            }
        }

        void ProcessUnstableMovement(float dt, ref Vector3 position)
        {
            ProcessInheritedVelocity();

            Vector3 displacement = Velocity * dt;
            UnstableCollideAndSlide(ref position, displacement, dt);
        }


        /// <summary>
        /// Gets the new position value after comparing the old size with the new one based on a given height anchor point (a.k.a size reference).
        /// </summary>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <param name="heightAnchorRatio">Anchor point used for the size change. This value range from 0 (bottom) to 1 (top).</param>
        /// <returns>Position value.</returns>
        Vector3 GetSizeOffsetPosition(Vector2 size, float heightAnchorRatio)
        {
            float verticalOffset = (BodySize.y - size.y) * heightAnchorRatio;

            Vector3 testPosition = Position + Up * verticalOffset;
            return testPosition;
        }

        /// <summary>
        /// Changes the body size of the actor. Additionaly, an overlap test can be performed beforehand in order to check
        /// if the character will fit or not. If the test failed (overlap detected), changes won't be applied.
        /// </summary>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <param name="sizeReferenceType">Anchor point used for the size change.</param>
        public void SetSize(Vector2 size, SizeReferenceType sizeReferenceType)
        {
            float heightAnchorRatio = 0f;
            switch (sizeReferenceType)
            {
                case SizeReferenceType.Top:
                    heightAnchorRatio = 1f;
                    break;
                case SizeReferenceType.Center:
                    heightAnchorRatio = 0.5f;
                    break;
                default:
                    break;
            }

            Position = GetSizeOffsetPosition(size, heightAnchorRatio);
            BodySize = size;
            SetColliderSize();
        }


        /// <summary>
        /// Checks if the body fits in place. If so, it automatically changes the body size.
        /// </summary>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <param name="sizeReferenceType">Anchor point used for the size change.</param>
        /// <returns>True if the body fits.</returns>
        public bool CheckAndSetSize(Vector2 size, SizeReferenceType sizeReferenceType = SizeReferenceType.Bottom)
        {
            if (!CheckSize(Position, size))
                return false;

            SetSize(size, sizeReferenceType);

            return true;
        }

        /// <summary>
        /// Checks if the body fits in place.
        /// </summary>
        /// <param name="position">Overlap position.</param>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <param name="filter">Filter used by the overlap function.</param>
        /// <returns>True if the body fits.</returns>
        public bool CheckSize(Vector3 position, Vector2 size, in HitInfoFilter filter)
        {
            return characterCollisions.CheckBodySize(size, position, in filter);
        }

        /// <summary>
        /// Checks if the body fits in place.
        /// </summary>
        /// <param name="position">Overlap position.</param>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <returns>True if the body fits.</returns>
        public bool CheckSize(Vector3 position, Vector2 size)
        {
            HitInfoFilter filter = new HitInfoFilter(
                PhysicsComponent.CollisionLayerMask,
                true,
                true
            );

            return characterCollisions.CheckBodySize(size, position, in filter);
        }

        /// <summary>
        /// Checks if the body fits in place.
        /// </summary>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <param name="filter">Filter used by the overlap function.</param>
        /// <returns>True if the body fits.</returns>
        public bool CheckSize(Vector2 size, in HitInfoFilter filter)
        {
            return characterCollisions.CheckBodySize(size, Position, in filter);
        }

        /// <summary>
        /// Checks if the body fits in place.
        /// </summary>
        /// <param name="size">A Vector2 representing the desired width (x) and height (y).</param>
        /// <returns>True if the body fits.</returns>
        public bool CheckSize(Vector2 size)
        {
            HitInfoFilter filter = new HitInfoFilter(
                PhysicsComponent.CollisionLayerMask,
                true,
                true
            );

            return characterCollisions.CheckBodySize(size, Position, in filter);
        }

        [System.Obsolete]
        /// <summary>
        /// Checks if the new character size fits in place. If this check is valid then the real size of the character is changed.
        /// </summary>
        public bool SetBodySize(Vector2 size)
        {
            HitInfoFilter filter = new HitInfoFilter(
                PhysicsComponent.CollisionLayerMask,
                true,
                true
            );

            if (!characterCollisions.CheckBodySize(size, Position, in filter))
                return false;

            SetSize(size, SizeReferenceType.Bottom);

            return true;
        }

        /// <summary>
        /// Interpolates (Lerps) the height of the body to a target height.
        /// </summary>
        /// <param name="targetHeight">The desired height.</param>
        /// <param name="lerpFactor">How fast the interpolation is going to be applied.</param>
        /// <param name="sizeReferenceType">The size reference pivot.</param>
        /// <returns></returns>
        public bool CheckAndInterpolateSize(Vector2 targetSize, float lerpFactor, SizeReferenceType sizeReferenceType)
        {
            bool validSize = CheckSize(targetSize);
            if (validSize)
            {
                Vector2 size = Vector2.Lerp(BodySize, targetSize, lerpFactor);
                SetSize(size, sizeReferenceType);
            }

            return validSize;
        }

        /// <summary>
        /// Interpolates (Lerps) the height of the body to a target height.
        /// </summary>
        /// <param name="targetHeight">The desired height.</param>
        /// <param name="lerpFactor">How fast the interpolation is going to be applied.</param>
        /// <param name="sizeReferenceType">The size reference pivot.</param>
        /// <returns></returns>
        public bool CheckAndInterpolateHeight(float targetHeight, float lerpFactor, SizeReferenceType sizeReferenceType) =>
            CheckAndInterpolateSize(new Vector2(DefaultBodySize.x, targetHeight), lerpFactor, SizeReferenceType.Bottom);


        void SetColliderSize()
        {
            float verticalOffset = IsStable ? Mathf.Max(StepOffset, CharacterConstants.ColliderMinBottomOffset) : 0f;

            float radius = BodySize.x / 2f;
            float height = BodySize.y - verticalOffset;

            ColliderComponent.Size = new Vector2(2f * radius, height);
            ColliderComponent.Offset = Vector2.up * (verticalOffset + height / 2f);
        }



        /// <summary>
        /// Sweeps the body from its current position (CharacterActor.Position) towards the desired destination using the "collide and slide" algorithm. 
        /// At the end, the character will be moved to a valid position. Triggers and one way platforms will be ignored.
        /// </summary>
        public void SweepAndTeleport(Vector3 destination)
        {
            HitInfoFilter filter = new HitInfoFilter(PhysicsComponent.CollisionLayerMask, false, true);
            SweepAndTeleport(destination, in filter);
        }

        /// <summary>
        /// Sweeps the body from its current position (CharacterActor.Position) towards the desired destination using the "collide and slide" algorithm. 
        /// At the end, the character will be moved to a valid position. 
        /// </summary>
        public void SweepAndTeleport(Vector3 destination, in HitInfoFilter filter)
        {
            Vector3 displacement = destination - Position;
            CollisionInfo collisionInfo = characterCollisions.CastBody(
                Position,
                displacement,
                0f,
                in filter
            );

            Position += collisionInfo.displacement;
        }

        /// <summary>
        /// Forces the character to be grounded (isGrounded = true) if possible. The detection distance includes the step down distance.
        /// </summary>
        public void ForceGrounded()
        {
            if (!CanEnterGroundedState)
                return;

            HitInfoFilter filter = new(
                PhysicsComponent.CollisionLayerMask,
                false,
                true
            );

            CollisionInfo collisionInfo = characterCollisions.CheckForGround(
                Position,
                BodySize.y * 0.8f, // 80% of the height
                stepDownDistance,
                filter
            );


            if (collisionInfo.hitInfo.hit)
            {
                float slopeAngle = Vector3.Angle(Up, GetGroundSlopeNormal(collisionInfo));

                if (slopeAngle <= slopeLimit)
                {
                    Position += collisionInfo.displacement;
                    SetGroundInfo(in collisionInfo, true);
                    SetDynamicGroundData(Position);
                }

            }
        }


        public Vector3 GetGroundSlopeNormal(CollisionInfo collisionInfo)
        {

#if UNITY_TERRAIN_MODULE
            if (IsOnTerrain)
                return collisionInfo.hitInfo.normal;
#endif

            float contactSlopeAngle = Vector3.Angle(Up, collisionInfo.hitInfo.normal);
            if (collisionInfo.isAnEdge)
            {
                if (contactSlopeAngle < slopeLimit && collisionInfo.edgeUpperSlopeAngle <= slopeLimit && collisionInfo.edgeLowerSlopeAngle <= slopeLimit)
                {
                    return Up;
                }
                else if (collisionInfo.edgeUpperSlopeAngle <= slopeLimit)
                {
                    return collisionInfo.edgeUpperNormal;
                }
                else if (collisionInfo.edgeLowerSlopeAngle <= slopeLimit)
                {
                    return collisionInfo.edgeLowerNormal;
                }
                else
                {
                    return collisionInfo.hitInfo.normal;
                }
            }
            else
            {
                return collisionInfo.hitInfo.normal;
            }
        }

        /// <summary>
        /// The last vertical displacement calculated by the ground probabing algorithm (PostGroundProbingPosition - PreGroundProbingPosition).
        /// </summary>
        public Vector3 GroundProbingDisplacement { get; private set; }

        /// <summary>
        /// The last rigidbody position prior to the ground probing algorithm.
        /// </summary>
        public Vector3 PreGroundProbingPosition { get; private set; }

        /// <summary>
        /// The last rigidbody position after the ground probing algorithm.
        /// </summary>
        public Vector3 PostGroundProbingPosition { get; private set; }

        bool EvaluateGroundStability(Transform groundTransform)
        {
            // Do not allow "unstable layers".
            if (!groundTransform.gameObject.layer.BelongsToLayerMask(stableLayerMask))
                return false;

            // Do not allow other characters
            if (!allowCharactersAsStableSurfaces && groundTransform.TryGetComponent(out BuiltinLocomotionSystem1 otherCharacter))
                return false;

            return true;
        }

        void ProbeGround(float dt)
        {
            Vector3 position = Position;
            HitInfoFilter sweepFilter = new HitInfoFilter(PhysicsComponent.CollisionLayerMask, false, true);
            HitInfoFilter overlapFilter = new HitInfoFilter(PhysicsComponent.CollisionLayerMask, false, true);

            CollisionInfo collisionInfo = characterCollisions.CheckForGround(
                position,
                StepOffset,
                stepDownDistance,
                in sweepFilter
            );

            if (!collisionInfo.hitInfo.hit)
            {
                ForceNotGrounded();
                ProcessInheritedVelocity();
                return;
            }

            float slopeAngle = Vector3.Angle(Up, GetGroundSlopeNormal(collisionInfo));
            bool isGroundStable = slopeAngle <= slopeLimit && EvaluateGroundStability(collisionInfo.hitInfo.transform);

            position += collisionInfo.displacement;
            if (edgeCompensation && IsAStableEdge(collisionInfo))
            {
                Vector3 compensation = Vector3.Project((collisionInfo.hitInfo.point - position), Up);
                position += compensation;
            }

            // Do an overlap test only when a step is detected (verticalDisplacementComponent > some threshold)
            float verticalDisplacementComponent = transform.InverseTransformDirection(position - Position).y;
            bool overlapCheck = false;
            if (verticalDisplacementComponent > CharacterConstants.SkinWidth)
            {
                overlapCheck = characterCollisions.CheckOverlap(
                    position,
                    StepOffset,
                    in overlapFilter
                );
            }

            bool badStepDetected = false;
            badStepDetected |= !isGroundStable;
            badStepDetected |= overlapCheck;

            if (badStepDetected)
            {
                if (preventBadSteps)
                {
                    if (WasGrounded)
                    {
                        // Restore the initial position and simulate again.
                        Vector3 dynamicGroundDisplacement = GroundVelocity * dt;
                        Vector3 initialPosition = preSimulationPosition + dynamicGroundDisplacement;
                        position = initialPosition;

                        Vector3 unstableDisplacement = (InputVelocity * dt).ProjectOnTangent(GroundStableNormal, Up);

                        StableCollideAndSlide(ref position, unstableDisplacement, true);


                    }
                }

                // Fallback to a raycast
                collisionInfo = characterCollisions.CheckForGroundRay(
                    position,
                    sweepFilter
                );

                SetGroundInfo(in collisionInfo, true);
            }
            else
            {
                // Stable hit ---------------------------------------------------
                SetGroundInfo(in collisionInfo, true);

                //position += collisionInfo.displacement;

                //if (edgeCompensation && IsAStableEdge(collisionInfo))
                //{
                //    Vector3 compensation = Vector3.Project((collisionInfo.hitInfo.point - position), Up);
                //    position += compensation;
                //}
            }

            if (IsStable)
                Position = position;
        }

        void UpdateStabilityFlags()
        {
            HasBecomeGrounded = IsGrounded && !WasGrounded;
            HasBecomeStable = IsStable && !WasStable;
            HasBecomeNotGrounded = !IsGrounded && WasGrounded;
            HasBecomeUnstable = !IsStable && WasStable;
            WasGrounded = IsGrounded;
            WasStable = IsStable;
        }

        /// <summary>
        /// Forces the character to abandon the grounded state (isGrounded = false). 
        /// 
        /// TIP: This is useful when making the character jump.
        /// </summary>
        /// <param name="ignoreGroundContactFrames">The number of FixedUpdate frames to consume in order to prevent the character to 
        /// re-enter grounded state right after a ForceNotGrounded call.</param>
        public void ForceNotGrounded(int ignoreGroundContactFrames = 3)
        {
            forceNotGroundedFrames = ignoreGroundContactFrames;
            inheritVelocityFlag = IsAllowedToFollowRigidbodyReference();

            UpdateStabilityFlags();

            ResetGroundInfo();

            forceNotGroundedFlag = true;
        }



        bool IsAStableEdge(CollisionInfo collisionInfo)
        {
            return collisionInfo.isAnEdge && collisionInfo.edgeUpperSlopeAngle <= slopeLimit;
        }

        protected void StableCollideAndSlide(ref Vector3 position, Vector3 displacement, bool useFullBody)
        {
            Vector3 groundPlaneNormal = GroundStableNormal;
            Vector3 slidingPlaneNormal = Vector3.zero;

            HitInfoFilter filter = new(
                PhysicsComponent.CollisionLayerMask,
                false,
                true
            );

            int iteration = 0;
            while (iteration < CharacterConstants.MaxSlideIterations)
            {
                iteration++;

                CollisionInfo collisionInfo = characterCollisions.CastBody(
                    position,
                    displacement,
                    useFullBody ? 0f : StepOffset,
                    in filter
                );

                if (collisionInfo.hitInfo.hit)
                {
                    // Physics interaction ---------------------------------------------------------------------------------------
                    if (canPushDynamicRigidbodies)
                    {
                        if (collisionInfo.hitInfo.IsRigidbody)
                        {
                            if (collisionInfo.hitInfo.IsDynamicRigidbody)
                            {
                                bool belongsToGroundRigidbody = false;

                                if (GroundCollider3D != null)
                                    if (GroundCollider3D.attachedRigidbody != null)
                                        if (GroundCollider3D.attachedRigidbody == collisionInfo.hitInfo.rigidbody3D)
                                            belongsToGroundRigidbody = true;


                                if (!belongsToGroundRigidbody)
                                {
                                    bool canPushThisObject = collisionInfo.hitInfo.layer.BelongsToLayerMask(pushableRigidbodyLayerMask);
                                    if (canPushThisObject)
                                    {
                                        // Use the remaining displacement.
                                        position += displacement;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (slideOnWalls)
                    {
                        position += collisionInfo.displacement;
                        displacement -= collisionInfo.displacement;

                        bool blocked = UpdateCollideAndSlideData(
                            collisionInfo,
                            ref slidingPlaneNormal,
                            ref groundPlaneNormal,
                            ref displacement
                        );
                    }
                    else
                    {
                        if (!WallCollision)
                            position += collisionInfo.displacement;

                        break;
                    }
                }
                else
                {
                    position += displacement;
                    break;
                }
            }
        }


        public bool CanEnterGroundedState => !alwaysNotGrounded && forceNotGroundedFrames == 0;

        protected void UnstableCollideAndSlide(ref Vector3 position, Vector3 displacement, float dt)
        {
            HitInfoFilter filter = new(
                PhysicsComponent.CollisionLayerMask,
                forceNotGroundedFrames != 0,
                true
            );


            Vector3 slidePlaneANormal = Vector3.zero;
            Vector3 slidePlaneBNormal = Vector3.zero;

            int iteration = 0;
            while (iteration < CharacterConstants.MaxSlideIterations || displacement == Vector3.zero)
            {
                iteration++;

                CollisionInfo collisionInfo = characterCollisions.CastBody(
                    position,
                    displacement,
                    0f,
                    in filter
                );

                if (collisionInfo.hitInfo.hit)
                {
                    float slopeAngle = Vector3.Angle(Up, collisionInfo.hitInfo.normal);
                    bool stableHit = slopeAngle <= slopeLimit;
                    bool bottomCollision = slopeAngle < 90f;

                    if (collisionInfo.hitInfo.IsRigidbody)
                    {
                        if (collisionInfo.hitInfo.IsKinematicRigidbody)
                        {
                            position += displacement;
                            break;
                        }

                        if (canPushDynamicRigidbodies && collisionInfo.hitInfo.IsDynamicRigidbody)
                        {
                            bool canPushThisObject = collisionInfo.hitInfo.layer.BelongsToLayerMask(pushableRigidbodyLayerMask);
                            if (canPushThisObject)
                            {
                                position += displacement;
                                break;
                            }
                        }
                    }

                    // Fall back to this
                    position += collisionInfo.displacement;
                    displacement -= collisionInfo.displacement;

                    // Determine the displacement vector and store the slide plane A
                    if (slidePlaneANormal == Vector3.zero)
                    {
                        if (preventUnstableClimbing && bottomCollision && !stableHit)
                        {
                            bool isUpwardsDisplacement = (Quaternion.Inverse(transform.rotation) * Vector3.Project(displacement, Up)).y > 0f;

                            if (isUpwardsDisplacement)
                                displacement = Vector3.Project(displacement, Up);
                            else
                                displacement = Vector3.ProjectOnPlane(displacement, collisionInfo.hitInfo.normal);
                        }
                        else
                        {
                            displacement = Vector3.ProjectOnPlane(displacement, collisionInfo.hitInfo.normal);
                        }

                        // store the slide plane A
                        slidePlaneANormal = collisionInfo.hitInfo.normal;
                    }
                    else if (slidePlaneBNormal == Vector3.zero)
                    {
                        slidePlaneBNormal = collisionInfo.hitInfo.normal;
                        Vector3 displacementDirection = Vector3.Cross(slidePlaneANormal, slidePlaneBNormal);
                        displacementDirection.Normalize();
                        displacement = Vector3.Project(displacement, displacementDirection);
                    }
                }
                else
                {
                    position += displacement;
                    break;
                }
            }
        }

        void UnstableProbeGround(ref Vector3 position, float dt)
        {
            if (!CanEnterGroundedState)
            {
                unstableGroundContactTime = 0f;
                PredictedGround = null;
                PredictedGroundDistance = 0f;

                ResetGroundInfo();
                return;
            }

            HitInfoFilter groundCheckFilter = new HitInfoFilter(
                PhysicsComponent.CollisionLayerMask,
                false,
                true
            );

            CollisionInfo collisionInfo = characterCollisions.CheckForGround(
                position,
                StepOffset,
                CharacterConstants.GroundPredictionDistance,
                in groundCheckFilter
            );

            if (collisionInfo.hitInfo.hit)
            {
                PredictedGround = collisionInfo.hitInfo.transform.gameObject;
                PredictedGroundDistance = collisionInfo.displacement.magnitude;

                bool validForGroundCheck = PredictedGroundDistance <= CharacterConstants.GroundCheckDistance;
                if (validForGroundCheck)
                {
                    unstableGroundContactTime += dt;

                    if (CanPerformUnstableGroundDetection(in collisionInfo.hitInfo))
                    {
                        position += collisionInfo.displacement;
                        SetGroundInfo(in collisionInfo, true);
                    }
                }
                else
                {
                    unstableGroundContactTime = 0f;
                    ResetGroundInfo();
                }

            }
            else
            {
                unstableGroundContactTime = 0f;
                PredictedGround = null;
                PredictedGroundDistance = 0f;

                ResetGroundInfo();
            }

            GroundProbingDisplacement = Vector3.zero;
        }

        protected bool CanPerformUnstableGroundDetection(in HitInfo hitInfo)
        {
            if (detectGroundWhileAscending)
                return true;
            else
                return LocalVelocity.y <= 0f || unstableGroundContactTime >= CharacterConstants.MaxUnstableGroundContactTime ||
                    hitInfo.IsRigidbody;
        }

        /// <summary>
        /// Gets the object below the character (only valid if the character is falling). The maximum prediction distance is defined by the constant "GroundPredictionDistance".
        /// </summary>
        public GameObject PredictedGround { get; private set; }

        /// <summary>
        /// Gets the distance to the "PredictedGround".
        /// </summary>
        public float PredictedGroundDistance { get; private set; }


        void SetGroundInfo(in CollisionInfo collisionInfo, bool setGroundContact)
        {
            ProcessNewGround(collisionInfo.hitInfo.transform);
            characterCollisionInfo.SetGroundInfo(in collisionInfo, this, setGroundContact);
        }

        void ResetGroundInfo() => characterCollisionInfo.ResetGroundInfo();

        void ProcessNewGround(Transform newGroundTransform)
        {
            bool isThisANewGround = newGroundTransform != GroundTransform;
            if (isThisANewGround)
            {
                if (newGroundTransform == null)
                {
                    groundRigidbodyComponent = null;
                }
                else
                {
                    if (!groundRigidbodyComponents.TryGetValue(newGroundTransform, out groundRigidbodyComponent))
                    {
                        groundRigidbodyComponent = newGroundTransform.GetComponent<RigidbodyComponent>();
                        if (groundRigidbodyComponent != null)
                        {
                            groundRigidbodyComponents.Add(newGroundTransform, groundRigidbodyComponent);
                        }
                    }
                }
                OnNewGroundEnter?.Invoke();
            }
        }

        bool UpdateCollideAndSlideData(CollisionInfo collisionInfo, ref Vector3 slidingPlaneNormal, ref Vector3 groundPlaneNormal, ref Vector3 displacement)
        {
            Vector3 normal = collisionInfo.hitInfo.normal;

            if (collisionInfo.contactSlopeAngle > slopeLimit || !EvaluateGroundStability(collisionInfo.hitInfo.transform))
            {
                if (slidingPlaneNormal != Vector3.zero)
                {
                    bool acuteAngleBetweenWalls = Vector3.Dot(normal, slidingPlaneNormal) > 0f;

                    if (acuteAngleBetweenWalls)
                        displacement = displacement.DeflectVector(groundPlaneNormal, normal);
                    else
                        displacement = Vector3.zero;

                }
                else
                {
                    displacement = displacement.DeflectVector(groundPlaneNormal, normal);
                }

                slidingPlaneNormal = normal;
            }
            else
            {
                displacement = displacement.ProjectOnTangent(normal, Up);

                groundPlaneNormal = normal;
                slidingPlaneNormal = Vector3.zero;

            }

            return displacement == Vector3.zero;
        }


    }
}