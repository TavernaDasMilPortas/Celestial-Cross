using System;
using System.Collections.Generic;

namespace CelestialCross.Progression
{
    [Serializable]
    public class DiaryNodeRequirement
    {
        public bool RequiresInvite;
        public bool RequiresPreviousNode;
        public string PreviousNodeID;
        public List<InviteCost> InviteCostOptions = new List<InviteCost>(); // Opções OR (1 Leidell OU 20 Genéricos)
    }
}
