using UnityEngine;
using UnityEngine.UI;

public class TurnPortraitUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private Color enemyColor = Color.red;

    public void Setup(Unit unit)
    {
        if (unit == null) return;

        if (iconImage != null && unit.EquippedPet != null)
        {
            iconImage.sprite = unit.EquippedPet.icon;
        }

        if (backgroundImage != null)
        {
            bool isPlayer = unit is Pet || unit.CompareTag("Player");
            backgroundImage.color = isPlayer ? playerColor : enemyColor;
        }
    }
}
