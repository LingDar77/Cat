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
        protected HashSet<AudioSource> unusedSources = new();
        protected HashSet<AudioSource> usedSources = new();
        [ContextMenu("Test")]
        private void Test()
        {
            var source = AllocateSource();
            source.playOnAwake = false;
            source.clip = ReferencedClips.Random();
            source.PlayTracked(this, () =>
            {
                Debug.Log("Play ended.");
            });
        }
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

        public AudioSource AllocateSource()
        {
            var obj = new GameObject("audio source", typeof(AudioSource));
            obj.transform.SetParent(transform);
            return obj.GetComponent<AudioSource>();
        }
    }
}