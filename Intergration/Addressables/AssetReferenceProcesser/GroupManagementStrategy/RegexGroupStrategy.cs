#if UNITY_EDITOR && ADDRESSABLES
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;


namespace TUI.Intergration.Addressables
{

    [CreateAssetMenu(fileName = "RegexGroupStrategy", menuName = "SaltyFishContainer/Addressables/GroupManagementStrategy/RegexGroupStrategy", order = 0)]
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