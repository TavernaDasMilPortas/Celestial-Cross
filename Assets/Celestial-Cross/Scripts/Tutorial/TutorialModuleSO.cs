using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Tutorial
{
    [CreateAssetMenu(fileName = "NewTutorialModule", menuName = "Celestial Cross/Tutorial/Module")]
    public class TutorialModuleSO : ScriptableObject
    {
        public string ModuleID;
        public string ModuleTitle;
        [TextArea] public string ModuleDescription;
        
        [Header("Flow")]
        public List<TutorialStep> Steps = new List<TutorialStep>();
        
        [Header("Initial Setup")]
        public LevelData OverrideLevelData;       // Mapa/grid fixo para o tutorial
        public List<TutorialUnitSetup> FixedUnits = new List<TutorialUnitSetup>(); // Unidades pré-posicionadas
    }
}
