using UnityEngine;

[System.Obsolete("Usar o novo sistema RewardPackageSO e RewardDefinition")]
[CreateAssetMenu(fileName = "NewRewardPackage", menuName = "Celestial Cross/Rewards/Reward Package (Obsolete)")]
public class RewardPackage : ScriptableObject
{
    public int Money;
    public int Energy;
    public int XP;
    // Adicione outros tipos de recompensa aqui, como itens, etc.
}
