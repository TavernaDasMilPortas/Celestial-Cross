using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.Tutorial
{
    /// <summary>
    /// Componente auxiliar para atualizar o spotlight via Script se necessário, 
    /// ou para ser usado em elementos específicos. 
    /// Para o tutorial principal, o TutorialOverlayUI já cuida disso.
    /// </summary>
    public class TutorialSpotlightMask : MonoBehaviour
    {
        private Material material;
        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (image != null)
            {
                material = Instantiate(image.material);
                image.material = material;
            }
        }

        public void UpdateValues(Vector2 center, Vector2 size, float feather = 0.01f)
        {
            if (material == null) return;
            material.SetVector("_HoleCenter", new Vector4(center.x, center.y, 0, 0));
            material.SetVector("_HoleSize", new Vector4(size.x, size.y, 0, 0));
            material.SetFloat("_Feather", feather);
        }
    }
}
