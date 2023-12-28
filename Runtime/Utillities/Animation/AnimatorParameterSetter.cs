using UnityEngine;

namespace TUI.Utillities
{
    public class AnimatorParameterSetter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string animatorParameterName;
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }
#endif
        public void SetFloatParam(string name, float value)
        {
            animator.SetFloat(name, value);
        }
        public void SetBoolParam(string name, bool value)
        {
            animator.SetBool(name, value);
        }
        public void SetFloatParam(float value)
        {
            animator.SetFloat(animatorParameterName, value);
        }
        public void SetBoolParam(bool value)
        {
            animator.SetBool(animatorParameterName, value);
        }
        public void SetTriggerParam(string name)
        {
            animator.SetTrigger(name);
        }
    }
}
