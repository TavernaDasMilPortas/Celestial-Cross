using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnPortraitUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private Color enemyColor = Color.red;

    [Header("Turn Visuals")]
    [SerializeField] private Sprite[] placeSprites;
    [SerializeField] private TMP_Text turnOrderText;
    [SerializeField] private Image textBackgroundImage; // A imagem que fica atrás do texto

    // Use default turnOrder = 1 to not break existing scripts immediately
    public void Setup(Unit unit, int turnOrder = 1)
    {
        if (unit == null) return;

        if (iconImage != null && unit.UnitData != null)
        {
            iconImage.sprite = unit.UnitData.icon;
        }

        if (backgroundImage != null)
        {
            bool isPlayer = unit is Pet || unit.CompareTag("Player") || unit.Team == Team.Player;
            backgroundImage.color = isPlayer ? playerColor : enemyColor;
        }

        // Sorteia um fundinho para trás do texto
        if (textBackgroundImage != null && placeSprites != null && placeSprites.Length > 0)
        {
            int rnd = Random.Range(0, placeSprites.Length);
            textBackgroundImage.sprite = placeSprites[rnd];
        }

        // Apply Text formating
        if (turnOrderText != null)
        {
            turnOrderText.text = $"#{turnOrder}";
        }
    }
}
