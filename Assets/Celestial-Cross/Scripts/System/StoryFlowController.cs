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
            if (GameFlowManager.Instance == null) return;

            GameFlowManager.Instance.SelectedLevel = node.LevelRef;
            GameFlowManager.Instance.SelectedDungeon = node.DungeonRef;
            GameFlowManager.Instance.FixedSlots = node.FixedSlots;
            
            // Assume preparation scene is generically PreparationScene
            GameFlowManager.Instance.SelectedUnitIDs.Clear();
            GameFlowManager.Instance.PlayerFormation.Clear();

            SceneManager.LoadScene("PreparationScene");
        }

        public void StartDialogueNode(DialogueStoryNode node)
        {
            if (node.DialogueGraph == null) return;
            CelestialCross.Dialogue.Manager.DialogueManager.NextGraphToLoad = node.DialogueGraph;
            SceneManager.LoadScene("DialogueScene"); // Nome padrão da cena
        }
    }
}
