using System;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities;
using Celestial_Cross.Scripts.Combat.Execution;

namespace Celestial_Cross.Scripts.Units
{
    public class BlueprintActionWrapper : IUnitAction
    {
        private readonly global::Unit caster;
        private readonly AbilityBlueprint blueprint;

        public string ActionName => blueprint.abilityName;
        public Sprite ActionIcon => blueprint.abilityIcon;
        public string ActionDescription => blueprint.abilityDescription;
        public int Range => blueprint.displayRange;

        public event Action<ActionForecast> OnForecastUpdated;

        public BlueprintActionWrapper(global::Unit caster, AbilityBlueprint blueprint)
        {
            this.caster = caster;
            this.blueprint = blueprint;
            AbilityExecutor.OnTargetPreviewChanged += HandleTargetPreview;
        }

        private void HandleTargetPreview(AbilityBlueprint runningBlueprint, System.Collections.Generic.List<global::Unit> targets)
        {
            if (blueprint != runningBlueprint) return;

            if (targets == null || targets.Count == 0)
            {
                OnForecastUpdated?.Invoke(default);
                return;
            }

            global::Unit lastTarget = targets[targets.Count - 1];
            if (lastTarget == null) return;
            
            Debug.Log($"[BlueprintActionWrapper] Gerando forecast para: {lastTarget.name}");

            int simulatedBaseDamage = 0;
            foreach (var step in blueprint.effectSteps)
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
            // O AbilityExecutor cuida das fases da habilidade (visualização e execução)
            if (AbilityExecutor.Instance != null)
            {
                AbilityExecutor.Instance.ExecuteAbility(caster, blueprint);
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
