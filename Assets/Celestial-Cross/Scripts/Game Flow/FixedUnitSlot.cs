using System;

namespace CelestialCross.Progression
{
    [Serializable]
    public class FixedUnitSlot
    {
        public UnitData UnitRef;        // A unidade obrigatória
        public bool IsLocked;           // Não pode ser removida da equipe
        public bool UsePlayerBuild;     // Se true e o jogador tem a unidade, usa a build dele
    }
}
