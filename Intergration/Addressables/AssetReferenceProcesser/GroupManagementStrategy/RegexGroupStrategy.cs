#if UNITY_EDITOR
namespace TUI.Intergration.Addressables
{
    using System.Text.RegularExpressions;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEngine;

    [CreateAssetMenu(fileName = "RegexGroupStrategy", menuName = "Addressables/Group Management Strategy/Regex Group Strategy", order = 0)]
    public class RegexGroupStrategy : GroupManagementStrategyBase
    {
        public string expression;
        public RegexOptions options;

        public override bool Match(string path, AddressableAssetGroup group)
        {
            return new Regex(expression, options).IsMatch(path);
        }
    }
}
#endif