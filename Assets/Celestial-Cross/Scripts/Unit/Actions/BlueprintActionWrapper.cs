using System;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Combat.Execution;

namespace Celestial_Cross.Scripts.Units
{
    public class BlueprintActionWrapper : IUnitAction
    {
        private readonly global::Unit caster;
        public readonly AbilityBlueprint Blueprint;

        public string ActionName => Blueprint.abilityName;
        public Sprite ActionIcon => Blueprint.abilityIcon;
        public string ActionDescription => Blueprint.abilityDescription;
        public int Range => Blueprint.displayRange;

        public int Level { get; set; } = 1;
        public Vector2Int Target { get; set; }

        public event Action<ActionForecast> OnForecastUpdated;

        public BlueprintActionWrapper(global::Unit caster, AbilityBlueprint blueprint)
        {
            this.caster = caster;
            this.Blueprint = blueprint;
            AbilityExecutor.OnTargetPreviewChanged += HandleTargetPreview;
        }

        public AreaPatternData GetAreaPattern()
        {
            foreach (var step in Blueprint.effectSteps)
            {
                if (step.targetingStrategy != null && step.targetingStrategy.AreaPattern != null)
                {
                    return step.targetingStrategy.AreaPattern;
                }
            }
            return null;
        }

        private void HandleTargetPreview(AbilityBlueprint runningBlueprint, System.Collections.Generic.List<global::Unit> targets)
        {
            if (Blueprint != runningBlueprint) return;

            if (targets == null || targets.Count == 0)
            {
                OnForecastUpdated?.Invoke(default);
                return;
            }

            global::Unit lastTarget = targets[targets.Count - 1];
            if (lastTarget == null) return;
            
            Debug.Log($"[BlueprintActionWrapper] Gerando forecast para: {lastTarget.name}");

            int simulatedBaseDamage = 0;
            foreach (var step in Blueprint.effectSteps)
            {
                if (step.effects == null) continue;

                foreach (var fx in step.effects)
                {
                    if (fx is DamageEffectData dmg)
                    {
                        // Use the correctly qualified type if needed, but here we assume the correct one is in scope
                        simulatedBaseDamage += dmg.amount;
                    }
                }
            }
            
            Debug.Log($"[BlueprintActionWrapper] Dano base simulado calculado: {simulatedBaseDamage}");

            // Reuse familiar combat calculation to preserve Crit / Defense modifiers UI simulation
            AttackResult sample = caster.CalculateAttack(lastTarget);

            ActionForecast forecast = new ActionForecast
            {
                Source = caster,
                Target = lastTarget,
                Damage = sample.damage + simulatedBaseDamage, // Add explicit additional logic
                IsCritical = sample.isCritical,
                AttackCount = caster.GetAttacksAgainst(lastTarget),
                CriticalChance = caster.Stats.criticalChance
            };

            OnForecastUpdated?.Invoke(forecast);
        }

        public void EnterAction()
        {
            if (AbilityExecutor.Instance != null)
            {
                AbilityExecutor.Instance.ExecuteAbility(caster, Blueprint, CelestialCross.Combat.CombatHook.OnManualCast, () => {
                    CameraController.Instance?.ResetFocus();
                    if (caster is global::EnemyUnit)
                        TurnManager.Instance.EndTurn();
                    else    
                        PlayerController.Instance.EndTurn();
                });
            }
            else
            {
                Debug.LogError("[BlueprintActionWrapper] AbilityExecutor não encontrado na cena!");
            }
        }

        public void UpdateAction()
        {
            // Lógica de update se necessário no futuro
        }

        public void Confirm()
        {
            // Confirmação via ActionBarUI. Se o executor já cuida disso, podemos deixar vazio por enquanto.
        }

        public void Cancel()
        {
            if (AbilityExecutor.Instance != null)
            {
                AbilityExecutor.Instance.AbortCurrentAbility();
            }
        }

        public string GetDetailStats()
        {
            return $"Range: {Range}";
        }
    }
}
