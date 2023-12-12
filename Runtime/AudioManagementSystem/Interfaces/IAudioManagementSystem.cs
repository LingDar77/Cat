
using UnityEngine;

namespace SFC.AduioManagement
{
    public interface IAudioManagementSystem<ReferenceType> : IGameSystem<IActionProvider>
    {
        void PlaySoundAtPosition(Vector3 position, ReferenceType reference);
    }
}
