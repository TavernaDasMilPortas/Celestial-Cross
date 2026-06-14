using UnityEngine;
using UnityEditor;
using Celestial_Cross.Scripts.Abilities.Graph;
using Celestial_Cross.Scripts.Abilities.Graph.Editor;

namespace Celestial_Cross.Scripts.Editor
{
    public class AIHintBatchUpdater
    {
        [MenuItem("Celestial Cross/AI/Batch Update AI Hints")]
        public static void ShowWindow()
        {
            if (EditorUtility.DisplayDialog("Batch Update AI Hints", 
                "Isso irá varrer todos os AbilityGraphSO no projeto e recalcular os AIHints das habilidades não-bloqueadas (isLocked = false).\n\nDeseja continuar?", "Sim, Atualizar", "Cancelar"))
            {
                RunBatchUpdate();
            }
        }

        private static void RunBatchUpdate()
        {
            string[] guids = AssetDatabase.FindAssets("t:AbilityGraphSO");
            int updatedCount = 0;
            int lockedCount = 0;
            int totalCount = guids.Length;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var graphSO = AssetDatabase.LoadAssetAtPath<AbilityGraphSO>(path);

                if (graphSO != null)
                {
                    if (graphSO.aiHint != null && graphSO.aiHint.isLocked)
                    {
                        lockedCount++;
                        continue;
                    }

                    AIHintCalculator.Recalculate(graphSO);
                    EditorUtility.SetDirty(graphSO);
                    updatedCount++;
                }
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("Batch Update Concluído", 
                $"Total de Grafos encontrados: {totalCount}\nGrafos Atualizados: {updatedCount}\nIgnorados (Locked): {lockedCount}", "OK");
        }
    }
}
