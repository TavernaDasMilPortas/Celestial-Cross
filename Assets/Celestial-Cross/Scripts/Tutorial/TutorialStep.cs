using UnityEngine;
using System;

namespace CelestialCross.Tutorial
{
    [Serializable]
    public class TutorialStep
    {
        public string StepID;
        
        [Header("Instrução")]
        [TextArea] public string InstructionText;       // Texto do banner
        public TutorialBannerPosition BannerPosition;   // Topo, Centro, Baixo
        public Sprite InstructionIcon;                   // Ícone opcional
        
        [Header("Highlight")]
        public TutorialHighlightTarget HighlightType;   // UIButton, GridTile, Unit, None
        public string TargetIdentifier;                  // Ex: "ActionButton_0", "Tile_3_2", "Unit_Player_0"
        public Vector2 SpotlightSize = new Vector2(100, 100); // Tamanho do buraco no dim
        
        [Header("Condição de Avanço")]
        public TutorialAdvanceCondition AdvanceCondition; // ClickHighlighted, ClickAnywhere, WaitSeconds, ActionConfirmed
        public float WaitDuration;                        // Se AdvanceCondition == WaitSeconds
        
        [Header("Ação Forçada (Resultado Premeditado)")]
        public bool ForceActionResult;                   // Se true, o resultado é mockado
        public int ForcedDamage;                          // Dano fixo a ser aplicado
        public bool ForcedCrit;                           // Forçar crítico
        
        [Header("Foco de Câmera")]
        public bool FocusCamera;                         // Move câmera para o alvo
        public Vector2Int CameraFocusGridPos;            // Posição no grid para focar
    }
}
