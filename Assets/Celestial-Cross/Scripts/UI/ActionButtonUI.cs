using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI shortcutText;
    [SerializeField] private Button button;

    private int actionIndex;

    public void Setup(IUnitAction action, int index)
    {
        actionIndex = index;
        
        if (nameText != null)
            nameText.text = action.ActionName;
            
        if (iconImage != null && action.ActionIcon != null)
        {
            iconImage.sprite = action.ActionIcon;
        }
        
        if (shortcutText != null)
            shortcutText.text = (index + 1).ToString();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // PlayerController uses 0-based index for SelectAction
        PlayerController.Instance.SelectAction(actionIndex);
    }
}
