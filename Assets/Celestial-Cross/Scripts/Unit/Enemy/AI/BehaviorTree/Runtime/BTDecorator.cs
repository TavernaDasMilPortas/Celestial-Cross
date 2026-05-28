namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public abstract class BTDecorator : BTNode
    {
        public BTNode Child { get; set; }
    }
}
