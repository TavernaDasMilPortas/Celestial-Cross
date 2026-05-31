using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialCross.Scenes.Unit
{
    public class UnitMainPanel : MonoBehaviour
    {
        [Header("Unit Info UI")]
        public Image unitSpriteImage; // Usará unitData.sprite
        public TextMeshProUGUI unitNameText;
        public TextMeshProUGUI unitLevelText;
        public Slider unitXpSlider;
        public TextMeshProUGUI unitXpText;

        [Header("Tabs")]
        public Button[] tabButtons = new Button[5];
        public string[] tabNames = { "Atributos", "Pet", "Equipamento", "Constelação", "Habilidades" };
        public Color activeTabColor = Color.white;
        public Color inactiveTabColor = Color.gray;

        private void Awake()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int index = i; // Capturar a variável para o closure
                if (tabButtons[i] != null)
                {
                    tabButtons[i].onClick.AddListener(() => OnTabClicked(index));
                }
            }
        }

        private void Start()
        {
            // Começa selecionando a primeira tab (Atributos)
            if (tabButtons.Length > 0 && tabButtons[0] != null)
            {
                OnTabClicked(0);
            }
        }

        public void LoadUnit(UnitData unitData, CelestialCross.Data.RuntimeUnitData runtimeData)
        {
            if (unitSpriteImage != null) unitSpriteImage.sprite = unitData.sprite;
            if (unitNameText != null) unitNameText.text = unitData.displayName;
            if (unitLevelText != null) unitLevelText.text = $"Lv. {runtimeData.Level}";
            
            // Xp bar lógica: runtimeData.CurrentXP / XPForNextLevel
        }

        private void OnTabClicked(int tabIndex)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null)
                {
                    // Lógica visual simples de ativo/inativo
                    tabButtons[i].image.color = (i == tabIndex) ? activeTabColor : inactiveTabColor;
                }
            }

            string tabName = (tabIndex >= 0 && tabIndex < tabNames.Length) ? tabNames[tabIndex] : "Detalhes";
            UnitSceneController.Instance.ShowDetailPanel(tabIndex, tabName);
        }
    }
}
