using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public class AbilityGraphData
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public List<NodeExport> Nodes { get; set; } = new List<NodeExport>();
        public List<LinkExport> Links { get; set; } = new List<LinkExport>();
        public List<VarExport> Variables { get; set; } = new List<VarExport>();
        public Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();
        
        public int Duration { get; set; }
        public bool IsBuff { get; set; }
        public bool CanStack { get; set; }
        public int MaxStacks { get; set; }
        public bool IsPersistent { get; set; }
        public int MaxLevel { get; set; }
    }

    public class NodeExport
    {
        public string Guid { get; set; }
        public string NodeType { get; set; }
        public string JsonData { get; set; }
    }

    public class LinkExport
    {
        public string BaseNodeGuid { get; set; }
        public string PortName { get; set; }
        public string TargetNodeGuid { get; set; }
        public string TargetPortName { get; set; }
    }

    public class VarExport
    {
        public string Name { get; set; }
        public float InitialValue { get; set; }
    }
}
