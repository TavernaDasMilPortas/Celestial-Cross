using UnityEngine;
using TMPro;

public class ActionModalUI : MonoBehaviour
{
    public static ActionModalUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject visualRoot;

    private void Awake()
    {
        Instance = this;
        Debug.Log("[ActionModalUI] Instance inicializada com sucesso.");
        Hide();
    }

    public void Show(IUnitAction action)
    {
        if (action == null) return;
        
        if (visualRoot == null)
        {
            Debug.LogError("[ActionModalUI] visualRoot não atribuído no Inspector!");
            return;
        }

        if (nameText != null) nameText.text = action.ActionName;
        if (statsText != null) statsText.text = action.GetDetailStats();
        if (descriptionText != null) descriptionText.text = action.ActionDescription;

        visualRoot.SetActive(true);
    }

    public void Hide()
    {
        visualRoot.SetActive(false);
    }
}
