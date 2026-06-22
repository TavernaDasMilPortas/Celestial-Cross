using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.Dialogue.Runtime
{
    public class DialogueReturnToHub : MonoBehaviour
    {
        [SerializeField] private Button returnButton;
        [SerializeField] private string hubSceneName = "HubScene";

        private void Start()
        {
            if (returnButton != null)
            {
                returnButton.gameObject.SetActive(false);
                returnButton.onClick.AddListener(OnReturnClicked);
            }

            if (Manager.DialogueManager.Instance != null)
            {
                Manager.DialogueManager.Instance.OnDialogueEnded.AddListener(ShowButton);
            }
        }

        private void OnDestroy()
        {
            if (Manager.DialogueManager.Instance != null)
            {
                Manager.DialogueManager.Instance.OnDialogueEnded.RemoveListener(ShowButton);
            }
            if (returnButton != null)
            {
                returnButton.onClick.RemoveListener(OnReturnClicked);
            }
        }

        private void ShowButton()
        {
            if (returnButton != null)
            {
                returnButton.gameObject.SetActive(true);
            }
        }

        private void OnReturnClicked()
        {
            if (global::GameFlowManager.Instance != null && global::GameFlowManager.Instance.SelectedStoryNode != null)
            {
                var node = global::GameFlowManager.Instance.SelectedStoryNode;
                var rewards = CelestialCross.System.ProgressionService.Instance?.GetRewardsForNode(node);
                
                if (rewards != null && rewards.Count > 0)
                {
                    // Injeção solicitada: Entregar as unidades DIRETAMENTE no clique do botão
                    for (int i = rewards.Count - 1; i >= 0; i--)
                    {
                        var def = rewards[i];
                        if (def != null && def.Type == CelestialCross.Data.Rewards.RewardType.Unit && def.UnitRef != null)
                        {
                            int amt = UnityEngine.Mathf.Max(1, def.Amount);
                            for (int k = 0; k < amt; k++)
                            {
                                if (AccountManager.Instance != null && AccountManager.Instance.PlayerAccount != null)
                                {
                                    var account = AccountManager.Instance.PlayerAccount;
                                    string unitID = def.UnitRef.UnitID;
                                    var existingUnit = account.OwnedUnits.Find(x => x.UnitID == unitID);
                                    
                                    if (existingUnit != null)
                                    {
                                        existingUnit.Fragments += 20; // Repetido ganha fragmentos
                                        string insigniaID = CelestialCross.System.ConstellationService.GetInsigniaItemID(unitID);
                                        account.AddItem(insigniaID, 1);
                                        Debug.Log($"[DialogueReturnToHub] Duplicata Injetada (Gacha-Style): {unitID} -> +20 Fragments, +1 Insignia");
                                    }
                                    else
                                    {
                                        var newUnit = new CelestialCross.Data.RuntimeUnitData(unitID, 4); // 4 Estrelas padrão
                                        account.OwnedUnits.Add(newUnit);
                                        if (!account.OwnedUnitIDs.Contains(unitID))
                                            account.OwnedUnitIDs.Add(unitID);
                                        Debug.Log($"[DialogueReturnToHub] Nova Unidade Injetada (Gacha-Style): {unitID} com 4 estrelas.");
                                    }
                                    
                                    AccountManager.Instance.SaveAccount();
                                }
                            }
                            // Removemos da lista para não conceder 2 vezes pelo RewardService logo abaixo
                            rewards.RemoveAt(i);
                        }
                    }

                    // Processa as demais recompensas (Dinheiro, energia, XP...)
                    var runtimeReward = CelestialCross.System.RewardService.CreateRuntimeReward(rewards);
                    CelestialCross.System.RewardService.ApplyRuntimeRewardToAccount(runtimeReward);
                }

                CelestialCross.System.ProgressionService.Instance?.RecordNodeCompletion(node);
            }
            CelestialCross.System.SceneTransitionManager.LoadScene(hubSceneName);
        }
    }
}
