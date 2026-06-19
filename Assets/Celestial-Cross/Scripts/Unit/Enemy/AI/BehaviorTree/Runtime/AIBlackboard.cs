using System.Collections.Generic;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class AIBlackboard
    {
        // Dados de Turno
        public List<Unit> allies = new List<Unit>();
        public List<Unit> enemies = new List<Unit>();
        public HashSet<Vector2Int> reachableTiles = new HashSet<Vector2Int>();
        public float myHpPercent;
        public int aliveAllyCount;
        public Unit closestEnemy;
        public Unit weakestEnemy;
        public Unit strongestEnemy;
        public bool isAlone;
        public int currentTurnNumber;
        public Vector2Int myPosition;
        public int myBaseRange;
        public Unit MyUnit;
        
        public class AbilityInfo {
            public IUnitAction action;
            public AbilitySubtype subtype;
            public int range;
            public AreaPatternData areaPattern;
            public AIAbilityHint hint;
            public int maxTargets = 1;
            public bool allowSameTargetMultipleTimes = false;
        }
        public List<AbilityInfo> availableAbilities = new List<AbilityInfo>();

        public class PlannedAction {
            public IUnitAction actionToExecute;
            public Vector2Int? moveTarget;
            public Unit targetUnit;
            public List<Unit> targetUnits;
            public List<Vector2Int> targetPositions;
        }
        public PlannedAction bestPlan;

        // Dados Persistentes
        public Dictionary<Unit, float> damageReceivedFrom = new Dictionary<Unit, float>();
        public Dictionary<Unit, float> damageDoneToward = new Dictionary<Unit, float>();
        public Unit lastAttacker;
        public int turnsAlive;
        public Dictionary<string, int> abilityCooldowns = new Dictionary<string, int>();
        
        public Dictionary<string, string> customStates = new Dictionary<string, string>();

        public void UpdateTurnData(Unit self)
        {
            // Placeholder para a Fase 3
        }
        
        public string GetState(string key)
        {
            if (customStates.TryGetValue(key, out var val)) return val;
            return string.Empty;
        }

        public void SetState(string key, string value)
        {
            customStates[key] = value;
        }
    }
}
