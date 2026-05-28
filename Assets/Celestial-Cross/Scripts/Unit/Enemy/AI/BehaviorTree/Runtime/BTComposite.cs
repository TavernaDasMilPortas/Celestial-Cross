using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public abstract class BTComposite : BTNode
    {
        public BTCompositeData Data { get; set; }
        public Dictionary<string, BTNode> ChildNodes = new Dictionary<string, BTNode>();
    }
}
