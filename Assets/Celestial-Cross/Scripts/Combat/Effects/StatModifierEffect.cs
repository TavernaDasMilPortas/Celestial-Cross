using UnityEngine;
using CelestialCross.Combat;

namespace CelestialCross.Combat
{
    /// <summary>
    /// Efeito que modifica os atributos de combate da unidade alvo.
    /// Pode ser usado em passivas (ex: Ganha 5 de ATK ao iniciar turno) ou condições.
    /// </summary>
    [System.Serializable]
    public class StatModifierEffect : AbilityEffectBase
    {
        public CombatStats modifiers;

        public override void Execute(CombatContext context)
        {
            foreach (var t in EffectTargetSolver.GetTargets(context, targetType))
            {
                if (t != null)
                {
                    Debug.Log($"[{targetType}] StatModifierEffect em {t.DisplayName}");
                    t.AddStatModifier(modifiers);
                }
            }
        }
    }
}
