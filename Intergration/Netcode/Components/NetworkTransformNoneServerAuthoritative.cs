#if NETCODE
using Unity.Netcode.Components;

namespace SFC.Intergration.Netcode
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