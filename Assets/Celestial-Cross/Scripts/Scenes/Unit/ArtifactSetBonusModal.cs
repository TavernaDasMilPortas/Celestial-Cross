using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Artifacts;

namespace CelestialCross.Scenes.Unit
{
    public class ArtifactSetBonusModal : MonoBehaviour
    {
        public GameObject modalRoot;
        public TextMeshProUGUI titleText;
        public Transform bonusesContainer;
        public GameObject bonusItemPrefab;
        public Button closeButton;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
        }

        public void Show(ArtifactSet set, int equippedCount)
        {
            if (set == null) return;

            if (modalRoot != null) modalRoot.SetActive(true);

            if (titleText != null)
            {
                string sName = string.IsNullOrEmpty(set.setName) ? set.name : set.setName;
                titleText.text = $"{sName} ({equippedCount} Equipados)";
            }

            if (bonusesContainer != null && bonusItemPrefab != null)
            {
                // Limpa container
                foreach (Transform child in bonusesContainer)
                {
                    if (child.gameObject != bonusItemPrefab)
                        Destroy(child.gameObject);
                }

                if (set.setBonuses != null)
                {
                    foreach (var bonus in set.setBonuses)
                    {
                        var go = Instantiate(bonusItemPrefab, bonusesContainer);
                        go.SetActive(true);

                        var txt = go.GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null)
                        {
                            string desc = $"<b>[{bonus.piecesRequired} Peças]</b> ";
                            
                            if (bonus.statBonuses != null && bonus.statBonuses.Count > 0)
                            {
                                foreach(var st in bonus.statBonuses)
                                {
                                    desc += $"{st.statType} +{st.value} ";
                                }
                            }

                            if (bonus.passiveGraph != null)
                            {
                                desc += $"\n<i>{bonus.passiveGraph.abilityDescription}</i>";
                            }
                            else if (bonus.passiveAbility != null)
                            {
                                desc += $"\n<i>{bonus.passiveAbility.abilityDescription}</i>";
                            }

                            txt.text = desc;
                        }

                        var cg = go.GetComponent<CanvasGroup>();
                        if (cg == null) cg = go.AddComponent<CanvasGroup>();
                        
                        if (equippedCount >= bonus.piecesRequired)
                        {
                            cg.alpha = 1f;
                        }
                        else
                        {
                            cg.alpha = 0.5f;
                        }
                    }
                }
            }
        }

        public void Hide()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
        }
    }
}
