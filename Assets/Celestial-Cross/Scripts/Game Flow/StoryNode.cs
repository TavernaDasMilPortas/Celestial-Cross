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

        public abstract void Execute();
    }

    [Serializable]
    public class DialogueStoryNode : StoryNode
    {
        public DialogueGraph DialogueGraph;

        public override void Execute()
        {
            if (DialogueGraph == null) return;
            CelestialCross.Dialogue.Manager.DialogueManager.NextGraphToLoad = DialogueGraph;
            UnityEngine.SceneManagement.SceneManager.LoadScene("DialogueScene"); // Nome padrão da cena
        }
    }

    [Serializable]
    public class CombatStoryNode : StoryNode
    {
        public LevelData LevelRef;
        public DungeonBaseSO DungeonRef;

        public override void Execute()
        {
            // Lógica para iniciar combate
            // Aqui integraríamos com o seu PlacementManager ou SceneLoader de combate
            Debug.Log($"Iniciando Combate: {Title}");
        }
    }
}