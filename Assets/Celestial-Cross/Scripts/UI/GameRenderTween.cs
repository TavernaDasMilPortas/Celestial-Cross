using UnityEngine;
using DG.Tweening;

namespace CelestialCross.UI
{
    public class GameRenderTween : MonoBehaviour
    {
        public static GameRenderTween Instance { get; private set; }

        [Header("Settings")]
        public float animDuration = 0.5f;

        [Header("Camera Zoom Logic")]
        [Tooltip("Multiplicador do zoom durante o posicionamento/intro (1.0 = zoom padrão do mapa)")]
        public float introZoomMultiplier = 1.0f;
        
        [Tooltip("Multiplicador do zoom durante o combate (zoom out para encolher a área visível do mapa)")]
        public float combatZoomMultiplier = 1.6f;

        private float originalBaseZoom = -1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void CaptureOriginalZoomIfNeeded()
        {
            if (originalBaseZoom < 0f && CameraController.Instance != null && CameraController.Instance.IsSetupComplete)
            {
                originalBaseZoom = CameraController.Instance.TargetZoom;
                CameraController.Instance.allowZoomOutsideBounds = true;
                
                // Precisamos garantir que o maxZoom da câmera não bloqueie nosso zoom de combate!
                float neededZoom = originalBaseZoom * Mathf.Max(introZoomMultiplier, combatZoomMultiplier);
                if (CameraController.Instance.maxZoom < neededZoom)
                {
                    CameraController.Instance.maxZoom = neededZoom + 2f;
                }
            }
        }

        public void SetIntroMode(bool instant = false)
        {
            CaptureOriginalZoomIfNeeded();
            if (CameraController.Instance == null || originalBaseZoom < 0f) return;

            float target = originalBaseZoom * introZoomMultiplier;

            if (instant)
            {
                CameraController.Instance.TargetZoom = target;
            }
            else
            {
                DOTween.To(() => CameraController.Instance.TargetZoom, 
                           x => CameraController.Instance.TargetZoom = x, 
                           target, animDuration).SetEase(Ease.OutCubic);
            }
        }

        public void SetCombatMode(bool instant = false)
        {
            CaptureOriginalZoomIfNeeded();
            if (CameraController.Instance == null || originalBaseZoom < 0f) return;

            float target = originalBaseZoom * combatZoomMultiplier;

            if (instant)
            {
                CameraController.Instance.TargetZoom = target;
            }
            else
            {
                DOTween.To(() => CameraController.Instance.TargetZoom, 
                           x => CameraController.Instance.TargetZoom = x, 
                           target, animDuration).SetEase(Ease.OutCubic);
            }
        }
    }
}
