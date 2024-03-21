namespace Cat.AduioManagement
{
    using System.Collections;
    using Cat.Utilities;
    using UnityEngine;
    public class BuiltinAudioProxy : MonoBehaviour
    {
        [SerializeField] private BuiltinAudioManagement system;
        [SerializeField] private AudioClip clip;
        [SerializeField] private float time = .5f;
        [SerializeField] private float volume = 1f;

        private void Start()
        {
            system = ISingletonSystem<BuiltinAudioManagement>.GetChecked();
            ISingletonSystem<BuiltinAudioManagement>.GetChecked().PlaySoundAtPosition(clip, transform.position, false).SpatialBlend(1).Loop(true).PlayAWhile(4);
        }

        [ContextMenu("PlaySoundOnce")]
        public void PlaySoundOnce()
        {
            system.PlaySoundAtPosition(clip, transform.position)
                .SpatialBlend(1)
                .Volume(volume);
        }

        [ContextMenu("PlaySoundInterval")]
        public void PlaySoundInterval()
        {
            StartCoroutine(DoPlaySoundInterval());
        }

        private IEnumerator DoPlaySoundInterval()
        {
            while (clip != null)
            {
                system.PlaySoundAtPosition(clip, transform.position)
                    .SpatialBlend(1)
                    .Volume(volume);
                yield return CoroutineHelper.GetWaitForSeconds(time);
            }
        }

    }
}