using UnityEngine;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree;

namespace Celestial_Cross.Scripts.Units.Enemy
{
    /// <summary>
    /// Subclasse de Unit para inimigos controlados por IA.
    /// O AIBrain é adicionado automaticamente via RequireComponent.
    /// </summary>
    [RequireComponent(typeof(AIBrain))]
    public class EnemyUnit : Unit
    {
        [Header("AI")]
        [SerializeField] private BehaviorTreeSO behaviorTreeSO;

        public BehaviorTreeSO BehaviorTree => behaviorTreeSO != null ? behaviorTreeSO : (unitData != null ? unitData.defaultBehaviorTree : null);

        public AIBrain Brain { get; private set; }

        public void SetBehaviorTree(BehaviorTreeSO tree)
        {
            this.behaviorTreeSO = tree;
            if (Brain != null)
            {
                Brain.ReinitializeTree();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Brain = GetComponent<AIBrain>();
        }
    }
}
