namespace Cat.AduioManagement
{
    using System.Collections;
    using System.Collections.Generic;
    using Cat.Utilities;
    using UnityEngine;
    public class BuiltinAudioManagement : SingletonSystemBase<BuiltinAudioManagement>, IAudioManagement<AudioClip>
    {
        [ReadOnlyInEditor]
        [SerializeField] private int CurrentAllocation;
        [SerializeField] private bool IgnoreMaxAllocation = false;
        [field: SerializeField] private int MaxAllocation { get; set; } = 16;

        private readonly LinkedHashSet<AudioSource> used = new();
        private readonly Queue<AudioSource> unused = new();
        private readonly List<AudioSource> buffer = new();

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(CheckUsedSources());
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
        }

        protected IEnumerator CheckUsedSources()
        {
            yield return CoroutineHelper.GetNextUpdate();
            while (true)
            {
                foreach (var source in used)
                {
                    if (source.isPlaying) continue;

                    buffer.Add(source);
                }
                foreach (var source in buffer)
                {
                    used.Remove(source);
                    unused.Enqueue(source);
                    source.transform.SetParent(transform);
                }
                buffer.Clear();
                yield return CoroutineHelper.GetNextUpdate();
            }
        }

        public AudioSource PlaySoundAtPosition(AudioClip clip, Vector3 positioin, bool play = true)
        {
            var source = GetValidAudioSource().Clip(clip);
            source.transform.position = positioin;
            if (play) source.Play();
            return source;
        }

        public AudioSource PlaySoundAttachedTo(AudioClip clip, Transform target, bool play = true)
        {
            var source = GetValidAudioSource().Clip(clip);
            source.transform.SetParent(target, false);
            source.transform.localPosition = Vector3.zero;
            if (play) source.Play();
            return source;
        }

        public AudioSource LendAudioSource()
        {
            return GetValidAudioSource(false);
        }

        public void ReturnAudioSource(AudioSource source)
        {
            unused.Enqueue(source);
        }

        protected virtual AudioSource GetValidAudioSource(bool markAsUsed = true)
        {
            AudioSource source;
            if (unused.Count != 0)
            {
                source = unused.Dequeue();
            }
            else
            {
                source = AllocateNewAudioSource();
            }
            if (markAsUsed) used.Add(source);
            return source;
        }

        protected virtual AudioSource AllocateNewAudioSource()
        {
            AudioSource source;
            if (!IgnoreMaxAllocation && CurrentAllocation >= MaxAllocation)
            {
                source = used.GetOldestItem();
                source.Stop();
                used.Remove(source);
            }
            else
            {
                source = new GameObject($"AudioSource.{CurrentAllocation++}", typeof(AudioSource)).GetComponent<AudioSource>();
            }
            source.transform.SetParent(transform);
            return source;
        }

        public void StopAll()
        {
            StopAllCoroutines();
            foreach (var source in used)
            {
                unused.Enqueue(source);
                source.Stop();
            }
            used.Clear();
        }
    }
}