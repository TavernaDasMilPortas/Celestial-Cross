using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialCross.Data;
using CelestialCross.System;
using DG.Tweening;

namespace CelestialCross.Scenes.Unit
{
    public class UnitDetailPanel_Constellation : MonoBehaviour
    {
        [Header("Nodes & Visuals")]
        public RectTransform nodesContainer;
        public Image[] starIcons = new Image[6];
        public Image[] connectionLines = new Image[5]; // Conectam 0-1, 1-2, 2-3, 3-4, 4-5

        [Header("Info Panel (Abilities List)")]
        public RectTransform skillListContainer;
        public GameObject skillListItemPrefab;

        [Header("Modals & Actions")]
        public ConstellationDetailsModal detailsModal;
        public CelestialCross.UI.Skills.SkillBranchModal branchModal;
        public Button detailsButton;
        
        [Header("Ações")]
        public TextMeshProUGUI insigniaCountText;
        public Button upgradeButton;

        private RuntimeUnitData currentUnit;
        private UnitData currentSO;
        private DG.Tweening.Sequence currentAnimSeq;

        private void Awake()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01); OnUpgradeClicked(); });
            if (detailsButton != null) detailsButton.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01); OnDetailsClicked(); });
        }

        public void Refresh(UnitData unitData, RuntimeUnitData runtimeData)
        {
            currentUnit = runtimeData;
            currentSO = unitData;

            if (currentUnit == null || currentSO == null) return;

            UpdateVisualLayout();
            RefreshUI();
        }

        private void UpdateVisualLayout()
        {
            if (currentSO.constellationConfig == null) return;

            var stars = currentSO.constellationConfig.stars;
            if (stars == null || stars.Count < 6) return;

            // Posicionar Estrelas
            for (int i = 0; i < starIcons.Length && i < stars.Count; i++)
            {
                if (starIcons[i] == null) continue;
                starIcons[i].rectTransform.anchoredPosition = stars[i].position;
                
                int idx = i;
                var btn = starIcons[i].GetComponent<Button>();
                if (btn == null) btn = starIcons[i].gameObject.AddComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01); ShowSkillInfo(idx); });
            }

            // Posicionar e Rotacionar Linhas
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
                    Vector2 p1 = stars[idx1].position;
                    Vector2 p2 = stars[idx2].position;
                    
                    RectTransform lineRT = connectionLines[i].rectTransform;
                    lineRT.anchoredPosition = (p1 + p2) / 2f;
                    float distance = Vector2.Distance(p1, p2);
                    lineRT.sizeDelta = new Vector2(distance, 5f);
                    float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * Mathf.Rad2Deg;
                    lineRT.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }

        public void RefreshUI()
        {
            int level = currentUnit.ConstellationLevel;
            
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                    starIcons[i].color = (i < level) ? Color.yellow : new Color(0.4f, 0.4f, 0.4f, 0.8f);
            }

            for (int i = 0; i < connectionLines.Length; i++)
            {
                if (connectionLines[i] != null)
                    connectionLines[i].color = (i + 1 < level) ? new Color(1f, 0.9f, 0f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.4f);
            }

            string insigniaID = ConstellationService.GetInsigniaItemID(currentUnit.UnitID);
            int count = AccountManager.Instance.PlayerAccount.GetItemCount(insigniaID);
            
            if (insigniaCountText != null) insigniaCountText.text = $"Insígnias: {count}";
            if (upgradeButton != null) upgradeButton.interactable = (count > 0 && level < 6);

            PopulateAcquiredSkillsList(level);

            currentAnimSeq?.Kill();
            currentAnimSeq = DG.Tweening.DOTween.Sequence();
            currentAnimSeq.SetUpdate(true);
            currentAnimSeq.SetLink(gameObject);
            float delay = 0f;

            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null && starIcons[i].gameObject.activeInHierarchy)
                {
                    starIcons[i].transform.DOKill();
                    starIcons[i].transform.localScale = Vector3.zero;
                    currentAnimSeq.Insert(delay, starIcons[i].transform.DOScale(1f, 0.3f).SetEase(DG.Tweening.Ease.OutBack).SetLink(starIcons[i].gameObject));
                    delay += 0.05f;
                }
            }

            if (skillListContainer != null)
            {
                float listDelay = delay;
                foreach (Transform child in skillListContainer)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        child.DOKill();
                        child.localScale = Vector3.zero;
                        currentAnimSeq.Insert(listDelay, child.DOScale(1f, 0.3f).SetEase(DG.Tweening.Ease.OutBack).SetLink(child.gameObject));
                        listDelay += 0.05f;
                    }
                }
            }
        }

        private void PopulateAcquiredSkillsList(int currentLevel)
        {
            if (skillListContainer == null || skillListItemPrefab == null || currentSO.constellationConfig == null) return;

            foreach (Transform child in skillListContainer)
            {
                if (child.gameObject != skillListItemPrefab)
                    Destroy(child.gameObject);
            }

            if (currentLevel <= 0)
            {
                var emptyGO = new GameObject("EmptyText", typeof(RectTransform), typeof(TextMeshProUGUI));
                emptyGO.transform.SetParent(skillListContainer, false);
                var emptyTxt = emptyGO.GetComponent<TextMeshProUGUI>();
                emptyTxt.text = "Nenhuma habilidade habilitada.";
                emptyTxt.alignment = TextAlignmentOptions.Center;
                emptyTxt.color = Color.gray;
                emptyTxt.fontSize = 20;
                var rt = emptyGO.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 50);
                return;
            }

            var stars = currentSO.constellationConfig.stars;
            for (int i = 0; i < currentLevel && i < stars.Count; i++)
            {
                var star = stars[i];
                var go = Instantiate(skillListItemPrefab, skillListContainer);
                go.SetActive(true);

                int idx = i;
                var btn = go.GetComponent<Button>();
                if (btn != null) btn.onClick.AddListener(() => { CelestialCross.Audio.AudioManager.Instance?.PlayUI(CelestialCross.Audio.SoundKey.ButtonClick01); ShowSkillInfo(idx); });

                var img = go.transform.Find("Icon")?.GetComponent<Image>();
                if (img != null && star.passiveGraph != null && star.passiveGraph.abilityIcon != null)
                {
                    img.sprite = star.passiveGraph.abilityIcon;
                }

                var text = go.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    string sName = string.IsNullOrEmpty(star.starName) ? $"Estrela {idx + 1}" : star.starName;
                    if (star.passiveGraph != null) sName = string.IsNullOrEmpty(star.passiveGraph.abilityName) ? star.passiveGraph.name : star.passiveGraph.abilityName;
                    text.text = sName;
                }
            }
        }

        private void ShowSkillInfo(int index)
        {
            if (currentSO.constellationConfig != null && currentSO.constellationConfig.stars.Count > index && branchModal != null)
            {
                var star = currentSO.constellationConfig.stars[index];
                if (star.passiveGraph != null)
                {
                    bool isActive = currentUnit != null && index < currentUnit.ConstellationLevel;
                    branchModal.Open(currentUnit?.UnitID, star.passiveGraph.name, star.passiveGraph, Celestial_Cross.Scripts.Abilities.SkillTree.SkillSlotType.Basic, () => {}, null, isActive);
                }
            }
        }

        private void OnDetailsClicked()
        {
            if (detailsModal != null)
            {
                detailsModal.Open(currentSO, currentUnit, branchModal);
            }
        }

        private void OnUpgradeClicked()
        {
            if (ConstellationService.TryUpgradeConstellation(currentUnit))
            {
                AccountManager.Instance.SaveAccount();
                RefreshUI();
            }
        }
    }
}
