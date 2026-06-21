using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Feedbacks;

namespace CelestialCross.Audio
{
    [global::System.Serializable]
    public class SoundMapping
    {
        public SoundKey Key;
        public AudioClip Clip;
        
        [Range(0f, 2f)]
        public float VolumeMultiplier = 1f;
        
        [Range(-3f, 3f)]
        public float Pitch = 1f;
        
        [Tooltip("If set, this will override the Clip and use all settings from this SoundData SO instead")]
        public MMF_MMSoundManagerSoundData SoundData;
    }

    [CreateAssetMenu(fileName = "SoundRegistry", menuName = "Celestial Cross/Audio/Sound Registry", order = 1)]
    public class SoundRegistrySO : ScriptableObject
    {
        public List<SoundMapping> Mappings = new List<SoundMapping>();

        private Dictionary<SoundKey, SoundMapping> _mappingDict;

        public void Initialize()
        {
            _mappingDict = new Dictionary<SoundKey, SoundMapping>();
            foreach (var mapping in Mappings)
            {
                if (!_mappingDict.ContainsKey(mapping.Key))
                {
                    _mappingDict.Add(mapping.Key, mapping);
                }
                else
                {
                    Debug.LogWarning($"[SoundRegistry] Duplicate key found: {mapping.Key}");
                }
            }
        }

        public SoundMapping GetMapping(SoundKey key)
        {
            if (_mappingDict == null) Initialize();

            if (_mappingDict.TryGetValue(key, out var mapping))
            {
                return mapping;
            }
            return null;
        }
    }
}
