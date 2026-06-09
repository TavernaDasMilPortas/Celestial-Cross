using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace CelestialCross.Settings.UI
{
    public class SettingsSceneUI : MonoBehaviour
    {
        [Header("Profile Section")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField birthDateInput; // Simples texto por enquanto
        [SerializeField] private Image profileIconImage;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Audio Section")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("System Section")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fpsToggle;
        [SerializeField] private TextMeshProUGUI unityIdText;

        [Header("Buttons")]
        [SerializeField] private Button saveBtn;
        [SerializeField] private Button backBtn;

        private void Start()
        {
            LoadCurrentValues();
            
            saveBtn.onClick.AddListener(SaveSettings);
            backBtn.onClick.AddListener(GoBack);

            // Listeners para feedback em tempo real (opcional)
            masterSlider.onValueChanged.AddListener((v) => { /* AudioMixer logic later */ });
        }

        private void LoadCurrentValues()
        {
            if (AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null) return;

            var acc = AccountManager.Instance.PlayerAccount;
            var s = acc.Settings;
            var p = acc.Profile;

            // Perfil
            nameInput.text = p.PlayerName;
            birthDateInput.text = p.BirthDate;
            levelText.text = $"Nível {p.Level}";

            // Áudio
            masterSlider.value = s.MasterVolume;
            musicSlider.value = s.MusicVolume;
            sfxSlider.value = s.SFXVolume;

            // Sistema
            qualityDropdown.value = s.QualityLevel;
            fpsToggle.isOn = s.ShowFPS;

            // ID da Conta (Unity Services)
            if (Unity.Services.Core.UnityServices.State == Unity.Services.Core.ServicesInitializationState.Initialized)
            {
                unityIdText.text = $"ID: {Unity.Services.Authentication.AuthenticationService.Instance.PlayerId}";
            }
        }

        private void SaveSettings()
        {
            var acc = AccountManager.Instance.PlayerAccount;
            
            // Atualiza Perfil
            acc.Profile.PlayerName = nameInput.text;
            acc.Profile.BirthDate = birthDateInput.text;

            // Atualiza Settings
            acc.Settings.MasterVolume = masterSlider.value;
            acc.Settings.MusicVolume = musicSlider.value;
            acc.Settings.SFXVolume = sfxSlider.value;
            acc.Settings.QualityLevel = qualityDropdown.value;
            acc.Settings.ShowFPS = fpsToggle.isOn;

            // Salva de fato (Local + Cloud)
            AccountManager.Instance.SaveAccount();
            
            Debug.Log("Configurações salvas!");
        }

        private void GoBack()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
        }
    }
}
