using System.Collections.Generic;
using SFC.Utillities;
using UnityEngine;

namespace SFC.AduioManagement
{
    /// <summary>
    /// The simplest implementation of an AudioManagementSystem.
    /// <see cref="IAudioManagementSystem"/>
    /// </summary>
    public class BuiltinAudioManagement : MonoBehaviour, IAudioManagementSystem<AudioClip>, ISingletonSystem<BuiltinAudioManagement>
    {
        [field: SerializeField] public int MaxAllocation { get; set; } = 16;
        protected int UsedAllocation { get => usedSources.Count + unusedSources.Count; }
        protected List<AudioSource> unusedSources = new();
        protected HashSet<AudioSource> usedSources = new();

        protected virtual void OnEnable()
        {
            if (ISingletonSystem<BuiltinAudioManagement>.Singleton != null) return;

            ISingletonSystem<BuiltinAudioManagement>.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnDisable()
        {
            if (ISingletonSystem<BuiltinAudioManagement>.Singleton.transform != this) return;
            ISingletonSystem<BuiltinAudioManagement>.Singleton = null;
        }

        public virtual void PlaySoundAtPosition(Vector3 position, AudioClip reference, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(reference, position, volume);
        }
        public virtual void PlaySoundFrom(Transform trans, AudioClip reference, System.Action<AudioSource> onReadyPlay = null)
        {
            var source = GetValidAudioSource();
            onReadyPlay?.Invoke(source);
            source.transform.SetParent(trans, false);
            source.transform.localPosition = Vector3.zero;
            source.clip = reference;
            source.Play();
            usedSources.Add(source);
            this.WaitUntil(
             () => source.isPlaying == false,
             () => ReturnAudioSource(source));
        }
        protected virtual AudioSource GetValidAudioSource()
        {
            if (UsedAllocation >= MaxAllocation)
            {
                Debug.LogWarning("Max allocation reached. Consider increasing the MaxAllocation value.", this);
            }
            if (unusedSources.Count == 0)
            {
                unusedSources.Add(new GameObject("Audio Source", typeof(AudioSource)).GetComponent<AudioSource>());
            }
            var source = unusedSources.RandomElement();
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