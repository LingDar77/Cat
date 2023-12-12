
using UnityEngine;

namespace SFC.AduioManagement
{
    public interface IAudioManagementSystem<ReferenceType> : IGameSystem<IActionProvider>
    {
        /// <summary>
        /// The maximun tracked number of audio sources to use;
        /// * When reached the limit while can not reuse any audio source,
        /// * the system will allocate more audio sources to satisfy user's request,
        /// * but the part may cause performent problems.
        /// * A warning will log this problem
        /// </summary>
        int MaxAllocation { get; }
        /// <summary>
        /// Simply play a sound at a positin.
        /// * Normaly this will not be counted as a use of allocation,
        /// * for it's usually used for short clips.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="reference"></param>
        /// <param name="volume"></param>
        void PlaySoundAtPosition(Vector3 position, ReferenceType reference, float volume = 1f);
        /// <summary>
        /// Play a sound from a transfrom, 
        /// this may cause allocation or reuse audio source.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="reference"></param>
        /// <param name="onReadyPlay"></param>
        void PlaySoundFrom(Transform trans, ReferenceType reference, System.Action<AudioSource> onReadyPlay = null);

    }
}
