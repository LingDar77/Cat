using TUI.AduioManagement;
using UnityEngine;
using UnityEngine.Audio;

namespace TUI.Intergration.Addressables
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;
    /// <summary>
    /// The basic addressable impmentation for AudioManagementSystem.
    /// <see cref="IAudioManagementSystem"/>
    /// </summary>
    public class AddressableAudioManagement : BuiltinAudioManagement, IAudioManagementSystem<string>, ISingletonSystem<AddressableAudioManagement>
    {
        protected override void OnEnable()
        {
            if (ISingletonSystem<AddressableAudioManagement>.Singleton != null) return;

            ISingletonSystem<AddressableAudioManagement>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        protected override void OnDisable()
        {
            if (ISingletonSystem<AddressableAudioManagement>.Singleton.transform != transform) return;
            ISingletonSystem<AddressableAudioManagement>.Singleton = null;
        }

        public void PlaySoundAtPosition(Vector3 position, string reference, float volume = 1, System.Action<AudioSource> onReadyPlay = null)
        {
            PlaySoundAtPosition(position, reference, null, onReadyPlay += source => source.volume = volume);
        }

        public void PlaySoundAtPosition(Vector3 position, string reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op => PlaySoundAtPosition(position, op.Result, group);
        }

        public void PlaySoundFrom(Transform trans, string reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op =>
            {
                PlaySoundFrom(trans, op.Result, null, onReadyPlay);
            };
        }

        public void PlaySoundFrom(Transform trans, string reference, System.Action<AudioSource> onReadyPlay = null)
        {
            PlaySoundFrom(trans, reference, null, onReadyPlay);
        }

        public void AttatchAudioSourceTo(Transform trans, string reference, System.Action<AudioSource> onReadyPlay = null)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op =>
           {
               PlaySoundFrom(trans, op.Result, null, onReadyPlay);
           };
        }

        public void AttatchAudioSourceTo(Transform trans, string reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null)
        {
            PlaySoundFrom(trans, reference, null, onReadyPlay);
        }

        public AudioSource[] Query(string reference)
        {
            return Query(source => source.clip.name == reference);
        }
    }
}