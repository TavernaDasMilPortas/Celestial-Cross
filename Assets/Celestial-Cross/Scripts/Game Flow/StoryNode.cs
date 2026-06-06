using System;
using UnityEngine;
using CelestialCross.Dialogue.Graph;
using CelestialCross.Data;
using CelestialCross.Data.Dungeon;

namespace CelestialCross.Progression
{
    [Serializable]
    public abstract class StoryNode
    {
        public string NodeID;
        public string Title;
        public bool IsAutoPlay = false;

        public NodeEntryCost EntryCost = new NodeEntryCost();
        public NodeRewardConfig Rewards = new NodeRewardConfig();
        
        [Tooltip("-1 significa tentativas infinitas. Qualquer outro valor limita quantas vezes o nó pode ser completado.")]
        public int MaxCompletions = -1;
        
        public CompletionResetPolicy ResetPolicy = new CompletionResetPolicy();
        
        [Tooltip("Ícone opcional para representar este nó na UI do Hub.")]
        public Sprite NodeIcon;
        
        public DiaryNodeRequirement Requirement = new DiaryNodeRequirement();

        public abstract void Execute();
    }

    [Serializable]
    public class DialogueStoryNode : StoryNode
    {
        public DialogueGraph DialogueGraph;

        public override void Execute()
        {
            if (DialogueGraph == null) return;
            CelestialCross.System.StoryFlowController.Instance?.StartDialogueNode(this);
        }
    }

    [Serializable]
    public class CombatStoryNode : StoryNode
    {
        public LevelData LevelRef;
        public DungeonBaseSO DungeonRef;
        
        public global::System.Collections.Generic.List<FixedUnitSlot> FixedSlots = new global::System.Collections.Generic.List<FixedUnitSlot>();

        public override void Execute()
        {
            CelestialCross.System.StoryFlowController.Instance?.StartCombatNode(this);
        }
    }
}