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
        private CanvasGroup canvasGroup;

        void Awake()
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>(true);
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
            mainCam = Camera.main;
        }

        public void Setup(Vector3 worldPos, int count)
        {
            if (textMesh == null) textMesh = GetComponentInChildren<TextMeshProUGUI>(true);
            targetWorldPos = worldPos;
            if (textMesh != null)
            {
                textMesh.enabled = true; // Força ligar caso estivesse desligado no prefab
                textMesh.gameObject.SetActive(true); // Garante que o GameObject filho tbm esteja ativo
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

            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                // Para não bloquear raycasts caso fique na frente de algo
                canvasGroup.blocksRaycasts = isVisible;
                canvasGroup.interactable = isVisible;
            }

            if (isVisible && rectTransform != null)
            {
                Vector3 current3DPos = targetWorldPos;
                Vector3 canvasWorldPos = Vector3.zero;
                bool positionFound = false;

                if (RenderTextureInputManager.Instance != null && RenderTextureInputManager.Instance.WorldToCanvasWorldPoint(current3DPos, out canvasWorldPos))
                {
                    positionFound = true;
                }
                else if (transform.parent != null)
                {
                    Vector3 screenPos = mainCam.WorldToScreenPoint(current3DPos);
                    if (screenPos.z >= 0)
                    {
                        RectTransform parentRect = transform.parent as RectTransform;
                        Canvas canvas = GetComponentInParent<Canvas>();
                        Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

                        if (parentRect != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, screenPos, uiCamera, out Vector3 worldPoint))
                        {
                            canvasWorldPos = worldPoint;
                            positionFound = true;
                        }
                    }
                }

                if (positionFound)
                {
                    rectTransform.position = canvasWorldPos;
                }
                else
                {
                    // Fallback to basic Screen Space Overlay if parent mapping fails
                    rectTransform.position = mainCam.WorldToScreenPoint(current3DPos);
                }
            }
        }
    }
}
