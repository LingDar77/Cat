using System.Collections;
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
        [SerializeField] private AudioClip[] ReferencedClips;
        [field: SerializeField] public int MaxAllocation { get; set; } = 16;
        protected List<AudioSource> unusedSources = new();
        protected HashSet<AudioSource> usedSources = new();

        public virtual void OnEnable()
        {
            if (ISingletonSystem<BuiltinAudioManagement>.Singleton != null) return;

            ISingletonSystem<BuiltinAudioManagement>.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public virtual void OnDisable()
        {
            if (ISingletonSystem<BuiltinAudioManagement>.Singleton.transform != this) return;
            ISingletonSystem<BuiltinAudioManagement>.Singleton = null;
        }

        public virtual void PlaySoundAtPosition(Vector3 position, AudioClip reference, float volume = 1f)
        {
            AudioSource.PlayClipAtPoint(reference, position, volume);
        }
        public void PlaySoundFrom(Transform trans, AudioClip reference, float volume = 1)
        {
            var source = GetValidAudioSource();
            source.transform.SetParent(trans, false);
            source.transform.localPosition = Vector3.zero;
            source.clip = reference;
            source.volume = volume;
            source.Play();
            usedSources.Add(source);
            Coroutine.WaitUntil(this,
             () => source.isPlaying == false,
             () => ReturnAudioSource(source));
        }
        protected AudioSource GetValidAudioSource()
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
        protected void ReturnAudioSource(AudioSource source)
        {
            usedSources.Remove(source);
            unusedSources.Add(source);
            source.transform.SetParent(transform, false);
        }
    }
}