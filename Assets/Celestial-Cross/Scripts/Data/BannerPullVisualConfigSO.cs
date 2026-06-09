using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Data
{
    [global::System.Serializable]
    public class BannerPullPosition
    {
        public string positionName;
        public Vector2 position;
    }

    [CreateAssetMenu(fileName = "NewBannerPullVisualConfig", menuName = "Celestial Cross/Gacha/Banner Pull Visual Config")]
    public class BannerPullVisualConfigSO : ScriptableObject
    {
        [Header("Estrelas (1 a 10)")]
        [Tooltip("Define a posição visual das 10 estrelas/resultados ao fazer um pull de 10.")]
        public List<BannerPullPosition> pullPositions = new List<BannerPullPosition>();

        [Header("Conexões Visuais (Opcional, apenas decorativo)")]
        [Tooltip("Pares de índices das estrelas que devem ser conectadas.")]
        public int[] connectionIndices = new int[0];

        public void EnsureTenStars()
        {
            while (pullPositions.Count < 10)
            {
                pullPositions.Add(new BannerPullPosition { positionName = $"Pull {pullPositions.Count + 1}" });
            }
        }
    }
}