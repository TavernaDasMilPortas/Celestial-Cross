using System;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI
{
    [Serializable]
    public struct AITurnPlan
    {
        public AIActionScore moveStep;    // Optional
        public AIActionScore actionStep;  // Attack or Skill
        public float combinedScore;
        public bool hasMove;
        public bool hasAction;

        public bool IsValid => hasMove || hasAction;
    }
}
