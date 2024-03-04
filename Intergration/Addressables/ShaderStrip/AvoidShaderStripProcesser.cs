#if UNITY_EDITOR
namespace Cat.Intergration.Addressables
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor.AddressableAssets;
    using UnityEditor.Build;
    using UnityEditor.Rendering;
    using UnityEngine;
    public class AvoidShaderStripProcesser : IPreprocessShaders
    {
        public int callbackOrder { get; } = 0;
        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            // var aa = AddressableAssetSettingsDefaultObject.GetSettings(false);
            
        }

    }
}
#endif
