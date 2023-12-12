
using UnityEngine;

namespace SFC.AduioManagement
{
    public interface IAudioManagementSystem<ReferenceType> : IGameSystem<IActionProvider>
    {
        /// <summary>
        /// The maximun tracked number of audio sources to use;
        /// </summary>
        int MaxAllocation { get; }
        /// <summary>
        /// Simply play a sound at a positin.
        /// Normaly this will not be counted as a use of allocation,
        /// for it's usually used for short clips.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="reference"></param>
        /// <param name="volume"></param>
        void PlaySoundAtPosition(Vector3 position, ReferenceType reference, float volume = 1f);
        AudioSource AllocateSource();
    }
}
