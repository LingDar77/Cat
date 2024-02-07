#if UNITY_EDITOR
namespace Cat.Utillities
{
    using UnityEditor;

    public class ConfigWindow<ConfigType> : EditorWindow where ConfigType : ConfigableObject<ConfigType>
    {
        protected Editor editor;
        protected virtual void OnEnable()
        {
            editor = Editor.CreateEditor(ConfigableObject<ConfigType>.Get(), typeof(HideScriptEditor));
        }

    }
}
#endif