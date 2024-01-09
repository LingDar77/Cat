using System.Collections.Generic;
using TUI.Utillities;
using UnityEngine;
using UnityEngine.Audio;

namespace TUI.AduioManagement
{
    /// <summary>
    /// The simplest implementation of an AudioManagementSystem.
    /// <see cref="IAudioManagementSystem"/>
    /// </summary>
    public class BuiltinAudioManagement : SingletonSystemBase<BuiltinAudioManagement>, IAudioManagementSystem<AudioClip>
    {
        [EditorReadOnly] public int CurrentAllocation = 0;
        [field: SerializeField] public int MaxAllocation { get; set; } = 16;
        [field: SerializeField] public bool ReplaceNearestToEnd { get; set; } = false;

        protected List<AudioSource> unusedSources = new();
        protected HashSet<AudioSource> usedSources = new();
        protected Dictionary<int, Coroutine> coroutines = new();

        public virtual void PlaySoundAtPosition(Vector3 position, AudioClip reference, float volume = 1f, System.Action<AudioSource> onReadyPlay = null)
        {
            PlaySoundAtPosition(position, reference, null, onReadyPlay += source => source.volume = volume);
        }
        public void PlaySoundAtPosition(Vector3 position, AudioClip reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null)
        {
            var source = GetValidAudioSource();
            source.transform.SetParent(null, false);
            source.transform.position = position;
            source.outputAudioMixerGroup = group;
            onReadyPlay?.Invoke(source);
            source.clip = reference;
            source.Play();
            usedSources.Add(source);
            Coroutine coroutine;
            coroutine = CoroutineHelper.WaitUntil(
            () => source.isPlaying == false,
            () => ReturnAudioSource(source));
            coroutines.Add(source.GetHashCode(), coroutine);
        }
        public virtual void PlaySoundFrom(Transform trans, AudioClip reference, System.Action<AudioSource> onReadyPlay = null)
        {
            PlaySoundFrom(trans, reference, null, onReadyPlay);
        }
        public void PlaySoundFrom(Transform trans, AudioClip reference, AudioMixerGroup group, System.Action<AudioSource> onReadyPlay = null)
        {
            var source = GetValidAudioSource();
            source.transform.SetParent(trans, false);
            source.transform.localPosition = Vector3.zero;
            source.outputAudioMixerGroup = group;
            onReadyPlay?.Invoke(source);
            source.clip = reference;
            source.Play();
            usedSources.Add(source);
            Coroutine coroutine;
            coroutine = CoroutineHelper.WaitUntil(
            () => source.isPlaying == false,
            () => ReturnAudioSource(source));
            coroutines.Add(source.GetHashCode(), coroutine);
        }
        protected virtual AudioSource GetValidAudioSource()
        {
            if (CurrentAllocation >= MaxAllocation && unusedSources.Count == 0)
            {
                if (ReplaceNearestToEnd && usedSources.Count != 0)
                {
                    AudioSource last = SelectNearestEndSource();
                    var hashcode = last.GetHashCode();
                    usedSources.Remove(last);
                    if (coroutines.ContainsKey(hashcode))
                    {
                        StopCoroutine(coroutines[hashcode]);
                        coroutines.Remove(hashcode);
                    }
                    last.Stop();
                    return last;
                }
                Debug.LogWarning($"Max allocation reached( {CurrentAllocation} allocated ). Consider increasing the MaxAllocation value.", this);
            }
            if (unusedSources.Count == 0)
            {
                unusedSources.Add(new GameObject("Audio Source", typeof(AudioSource)).GetComponent<AudioSource>());
#if UNITY_EDITOR
                ++CurrentAllocation;
#endif
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
            coroutines.Remove(source.GetHashCode());
            source.transform.SetParent(transform, false);
        }
        protected virtual AudioSource SelectNearestEndSource()
        {
            float nearestProgress = 0;
            AudioSource nearestSource = null;
            foreach (var source in usedSources)
            {
                if (!source.isPlaying)
                {
                    nearestSource = source;
                    break;
                }
                var progress = source.time / source.clip.length;
                if (progress > nearestProgress)
                {
                    nearestProgress = progress;
                    nearestSource = source;
                }
            }
            return nearestSource;
        }

    }
}