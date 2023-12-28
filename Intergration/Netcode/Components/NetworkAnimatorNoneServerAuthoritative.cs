#if NETCODE
using Unity.Netcode.Components;

namespace TUI.Intergration.Netcode
{
    public class NetworkAnimatorNoneServerAuthoritative : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
#endif