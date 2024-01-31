#if XRIT
namespace TUI.Intergration.XRIT.InteractionSystem
{
    using TUI.LocomotionSystem;
    using UnityEngine;
    public class BasicControllerOffsetter : MonoBehaviour
    {
        [ImplementedInterface(typeof(IRotateBiasable))]
        [SerializeField] private Object BiasableImplement;
        private IRotateBiasable biasable;

        private void OnEnable()
        {
            biasable = BiasableImplement as IRotateBiasable;
        }

        private void FixedUpdate()
        {
            LateUpdate();
        }
        private void LateUpdate()
        {
            if (!biasable.Initialized) return;
            transform.rotation = biasable.Bias;
        }
    }
}
#endif