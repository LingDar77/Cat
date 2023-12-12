using System.Collections.Generic;
using UnityEngine;

namespace SFC.AduioManagement
{
    public class BuiltinAudioManagement : MonoBehaviour, IAudioManagementSystem<AudioClip>, ISingletonSystem<BuiltinAudioManagement>
    {
        [field: SerializeField] public int MaxAllocation { get; set; } = 16;
        [SerializeField] private List<AudioClip> ReferencedClips;

        public void OnEnable()
        {
            if (ISingletonSystem<BuiltinAudioManagement>.Singleton != null) return;

            ISingletonSystem<BuiltinAudioManagement>.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (ISingletonSystem<BuiltinAudioManagement>.Singleton.transform != this) return;
            ISingletonSystem<BuiltinAudioManagement>.Singleton = null;
        }

        public void PlaySoundAtPosition(Vector3 position, AudioClip reference)
        {
            AudioSource.PlayClipAtPoint(reference, position);
        }
    }
}