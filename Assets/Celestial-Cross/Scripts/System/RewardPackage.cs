using UnityEngine;

[CreateAssetMenu(fileName = "NewRewardPackage", menuName = "Celestial Cross/Rewards/Reward Package")]
public class RewardPackage : ScriptableObject
{
    public int Money;
    public int Energy;
    public int XP;
    // Adicione outros tipos de recompensa aqui, como itens, etc.
}
