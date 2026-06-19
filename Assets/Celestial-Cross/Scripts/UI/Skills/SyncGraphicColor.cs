using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.UI.Skills
{
    /// <summary>
    /// Sincroniza a cor (Color Tint) deste gráfico com a de um gráfico pai (ex: a borda de um Button).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class SyncGraphicColor : MonoBehaviour
    {
        [Tooltip("O gráfico (ex: Image da borda) que este componente deve imitar a cor. Se vazio, pegará o do pai.")]
        public Graphic targetGraphicToSync;
        
        private Graphic myGraphic;

        private void Awake()
        {
            myGraphic = GetComponent<Graphic>();
            
            // Tenta pegar automaticamente do pai caso não tenha sido atribuído manualmente
            if (targetGraphicToSync == null && transform.parent != null)
            {
                targetGraphicToSync = transform.parent.GetComponent<Graphic>();
            }
        }

        private void Update()
        {
            if (targetGraphicToSync != null && myGraphic != null)
            {
                // Copia a cor exata do renderizador do canvas (incluindo as transições de Hover/Pressed do Button)
                myGraphic.canvasRenderer.SetColor(targetGraphicToSync.canvasRenderer.GetColor());
            }
        }
    }
}
