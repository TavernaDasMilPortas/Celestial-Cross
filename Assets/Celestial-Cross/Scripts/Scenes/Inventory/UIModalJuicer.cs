using UnityEngine;
using DG.Tweening;
using CelestialCross.Audio;

namespace CelestialCross.Scenes.Inventory
{
    /// <summary>
    /// Classe utilitária para aplicar as animações de estilo Persona 5 aos modais,
    /// evitando repetição de código em cada um deles.
    /// </summary>
    public static class UIModalJuicer
    {
        public static void AnimateModalShow(RectTransform modalTransform)
        {
            if (modalTransform == null) return;
            
            // Tocar som de abertura rápida
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayUI(SoundKey.MenuOpenFast01);

            // Cancelar animações pendentes para não bugar ao clicar várias vezes
            modalTransform.DOKill(true);
            
            // Setar estado inicial exagerado
            modalTransform.localScale = new Vector3(0.5f, 0.5f, 1f);
            modalTransform.localEulerAngles = new Vector3(0, 0, -8f); // Rotação angular excêntrica

            // Animar para a posição/escala normal com overshoot
            modalTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            modalTransform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
        }

        public static void AnimateModalHide(RectTransform modalTransform, global::System.Action onComplete = null)
        {
            if (modalTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            // Tocar som de fechamento
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayUI(SoundKey.MenuClose01);

            modalTransform.DOKill(true);

            // Animar fechamento rápido e encolhimento
            modalTransform.DOScale(new Vector3(0.8f, 0.8f, 1f), 0.15f).SetEase(Ease.InBack);
            modalTransform.DOLocalRotate(new Vector3(0, 0, 5f), 0.15f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    // Garantir que a escala volte a 1 internamente para a próxima vez que for ativado
                    modalTransform.localScale = Vector3.one;
                    modalTransform.localEulerAngles = Vector3.zero;
                    onComplete?.Invoke();
                });
        }
    }
}
