using UnityEngine;
using Sirenix.OdinInspector;

namespace CelestialCross.Grid
{
    public enum HighlightType { Movement, Attack, Special, Preview }

    [CreateAssetMenu(fileName = "NewHighlightConfig", menuName = "Celestial Cross/Highlight Config")]
    public class HighlightConfig : ScriptableObject
    {
        [Title("Esquema de Cores")]
        [HorizontalGroup("Colors")]
        [VerticalGroup("Colors/Left")] public Color moveColor = new Color(0, 0.5f, 1, 0.5f);
        [VerticalGroup("Colors/Left")] public Color attackColor = new Color(1, 0, 0, 0.5f);
        [VerticalGroup("Colors/Right")] public Color specialColor = new Color(0, 1, 0, 0.5f);
        [VerticalGroup("Colors/Right")] public Color previewColor = new Color(1, 0.9f, 0, 0.5f);

        [Title("Blob Tileset (Onde a linha de borda aparece)")]
        [InfoBox("Atribua os sprites de acordo com os LADOS que devem ter uma borda visual.")]
        
        [BoxGroup("Isolado")]
        [LabelText("Borda nos 4 Lados (CBED)")] public Sprite Borda_CBED;

        [BoxGroup("Pontas (Formato U)")]
        [HorizontalGroup("Pontas/Row1"), LabelText("Borda em B, E, D")] public Sprite Borda_BED;
        [HorizontalGroup("Pontas/Row1"), LabelText("Borda em C, B, E")] public Sprite Borda_CBE;
        [HorizontalGroup("Pontas/Row2"), LabelText("Borda em C, E, D")] public Sprite Borda_CED;
        [HorizontalGroup("Pontas/Row2"), LabelText("Borda em C, B, D")] public Sprite Borda_CBD;

        [BoxGroup("Linhas Paralelas")]
        [HorizontalGroup("Linhas/Row1"), LabelText("Borda em E e D (||)")] public Sprite Borda_ED;
        [HorizontalGroup("Linhas/Row1"), LabelText("Borda em C e B (=)")] public Sprite Borda_CB;

        [BoxGroup("Quinas (Formato L)")]
        [HorizontalGroup("Quinas/Row1"), LabelText("Borda em B e E")] public Sprite Borda_BE;
        [HorizontalGroup("Quinas/Row1"), LabelText("Borda em B e D")] public Sprite Borda_BD;
        [HorizontalGroup("Quinas/Row2"), LabelText("Borda em C e E")] public Sprite Borda_CE;
        [HorizontalGroup("Quinas/Row2"), LabelText("Borda em C e D")] public Sprite Borda_CD;

        [BoxGroup("Lados Únicos")]
        [HorizontalGroup("Lados/Row1"), LabelText("Borda apenas em E")] public Sprite Borda_E;
        [HorizontalGroup("Lados/Row1"), LabelText("Borda apenas em D")] public Sprite Borda_D;
        [HorizontalGroup("Lados/Row2"), LabelText("Borda apenas em B")] public Sprite Borda_B;
        [HorizontalGroup("Lados/Row2"), LabelText("Borda apenas em C")] public Sprite Borda_C;

        [BoxGroup("Interior")]
        [LabelText("Sem Bordas (Centro da área)")] public Sprite Sem_Borda;

        /// <summary>
        /// Mapeia a conexão dos vizinhos (bitmask) para o sprite de borda correspondente.
        /// </summary>
        public Sprite GetSprite(int mask)
        {
            return mask switch
            {
                0 => Borda_CBED, // Sem vizinhos = borda em tudo
                1 => Borda_BED,  // Vizinho em Cima = borda em Baixo, Esquerda, Direita
                2 => Borda_CBE,  // Vizinho na Direita = borda em Cima, Baixo, Esquerda
                3 => Borda_BE,   // Vizinhos C+D = borda em Baixo, Esquerda (Quina)
                4 => Borda_CED,  // Vizinho em Baixo = borda em Cima, Esquerda, Direita
                5 => Borda_ED,   // Vizinhos C+B = borda em Esquerda, Direita (Corredor)
                6 => Borda_CE,   // Vizinhos D+B = borda em Cima, Esquerda (Quina)
                7 => Borda_E,    // Vizinhos C+D+B = borda apenas na Esquerda
                8 => Borda_CBD,  // Vizinho na Esquerda = borda em Cima, Baixo, Direita
                9 => Borda_BD,   // Vizinhos C+E = borda em Baixo, Direita (Quina)
                10 => Borda_CB,  // Vizinhos E+D = borda em Cima, Baixo (Corredor)
                11 => Borda_B,   // Vizinhos E+C+D = borda apenas em Baixo
                12 => Borda_CD,  // Vizinhos B+E = borda em Cima, Direita (Quina)
                13 => Borda_D,   // Vizinhos B+E+C = borda apenas na Direita
                14 => Borda_C,   // Vizinhos D+B+E = borda apenas em Cima
                15 => Sem_Borda, // Todos vizinhos presentes = sem bordas
                _ => Sem_Borda
            };
        }

        public Color GetColor(HighlightType type)
        {
            return type switch
            {
                HighlightType.Movement => moveColor,
                HighlightType.Attack => attackColor,
                HighlightType.Special => specialColor,
                HighlightType.Preview => previewColor,
                _ => Color.white
            };
        }
    }
}
