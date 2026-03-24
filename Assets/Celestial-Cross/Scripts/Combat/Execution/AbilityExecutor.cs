using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void ExecuteAbility(Unit caster, AbilityBlueprint blueprint, CombatHook currentHook = CombatHook.OnManualCast, Action onComplete = null)
        {
            StartCoroutine(ExecuteBlueprintCoroutine(caster, blueprint, currentHook, onComplete));
        }

        private IEnumerator ExecuteBlueprintCoroutine(Unit caster, AbilityBlueprint blueprint, CombatHook currentHook, Action onComplete)
        {
            Debug.Log($"[AbilityExecutor] Iniciando execu��o de: {blueprint.name} - Hook: {currentHook}");
            var context = new AbilityExecutionContext(caster, blueprint);

            foreach (var step in blueprint.effectSteps)
            {
                // Ignora passos que n�o pertencem ao momento (hook) que estamos disparando
                if (step.trigger != currentHook) continue;

                if (step.targetingStrategy == null) continue;

                List<Unit> currentTargets = new List<Unit>();

                if (step.targetingStrategy.RequiresManualSelection && !(step.targetingStrategy is Celestial_Cross.Scripts.Abilities.Targeting.SelfTargetingStrategy))
                {
                    Debug.Log("[AbilityExecutor] Pausando execu��o para sele��o manual de alvos...");

                    TargetSelector selector = caster.gameObject.AddComponent<TargetSelector>();
                    selector.Begin(caster, step.targetingStrategy.ManualRange, step.targetingStrategy.ManualRule, step.targetingStrategy.AreaPattern, step.targetingStrategy.AreaRotationSteps);

                    bool selectionConfirmed = false;
                    List<Unit> selected = new List<Unit>();

                    Action onRequested = () => { selectionConfirmed = true; };
                    Action<List<Unit>> onTargets = (targets) => { selected = targets; };
                    Action<List<Unit>> onPreview = (targets) => { OnTargetPreviewChanged?.Invoke(blueprint, targets); };

                    selector.OnExecuteRequested += onRequested;
                    selector.OnTargetsConfirmed += onTargets;
                    selector.OnSelectedTargetsChanged += onPreview;

                    yield return new WaitUntil(() => selectionConfirmed);

                    var prop = typeof(TargetSelector).GetField("selectedTargets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (selected.Count == 0 && prop != null)
                    {
                        var rawList = prop.GetValue(selector) as List<Unit>;
                        if (rawList != null) selected.AddRange(rawList);
                    }

                    selector.OnExecuteRequested -= onRequested;
                    selector.OnTargetsConfirmed -= onTargets;
                    selector.OnSelectedTargetsChanged -= onPreview;
                    
                    OnTargetPreviewChanged?.Invoke(blueprint, new List<Unit>());

                    var methodCancel = typeof(TargetSelector).GetMethod("Cancel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    methodCancel?.Invoke(selector, null);

                    currentTargets = selected;

                    List<Vector2Int> execPoints = new List<Vector2Int>();
                    if (step.targetingStrategy.AreaPattern != null && step.targetingStrategy.ManualRule.origin == TargetOrigin.Point)
                    {
                        foreach(var p in selector.SelectedPoints)
                        {
                            foreach(var cell in AreaResolver.ResolveCells(p, step.targetingStrategy.AreaPattern, step.targetingStrategy.AreaRotationSteps))
                                execPoints.Add(cell);
                        }
                    }
                    else if (step.targetingStrategy.AreaPattern != null)
                    {
                        foreach(var u in selected) 
                        {
                            foreach(var cell in AreaResolver.ResolveCells(u.GridPosition, step.targetingStrategy.AreaPattern, step.targetingStrategy.AreaRotationSteps))
                                execPoints.Add(cell);
                        }
                    }
                    else 
                    {
                        foreach(var p in selector.SelectedPoints) execPoints.Add(p);
                        foreach(var u in selected) execPoints.Add(u.GridPosition);
                    }
                    
                    // Darken selected area
                    foreach(var p in execPoints) 
                        GridMap.Instance?.GetTile(p)?.Darken();
                        
                    yield return new WaitForSeconds(0.4f);
                    Debug.Log($"[AbilityExecutor] Sele��o manual confirmada. {currentTargets.Count} alvo(s) escolhidos.");
                }
                else
                {
                    var cbContext = new CombatContext(caster); 
                    currentTargets = step.targetingStrategy.GetTargets(cbContext);
                }

                context.LastTargets = currentTargets;

                Debug.Log($"[AbilityExecutor] Aplicando {step.effects.Count} efeitos em {currentTargets.Count} alvos.");
                foreach (var target in currentTargets)
                {
                    foreach (var effect in step.effects)
                    {
                        if (effect != null)
                        {
                            var combatContext = new CombatContext(caster, target);
                            combatContext.Variables = context.Variables; // Passa a mochila!
                            effect.Execute(combatContext);
                        }
                    }
                }

                yield return new WaitForSeconds(0.1f); 
                GridMap.Instance?.ResetAllTileVisuals();
            }

            Debug.Log($"[AbilityExecutor] Execu��o de {blueprint.name} finalizada.");
            onComplete?.Invoke();
        }
    }
}
