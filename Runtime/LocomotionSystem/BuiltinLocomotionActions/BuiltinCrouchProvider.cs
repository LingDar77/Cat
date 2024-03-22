namespace Cat.LocomotionSystem.Actions
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinCrouchProvider : ActionProviderBase
    {
        public float MinimumCrouchHeight = 1f;
        public float CrouchSpeed = 5;
        [Tooltip("The Key to Perform Crouch.")]
        [SerializeField] private InputActionProperty CrouchControl;
        private LocomotionShape shape;
        private float initalCapsuleHeight;
        private float currentHeight;

        protected override void OnEnable()
        {
            base.OnEnable();
            shape = LocomotionSystem.transform.GetComponent<LocomotionShape>();
            initalCapsuleHeight = shape.BodySize.y;
            currentHeight = initalCapsuleHeight;
        }

        public override void BeforeProcess(float deltaTime)
        {
            if (CrouchControl.action.IsPressed())
            {
                currentHeight = Mathf.Lerp(currentHeight, MinimumCrouchHeight, deltaTime * CrouchSpeed);
                LocomotionSystem.SetHeight(currentHeight);
            }
            else
            {
                currentHeight = Mathf.Lerp(currentHeight, initalCapsuleHeight, deltaTime * CrouchSpeed);
                LocomotionSystem.SetHeight(currentHeight);
            }
        }
    }
}
