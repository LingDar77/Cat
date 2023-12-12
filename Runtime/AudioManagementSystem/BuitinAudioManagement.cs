using System.Collections;
using System.Collections.Generic;
using SFC.Utillities;
using UnityEngine;

namespace SFC.AduioManagement
{
    public class BuitinAudioManagement : MonoBehaviour, IAudioManagementSystem<AudioClip>, ISingletonSystem<BuitinAudioManagement>
    {
        [SerializeField] private List<AudioClip> ReferencedClips;
        private HashSet<AudioClip> referencedClips;
        public void OnEnable()
        {
            if (ISingletonSystem<BuitinAudioManagement>.Singleton != null) return;

            ISingletonSystem<BuitinAudioManagement>.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (ISingletonSystem<BuitinAudioManagement>.Singleton.transform != this) return;
            ISingletonSystem<BuitinAudioManagement>.Singleton = null;
        }

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                AudioSource.PlayClipAtPoint(ReferencedClips.Random(), Camera.main.transform.position);
                
            }
        }

        public void PlaySoundAtPosition(Vector3 position, AudioClip reference)
        {
        }
    }
}