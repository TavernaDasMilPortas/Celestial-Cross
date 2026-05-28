using System.Collections.Generic;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree
{
    [CreateAssetMenu(fileName = "NewBehaviorTree", menuName = "Celestial Cross/AI/Behavior Tree")]
    public class BehaviorTreeSO : ScriptableObject
    {
        [Header("Tree Info")]
        public string treeName;
        [TextArea(3, 5)]
        public string description;

        [Header("Graph Data")]
        public List<BTNodeData> NodeData = new List<BTNodeData>();
        public List<BTLinkData> NodeLinks = new List<BTLinkData>();
    }
}
