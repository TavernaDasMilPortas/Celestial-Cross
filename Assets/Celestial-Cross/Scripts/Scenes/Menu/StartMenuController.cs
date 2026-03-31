using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private LevelData defaultLevel;
    [SerializeField] private string preparationSceneName = "PreparationScene";

    [Header("UI")]
    [SerializeField] private Button startButton;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
    }

    public void OnStartClicked()
    {
        if (GameFlowManager.Instance == null)
        {
            Debug.LogError("[StartMenuController] GameFlowManager não encontrado na cena.");
            return;
        }

        if (defaultLevel == null)
        {
            Debug.LogError("[StartMenuController] defaultLevel não configurado.");
            return;
        }

        GameFlowManager.Instance.SelectedLevel = defaultLevel;

        // A seleção acontece na PreparationScene.
        GameFlowManager.Instance.SelectedUnitIDs.Clear();
        GameFlowManager.Instance.PlayerFormation.Clear();

        if (string.IsNullOrWhiteSpace(preparationSceneName))
        {
            Debug.LogError("[StartMenuController] preparationSceneName vazio.");
            return;
        }

        SceneManager.LoadScene(preparationSceneName);
    }
}
