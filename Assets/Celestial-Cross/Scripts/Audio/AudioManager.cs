using UnityEngine;
using UnityEngine.Audio;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace CelestialCross.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private SoundRegistrySO _soundRegistry;
        
        [Header("Runtime")]
        [SerializeField] private MMSoundManager _feelSoundManager; // Reference if needed, but it's a singleton

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;

            var s = AccountManager.Instance.PlayerAccount.Settings;
            
            // Qualidade Gráfica
            QualitySettings.SetQualityLevel(s.QualityLevel);

            if (MMSoundManager.Instance == null) return;

            // Define volumes no MMSoundManager (que converte internamente para Mixer dB)
            MMSoundManager.Instance.SetVolumeMaster(s.MasterVolume);
            MMSoundManager.Instance.SetVolumeMusic(s.MusicVolume);
            MMSoundManager.Instance.SetVolumeSfx(s.SFXVolume);
            MMSoundManager.Instance.SetVolumeUI(s.SFXVolume); // Usando SFXVolume para UI por enquanto
        }

        public void PlayMusic(SoundKey key, bool loop = true)
        {
            if (key == SoundKey.None || _soundRegistry == null) return;
            
            var mapping = _soundRegistry.GetMapping(key);
            if (mapping == null) return;

            if (mapping.SoundData != null)
            {
                mapping.SoundData.Play(Vector3.zero);
            }
            else if (mapping.Clip != null)
            {
                var options = MMSoundManagerPlayOptions.Default;
                options.Loop = loop;
                options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
                options.Volume = mapping.VolumeMultiplier;
                options.Pitch = mapping.Pitch;
                options.Persistent = true;
                
                MMSoundManagerSoundPlayEvent.Trigger(mapping.Clip, options);
            }
        }
        
        public void StopMusic()
        {
            MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.StopTrack, MMSoundManager.MMSoundManagerTracks.Music);
        }

        public void PlaySFX(SoundKey key, Vector3 position = default)
        {
            if (key == SoundKey.None || _soundRegistry == null) return;

            var mapping = _soundRegistry.GetMapping(key);
            if (mapping == null) return;

            if (mapping.SoundData != null)
            {
                mapping.SoundData.Play(position);
            }
            else if (mapping.Clip != null)
            {
                var options = MMSoundManagerPlayOptions.Default;
                options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
                options.Location = position;
                options.Volume = mapping.VolumeMultiplier;
                options.Pitch = mapping.Pitch;
                
                MMSoundManagerSoundPlayEvent.Trigger(mapping.Clip, options);
            }
        }
        
        public void PlayUI(SoundKey key)
        {
            if (key == SoundKey.None || _soundRegistry == null) return;

            var mapping = _soundRegistry.GetMapping(key);
            if (mapping == null) return;

            if (mapping.SoundData != null)
            {
                mapping.SoundData.Play(Vector3.zero);
            }
            else if (mapping.Clip != null)
            {
                var options = MMSoundManagerPlayOptions.Default;
                options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.UI;
                options.Volume = mapping.VolumeMultiplier;
                options.Pitch = mapping.Pitch;
                
                MMSoundManagerSoundPlayEvent.Trigger(mapping.Clip, options);
            }
        }
    }
}
