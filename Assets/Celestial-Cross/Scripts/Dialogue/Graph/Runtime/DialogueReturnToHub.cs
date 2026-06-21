using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.Dialogue.Runtime
{
    public class DialogueReturnToHub : MonoBehaviour
    {
        [SerializeField] private Button returnButton;
        [SerializeField] private string hubSceneName = "HubScene";

        private void Start()
        {
            if (returnButton != null)
            {
                returnButton.gameObject.SetActive(false);
                returnButton.onClick.AddListener(OnReturnClicked);
            }

            if (Manager.DialogueManager.Instance != null)
            {
                Manager.DialogueManager.Instance.OnDialogueEnded.AddListener(ShowButton);
            }
        }

        private void OnDestroy()
        {
            if (Manager.DialogueManager.Instance != null)
            {
                Manager.DialogueManager.Instance.OnDialogueEnded.RemoveListener(ShowButton);
            }
            if (returnButton != null)
            {
                returnButton.onClick.RemoveListener(OnReturnClicked);
            }
        }

        private void ShowButton()
        {
            if (returnButton != null)
            {
                returnButton.gameObject.SetActive(true);
            }
        }

        private void OnReturnClicked()
        {
            CelestialCross.System.SceneTransitionManager.LoadScene(hubSceneName);
        }
    }
}
