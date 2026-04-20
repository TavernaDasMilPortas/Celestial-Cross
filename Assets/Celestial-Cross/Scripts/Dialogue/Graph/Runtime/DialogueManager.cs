using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CelestialCross.Dialogue.Graph;
using CelestialCross.Dialogue.Runtime;

namespace CelestialCross.Dialogue.Manager
{
    public class DialogueManager : MonoBehaviour
    {
        /// <summary>
        /// Property to pass the graph between scenes.
        /// Hub sets this before loading the DialogueScene.
        /// </summary>
        public static DialogueGraph NextGraphToLoad { get; set; }

        [Header("Data")]
        [SerializeField] private DialogueGraph dialogueGraph;
        
        [Header("UI Reference")]
        [SerializeField] private DialogueUI dialogueUI;

        private DialogueNodeData _currentNode;
        private string[] _currentPages;
        private int _currentPageIndex;
        private bool _isGraphActive;

        public static DialogueManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Se viemos do Hub com um grafo configurado, usamos ele
            if (NextGraphToLoad != null)
            {
                dialogueGraph = NextGraphToLoad;
                NextGraphToLoad = null; // Limpa para evitar reuso acidental
            }
        }

        private void Start()
        {
            if (dialogueGraph != null)
            {
                StartDialogue(dialogueGraph);
            }
        }

        public void StartDialogue(DialogueGraph graph)
        {
            if (graph == null) return;
            
            dialogueGraph = graph;
            _isGraphActive = true;
            
            // Garantir que a UI está pronta antes de processar
            if (dialogueUI == null)
            {
                Debug.LogWarning("[DialogueManager] Tentativa de iniciar diálogo sem DialogueUI referenciada.");
                return;
            }
            
            // Localizar o nó de entrada (Start)
            var startLink = dialogueGraph.nodeLinks.FirstOrDefault(x => 
                dialogueGraph.nodeData.All(n => n.guid != x.baseNodeGuid)); // O nó base do primeiro link costuma ser o Start se não houver nodeData para ele (ou GUID fixo)
            
            // No nosso sistema, o EntryPointNode não é salvo no nodeData, mas gera links
            if (startLink != null)
            {
                NavigateToNode(startLink.targetNodeGuid);
            }
            else if (dialogueGraph.nodeData.Count > 0)
            {
                NavigateToNode(dialogueGraph.nodeData[0].guid);
            }
        }

        private void NavigateToNode(string guid)
        {
            _currentNode = dialogueGraph.nodeData.FirstOrDefault(x => x.guid == guid);
            
            if (_currentNode == null)
            {
                EndDialogue();
                return;
            }

            ProcessNode();
        }

        private void ProcessNode()
        {
            switch (_currentNode.nodeType)
            {
                case NodeType.Speech:
                    ShowSpeech();
                    break;
                case NodeType.Choice:
                    ShowChoices();
                    break;
                case NodeType.Condition:
                    CheckCondition();
                    break;
                case NodeType.Action:
                    ExecuteAction();
                    break;
                case NodeType.End:
                    EndDialogue();
                    break;
            }
        }

        private void ShowSpeech()
        {
            // Divide o texto em páginas se for muito longo
            _currentPages = DialogueTextProcessor.SplitDialogueText(_currentNode.dialogueText, 150);
            _currentPageIndex = 0;
            
            DisplayCurrentPage();
        }

        private void DisplayCurrentPage()
        {
            dialogueUI.ShowSpeech(_currentNode.speakerName, _currentPages[_currentPageIndex], _currentNode.characterSprite);
        }

        private void Update()
        {
            if (!_isGraphActive) return;

            // Suporte para clique na tela ou tecla de avanço
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                OnPlayerClick();
            }
        }

        public void OnPlayerClick()
        {
            if (!_isGraphActive) return;
            if (dialogueUI.IsTyping)
            {
                dialogueUI.SkipTypewriter();
                return;
            }

            if (_currentNode.nodeType == NodeType.Speech)
            {
                if (_currentPageIndex < _currentPages.Length - 1)
                {
                    _currentPageIndex++;
                    DisplayCurrentPage();
                }
                else
                {
                    // Fim das páginas desta fala, vai para o próximo nó
                    var link = dialogueGraph.nodeLinks.FirstOrDefault(x => x.baseNodeGuid == _currentNode.guid);
                    if (link != null) NavigateToNode(link.targetNodeGuid);
                    else EndDialogue();
                }
            }
        }

        private void ShowChoices()
        {
            // Usamos os links para determinar as escolhas baseadas nos portNames
            var links = dialogueGraph.nodeLinks.Where(x => x.baseNodeGuid == _currentNode.guid).ToList();
            List<string> choiceTexts = links.Select(x => x.portName).ToList();
            
            // Se o nó de escolha tiver um texto de fala próprio, exibi-lo antes
            if (!string.IsNullOrEmpty(_currentNode.dialogueText))
            {
                dialogueUI.ShowSpeech(_currentNode.speakerName, _currentNode.dialogueText, _currentNode.characterSprite);
            }

            dialogueUI.ShowChoices(choiceTexts, (index) => {
                NavigateToNode(links[index].targetNodeGuid);
            });
        }

        private void CheckCondition()
        {
            // Lógica de Blackboard simplificada
            bool result = false;
            var prop = dialogueGraph.exposedProperties.FirstOrDefault(x => x.propertyName == _currentNode.variableName);
            
            if (prop != null)
            {
                // Implementar comparação real baseada no tipo (Int, Bool, String)
                // Por agora, um placeholder True
                result = true; 
            }

            string targetPort = result ? "True" : "False";
            var link = dialogueGraph.nodeLinks.FirstOrDefault(x => x.baseNodeGuid == _currentNode.guid && x.portName == targetPort);
            
            if (link != null) NavigateToNode(link.targetNodeGuid);
            else EndDialogue();
        }

        private void ExecuteAction()
        {
            // Lógica de alterar variável do Blackboard
            var link = dialogueGraph.nodeLinks.FirstOrDefault(x => x.baseNodeGuid == _currentNode.guid);
            if (link != null) NavigateToNode(link.targetNodeGuid);
            else EndDialogue();
        }

        private void EndDialogue()
        {
            _isGraphActive = false;
            dialogueUI.Hide();
            Debug.Log("Dialogue Ended");
        }
    }
}
