using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CelestialCross.Combat
{
    public static class EffectTargetSolver
    {
        public static IEnumerable<Unit> GetTargets(CombatContext context, EffectTargetType type)
        {
            if (type == EffectTargetType.Default)
                return new List<Unit> { context.target }.Where(u => u != null);

            if (type == EffectTargetType.Self)
                return new List<Unit> { context.source }.Where(u => u != null);

            // Para os demais, precisamos de todas as unidades no combate
            var allUnits = Object.FindObjectsOfType<Unit>().ToList();
            var source = context.source;

            if (source == null) return Enumerable.Empty<Unit>();

            switch (type)
            {
                case EffectTargetType.AllEnemies:
                    return allUnits.Where(u => IsEnemy(source, u));

                case EffectTargetType.AllAllies:
                    return allUnits.Where(u => !IsEnemy(source, u));

                case EffectTargetType.EnemyMostHP:
                    return allUnits.Where(u => IsEnemy(source, u))
                        .OrderByDescending(u => u.Health.CurrentHealth)
                        .Take(1);

                case EffectTargetType.EnemyLeastHP:
                    return allUnits.Where(u => IsEnemy(source, u))
                        .OrderBy(u => u.Health.CurrentHealth)
                        .Take(1);

                case EffectTargetType.AllyMostHP:
                    return allUnits.Where(u => !IsEnemy(source, u))
                        .OrderByDescending(u => u.Health.CurrentHealth)
                        .Take(1);

                case EffectTargetType.AllyLeastHP:
                    return allUnits.Where(u => !IsEnemy(source, u))
                        .OrderBy(u => u.Health.CurrentHealth)
                        .Take(1);

                case EffectTargetType.NearestEnemy:
                    return allUnits.Where(u => IsEnemy(source, u))
                        .OrderBy(u => Vector2Int.Distance(source.GridPosition, u.GridPosition))
                        .Take(1);

                case EffectTargetType.FarthestEnemy:
                    return allUnits.Where(u => IsEnemy(source, u))
                        .OrderByDescending(u => Vector2Int.Distance(source.GridPosition, u.GridPosition))
                        .Take(1);

                case EffectTargetType.NearestAlly:
                    return allUnits.Where(u => !IsEnemy(source, u) && u != source)
                        .OrderBy(u => Vector2Int.Distance(source.GridPosition, u.GridPosition))
                        .Take(1);

                case EffectTargetType.FarthestAlly:
                    return allUnits.Where(u => !IsEnemy(source, u) && u != source)
                        .OrderByDescending(u => Vector2Int.Distance(source.GridPosition, u.GridPosition))
                        .Take(1);
            }

            return Enumerable.Empty<Unit>();
        }

        private static bool IsEnemy(Unit a, Unit b)
        {
            if (a == null || b == null) return false;
            // Pet é time Player. EnemyUnit é time Inimigo.
            bool aIsPlayer = a is Pet;
            bool bIsPlayer = b is Pet;
            return aIsPlayer != bIsPlayer;
        }
    }
}
