using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MoreMountains.Feedbacks;

namespace CelestialCross.Scenes.Unit
{
    public class UnitMainPanel : MonoBehaviour
    {
        [Header("Unit Info UI")]
        public Image unitSpriteImage; // Usará unitData.sprite
        public TextMeshProUGUI unitNameText;
        public TextMeshProUGUI unitLevelText;
        public Image unitXpFillImage;
        public TextMeshProUGUI unitXpText;
        public LevelingConfig levelingConfig;

        [Header("Tabs")]
        public Button[] tabButtons = new Button[5];
        public string[] tabNames = { "Atributos", "Pet", "Equipamento", "Constelação", "Habilidades" };
        public Color activeTabColor = Color.white;
        public Color inactiveTabColor = Color.gray;

        [Header("Juice - FEEL")]
        public MMF_Player tabSwitchFeedback;
        public MMF_Player unitLoadFeedback;

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
            // Começa selecionando a primeira tab (Atributos) sem animar
            if (tabButtons.Length > 0 && tabButtons[0] != null)
            {
                for (int i = 0; i < tabButtons.Length; i++)
                {
                    if (tabButtons[i] != null)
                    {
                        tabButtons[i].image.color = (i == 0) ? activeTabColor : inactiveTabColor;
                        tabButtons[i].transform.localScale = (i == 0) ? Vector3.one * 1.12f : Vector3.one;
                    }
                }
                UnitSceneController.Instance.ShowDetailPanel(0, tabNames[0]);
            }
        }

        public void LoadUnit(UnitData unitData, CelestialCross.Data.RuntimeUnitData runtimeData)
        {
            if (unitSpriteImage != null) 
            {
                unitSpriteImage.sprite = unitData.sprite;
                unitSpriteImage.DOKill();
                unitSpriteImage.color = new Color(1,1,1,0);
                unitSpriteImage.transform.localScale = Vector3.one * 0.85f;

                Sequence seq = DOTween.Sequence();
                seq.Join(unitSpriteImage.DOFade(1f, 0.2f));
                seq.Join(unitSpriteImage.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
                seq.Append(unitSpriteImage.transform.DOPunchScale(Vector3.one * 0.05f, 0.4f, 5, 0.5f));
            }

            if (unitNameText != null) 
            {
                unitNameText.text = unitData.displayName;
                unitNameText.DOKill();
                unitNameText.color = new Color(unitNameText.color.r, unitNameText.color.g, unitNameText.color.b, 0f);
                unitNameText.DOFade(1f, 0.3f).SetDelay(0.1f);
            }

            if (unitLevelText != null) 
            {
                unitLevelText.text = $"Lv. {runtimeData.Level}";
                unitLevelText.DOKill();
                unitLevelText.color = new Color(unitLevelText.color.r, unitLevelText.color.g, unitLevelText.color.b, 0f);
                unitLevelText.DOFade(1f, 0.3f).SetDelay(0.15f);
            }
            
            if (levelingConfig != null)
            {
                int xpToNext = levelingConfig.GetXPForNextLevel(runtimeData.Level);
                if (unitXpText != null)
                {
                    unitXpText.text = $"{runtimeData.CurrentXP} / {xpToNext}";
                }

                if (unitXpFillImage != null)
                {
                    unitXpFillImage.DOKill();
                    float targetFill = xpToNext > 0 ? (float)runtimeData.CurrentXP / xpToNext : 1f;
                    unitXpFillImage.fillAmount = 0f;
                    unitXpFillImage.DOFillAmount(targetFill, 0.6f).SetEase(Ease.OutCubic);
                }
            }

            unitLoadFeedback?.PlayFeedbacks();
        }

        private void OnTabClicked(int tabIndex)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null)
                {
                    tabButtons[i].image.DOKill();
                    tabButtons[i].transform.DOKill();

                    if (i == tabIndex)
                    {
                        tabButtons[i].image.DOColor(activeTabColor, 0.15f);
                        tabButtons[i].transform.DOScale(1.12f, 0.25f).SetEase(Ease.OutBack);
                    }
                    else
                    {
                        tabButtons[i].image.DOColor(inactiveTabColor, 0.15f);
                        tabButtons[i].transform.DOScale(1f, 0.2f).SetEase(Ease.OutCubic);
                    }
                }
            }

            string tabName = (tabIndex >= 0 && tabIndex < tabNames.Length) ? tabNames[tabIndex] : "Detalhes";
            UnitSceneController.Instance.ShowDetailPanel(tabIndex, tabName);
            
            tabSwitchFeedback?.PlayFeedbacks();
        }
    }
}
