namespace Cat.Utillities
{
    using UnityEngine;
    public class AnimatorDriver : MonoBehaviour, IVector3Driver
    {
        [SerializeField] private Animator Animator;
#if UNITY_EDITOR
        [SerializeField] private string ParameterName;
#endif
        [ReadOnlyInEditor] public int paramHash;

        private void OnValidate()
        {
            this.EnsureComponent(ref Animator);
#if UNITY_EDITOR
            paramHash = Animator.StringToHash(ParameterName);
#endif
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