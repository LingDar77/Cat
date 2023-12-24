using SFC.AduioManagement;
using UnityEngine;

namespace SFC.Intergration.Addressables
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
        public void PlaySoundAtPosition(Vector3 position, string reference, float volume = 1)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op => PlaySoundAtPosition(position, op.Result, volume);
        }
        public void PlaySoundFrom(Transform trans, string reference, System.Action<AudioSource> onReadyPlay = null)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op =>
            {
                PlaySoundFrom(trans, op.Result, onReadyPlay);
            };
        }
    }
}