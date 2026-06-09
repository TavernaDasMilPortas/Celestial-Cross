using System;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree
{
    [Serializable]
    public class BTLinkData
    {
        public string ParentGuid;
        public string ParentPort;
        public string ChildGuid;
        public string ChildPort;
    }
}
