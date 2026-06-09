using System;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree
{
    [Serializable]
    public class BTNodeData
    {
        public string Guid;
        public string NodeType;
        public string NodeTitle;
        public Vector2 Position;
        
        [TextArea(3, 10)]
        public string JsonData;
    }
}
