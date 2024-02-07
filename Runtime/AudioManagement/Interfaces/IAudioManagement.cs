namespace Cat.AduioManagement
{
    using UnityEngine;
    public interface IAudioManagement<ReferenceType> : ICatSystem<IAudioManagement<ReferenceType>>
    {
        AudioSource PlaySoundAtPosition(ReferenceType reference, Vector3 positioin, bool play = true);
        
        AudioSource PlaySoundAttachedTo(ReferenceType reference, Transform target, bool play = true);
    
        AudioSource LendAudioSource();

        void ReturnAudioSource(AudioSource source);
    }
}