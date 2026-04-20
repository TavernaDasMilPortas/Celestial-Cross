using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CelestialCross.Progression
{
    public class ChapterMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChapterData chapterData;
        [SerializeField] private Transform container;
        [SerializeField] private Button nodeButtonPrefab;

        [Header("Visuals")]
        [SerializeField] private TMP_Text chapterTitleText;
        [SerializeField] private Sprite lockedIcon;
        [SerializeField] private Sprite completedIcon;

        private void Start()
        {
            if (chapterData != null)
            {
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            if (chapterTitleText != null) chapterTitleText.text = chapterData.ChapterTitle;

            // Limpar container
            foreach (Transform child in container) Destroy(child.gameObject);

            // Obter progresso da conta
            HashSet<string> completedNodes = new HashSet<string>(AccountManager.Instance?.PlayerAccount?.CompletedNodeIDs ?? new List<string>());

            for (int i = 0; i < chapterData.Nodes.Count; i++)
            {
                var node = chapterData.Nodes[i];
                Button btn = Instantiate(nodeButtonPrefab, container);
                
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = node.Title;

                bool isCompleted = completedNodes.Contains(node.NodeID);
                bool isLocked = false;

                // Regra de Trava: Se o nó anterior não estiver completo (e não for o primeiro)
                if (i > 0)
                {
                    string previousNodeID = chapterData.Nodes[i-1].NodeID;
                    if (!completedNodes.Contains(previousNodeID))
                    {
                        isLocked = true;
                    }
                }

                // Aplicar visual
                btn.interactable = !isLocked;
                
                // Configurar clique
                btn.onClick.AddListener(() => {
                    node.Execute();
                });
            }
        }
    }
}