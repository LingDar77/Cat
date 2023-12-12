using SFC.AduioManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SFC.Intergration.AA
{
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
            DontDestroyOnLoad(gameObject);
        }
        protected override void OnDisable()
        {
            if (ISingletonSystem<AddressableAudioManagement>.Singleton.transform != this) return;
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