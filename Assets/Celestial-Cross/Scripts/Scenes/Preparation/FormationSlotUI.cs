using UnityEngine;
using UnityEngine.UI;

public class FormationSlotUI : MonoBehaviour
{
    public Vector2Int GridPos;

    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;

    public System.Action<FormationSlotUI> OnClicked;

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null) return;

        iconImage.sprite = sprite;
        iconImage.enabled = sprite != null;
    }

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }
}
