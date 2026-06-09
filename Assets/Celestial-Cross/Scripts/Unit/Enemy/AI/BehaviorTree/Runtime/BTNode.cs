using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public abstract class BTNode
    {
        public string Guid { get; set; }
        public System.Collections.Generic.Dictionary<string, BTNode> DataInputs { get; set; } = new System.Collections.Generic.Dictionary<string, BTNode>();

        public abstract BTResult Evaluate(AIBlackboard blackboard);
    }
}
