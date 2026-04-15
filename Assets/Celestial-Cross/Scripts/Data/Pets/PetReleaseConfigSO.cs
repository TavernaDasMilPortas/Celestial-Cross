using UnityEngine;

namespace CelestialCross.Data.Pets
{
    [CreateAssetMenu(fileName = "PetReleaseConfig", menuName = "Celestial Cross/Pets/Pet Release Configuration")]
    public class PetReleaseConfigSO : ScriptableObject
    {
        [Header("Recompensas por Soltar um Pet")]
        [Tooltip("Quantidade de Poeira Estelar Global (Stardust) recebida por soltar o pet. (Index = Estrelas do Pet)")]
        public int[] StardustPerStar = new int[] { 0, 10, 25, 50, 100, 250 }; // Posição 0 vazia para ignorar pets de 0 estrelas
        
        [Space(10)]
        [Tooltip("Quantidade de Pet-Souls exclusivas da espécie recebidas por soltar o pet. (Index = Estrelas do Pet)")]
        public int[] PetSoulsPerStar = new int[] { 0, 1, 2, 5, 10, 25 }; 
    }
}