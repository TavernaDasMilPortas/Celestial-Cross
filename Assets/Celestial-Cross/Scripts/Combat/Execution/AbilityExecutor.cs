using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Abilities.Graph.Runtime;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Combat.Execution
{
    public class AbilityExecutionContext
    {
        public Unit Caster;
        public AbilityBlueprint Blueprint;
        
        public List<Unit> LastTargets = new List<Unit>();
        public Vector3 LastPoint;
        public Dictionary<string, float> Variables = new Dictionary<string, float>();
        
        public AbilityExecutionContext(Unit caster, AbilityBlueprint blueprint)
        {
            Caster = caster;
            Blueprint = blueprint;
        }
    }

    public class AbilityExecutor : MonoBehaviour
    {
        public static AbilityExecutor Instance;

        public static event Action<AbilityBlueprint, List<Unit>> OnTargetPreviewChanged;

        private Coroutine activeAbilityRoutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AbortCurrentAbility()
        {
            if (activeAbilityRoutine != null)
            {
                StopCoroutine(activeAbilityRoutine);
                activeAbilityRoutine = null;
                
                // Limpa seletores residuais
                foreach (var selector in GetComponents<TargetSelector>())
                {
                    Destroy(selector);
                }
                
                GridMap.Instance?.ResetAllTileVisuals();
                OnTargetPreviewChanged?.Invoke(null, new List<Unit>());
                CombatLogger.Log("Habilidade anterior abortada para iniciar nova a��o.", LogCategory.System);
            }
        }

        public void ExecuteAbility(Unit caster, AbilityBlueprint blueprint, CombatHook currentHook = CombatHook.OnManualCast, Action onComplete = null)
        {
            // Se for OnManualCast (clique do jogador), abortamos qualquer execu��o pendente
            if (currentHook == CombatHook.OnManualCast)
            {
                AbortCurrentAbility();
            }

            activeAbilityRoutine = StartCoroutine(ExecuteBlueprintCoroutine(caster, blueprint, currentHook, onComplete));
        }

        private IEnumerator ExecuteBlueprintCoroutine(Unit caster, AbilityBlueprint blueprint, CombatHook currentHook, Action onComplete)
        {
            CombatLogger.Log($"<color=white>[AbilityExecutor]</color> Iniciando habilidade: <b>{blueprint.name}</b> (Hook: {currentHook})", LogCategory.Ability);

            // Prioridade para o Sistema de Grafo
            if (blueprint.abilityGraph != null && AbilityGraphInterpreter.Instance != null)
            {
                yield return StartCoroutine(AbilityGraphInterpreter.Instance.ExecuteGraphCoroutine(caster, blueprint.abilityGraph, currentHook, onComplete));
                yield break;
            }

            var context = new AbilityExecutionContext(caster, blueprint);

            // Determine which steps to execute based on the hook
            var stepsToExecute = new List<EffectStep>();
            if (currentHook == CombatHook.OnManualCast)
            {
                if (blueprint.effectSteps != null)
                    stepsToExecute.AddRange(blueprint.effectSteps);
            }
            else
            {
                if (blueprint.modifierSteps != null)
                    stepsToExecute.AddRange(blueprint.modifierSteps);
            }

            foreach (var step in stepsToExecute)
            {
                if (step == null) continue;

                // Ignora passos que n???o pertencem ao momento (hook) que estamos disparando
                if (step.trigger != currentHook) continue;

                List<Unit> currentTargets = new List<Unit>();

                if (step.reusePreviousTargets)
                {
                    currentTargets = new List<Unit>(context.LastTargets);
                    Debug.Log($"[AbilityExecutor] Reutilizando {currentTargets.Count} alvos do passo anterior.");
                }
                else if (step.targetingStrategy != null)
                {
                    if (step.targetingStrategy.RequiresManualSelection && currentHook == CombatHook.OnManualCast)
                    {
                        Debug.Log("[AbilityExecutor] Pausando execução para seleção manual de alvos...");

                        TargetSelector selector = caster.gameObject.AddComponent<TargetSelector>();
                        selector.Begin(caster, step.targetingStrategy.ManualRange, step.targetingStrategy.ManualRule, step.targetingStrategy.AreaPattern, step.targetingStrategy.PreferredDirection, null, step.targetingStrategy.AutoRotateArea);

                        bool selectionConfirmed = false;
                        List<Unit> selected = new List<Unit>();

                        Action<List<Unit>> onTargets = (targets) => { 
                            selected = targets; 
                            selectionConfirmed = true; 
                        };
                        Action<List<Unit>> onPreview = (targets) => { OnTargetPreviewChanged?.Invoke(blueprint, targets); };

                        selector.OnTargetsConfirmed += onTargets;
                        selector.OnSelectedTargetsChanged += onPreview;

                        yield return new WaitUntil(() => selectionConfirmed);

                        Direction finalRotation = selector.CurrentRotation;

                        selector.OnTargetsConfirmed -= onTargets;
                        selector.OnSelectedTargetsChanged -= onPreview;

                        OnTargetPreviewChanged?.Invoke(blueprint, new List<Unit>());

                        currentTargets = selected;

                        List<Vector2Int> execPoints = new List<Vector2Int>();
                        if (step.targetingStrategy.AreaPattern != null && step.targetingStrategy.ManualRule.origin == TargetOrigin.Point)
                        {
                            foreach(var origin in selector.SelectedPoints)
                            {
                                Direction dir = finalRotation;
                                foreach(var cell in AreaResolver.ResolveCells(origin, step.targetingStrategy.AreaPattern, dir))
                                    if (!execPoints.Contains(cell)) execPoints.Add(cell);
                            }
                        }
                        else if (step.targetingStrategy.AreaPattern != null)
                        {
                            foreach(var u in currentTargets)
                            {
                                Direction dir = finalRotation;
                                foreach(var cell in AreaResolver.ResolveCells(u.GridPosition, step.targetingStrategy.AreaPattern, dir))
                                    if (!execPoints.Contains(cell)) execPoints.Add(cell);
                            }
                        }
                        else 
                        {
                            foreach(var p in selector.SelectedPoints) if (!execPoints.Contains(p)) execPoints.Add(p);
                            foreach(var u in currentTargets) if (!execPoints.Contains(u.GridPosition)) execPoints.Add(u.GridPosition);
                        }
                        
                        // Darken selected area
                        foreach(var p in execPoints) 
                            GridMap.Instance?.GetTile(p)?.Darken();
                            
                        yield return new WaitForSeconds(0.4f);
                        Destroy(selector); // Cleanup selector properly
                        Debug.Log($"[AbilityExecutor] Seleção manual confirmada. {currentTargets.Count} alvo(s) escolhidos.");
                    }
                    else
                    {
                        var cbContext = new CombatContext(caster);
                        if (step.targetingStrategy != null)
                        {
                            currentTargets = step.targetingStrategy.GetTargets(cbContext);
                        }
                    }
                }

                context.LastTargets = currentTargets;

                var stepContext = new CombatContext(caster);
                stepContext.Variables = context.Variables;

                Debug.Log($"[AbilityExecutor] Aplicando {step.effects.Count} efeitos em {currentTargets.Count} alvos.");
                foreach (var target in currentTargets)
                {
                    foreach (var effect in step.effects)
                    {
                        if (effect != null)
                        {
                            var combatContext = new CombatContext(caster, target);
                            combatContext.Variables = context.Variables;

                            if (effect is Celestial_Cross.Scripts.Abilities.DamageEffectData dmg)
                                combatContext.amount = dmg.GetBaseAmount(combatContext);
                            else if (effect is Celestial_Cross.Scripts.Abilities.HealEffectData heal)
                                combatContext.amount = heal.GetBaseAmount(combatContext);

                            if (effect.scaleWithDistance)
                            {
                                float distance = Vector2Int.Distance(caster.GridPosition, target.GridPosition);
                                combatContext.amount = (int)(combatContext.amount * (1 + distance * effect.distanceScaleFactor));
                            }

                            Debug.Log($"[AbilityExecutor] Preparando efeito {effect.GetType().Name}. Amount inicial: {combatContext.amount}");

                            yield return StartCoroutine(effect.ExecuteCoroutine(combatContext));
                            target.GetComponent<PassiveManager>()?.TriggerHook(CombatHook.OnAfterTakeDamage, combatContext);
                        }
                    }
                }

                yield return new WaitForSeconds(0.1f);
            }

            GridMap.Instance?.ResetAllTileVisuals();
            onComplete?.Invoke();
        }
    }
}





