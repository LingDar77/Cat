#if XRIT
using UnityEngine;

namespace SFC.Intergration.XRIT.InteractionSystem
{
    public class BasicControllerOffsetter : MonoBehaviour
    {
        private IRotateBiasable biasable;

        private void Awake()
        {
            biasable = transform.root.GetComponentInChildren<IRotateBiasable>();
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