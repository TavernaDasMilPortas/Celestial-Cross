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
                CombatLogger.Log("Habilidade anterior abortada para iniciar nova ação.", LogCategory.System);
            }
        }

        public void ExecuteAbility(Unit caster, AbilityBlueprint blueprint, CombatHook currentHook = CombatHook.OnManualCast, Action onComplete = null)
        {
            // Se for OnManualCast (clique do jogador), abortamos qualquer execução pendente
            if (currentHook == CombatHook.OnManualCast)
            {
                AbortCurrentAbility();
            }

            activeAbilityRoutine = StartCoroutine(ExecuteBlueprintCoroutine(caster, blueprint, currentHook, onComplete));
        }

        private IEnumerator ExecuteBlueprintCoroutine(Unit caster, AbilityBlueprint blueprint, CombatHook currentHook, Action onComplete)
        {
            // NEW: Handle PassiveAbilityBlueprint
            if (blueprint is PassiveAbilityBlueprint passiveBlueprint)
            {
                var combatContext = new CombatContext(caster, caster); // Contexto gen??rico para passivas
                foreach (var passiveEffect in passiveBlueprint.passiveEffects)
                {
                    if (passiveEffect.triggerHook == currentHook)
                    {
                        CombatLogger.Log($"<color=magenta>[AbilityExecutor]</color> Executando Passiva: {passiveEffect.GetType().Name} para o hook {currentHook}");
                        passiveEffect.Execute(combatContext);
                    }
                }
                onComplete?.Invoke();
                yield break; // Finaliza a corrotina para passivas
            }

            CombatLogger.Log($"<color=white>[AbilityExecutor]</color> Iniciando habilidade: <b>{blueprint.name}</b> (Hook: {currentHook})");
            var context = new AbilityExecutionContext(caster, blueprint);

            foreach (var step in blueprint.effectSteps)
            {
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
                    if (step.targetingStrategy.RequiresManualSelection && !(step.targetingStrategy is Celestial_Cross.Scripts.Abilities.Targeting.SelfTargetingStrategy))
                    {
                        Debug.Log("[AbilityExecutor] Pausando execu??????o para sele??????o manual de alvos...");

                        TargetSelector selector = caster.gameObject.AddComponent<TargetSelector>();
                        selector.Begin(caster, step.targetingStrategy.ManualRange, step.targetingStrategy.ManualRule, step.targetingStrategy.AreaPattern, step.targetingStrategy.PreferredDirection);

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

                        selector.OnTargetsConfirmed -= onTargets;
                        selector.OnSelectedTargetsChanged -= onPreview;
                        
                        OnTargetPreviewChanged?.Invoke(blueprint, new List<Unit>());

                        currentTargets = selected;

                        List<Vector2Int> execPoints = new List<Vector2Int>();
                        if (step.targetingStrategy.AreaPattern != null && step.targetingStrategy.ManualRule.origin == TargetOrigin.Point)
                        {
                            foreach(var origin in selector.SelectedPoints)
                            {
                                Direction dir = step.targetingStrategy.PreferredDirection;
                                foreach(var cell in AreaResolver.ResolveCells(origin, step.targetingStrategy.AreaPattern, dir))
                                    if (!execPoints.Contains(cell)) execPoints.Add(cell);
                            }
                        }
                        else if (step.targetingStrategy.AreaPattern != null)
                        {
                            foreach(var u in currentTargets) 
                            {
                                Direction dir = step.targetingStrategy.PreferredDirection;
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
                        UnityEngine.Object.Destroy(selector); // Cleanup selector properly
                        Debug.Log($"[AbilityExecutor] SeleÃ§Ã£o manual confirmada. {currentTargets.Count} alvo(s) escolhidos.");
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

                // Create a CombatContext to store persistent variables for this step
                // This context will be shared between passive triggers and the active effects
                var stepContext = new CombatContext(caster);
                stepContext.Variables = context.Variables; // Share variables blueprint-wide if needed

                // --- NEW: SPEED STEP CALCULATION ---
                // If the caster is much faster than the target, some steps might repeat.
                // We calculate if there's an advantage relative to each target.
                // ------------------------------------

                Debug.Log($"[AbilityExecutor] Aplicando {step.effects.Count} efeitos em {currentTargets.Count} alvos.");
                foreach (var target in currentTargets)
                {
                    int repeats = 1;
                    
                    // Logical expansion: Handle speed-based double hits for steps that allow it.
                    // This is only for Offensive/Active steps usually.
                    if (currentHook == CombatHook.OnManualCast)
                    {
                        // Check if caster SPD > target SPD * 2 (or other logic).
                        // Let's stick to the SpeedAdvantageCondition if present on effects or a generic rule.
                        // For now, we allow the effect themselves to handle repeats via Conditions if they want,
                        // or we can force a repeat here if the step defines it.
                    }

                    for (int i = 0; i < repeats; i++)
                    {
                        foreach (var effect in step.effects)
                        {
                            if (effect != null)
                            {
                                // IMPORTANT: Create the context ONCE per effect instance
                                var combatContext = new CombatContext(caster, target);
                                combatContext.Variables = context.Variables;

                                // INJECT BASE VALUES FROM EFFECT TO CONTEXT
                                if (effect is Celestial_Cross.Scripts.Abilities.DamageEffectData dmg)
                                    combatContext.amount = dmg.GetBaseAmount(combatContext);
                                else if (effect is Celestial_Cross.Scripts.Abilities.HealEffectData heal)
                                    combatContext.amount = heal.GetBaseAmount(combatContext);

                                Debug.Log($"[AbilityExecutor] Preparando efeito {effect.GetType().Name}. Amount inicial (Base do Asset): {combatContext.amount}");

                                yield return StartCoroutine(effect.ExecuteCoroutine(combatContext));
                                target.TriggerPassives(CombatHook.OnAfterTakeDamage, combatContext);
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(0.1f); 
                GridMap.Instance?.ResetAllTileVisuals();
            }

            Debug.Log($"[AbilityExecutor] ExecuÃ§Ã£o de {blueprint.name} finalizada.");
            
            // --- FINALIZAÃ‡ÃƒO DO TURNO ---
            if (currentHook == CombatHook.OnManualCast)
            {
                PlayerController.Instance?.EndTurn();
            }

            onComplete?.Invoke();
        }
    }
}


