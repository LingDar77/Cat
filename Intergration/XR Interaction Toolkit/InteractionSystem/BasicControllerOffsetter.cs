#if XRIT
using UnityEngine;

namespace SFC.Intergration.XRIT.InteractionSystem
{
    public class BasicControllerOffsetter : MonoBehaviour
    {
        [InterfaceRequired(typeof(IRotateBiasable))]
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