namespace TUI.Intergration.XRIT.KinematicLocomotionSystem
{
    using System.Collections.Generic;
    using TUI.KinematicLocomotionSystem;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;
    using TUI.Utillities;
    using TUI.LocomotioinSystem;

    /// <summary>
    /// The simple implementation of a vr charactor controller by KCC.
    /// The controller is orderless for actions.
    /// </summary>
    [RequireComponent(typeof(KinematicCharacterMotor))]
    public class KinematicLocomotionController : MonoBehaviour, ICharacterController, ILocomotionSystem
    {
        [ReadOnlyInEditor]
        public KinematicCharacterMotor Motor;
        [ReadOnlyInEditor]
        [SerializeField] private Vector3 currentVelocity;
        public Vector3 CurrentVelocity => currentVelocity;
        [ReadOnlyInEditor]
        [SerializeField] private Quaternion currentRotation;
        public Quaternion CurrentRotation => currentRotation;

        public ICollection<IActionProvider> ActionProviders { get => actions; }

        public KinematicLocomotionSystemConfig ControllerConfig = new()
        {
            ShouldDoSimulation = true,
            ShouldDoInterpolate = true,
            ShouldDoCustomInterpolate = true,
            ShouldDoCustomRotationInterpolate = false
        };

        protected HashSet<IActionProvider> actions = new();

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!Motor) Motor = GetComponent<KinematicCharacterMotor>();
        }
#endif
        protected virtual void OnEnable()
        {
            Motor.CharacterController = this;
        }
        protected virtual void OnDisable()
        {
            Motor.CharacterController = null;
        }
        public virtual void RegisterActionProvider(IActionProvider action)
        {
            actions.Add(action);
        }

        public virtual void UnregisterActionProvider(IActionProvider action)
        {
            actions.Remove(action);
        }

        public virtual KinematicLocomotionSystemConfig GetConfig()
        {
            return ControllerConfig;
        }

        public bool IsStableOnGround()
        {
            return Motor.GroundingStatus.IsStableOnGround;
        }

        public void MarkUngrounded()
        {
            Motor.GroundingStatus.IsStableOnGround = false;
        }

        #region KCC Implementation
        public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (Motor.CharacterController == null) return;
            foreach (var action in actions)
            {
                action.ProcessRotation(ref currentRotation, deltaTime);
            }
            this.currentRotation = currentRotation;
        }

        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (Motor.CharacterController == null) return;
            foreach (var action in actions)
            {
                action.ProcessVelocity(ref currentVelocity, deltaTime);
            }
            this.currentVelocity = currentVelocity;
        }
        public virtual void AfterCharacterUpdate(float deltaTime)
        {
            if (Motor.CharacterController == null) return;
            foreach (var action in actions)
            {
                action.AfterProcess(deltaTime);
            }
        }

        public virtual void BeforeCharacterUpdate(float deltaTime)
        {
            if (Motor.CharacterController == null) return;
            foreach (var action in actions)
            {
                action.BeforeProcess(deltaTime);
            }
        }

        public virtual bool IsColliderValidForCollisions(Collider coll)
        {
            var interatable = coll.GetComponentInParent<IXRSelectInteractable>();
            if (interatable != null && interatable.isSelected) return false;
            return true;
        }

        public virtual void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public virtual void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public virtual void PostGroundingUpdate(float deltaTime)
        {
        }

        public virtual void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public virtual bool IsRigidbodyValidForCollisions(Rigidbody rigidbody)
        {
            return true;
        }
        #endregion

    }
}