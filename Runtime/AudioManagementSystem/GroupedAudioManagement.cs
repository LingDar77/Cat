using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TUI.AduioManagement
{
    public class GroupedAudioManagement : BuiltinAudioManagement, ISingletonSystem<GroupedAudioManagement>
    {
        [System.Serializable]
        public class AudioGroup
        {
            public string GroupName = "Default";
            public List<AudioClip> GroupMembers = new();
            public float GroupVolume = 1f;
        }

        public List<AudioGroup> AudioGroups = new();
        public Dictionary<string, HashSet<AudioClip>> GroupMap = new();
        public Dictionary<string, float> GroupVolumes = new();
        protected override void OnEnable()
        {
            if (ISingletonSystem<GroupedAudioManagement>.Singleton != null) return;
            ISingletonSystem<GroupedAudioManagement>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);
            UpdateAllGlobalConfig();
        }
        protected override void OnDisable()
        {
            if (ISingletonSystem<GroupedAudioManagement>.Singleton.transform != this) return;
            ISingletonSystem<GroupedAudioManagement>.Singleton = null;
        }

        protected virtual void UpdateAllGlobalConfig()
        {
            GroupMap.Clear();
            GroupVolumes.Clear();
            foreach (var group in AudioGroups)
            {
                GroupMap.Add(group.GroupName, group.GroupMembers.ToHashSet());
                GroupVolumes.Add(group.GroupName, group.GroupVolume);
                foreach (var source in usedSources)
                {
                    if (!group.GroupMembers.Contains(source.clip)) continue;

                    source.volume = group.GroupVolume;
                }
            }
        }

        protected virtual void UpdateSingleGroupConfig(string groupName)
        {
            foreach (var source in usedSources)
            {
                if (!GroupMap[groupName].Contains(source.clip)) continue;

                source.volume = GroupVolumes[groupName];
            }
        }
        public void InitGlobalGroupConfig(List<AudioGroup> audioGroups)
        {
            AudioGroups = audioGroups;
            UpdateAllGlobalConfig();
        }
        public void SetGlobalGroupVolume(string groupName, float volume)
        {
            GroupVolumes[groupName] = volume;
            UpdateSingleGroupConfig(groupName);
        }

        public override void PlaySoundAtPosition(Vector3 position, AudioClip reference, float volume = 1)
        {
            if (volume == 1)
            {
                foreach (var (name, group) in GroupMap)
                {
                    if (!group.Contains(reference)) continue;
                    volume = GroupVolumes[name];
                    break;
                }
            }
            base.PlaySoundAtPosition(position, reference, volume);
        }
        public override void PlaySoundFrom(Transform trans, AudioClip reference, Action<AudioSource> onReadyPlay = null)
        {
            foreach (var (name, group) in GroupMap)
            {
                if (!group.Contains(reference)) continue;
                onReadyPlay += source => source.volume = GroupVolumes[name];
                break;
            }
            base.PlaySoundFrom(trans, reference, onReadyPlay);
        }
    }
}