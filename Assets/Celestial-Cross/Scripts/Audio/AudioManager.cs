using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace CelestialCross.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSourcePrefab;

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
            
            // Define volumes no Mixer (escala logarítmica recomendada: 20 * log10(vol))
            SetMixerVolume("MasterVol", s.MasterVolume);
            SetMixerVolume("MusicVol", s.MusicVolume);
            SetMixerVolume("SFXVol", s.SFXVolume);

            // Qualidade Gráfica
            QualitySettings.SetQualityLevel(s.QualityLevel);
        }

        private void SetMixerVolume(string parameter, float normalizedValue)
        {
            if (mainMixer == null) return;
            // Converte 0-1 para -80dB a 0dB
            float db = normalizedValue > 0.0001f ? Mathf.Log10(normalizedValue) * 20 : -80f;
            mainMixer.SetFloat(parameter, db);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip, float pitchVar = 0.1f)
        {
            if (clip == null) return;
            // Simples por enquanto, futuramente usar um Pool
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, AccountManager.Instance.PlayerAccount.Settings.SFXVolume);
        }
    }
}
