namespace Cat.Utillities
{
    using UnityEngine;
    public class AnimatorDriver : MonoBehaviour, IVector3Driver
    {
        [SerializeField] private Animator Animator;
        [SerializeField] private string ParameterName;

        private int paramHash;

        private void OnValidate()
        {
            this.EnsureComponent(ref Animator);
        }
        private void OnEnable()
        {
            paramHash = Animator.StringToHash(ParameterName);
        }
        public void Drive(Vector3 value)
        {
            if (Animator == null) return;

#if UNITY_EDITOR
            Animator.SetFloat(ParameterName, value.magnitude);
#else
            animator.SetFloat(paramHash, value.magnitude);
#endif
        }
    }
}