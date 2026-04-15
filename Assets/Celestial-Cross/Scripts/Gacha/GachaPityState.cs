using System;

namespace CelestialCross.Gacha
{
    [Serializable]
    public class GachaPityState
    {
        public string BannerID;
        public int PullsSinceLastSupreme;
        public int PullsSinceLastOverBase; // Pity de 10 tiros
        public bool Lost5050; 
        
        [UnityEngine.Tooltip("O ID do supremo escolhido pelo jogador para focar")]
        public string SelectedSupremeChoice; 
        
        public GachaPityState(string bannerId)
        {
            BannerID = bannerId;
            PullsSinceLastSupreme = 0;
            PullsSinceLastOverBase = 0;
            Lost5050 = false;
            SelectedSupremeChoice = "";
        }
    }
}