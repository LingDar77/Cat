using System.Collections.Generic;
using SFC.AduioManagement;
using SFC.Utillities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SFC.Intergration.AA
{
    /// <summary>
    /// The basic addressable impmentation for AudioManagementSystem.
    /// <see cref="IAudioManagementSystem"/>
    /// </summary>
    public class AddressableAudioManagement : MonoBehaviour, IAudioManagementSystem<string>, ISingletonSystem<AddressableAudioManagement>
    {
        [field: SerializeField] public int MaxAllocation { get; set; } = 16;
        protected List<AudioSource> unusedSources = new();
        protected HashSet<AudioSource> usedSources = new();

        protected virtual void OnEnable()
        {
            if (ISingletonSystem<AddressableAudioManagement>.Singleton != null) return;

            ISingletonSystem<AddressableAudioManagement>.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDisable()
        {
            if (ISingletonSystem<AddressableAudioManagement>.Singleton.transform != this) return;
            ISingletonSystem<AddressableAudioManagement>.Singleton = null;
        }

        public void PlaySoundAtPosition(Vector3 position, string reference, float volume = 1)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op => AudioSource.PlayClipAtPoint(op.Result, position, volume);
        }

        public void PlaySoundFrom(Transform trans, string reference, System.Action<AudioSource> onReadyPlay = null)
        {
            Addressables.LoadAssetAsync<AudioClip>(reference).Completed += op =>
            {
                var source = GetValidAudioSource();
                onReadyPlay?.Invoke(source);
                source.transform.SetParent(trans, false);
                source.transform.localPosition = Vector3.zero;
                source.clip = op.Result;
                source.Play();
                usedSources.Add(source);
                this.WaitUntil(
                    () => source.isPlaying == false,
                    () => ReturnAudioSource(source));
            };
        }

        protected virtual AudioSource GetValidAudioSource()
        {
            if (usedSources.Count + unusedSources.Count >= MaxAllocation)
            {
                Debug.LogWarning("Max allocation reached. Consider increasing the MaxAllocation value.", this);
            }
            if (unusedSources.Count == 0)
            {
                unusedSources.Add(new GameObject("Audio Source", typeof(AudioSource)).GetComponent<AudioSource>());
            }
            var source = unusedSources.Random();
            unusedSources.Remove(source);
            usedSources.Add(source);
            return source;
        }
        protected virtual void ReturnAudioSource(AudioSource source)
        {
            usedSources.Remove(source);
            unusedSources.Add(source);
            source.transform.SetParent(transform, false);
        }

    }
}