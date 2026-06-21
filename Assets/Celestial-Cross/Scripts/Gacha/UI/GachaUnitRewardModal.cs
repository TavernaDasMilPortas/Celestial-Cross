using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CelestialCross.Data;
using CelestialCross.Data.Pets;
using CelestialCross.Scenes.Inventory;
using DG.Tweening;

namespace CelestialCross.Gacha.UI
{
    public class GachaUnitRewardModal : MonoBehaviour
    {
        [Header("UI References")]
        public Image spriteImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI statsText;
        public TextMeshProUGUI rarityText;
        public Transform starsContainer;
        public GameObject starPrefab;

        [Header("Skills")]
        public Transform skillsContainer;
        public Button skillButtonPrefab;
        public PetSkillModal petSkillModal; // O modal de exibição da skill

        [Header("Buttons")]
        public Button closeButton;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        public void ShowUnit(RuntimeUnitData unit, GachaRewardEntry entry)
        {
            if (nameText != null) nameText.text = entry.UnitData.displayName;
            if (spriteImage != null)
            {
                spriteImage.sprite = entry.UnitData.sprite != null ? entry.UnitData.sprite : entry.UnitData.icon;
                spriteImage.gameObject.SetActive(spriteImage.sprite != null);
                spriteImage.preserveAspect = true;
            }
            if (rarityText != null) rarityText.text = "Unit - " + entry.Rarity.ToString();

            // Stats Base
            if (statsText != null)
            {
                statsText.text = $"<b>HP:</b> {entry.UnitData.baseStats.health} | <b>ATK:</b> {entry.UnitData.baseStats.attack}\n" +
                                 $"<b>DEF:</b> {entry.UnitData.baseStats.defense} | <b>SPD:</b> {entry.UnitData.baseStats.speed}\n" +
                                 $"<b>CRIT:</b> {entry.UnitData.baseStats.criticalChance}% | <b>C.DMG:</b> {entry.UnitData.baseStats.criticalDamage}%\n" +
                                 $"<b>ACC:</b> {entry.UnitData.baseStats.effectAccuracy}% | <b>RES:</b> {entry.UnitData.baseStats.effectResistance}%";
            }

            SetupStars(entry.ItemStars);
            SetupUnitSkills(entry.UnitData);

            DoPopIn();
        }

        public void ShowPet(RuntimePetData pet, GachaRewardEntry entry)
        {
            if (nameText != null) nameText.text = entry.PetSpeciesData.SpeciesName;
            if (spriteImage != null)
            {
                spriteImage.sprite = entry.PetSpeciesData.sprite != null ? entry.PetSpeciesData.sprite : entry.PetSpeciesData.Icon;
                spriteImage.gameObject.SetActive(spriteImage.sprite != null);
                spriteImage.preserveAspect = true;
            }
            if (rarityText != null) rarityText.text = "Pet - " + entry.Rarity.ToString();

            if (statsText != null)
            {
                statsText.text = $"<b>+HP:</b> {pet.Health} | <b>+ATK:</b> {pet.Attack}\n" +
                                 $"<b>+DEF:</b> {pet.Defense} | <b>+SPD:</b> {pet.Speed}\n" +
                                 $"<b>+CRIT:</b> {pet.CriticalChance}% | <b>+C.DMG:</b> {pet.CriticalDamage}%\n" +
                                 $"<b>+ACC:</b> {pet.EffectAccuracy}% | <b>+RES:</b> {pet.EffectResistance}%";
            }

            SetupStars(entry.ItemStars);
            SetupPetSkills(entry.PetSpeciesData);

            DoPopIn();
        }

        private void SetupStars(int amount)
        {
            if (starsContainer == null || starPrefab == null) return;
            foreach (Transform child in starsContainer) Destroy(child.gameObject);
            for (int i = 0; i < amount; i++) Instantiate(starPrefab, starsContainer);
        }

        private void SetupUnitSkills(UnitData unitData)
        {
            if (skillsContainer == null || skillButtonPrefab == null) return;
            foreach (Transform child in skillsContainer) Destroy(child.gameObject);

            if (unitData.abilityGraphs == null) return;

            foreach (var skill in unitData.abilityGraphs)
            {
                var btn = Instantiate(skillButtonPrefab, skillsContainer);
                var img = btn.transform.Find("Icon")?.GetComponent<Image>();
                if (img == null)
                {
                    var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconObj.transform.SetParent(btn.transform, false);
                    img = iconObj.GetComponent<Image>();
                    var rect = img.rectTransform;
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.sizeDelta = new Vector2(-10, -10); // Margem interna
                }
                if (img != null) {
                    img.sprite = skill.abilityIcon;
                    img.color = skill.abilityIcon != null ? Color.white : new Color(0,0,0,0);
                    img.preserveAspect = true;
                }
                
                var capSkill = skill;
                btn.onClick.AddListener(() => {
                    if (petSkillModal != null)
                    {
                        petSkillModal.Show(capSkill.abilityName, capSkill.abilityIcon, capSkill.abilityDescription);
                    }
                });
            }
        }

        private void SetupPetSkills(CelestialCross.Data.Pets.PetSpeciesSO speciesData)
        {
            if (skillsContainer == null || skillButtonPrefab == null) return;
            foreach (Transform child in skillsContainer) Destroy(child.gameObject);

            if (speciesData.AbilityGraphs == null) return;

            foreach (var skill in speciesData.AbilityGraphs)
            {
                var btn = Instantiate(skillButtonPrefab, skillsContainer);
                var img = btn.transform.Find("Icon")?.GetComponent<Image>();
                if (img == null)
                {
                    var iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconObj.transform.SetParent(btn.transform, false);
                    img = iconObj.GetComponent<Image>();
                    var rect = img.rectTransform;
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.sizeDelta = new Vector2(-10, -10); // Margem interna
                }
                if (img != null) {
                    img.sprite = skill.abilityIcon;
                    img.color = skill.abilityIcon != null ? Color.white : new Color(0,0,0,0);
                    img.preserveAspect = true;
                }
                
                var capSkill = skill;
                btn.onClick.AddListener(() => {
                    if (petSkillModal != null)
                    {
                        petSkillModal.Show(capSkill.abilityName, capSkill.abilityIcon, capSkill.abilityDescription);
                    }
                });
            }
        }

        private void DoPopIn()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        public void Close()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOScale(0f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
