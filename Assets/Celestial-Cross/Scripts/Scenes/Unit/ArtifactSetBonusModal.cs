using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Artifacts;
using DG.Tweening;
using System.Collections.Generic;

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

            if (UnitSceneController.Instance != null) UnitSceneController.Instance.ShowModalOverlay();

            if (modalRoot != null) 
            {
                modalRoot.SetActive(true);
                var rect = modalRoot.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOKill();
                    rect.localScale = Vector3.zero;
                    rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
                }
            }

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
                    Sequence seq = DOTween.Sequence();
                    seq.SetUpdate(true);
                    float delay = 0.15f;

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
                        
                        float targetAlpha = equippedCount >= bonus.piecesRequired ? 1f : 0.5f;
                        cg.alpha = 0f;
                        
                        go.transform.localScale = Vector3.zero;
                        seq.Insert(delay, go.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack));
                        seq.Insert(delay, cg.DOFade(targetAlpha, 0.2f));
                        delay += 0.08f;
                    }
                }
            }
        }

        public void Hide()
        {
            if (modalRoot != null && modalRoot.activeSelf)
            {
                var rect = modalRoot.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.DOKill();
                    rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                        modalRoot.SetActive(false);
                        if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                    });
                }
                else
                {
                    modalRoot.SetActive(false);
                    if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                }
            }
        }
    }
}
