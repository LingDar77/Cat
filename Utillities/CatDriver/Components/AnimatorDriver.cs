namespace Cat.Utillities
{
    using UnityEngine;
    public class AnimatorDriver : CatDriver<Vector3>
    {
        [SerializeField] private Animator Animator;
#if UNITY_EDITOR
        [SerializeField] private string ParameterName;
#endif
        [ReadOnlyInEditor] public int ParamHash;

        private void OnValidate()
        {
            this.EnsureComponent(ref Animator);
#if UNITY_EDITOR
            ParamHash = Animator.StringToHash(ParameterName);
#endif
        }
        public override void Drive(Vector3 value)
        {
            if (Animator == null) return;

#if UNITY_EDITOR
            Animator.SetFloat(ParameterName, value.magnitude);
#else
            Animator.SetFloat(ParamHash, value.magnitude);
#endif
        }

    }
}