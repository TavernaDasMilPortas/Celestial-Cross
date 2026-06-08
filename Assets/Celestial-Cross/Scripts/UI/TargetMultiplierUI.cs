using UnityEngine;
using TMPro;

namespace Celestial_Cross.Scripts.UI
{
    public class TargetMultiplierUI : MonoBehaviour
    {
        private TextMeshProUGUI textMesh;
        private Vector3 targetWorldPos;
        private Camera mainCam;
        private RectTransform rectTransform;

        void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
            if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();
            mainCam = Camera.main;
        }

        public void Setup(Vector3 worldPos, int count)
        {
            targetWorldPos = worldPos;
            if (textMesh != null)
            {
                textMesh.text = $"x{count}";
            }
            UpdatePositionAndVisibility();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            UpdatePositionAndVisibility();
        }

        private void UpdatePositionAndVisibility()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            Vector3 viewportPoint = mainCam.WorldToViewportPoint(targetWorldPos);
            
            // Frustum check
            bool isVisible = viewportPoint.z > 0 && 
                             viewportPoint.x > 0 && viewportPoint.x < 1 && 
                             viewportPoint.y > 0 && viewportPoint.y < 1;

            if (textMesh != null)
            {
                textMesh.enabled = isVisible;
            }

            if (isVisible && rectTransform != null)
            {
                // Screen Space Overlay UI positioning
                rectTransform.position = mainCam.WorldToScreenPoint(targetWorldPos);
            }
        }
    }
}
