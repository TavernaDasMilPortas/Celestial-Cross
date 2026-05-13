using UnityEngine;
using System.Collections.Generic;
using CelestialCross.Tutorial;

namespace CelestialCross.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        public bool IsActive { get; private set; }

        [Header("References")]
        [SerializeField] private TutorialOverlayUI overlayUI;

        private TutorialModuleSO currentModule;
        private int currentStepIndex;

        public TutorialStep CurrentStep => (currentModule != null && currentStepIndex < currentModule.Steps.Count) 
            ? currentModule.Steps[currentStepIndex] 
            : null;

        public bool CurrentStepForceResult => CurrentStep != null && CurrentStep.ForceActionResult;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartTutorial(TutorialModuleSO module)
        {
            if (module == null) return;

            currentModule = module;
            currentStepIndex = 0;
            IsActive = true;

            if (overlayUI != null) overlayUI.Show();

            ApplyCurrentStep();
        }

        public void AdvanceStep()
        {
            currentStepIndex++;
            if (currentStepIndex >= currentModule.Steps.Count)
            {
                EndTutorial();
            }
            else
            {
                ApplyCurrentStep();
            }
        }

        public void EndTutorial()
        {
            IsActive = false;
            currentModule = null;
            if (overlayUI != null) overlayUI.Hide();
            Debug.Log("[TutorialManager] Tutorial finalizado.");
        }

        private void ApplyCurrentStep()
        {
            var step = CurrentStep;
            if (step == null) return;

            Debug.Log($"[TutorialManager] Aplicando passo: {step.StepID}");

            // Atualiza UI
            if (overlayUI != null)
            {
                overlayUI.SetInstruction(step.InstructionText, step.InstructionIcon, step.BannerPosition);
                
                // Configura o Spotlight
                UpdateSpotlightForStep(step);
            }

            // Foca Câmera
            if (step.FocusCamera && CameraController.Instance != null)
            {
                // Implementação simples de foco no grid
                // CameraController.Instance.FocusOn(GridMap.Instance.GridToWorld(step.CameraFocusGridPos));
            }

            // Se for por tempo, inicia contagem
            if (step.AdvanceCondition == TutorialAdvanceCondition.WaitSeconds)
            {
                Invoke(nameof(AdvanceStep), step.WaitDuration);
            }
        }

        private void UpdateSpotlightForStep(TutorialStep step)
        {
            if (step.HighlightType == TutorialHighlightTarget.None)
            {
                overlayUI.SetSpotlight(Vector2.zero, Vector2.zero, false);
                return;
            }

            Vector2 screenPos = Vector2.zero;
            Vector2 size = step.SpotlightSize;

            switch (step.HighlightType)
            {
                case TutorialHighlightTarget.UIButton:
                    // Encontrar botão pelo ID (identificador no plano era "ActionButton_0")
                    GameObject btn = GameObject.Find(step.TargetIdentifier);
                    if (btn != null)
                    {
                        screenPos = RectTransformUtility.WorldToScreenPoint(null, btn.transform.position);
                    }
                    break;

                case TutorialHighlightTarget.GridTile:
                    // Parse "Tile_X_Y"
                    string[] parts = step.TargetIdentifier.Split('_');
                    if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                    {
                        Vector3 worldPos = GridMap.Instance.GridToWorld(new Vector2Int(x, y));
                        screenPos = Camera.main.WorldToScreenPoint(worldPos);
                    }
                    break;

                case TutorialHighlightTarget.Unit:
                    // Encontrar unidade (simplificado)
                    GameObject unit = GameObject.Find(step.TargetIdentifier);
                    if (unit != null)
                    {
                        screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                    }
                    break;
            }

            overlayUI.SetSpotlight(screenPos, size, true);
        }

        // ==========================================
        // NOTIFICAÇÕES DOS SISTEMAS
        // ==========================================

        public void NotifyActionSelected(int actionIndex)
        {
            if (!IsActive || CurrentStep == null) return;
            if (CurrentStep.AdvanceCondition == TutorialAdvanceCondition.ActionConfirmed)
            {
                // Verifica se era a ação esperada se quisermos ser mais rígidos
                AdvanceStep();
            }
        }

        public void NotifyTargetConfirmed()
        {
            if (!IsActive || CurrentStep == null) return;
            if (CurrentStep.AdvanceCondition == TutorialAdvanceCondition.ActionConfirmed)
            {
                AdvanceStep();
            }
        }

        public void NotifyUnitPlaced(UnitData unit)
        {
            if (!IsActive || CurrentStep == null) return;
            if (CurrentStep.AdvanceCondition == TutorialAdvanceCondition.UnitPlaced)
            {
                AdvanceStep();
            }
        }

        public void NotifyTurnEnded()
        {
            if (!IsActive || CurrentStep == null) return;
            if (CurrentStep.AdvanceCondition == TutorialAdvanceCondition.TurnEnded)
            {
                AdvanceStep();
            }
        }

        public void OnOverlayClicked()
        {
            if (!IsActive || CurrentStep == null) return;
            if (CurrentStep.AdvanceCondition == TutorialAdvanceCondition.ClickAnywhere)
            {
                AdvanceStep();
            }
        }
    }
}
