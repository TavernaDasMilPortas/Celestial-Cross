using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Celestial_Cross.Scripts.Units.Enemy;

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

    private Unit linkedUnit;

    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        
        btn.onClick.AddListener(OnClickPortrait);
    }

    private void OnClickPortrait()
    {
        if (linkedUnit == null || CameraController.Instance == null) return;
        
        // Bloqueia clique no turno do inimigo (se a unidade atual for inimigo)
        if (TurnManager.Instance != null && TurnManager.Instance.CurrentUnit is EnemyUnit)
            return;

        CameraController.Instance.Follow(linkedUnit);
    }

    // Use default turnOrder = 1 to not break existing scripts immediately
    public void Setup(Unit unit, int turnOrder = 1)
    {
        if (unit == null) return;
        linkedUnit = unit;

        if (iconImage != null)
        {
            if (unit.UnitData != null)
                iconImage.sprite = unit.UnitData.icon;
            else if (unit.petSpeciesData != null)
                iconImage.sprite = unit.petSpeciesData.Icon;
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
