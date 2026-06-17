using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace CelestialCross.UI
{
    public class UnitModalUI : MonoBehaviour
    {
        [Header("References - Texts")]
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI hpValueText;

        [Header("References - Images")]
        public Image nameInnerBorder;
        public Image nameOuterBorder;
        public Image hpFillImage;
        public Image hpInnerBorder;
        public Image hpOuterBorder;
        public Image petIconImage;

        [Header("Extras")]
        public GameObject lightningDivider; // O raio que o usuário quer controlar

        [Header("Settings")]
        public Color allyHpColor = Color.cyan;
        public Color enemyHpColor = Color.red;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeStrength = 10f;
        [SerializeField] private int shakeVibrato = 10;

        private Unit currentUnit;
        private int lastHp;

        public void UpdatePanel(Unit unit)
        {
            if (unit == null) return;

            // Desinscreve do anterior se houver
            if (currentUnit != null && currentUnit.Health != null)
            {
                currentUnit.Health.OnHealthChanged -= HandleHealthChanged;
            }

            currentUnit = unit;
            bool isAlly = currentUnit.Team == Team.Player;

            // Configurar o Nome
            if (nameText != null) nameText.text = currentUnit.DisplayName;

            // Configurar a cor da vida dependendo da facção
            if (hpFillImage != null)
            {
                hpFillImage.color = isAlly ? allyHpColor : enemyHpColor;
            }

            // Esconder o HP numérico se for inimigo
            if (hpValueText != null)
            {
                hpValueText.gameObject.SetActive(isAlly);
            }

            if (currentUnit.Health != null)
            {
                currentUnit.Health.OnHealthChanged += HandleHealthChanged;
                lastHp = currentUnit.Health.CurrentHealth;
                UpdateHealthUI(currentUnit.Health.CurrentHealth, currentUnit.Health.MaxHealth, false);
            }

            if (petIconImage != null)
            {
                if (currentUnit.petSpeciesData != null && currentUnit.petSpeciesData.Icon != null)
                {
                    petIconImage.sprite = currentUnit.petSpeciesData.Icon;
                    petIconImage.gameObject.SetActive(true);
                }
                else
                {
                    petIconImage.gameObject.SetActive(false);
                }
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            bool isDamage = current < lastHp;
            lastHp = current;
            UpdateHealthUI(current, max, isDamage);
        }

        private void UpdateHealthUI(int current, int max, bool shake)
        {
            if (hpValueText != null && hpValueText.gameObject.activeSelf)
            {
                hpValueText.text = $"HP: {current} / {max}";
            }

            if (hpFillImage != null)
            {
                // Anima o HP diminuindo/subindo
                hpFillImage.DOFillAmount((float)current / max, 0.3f).SetEase(Ease.OutQuad);
            }

            if (shake)
            {
                // Chacoalha o modal usando DoTween
                transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, 90f, false, true);
                transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), shakeDuration, shakeVibrato);
            }
        }

        private void OnDestroy()
        {
            if (currentUnit != null && currentUnit.Health != null)
            {
                currentUnit.Health.OnHealthChanged -= HandleHealthChanged;
            }
            transform.DOKill();
        }

        public void SetLightningActive(bool active)
        {
            if (lightningDivider != null)
            {
                lightningDivider.SetActive(active);
            }
        }
    }
}
