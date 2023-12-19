
using SFC.SDKManagementSystem;

namespace SFC.SDKProvider
{
    public interface ISDKProvider : IEnabledSetable, ITransformGetable
    {

        /// <summary>
        /// If the provider matches the systemID, the provider will be activated.
        /// </summary>
        /// <param name="systemID"> the system identifier ID, which is given by Input System </param>
        /// <returns></returns>
        bool Match(string systemID);
    }
}