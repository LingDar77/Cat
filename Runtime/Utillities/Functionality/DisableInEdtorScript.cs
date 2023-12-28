using UnityEngine;
namespace TUI.Utillities
{
    public class DisableInEdtorScript : MonoBehaviour
    {
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (enabled && !Application.isPlaying) enabled = false;
#endif
        }
    }
}