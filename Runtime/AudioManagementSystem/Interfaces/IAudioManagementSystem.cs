
using UnityEngine;

namespace SFC.AduioManagement
{
    public interface IAudioManagementSystem<ReferenceType> : IGameSystem<IActionProvider>
    {
        /// <summary>
        /// The allow number of audio sources to use;
        /// </summary>
        int MaxAllocation { get; }
        /// <summary>
        /// Simply play a sound at a positin.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="reference"></param>
        void PlaySoundAtPosition(Vector3 position, ReferenceType reference);
    }
}
