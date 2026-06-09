using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialCross.Progression;

namespace CelestialCross.System
{
    public class StoryFlowController : MonoBehaviour
    {
        public static StoryFlowController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartCombatNode(CombatStoryNode node)
        {
            if (GameFlowManager.Instance == null) 
            {
                Debug.LogError("[StoryFlowController] GameFlowManager.Instance is NULL. Cannot start combat node.");
                return;
            }

            Debug.Log($"[StoryFlowController] Starting Combat Node: {node.Title}. Loading PreparationScene...");
            GameFlowManager.Instance.SelectedLevel = node.LevelRef;
            GameFlowManager.Instance.SelectedDungeon = node.DungeonRef;
            GameFlowManager.Instance.SelectedDungeonNode = null; // LIMPA o lingering node
            GameFlowManager.Instance.SelectedStoryNode = node;
            GameFlowManager.Instance.FixedSlots = node.FixedSlots;
            
            // Assume preparation scene is generically PreparationScene
            GameFlowManager.Instance.SelectedUnitIDs.Clear();
            GameFlowManager.Instance.PlayerFormation.Clear();

            SceneManager.LoadScene("PreparationScene");
        }

        public void StartDialogueNode(DialogueStoryNode node)
        {
            if (node.DialogueGraph == null) 
            {
                Debug.LogError("[StoryFlowController] DialogueGraph is NULL. Cannot start dialogue.");
                return;
            }
            Debug.Log($"[StoryFlowController] Starting Dialogue Node: {node.Title}. Loading DialogueScene...");
            CelestialCross.Dialogue.Manager.DialogueManager.NextGraphToLoad = node.DialogueGraph;
            SceneManager.LoadScene("DialogueScene"); // Nome padrão da cena
        }
    }
}
