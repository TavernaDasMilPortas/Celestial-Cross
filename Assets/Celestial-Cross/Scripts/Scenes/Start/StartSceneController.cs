using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.System;
using CelestialCross.Audio;

namespace CelestialCross.Scenes.Start
{
    public class StartSceneController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string nextSceneName = "HubScene";

        public void OnStartClicked()
        {
            // Play a sound if AudioManager exists
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUI(SoundKey.GameStateChange01);
            }

            Debug.Log($"[StartSceneController] Iniciando o jogo! Carregando: {nextSceneName}");

            // Load next scene
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadSceneWithFlash(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}
