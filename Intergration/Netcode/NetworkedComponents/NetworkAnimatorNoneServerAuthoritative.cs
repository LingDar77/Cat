using Unity.Netcode.Components;

namespace SFC.Intergration.Netcode
{
    public class NetworkAnimatorNoneServerAuthoritative : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}