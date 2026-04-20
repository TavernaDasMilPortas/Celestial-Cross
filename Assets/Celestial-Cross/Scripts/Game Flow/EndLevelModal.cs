using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Progression
{
    public class EndLevelModal : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject modalPanel;
        
        [Header("Buttons")]
        [SerializeField] private Button nextStepButton;
        [SerializeField] private Button returnToMenuButton;

        [Header("Text")]
        [SerializeField] private TMP_Text statusText;

        public static EndLevelModal Instance { get; private set; }

        private StoryNode _nextAvailableNode;

        private void Awake()
        {
            Instance = this;
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        public void Show(bool success, string message, StoryNode nextNode = null)
        {
            if (modalPanel == null) return;
            
            modalPanel.SetActive(true);
            if (statusText != null) statusText.text = success ? "Vitória!" : "Derrota...";
            
            _nextAvailableNode = nextNode;
            
            if (nextStepButton != null)
            {
                nextStepButton.gameObject.SetActive(success && _nextAvailableNode != null);
                nextStepButton.onClick.RemoveAllListeners();
                nextStepButton.onClick.AddListener(() => {
                    modalPanel.SetActive(false);
                    _nextAvailableNode.Execute();
                });
            }

            if (returnToMenuButton != null)
            {
                returnToMenuButton.onClick.RemoveAllListeners();
                returnToMenuButton.onClick.AddListener(() => {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
                });
            }
        }
    }
}