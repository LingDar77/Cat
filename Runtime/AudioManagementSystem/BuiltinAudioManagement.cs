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
        [EditorReadOnly] public int CurrentAllocation = 0;
        [field: SerializeField] public bool ReplaceLastAllocated { get; set; } = false;

        protected List<AudioSource> unusedSources = new();
        protected HashSet<AudioSource> usedSources = new();
        protected Dictionary<AudioSource, Coroutine> coroutines = new();
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
            Coroutine coroutine;
            coroutine = this.WaitUntil(
             () => source.isPlaying == false,
             () => ReturnAudioSource(source));
            coroutines.Add(source, coroutine);
        }
        protected virtual AudioSource GetValidAudioSource()
        {
            if (CurrentAllocation >= MaxAllocation)
            {
                Debug.LogWarning($"Max allocation reached( {CurrentAllocation} allocated ). Consider increasing the MaxAllocation value.", this);
                if (ReplaceLastAllocated && usedSources.Count != 0)
                {
                    AudioSource last = null;
                    usedSources.RemoveWhere(source =>
                    {
                        last = source;
                        return true;
                    });
                    if (coroutines.ContainsKey(last))
                    {
                        StopCoroutine(coroutines[last]);
                        coroutines.Remove(last);
                    }
                    last.Stop();
                    return last;
                }
            }
            if (unusedSources.Count == 0)
            {
                unusedSources.Add(new GameObject("Audio Source", typeof(AudioSource)).GetComponent<AudioSource>());
                ++CurrentAllocation;
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
            coroutines.Remove(source);
            source.transform.SetParent(transform, false);
        }
    }
}