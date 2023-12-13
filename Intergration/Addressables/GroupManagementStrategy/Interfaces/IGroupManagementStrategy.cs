#if UNITY_EDITOR && ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;

namespace SFC.Intergration.AA
{
    public interface IGroupManagementStrategy
    {
        bool Match(string path, AddressableAssetGroup group);
    }
}
#endif