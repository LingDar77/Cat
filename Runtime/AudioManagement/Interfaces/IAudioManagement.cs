using UnityEngine;
using UnityEngine.Audio;

namespace TUI.AduioManagement
{
    /// <summary>
    /// Definition of an audio management system that allocate certain amount of audio sources 
    /// and manage them to correctlly play audio resources.
    /// * The system hides the load resources actions and allocated audio sources actions
    /// * so users can focus on how and what to play in front of it.
    /// </summary>
    /// <typeparam name="ReferenceType">The type of audio asset reference. </typeparam> 
    public interface IAudioManagement<ReferenceType> : IGameSystem<IAudioManagement<ReferenceType>>
    {
        /// <summary>
        /// The maximun tracked number of audio sources to use;
        /// * When reached the limit while can not reuse any audio source,
        /// * the system will allocate more audio sources to satisfy user's request,
        /// * but the part may cause performent problems.
        /// * A warning will log this problem.
        /// * Note that every allocated audio source may be recycled by the system
        /// * after playing ended, you should not try to reuse it manully.
        /// </summary>
        int MaxAllocation { get; }
        /// <summary>
        /// Is the system allowed to replace the nearest to end audio source
        /// to avoid more allocation that is exceeded the max allocation.
        /// But this operation will cost some time to find the correct one,
        /// it may cause some performence drop.
        /// * Enable this may cause some problems, for the extra allocation 
        /// * will force stop a playing audio source.
        /// </summary>
        bool ReplaceNearestToEnd { get; }

        event System.Action<AudioClip> OnCompletePlay;


        /// <summary>
        /// Simply play a sound at a positin.
        /// * Normaly this will not be counted as a use of allocation,
        /// * for it's usually used for short clips.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="reference"></param>
        /// <param name="volume"></param>
        void PlaySoundAtPosition(Vector3 position, ReferenceType reference, float volume = 1f, System.Action<AudioSource> onReadyPlay = null);
        void PlaySoundAtPosition(Vector3 position, ReferenceType reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null);


        void AttatchAudioSourceTo(Transform trans, ReferenceType reference, System.Action<AudioSource> onReadyPlay = null);
        void AttatchAudioSourceTo(Transform trans, ReferenceType reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null);

        AudioSource[] Query(ReferenceType reference);
        AudioSource[] Query(AudioMixerGroup mixerGroup);
        AudioSource[] Query(System.Func<AudioSource, bool> predicate);


        #region Obsolete
        /// <summary>
        /// Play a sound from a transfrom, 
        /// this may cause allocation or reuse audio source.
        /// * Note that you can manully stop the audio source.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="reference"></param>
        /// <param name="onReadyPlay"></param>
        [System.Obsolete]
        void PlaySoundFrom(Transform trans, ReferenceType reference, System.Action<AudioSource> onReadyPlay = null);
        [System.Obsolete]
        void PlaySoundFrom(Transform trans, ReferenceType reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null);
        #endregion

    }
}
