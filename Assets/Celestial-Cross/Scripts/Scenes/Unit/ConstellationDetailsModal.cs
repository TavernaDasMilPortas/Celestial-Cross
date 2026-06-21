using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data;
using CelestialCross.System;
using CelestialCross.UI.Skills;
using DG.Tweening;

namespace CelestialCross.Scenes.Unit
{
    public class ConstellationDetailsModal : MonoBehaviour
    {
        [Header("UI References")]
        public RectTransform constellationContainer;
        public Image[] starIcons = new Image[6];
        public Image[] connectionLines = new Image[5];
        
        [Header("Abilities List")]
        public RectTransform listContainer;
        public GameObject listItemPrefab;

        [Header("Modals")]
        public SkillBranchModal branchModal;
        public Button closeButton;

        private UnitData currentSO;
        private RuntimeUnitData currentUnit;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void Open(UnitData unitData, RuntimeUnitData runtimeData, SkillBranchModal bModal)
        {
            currentSO = unitData;
            currentUnit = runtimeData;
            branchModal = bModal;

            if (UnitSceneController.Instance != null) UnitSceneController.Instance.ShowModalOverlay();

            transform.SetAsLastSibling();
            gameObject.SetActive(true);

            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            RefreshVisuals();
            PopulateList();
        }

        public void Close()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null && gameObject.activeSelf)
            {
                rect.DOKill();
                rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    gameObject.SetActive(false);
                    if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
                });
            }
            else
            {
                gameObject.SetActive(false);
                if (UnitSceneController.Instance != null) UnitSceneController.Instance.HideModalOverlay();
            }
        }

        private void RefreshVisuals()
        {
            if (currentSO == null || currentSO.constellationConfig == null) return;

            var stars = currentSO.constellationConfig.stars;
            int level = currentUnit != null ? currentUnit.ConstellationLevel : 0;

            // Escala para fazer a constelação maior no modal
            float scaleMultiplier = 2.0f;

            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);
            float delay = 0.2f;

            for (int i = 0; i < starIcons.Length && i < stars.Count; i++)
            {
                if (starIcons[i] == null) continue;
                
                starIcons[i].rectTransform.anchoredPosition = stars[i].position * scaleMultiplier;
                starIcons[i].color = (i < level) ? Color.yellow : new Color(0.4f, 0.4f, 0.4f, 0.8f);

                int idx = i;
                var btn = starIcons[i].GetComponent<Button>();
                if (btn == null) btn = starIcons[i].gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnAbilityClicked(idx));

                starIcons[i].transform.DOKill();
                starIcons[i].transform.localScale = Vector3.zero;
                seq.Insert(delay, starIcons[i].transform.DOScale(1f, 0.3f).SetEase(Ease.OutElastic));
                delay += 0.08f;
            }

            int[] indices = currentSO.constellationConfig.connectionIndices;
            bool useCustomConnections = indices != null && indices.Length > 1 && indices.Length % 2 == 0;

            for (int i = 0; i < connectionLines.Length; i++)
            {
                if (connectionLines[i] == null) continue;

                int idx1, idx2;
                if (useCustomConnections && (i * 2 + 1) < indices.Length)
                {
                    idx1 = indices[i * 2];
                    idx2 = indices[i * 2 + 1];
                }
                else
                {
                    idx1 = i;
                    idx2 = i + 1;
                }

                if (idx1 < stars.Count && idx2 < stars.Count)
                {
                    Vector2 p1 = stars[idx1].position * scaleMultiplier;
                    Vector2 p2 = stars[idx2].position * scaleMultiplier;
                    
                    RectTransform lineRT = connectionLines[i].rectTransform;
                    lineRT.anchoredPosition = (p1 + p2) / 2f;
                    float distance = Vector2.Distance(p1, p2);
                    lineRT.sizeDelta = new Vector2(distance, 5f);
                    float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;
                    lineRT.localRotation = Quaternion.Euler(0, 0, angle);

                    connectionLines[i].color = (i + 1 < level) ? new Color(1f, 0.9f, 0f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
                }
            }
        }

        private void PopulateList()
        {
            if (currentSO == null || currentSO.constellationConfig == null) return;
            
            foreach(Transform child in listContainer)
            {
                Destroy(child.gameObject);
            }

            var stars = currentSO.constellationConfig.stars;
            int level = currentUnit != null ? currentUnit.ConstellationLevel : 0;

            for (int i = 0; i < stars.Count && i < 6; i++)
            {
                var star = stars[i];
                var go = Instantiate(listItemPrefab, listContainer);
                go.SetActive(true);

                int idx = i;
                bool isActive = idx < level;

                var btn = go.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => OnAbilityClicked(idx));

                var img = go.transform.Find("Icon")?.GetComponent<Image>();
                if (img != null && star.passiveGraph != null && star.passiveGraph.abilityIcon != null)
                {
                    img.sprite = star.passiveGraph.abilityIcon;
                    img.color = isActive ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
                }

                var text = go.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    string sName = string.IsNullOrEmpty(star.starName) ? $"Estrela {idx + 1}" : star.starName;
                    if (star.passiveGraph != null) sName = string.IsNullOrEmpty(star.passiveGraph.abilityName) ? star.passiveGraph.name : star.passiveGraph.abilityName;
                    
                    text.text = isActive ? sName : $"{sName} <color=red><size=80%>[Bloqueada]</size></color>";
                    text.color = isActive ? Color.white : Color.gray;
                }
            }
        }

        private void OnAbilityClicked(int index)
        {
            if (branchModal == null || currentSO == null || currentSO.constellationConfig == null) return;
            
            var stars = currentSO.constellationConfig.stars;
            if (index >= stars.Count) return;

            var star = stars[index];
            if (star.passiveGraph != null)
            {
                bool isActive = currentUnit != null && index < currentUnit.ConstellationLevel;
                branchModal.Open(currentUnit?.UnitID, star.passiveGraph.name, star.passiveGraph, Celestial_Cross.Scripts.Abilities.SkillTree.SkillSlotType.Basic, () => {}, null, isActive);
            }
        }
    }
}
