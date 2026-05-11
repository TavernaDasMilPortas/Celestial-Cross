using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Data
{
    [global::System.Serializable]
    public class ConstellationStar
    {
        public string starName;
        public Vector2 position;
        public Celestial_Cross.Scripts.Abilities.Graph.AbilityGraphSO passiveGraph;
        
        [TextArea(2, 4)]
        public string customDescription; // Opcional, substitui a descrição do grafo se preenchido
    }

    [CreateAssetMenu(fileName = "NewConstellationConfig", menuName = "Celestial Cross/Constellation/Config")]
    public class ConstellationConfigSO : ScriptableObject
    {
        [Header("Estrelas (C1 a C6)")]
        [Tooltip("A ordem na lista define o nível (Índice 0 = Estrela 1, etc)")]
        public List<ConstellationStar> stars = new List<ConstellationStar>();

        [Header("Conexões Visuais")]
        [Tooltip("Pares de índices das estrelas que devem ser conectadas. Ex: [0,1, 1,2] conecta 0-1 e 1-2")]
        public int[] connectionIndices = new int[0];

        public void EnsureSixStars()
        {
            while (stars.Count < 6)
            {
                stars.Add(new ConstellationStar { starName = $"Estrela {stars.Count + 1}" });
            }
        }
    }
}
