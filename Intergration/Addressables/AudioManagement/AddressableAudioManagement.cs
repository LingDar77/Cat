namespace Cat.Intergration.Addressables.AudioManagement
{
    using System.Threading.Tasks;
    using global::Cat.Utillities;
    using global::Cat.AduioManagement;
    using UnityEngine;
    using Addressables = UnityEngine.AddressableAssets.Addressables;
    public class AddressableAudioManagement : BuiltinAudioManagement, IAudioManagement<string>
    {
        public AudioSource PlaySoundAtPosition(string reference, Vector3 positioin, bool play = true)
        {
            return PlaySoundAtPosition(Addressables.LoadAssetAsync<AudioClip>(reference).WaitForCompletion(), positioin, play);
        }

        public AudioSource PlaySoundAttachedTo(string reference, Transform target, bool play = true)
        {
            return PlaySoundAttachedTo(Addressables.LoadAssetAsync<AudioClip>(reference).WaitForCompletion(), target, play);
        }

        public async Task<AudioSource> PlaySoundAtPositionAsync(string reference, Vector3 positioin, bool play = true)
        {
            var clip = await Addressables.LoadAssetAsync<AudioClip>(reference).Task;
            var source = GetValidAudioSource().Clip(clip);
            source.transform.position = positioin;
            if (play) source.Play();
            return source;
        }

        public async Task<AudioSource> PlaySoundAttachedToAsync(string reference, Transform target, bool play = true)
        {
            var clip = await Addressables.LoadAssetAsync<AudioClip>(reference).Task;
            var source = GetValidAudioSource().Clip(clip);
            source.transform.SetParent(target, false);
            source.transform.localPosition = Vector3.zero;
            if (play) source.Play();
            return source;
        }
    }
}