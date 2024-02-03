#if UNITY_EDITOR
namespace TUI.Intergration.Addressables
{
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

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