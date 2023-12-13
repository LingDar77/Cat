#if UNITY_EDITOR && ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace SFC.Intergration.AA
{
    public interface IGroupManagementStrategy
    {
        bool Match(string path, AddressableAssetGroup group);
    }

    public abstract class GroupManagementStrategyBase : ScriptableObject, IGroupManagementStrategy
    {
        public virtual bool Match(string path, AddressableAssetGroup group)
        {
            return false;
        }
    }
}
#endif