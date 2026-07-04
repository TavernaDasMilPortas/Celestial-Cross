using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Combat.Execution
{
    public class AbilityExecutor : MonoBehaviour
    {
        public static AbilityExecutor Instance;

        public static event Action<AbilityGraphSO, List<Unit>> OnTargetPreviewChanged;

        private int executionCount = 0;
        private Coroutine activeAbilityRoutine; // Mantido para Abort
        
        public bool IsExecuting => executionCount > 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AbortCurrentAbility()
        {
            if (activeAbilityRoutine != null || executionCount > 0)
            {
                StopAllCoroutines(); // Para todas as execuções filhas e passivas
                activeAbilityRoutine = null;
                executionCount = 0;
                
                // Limpa seletores residuais em QUALQUER objeto
                var allSelectors = FindObjectsByType<TargetSelector>(FindObjectsSortMode.None);
                foreach (var selector in allSelectors)
                {
                    Destroy(selector);
                }
                
                GridMap.Instance?.ResetAllTileVisuals();
                OnTargetPreviewChanged?.Invoke(null, new List<Unit>());
                
                // Reset camera focus if it was locked to an action
                CameraController.Instance?.ResetFocus();

                CombatLogger.Log("Habilidade anterior abortada para iniciar nova ação.", LogCategory.System);
            }
        }

        public void ExecuteGraph(Unit caster, AbilityGraphSO graph, CombatHook currentHook = CombatHook.OnManualCast, Action onComplete = null, int level = 1, string slotId = "", Vector2Int? presetTargetPos = null, List<Vector2Int> presetTargetPositions = null)
        {
            if (currentHook == CombatHook.OnManualCast)
            {
                AbortCurrentAbility();
            }

            activeAbilityRoutine = StartCoroutine(ExecuteGraphCoroutine(caster, graph, currentHook, onComplete, level, slotId, presetTargetPos, presetTargetPositions));
        }

        private IEnumerator ExecuteGraphCoroutine(Unit caster, AbilityGraphSO graph, CombatHook currentHook, Action onComplete, int level = 1, string slotId = "", Vector2Int? presetTargetPos = null, List<Vector2Int> presetTargetPositions = null)
        {
            try
            {
                executionCount++;
                CombatLogger.Log($"<color=white>[AbilityExecutor]</color> Iniciando grafo: <b>{graph.name}</b> (Hook: {currentHook})", LogCategory.Ability);
                
                if (AbilityGraphInterpreter.Instance != null)
                {
                    yield return StartCoroutine(AbilityGraphInterpreter.Instance.ExecuteGraphCoroutine(caster, graph, currentHook, onComplete, level, slotId, presetTargetPos, presetTargetPositions));
                }
                else
                {
                    Debug.LogError("[AbilityExecutor] AbilityGraphInterpreter não encontrado!");
                    onComplete?.Invoke();
                }

                // Espera todos os popups de dano sumirem antes de focar ou concluir
                if (DamagePopupManager.Instance != null)
                {
                    yield return new WaitUntil(() => !DamagePopupManager.Instance.HasActivePopups);
                }

                // Ao fim da ação, focar novamente no caster se o turno dele não acabou
                if (caster != null && (caster.hasActedThisTurn == false || caster.hasMovedThisTurn == false))
                {
                    CameraController.Instance?.Follow(caster);
                }
            }
            finally
            {
                executionCount--;
                if (executionCount == 0) activeAbilityRoutine = null;
            }
        }
    }
}
