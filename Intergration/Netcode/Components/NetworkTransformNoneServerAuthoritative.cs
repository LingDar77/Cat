#if NETCODE
using Unity.Netcode.Components;

namespace TUI.Intergration.Netcode
{
    public class NetworkTransformNoneServerAuthoritative : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
#endif