using UnityEngine;

namespace CelestialCross.System
{
    /// <summary>
    /// Anexe este script a qualquer Modal, Aba ou Menu que contenha componentes BetterImage.
    /// Sempre que o painel for ativado (OnEnable), ele forçará o recarregamento dos componentes
    /// BetterUI do painel após um pequeno delay, garantindo que eles não fiquem brancos.
    /// </summary>
    public class BetterUIAutoRefresher : MonoBehaviour
    {
        [Tooltip("Tempo de espera antes de atualizar as imagens. 0.1s geralmente é o suficiente para o Canvas montar o layout.")]
        public float Delay = 0.1f;

        private void OnEnable()
        {
            // Aciona o fixer passando a si mesmo como raiz, para corrigir apenas as imagens deste painel.
            if (BetterUIFixer.Instance != null)
            {
                BetterUIFixer.Instance.RefreshImagesDelayed(gameObject, Delay);
            }
        }
    }
}
