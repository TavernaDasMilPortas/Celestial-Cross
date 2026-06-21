using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using CelestialCross.Audio;

namespace CelestialCross.Scenes.Inventory
{
    /// <summary>
    /// Captura todos os botões filhos no Awake e injeta animações de hover
    /// e som de clique para dar um "Juice" geral na interface sem precisar de trabalho braçal.
    /// </summary>
    public class InventoryJuicer : MonoBehaviour
    {
        private void Awake()
        {
            var allButtons = GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                // Verifica se já não colocamos event trigger pra não duplicar
                if (btn.GetComponent<EventTrigger>() == null)
                {
                    InjectJuiceIntoButton(btn);
                }
            }
        }

        private void InjectJuiceIntoButton(Button btn)
        {
            var rt = btn.GetComponent<RectTransform>();
            if (rt == null) return;

            // Adicionar som de clique genérico
            btn.onClick.AddListener(() => {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayUI(SoundKey.ButtonClick01);
                
                rt.DOKill(true);
                rt.localScale = Vector3.one;
                rt.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 5, 0.5f)
                    .OnComplete(() => rt.localScale = Vector3.one);
            });

            // Criar e configurar o EventTrigger para Hover
            EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();

            // Pointer Enter
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => {
                if (!btn.interactable) return;
                rt.DOKill(true);
                rt.DOScale(1.05f, 0.15f).SetEase(Ease.OutQuad);
                rt.DOLocalRotate(new Vector3(0, 0, 2f), 0.15f).SetEase(Ease.OutQuad);
            });
            trigger.triggers.Add(enterEntry);

            // Pointer Exit
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => {
                if (!btn.interactable) return;
                rt.DOKill(true);
                rt.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
                rt.DOLocalRotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
            });
            trigger.triggers.Add(exitEntry);
        }
    }
}
